namespace BuildingBlocks.Contracts.Services.Tenants;

public interface ITenantService
{
    Ulid? TenantId { get; }
    void SetTenantId(Ulid tenantId);
}