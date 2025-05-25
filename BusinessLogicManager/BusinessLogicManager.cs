using MongoDB.Bson;
using MongoDB.Driver;
using YourNamespace.Data; // für DBManager<T>
using YourNamespace.Models; // für DBManager<T>

namespace YourNamespace.Logic
{
    public class BusinessLogicManager
    {
        // Generische Datenmanager für alle relevanten BusinessObjects
        private readonly DBManager<BusinessObjects.Employee> _employeeManager;
        private readonly DBManager<BusinessObjects.Project> _projectManager;
        private readonly DBManager<BusinessObjects.WorkTimeEntry> _workTimeManager;
        private readonly DBManager<BusinessObjects.ProjectTimeEntry> _projectTimeManager;
        private readonly DBManager<BusinessObjects.LeaveRequest> _leaveRequestManager;
        private readonly DBManager<BusinessObjects.SickNote> _sickNoteManager;
        private readonly DBManager<BusinessObjects.ExpenseReport> _expenseReportManager;
        private readonly DBManager<BusinessObjects.Payroll> _payrollManager;
        private readonly DBManager<BusinessObjects.TravelRequest> _travelRequestManager;
        private readonly DBManager<BusinessObjects.Document> _documentManager;
        private readonly DBManager<BusinessObjects.ApprovalWorkflow> _workflowManager;

        // Konstruktor – Initialisierung mit Collection-Namen für MongoDB
        public BusinessLogicManager(string dbName, string connectionString = "mongodb://localhost:27017")
        {
            _employeeManager = new(dbName, "Employees", connectionString);
            _projectManager = new(dbName, "Projects", connectionString);
            _workTimeManager = new(dbName, "WorkTimeEntries", connectionString);
            _projectTimeManager = new(dbName, "ProjectTimeEntries", connectionString);
            _leaveRequestManager = new(dbName, "LeaveRequests", connectionString);
            _sickNoteManager = new(dbName, "SickNotes", connectionString);
            _expenseReportManager = new(dbName, "ExpenseReports", connectionString);
            _payrollManager = new(dbName, "Payrolls", connectionString);
            _travelRequestManager = new(dbName, "TravelRequests", connectionString);
            _documentManager = new(dbName, "Documents", connectionString);
            _workflowManager = new(dbName, "ApprovalWorkflows", connectionString);
        }

        #region Mitarbeiterverwaltung

        public async Task<List<BusinessObjects.Employee>> GetAllEmployeesAsync()
            => await _employeeManager.GetAllAsync();

        public async Task<BusinessObjects.Employee> GetEmployeeByIdAsync(ObjectId employeeId)
            => await _employeeManager.GetByIdAsync(employeeId.ToString());

        public async Task AddOrUpdateEmployeeAsync(BusinessObjects.Employee employee)
        {
            if (employee.Id == ObjectId.Empty)
                await _employeeManager.InsertAsync(employee);
            else
                await _employeeManager.UpdateAsync(employee.Id.ToString(), employee);
        }

        public async Task<bool> DeactivateEmployeeAsync(ObjectId employeeId)
        {
            var emp = await _employeeManager.GetByIdAsync(employeeId.ToString());
            if (emp == null) return false;
            emp.IsActive = false;
            return await _employeeManager.UpdateAsync(employeeId.ToString(), emp);
        }

        #endregion

        #region Zeiterfassung & Projekte

        public async Task AddWorkTimeEntryAsync(BusinessObjects.WorkTimeEntry entry)
            => await _workTimeManager.InsertAsync(entry);

        public async Task<List<BusinessObjects.WorkTimeEntry>> GetWorkTimeEntriesForEmployee(ObjectId employeeId, DateTime? from = null, DateTime? to = null)
        {
            var filter = Builders<BusinessObjects.WorkTimeEntry>.Filter.Eq(x => x.EmployeeId, employeeId);
            if (from.HasValue)
                filter &= Builders<BusinessObjects.WorkTimeEntry>.Filter.Gte(x => x.Date, from.Value);
            if (to.HasValue)
                filter &= Builders<BusinessObjects.WorkTimeEntry>.Filter.Lte(x => x.Date, to.Value);
            return await _workTimeManager.FindAsync(filter);
        }

        public async Task<List<BusinessObjects.ProjectTimeEntry>> GetProjectTimesForProject(ObjectId projectId, DateTime? from = null, DateTime? to = null)
        {
            var filter = Builders<BusinessObjects.ProjectTimeEntry>.Filter.Eq(x => x.ProjectId, projectId);
            if (from.HasValue)
                filter &= Builders<BusinessObjects.ProjectTimeEntry>.Filter.Gte(x => x.Date, from.Value);
            if (to.HasValue)
                filter &= Builders<BusinessObjects.ProjectTimeEntry>.Filter.Lte(x => x.Date, to.Value);
            return await _projectTimeManager.FindAsync(filter);
        }

