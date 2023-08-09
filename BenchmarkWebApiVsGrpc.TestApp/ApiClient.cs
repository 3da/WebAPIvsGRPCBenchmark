using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Net;
using BenchmarkWebApiVsGrpc.WebApp;

namespace BenchmarkWebApiVsGrpc.TestApp
{
    internal class ApiClient : IDisposable
    {
        private readonly GrpcChannel _channel;
        public HttpClient HttpClientV2 { get; }
        public HttpClient HttpClientV1 { get; }
        public GrpcLogService.GrpcLogServiceClient GrpcClient { get; }

        public ApiClient()
        {
            const string addressV2 = "http://localhost:55002/";
            const string addressV1 = "http://localhost:55001/";

            HttpClientV2 = new HttpClient
            {
                BaseAddress = new Uri(addressV2),
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            };

            HttpClientV1 = new HttpClient
            {
                BaseAddress = new Uri(addressV1),
                DefaultRequestVersion = HttpVersion.Version11,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            };

            _channel = GrpcChannel.ForAddress(addressV2);
            GrpcClient = new GrpcLogService.GrpcLogServiceClient(_channel);
        }

        public void Dispose()
        {
            _channel.Dispose();
            HttpClientV2.Dispose();
            HttpClientV1.Dispose();
        }
    }
}
