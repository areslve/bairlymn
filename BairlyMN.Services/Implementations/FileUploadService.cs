using BairlyMN.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BairlyMN.Services.Implementations;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSize = 10 * 1024 * 1024;

    public FileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;
        if (file.Length > MaxFileSize) return false;
        if (!file.ContentType.StartsWith("image/")) return false;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }

    public async Task<string> UploadListingImageAsync(IFormFile file)
        => await SaveFileAsync(file, "uploads");

    public async Task<string> UploadAvatarAsync(IFormFile file)
        => await SaveFileAsync(file, "avatars");

    public async Task<string> UploadBannerAsync(IFormFile file)
        => await SaveFileAsync(file, "banners");

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    private async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var folderPath = Path.Combine(_env.WebRootPath, folder);
        Directory.CreateDirectory(folderPath);
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(folderPath, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/{folder}/{fileName}";
    }
}