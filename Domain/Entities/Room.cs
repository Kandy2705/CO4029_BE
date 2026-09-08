using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CO4029_BE.Domain.Entities;

[Table("room")]
public class Room : BaseModel
{
    [PrimaryKey("id", false)]
    public string? Id { get; set; }

    [Column("floor_id")]
    public string? FloorId { get; set; }

    [Column("room_code")]
    public string? RoomCode { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("room_type")]
    public string? RoomType { get; set; }

    [Column("local_x")]
    public float? LocalX { get; set; }

    [Column("local_y")]
    public float? LocalY { get; set; }

    [Column("local_z")]
    public float? LocalZ { get; set; }

    [Column("source_url")]
    public string? SourceUrl { get; set; }

    [Column("verified")]
    public bool Verified { get; set; }
}
