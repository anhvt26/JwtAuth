using System;
using System.Collections.Generic;
using JwtAuth.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Database;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdministrativeRegion> AdministrativeRegions { get; set; }

    public virtual DbSet<AdministrativeUnit> AdministrativeUnits { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Ward> Wards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_uca1400_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AdministrativeRegion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("administrative_regions");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name")
                .UseCollation("utf8mb4_unicode_520_ci");
            entity.Property(e => e.CodeNameEn)
                .HasMaxLength(255)
                .HasColumnName("code_name_en")
                .UseCollation("utf8mb4_unicode_520_ci");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name")
                .UseCollation("utf8mb4_unicode_520_ci");
            entity.Property(e => e.NameEn)
                .HasMaxLength(255)
                .HasColumnName("name_en")
                .UseCollation("utf8mb4_unicode_520_ci");
        });

        modelBuilder.Entity<AdministrativeUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("administrative_units");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name")
                .UseCollation("utf8mb4_unicode_520_ci");
            entity.Property(e => e.CodeNameEn)
                .HasMaxLength(255)
                .HasColumnName("code_name_en")
                .UseCollation("utf8mb4_unicode_520_ci");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name")
                .UseCollation("utf8mb4_unicode_520_ci");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(255)
                .HasColumnName("full_name_en")
                .UseCollation("utf8mb4_unicode_520_ci");
            entity.Property(e => e.ShortName)
                .HasMaxLength(255)
                .HasColumnName("short_name")
                .UseCollation("utf8mb4_unicode_520_ci");
            entity.Property(e => e.ShortNameEn)
                .HasMaxLength(255)
                .HasColumnName("short_name_en")
                .UseCollation("utf8mb4_unicode_520_ci");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity
                .ToTable("provinces")
                .UseCollation("utf8mb4_unicode_520_ci");

            entity.HasIndex(e => e.AdministrativeUnitId, "idx_provinces_unit");

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.AdministrativeUnitId)
                .HasColumnType("int(11)")
                .HasColumnName("administrative_unit_id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(255)
                .HasColumnName("full_name_en");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NameEn)
                .HasMaxLength(255)
                .HasColumnName("name_en");

            entity.HasOne(d => d.AdministrativeUnit).WithMany(p => p.Provinces)
                .HasForeignKey(d => d.AdministrativeUnitId)
                .HasConstraintName("provinces_administrative_unit_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("roles")
                .UseCollation("utf8mb4_unicode_520_ci");

            entity.HasIndex(e => e.AccountUuid, "roles_account__fk");

            entity.HasIndex(e => e.Uuid, "roles_uuid_index").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.AccountUuid)
                .HasMaxLength(36)
                .HasColumnName("account_uuid");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .HasColumnName("name");
            entity.Property(e => e.Specialpermission)
                .HasColumnType("int(11)")
                .HasColumnName("specialpermission");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("0 - Bị xóa")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Uuid)
                .HasMaxLength(36)
                .HasDefaultValueSql("uuid()")
                .HasColumnName("uuid");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("users")
                .UseCollation("utf8mb4_unicode_520_ci");

            entity.HasIndex(e => e.Uuid, "users_uuid_uindex").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20)")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");
            entity.Property(e => e.Count)
                .HasColumnType("int(22)")
                .HasColumnName("count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.IdentityNumber)
                .HasMaxLength(20)
                .HasColumnName("identity_number");
            entity.Property(e => e.Password)
                .HasColumnType("text")
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(10)
                .HasColumnName("phone_number");
            entity.Property(e => e.ProvinceId)
                .HasMaxLength(20)
                .HasColumnName("province_id");
            entity.Property(e => e.RefreshRootExpireAt)
                .HasColumnType("timestamp")
                .HasColumnName("refresh_root_expire_at");
            entity.Property(e => e.ShortName)
                .HasMaxLength(255)
                .HasColumnName("short_name");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("status");
            entity.Property(e => e.TokenTimes)
                .HasColumnType("int(11)")
                .HasColumnName("token_times");
            entity.Property(e => e.Type)
                .HasDefaultValueSql("'1'")
                .HasComment("1 - User (lowest role), 100 - Super Admin (highest role)")
                .HasColumnType("int(11)")
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsFixedLength()
                .HasColumnName("username");
            entity.Property(e => e.Uuid)
                .HasMaxLength(36)
                .HasDefaultValueSql("uuid()")
                .IsFixedLength()
                .HasColumnName("uuid");
            entity.Property(e => e.WardId)
                .HasMaxLength(20)
                .HasColumnName("ward_id");
        });

        modelBuilder.Entity<Ward>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity
                .ToTable("wards")
                .UseCollation("utf8mb4_unicode_520_ci");

            entity.HasIndex(e => e.ProvinceCode, "idx_wards_province");

            entity.HasIndex(e => e.AdministrativeUnitId, "idx_wards_unit");

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.AdministrativeUnitId)
                .HasColumnType("int(11)")
                .HasColumnName("administrative_unit_id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(255)
                .HasColumnName("full_name_en");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NameEn)
                .HasMaxLength(255)
                .HasColumnName("name_en");
            entity.Property(e => e.ProvinceCode)
                .HasMaxLength(20)
                .HasColumnName("province_code");

            entity.HasOne(d => d.AdministrativeUnit).WithMany(p => p.Wards)
                .HasForeignKey(d => d.AdministrativeUnitId)
                .HasConstraintName("wards_administrative_unit_id_fkey");

            entity.HasOne(d => d.ProvinceCodeNavigation).WithMany(p => p.Wards)
                .HasForeignKey(d => d.ProvinceCode)
                .HasConstraintName("wards_province_code_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
