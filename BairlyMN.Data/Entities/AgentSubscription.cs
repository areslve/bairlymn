using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// BairlyMN.Data/Entities/AgentSubscription.cs

namespace BairlyMN.Data.Entities;

public class AgentSubscription
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    // Хугацаа
    public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime EndsAt { get; set; }

    // Admin олгосон
    public string GrantedByAdminId { get; set; } = string.Empty;
    public ApplicationUser GrantedByAdmin { get; set; } = null!;

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    // Тэмдэглэл (төлбөр хэрхэн орсон гэх мэт)
    public string? Note { get; set; }

    // Идэвхтэй эсэх (admin хасч болно)
    public bool IsActive { get; set; } = true;

    // Computed
    public bool IsValid => IsActive && EndsAt >= DateTime.UtcNow;
}