namespace Auditor.Models;

/// <summary>A managed list that questions can be answered from (airlines, aircraft types, ground handlers…).</summary>
public class ReferenceList
{
    public long Id { get; set; }

    /// <summary>Stable key for lists the application relies on, e.g. "GROUND_HANDLERS". Null for lists users add.</summary>
    public string? Code { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<ReferenceListItem> Items { get; set; } = new List<ReferenceListItem>();

    public virtual ICollection<QuestionBank> Questions { get; set; } = new List<QuestionBank>();
}

public class ReferenceListItem
{
    public long Id { get; set; }

    public long ListId { get; set; }

    public string Name { get; set; } = "";

    /// <summary>Optional short code, e.g. an airline's IATA code.</summary>
    public string? Code { get; set; }

    /// <summary>Optional grouping shown in the dropdown, e.g. "Africa". For aircraft types it is the body type.</summary>
    public string? GroupName { get; set; }

    /// <summary>When set, the item is only offered at this airport (parking bays). Null offers it everywhere.</summary>
    public long? AirportId { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public virtual ReferenceList List { get; set; } = null!;

    public virtual Airport? Airport { get; set; }
}
