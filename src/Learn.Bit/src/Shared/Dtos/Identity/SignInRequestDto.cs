﻿
namespace Learn.Bit.Shared.Dtos.Identity;

[DtoResourceType(typeof(AppStrings))]
public partial class SignInRequestDto : IdentityRequestDto
{
    /// <example>123456</example>
    [Display(Name = nameof(AppStrings.Password))]
    public string? Password { get; set; }

    /// <summary>
    /// For either Otp or magic link
    /// </summary>
    /// <example>null</example>
    [StringLength(6)]
    [Display(Name = nameof(AppStrings.Otp))]
    public string? Otp { get; set; }

    [JsonIgnore]
    [Display(Name = nameof(AppStrings.RememberMe))]
    public bool RememberMe { get; set; } = true;

    /// <summary>
    /// Two factor code generated by authenticator app
    /// </summary>
    /// <example>null</example>
    [Display(Name = nameof(AppStrings.TwoFactorCode))]
    public string? TwoFactorCode { get; set; }

    /// <example>Samsung Android 14</example>
    public string? DeviceInfo { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var result = base.Validate(validationContext).ToList();

        if (string.IsNullOrEmpty(Password) && string.IsNullOrEmpty(Otp))
        {
            result.Add(new ValidationResult(
                errorMessage: nameof(AppStrings.EitherProvidePasswordOrOtp),
                memberNames: [nameof(Password), nameof(Otp)]
            ));
        }

        return result;
    }
}
