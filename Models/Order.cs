using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using Store.Resources.Models;

namespace Store.Models;

public class Order
{
    [BindNever] 
    public int OrderID { get; set; }

    [BindNever]
    public ICollection<CartLine> Lines { get; set; }
        = new List<CartLine>();

    public string? UserId { get; set; }

    // public IdentityUser? User { get; set; }

    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public string? Name { get; set; }

    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }

    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public string? City { get; set; }

    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public string? State { get; set; }
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public string? Zip { get; set; }

    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public string? Country { get; set; }

    public string Nonce { get; set; } = "";
    public bool GiftWrap { get; set; }
    
    [BindNever]
    public bool Shipped { get; set; }
    
    public string? PaymentMethod { get; set; }

    public string? PaymentId { get; set; }
    public string? PaymentStatus { get; set; }
}
