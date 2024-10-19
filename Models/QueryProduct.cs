using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Store.Validation;
using Store.Resources.Models;

namespace Store.Models
{
    public class QueryProduct
    {
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("QueryNameDisplay", NameResourceType = typeof(FormNames))]
        public string? QueryName { get; set; }

        [LocalizedDisplayName("CategoryNumberDisplay", NameResourceType = typeof(FormNames))]
        public int? CategoryNumber { get; set; }

        [Range(1, 9999999, ErrorMessageResourceName = "Range",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("LowRangeDisplay", NameResourceType = typeof(FormNames))]
        public int? PriceLow { get; set; }

        [Range(1, 9999999, ErrorMessageResourceName = "Range",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [Required(ErrorMessageResourceName = "Required",
            ErrorMessageResourceType = typeof(ValidationMessages))]
        [LocalizedDisplayName("MaxRangeDisplay", NameResourceType = typeof(FormNames))]
        public int? PriceHigh { get; set; }
    }
}
