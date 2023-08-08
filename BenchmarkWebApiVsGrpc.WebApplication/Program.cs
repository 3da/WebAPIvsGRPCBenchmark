using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BenchmarkWebApiVsGrpc.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 500 * 1024 * 1024;
                options.Limits.MaxRequestBufferSize = 10 * 1024 * 1024;
                options.ConfigureHttpsDefaults(q => q.AllowAnyClientCertificate());
                options.Listen(IPAddress.Loopback, 55001, cfg =>
                {
                    cfg.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
                });

                options.Listen(IPAddress.Loopback, 55002, cfg =>
                {
                    cfg.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });
            });

            builder.Services.AddSingleton<GrpcLogger>()
                .AddSingleton<WebApiLogger>();

            builder.Services.AddHostedService(q => q.GetRequiredService<GrpcLogger>());
            builder.Services.AddHostedService(q => q.GetRequiredService<WebApiLogger>());

            builder.Services.AddControllers();
            builder.Services.AddGrpc(q =>
            {
                q.MaxReceiveMessageSize = 500 * 1024 * 1024;
                q.MaxSendMessageSize = 500 * 1024 * 1024;
            });

            var app = builder.Build();


            app.MapControllers();
            app.MapGrpcService<GrpcLogServiceImpl>();


            app.Run();
        }
    }
}