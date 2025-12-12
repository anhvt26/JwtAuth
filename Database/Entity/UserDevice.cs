using System;
using System.Collections.Generic;

namespace JwtAuth.Database.Entity;

public partial class UserDevice
{
    public long Id { get; set; }

    public string Uuid { get; set; } = null!;

    public string UserUuid { get; set; } = null!;

    public string DeviceUuid { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    /// <summary>
    /// 0 - not revoked, 1 - revoked
    /// </summary>
    public int? IsRevoked { get; set; }

    public DateTime? RefreshRootExpireAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Device DeviceUu { get; set; } = null!;

    public virtual User UserUu { get; set; } = null!;
}
