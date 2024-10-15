using EduCourse.Entities;
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

        // POST: /Auth/Login
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

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(User model, string password, bool rememberMe = true)
        {
            if (ModelState.IsValid)
            {
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
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in claimResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
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
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in claimResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
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
