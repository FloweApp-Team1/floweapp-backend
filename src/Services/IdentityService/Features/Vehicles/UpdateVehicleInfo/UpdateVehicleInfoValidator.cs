using FluentValidation;

namespace IdentityService.Features.Vehicles.UpdateVehicleInfo;

public class UpdateVehicleInfoValidator : AbstractValidator<UpdateVehicleInfoCommand>
{
    public UpdateVehicleInfoValidator()
    {
        RuleFor(x => x.VehicleTypeId)
            .NotEmpty()
            .WithMessage("Vehicle type is required.");

        RuleFor(x => x.PlateNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.LicenseDocument)
            .Must(BeValidImage)
            .WithMessage("License document must be a valid JPG, JPEG or PNG file with a maximum size of 5 MB.")
            .When(x => x.LicenseDocument is not null);
    }

    private static bool BeValidImage(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        const long maxSize = 5 * 1024 * 1024; // 5 MB

        if (!allowedExtensions.Contains(extension) || file.Length > maxSize)
            return false;

        return HasValidImageSignature(file);
    }

    private static bool HasValidImageSignature(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var buffer = new byte[8];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);
        stream.Position = 0;

        if (bytesRead < 4)
            return false;

        // JPEG: FF D8 FF
        bool isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        bool isPng = buffer[0] == 0x89 && buffer[1] == 0x50 &&
                     buffer[2] == 0x4E && buffer[3] == 0x47;

        return isJpeg || isPng;
    }
}
