using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using JJManager.Core.Database.Entities;

namespace JJManager.Infrastructure.Database;

/// <summary>
/// Entity Framework Core DbContext for JJManager
/// Uses SQLite for cross-platform compatibility
/// </summary>
public class JJManagerDbContext : DbContext
{
    public DbSet<ConfigEntity> Configs { get; set; } = null!;
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<UserProductEntity> UserProducts { get; set; } = null!;
    public DbSet<ProfileEntity> Profiles { get; set; } = null!;
    public DbSet<InputEntity> Inputs { get; set; } = null!;
    public DbSet<OutputEntity> Outputs { get; set; } = null!;

    public JJManagerDbContext(DbContextOptions<JJManagerDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Suppress pending model changes warning since we use EnsureCreated() instead of Migrate()
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Config entity
        modelBuilder.Entity<ConfigEntity>(entity =>
        {
            entity.ToTable("configs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired();
        });

        // Product entity
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("jj_products");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProductName, e.ConnectionType }).IsUnique();
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConnectionType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ClassName).HasMaxLength(100);
        });

        // UserProduct entity
        modelBuilder.Entity<UserProductEntity>(entity =>
        {
            entity.ToTable("user_products");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ConnectionId, e.ProductId }).IsUnique();
            entity.Property(e => e.ConnectionId).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.UserProducts)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Profile)
                .WithMany(p => p.UserProducts)
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Profile entity
        modelBuilder.Entity<ProfileEntity>(entity =>
        {
            entity.ToTable("profiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Profiles)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Input entity
        modelBuilder.Entity<InputEntity>(entity =>
        {
            entity.ToTable("inputs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProfileId, e.Index }).IsUnique();
            entity.Property(e => e.Mode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Profile)
                .WithMany(p => p.Inputs)
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Output entity
        modelBuilder.Entity<OutputEntity>(entity =>
        {
            entity.ToTable("outputs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProfileId, e.Index }).IsUnique();
            entity.Property(e => e.Mode).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Profile)
                .WithMany(p => p.Outputs)
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed products - using fixed GUIDs for consistency
        modelBuilder.Entity<ProductEntity>().HasData(
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000001"), ProductName = "Streamdeck JJSD-01", ConnectionType = "HID", ClassName = "JJSD01" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000002"), ProductName = "Mixer de Áudio JJM-01", ConnectionType = "HID", ClassName = "JJM01" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000003"), ProductName = "ButtonBox JJB-01", ConnectionType = "Joystick", ClassName = "JJB01" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000004"), ProductName = "ButtonBox JJB-01 V2", ConnectionType = "HID", ClassName = "JJB01_V2" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000005"), ProductName = "ButtonBox JJBP-06", ConnectionType = "HID", ClassName = "JJBP06" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000006"), ProductName = "ButtonBox JJB-999", ConnectionType = "HID", ClassName = "JJB999" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000007"), ProductName = "Hub ARGB JJHL-01", ConnectionType = "HID", ClassName = "JJHL01" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000008"), ProductName = "Hub ARGB JJHL-01 Plus", ConnectionType = "HID", ClassName = "JJHL01" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000009"), ProductName = "Hub RGB JJHL-02", ConnectionType = "HID", ClassName = "JJHL02" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000010"), ProductName = "Hub RGB JJHL-02 Plus", ConnectionType = "HID", ClassName = "JJHL02" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000011"), ProductName = "Dashboard JJDB-01", ConnectionType = "HID", ClassName = "JJDB01" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000012"), ProductName = "LoadCell JJLC-01", ConnectionType = "HID", ClassName = "JJLC01" },
            new ProductEntity { Id = new Guid("10000001-0000-0000-0000-000000000013"), ProductName = "ButtonBox JJB-Slim Type A", ConnectionType = "HID", ClassName = "JJBSlim_A" }
        );

        // Seed initial config - using fixed GUIDs for consistency
        modelBuilder.Entity<ConfigEntity>().HasData(
            new ConfigEntity { Id = new Guid("20000001-0000-0000-0000-000000000001"), Key = "software_version", Value = "2.0.0", Description = "Current software version" },
            new ConfigEntity { Id = new Guid("20000001-0000-0000-0000-000000000002"), Key = "theme", Value = "dark", Description = "UI theme (dark/light)" },
            new ConfigEntity { Id = new Guid("20000001-0000-0000-0000-000000000003"), Key = "auto_start", Value = "false", Description = "Start with system" },
            new ConfigEntity { Id = new Guid("20000001-0000-0000-0000-000000000004"), Key = "minimize_to_tray", Value = "true", Description = "Minimize to system tray" }
        );
    }
}
