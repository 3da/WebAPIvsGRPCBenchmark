using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkWebApiVsGrpc.WebApp;
using Grpc.Core;
using Open.ChannelExtensions;

namespace BenchmarkWebApiVsGrpc.TestApp
{
    //[SimpleJob(RuntimeMoniker.Net50)]
    //[SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70, baseline: true)]
    [MarkdownExporter]
    [HtmlExporter]
    [MemoryDiagnoser]
    public class UserApiParallelBenchmark
    {
        const int DataCount = 1000000;
        const int PagesCount = 1000;
        const int PageSize = DataCount / PagesCount;
        private const int Parallelism = 16;

        [GlobalSetup]
        public async Task SetupAsync()
        {
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = DataCount)]
        public async Task<int> WebApiHttp1Async()
        {
            int result = 0;
            var batches = Enumerable.Range(0, PagesCount).Select(q => q).ToArray();
            await batches.ToChannel().ReadAllConcurrentlyAsync(Parallelism, async from =>
            {
                using var client = new ApiClient();
                var users = await client.HttpClientV1.GetFromJsonAsync<CommonLib.User[]>($"/api/User/getpage?page={from}&size={PageSize}");

                var temp = new HashCode();
                foreach (var user in users)
                {
                    temp.Add(user.CreateDateTime);
                    temp.Add(user.Address);
                    temp.Add(user.Email);
                    temp.Add(user.UserName);
                    temp.Add(user.Age);
                }

                Interlocked.Add(ref result, temp.ToHashCode());
            });

            return result;
        }

        [Benchmark(OperationsPerInvoke = DataCount)]
        public async Task<int> GrpcHttp2Async()
        {
            int result = 0;
            var batches = Enumerable.Range(0, PagesCount).Select(q => q).ToArray();
            await batches.ToChannel().ReadAllConcurrentlyAsync(Parallelism, async from =>
            {
                using var client = new ApiClient();
                var users = client.GrpcClient.Users(new UserRequest()
                {
                    Page = from,
                    PageSize = PageSize
                }).ResponseStream.ReadAllAsync();

                await foreach (var user in users)
                {
                    var temp = new HashCode();
                    temp.Add(user.CreateDateTime.ToDateTime());
                    temp.Add(user.Address);
                    temp.Add(user.Email);
                    temp.Add(user.UserName);
                    temp.Add(user.Age);
                    Interlocked.Add(ref result, temp.ToHashCode());
                }
            });

            return result;
        }

    }
}
