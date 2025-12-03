using System;
using System.Collections.Generic;

namespace JwtAuth.Database.Entity;

public partial class Role
{
    public int Id { get; set; }

    public string Uuid { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int Specialpermission { get; set; }

    public string? AccountUuid { get; set; }

    /// <summary>
    /// 0 - Bị xóa
    /// </summary>
    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
