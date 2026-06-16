using AgriSure.Claims.Api.Domain;
using AgriSure.Claims.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AgriSure.Claims.Api.Data;

public sealed class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimTimelineEntry> TimelineEntries => Set<ClaimTimelineEntry>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.ToTable("claims");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.ClaimNumber }).IsUnique();
            entity.Property(x => x.TenantId).HasMaxLength(80);
            entity.Property(x => x.ClaimNumber).HasMaxLength(40);
            entity.Property(x => x.PolicyNumber).HasMaxLength(40);
            entity.Property(x => x.ProducerActorId).HasMaxLength(80);
            entity.Property(x => x.ProducerName).HasMaxLength(160);
            entity.Property(x => x.Crop).HasMaxLength(80);
            entity.Property(x => x.County).HasMaxLength(100);
            entity.Property(x => x.FieldNumber).HasMaxLength(40);
            entity.Property(x => x.InsuredAcres).HasPrecision(10, 2);
            entity.Property(x => x.ApprovedYield).HasPrecision(10, 2);
            entity.Property(x => x.CoverageLevel).HasPrecision(5, 4);
            entity.Property(x => x.DemonstrationPrice).HasPrecision(10, 2);
            entity.Property(x => x.ActualProduction).HasPrecision(14, 2);
            entity.Property(x => x.EstimatedIndemnity).HasPrecision(14, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.LossCause).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.InspectionNotes).HasMaxLength(4000);
            entity.Property(x => x.AssignedAdjusterId).HasMaxLength(80);
            entity.Property(x => x.AssignedAdjusterName).HasMaxLength(160);
            entity.HasMany(x => x.Timeline)
                .WithOne()
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClaimTimelineEntry>(entity =>
        {
            entity.ToTable("claim_timeline");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ClaimId, x.OccurredAtUtc });
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.Property(x => x.ActorName).HasMaxLength(160);
            entity.Property(x => x.ActorRole).HasMaxLength(80);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ProcessedAtUtc, x.OccurredAtUtc });
            entity.Property(x => x.EventType).HasMaxLength(160);
            entity.Property(x => x.RoutingKey).HasMaxLength(160);
            entity.Property(x => x.Payload).HasColumnType("jsonb");
            entity.Property(x => x.LastError).HasMaxLength(1000);
        });
    }
}
