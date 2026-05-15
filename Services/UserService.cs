using Microsoft.EntityFrameworkCore;
using SFT.Data;
using SFT.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace SFT.Services
{
    public class UserService
    {
        private AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }


        public bool RegisterUser(string email, string password)
        {
            if (_context.Users.Any(u => u.Email == email))
                return false;

            var user = new User
            {
                Email = email,
                PasswordHash = HashPassword(password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return true;
        }

        public User? LoginUser(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || user.PasswordHash != HashPassword(password))
                return null;
            return user;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
