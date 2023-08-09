using BenchmarkWebApiVsGrpc.WebApp.Db;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWebApiVsGrpc.WebApp.Cotrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("getpage")]
        public CommonLib.User[] GetPage([FromQuery] int page, [FromQuery] int size)
        {
            return _userRepository.GetPage(page, size).ToArray();
        }
    }
}
