using SFT.Models;

namespace SFT.Services
{
    public class AxiomAuditService : IAuditService
    {
        public void RunComplianceCheck(Purchase purchase)
        {
            // 1. THE ENGINE TRIGGER
            // This executes the weighted logic: Tier 3 Red Zone checks, 
            // Brand blacklists, and Material bonuses.
            purchase.PerformAudit();

            // 2. THE COMPLIANCE OVERWATCH
            // Since you are building "Humanity One" as an accountability body,
            // this is where the service-level 'enforcement' happens.
            if (purchase.IsHighRisk)
            {
                // Logic for critical flags:
                // We could eventually log these to a separate 'SecurityAudit' table
                // or prepare a specific alert for the Federal Identity registry.
            }

            if (purchase.IntegrityScore == 0)
            {
                // Hard-stop logic for non-negotiable labor violations.
            }
        }
    }
}