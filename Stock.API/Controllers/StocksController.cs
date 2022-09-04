
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock.API.Models;

namespace Stock.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StocksController : Controller
    {
        private readonly AppDbContext appDbContext;

        public StocksController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        // GET: /<controller>/
        public async Task<IActionResult> GetStock()
        {

            return Ok(await appDbContext.Stocks.ToListAsync());
        }
    }
}
