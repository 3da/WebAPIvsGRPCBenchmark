using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using BenchmarkJsonApiVsGrpc.WebApp;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace BenchmarkWebApiVsGrpc.WebApp
{
    public class GrpcLogServiceImpl : GrpcLogService.GrpcLogServiceBase
    {
        private readonly GrpcLogger _logger;

        public GrpcLogServiceImpl(GrpcLogger logger)
        {
            _logger = logger;
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
    }
}
