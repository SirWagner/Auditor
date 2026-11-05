using System.ComponentModel.DataAnnotations;

namespace Auditor.ViewModels.Users
{
    public class AppUserFormViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(255, ErrorMessage = "Full name cannot exceed 255 characters.")]
        [Display(Name = "Full Name")]
        public string? DisplayName { get; set; }

        [Required(ErrorMessage = "Employee number is required.")]
        [StringLength(30, ErrorMessage = "Employee number cannot exceed 30 characters.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9\-/]*$", ErrorMessage = "Employee number can contain letters, numbers, - and /.")]
        [Display(Name = "Employee Number")]
        public string? EmployeeNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(255)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string? Role { get; set; }

        /// <summary>Null = all airports.</summary>
        [Display(Name = "Airport")]
        public long? AirportId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>Required on create; on edit, leave blank to keep the current password.</summary>
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        public bool IsNew => Id == 0;
    }
}
