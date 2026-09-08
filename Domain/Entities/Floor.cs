using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CO4029_BE.Domain.Entities;

[Table("floor")]
public class Floor : BaseModel
{
    [PrimaryKey("id", false)]
    public string? Id { get; set; }

    [Column("building_id")]
    public string? BuildingId { get; set; }

    [Column("floor_number")]
    public int FloorNumber { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("floor_plan_url")]
    public string? FloorPlanUrl { get; set; }

    [Column("source_url")]
    public string? SourceUrl { get; set; }

    [Column("verified")]
    public bool Verified { get; set; }
}
