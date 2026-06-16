using AgriSure.Policy.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgriSure.Policy.Api.Data;

public sealed class PolicyDbContext(DbContextOptions<PolicyDbContext> options) : DbContext(options)
{
    public DbSet<Producer> Producers => Set<Producer>();
    public DbSet<CropPolicy> Policies => Set<CropPolicy>();
    public DbSet<InsuredField> Fields => Set<InsuredField>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producer>(entity =>
        {
            entity.ToTable("producers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.ExternalActorId }).IsUnique();
            entity.Property(x => x.TenantId).HasMaxLength(80);
            entity.Property(x => x.ExternalActorId).HasMaxLength(80);
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.Email).HasMaxLength(200);
        });

        modelBuilder.Entity<CropPolicy>(entity =>
        {
            entity.ToTable("policies");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.PolicyNumber }).IsUnique();
            entity.Property(x => x.TenantId).HasMaxLength(80);
            entity.Property(x => x.PolicyNumber).HasMaxLength(40);
            entity.Property(x => x.ProducerName).HasMaxLength(160);
            entity.Property(x => x.Crop).HasMaxLength(80);
            entity.Property(x => x.County).HasMaxLength(100);
            entity.Property(x => x.State).HasMaxLength(2);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.Property(x => x.CoverageLevel).HasPrecision(5, 4);
            entity.Property(x => x.ApprovedYield).HasPrecision(10, 2);
            entity.Property(x => x.DemonstrationPrice).HasPrecision(10, 2);
            entity.HasMany(x => x.Fields)
                .WithOne()
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InsuredField>(entity =>
        {
            entity.ToTable("insured_fields");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PolicyId, x.FieldNumber }).IsUnique();
            entity.Property(x => x.FieldNumber).HasMaxLength(40);
            entity.Property(x => x.FarmNumber).HasMaxLength(40);
            entity.Property(x => x.TractNumber).HasMaxLength(40);
            entity.Property(x => x.InsuredAcres).HasPrecision(10, 2);
            entity.Property(x => x.GeoJson).HasColumnType("jsonb");
        });
    }
}
