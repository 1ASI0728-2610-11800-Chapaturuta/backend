using Frock_backend.Subscriptions.Domain.Model.ValueObjects;

namespace Frock_backend.Subscriptions.Domain.Model.Aggregates;

public class Plan
{
    public int Id { get; }
    public string Name { get; set; }
    public PlanType PlanType { get; set; }
    public TargetRole TargetRole { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public string Benefits { get; set; }
    public int? DiscoveryQuota { get; set; }
    public bool IsActive { get; set; }

    protected Plan()
    {
        Name = string.Empty;
        Currency = "PEN";
        Benefits = string.Empty;
        IsActive = true;
    }

    public Plan(
        string name,
        PlanType planType,
        TargetRole targetRole,
        decimal price,
        string currency,
        BillingCycle billingCycle,
        string benefits,
        int? discoveryQuota)
    {
        Name = name;
        PlanType = planType;
        TargetRole = targetRole;
        Price = price;
        Currency = string.IsNullOrWhiteSpace(currency) ? "PEN" : currency;
        BillingCycle = billingCycle;
        Benefits = benefits ?? string.Empty;
        DiscoveryQuota = discoveryQuota;
        IsActive = true;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(newPrice));
        Price = newPrice;
    }
}
