using FluentValidation;
using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Shared.Core.Compliance;
using Project420.Shared.Core.Compliance.Validators;

namespace Project420.Management.BLL.StockManagement.Validators;

/// <summary>
/// FluentValidation validator for product updates.
/// Validates all input data and enforces Cannabis Act compliance.
/// </summary>
public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        // ============================================================
        // IDENTITY VALIDATION
        // ============================================================

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");

        // ============================================================
        // BASIC INFORMATION VALIDATION
        // ============================================================

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(CannabisComplianceConstants.SKU_MAX_LENGTH).WithMessage($"SKU cannot exceed {CannabisComplianceConstants.SKU_MAX_LENGTH} characters")
            .Matches(CannabisComplianceConstants.SKU_FORMAT_PATTERN).WithMessage("SKU can only contain uppercase letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(2, 200).WithMessage("Product name must be between 2 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // ============================================================
        // CANNABIS COMPLIANCE VALIDATION (SA Cannabis Act 2024)
        // ============================================================

        RuleFor(x => x.THCPercentage)
            .MaximumLength(CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH).WithMessage($"THC content cannot exceed {CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.THCPercentage));

        RuleFor(x => x.CBDPercentage)
            .MaximumLength(CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH).WithMessage($"CBD content cannot exceed {CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.CBDPercentage));

        // At least one cannabinoid must be specified for cannabis products
        RuleFor(x => x)
            .Must(x => CannabisContentValidator.HasRequiredCannabinoidContent(x.THCPercentage, x.CBDPercentage))
            .WithMessage(CannabisComplianceConstants.ValidationMessages.THC_CBD_REQUIRED)
            .WithName("Cannabis Compliance");

        RuleFor(x => x.BatchNumber)
            .MaximumLength(CannabisComplianceConstants.BATCH_NUMBER_MAX_LENGTH).WithMessage($"Batch number cannot exceed {CannabisComplianceConstants.BATCH_NUMBER_MAX_LENGTH} characters")
            .Must(BatchNumberValidator.IsValidBatchNumber).WithMessage("Batch number format is invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.BatchNumber));

        RuleFor(x => x.StrainName)
            .MaximumLength(CannabisComplianceConstants.STRAIN_NAME_MAX_LENGTH).WithMessage($"Strain name cannot exceed {CannabisComplianceConstants.STRAIN_NAME_MAX_LENGTH} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.StrainName));

        RuleFor(x => x.LabTestDate)
            .Must(LabTestDateValidator.IsValidLabTestDate).WithMessage("Lab test date cannot be in the future")
            .When(x => x.LabTestDate.HasValue);

        RuleFor(x => x.ExpiryDate)
            .Must(LabTestDateValidator.IsValidExpiryDate).WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiryDate.HasValue);

        // ============================================================
        // PRICING VALIDATION
        // ============================================================

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Selling price must be greater than R0")
            .LessThanOrEqualTo(999999.99m).WithMessage("Selling price cannot exceed R999,999.99")
            .Must((dto, price) => price > dto.CostPrice).WithMessage("Selling price must be greater than cost price");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative")
            .LessThanOrEqualTo(999999.99m).WithMessage("Cost price cannot exceed R999,999.99");

        // ============================================================
        // INVENTORY/STOCK VALIDATION
        // ============================================================

        RuleFor(x => x.StockOnHand)
            .GreaterThanOrEqualTo(0).WithMessage("Stock on hand cannot be negative");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative");

        // ============================================================
        // CATEGORY VALIDATION (Future Enhancement)
        // ============================================================

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be a positive number")
            .When(x => x.CategoryId.HasValue);
    }
}
