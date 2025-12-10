using System;
using System.Collections.Generic;

namespace JwtAuth.Database.Entity;

public partial class Device
{
    public long Id { get; set; }

    public string Uuid { get; set; } = null!;

    /// <summary>
    /// mobile, desktop, tablet
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// ip 17
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// operating system
    /// </summary>
    public string? OS { get; set; }

    public string? Browser { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();
}
