using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using Store.Resources.Models;
using Npgsql.Internal.TypeHandlers;
using Braintree.Test;
using Store.Validation;

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

	[MaxLength(1000, ErrorMessageResourceName = "MaxLength",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[Required(ErrorMessageResourceName = "Required",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[LocalizedDisplayName("FullNameDisplay", NameResourceType = typeof(FormNames))]
	public string? Name { get; set; }

	[MaxLength(1000, ErrorMessageResourceName = "MaxLength",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[Required(ErrorMessageResourceName = "Required",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[LocalizedDisplayName("AddressLine1Display", NameResourceType = typeof(FormNames))]
	public string? Line1 { get; set; }

	[MaxLength(1000, ErrorMessageResourceName = "MaxLength",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[LocalizedDisplayName("AddressLine2Display", NameResourceType = typeof(FormNames))]
    public string? Line2 { get; set; }

	[MaxLength(1000, ErrorMessageResourceName = "MaxLength",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[LocalizedDisplayName("AddressLine3Display", NameResourceType = typeof(FormNames))]
    public string? Line3 { get; set; }

	[MaxLength(100, ErrorMessageResourceName = "MaxLength",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[Required(ErrorMessageResourceName = "Required",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[LocalizedDisplayName("CityDisplay", NameResourceType = typeof(FormNames))]
    public string? City { get; set; }

	[MaxLength(100, ErrorMessageResourceName = "MaxLength",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[Required(ErrorMessageResourceName = "Required",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[LocalizedDisplayName("RegionDisplay", NameResourceType = typeof(FormNames))]
    public string? State { get; set; }

	[MaxLength(10, ErrorMessageResourceName = "MaxLength",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[Required(ErrorMessageResourceName = "Required",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[LocalizedDisplayName("ZipDisplay", NameResourceType = typeof(FormNames))]
    public string? Zip { get; set; }

	[MaxLength(100, ErrorMessageResourceName = "MaxLength",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[Required(ErrorMessageResourceName = "Required",
		ErrorMessageResourceType = typeof(ValidationMessages))]
	[LocalizedDisplayName("CountryDisplay", NameResourceType = typeof(FormNames))]
    public string? Country { get; set; }

    [BindNever]
    public bool Shipped { get; set; }
    
    public string? PaymentMethod { get; set; }

    public string? PaymentId { get; set; }

    public string? PaymentStatus { get; set; }

	public DateTime? OrderCreationDate { get; set; }
}
