using BairlyMN.Data.Entities;
using BairlyMN.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BairlyMN.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ── GET /Account/Login ────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // ── POST /Account/Login ───────────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Таны данс түр хаагдсан. Арай дараа оролдоно уу.");
            return View(model);
        }

        ModelState.AddModelError("", "И-мэйл эсвэл нууц үг буруу байна.");
        return View(model);
    }

    // ── GET /Account/Register ─────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    // ── POST /Account/Register ────────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["Success"] = "Бүртгэл амжилттай үүслээ. Тавтай морил!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", TranslateIdentityError(error));

        return View(model);
    }

    // ── POST /Account/Logout ──────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ── GET /Account/AccessDenied ─────────────────────────────────
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ── Translate common Identity errors to Mongolian ─────────────
    private static string TranslateIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateUserName" or "DuplicateEmail"
                => "Энэ и-мэйл хаяг аль хэдийн бүртгэлтэй байна.",
            "PasswordTooShort"
                => "Нууц үг хамгийн багадаа 8 тэмдэгт байх ёстой.",
            "PasswordRequiresDigit"
                => "Нууц үг тоо агуулсан байх ёстой.",
            "PasswordRequiresLower"
                => "Нууц үг жижиг үсэг агуулсан байх ёстой.",
            "PasswordRequiresUpper"
                => "Нууц үг том үсэг агуулсан байх ёстой.",
            "PasswordRequiresNonAlphanumeric"
                => "Нууц үг тусгай тэмдэгт агуулсан байх ёстой.",
            _ => error.Description
        };
    }
}