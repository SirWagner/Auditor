namespace Auditor.Security
{
    /// <summary>
    /// Application roles. Stored as plain text in app_user.role and issued as the role claim at sign-in.
    /// </summary>
    public static class Roles
    {
        /// <summary>Full access, including user management.</summary>
        public const string Admin = "Admin";

        /// <summary>Manages airports, sites, questions, templates and schedules; reviews audits.</summary>
        public const string Manager = "Manager";

        /// <summary>Accepts and executes the audits assigned to them.</summary>
        public const string Auditor = "Auditor";

        /// <summary>Read-only access to the dashboard and audit results.</summary>
        public const string Viewer = "Viewer";

        public static readonly IReadOnlyList<string> All = new[] { Admin, Manager, Auditor, Viewer };

        public static readonly IReadOnlyDictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            [Admin] = "Full access, including user management",
            [Manager] = "Manages airports, sites, questions, templates and schedules; reviews audits",
            [Auditor] = "Accepts and executes the audits assigned to them",
            [Viewer] = "Read-only access to the dashboard and audit results",
        };

        // Comma-separated lists for [Authorize(Roles = ...)].
        public const string AdminOrManager = Admin + "," + Manager;
        public const string Reporting = Admin + "," + Manager + "," + Viewer;
        public const string Staff = Admin + "," + Manager + "," + Auditor;
        public const string Everyone = Admin + "," + Manager + "," + Auditor + "," + Viewer;
    }
}