        public async Task<double> GetTotalBillableHoursForProject(ObjectId projectId)
        {
            var entries = await GetProjectTimesForProject(projectId);
            return entries.Sum(e => e.Hours);
        }

        #endregion

        #region Projektmanagement

        public async Task<List<BusinessObjects.Project>> GetAllProjectsAsync()
            => await _projectManager.GetAllAsync();

        public async Task<List<BusinessObjects.Employee>> GetProjectTeamMembers(ObjectId projectId)
        {
            var project = await _projectManager.GetByIdAsync(projectId.ToString());
            if (project == null || project.MemberEmployeeIds == null) return new List<BusinessObjects.Employee>();
            var tasks = project.MemberEmployeeIds.Select(id => _employeeManager.GetByIdAsync(id.ToString()));
            var result = await Task.WhenAll(tasks);
            return result.ToList();
        }

        // Projekt-Auslastung für Management/Controlling
        public async Task<Dictionary<ObjectId, double>> GetConsultantUtilization(DateTime from, DateTime to)
        {
            var employees = await _employeeManager.GetAllAsync();
            var result = new Dictionary<ObjectId, double>();
            foreach (var emp in employees.Where(e => e.IsActive))
            {
                var times = await GetWorkTimeEntriesForEmployee(emp.Id, from, to);
                double hours = times.Sum(t => t.Hours);
                result.Add(emp.Id, hours);
            }
            return result;
        }

        #endregion

        #region Urlaub, Krankmeldung, Abwesenheiten

        public async Task<List<BusinessObjects.LeaveRequest>> GetLeaveRequestsForEmployee(ObjectId employeeId)
        {
            var filter = Builders<BusinessObjects.LeaveRequest>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _leaveRequestManager.FindAsync(filter);
        }

        public async Task<List<BusinessObjects.SickNote>> GetSickNotesForEmployee(ObjectId employeeId)
        {
            var filter = Builders<BusinessObjects.SickNote>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _sickNoteManager.FindAsync(filter);
        }

        // Für HR und Geschäftsleitung: Abwesenheiten-Report
        public async Task<Dictionary<string, int>> GetAbsenceStatistics(DateTime from, DateTime to)
        {
            var leave = await _leaveRequestManager.FindAsync(
                Builders<BusinessObjects.LeaveRequest>.Filter.And(
                    Builders<BusinessObjects.LeaveRequest>.Filter.Gte(x => x.StartDate, from),
                    Builders<BusinessObjects.LeaveRequest>.Filter.Lte(x => x.EndDate, to)
                )
            );
            var sick = await _sickNoteManager.FindAsync(
                Builders<BusinessObjects.SickNote>.Filter.And(
                    Builders<BusinessObjects.SickNote>.Filter.Gte(x => x.StartDate, from),
                    Builders<BusinessObjects.SickNote>.Filter.Lte(x => x.EndDate, to)
                )
            );

            return new Dictionary<string, int>
            {
                { "Urlaubstage", leave.Sum(l => (l.EndDate - l.StartDate).Days + 1) },
                { "Krankheitstage", sick.Sum(s => (s.EndDate - s.StartDate).Days + 1) }
            };
        }

        #endregion

        #region Spesen, Reisekosten, Payroll

        public async Task<List<BusinessObjects.ExpenseReport>> GetExpenseReportsForEmployee(ObjectId employeeId)
        {
            var filter = Builders<BusinessObjects.ExpenseReport>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _expenseReportManager.FindAsync(filter);
        }

        public async Task<List<BusinessObjects.Payroll>> GetPayrollForEmployee(ObjectId employeeId)
        {
            var filter = Builders<BusinessObjects.Payroll>.Filter.Eq(x => x.EmployeeId, employeeId);
            return await _payrollManager.FindAsync(filter);
        }

        public async Task<decimal> GetTotalExpensesForProject(ObjectId projectId)
        {
            // Summe aller ExpenseReports, die Spesen für ein Projekt enthalten (falls Projektreferenz in ExpenseItem ergänzt)
            // Annahme: ExpenseItems enthalten ProjectId
            var reports = await _expenseReportManager.GetAllAsync();
            var sum = reports
                .SelectMany(r => r.Items ?? new List<BusinessObjects.ExpenseItem>())
                .Where(i => i.Description != null && i.Description.Contains(projectId.ToString()))
                .Sum(i => i.Amount);
            return sum;
        }

        #endregion

