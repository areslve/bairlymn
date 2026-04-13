using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace BairlyMN.Mvc.ViewModels;

public enum BannerHeight
{
    Small = 80,
    Medium = 120,
    Large = 180,
    Full = 260
}

public class BannerCreateViewModel
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(400)]
    public string? Subtitle { get; set; }

    [MaxLength(500)]
    public string? LinkUrl { get; set; }

    [Required(ErrorMessage = "Зураг заавал оруулна уу")]
    public IFormFile? ImageFile { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public BannerHeight Height { get; set; } = BannerHeight.Medium;
}

public class BannerEditViewModel
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(400)]
    public string? Subtitle { get; set; }

    [MaxLength(500)]
    public string? LinkUrl { get; set; }

    public IFormFile? ImageFile { get; set; }
    public string? CurrentImagePath { get; set; }

    public bool IsActive { get; set; }
    public int SortOrder { get; set; }

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public BannerHeight Height { get; set; } = BannerHeight.Medium;
}