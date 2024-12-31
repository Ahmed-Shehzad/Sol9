namespace BuildingBlocks.Contracts.Services.Tenants;

public class TenantService : ITenantService
{
    public Ulid? TenantId { get; private set; }

    public void SetTenantId(Ulid tenantId)
    {
        TenantId = tenantId;
    }
}