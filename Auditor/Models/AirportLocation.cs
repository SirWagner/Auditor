using System;
using System.Collections.Generic;

namespace Auditor.Models;

/// <summary>
/// A named area within an airport (a terminal building, a terminal section, an airside or a landside location).
/// One airport has many of each, which is what allows the areas to be multi-selected.
/// </summary>
public partial class AirportLocation
{
    public long Id { get; set; }

    public long AirportId { get; set; }

    /// <summary>One of <see cref="Lookups.AirportLookups.AreaTypes"/>.</summary>
    public string AreaType { get; set; }

    public string Name { get; set; }

    public virtual Airport Airport { get; set; }

    public virtual ICollection<AuditSite> TerminalAuditSites { get; set; } = new List<AuditSite>();
}
