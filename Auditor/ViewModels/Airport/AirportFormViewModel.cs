using System.ComponentModel.DataAnnotations;
using Auditor.Models.Lookups;

namespace Auditor.ViewModels.Airport
{
    /// <summary>
    /// Create/Edit form for an airport, including its multi-valued areas.
    /// </summary>
    public class AirportFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Airport Code is required.")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "Airport Code must be 3 to 10 characters.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Airport Code can only contain letters and numbers.")]
        [Display(Name = "Airport Code")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Airport Name is required.")]
        [StringLength(200, ErrorMessage = "Airport Name cannot exceed 200 characters.")]
        [Display(Name = "Airport Name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Airport Type is required.")]
        [Display(Name = "Airport Type")]
        public string? AirportType { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Region is required.")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "Airport Manager is required.")]
        [StringLength(200, ErrorMessage = "Airport Manager cannot exceed 200 characters.")]
        [Display(Name = "Airport Manager")]
        public string? AirportManager { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address, e.g. ops@airport.co.za.")]
        [StringLength(255, ErrorMessage = "Email Address cannot exceed 255 characters.")]
        [Display(Name = "Email Address")]
        public string? EmailAddress { get; set; }

        [StringLength(20, ErrorMessage = "Tel Number cannot exceed 20 characters.")]
        [RegularExpression(@"^\+?[0-9 ()-]{7,20}$", ErrorMessage = "Enter a valid telephone number, e.g. +27 11 921 6262.")]
        [Display(Name = "Tel Number")]
        public string? TelNumber { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public List<string> Terminals { get; set; } = new();

        public List<string> TerminalLocations { get; set; } = new();

        public List<string> AirsideLocations { get; set; } = new();

        public List<string> LandsideLocations { get; set; } = new();

        /// <summary>Area lists keyed by <see cref="AirportLookups.AreaTypes"/> key.</summary>
        public IEnumerable<(string AreaType, List<string> Names)> AreaLists => new[]
        {
            (AirportLookups.Terminal, Terminals),
            (AirportLookups.TerminalLocation, TerminalLocations),
            (AirportLookups.Airside, AirsideLocations),
            (AirportLookups.Landside, LandsideLocations),
        };
    }
}
