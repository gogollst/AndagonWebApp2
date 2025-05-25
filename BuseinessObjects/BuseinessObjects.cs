using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourNamespace.Models
{
    public static class BusinessObjects
    {
        // 1. Mitarbeiter (Employee)
        public class Employee
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public string PersonnelNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public DateTime HireDate { get; set; }
            public bool IsActive { get; set; }
        }

        // 2. Projekt (Project)
        public class Project
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public string ProjectNumber { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public List<ObjectId> MemberEmployeeIds { get; set; }
        }

        // 3. Arbeitszeiterfassung (WorkTimeEntry)
        public class WorkTimeEntry
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId EmployeeId { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public double Hours => (EndTime - StartTime).TotalHours;
            public string Description { get; set; }
            public bool IsBillable { get; set; }
            public ObjectId? ProjectId { get; set; }
        }

        // 4. Projektzeiterfassung (ProjectTimeEntry)
        public class ProjectTimeEntry
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId EmployeeId { get; set; }
            public ObjectId ProjectId { get; set; }
            public DateTime Date { get; set; }
            public double Hours { get; set; }
            public string Activity { get; set; }
            public string Description { get; set; }
        }

        // 5. Urlaubsantrag (LeaveRequest)
        public class LeaveRequest
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId EmployeeId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public LeaveType Type { get; set; }
            public string Reason { get; set; }
            public LeaveRequestStatus Status { get; set; }
            public DateTime RequestDate { get; set; }
            public DateTime? ApprovalDate { get; set; }
            public ObjectId? ApproverId { get; set; }
        }

        public enum LeaveType
        {
            Urlaub,       // bezahlter Urlaub
            Sonderurlaub, // z.B. Hochzeit, Umzug
            Unbezahlt,    // unbezahlter Urlaub
            Elternzeit,   // Elternzeit
            Pflegezeit    // Pflege Angehöriger
        }

        public enum LeaveRequestStatus
        {
            Beantragt,
            Genehmigt,
            Abgelehnt,
            Storniert
        }

        // 6. Krankmeldung (SickNote)
        public class SickNote
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId EmployeeId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string DiagnosisCode { get; set; }
            public bool DoctorCertificateProvided { get; set; }
            public DateTime NotificationDate { get; set; }
        }

        // 7. Spesenabrechnung (ExpenseReport)
        public class ExpenseReport
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId EmployeeId { get; set; }
            public DateTime Date { get; set; }
            public List<ExpenseItem> Items { get; set; }
            public ExpenseReportStatus Status { get; set; }
            public DateTime SubmissionDate { get; set; }
            public DateTime? ApprovalDate { get; set; }
            public ObjectId? ApproverId { get; set; }
            public string Comment { get; set; }
        }

        public class ExpenseItem
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ExpenseType Type { get; set; }
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public string Description { get; set; }
            public ObjectId? ReceiptFileId { get; set; }
            public bool IsTaxRelevant { get; set; }
            public bool IsDomestic { get; set; }
            public bool IsOvernight { get; set; }
        }

        public enum ExpenseType
        {
            Fahrtkosten,
            Übernachtung,
            Verpflegung,
            Sonstiges,
            Bewirtung
        }

        public enum ExpenseReportStatus
        {
            Angelegt,
            Eingereicht,
            Genehmigt,
            Abgelehnt,
            Erstattet
        }

        // 8. Payroll (Lohn- und Gehaltsabrechnung)
        public class Payroll
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId EmployeeId { get; set; }
            public DateTime PeriodStart { get; set; }
            public DateTime PeriodEnd { get; set; }
            public decimal GrossSalary { get; set; }
            public decimal NetSalary { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal SocialSecurityAmount { get; set; }
            public DateTime PayoutDate { get; set; }
            public PayrollStatus Status { get; set; }
            public List<ObjectId> DocumentIds { get; set; }
        }

        public enum PayrollStatus
        {
            Angelegt,
            Berechnet,
            Abgerechnet,
            Ausgezahlt,
            Fehler
        }

        // 9. Reisekostenantrag (TravelRequest)
        public class TravelRequest
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId EmployeeId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Destination { get; set; }
            public string Purpose { get; set; }
            public TravelRequestStatus Status { get; set; }
            public DateTime RequestDate { get; set; }
            public DateTime? ApprovalDate { get; set; }
            public ObjectId? ApproverId { get; set; }
            public List<ObjectId> LinkedExpenseReportIds { get; set; }
            public List<ObjectId> DocumentIds { get; set; }
        }

        public enum TravelRequestStatus
        {
            Beantragt,
            Genehmigt,
            Abgelehnt,
            Storniert,
            Abgeschlossen
        }

        // 10. Reisekostenposition (TravelExpense)
        public class TravelExpense
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId TravelRequestId { get; set; }
            public ObjectId EmployeeId { get; set; }
            public DateTime Date { get; set; }
            public TravelExpenseType Type { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public string Description { get; set; }
            public ObjectId? ReceiptDocumentId { get; set; }
            public bool IsApproved { get; set; }
        }

        public enum TravelExpenseType
        {
            Fahrtkosten,
            Übernachtung,
            Verpflegung,
            Sonstiges
        }

        // 11. Genehmigungsworkflow (ApprovalWorkflow)
        public class ApprovalWorkflow
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public ObjectId EntityId { get; set; }
            public WorkflowEntityType EntityType { get; set; }
            public List<ApprovalStep> Steps { get; set; }
            public WorkflowStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
        }

        public class ApprovalStep
        {
            public int StepNumber { get; set; }
            public ObjectId ApproverId { get; set; }
            public ApprovalDecision Decision { get; set; }
            public DateTime? DecisionDate { get; set; }
            public string Comment { get; set; }
        }

        public enum WorkflowEntityType
        {
            ExpenseReport,
            LeaveRequest,
            TravelRequest,
            Payroll,
            Other
        }

        public enum WorkflowStatus
        {
            Offen,
            InBearbeitung,
            Abgeschlossen,
            Abgelehnt
        }

        public enum ApprovalDecision
        {
            Offen,
            Genehmigt,
            Abgelehnt
        }

        // 12. Dokumentenupload (Document)
        public class Document
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public long Size { get; set; }
            public DateTime UploadDate { get; set; }
            public ObjectId UploadedBy { get; set; }
            public DocumentCategory Category { get; set; }
            public string StoragePath { get; set; }
            public string Description { get; set; }
            public List<DocumentReference> LinkedEntities { get; set; }
        }

        public enum DocumentCategory
        {
            Lohnabrechnung,
            Reisekosten,
            Krankmeldung,
            Urlaub,
            Vertrag,
            Sonstiges
        }

        public class DocumentReference
        {
            public ObjectId EntityId { get; set; }
            public WorkflowEntityType EntityType { get; set; }
            public string Description { get; set; }
        }
    }
}
