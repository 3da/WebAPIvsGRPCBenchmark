using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using BenchmarkWebApiVsGrpc.WebApp;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace BenchmarkWebApiVsGrpc.TestApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestAsync();

            BenchmarkRunner.Run<CompareLogApiBenchmark>();
            BenchmarkRunner.Run<CompareLogApiParallelBenchmark>();
            BenchmarkRunner.Run<CompareFileApiBenchmark>();
        }

        private static async Task TestAsync()
        {
            var client = new ApiClient();

            var test = await client.HttpClientV2.GetAsync("/api/log");

            var testStr = await test.Content.ReadAsStringAsync();


            for (int i = 0; i < 10; i++)
            {
                await client.GrpcClient.LogAsync(new LogMessage
                {
                    Category = "Category " + (10 - i),
                    Level = (uint)i,
                    Text = "Message " + i,
                    Time = Timestamp.FromDateTime(DateTime.UtcNow)
                });

                var formData = new MultipartFormDataContent
                {
                    {new StringContent("Category " + (10 - i)), "category"},
                    {new StringContent(i.ToString()), "level"},
                    {new StringContent("Message " + i), "text"},
                    {new StringContent(DateTime.UtcNow.ToString("O")), "time"},
                };

                var result = await client.HttpClientV2.PostAsync("/api/log", formData);
                result.EnsureSuccessStatusCode();

                result = await client.HttpClientV1.PostAsync("/api/log", formData);
                result.EnsureSuccessStatusCode();
            }

            var rand = new Random();
            var binaryData = new byte[1024 * 1024 * 100];
            rand.NextBytes(binaryData);

            var title = "Hello world";
            await client.GrpcClient.FileAsync(new BinMessage
            {
                Data = ByteString.CopyFrom(binaryData),
                Title = title
            });

            var multipartFormDataContent = new MultipartFormDataContent
            {
                { new ByteArrayContent(binaryData), "file", title }
            };

            var result2 = await client.HttpClientV2.PostAsync("/api/file", multipartFormDataContent);
            result2.EnsureSuccessStatusCode();

            result2 = await client.HttpClientV1.PostAsync("/api/file", multipartFormDataContent);
            result2.EnsureSuccessStatusCode();
        }
    }
}