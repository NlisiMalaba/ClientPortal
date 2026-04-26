namespace Domain;

public sealed record PlanFeatureFlags(
    bool CustomDomainEnabled,
    bool AdvancedReportingEnabled,
    bool ApiAccessEnabled,
    bool PrioritySupportEnabled,
    int MaxUsers);
