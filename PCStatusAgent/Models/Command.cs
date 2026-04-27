
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PCStatusAgent.Models;

[Table("commands")]
public class Command : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("command")]
    public string CommandType { get; set; } = string.Empty;

    [Column("is_executed")]
    public bool IsExecuted { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}