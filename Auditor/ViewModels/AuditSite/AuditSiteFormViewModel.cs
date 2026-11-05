using System.ComponentModel.DataAnnotations;
using Auditor.Models.Lookups;

namespace Auditor.ViewModels.AuditSite
{
    /// <summary>
    /// Create/Edit form for an audit site.
    /// </summary>
    public class AuditSiteFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Airport is required.")]
        [Display(Name = "Airport")]
        public long? AirportId { get; set; }

        [Display(Name = "Terminal")]
        public long? TerminalId { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public long? DepartmentId { get; set; }

        [Required(ErrorMessage = "Site Name is required.")]
        [StringLength(200, ErrorMessage = "Site Name cannot exceed 200 characters.")]
        [Display(Name = "Site Name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Site Category is required.")]
        [Display(Name = "Site Category")]
        public string? SiteCategory { get; set; }

        [Required(ErrorMessage = "Site Sub-Category is required.")]
        [Display(Name = "Site Sub-Category")]
        public string? SiteSubCategory { get; set; }

        [Required(ErrorMessage = "Site Type is required.")]
        [Display(Name = "Site Type")]
        public string? SiteType { get; set; }

        [StringLength(200, ErrorMessage = "Service Provider cannot exceed 200 characters.")]
        [Display(Name = "Service Provider")]
        public string? ServiceProvider { get; set; }

        [StringLength(150, ErrorMessage = "Contact Name cannot exceed 150 characters.")]
        [Display(Name = "Contact Name")]
        public string? ContactName { get; set; }

        [EmailAddress(ErrorMessage = "Enter a valid contact email address.")]
        [StringLength(255, ErrorMessage = "Contact Email cannot exceed 255 characters.")]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [StringLength(20, ErrorMessage = "Contact Number cannot exceed 20 characters.")]
        [RegularExpression(@"^\+?[0-9 ()-]{7,20}$", ErrorMessage = "Enter a valid contact number, e.g. +27 41 507 7319.")]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = AuditSiteLookups.Active;
    }
}
