namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateInfoViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }

        /// <summary>Optional. When set, the template is only offered for schedules at this airport.</summary>
        public long? AirportId { get; set; }

        /// <summary>Optional. When set, the template is only offered for these sites.</summary>
        public List<long> SiteIds { get; set; } = new();

        // Options for the form
        public List<TemplateAirportOption> Airports { get; set; } = new();
        public List<TemplateSiteOption> Sites { get; set; } = new();
    }

    public record TemplateAirportOption(long Id, string Code, string Name);

    public record TemplateSiteOption(long Id, string Name, long AirportId, string AirportCode, string? Area);
}
