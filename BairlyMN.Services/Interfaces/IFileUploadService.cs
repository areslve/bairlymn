using Microsoft.AspNetCore.Http;

namespace BairlyMN.Services.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadListingImageAsync(IFormFile file);
    Task<string> UploadAvatarAsync(IFormFile file);
    Task<string> UploadBannerAsync(IFormFile file);
    void DeleteFile(string relativePath);
    bool IsValidImageFile(IFormFile file);
}