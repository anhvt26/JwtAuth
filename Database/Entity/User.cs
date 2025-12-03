namespace auth.Database.Entity;

public partial class User
{
    public long Id { get; set; }

    public string Uuid { get; set; } = null!;

    /// <summary>
    /// 1 - User (lowest role), 100 - Super Admin (highest role)
    /// </summary>
    public int Type { get; set; }

    public string? FullName { get; set; }

    public string? ShortName { get; set; }

    public int Status { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public int TokenTimes { get; set; }

    public string? Code { get; set; }

    public string? PhoneNumber { get; set; }

    public string? IdentityNumber { get; set; }

    public string? ProvinceId { get; set; }

    public string? WardId { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
