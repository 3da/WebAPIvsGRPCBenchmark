using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkWebApiVsGrpc.WebApp;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace BenchmarkWebApiVsGrpc.TestApp
{
    //[SimpleJob(RuntimeMoniker.Net50)]
    //[SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70, baseline: true)]
    [MarkdownExporter]
    [HtmlExporter]
    [MemoryDiagnoser]
    public class FileApiParallelBenchmark
    {
        const int OperationsPerInvoke = 20;
        private const int ChannelsCount = 8;
        private HttpClient[] _httpClients;
        private GrpcLogService.GrpcLogServiceClient[] _grpcClients;

        [Params(1, 1024, 1024 * 10, 1024 * 100)]
        public int FileSize; //In KiB

        [GlobalSetup]
        public void Setup()
        {
            var rand = new Random();
            _binaryData = Enumerable.Range(0, 20)
                .Select(_ =>
                {
                    var buf = new byte[FileSize * 1024];
                    rand.NextBytes(buf);
                    return buf;
                }).ToArray();

            _httpClients = Enumerable.Range(0, ChannelsCount).Select(_ => new ApiClient().HttpClientV1).ToArray();
            _grpcClients = Enumerable.Range(0, ChannelsCount).Select(_ => new ApiClient().GrpcClient).ToArray();
        }

        private int _index = 0;
        private byte[][] _binaryData;


        [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
        public async Task<HttpResponseMessage[]> WebApiHttp1Async()
        {
            return await Task.WhenAll(Enumerable.Range(0, OperationsPerInvoke).Select(_ => Task.Run(async () =>
            {
                var i = Interlocked.Increment(ref _index);
                var buf = _binaryData[i % 20];
                var formData = new MultipartFormDataContent
                {
                    {new ByteArrayContent(buf), "file", "File" + i},
                };

                return await _httpClients[i % ChannelsCount].PostAsync("/api/file", formData);
            })));
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task<Empty[]> GrpcHttp2Async()
        {
            return await Task.WhenAll(Enumerable.Range(0, OperationsPerInvoke).Select(_ => Task.Run(async () =>
            {
                var i = Interlocked.Increment(ref _index);
                var buf = _binaryData[i % 20];
                return await _grpcClients[i % ChannelsCount].FileAsync(new BinMessage
                {
                    Data = UnsafeByteOperations.UnsafeWrap(buf),
                    Title = "File" + i
                });
            })));
        }
    }
}
