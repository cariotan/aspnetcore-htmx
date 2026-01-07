public record TwoFactorModel(
	[Display(Name = "code")]
	string Code,
	bool RememberClient
);