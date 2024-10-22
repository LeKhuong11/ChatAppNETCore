using ChatAppNETCore.Models;
using Microsoft.EntityFrameworkCore;


namespace ChatAppNETCore.Services
{
    public class UserService(ChatAppContext context)
    {
        private readonly ChatAppContext _context = context;

        public async Task<C_User> RegisterUser(C_User user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.C_Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<C_User> ValidateUser(string email, string password)
        {
            var user = await _context.C_Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public async Task<List<C_User>> getAllUserWithOutCurrentUser(string userId)
        {
            var users = await _context.C_Users
                 .Where(user => user.Id != Guid.Parse(userId))
                 .ToListAsync();
            return users;
        }
    }
}
