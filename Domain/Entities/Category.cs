using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CO4029_BE.Domain.Entities;

[Table("category")]
public class Category: BaseModel
{
    [PrimaryKey("id", false)]
    public string? ID { get; set; }
    [Column("name")]
    public string Name { get; set; } = default!;
}
