using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SFT.Data;
using SFT.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace SFT.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly AppDbContext _context;

        public ProfileModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Purchase> Purchases { get; set; } = new();
        public int TotalPurchases { get; set; }
        public decimal TotalSpent { get; set; }

        public string SustainabilityRating { get; set; } = "Initializing...";
        public string RatingClass { get; set; } = "text-secondary";

        // These properties specifically power the black dashboard boxes
        public double AverageIntegrityScore { get; set; }
        public int HighRiskCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            Purchases = await _context.Purchases
                .Where(p => p.UserId == userId)
                .ToListAsync();

            TotalPurchases = Purchases.Count;
            TotalSpent = Purchases.Sum(p => p.Price);

            // Logic for the Humanity One Dashboard metrics
            if (Purchases.Any())
            {
                AverageIntegrityScore = Purchases.Average(p => p.IntegrityScore);
                HighRiskCount = Purchases.Count(p => p.IntegrityScore < 70 || p.IsHighRisk);

                // Fail-safe: if there is even one high risk item, or the average is low
                bool isUrgentFail = HighRiskCount > 0 || AverageIntegrityScore < 50;

                if (isUrgentFail)
                {
                    SustainabilityRating = "CRITICAL RISK";
                    RatingClass = "bg-danger text-white"; // Red box, white letters
                }
                else if (AverageIntegrityScore < 80)
                {
                    SustainabilityRating = "MODERATE RISK";
                    RatingClass = "bg-warning text-dark"; // Yellow box, dark letters
                }
                else
                {
                    SustainabilityRating = "HIGH INTEGRITY";
                    RatingClass = "bg-success text-white"; // Green box, white letters
                }

            }
            return Page();
        }

        // --- EXPORT HANDLERS (May 2026 Ready) ---

        public async Task<IActionResult> OnGetExportCsvAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Account/Login");

            var purchases = await _context.Purchases.Where(p => p.UserId == userId).ToListAsync();

            var csvLines = new List<string> { "Brand,Item,Origin,Tier,IntegrityScore,Price,Status" };
            csvLines.AddRange(purchases.Select(p =>
                $"{p.Brand},{p.ItemName},{p.OriginLocation},{p.Tier},\"{p.IntegrityScore}%\",{p.Price},{(p.IsHighRisk ? "RISK" : "CLEAN")}"));

            var bytes = Encoding.UTF8.GetBytes(string.Join("\n", csvLines));
            return File(bytes, "text/csv", $"HumanityOne_Audit_{DateTime.Now:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> OnGetExportTxtAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Account/Login");

            var purchases = await _context.Purchases.Where(p => p.UserId == userId).ToListAsync();

            var lines = new List<string> {
                "HUMANITY ONE: SUPPLY CHAIN INTEGRITY REPORT",
                $"Generated: {DateTime.Now:MMMM dd, yyyy HH:mm}",
                "Compliance Standard: UFLPA-May-2026-Ready",
                "------------------------------------------"
            };

            lines.AddRange(purchases.Select(p =>
                $"[{(p.IsHighRisk ? "!!!" : "OK")}] Score: {p.IntegrityScore}% | {p.Brand} - {p.ItemName} | Tier: {p.Tier} | Origin: {p.OriginLocation}"));

            var bytes = Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
            return File(bytes, "text/plain", $"HumanityOne_Summary_{DateTime.Now:yyyyMMdd}.txt");
        }

        public async Task<IActionResult> OnGetExportJsonAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Account/Login");

            var purchases = await _context.Purchases.Where(p => p.UserId == userId).ToListAsync();

            var auditExport = new
            {
                ReportTitle = "Humanity One: Supply Chain Integrity Audit",
                ExportDate = DateTime.UtcNow,
                ComplianceStandard = "UFLPA-May-2026-Ready",
                TotalAudits = purchases.Count,
                GlobalIntegrityScore = purchases.Any() ? purchases.Average(p => p.IntegrityScore) : 100,
                Data = purchases.Select(p => new {
                    p.Brand,
                    p.ItemName,
                    p.OriginLocation,
                    Tier = p.Tier.ToString(),
                    p.IntegrityScore,
                    p.IsHighRisk,
                    p.AuditDate
                })
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(auditExport, options);
            var bytes = Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", $"HumanityOne_Audit_{DateTime.Now:yyyyMMdd}.json");
        }
    }
}