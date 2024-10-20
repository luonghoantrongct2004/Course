using EduCourse.Entities;
using EduCourse.Helpers;
using EduCourse.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduCourse.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    var existingClaims = await _userManager.GetClaimsAsync(user);
                    var claimsToAdd = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

                    foreach (var claim in claimsToAdd)
                    {
                        if (!existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                        {
                            await _userManager.AddClaimAsync(user, claim);
                        }
                    }

                    // Success response to be handled by SweetAlert2
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                }
                else
                {
                    // Error response for invalid login
                    return Json(new { success = false, message = "Thông tin nhập không chính xác!" });
                }
            }

            // If model validation fails
            return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model, string password, bool rememberMe = true)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    var error = $"Email {model.Email} đã được sử dụng!";
                    return Json(new { success = false, message = error });
                }

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    DateJoined = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Student"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Student"));
                    }

                    await _userManager.AddToRoleAsync(user, "Student");
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

                    var claimResult = await _userManager.AddClaimsAsync(user, claims);

                    if (claimResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: rememberMe);

                        // Success response for SweetAlert2
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                    }
                    else
                    {
                        // Error in adding claims
                        var errors = claimResult.Errors.Select(e => ErrorTranslator.Translate(e.Description)).First();
                        return Json(new { success = false, errors });
                    }
                }
                else
                {
                    // Error in creating user
                    var errors = result.Errors.Select(e => ErrorTranslator.Translate(e.Description)).First();
                    return Json(new { success = false, errors });
                }
            }

            // Model validation error
            return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });
        }


        public IActionResult RegisterTutor()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterTutor(User model, string password, bool rememberMe = true)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    var error = $"Email {model.Email} đã được sử dụng!";
                    return Json(new { success = false, message = error });
                }

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    DateJoined = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Instructure"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Instructure"));
                    }

                    await _userManager.AddToRoleAsync(user, "Instructure");
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

                    var claimResult = await _userManager.AddClaimsAsync(user, claims);

                    if (claimResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: rememberMe);

                        // Return success response
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                    }
                    else
                    {
                        var errors = claimResult.Errors.Select(e => ErrorTranslator.Translate(e.Description)).First();
                        return Json(new { success = false, errors });
                    }
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Json(new { success = false, errors });
                }
            }

            // Model validation errors
            return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });
        }

        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
