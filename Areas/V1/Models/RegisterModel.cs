public record RegisterModel(
	[Display(Name = "email")]
	string Email,
	[Display(Name = "password")]
	string Password,
	bool RequireTwoFactor
);