        #region Genehmigungsworkflows & Dokumente

        public async Task<List<BusinessObjects.ApprovalWorkflow>> GetOpenApprovalWorkflows()
        {
            var filter = Builders<BusinessObjects.ApprovalWorkflow>.Filter.Eq(x => x.Status, BusinessObjects.WorkflowStatus.Offen);
            return await _workflowManager.FindAsync(filter);
        }

        public async Task StartApprovalWorkflow(ObjectId entityId, BusinessObjects.WorkflowEntityType entityType, List<ObjectId> approverIds)
        {
            var steps = approverIds.Select((approver, idx) => new BusinessObjects.ApprovalStep
            {
                StepNumber = idx + 1,
                ApproverId = approver,
                Decision = BusinessObjects.ApprovalDecision.Offen
            }).ToList();

            var workflow = new BusinessObjects.ApprovalWorkflow
            {
                EntityId = entityId,
                EntityType = entityType,
                Steps = steps,
                Status = BusinessObjects.WorkflowStatus.Offen,
                CreatedAt = DateTime.UtcNow
            };
            await _workflowManager.InsertAsync(workflow);
        }

        public async Task<List<BusinessObjects.Document>> GetDocumentsForEntity(ObjectId entityId)
        {
            var filter = Builders<BusinessObjects.Document>.Filter.ElemMatch(x => x.LinkedEntities, de => de.EntityId == entityId);
            return await _documentManager.FindAsync(filter);
        }

        #endregion

        #region Reports & Analytics (für Geschäftsleitung, Projektleiter, Buchhaltung, Controlling etc.)

        // Umsatz pro Projekt
        public async Task<Dictionary<ObjectId, double>> GetProjectRevenueAnalytics(DateTime from, DateTime to)
        {
            var projects = await _projectManager.GetAllAsync();
            var result = new Dictionary<ObjectId, double>();
            foreach (var p in projects)
            {
                // Beispiel: Umsatz = billbare Stunden x Stundensatz (Stundensatz-Logik könnte im Projekt/Assignment gespeichert werden)
                double billableHours = await GetTotalBillableHoursForProject(p.Id);
                double hourlyRate = 120; // Kannst du auch als Parameter oder in Project speichern
                result[p.Id] = billableHours * hourlyRate;
            }
            return result;
        }

        // Top Performer/Utilization für Projektleiter
        public async Task<List<(ObjectId employeeId, double totalHours)>> GetTopConsultantUtilization(DateTime from, DateTime to, int topN = 5)
        {
            var utilization = await GetConsultantUtilization(from, to);
            return utilization.OrderByDescending(x => x.Value)
                .Take(topN)
                .Select(x => (x.Key, x.Value))
                .ToList();
        }

        // Offene ToDos, Genehmigungen, Tasks für Dashboards (Rollenbasiert)
        public async Task<int> GetPendingApprovalsCountForUser(ObjectId approverId)
        {
            var workflows = await GetOpenApprovalWorkflows();
            return workflows
                .SelectMany(w => w.Steps)
                .Count(s => s.ApproverId == approverId && s.Decision == BusinessObjects.ApprovalDecision.Offen);
        }

        // Fehlzeitenquote, Abwesenheiten-Heatmap, Bench-Report für HR
        public async Task<Dictionary<ObjectId, double>> GetEmployeeAbsenceRate(DateTime from, DateTime to)
        {
            var employees = await _employeeManager.GetAllAsync();
            var dict = new Dictionary<ObjectId, double>();
            foreach (var emp in employees)
            {
                var absences = await GetAbsenceStatistics(from, to);
                double days = 0;
                if (absences.TryGetValue("Urlaubstage", out int value))
                {
                    days += value;
                }
                if (absences.ContainsKey("Krankheitstage"))
                {
                    days += absences["Krankheitstage"];
                }
                dict[emp.Id] = days;
            }
            return dict;
        }

        // Forecast: Verfügbare Kapazität pro Berater, Projekt oder Abteilung (Future-Proof)
        public async Task<double> GetConsultantCapacityForecast(ObjectId employeeId, DateTime from, DateTime to, double plannedHoursPerDay = 8)
        {
            var workEntries = await GetWorkTimeEntriesForEmployee(employeeId, from, to);
            double actual = workEntries.Sum(w => w.Hours);
            double totalPlanned = ((to - from).TotalDays + 1) * plannedHoursPerDay;
            return totalPlanned - actual; // positive Zahl = verfügbare Kapazität
        }

        #endregion

        #region Sonstiges & Utility

        // Health Check, DB Connectivity, etc.
        public bool IsDbOnline() => _employeeManager.Ping();

        #endregion
    }
}
