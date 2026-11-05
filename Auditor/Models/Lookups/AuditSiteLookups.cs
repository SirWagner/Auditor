namespace Auditor.Models.Lookups;

/// <summary>
/// Fixed dropdown values and dependency rules for the Audit Site module.
/// </summary>
public static class AuditSiteLookups
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly IReadOnlyList<string> Statuses = new[] { Active, Inactive };

    public const string Operations = "Operations";
    public const string Commercial = "Commercial";

    public const string SubTerminal = "Terminal";
    public const string SubAirside = "Airside";
    public const string SubLandside = "Landside";

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> SubCategoriesByCategory =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [Operations] = new[] { SubTerminal, SubAirside, SubLandside },
            [Commercial] = new[] { "Food & Beverage", "Retail", "Bank", "Services", "Car Rental" },
        };

    public static IEnumerable<string> Categories => SubCategoriesByCategory.Keys;

    /// <summary>Site types that come from a fixed list rather than from the airport's own locations.</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> FixedSiteTypes =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["Food & Beverage"] = new[] { "Sit-Down", "Take-Away", "Sit-Down with Take-Away" },
            ["Retail"] = new[] { "Retail" },
            ["Bank"] = new[] { "Bank" },
            ["Services"] = new[] { "Forex", "Pharmacy", "Information Desk", "Other Services" },
            ["Car Rental"] = new[] { "Car Rental" },
        };

    /// <summary>
    /// The standard ACSA checklist a new site of this kind is linked to (see SeedData/acsa-seed.json).
    /// Null when there's no single obvious template; the user links one on the site's page instead.
    /// </summary>
    public static string? DefaultTemplateFor(string? subCategory, string? siteType) => (subCategory, siteType) switch
    {
        ("Food & Beverage", "Sit-Down") => "Commercial – F&B Sit-Down",
        ("Food & Beverage", "Sit-Down with Take-Away") => "Commercial – F&B Sit-Down with Take-Away",
        ("Food & Beverage", "Take-Away") => "Commercial – F&B Take-Away",
        ("Retail", _) => "Commercial – Retail",
        ("Bank", _) or ("Services", _) => "Commercial – Services",
        ("Car Rental", _) => "Commercial – Car Rental",
        _ => null,
    };

    /// <summary>Operations sub-categories take their site types from the airport's locations of this area type.</summary>
    public static readonly IReadOnlyDictionary<string, string> LocationAreaForSubCategory =
        new Dictionary<string, string>
        {
            [SubTerminal] = AirportLookups.TerminalLocation,
            [SubAirside] = AirportLookups.Airside,
            [SubLandside] = AirportLookups.Landside,
        };

    public static bool IsValidSubCategory(string? category, string? subCategory) =>
        category != null && subCategory != null
        && SubCategoriesByCategory.TryGetValue(category, out var subs)
        && subs.Contains(subCategory);

    public static bool RequiresTerminal(string? category, string? subCategory) =>
        category == Operations && subCategory == SubTerminal;
}
