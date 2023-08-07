using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkJsonApiVsGrpc.WebApp;
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
    public class CompareFileApiBenchmark
    {
        private HttpClient _httpClientV2;
        private HttpClient _httpClientV1;
        private GrpcLogService.GrpcLogServiceClient _grpcClient;

        [Params(1024, 1024 * 1024, 1024 * 1024 * 10, 1024 * 1024 * 100)]
        public int FileSize;

        [GlobalSetup]
        public void Setup()
        {
            var rand = new Random();
            _binaryData = Enumerable.Range(0, 20).Select(_ =>
            {
                var buf = new byte[FileSize];
                rand.NextBytes(buf);
                return buf;
            }).ToArray();

            var client = new ApiClient();
            _httpClientV2 = client.HttpClientV2;
            _httpClientV1 = client.HttpClientV1;
            _grpcClient = client.GrpcClient;
        }

        private int _index = 0;
        private byte[][] _binaryData;

        [Benchmark(Baseline = true)]
        public async Task<HttpResponseMessage> JsonApiHttp1Async()
        {
            var i = Interlocked.Increment(ref _index);
            var buf = _binaryData[i % 20];
            var formData = new MultipartFormDataContent
                {
                    {new ByteArrayContent(buf), "file", "File" + i},
                };

            return await _httpClientV1.PostAsync("/api/file", formData);
        }

        [Benchmark]
        public async Task<HttpResponseMessage> JsonApiHttp2Async()
        {
            var i = Interlocked.Increment(ref _index);
            var buf = _binaryData[i % 20];
            var formData = new MultipartFormDataContent
            {
                {new ByteArrayContent(buf), "file", "File" + i},
            };

            return await _httpClientV2.PostAsync("/api/file", formData);
        }

        [Benchmark]
        public async Task<Empty> GrpcHttp2Async()
        {
            var i = Interlocked.Increment(ref _index);
            var buf = _binaryData[i % 20];
            return await _grpcClient.FileAsync(new BinMessage
            {
                Data = ByteString.CopyFrom(buf),
                Title = "File" + i
            });
        }

    }
}
