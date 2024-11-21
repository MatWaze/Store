using Store.Resources.Models;
using Store.Validation;
using System.ComponentModel.DataAnnotations;

namespace Store.Models.ViewModels
{
    public class BasicInfoViewModel
    {
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("UserNameDisplay", 
            NameResourceType = typeof(FormNames))]
		[MaxLength(100, ErrorMessageResourceName = "MaxLength",
			ErrorMessageResourceType = typeof(ValidationMessages))]
		public string? UserName { get; set; }

        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
		[LocalizedDisplayName("FullNameDisplay",
			NameResourceType = typeof(FormNames))]
		[MaxLength(100, ErrorMessageResourceName = "MaxLength",
    		ErrorMessageResourceType = typeof(ValidationMessages))]
		public string? FullName { get; set; }
        
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
		[LocalizedDisplayName("EmailDisplay",
			NameResourceType = typeof(FormNames))]
		[MaxLength(1000, ErrorMessageResourceName = "MaxLength",
			ErrorMessageResourceType = typeof(ValidationMessages))]
		public string? Email { get; set; }
    }
}
