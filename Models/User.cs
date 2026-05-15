using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SFT.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [RegularExpression(@"^(Low|Medium|High)$", ErrorMessage = "Must be Low, Medium, or High")]
        public string SustainabilityLevel { get; set; }

    }
}