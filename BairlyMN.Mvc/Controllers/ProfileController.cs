using BairlyMN.Data.Entities;
using BairlyMN.Mvc.ViewModels;
using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BairlyMN.Mvc.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IListingService _listings;
    private readonly IChatService _chat;
    private readonly IFileUploadService _fileUpload;

    public ProfileController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        IListingService listings,
        IChatService chat,
        IFileUploadService fileUpload)
    {
        _users = users;
        _signIn = signIn;
        _listings = listings;
        _chat = chat;
        _fileUpload = fileUpload;
    }

    // GET /Profile
    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        return View(new ProfileViewModel
        {
            User = user,
            Listings = await _listings.GetUserListingsAsync(user.Id),
            Favorites = await _listings.GetFavoriteListingsAsync(user.Id),
            Conversations = await _chat.GetUserConversationsAsync(user.Id)
        });
    }

    // GET /Profile/Edit
    public async Task<IActionResult> Edit()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        return View(new EditProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            PhoneNumber = user.PhoneNumber,
            CurrentAvatarPath = user.AvatarPath
        });
    }

    // POST /Profile/Edit
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        user.FirstName = vm.FirstName?.Trim();
        user.LastName = vm.LastName?.Trim();
        user.Bio = vm.Bio?.Trim();
        user.PhoneNumber = vm.PhoneNumber?.Trim();

        if (vm.AvatarFile != null && vm.AvatarFile.Length > 0)
        {
            if (!_fileUpload.IsValidImageFile(vm.AvatarFile))
            {
                ModelState.AddModelError("AvatarFile",
                    "Зөвхөн зураг (.jpg, .png, .webp) — дээд тал нь 10MB");
                return View(vm);
            }

            // Delete old avatar
            if (!string.IsNullOrEmpty(user.AvatarPath))
                _fileUpload.DeleteFile(user.AvatarPath);

            user.AvatarPath = await _fileUpload.UploadAvatarAsync(vm.AvatarFile);
        }

        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            return View(vm);
        }

        // Re-sign in to refresh claims (avatar path г шинэчлэхийн тулд)
        await _signIn.RefreshSignInAsync(user);

        TempData["Success"] = "Профайл амжилттай шинэчлэгдлээ!";
        return RedirectToAction(nameof(Index));
    }
}