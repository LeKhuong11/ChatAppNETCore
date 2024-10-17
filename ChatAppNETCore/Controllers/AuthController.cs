using ChatAppNETCore.Hubs;
using ChatAppNETCore.Models;
using ChatAppNETCore.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StudentManage.Models.Auth;
using System.Security.Claims;

namespace ChatAppNETCore.Controllers
{
    public class AuthController : Controller
    {
        private readonly ChatAppContext _context;
        private readonly UserService _userService;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public AuthController(ChatAppContext context, UserService userService, IHubContext<ChatHub> chatHubContext)
        {
            _context = context;
            _userService = userService;
            _chatHubContext = chatHubContext;
        }


        //Nếu không chỉ định tên view, sẽ tự động tìm view có cùng tên với phương thức Register()
        [HttpGet]
        public IActionResult Register() => View("Register");

        [HttpPost]
        public async Task<IActionResult> Register(C_User user)
        {
            if (ModelState.IsValid)
            {
                await _userService.RegisterUser(user);
                return RedirectToAction("Login");
            }
            return View(user);
        }


        [HttpGet]
        public IActionResult Login() => View("Login");

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                string passwordHashed = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Tìm người dùng dựa trên email và mật khẩu (hashed)
                var user = _context.C_Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    // Tạo danh sách các Claim (dữ liệu liên quan đến người dùng)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    // Tạo danh tính người dùng
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Tạo cookie đăng nhập
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true, // Ghi nhớ phiên đăng nhập
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) // Thời gian hết hạn
                    };

                    // Đăng nhập vào hệ thống
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                    );

                    await _chatHubContext.Clients.All.SendAsync("AddUserToOnlineList", user.Id);

                    TempData["SuccessMessage"] = "Login successful!";

                    // Chuyển hướng về trang quản lý sinh viên
                    return RedirectToAction("Index", "Chat");
                }

                TempData["ErrorMessage"] = "Invalid login credentials. Please try again!";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _chatHubContext.Clients.All.SendAsync("RemoveUserFromOnlineList", userId);

            TempData["SuccessMessage"] = "You have been logged out successfully.";

            return RedirectToAction("Login", "Auth");
        }
    }
}
