using System;
using System.Collections.Generic;

namespace JwtAuth.Database.Entity;

public partial class AdministrativeRegion
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? CodeName { get; set; }

    public string? CodeNameEn { get; set; }
}
