namespace Auditor.Models.Lookups;

/// <summary>
/// Fixed dropdown values for the Airport module. Edit these lists to change what users can pick.
/// </summary>
public static class AirportLookups
{
    public static readonly IReadOnlyList<string> AirportTypes = new[]
    {
        "International",
        "Domestic",
        "Regional",
        "Military",
        "Private / General Aviation"
    };

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> RegionsByCountry =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["Botswana"] = new[] { "Central", "Chobe", "Ghanzi", "Kgalagadi", "Kgatleng", "Kweneng", "North-East", "North-West", "South-East", "Southern" },
            ["Eswatini"] = new[] { "Hhohho", "Lubombo", "Manzini", "Shiselweni" },
            ["Lesotho"] = new[] { "Berea", "Butha-Buthe", "Leribe", "Mafeteng", "Maseru", "Mohale's Hoek", "Mokhotlong", "Qacha's Nek", "Quthing", "Thaba-Tseka" },
            ["Malawi"] = new[] { "Central", "Northern", "Southern" },
            ["Mozambique"] = new[] { "Cabo Delgado", "Gaza", "Inhambane", "Manica", "Maputo City", "Maputo Province", "Nampula", "Niassa", "Sofala", "Tete", "Zambezia" },
            ["Namibia"] = new[] { "Erongo", "Hardap", "//Kharas", "Kavango East", "Kavango West", "Khomas", "Kunene", "Ohangwena", "Omaheke", "Omusati", "Oshana", "Oshikoto", "Otjozondjupa", "Zambezi" },
            ["South Africa"] = new[] { "Eastern Cape", "Free State", "Gauteng", "KwaZulu-Natal", "Limpopo", "Mpumalanga", "North West", "Northern Cape", "Western Cape" },
            ["Zambia"] = new[] { "Central", "Copperbelt", "Eastern", "Luapula", "Lusaka", "Muchinga", "North-Western", "Northern", "Southern", "Western" },
            ["Zimbabwe"] = new[] { "Bulawayo", "Harare", "Manicaland", "Mashonaland Central", "Mashonaland East", "Mashonaland West", "Masvingo", "Matabeleland North", "Matabeleland South", "Midlands" },
        };

    public static IEnumerable<string> Countries => RegionsByCountry.Keys.OrderBy(c => c);

    // Area types an airport can hold several of.
    public const string Terminal = "Terminal";
    public const string TerminalLocation = "TerminalLocation";
    public const string Airside = "Airside";
    public const string Landside = "Landside";

    public static readonly IReadOnlyDictionary<string, string> AreaTypes = new Dictionary<string, string>
    {
        [Terminal] = "Terminals",
        [TerminalLocation] = "Terminal Locations",
        [Airside] = "Airside Locations",
        [Landside] = "Landside Locations",
    };

    public static bool IsValidRegion(string? country, string? region) =>
        country != null && region != null
        && RegionsByCountry.TryGetValue(country, out var regions)
        && regions.Contains(region);
}
