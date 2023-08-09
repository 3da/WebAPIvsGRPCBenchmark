using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkWebApiVsGrpc.WebApp;
using Google.Protobuf.WellKnownTypes;

namespace BenchmarkWebApiVsGrpc.TestApp
{
    //[SimpleJob(RuntimeMoniker.Net50)]
    //[SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70, baseline: true)]
    [MarkdownExporter]
    [HtmlExporter]
    [MemoryDiagnoser]
    public class LogApiParallelBenchmark
    {
        const int OperationsPerInvoke = 20;
        private HttpClient _httpClientV2;
        private HttpClient _httpClientV1;
        private GrpcLogService.GrpcLogServiceClient _grpcClient;

        [GlobalSetup]
        public async Task SetupAsync()
        {
            var client = new ApiClient();
            _httpClientV2 = client.HttpClientV2;
            _httpClientV1 = client.HttpClientV1;
            _grpcClient = client.GrpcClient;
        }

        private int _index = 0;

        [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
        public async Task<HttpResponseMessage[]> WebApiHttp1Async()
        {
            var client = _httpClientV1;
            return await Task.WhenAll(Enumerable.Range(0, OperationsPerInvoke).Select(e => Task.Run(async () =>
            {
                var i = Interlocked.Increment(ref _index);
                var formData = new MultipartFormDataContent
                {
                    {new StringContent("Category " + e % 5), "category"},
                    {new StringContent(i.ToString()), "level"},
                    {new StringContent("Message from JSON API with HTTP/1.1: " + i), "text"},
                    {new StringContent(DateTime.UtcNow.ToString("O")), "time"},
                };
                return await client.PostAsync("/api/log", formData);
            })));

        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task<HttpResponseMessage[]> WebApiHttp2Async()
        {
            var client = _httpClientV2;

            return await Task.WhenAll(Enumerable.Range(0, OperationsPerInvoke).Select(e => Task.Run(async () =>
            {
                var i = Interlocked.Increment(ref _index);
                var formData = new MultipartFormDataContent
                {
                    {new StringContent("Category " + e % 5), "category"},
                    {new StringContent(i.ToString()), "level"},
                    {new StringContent("Message from JSON API with HTTP/2: " + i), "text"},
                    {new StringContent(DateTime.UtcNow.ToString("O")), "time"},
                };

                return await client.PostAsync("/api/log", formData);
            })));
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public async Task<Empty[]> GrpcHttp2Async()
        {
            var client = _grpcClient;
            return await Task.WhenAll(Enumerable.Range(0, OperationsPerInvoke).Select(e => Task.Run(async () =>
            {
                var i = Interlocked.Increment(ref _index);
                return await client.LogAsync(new LogMessage
                {
                    Category = "Category " + e % 5,
                    Level = (uint)i,
                    Text = "Message from GRPC with HTTP/2: " + i,
                    Time = Timestamp.FromDateTime(DateTime.UtcNow)
                });
            })));
        }

    }
}
