using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using BenchmarkWebApiVsGrpc.WebApp.Db;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace BenchmarkWebApiVsGrpc.WebApp
{
    public class GrpcLogServiceImpl : GrpcLogService.GrpcLogServiceBase
    {
        private readonly GrpcLogger _logger;
        private readonly UserRepository _userRepository;

        public GrpcLogServiceImpl(GrpcLogger logger, UserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public override async Task<Empty> Log(LogMessage request, ServerCallContext context)
        {
            await _logger.EnqueueAsync($"{request.Time.ToDateTime().ToString("O")} [{request.Level}] <{request.Category}> {request.Text}");

            return new Empty();
        }

        public override async Task<Empty> File(BinMessage request, ServerCallContext context)
        {
#if !SKIP_WORK
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(request.Data.ToByteArray());
            var hashStr = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            await _logger.EnqueueAsync($"{DateTime.UtcNow:O} [0] Received file \"{request.Title}\" with md5 hash: {hashStr}");
#endif
            return new Empty();
        }

        public override async Task Users(UserRequest request, IServerStreamWriter<User> responseStream, ServerCallContext context)
        {
            foreach (var user in _userRepository.GetPage(request.Page, request.PageSize).ToArray())
            {
                await responseStream.WriteAsync(new User()
                {
                    Address = user.Address,
                    Age = user.Age,
                    CreateDateTime = Timestamp.FromDateTime(user.CreateDateTime),
                    Email = user.Email,
                    UserName = user.UserName
                }, context.CancellationToken);
            }
        }
    }
}
