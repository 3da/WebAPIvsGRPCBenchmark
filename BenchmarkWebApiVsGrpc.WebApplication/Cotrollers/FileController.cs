using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWebApiVsGrpc.WebApp.Cotrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly WebApiLogger _logger;

        public FileController(WebApiLogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Test()
        {
            return "Hello world";
        }

        [HttpPost]
        public async Task ProcessFileAsync(IFormFile file)
        {
#if !SKIP_WORK
            await using var stream = file.OpenReadStream();
            using var md5 = MD5.Create();
            var hash = await md5.ComputeHashAsync(stream);
            var hashStr = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            await _logger.EnqueueAsync($"{DateTime.UtcNow:O} [0] Received file \"{file.FileName}\" with md5 hash: {hashStr}");
#endif
        }
    }
}
