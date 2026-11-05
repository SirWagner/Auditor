using System.ComponentModel.DataAnnotations;

namespace Auditor.ViewModels.AuditSchedule
{
    /// <summary>
    /// Create/Edit form for an audit schedule: which template is run where, when, and by whom.
    /// </summary>
    public class AuditScheduleFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Airport is required.")]
        [Display(Name = "Airport")]
        public long? AirportId { get; set; }

        [Required(ErrorMessage = "Audit site is required.")]
        [Display(Name = "Audit Site")]
        public long? SiteId { get; set; }

        [Required(ErrorMessage = "Audit template is required.")]
        [Display(Name = "Audit Template")]
        public long? TemplateId { get; set; }

        [Required(ErrorMessage = "Scheduled date is required.")]
        [Display(Name = "Scheduled Date")]
        public DateTime? ScheduledDate { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Auditors")]
        public List<long> AuditorIds { get; set; } = new();

        /// <summary>Required when an existing schedule is changed.</summary>
        [StringLength(1000)]
        [Display(Name = "Reason for change")]
        public string? ModificationReason { get; set; }

        public bool IsNew => Id == 0;
    }
}

namespace Auditor.ViewModels.AuditSchedule
{
    public sealed record TemplateOption(long Id, string Name, string Version, int Questions);
}
