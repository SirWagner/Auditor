namespace Auditor.Models.Lookups
{
    /// <summary>Codes of the lists the application itself relies on. These lists can be edited but not deleted.</summary>
    public static class ReferenceLists
    {
        public const string Airlines = "AIRLINES";
        public const string AircraftTypes = "AIRCRAFT_TYPES";
        public const string BodyTypes = "BODY_TYPES";
        public const string GroundHandlers = "GROUND_HANDLERS";
        public const string ParkingBays = "PARKING_BAYS";
        public const string ServiceProviders = "SERVICE_PROVIDERS";
        public const string Responsibilities = "RESPONSIBILITIES";
    }

    /// <summary>Responsibility values with special meaning in reports.</summary>
    public static class Responsibilities
    {
        public const string None = "No Responsibility";
        public const string Acsa = "ACSA";
        public const string RampHandling = "Ramp Handling Company";
        public const string Cleaning = "Cleaning Company";
        public const string Concessionaire = "Concessionaire";
        public const string Airline = "Airline";

        /// <summary>Shown when neither the template question nor the question bank says who is responsible.</summary>
        public const string NotAssigned = "Not assigned";

        /// <summary>Shown when the responsibility is known but no company is named.</summary>
        public const string NotNamed = "Not named";
    }
}
