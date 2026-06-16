using AgriSure.Operations.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgriSure.Operations.Api.Data;

public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options) : DbContext(options)
{
    public DbSet<ClaimProjection> Claims => Set<ClaimProjection>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClaimProjection>(entity =>
        {
            entity.ToTable("claim_projections");
            entity.HasKey(x => x.ClaimId);
            entity.HasIndex(x => new { x.TenantId, x.Status });
            entity.Property(x => x.TenantId).HasMaxLength(80);
            entity.Property(x => x.ClaimNumber).HasMaxLength(40);
            entity.Property(x => x.PolicyNumber).HasMaxLength(40);
            entity.Property(x => x.ProducerName).HasMaxLength(160);
            entity.Property(x => x.Crop).HasMaxLength(80);
            entity.Property(x => x.County).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(40);
            entity.Property(x => x.EstimatedIndemnity).HasPrecision(14, 2);
            entity.Property(x => x.LastNote).HasMaxLength(1000);
        });

        modelBuilder.Entity<ProcessedMessage>(entity =>
        {
            entity.ToTable("processed_messages");
            entity.HasKey(x => x.EventId);
            entity.Property(x => x.EventType).HasMaxLength(160);
        });
    }
}
