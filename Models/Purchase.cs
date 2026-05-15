using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SFT.Models
{
    public enum SupplierTier
    {
        [Display(Name = "Tier I: Primary Integration")]
        Tier1_Assembly, // Factory level

        [Display(Name = "Tier II: Strategic Component")]
        Tier2_Mill,     // Fabric/Processing level

        [Display(Name = "Tier III: Raw Origin Extraction")]
        Tier3_Raw       // Raw Material level
    }

    public class Purchase
    {
        public int Id { get; set; }

        [Required]
        public string? Brand { get; set; }

        [Required]
        public string? ItemName { get; set; }

        public string? Material { get; set; }

        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [ValidateNever]
        public string AuditStamp { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // --- EXQUISITE INTEGRITY SUITE (MAY 2026) ---

        public SupplierTier Tier { get; set; } = SupplierTier.Tier1_Assembly;

        [ValidateNever]
        public int IntegrityScore { get; set; } = 100;

        // The Database Column
        public string? OriginLocation { get; set; }

        // --- CRITICAL INTEGRATION FOR PROFILE PAGE ---
        // This "Redirects" any call for 'Origin' to 'OriginLocation'
        // so your Profile.cshtml stops throwing errors.
        [NotMapped]
        public string? Origin
        {
            get => OriginLocation;
            set => OriginLocation = value;
        }

        [ValidateNever]
        public bool IsHighRisk { get; set; } = false;

        public DateTime AuditDate { get; private set; } = DateTime.UtcNow;

        // --- THE "AUTO-PILOT" AUDITOR LOGIC: WEIGHTED COMPLIANCE ---
        public void PerformAudit()
        {
            // Corporate Accountability Blacklist
            var blacklist = new List<string> { "Shein", "Temu", "Zara", "Fashion Nova", "H&M", "Boohoo" };

            // High-Risk Geographies (UFLPA & Ethical Compliance)
            var redZones = new List<string> { "Xinjiang", "Turkestan", "XUAR", "Uzbekistan", "Turkmenistan" };

            IsHighRisk = false;
            double tempScore = 100;

            // --- STAGE A: GEOGRAPHY & TIER ANALYSIS ---
            bool hasOriginViolation = !string.IsNullOrEmpty(OriginLocation) &&
                                     redZones.Any(z => OriginLocation.Contains(z, StringComparison.OrdinalIgnoreCase));

            if (hasOriginViolation)
            {
                IsHighRisk = true;
                // Axiom Logic: Raw materials from red zones result in total audit failure (0%)
                if (Tier == SupplierTier.Tier3_Raw)
                {
                    tempScore = 0;
                }
                else
                {
                    tempScore -= 75; // Severe penalty for integration/processing in red zones
                }
            }

            // --- STAGE B: BRAND COMPLIANCE ---
            bool hasBrandViolation = !string.IsNullOrEmpty(Brand) &&
                                    blacklist.Any(b => Brand.Contains(b, StringComparison.OrdinalIgnoreCase));

            if (hasBrandViolation)
            {
                IsHighRisk = true;
                tempScore -= 40;
            }

            // --- STAGE C: MATERIAL INTEGRITY BONUS ---
            if (!string.IsNullOrEmpty(Material))
            {
                if (Material.Contains("Organic", StringComparison.OrdinalIgnoreCase) ||
                    Material.Contains("Ring-spun", StringComparison.OrdinalIgnoreCase) ||
                    Material.Contains("Recycled", StringComparison.OrdinalIgnoreCase))
                {
                    tempScore += 10;
                }
            }

            // --- STAGE D: FINAL COMMITMENT ---
            IntegrityScore = (int)Math.Clamp(tempScore, 0, 100);
            AuditDate = DateTime.UtcNow;
        }
    }
}