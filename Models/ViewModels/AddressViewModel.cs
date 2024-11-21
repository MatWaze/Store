using Microsoft.EntityFrameworkCore;
using Store.Resources.Models;
using Store.Validation;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Store.Models.ViewModels
{
    public class AddressViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("CountryDisplay", NameResourceType = typeof(FormNames))]
		[MaxLength(100, ErrorMessageResourceName = "MaxLength",
			ErrorMessageResourceType = typeof(ValidationMessages))]
		public string? Country { get; set; }

        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("RegionDisplay", NameResourceType = typeof(FormNames))]
		[MaxLength(100, ErrorMessageResourceName = "MaxLength",
			ErrorMessageResourceType = typeof(ValidationMessages))]
		public string? Region { get; set; }

        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("CityDisplay", NameResourceType = typeof(FormNames))]
		[MaxLength(100, ErrorMessageResourceName = "MaxLength",
			ErrorMessageResourceType = typeof(ValidationMessages))]
		public string? City { get; set; }

        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("AddressLine1Display", NameResourceType = typeof(FormNames))]
		[MaxLength(1000, ErrorMessageResourceName = "MaxLength",
    		ErrorMessageResourceType = typeof(ValidationMessages))]
		public string? Street { get; set; }

        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("ZipDisplay", NameResourceType = typeof(FormNames))]
		[MaxLength(10, ErrorMessageResourceName = "MaxLength",
			ErrorMessageResourceType = typeof(ValidationMessages))]
		public string? PostalCode { get; set; }
    }
}
