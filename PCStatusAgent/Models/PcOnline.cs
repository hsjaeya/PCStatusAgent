using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PCStatusAgent.Models;

[Table("pc_online")]
public class PcOnline : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("last_seen")]
    public DateTime LastSeen { get; set; }

    [Column("pc_name")]
    public string? PcName { get; set; }

    [Column("cpu_percent")]
    public float? CpuPercent { get; set; }

    [Column("ram_percent")]
    public float? RamPercent { get; set; }

    [Column("ram_used")]
    public float? RamUsed { get; set; }

    [Column("ram_total")]
    public float? RamTotal { get; set; }

    [Column("temperature")]
    public float? Temperature { get; set; }
}