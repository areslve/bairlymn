using System.ComponentModel.DataAnnotations;

namespace BairlyMN.Mvc.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "И-мэйл хаяг оруулна уу")]
    [EmailAddress(ErrorMessage = "И-мэйл хаяг буруу байна")]
    [Display(Name = "И-мэйл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Нууц үг оруулна уу")]
    [DataType(DataType.Password)]
    [Display(Name = "Нууц үг")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Намайг сана")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Нэр оруулна уу")]
    [MaxLength(100, ErrorMessage = "Нэр хэт урт байна")]
    [Display(Name = "Нэр")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Овог оруулна уу")]
    [MaxLength(100, ErrorMessage = "Овог хэт урт байна")]
    [Display(Name = "Овог")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "И-мэйл хаяг оруулна уу")]
    [EmailAddress(ErrorMessage = "И-мэйл хаяг буруу байна")]
    [Display(Name = "И-мэйл")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Утасны дугаар буруу байна")]
    [Display(Name = "Утасны дугаар")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Нууц үг оруулна уу")]
    [MinLength(8, ErrorMessage = "Нууц үг хамгийн багадаа 8 тэмдэгт байх ёстой")]
    [DataType(DataType.Password)]
    [Display(Name = "Нууц үг")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Нууц үгийг давтан оруулна уу")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Нууц үг таарахгүй байна")]
    [Display(Name = "Нууц үг давтах")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}