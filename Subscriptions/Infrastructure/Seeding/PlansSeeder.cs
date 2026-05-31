using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Subscriptions.Infrastructure.Seeding;

public static class PlansSeeder
{
    /**
     * <summary>
     *     Seeds the default Free and Premium plans if they have not been registered yet.
     *     Must be invoked by Program.cs after the database has been migrated/created.
     * </summary>
     * <param name="context">The application database context.</param>
     */
    public static async Task SeedAsync(AppDbContext context)
    {
        var plansSet = context.Set<Plan>();

        var hasFree = await plansSet.AnyAsync(p => p.PlanType == PlanType.Free && p.Name == "Free");
        if (!hasFree)
        {
            var freePlan = new Plan(
                name: "Free",
                planType: PlanType.Free,
                targetRole: TargetRole.Both,
                price: 0m,
                currency: "PEN",
                billingCycle: BillingCycle.Monthly,
                benefits: "Acceso limitado a Discovery con IA (10 consultas/mes)",
                discoveryQuota: 10
            );
            await plansSet.AddAsync(freePlan);
        }

        var hasPremium = await plansSet.AnyAsync(p => p.PlanType == PlanType.Premium && p.Name == "Premium");
        if (!hasPremium)
        {
            var premiumPlan = new Plan(
                name: "Premium",
                planType: PlanType.Premium,
                targetRole: TargetRole.Both,
                price: 29.90m,
                currency: "PEN",
                billingCycle: BillingCycle.Monthly,
                benefits: "Uso ilimitado de Discovery con IA",
                discoveryQuota: null
            );
            await plansSet.AddAsync(premiumPlan);
        }

        await context.SaveChangesAsync();
    }
}
