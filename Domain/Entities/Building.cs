using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CO4029_BE.Domain.Entities;

[Table("building")]
public class Building: BaseModel
{
    [PrimaryKey("id", false)]
    public string? ID { get; set; }
    [Column("content")]
    public string? Content { get; set; }
    [Column("name")]
    public string? Name { get; set; }
    [Column("latitude")]
    public double? Latitude { get; set; }
    [Column("longitude")]
    public double? Longitude { get; set; }
    [Column("user_id")]
    public string? UserId { get; set; }
}