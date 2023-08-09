# Web API vs GRPC Benchmark
### This is Benchmark test, comparing performance of **.Net 7 Web API through HTTP/1.1, HTTP/2.0 and GRPC through HTTP/2.0**.

#### For test it solves some tasks:
1. Sequenced log writing, including message, importance level, category, timestamp.
1. Parallel log writing. 
1. Sequenced sending of files with different sizes.

### Это бенчмарк тест, который сравнивает производительность **.Net 7 Web API через HTTP/1.1, HTTP/2.0 и GRPC через HTTP/2.0**.

#### Для теста решается несколько задач:
1. Последовательная запись лога включая сообщение, уровень важности, категорию, временную метку.
1. Параллельная запись лога.
1. Последовательная отправка файлов различного размера.

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3208/22H2/2022Update)

AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores

.NET SDK 7.0.202

  [Host]   : .NET 5.0.17 (5.0.1722.21314), X64 RyuJIT AVX2
  
  .NET 7.0 : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

## Results for sequenced log writing. Результаты последовательной записи лога
|            Method |     Mean |   Error |  StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| WebApiHttp1Async | 167.9 μs | 0.55 μs | 0.51 μs |  1.00 | 0.2441 |   7.52 KB |        1.00 |
| WebApiHttp2Async | 220.2 μs | 0.58 μs | 0.55 μs |  1.31 | 0.4883 |  14.68 KB |        1.95 |
|    GrpcHttp2Async | 158.3 μs | 0.75 μs | 0.66 μs |  0.94 | 0.2441 |   6.79 KB |        0.90 |

## Results for parallel log writing. Результаты параллельной записи лога
|            Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|------------------ |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| WebApiHttp1Async | 22.50 μs | 0.288 μs | 0.270 μs |  1.00 |    0.00 | 0.4395 | 0.0977 |   7.77 KB |        1.00 |
| WebApiHttp2Async | 22.19 μs | 0.398 μs | 0.372 μs |  0.99 |    0.02 | 0.8789 | 0.2441 |  14.96 KB |        1.92 |
|    GrpcHttp2Async | 13.38 μs | 0.090 μs | 0.084 μs |  0.59 |    0.01 | 0.4150 | 0.0732 |   7.05 KB |        0.91 |

## Results for sequenced sending of files. Результаты последовательной отправки файлов
|            Method |  FileSize |         Mean |        Error |       StdDev | Ratio | RatioSD |   Gen0 |   Gen1 |   Gen2 |   Allocated | Alloc Ratio |
|------------------ |---------- |-------------:|-------------:|-------------:|------:|--------:|-------:|-------:|-------:|------------:|------------:|
| **WebApiHttp1Async** |      **1024** |     **156.5 μs** |      **0.64 μs** |      **0.60 μs** |  **1.00** |    **0.00** | **0.2441** |      **-** |      **-** |        **5 KB** |        **1.00** |
| WebApiHttp2Async |      1024 |     200.5 μs |      0.43 μs |      0.40 μs |  1.28 |    0.01 | 0.4883 |      - |      - |     8.72 KB |        1.74 |
|    GrpcHttp2Async |      1024 |     166.3 μs |      0.40 μs |      0.37 μs |  1.06 |    0.00 | 0.2441 |      - |      - |     7.69 KB |        1.54 |
|                   |           |              |              |              |       |         |        |        |        |             |             |
| **WebApiHttp1Async** |   **1048576** |   **5,574.1 μs** |     **46.37 μs** |     **41.11 μs** |  **1.00** |    **0.00** |      **-** |      **-** |      **-** |        **5 KB** |        **1.00** |
| WebApiHttp2Async |   1048576 |   5,844.5 μs |     72.89 μs |     64.62 μs |  1.05 |    0.01 |      - |      - |      - |    18.55 KB |        3.71 |
|    GrpcHttp2Async |   1048576 |   4,042.1 μs |     79.90 μs |     95.11 μs |  0.72 |    0.02 | 7.8125 | 7.8125 | 7.8125 |   1040.4 KB |      208.00 |
|                   |           |              |              |              |       |         |        |        |        |             |             |
| **WebApiHttp1Async** |  **10485760** |  **53,827.2 μs** |    **450.45 μs** |    **421.35 μs** |  **1.00** |    **0.00** |      **-** |      **-** |      **-** |     **5.27 KB** |        **1.00** |
| WebApiHttp2Async |  10485760 |  55,903.6 μs |    259.13 μs |    216.38 μs |  1.04 |    0.01 |      - |      - |      - |    112.2 KB |       21.30 |
|    GrpcHttp2Async |  10485760 |  36,545.3 μs |    727.23 μs |  1,088.48 μs |  0.68 |    0.02 |      - |      - |      - |  10346.4 KB |    1,964.17 |
|                   |           |              |              |              |       |         |        |        |        |             |             |
| **WebApiHttp1Async** | **104857600** | **535,043.5 μs** |  **3,210.17 μs** |  **2,680.63 μs** |  **1.00** |    **0.00** |      **-** |      **-** |      **-** |     **6.41 KB** |        **1.00** |
| WebApiHttp2Async | 104857600 | 559,644.9 μs |  2,768.52 μs |  2,311.84 μs |  1.05 |    0.01 |      - |      - |      - |  1047.34 KB |      163.49 |
|    GrpcHttp2Async | 104857600 | 439,736.5 μs | 11,813.59 μs | 34,647.21 μs |  0.82 |    0.07 |      - |      - |      - | 103405.7 KB |   16,141.38 |

## Conclusions
1. Web API through HTTP/2 works slower for all tasks except parallel sending where it shows the same performance as HTTP/1.1.
1. GRPC is 5% faster than Web API in sending short sequenced requests, like sending log records.
1. GRPC is 40% faster than Web API in sending short parallel requests.
1. GRPC is 6% slower than Web API in sequenced sending 1 KiB files.
1. GRPC is 20-30% faster than Web API in sequenced sending files of 1-100 MiB.

Besides this results, it is needed to be taken into account that GRPC uses binary serialization what can be valuable advantage for reducing network traffic. 

## Выводы
1. Web API через HTTP/2 работает медленнее в .Net 7 во всех задачах, кроме параллельной отправки запросов, где показывает ту же скорость что и HTTP/1.1.
1. GRPC на 5% быстрее, чем Web API при отправке коротких последовательных запросах, вроде отправки записей лога.
1. GRPC на 40% быстрее, чем Web API при отправке коротких параллельных запросов.
1. GRPC на 6% медленнее, чем Web API при последовательной отправке файлов размером 1 КБ.
1. GRPC на 20-30% быстрее, чем Web API при последовательной отправке файлов размером 1-100 МБ.

Кроме приведённых данных нужно учитывать, что GRPC использует бинарную сериализацию, что может быть значительным преимуществом для сокращения сетевого трафика.

## Useful links. Полезные ссылки
1. https://github.com/dotnet/BenchmarkDotNet
1. https://learn.microsoft.com/en-us/aspnet/core/grpc/basics?view=aspnetcore-7.0
1. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http2?view=aspnetcore-7.0
1. https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-7.0&tabs=visual-studio

