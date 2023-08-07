using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace BenchmarkWebApiVsGrpc.WebApp
{
    public class FileLogWorker : BackgroundService
    {
        private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();
        private readonly ChannelWriter<string> _writer;
        private readonly ChannelReader<string> _reader;
        private readonly FileStream _fileStream;

        public FileLogWorker(string logFilePath)
        {
            _fileStream = File.Open(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = _channel.Writer;
            _reader = _channel.Reader;
        }

        public ValueTask EnqueueAsync(string message, CancellationToken token = default)
        {
#if SKIP_WORK
            return ValueTask.CompletedTask;
#else
            return _writer.WriteAsync(message, token);
#endif
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var message in _reader.ReadAllAsync(stoppingToken))
            {
                await _fileStream.WriteAsync(Encoding.Default.GetBytes(message + "\n"), stoppingToken);
                await _fileStream.FlushAsync(stoppingToken);
            }
        }
    }

    public class GrpcLogger : FileLogWorker
    {
        public GrpcLogger() : base("GRPCLog.txt")
        {
        }
    }

    public class JsonApiLogger : FileLogWorker
    {
        public JsonApiLogger() : base("JSONLog.txt")
        {
        }
    }
}
