using Domain;
using Infrastructure.Persistence.Public;

namespace Infrastructure.Persistence;

public static class PublicTenantMapper
{
    public static Tenant ToDomain(PublicTenant row)
    {
        Plan plan = Enum.TryParse(row.Plan, ignoreCase: true, out Plan parsedPlan)
            ? parsedPlan
            : Plan.Starter;

        return Tenant.Create(
            id: row.Id,
            slug: row.Slug,
            name: row.Name,
            domain: row.Domain,
            plan: plan,
            settings: TenantSettings.Default(),
            isActive: row.IsActive);
    }
}
