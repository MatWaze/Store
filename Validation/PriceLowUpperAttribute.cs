using Store.Models;
using System.ComponentModel.DataAnnotations;

namespace Store.Validation
{
    public class PriceLowUpperAttribute : ValidationAttribute
    {
        public string? Phrase { get; set; }
        public string? Price { get; set; }
        protected override ValidationResult? IsValid(object? value,
        ValidationContext validationContext)
        {
            if (value != null && Phrase != null && Price != null)
            {
                Product? product = value as Product;
                if (product != null
                && product.Name.StartsWith(Phrase,
                StringComparison.OrdinalIgnoreCase)
                && product.Price > decimal.Parse(Price))
                {
                    return new ValidationResult(ErrorMessage ??
                    $"{Phrase} products cannot cost more than $"
                    + Price);
                }
            }
            return ValidationResult.Success;
        }
    }
}
