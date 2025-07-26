using System.ComponentModel.DataAnnotations;

public record LoginModel([Display(Name = "email")] string Email, [Display(Name = "password")] string Password);