using AgriSure.Policy.Api.Data;
using AgriSure.Policy.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgriSure.Policy.Api.Infrastructure;

public static class PolicySeed
{
    public static readonly Guid ProducerId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid PolicyId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid NorthFieldId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid SouthFieldId = Guid.Parse("30000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(PolicyDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Policies.AnyAsync(cancellationToken))
        {
            return;
        }

        var producer = new Producer(
            ProducerId,
            "northstar-agency",
            "producer-1001",
            "Jordan Miller",
            "jordan.miller@example.test");

        var policy = new CropPolicy(
            PolicyId,
            "northstar-agency",
            "MPCI-MN-2026-00421",
            ProducerId,
            producer.Name,
            "Corn",
            2026,
            "Washington",
            "MN",
            0.75m,
            190m,
            4.65m,
            new DateOnly(2026, 3, 15),
            new DateOnly(2026, 12, 31));

        policy.Fields.Add(new InsuredField(
            NorthFieldId,
            PolicyId,
            "F-102",
            "FM-3842",
            "TR-17",
            118.40m,
            new DateOnly(2026, 5, 6),
            """{"type":"Polygon","coordinates":[[[12,18],[77,12],[88,48],[61,82],[20,73],[12,18]]]}"""));

        policy.Fields.Add(new InsuredField(
            SouthFieldId,
            PolicyId,
            "F-103",
            "FM-3842",
            "TR-18",
            86.25m,
            new DateOnly(2026, 5, 8),
            """{"type":"Polygon","coordinates":[[[18,21],[63,10],[91,35],[77,81],[35,87],[12,59],[18,21]]]}"""));

        db.Producers.Add(producer);
        db.Policies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);
    }
}
