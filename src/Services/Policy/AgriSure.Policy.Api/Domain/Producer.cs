namespace AgriSure.Policy.Api.Domain;

public sealed class Producer
{
    private Producer() { }

    public Producer(Guid id, string tenantId, string externalActorId, string name, string email)
    {
        Id = id;
        TenantId = tenantId;
        ExternalActorId = externalActorId;
        Name = name;
        Email = email;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public string ExternalActorId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
}
