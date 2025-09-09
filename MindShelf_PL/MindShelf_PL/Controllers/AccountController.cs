using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.AccountDto;
using MindShelf_DAL.Models;
using Microsoft.AspNetCore.Authorization;

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

			// Pass display name and privacy settings from claims to the view
			var claims = await _userManager.GetClaimsAsync(user);
			var displayNameClaim = claims.FirstOrDefault(c => c.Type == "display_name")?.Value;
			ViewBag.DisplayName = displayNameClaim ?? user.UserName;
			
			// Load privacy settings
			var shareAvatarClaim = claims.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
			var shareAddressClaim = claims.FirstOrDefault(c => c.Type == "share_address")?.Value;
			var sharePhoneClaim = claims.FirstOrDefault(c => c.Type == "share_phone")?.Value;
			
			ViewBag.ShareAvatar = shareAvatarClaim == null ? true : shareAvatarClaim == "true";
			ViewBag.ShareAddress = shareAddressClaim == "true";
			ViewBag.SharePhone = sharePhoneClaim == "true";
			
			// Debug: Log what we're loading
			System.Diagnostics.Debug.WriteLine($"Profile loading display_name claim: {displayNameClaim}, fallback: {user.UserName}");
			System.Diagnostics.Debug.WriteLine($"Profile privacy - ShareAvatar: {ViewBag.ShareAvatar}, ShareAddress: {ViewBag.ShareAddress}, SharePhone: {ViewBag.SharePhone}");

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

			// Load privacy flags from claims
			var claims = await _userManager.GetClaimsAsync(user);
			var shareAvatarClaim = claims.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
			var shareAddressClaim = claims.FirstOrDefault(c => c.Type == "share_address")?.Value;
			var sharePhoneClaim = claims.FirstOrDefault(c => c.Type == "share_phone")?.Value;
			
			// Set default values: ShareAvatar=true (show by default), ShareAddress=false, SharePhone=false
			ViewBag.ShareAvatar = shareAvatarClaim == null ? true : shareAvatarClaim == "true";
			ViewBag.ShareAddress = shareAddressClaim == "true";
			ViewBag.SharePhone = sharePhoneClaim == "true";
			
			// Debug: Log what privacy values we're loading
			System.Diagnostics.Debug.WriteLine($"Privacy flags loaded - ShareAvatar: {shareAvatarClaim} -> {ViewBag.ShareAvatar}, ShareAddress: {shareAddressClaim} -> {ViewBag.ShareAddress}, SharePhone: {sharePhoneClaim} -> {ViewBag.SharePhone}");
			var displayNameClaim = claims.FirstOrDefault(c => c.Type == "display_name")?.Value;
			ViewBag.DisplayName = displayNameClaim ?? user.UserName;
			// Debug: Log what we're loading
			System.Diagnostics.Debug.WriteLine($"Loading display_name claim: {displayNameClaim}, fallback: {user.UserName}");

			return View(user);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Settings([Bind("UserName,PhoneNumber,Address,ProfileImageUrl")] User model, IFormFile profileImage, string? currentPassword, string? ShareAvatar = null, string? ShareAddress = null, string? SharePhone = null, string? DisplayName = null)
		{
			// Debug: Log what we're receiving from the form
			System.Diagnostics.Debug.WriteLine($"Settings POST - Received values: ShareAvatar={ShareAvatar}, ShareAddress={ShareAddress}, SharePhone={SharePhone}, DisplayName={DisplayName}");
			
			if (!User.Identity.IsAuthenticated)
				return RedirectToAction(nameof(Login));

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return RedirectToAction(nameof(Login));

			// Make image optional and ignore unrelated validations
			ModelState.Remove("ProfileImageUrl");
			ModelState.Remove("profileImage");
			ModelState.Remove("ShoppingCart");

			// Require current password if phone is being changed (only for local-password users)
			if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.Ordinal))
			{
				var hasLocalPassword = await _userManager.HasPasswordAsync(user);
				if (hasLocalPassword)
				{
					if (string.IsNullOrWhiteSpace(currentPassword) || !(await _userManager.CheckPasswordAsync(user, currentPassword)))
					{
						ModelState.AddModelError("", "يرجى إدخال كلمة المرور الحالية لتعديل رقم الهاتف");
					}
				}
			}

			if (ModelState.IsValid)
			{
				// Use a claim for display name so we can allow spaces and any language
				async Task UpsertClaim(string type, string value)
				{
					var existing = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == type);
					var newClaim = new System.Security.Claims.Claim(type, value ?? string.Empty);
					if (existing == null)
						await _userManager.AddClaimAsync(user, newClaim);
					else if (existing.Value != newClaim.Value)
						await _userManager.ReplaceClaimAsync(user, existing, newClaim);
				}

				if (!string.IsNullOrWhiteSpace(DisplayName))
				{
					await UpsertClaim("display_name", DisplayName!.Trim());
					// Debug: Log what we're saving
					System.Diagnostics.Debug.WriteLine($"Saving display_name claim: {DisplayName.Trim()}");
					
					// Verify it was saved
					var savedClaims = await _userManager.GetClaimsAsync(user);
					var savedDisplayName = savedClaims.FirstOrDefault(c => c.Type == "display_name")?.Value;
					System.Diagnostics.Debug.WriteLine($"Verified saved display_name claim: {savedDisplayName}");
				}

				user.Address = model.Address;
				user.PhoneNumber = model.PhoneNumber;

				// Handle optional profile image
				if (profileImage != null && profileImage.Length > 0)
				{
					var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
					var ext = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
					if (!allowed.Contains(ext) || !profileImage.ContentType.StartsWith("image/"))
					{
						ModelState.AddModelError("", "صيغة الصورة غير مدعومة. المسموح: JPG, PNG, WEBP");
					}
					const long maxBytes = 5 * 1024 * 1024;
					if (profileImage.Length > maxBytes)
					{
						ModelState.AddModelError("", "حجم الصورة يجب ألا يتجاوز 5 ميجابايت");
					}

					if (ModelState.ErrorCount == 0)
					{
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
				}

				await _userManager.UpdateAsync(user);

				// Update privacy flags as claims (bool values)
				async Task UpsertClaimBool(string type, bool value)
				{
					var existing = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == type);
					var newClaim = new System.Security.Claims.Claim(type, value ? "true" : "false");
					if (existing == null)
						await _userManager.AddClaimAsync(user, newClaim);
					else if (existing.Value != newClaim.Value)
						await _userManager.ReplaceClaimAsync(user, existing, newClaim);
				}

				// Convert string values to bool (null or empty = false, "true" = true)
				bool shareAvatarValue = ShareAvatar == "true";
				bool shareAddressValue = ShareAddress == "true";
				bool sharePhoneValue = SharePhone == "true";
				
				// Debug: Log what privacy values we're receiving
				System.Diagnostics.Debug.WriteLine($"Privacy flags received - ShareAvatar: {ShareAvatar} -> {shareAvatarValue}, ShareAddress: {ShareAddress} -> {shareAddressValue}, SharePhone: {SharePhone} -> {sharePhoneValue}");
				
				await UpsertClaimBool("share_avatar", shareAvatarValue);
				await UpsertClaimBool("share_address", shareAddressValue);
				await UpsertClaimBool("share_phone", sharePhoneValue);
				
				// Debug: Verify what was saved
				var verifyClaims = await _userManager.GetClaimsAsync(user);
				var verifyShareAvatar = verifyClaims.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
				var verifyShareAddress = verifyClaims.FirstOrDefault(c => c.Type == "share_address")?.Value;
				var verifySharePhone = verifyClaims.FirstOrDefault(c => c.Type == "share_phone")?.Value;
				System.Diagnostics.Debug.WriteLine($"Privacy flags saved - ShareAvatar: {verifyShareAvatar}, ShareAddress: {verifyShareAddress}, SharePhone: {verifySharePhone}");

				// Refresh authentication cookie to reflect new user data/claims
				await _signInManager.RefreshSignInAsync(user);
				var externalLogins = await _userManager.GetLoginsAsync(user);
				if (externalLogins != null && externalLogins.Count > 0)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);
				}

				TempData["Success"] = "تم حفظ البيانات بنجاح";

				var claims = await _userManager.GetClaimsAsync(user);
				var shareAvatarClaim = claims.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
				var shareAddressClaim = claims.FirstOrDefault(c => c.Type == "share_address")?.Value;
				var sharePhoneClaim = claims.FirstOrDefault(c => c.Type == "share_phone")?.Value;
				
				ViewBag.ShareAvatar = shareAvatarClaim == null ? true : shareAvatarClaim == "true";
				ViewBag.ShareAddress = shareAddressClaim == "true";
				ViewBag.SharePhone = sharePhoneClaim == "true";
				ViewBag.DisplayName = claims.FirstOrDefault(c => c.Type == "display_name")?.Value ?? user.UserName;
				return View(user);
			}

			// Keep existing flags on error
			{
				var claims2 = await _userManager.GetClaimsAsync(user);
				var shareAvatarClaim2 = claims2.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
				var shareAddressClaim2 = claims2.FirstOrDefault(c => c.Type == "share_address")?.Value;
				var sharePhoneClaim2 = claims2.FirstOrDefault(c => c.Type == "share_phone")?.Value;
				
				ViewBag.ShareAvatar = shareAvatarClaim2 == null ? true : shareAvatarClaim2 == "true";
				ViewBag.ShareAddress = shareAddressClaim2 == "true";
				ViewBag.SharePhone = sharePhoneClaim2 == "true";
				ViewBag.DisplayName = claims2.FirstOrDefault(c => c.Type == "display_name")?.Value ?? user.UserName;
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
				var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				if (existingUser != null)
				{
					var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
							   ?? info.Principal.FindFirst("name")?.Value
							   ?? existingUser.UserName;
					var picture = info.Principal.FindFirst("urn:google:picture")?.Value
								  ?? info.Principal.FindFirst("picture")?.Value
								  ?? existingUser.ProfileImageUrl;

					bool updated = false;
					// We keep UserName as is (sign-in identifier); no separate DisplayName field
					if (!string.IsNullOrWhiteSpace(picture) && existingUser.ProfileImageUrl != picture)
					{
						existingUser.ProfileImageUrl = picture;
						updated = true;
					}
					if (updated) await _userManager.UpdateAsync(existingUser);
				}
				return LocalRedirect(returnUrl);
			}
			else
			{
				var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
								 ?? info.Principal.FindFirst("email")?.Value;
				var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
						   ?? info.Principal.FindFirst("name")?.Value
						   ?? email?.Split('@').FirstOrDefault();
				var picture = info.Principal.FindFirst("urn:google:picture")?.Value
							  ?? info.Principal.FindFirst("picture")?.Value;

				if (!string.IsNullOrWhiteSpace(email))
				{
					var user = await _userManager.FindByEmailAsync(email);
					if (user == null)
					{
						user = new User
						{
							UserName = email, // keep username unique (email-based sign-in)
							Email = email,
							EmailConfirmed = true,
							ProfileImageUrl = picture
						};
						var createResult = await _userManager.CreateAsync(user);
						if (!createResult.Succeeded) return RedirectToAction(nameof(Login));
					}
					else
					{
						bool updated = false;
						if (!string.IsNullOrWhiteSpace(picture) && user.ProfileImageUrl != picture)
						{
							user.ProfileImageUrl = picture;
							updated = true;
						}
						if (updated) await _userManager.UpdateAsync(user);
					}

					await _userManager.AddLoginAsync(user, info);
					await _signInManager.SignInAsync(user, isPersistent: false);
					return LocalRedirect(returnUrl);
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

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Public(string? userId = null, string? userName = null)
		{
			User? user = null;
			if (!string.IsNullOrWhiteSpace(userId))
			{
				user = await _userManager.FindByIdAsync(userId);
			}
			else if (!string.IsNullOrWhiteSpace(userName))
			{
				user = await _userManager.FindByNameAsync(userName);
			}

			if (user == null)
			{
				return NotFound();
			}

			// Load privacy settings for the target user
			var claims = await _userManager.GetClaimsAsync(user);
			var displayNameClaim = claims.FirstOrDefault(c => c.Type == "display_name")?.Value;
			var shareAvatarClaim = claims.FirstOrDefault(c => c.Type == "share_avatar")?.Value;
			var shareAddressClaim = claims.FirstOrDefault(c => c.Type == "share_address")?.Value;
			var sharePhoneClaim = claims.FirstOrDefault(c => c.Type == "share_phone")?.Value;
			
			ViewBag.DisplayName = displayNameClaim ?? user.UserName;
			ViewBag.ShareAvatar = shareAvatarClaim == null ? true : shareAvatarClaim == "true";
			ViewBag.ShareAddress = shareAddressClaim == "true";
			ViewBag.SharePhone = sharePhoneClaim == "true";

			return View(user);
		}
	}
}
