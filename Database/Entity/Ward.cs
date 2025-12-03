using System;
using System.Collections.Generic;

namespace auth.Database.Entity;

public partial class Ward
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? FullName { get; set; }

    public string? FullNameEn { get; set; }

    public string? CodeName { get; set; }

    public string? ProvinceCode { get; set; }

    public int? AdministrativeUnitId { get; set; }

    public virtual AdministrativeUnit? AdministrativeUnit { get; set; }

    public virtual Province? ProvinceCodeNavigation { get; set; }
}
