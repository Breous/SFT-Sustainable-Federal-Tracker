using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SFT.Models;
using SFT.Data;
using SFT.Services;

namespace SFT.Pages
{
    [Authorize]
    public class AuditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;

        public AuditModel(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [BindProperty]
        public Purchase Purchase { get; set; } = new();

        public string StatusClass { get; set; } = "bg-secondary"; // Default to neutral
        public double LatestScore { get; set; }

        public IList<Purchase> Purchases { get; set; } = new List<Purchase>();

        /// <summary>
        /// Retrieves the personal audit history for the authenticated user.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Pull the data
            Purchases = await _context.Purchases
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            // 2. Default to Dark/Neutral so you KNOW if the logic worked
            StatusClass = "bg-dark text-white";

            // 3. The Science
            if (Purchases != null && Purchases.Any())
            {
                var latest = Purchases.First();

                // Check for China/Xinjiang specifically to force the top bar to Red
                bool originFail = !string.IsNullOrEmpty(latest.OriginLocation) &&
                                  (latest.OriginLocation.Contains("China", StringComparison.OrdinalIgnoreCase) ||
                                   latest.OriginLocation.Contains("Xinjiang", StringComparison.OrdinalIgnoreCase));

                LatestScore = originFail ? 0 : latest.IntegrityScore;

                // THE FORCE-COLOR LOGIC
                if (LatestScore < 50 || latest.IsHighRisk || originFail)
                {
                    StatusClass = "bg-danger text-white"; // FORCE RED
                }
                else if (LatestScore < 80)
                {
                    StatusClass = "bg-warning text-dark"; // FORCE YELLOW
                }
                else
                {
                    StatusClass = "bg-success text-white"; // FORCE GREEN
                }
            }

            return Page();
        }

        /// <summary>
        /// Processes a new audit entry, executes the Axiom logic, and records the stamp.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Purchase.UserId = userId ?? string.Empty;

            // Localization: Ensuring the 'Date' reflects Indiana time (EST)
            DateTime utcNow = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            Purchase.Date = TimeZoneInfo.ConvertTimeFromUtc(utcNow, easternZone);

            // --- VALIDATION MAINTENANCE ---
            ModelState.Remove("Purchase.AuditStamp");
            ModelState.Remove("Purchase.IntegrityScore");
            ModelState.Remove("Purchase.IsHighRisk");
            ModelState.Remove("Purchase.UserId");
            ModelState.Remove("Purchase.Date");
            ModelState.Remove("Purchase.User");
            ModelState.Remove("Purchase.AuditDate");

            if (!ModelState.IsValid)
            {
                Purchases = await _context.Purchases
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.Date)
                    .ToListAsync();
                return Page();
            }

            // --- AXIOM ENGINE TRIGGER ---
            Purchase.PerformAudit();

            // --- AXIOM FIREWALL OVERRIDE: 2026 UFLPA PROTOCOL ---
            var axiomBlacklist = new List<string> {
    "China", "Xinjiang", "XUAR", "XPCC", // Prohibited Regions/Entities
    "Shein", "Temu", "Zara", "Fashion Nova", "H&M", "Boohoo", // High-Risk Retail
    "Esquel", "Luthai", "Huafu", "Zhongtai" // Federal Enforcement Targets
};

            bool isProhibited = false;

            // 1. Check Origin
            if (!string.IsNullOrEmpty(Purchase.Origin) && axiomBlacklist.Any(o => Purchase.Origin.Contains(o, StringComparison.OrdinalIgnoreCase)))
                isProhibited = true;

            // 2. Check Brand
            if (!string.IsNullOrEmpty(Purchase.Brand) && axiomBlacklist.Any(b => Purchase.Brand.Contains(b, StringComparison.OrdinalIgnoreCase)))
                isProhibited = true;

            if (isProhibited)
            {
                Purchase.IntegrityScore = 0;
                Purchase.IsHighRisk = true;
            }

            // Federal Identity: Generate the cryptographic registry stamp
            Purchase.AuditStamp = SFT.Core.FederalIdentity.GenerateStamp(Purchase.ItemName ?? "SECURE_ASSET");

            _context.Purchases.Add(Purchase);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Audit");
        }
    }
}