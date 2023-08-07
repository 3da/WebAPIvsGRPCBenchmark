using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWebApiVsGrpc.WebApp.Cotrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly JsonApiLogger _logger;

        public LogController(JsonApiLogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Test()
        {
            return "Hello world";
        }

        [HttpPost]
        public async Task LogAsync([FromForm] DateTime time, [FromForm] int level, [FromForm] string category,
            [FromForm] string text)
        {
            await _logger.EnqueueAsync($"{time.ToString("O")} [{level}] <{category}> {text}");
        }
    }
}
