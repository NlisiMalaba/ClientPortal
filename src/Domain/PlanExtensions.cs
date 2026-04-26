namespace Domain;

public static class PlanExtensions
{
    public static PlanFeatureFlags GetFeatureFlags(this Plan plan)
    {
        return plan switch
        {
            Plan.Free => new PlanFeatureFlags(
                CustomDomainEnabled: false,
                AdvancedReportingEnabled: false,
                ApiAccessEnabled: false,
                PrioritySupportEnabled: false,
                MaxUsers: 3),
            Plan.Starter => new PlanFeatureFlags(
                CustomDomainEnabled: false,
                AdvancedReportingEnabled: false,
                ApiAccessEnabled: true,
                PrioritySupportEnabled: false,
                MaxUsers: 10),
            Plan.Professional => new PlanFeatureFlags(
                CustomDomainEnabled: true,
                AdvancedReportingEnabled: true,
                ApiAccessEnabled: true,
                PrioritySupportEnabled: false,
                MaxUsers: 50),
            Plan.Enterprise => new PlanFeatureFlags(
                CustomDomainEnabled: true,
                AdvancedReportingEnabled: true,
                ApiAccessEnabled: true,
                PrioritySupportEnabled: true,
                MaxUsers: int.MaxValue),
            _ => throw new ArgumentOutOfRangeException(nameof(plan), plan, "Unsupported plan.")
        };
    }
}
