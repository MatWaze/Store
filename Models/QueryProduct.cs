using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class QueryProduct
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter a name")]
        public string QueryName { get; set; }

        public int CategoryNumber { get; set; }
        
        [Range(1, 99999, ErrorMessage = "Please enter a positive number")]
        public int PriceLow { get; set; }

        [Range(1, 99999, ErrorMessage = "Please enter a positive number")]
        public int PriceHigh { get; set; }
    }
}
