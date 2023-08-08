using System;
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
    public class CompareLogApiBenchmark
    {
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

        [Benchmark(Baseline = true)]
        public async Task<HttpResponseMessage> WebApiHttp1Async()
        {

            var i = Interlocked.Increment(ref _index);
            var formData = new MultipartFormDataContent
                {
                    {new StringContent("Category " + i % 5), "category"},
                    {new StringContent(i.ToString()), "level"},
                    {new StringContent("Message from JSON API with HTTP/1.1: " + i), "text"},
                    {new StringContent(DateTime.UtcNow.ToString("O")), "time"},
                };

            return await _httpClientV1.PostAsync("/api/log", formData);
        }

        [Benchmark]
        public async Task<HttpResponseMessage> WebApiHttp2Async()
        {
            var i = Interlocked.Increment(ref _index);
            var formData = new MultipartFormDataContent
            {
                {new StringContent("Category " + i % 5), "category"},
                {new StringContent(i.ToString()), "level"},
                {new StringContent("Message from JSON API with HTTP/2: " + i), "text"},
                {new StringContent(DateTime.UtcNow.ToString("O")), "time"},
            };

            return await _httpClientV2.PostAsync("/api/log", formData);
        }

        [Benchmark]
        public async Task<Empty> GrpcHttp2Async()
        {
            var i = Interlocked.Increment(ref _index);
            return await _grpcClient.LogAsync(new LogMessage
            {
                Category = "Category " + i % 5,
                Level = (uint)i,
                Text = "Message from GRPC with HTTP/2: " + i,
                Time = Timestamp.FromDateTime(DateTime.UtcNow)
            });
        }

    }
}
