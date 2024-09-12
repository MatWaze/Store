using Microsoft.AspNetCore.Mvc;
using Store.Models;

namespace Store.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidationController : Controller
    {
        private DataContext context;
        public ValidationController(DataContext ctx)
        {
            context = ctx;
        }

        [HttpGet("productname")]
        public bool Name(string name)
        {
            return name != null || name != "";
        }
    }
}
