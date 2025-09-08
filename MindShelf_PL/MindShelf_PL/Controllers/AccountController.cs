using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.AccountDto;
using MindShelf_DAL.Models;

namespace MindShelf_PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Gender  =model.Gender,
                    Address = model.Address,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync("User"))
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user,
                        model.Password,
                        isPersistent: false,
                        lockoutOnFailure: false
                    );

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Invalid login attempt");
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(Login));

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(Login));

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings([Bind("UserName,PhoneNumber,Address,ProfileImageUrl")] User model, IFormFile profileImage)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(Login));

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            // Ignore validation for properties not posted in the form
            ModelState.Remove("ProfileImageUrl");
            ModelState.Remove("ShoppingCart");

            if (ModelState.IsValid)
            {
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.ProfileImageUrl = model.ProfileImageUrl;

                if (profileImage != null && profileImage.Length > 0)
                {
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var ext = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
                    if (!allowed.Contains(ext) || !profileImage.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("", "صيغة الصورة غير مدعومة. المسموح: JPG, PNG, WEBP");
                        TempData["Error"] = "فشل حفظ البيانات: صيغة الصورة غير مدعومة";
                        return View(user);
                    }
                    const long maxBytes = 5 * 1024 * 1024;
                    if (profileImage.Length > maxBytes)
                    {
                        ModelState.AddModelError("", "حجم الصورة يجب ألا يتجاوز 5 ميجابايت");
                        TempData["Error"] = "فشل حفظ البيانات: حجم الصورة كبير";
                        return View(user);
                    }

                    var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "profiles");
                    Directory.CreateDirectory(uploadsRoot);
                    var fileName = $"{Guid.NewGuid()}" + Path.GetExtension(profileImage.FileName);
                    var filePath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }
                    user.ProfileImageUrl = $"/Images/profiles/{fileName}";
                }

                await _userManager.UpdateAsync(user);
                TempData["Success"] = "تم تحديث البيانات بنجاح";
                return RedirectToAction(nameof(Profile));
            }

            return View(user);
        }


        [HttpPost]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }


            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {

                var email = info.Principal.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                if (email != null)
                {
                    var user = new User
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddLoginAsync(user, info);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                return RedirectToAction(nameof(Login));
            }
        }


        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordEmailDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "This user does not exist");
                return View(model);
            }

            var resetModel = new ForgetPasswordDto { Email = model.Email };
            return View("ResetPassword", resetModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveForgetPassword(ForgetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return View("ResetPassword", model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "This user does not exist");
                return View("ResetPassword", model);
            }

            string hashedPassword = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);
            user.PasswordHash = hashedPassword;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Claims()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Json(claims);
        }
    }
}
