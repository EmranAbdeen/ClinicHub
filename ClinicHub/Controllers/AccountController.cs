using ClinicHub.DTOs.Account;
using ClinicHub.Filters;
using ClinicHub.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if(!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var user = await _accountService.LoginAsync(loginDto);

            if (user == null)
            {
                TempData["Error"] = "Invalid email or password.";
                return View(loginDto);
            }
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            var result = await _accountService.RegisterAsync(registerDto);

            if (!result)
            {
                TempData["Error"] = "Email already exists or registration failed.";
                return View(registerDto);
            }

            TempData["Success"] = "Your account was created successfully. Please log in.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AuthFilter]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [AuthFilter]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var result = await _accountService.ChangePasswordAsync(userId.Value, dto);
            if (!result)
            {
                ModelState.AddModelError("CurrentPassword", "The current password is incorrect.");
                TempData["Error"] = "The current password is incorrect.";
                return View(dto);
            }

            TempData["Success"] = "Your password has been changed successfully.";

            return RedirectToAction("ChangePassword");
        }

        [HttpGet]
        [AuthFilter(Roles = new[] { "Super_admin" })]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        [AuthFilter(Roles = new[] { "Super_admin" })]
        public async Task<IActionResult> CreateAdmin(CreateAdminDto dto) 
        { 
            if (!ModelState.IsValid) 
            { 
                return View(dto);
            } 
            
            var result = await _accountService.CreateAdminAsync(dto);
            if (!result) 
            { 
                TempData["Error"] = "Email already exists or admin creation failed.";
                return View(dto);
            } 
            
            TempData["Success"] = "Admin added successfully.";
            return RedirectToAction("Index", "Dashboard"); 
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}