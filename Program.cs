using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SFT.Data;
using SFT.Models;
using SFT.Services;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=sustainable_fashion.db"));

// Register Identity
builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

// Register services
builder.Services.AddScoped<IAuditService, AxiomAuditService>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        dbContext.Database.EnsureCreated();

        var legacyPurchases = dbContext.Purchases.ToList();
        if (legacyPurchases != null && legacyPurchases.Any())
        {
            foreach (var purchase in legacyPurchases)
            {
                // Safety check: ensure the item isn't null before auditing
                if (purchase != null)
                {
                    auditService.RunComplianceCheck(purchase);

                    if (string.IsNullOrEmpty(purchase.AuditStamp))
                    {
                        purchase.AuditStamp = GenerateAuditStamp(purchase.ItemName ?? "LegacyEntry");
                    }
                }
            }
            dbContext.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // This prevents the whole app from crashing if the database has one bad row
        Console.WriteLine("Database init warning: " + ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

// Move the method here
static string GenerateAuditStamp(string entryData)
{
    const string uei = "ZZCXD7WSEQK6";
    string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");

    using var sha256 = SHA256.Create();
    var rawData = System.Text.Encoding.UTF8.GetBytes(uei + entryData + timestamp);
    var hash = sha256.ComputeHash(rawData);
    string signature = Convert.ToHexString(hash).Substring(0, 8);

    return $"H1-{uei}-{DateTime.UtcNow:yyyyMMdd}-{signature}";
}