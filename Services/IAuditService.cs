using SFT.Models;

namespace SFT.Services
{
    public interface IAuditService
    {
        void RunComplianceCheck(Purchase purchase);
    }
}