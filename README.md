# InsightsOnFiles
Some performace tests for file-based operations on .NET / .NET Core

This repository contains triangulation and performance tests for FileStreams in .NET Core 2.2 and .NET 4.7.2 on Windows. Especially the performance comparison between asynchronous and synchronous streams is interesting, as the latter performs better than the async version across the board.

The current recommendations are:

- if files are small, load them synchronously. Only when files have several tens of megabytes, you should consider loading them asynchronously (keep in mind, we are loading bytes in these tests, we do not parse anything. But parsing is usually done in memory).
- Larger buffer sizes reduce the total time tremendously. Keep in mind that buffers larger than 84975 bytes will be placed on the Large Object Heap.
- `FileOptions.SequentialScan` has no significant impact (I only tested files up to 100MB)
- There is no significant impact when changing the file stream's internal buffer size.
- On .NET Core, if your stream buffer size is larger than the file size (same size won't be enough), then `ReadAsync` will complete synchronously (see [this automated test](https://github.com/feO2x/InsightsOnFiles/blob/22133bcf746513b9f298d442c008f56da626fa9c/InsightsOnFiles.Tests/AFileStreamOnWindows.cs#L52)).

Please keep the following things in mind:
- FileSize is the file size in bytes that was read. They range from 1KB to 100MB.
- BufferSize is the amount of bytes that were used in buffer arrays (both passed to the FileStream and used as the caller's buffer).
- When IsDBS is set to true, then the FileStream's internal buffer size is set to the default value of 4096. If it is false, then BufferSize is set.
- All files were read  sequentially from start to finish.
- All tests were performed on an SSD.
- Mean                : Arithmetic mean of all measurements
- Error               : Half of 99.9% confidence interval
- StdDev              : Standard deviation of all measurements
- Gen 0/1k Op         : GC Generation 0 collects per 1k Operations
- Gen 1/1k Op         : GC Generation 1 collects per 1k Operations
- Gen 2/1k Op         : GC Generation 2 collects per 1k Operations
- Allocated Memory/Op : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
- 1 us                : 1 Microsecond (0.000001 sec)

// * Summary *

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17763.379 (1809/October2018Update/Redstone5)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.0.100-preview3-010431
  [Host] : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
  Clr    : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3362.0
  Core   : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT


|                           Method | Runtime |  FileSize | BufferSize | IsDBS |            Mean |          Error |         StdDev |          Median | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|--------------------------------- |-------- |---------- |----------- |------ |----------------:|---------------:|---------------:|----------------:|------------:|------------:|------------:|--------------------:|
|                  AsyncFileStream |     Clr |      1024 |        512 | False |       170.56 us |      1.4135 us |      1.2531 us |       169.99 us |      0.2441 |           - |           - |              1594 B |
|                   SyncFileStream |     Clr |      1024 |        512 | False |        82.49 us |      0.6349 us |      0.5939 us |        82.32 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |        512 | False |       171.53 us |      1.8614 us |      1.7412 us |       170.72 us |      0.2441 |           - |           - |              1594 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |        512 | False |        80.44 us |      0.4994 us |      0.4427 us |        80.30 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |        512 | False |       155.79 us |      1.4404 us |      1.3474 us |       155.98 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |        512 | False |        79.47 us |      0.1840 us |      0.1631 us |        79.43 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |        512 | False |       153.64 us |      1.7769 us |      1.6622 us |       154.10 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |        512 | False |        81.98 us |      1.2541 us |      1.1731 us |        82.49 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |      1024 |        512 |  True |       149.17 us |      0.8578 us |      0.8024 us |       149.01 us |      0.9766 |           - |           - |              5740 B |
|                   SyncFileStream |     Clr |      1024 |        512 |  True |        78.35 us |      0.4490 us |      0.3980 us |        78.41 us |      0.8545 |           - |           - |              4427 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |        512 |  True |       155.84 us |      0.8707 us |      0.8145 us |       155.88 us |      0.9766 |           - |           - |              5742 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |        512 |  True |        80.09 us |      1.0782 us |      1.0085 us |        79.94 us |      0.8545 |           - |           - |              4427 B |
|                  AsyncFileStream |    Core |      1024 |        512 |  True |       100.30 us |      1.9202 us |      1.7961 us |        98.99 us |      0.9766 |           - |           - |              5074 B |
|                   SyncFileStream |    Core |      1024 |        512 |  True |        83.30 us |      1.5365 us |      1.4373 us |        83.69 us |      0.9766 |           - |           - |              4640 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |        512 |  True |       104.09 us |      1.6703 us |      1.5624 us |       104.39 us |      0.9766 |           - |           - |              5077 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |        512 |  True |        82.96 us |      1.6087 us |      1.6521 us |        82.81 us |      0.9766 |           - |           - |              4640 B |
|                  AsyncFileStream |     Clr |      1024 |       4096 | False |       134.27 us |      2.3009 us |      2.1523 us |       134.32 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |      1024 |       4096 | False |        82.31 us |      1.5905 us |      1.4099 us |        81.74 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |       4096 | False |       134.75 us |      2.2244 us |      1.9718 us |       134.57 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |       4096 | False |        83.40 us |      1.6515 us |      2.4207 us |        83.25 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |       4096 | False |       128.89 us |      0.6804 us |      0.6365 us |       128.68 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |       4096 | False |        81.98 us |      1.5813 us |      1.7576 us |        81.45 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |       4096 | False |       129.73 us |      0.7186 us |      0.6722 us |       129.57 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |       4096 | False |        80.82 us |      1.5884 us |      1.6312 us |        80.56 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |      1024 |       4096 |  True |       135.37 us |      3.2561 us |      3.3437 us |       134.63 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |      1024 |       4096 |  True |        81.70 us |      1.5715 us |      1.6815 us |        81.51 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |       4096 |  True |       135.83 us |      2.6098 us |      3.3005 us |       135.25 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |       4096 |  True |        83.22 us |      1.6560 us |      1.7719 us |        82.92 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |       4096 |  True |       129.36 us |      1.3810 us |      1.2918 us |       129.26 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |       4096 |  True |        81.50 us |      1.5767 us |      1.9364 us |        81.69 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |       4096 |  True |       130.69 us |      1.1529 us |      1.0784 us |       130.52 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |       4096 |  True |        80.81 us |      1.6089 us |      1.9152 us |        80.07 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |      1024 |      16384 | False |       133.75 us |      2.5513 us |      2.5057 us |       133.35 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |      1024 |      16384 | False |        81.79 us |      1.3889 us |      1.2313 us |        81.84 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |      16384 | False |       133.78 us |      3.0227 us |      2.8275 us |       133.03 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |      16384 | False |        83.11 us |      1.6497 us |      2.4181 us |        82.87 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |      16384 | False |       132.05 us |      0.5110 us |      0.4780 us |       131.92 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |      16384 | False |        81.05 us |      1.5917 us |      2.2313 us |        81.05 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |      16384 | False |       131.88 us |      1.3734 us |      1.2847 us |       131.97 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |      16384 | False |        81.98 us |      1.6080 us |      2.0336 us |        81.49 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |      1024 |      16384 |  True |       133.35 us |      2.4367 us |      2.5023 us |       133.74 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |      1024 |      16384 |  True |        82.49 us |      1.9446 us |      2.1614 us |        81.83 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |      16384 |  True |       134.63 us |      2.6662 us |      2.7380 us |       134.26 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |      16384 |  True |        83.31 us |      1.6591 us |      3.4631 us |        82.08 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |      16384 |  True |       130.63 us |      0.7292 us |      0.6465 us |       130.68 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |      16384 |  True |        81.40 us |      1.6076 us |      2.5498 us |        81.00 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |      16384 |  True |       128.76 us |      1.2815 us |      1.1987 us |       128.96 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |      16384 |  True |        80.71 us |      1.3633 us |      1.2753 us |        80.24 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |      1024 |      84975 | False |       133.59 us |      2.6677 us |      2.6201 us |       133.43 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |      1024 |      84975 | False |        80.86 us |      1.2955 us |      1.1485 us |        80.81 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |      84975 | False |       135.18 us |      2.4796 us |      2.3194 us |       134.75 us |      0.2441 |           - |           - |              1206 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |      84975 | False |        83.00 us |      1.6453 us |      2.7033 us |        82.59 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |      84975 | False |       130.84 us |      1.3926 us |      1.3026 us |       130.89 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |      84975 | False |        82.06 us |      1.6055 us |      1.7178 us |        81.79 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |      84975 | False |       129.09 us |      0.8713 us |      0.8150 us |       129.13 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |      84975 | False |        82.07 us |      1.6109 us |      1.9177 us |        82.03 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |      1024 |      84975 |  True |       135.55 us |      2.6300 us |      3.0288 us |       135.38 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |      1024 |      84975 |  True |        83.59 us |      1.6416 us |      2.5069 us |        83.62 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |      84975 |  True |       136.55 us |      2.0373 us |      1.9057 us |       136.83 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |      84975 |  True |        82.47 us |      1.6329 us |      2.5423 us |        81.72 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |      84975 |  True |       130.62 us |      0.9122 us |      0.8533 us |       130.54 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |      84975 |  True |        82.79 us |      1.5618 us |      1.3845 us |        83.19 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |      84975 |  True |       129.59 us |      0.7712 us |      0.7214 us |       129.90 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |      84975 |  True |        80.87 us |      1.5722 us |      1.9309 us |        80.68 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |      1024 |    1048576 | False |       135.04 us |      3.0398 us |      2.6947 us |       134.62 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |      1024 |    1048576 | False |        84.10 us |      1.6658 us |      3.1288 us |        83.55 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |    1048576 | False |       133.34 us |      1.8420 us |      1.7230 us |       134.05 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |    1048576 | False |        83.89 us |      1.7467 us |      2.0793 us |        83.69 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |    1048576 | False |       126.57 us |      1.0327 us |      0.9660 us |       126.58 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |    1048576 | False |        79.47 us |      0.7304 us |      0.6832 us |        79.67 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |    1048576 | False |       124.34 us |      1.3425 us |      1.2558 us |       123.98 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |    1048576 | False |        78.81 us |      0.4049 us |      0.3787 us |        78.65 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |      1024 |    1048576 |  True |       130.17 us |      1.4318 us |      1.3393 us |       130.46 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |      1024 |    1048576 |  True |        78.55 us |      0.5068 us |      0.4741 us |        78.38 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |      1024 |    1048576 |  True |       128.54 us |      0.7197 us |      0.6380 us |       128.60 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |      1024 |    1048576 |  True |        79.37 us |      0.7126 us |      0.6666 us |        79.37 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |      1024 |    1048576 |  True |       129.19 us |      0.6709 us |      0.6276 us |       129.16 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |      1024 |    1048576 |  True |        77.74 us |      0.1835 us |      0.1716 us |        77.68 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |      1024 |    1048576 |  True |       124.04 us |      0.9381 us |      0.8775 us |       123.73 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |      1024 |    1048576 |  True |        79.77 us |      0.6872 us |      0.6428 us |        79.99 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |        512 | False |     3,656.84 us |     24.4327 us |     22.8544 us |     3,660.39 us |      7.8125 |      3.9063 |           - |             50785 B |
|                   SyncFileStream |     Clr |     65536 |        512 | False |       352.64 us |      1.7121 us |      1.5178 us |       352.05 us |           - |           - |           - |               308 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |        512 | False |     3,720.38 us |     52.0775 us |     48.7133 us |     3,747.05 us |      7.8125 |      3.9063 |           - |             50881 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |        512 | False |       351.77 us |      1.0388 us |      0.9717 us |       351.91 us |           - |           - |           - |               308 B |
|                  AsyncFileStream |    Core |     65536 |        512 | False |     2,913.13 us |     26.2866 us |     24.5885 us |     2,909.84 us |      7.8125 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |        512 | False |       350.51 us |      0.9706 us |      0.8105 us |       350.57 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |        512 | False |     2,892.35 us |     35.1118 us |     32.8436 us |     2,897.02 us |      7.8125 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |        512 | False |       357.43 us |      3.4151 us |      3.1945 us |       358.03 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |        512 |  True |       963.76 us |      2.6922 us |      2.3865 us |       963.18 us |      7.8125 |      1.9531 |           - |             43825 B |
|                   SyncFileStream |     Clr |     65536 |        512 |  True |       117.55 us |      0.6029 us |      0.5640 us |       117.32 us |      0.8545 |           - |           - |              4427 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |        512 |  True |       959.83 us |      3.0011 us |      2.6604 us |       959.65 us |      8.7891 |      0.9766 |           - |             43837 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |        512 |  True |       116.97 us |      0.2971 us |      0.2780 us |       116.90 us |      0.8545 |           - |           - |              4426 B |
|                  AsyncFileStream |    Core |     65536 |        512 |  True |       349.13 us |      1.4308 us |      1.3384 us |       348.84 us |      1.4648 |           - |           - |              7234 B |
|                   SyncFileStream |    Core |     65536 |        512 |  True |       117.98 us |      0.6106 us |      0.5099 us |       118.02 us |      0.9766 |           - |           - |              4640 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |        512 |  True |       346.20 us |      1.5081 us |      1.4107 us |       346.19 us |      1.4648 |           - |           - |              7236 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |        512 |  True |       117.01 us |      0.5989 us |      0.5602 us |       116.72 us |      0.9766 |           - |           - |              4640 B |
|                  AsyncFileStream |     Clr |     65536 |       4096 | False |       609.75 us |     11.7802 us |     11.5697 us |       614.66 us |      0.9766 |           - |           - |              7112 B |
|                   SyncFileStream |     Clr |     65536 |       4096 | False |       112.97 us |      0.3480 us |      0.3255 us |       112.89 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |       4096 | False |       595.15 us |      5.9455 us |      5.2706 us |       593.63 us |      0.9766 |           - |           - |              7112 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |       4096 | False |       115.27 us |      1.3560 us |      1.2684 us |       114.97 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |     65536 |       4096 | False |       494.11 us |      5.5974 us |      5.2358 us |       494.88 us |      0.9766 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |       4096 | False |       113.07 us |      0.5249 us |      0.4910 us |       113.07 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |       4096 | False |       509.38 us |      8.3466 us |      7.8074 us |       509.92 us |      0.9766 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |       4096 | False |       112.21 us |      0.2564 us |      0.2398 us |       112.29 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |       4096 |  True |       590.65 us |     12.4082 us |     14.7711 us |       591.30 us |      0.9766 |           - |           - |              7112 B |
|                   SyncFileStream |     Clr |     65536 |       4096 |  True |       114.46 us |      0.3212 us |      0.2507 us |       114.40 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |       4096 |  True |       576.45 us |     11.4175 us |     14.8460 us |       569.54 us |      0.9766 |           - |           - |              7112 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |       4096 |  True |       112.74 us |      0.4624 us |      0.4099 us |       112.64 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |     65536 |       4096 |  True |       502.65 us |      8.0318 us |      7.5129 us |       500.13 us |      0.9766 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |       4096 |  True |       112.82 us |      0.3984 us |      0.3727 us |       112.82 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |       4096 |  True |       480.56 us |      8.9026 us |      8.7435 us |       482.04 us |      0.9766 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |       4096 |  True |       114.05 us |      0.2725 us |      0.2275 us |       114.00 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |      16384 | False |       250.81 us |      0.7071 us |      0.5905 us |       250.76 us |      0.4883 |           - |           - |              2368 B |
|                   SyncFileStream |     Clr |     65536 |      16384 | False |        86.98 us |      0.2803 us |      0.2485 us |        86.98 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |      16384 | False |       254.37 us |      1.3762 us |      1.2873 us |       253.93 us |      0.4883 |           - |           - |              2384 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |      16384 | False |        88.12 us |      0.1445 us |      0.1128 us |        88.15 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |     65536 |      16384 | False |       203.57 us |      3.0191 us |      2.8241 us |       204.11 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |      16384 | False |        88.67 us |      0.8888 us |      0.8313 us |        88.96 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |      16384 | False |       205.61 us |      1.2549 us |      1.1125 us |       205.78 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |      16384 | False |        87.66 us |      0.6691 us |      0.5587 us |        87.48 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |      16384 |  True |       255.38 us |      1.1701 us |      1.0372 us |       255.56 us |      0.4883 |           - |           - |              2376 B |
|                   SyncFileStream |     Clr |     65536 |      16384 |  True |        87.22 us |      0.2523 us |      0.2360 us |        87.27 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |      16384 |  True |       251.76 us |      1.0051 us |      0.8393 us |       251.83 us |      0.4883 |           - |           - |              2368 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |      16384 |  True |        89.91 us |      0.2132 us |      0.1994 us |        89.92 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |     65536 |      16384 |  True |       208.03 us |      2.3489 us |      2.1972 us |       208.69 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |      16384 |  True |        87.78 us |      0.5788 us |      0.5414 us |        87.63 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |      16384 |  True |       210.03 us |      4.0748 us |      3.8116 us |       210.27 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |      16384 |  True |        86.67 us |      0.2818 us |      0.2498 us |        86.59 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |      84975 | False |       132.50 us |      0.4421 us |      0.4135 us |       132.46 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |     65536 |      84975 | False |        80.58 us |      0.2805 us |      0.2624 us |        80.54 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |      84975 | False |       130.99 us |      1.2547 us |      1.1737 us |       130.75 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |      84975 | False |        83.29 us |      1.3448 us |      1.2579 us |        83.25 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |     65536 |      84975 | False |       126.61 us |      0.9373 us |      0.8768 us |       126.41 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |      84975 | False |        80.67 us |      0.5946 us |      0.5562 us |        80.45 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |      84975 | False |       129.23 us |      0.8422 us |      0.7466 us |       128.99 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |      84975 | False |        79.73 us |      0.4084 us |      0.3621 us |        79.60 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |      84975 |  True |       131.45 us |      1.6644 us |      1.5569 us |       130.64 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |     65536 |      84975 |  True |        84.14 us |      1.1864 us |      1.1098 us |        84.47 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |      84975 |  True |       131.32 us |      1.1079 us |      0.9821 us |       131.11 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |      84975 |  True |        80.46 us |      0.3184 us |      0.2978 us |        80.43 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |     65536 |      84975 |  True |       130.16 us |      1.4392 us |      1.3463 us |       130.03 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |      84975 |  True |        82.11 us |      1.3672 us |      1.2789 us |        82.54 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |      84975 |  True |       127.77 us |      1.2281 us |      1.1487 us |       127.78 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |      84975 |  True |        80.63 us |      0.3092 us |      0.2582 us |        80.56 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |    1048576 | False |       132.85 us |      0.4909 us |      0.4591 us |       132.62 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |     65536 |    1048576 | False |        81.43 us |      0.1423 us |      0.1111 us |        81.47 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |    1048576 | False |       133.09 us |      1.7252 us |      1.6137 us |       133.28 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |    1048576 | False |        83.95 us |      1.2858 us |      1.2027 us |        84.23 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |     65536 |    1048576 | False |       127.05 us |      1.3086 us |      1.2241 us |       127.14 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |    1048576 | False |        81.34 us |      0.2172 us |      0.1814 us |        81.39 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |    1048576 | False |       128.56 us |      0.3813 us |      0.3380 us |       128.57 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |    1048576 | False |        80.60 us |      0.4293 us |      0.4015 us |        80.56 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |     65536 |    1048576 |  True |       132.02 us |      2.3842 us |      2.2302 us |       130.91 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |     65536 |    1048576 |  True |        83.91 us |      1.5621 us |      1.4612 us |        83.94 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |     65536 |    1048576 |  True |       130.49 us |      0.3871 us |      0.3432 us |       130.40 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |     65536 |    1048576 |  True |        82.58 us |      0.4510 us |      0.3766 us |        82.65 us |           - |           - |           - |               305 B |
|                  AsyncFileStream |    Core |     65536 |    1048576 |  True |       130.92 us |      1.0710 us |      1.0018 us |       131.30 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |     65536 |    1048576 |  True |        80.53 us |      0.2920 us |      0.2589 us |        80.56 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |     65536 |    1048576 |  True |       128.05 us |      2.1685 us |      2.0285 us |       127.53 us |      0.2441 |           - |           - |              1047 B |
|  SyncFileStreamSequentialOptions |    Core |     65536 |    1048576 |  True |        83.25 us |      1.2474 us |      1.1668 us |        83.88 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |        512 | False |    55,887.69 us |    213.2876 us |    199.5093 us |    55,923.62 us |    111.1111 |           - |           - |            801922 B |
|                   SyncFileStream |     Clr |   1048576 |        512 | False |     4,461.32 us |     19.8776 us |     17.6209 us |     4,460.84 us |           - |           - |           - |               320 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |        512 | False |    56,241.41 us |    725.5659 us |    678.6948 us |    56,088.57 us |    111.1111 |           - |           - |            801922 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |        512 | False |     4,444.26 us |     19.1221 us |     17.8868 us |     4,436.79 us |           - |           - |           - |               320 B |
|                  AsyncFileStream |    Core |   1048576 |        512 | False |    44,272.58 us |    330.7788 us |    309.4107 us |    44,327.29 us |     83.3333 |           - |           - |              1057 B |
|                   SyncFileStream |    Core |   1048576 |        512 | False |     4,444.94 us |     17.0965 us |     15.9921 us |     4,443.46 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |        512 | False |    44,224.94 us |    807.3652 us |    755.2099 us |    43,881.38 us |     83.3333 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |        512 | False |     4,506.66 us |     48.2879 us |     45.1685 us |     4,511.40 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |        512 |  True |    13,007.62 us |     21.7590 us |     19.2887 us |    13,008.24 us |    109.3750 |     15.6250 |           - |            583820 B |
|                   SyncFileStream |     Clr |   1048576 |        512 |  True |       714.06 us |      1.4400 us |      1.2766 us |       714.13 us |           - |           - |           - |              4432 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |        512 |  True |    13,069.43 us |     39.2385 us |     36.7037 us |    13,081.60 us |    109.3750 |     15.6250 |           - |            584333 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |        512 |  True |       715.92 us |      3.2267 us |      3.0182 us |       716.04 us |           - |           - |           - |              4432 B |
|                  AsyncFileStream |    Core |   1048576 |        512 |  True |     4,299.52 us |     19.0641 us |     17.8325 us |     4,302.25 us |      7.8125 |           - |           - |             41808 B |
|                   SyncFileStream |    Core |   1048576 |        512 |  True |       714.98 us |      6.7483 us |      6.3124 us |       712.06 us |      0.9766 |           - |           - |              4640 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |        512 |  True |     4,334.26 us |     16.4776 us |     15.4131 us |     4,331.42 us |      7.8125 |           - |           - |             41818 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |        512 |  True |       707.60 us |      1.9440 us |      1.7233 us |       707.43 us |      0.9766 |           - |           - |              4640 B |
|                  AsyncFileStream |     Clr |   1048576 |       4096 | False |     7,308.94 us |     63.6267 us |     59.5164 us |     7,293.31 us |     15.6250 |      7.8125 |           - |            100674 B |
|                   SyncFileStream |     Clr |   1048576 |       4096 | False |       667.33 us |      3.9544 us |      3.6989 us |       667.80 us |           - |           - |           - |               312 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |       4096 | False |     7,194.11 us |     30.2396 us |     26.8066 us |     7,186.67 us |     15.6250 |      7.8125 |           - |            100866 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |       4096 | False |       661.59 us |      2.8877 us |      2.5599 us |       661.88 us |           - |           - |           - |               312 B |
|                  AsyncFileStream |    Core |   1048576 |       4096 | False |     5,756.27 us |    106.9347 us |    100.0268 us |     5,720.33 us |     15.6250 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |   1048576 |       4096 | False |       654.90 us |      3.2749 us |      3.0633 us |       653.29 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |       4096 | False |     5,750.39 us |     59.7183 us |     55.8605 us |     5,733.96 us |     15.6250 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |       4096 | False |       663.39 us |      6.4907 us |      5.7539 us |       663.04 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |       4096 |  True |     7,212.18 us |     56.2382 us |     52.6052 us |     7,216.65 us |     15.6250 |      7.8125 |           - |            100482 B |
|                   SyncFileStream |     Clr |   1048576 |       4096 |  True |       663.00 us |      3.9719 us |      3.5210 us |       663.17 us |           - |           - |           - |               312 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |       4096 |  True |     7,359.44 us |     33.9843 us |     31.7889 us |     7,357.15 us |     15.6250 |      7.8125 |           - |            100674 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |       4096 |  True |       657.35 us |      1.5648 us |      1.3872 us |       657.29 us |           - |           - |           - |               312 B |
|                  AsyncFileStream |    Core |   1048576 |       4096 |  True |     5,764.34 us |     50.5032 us |     47.2407 us |     5,755.60 us |     15.6250 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |   1048576 |       4096 |  True |       665.58 us |      7.6158 us |      7.1238 us |       666.44 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |       4096 |  True |     5,775.23 us |     39.3826 us |     36.8385 us |     5,759.59 us |     15.6250 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |       4096 |  True |       658.71 us |      2.9784 us |      2.4871 us |       658.74 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |      16384 | False |     1,979.08 us |      6.6668 us |      5.5671 us |     1,977.50 us |      3.9063 |           - |           - |             25857 B |
|                   SyncFileStream |     Clr |   1048576 |      16384 | False |       254.97 us |      1.6169 us |      1.4334 us |       254.82 us |           - |           - |           - |               308 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |      16384 | False |     1,970.81 us |      2.4388 us |      2.1619 us |     1,970.46 us |      3.9063 |           - |           - |             25857 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |      16384 | False |       251.29 us |      1.5440 us |      1.4443 us |       251.56 us |           - |           - |           - |               308 B |
|                  AsyncFileStream |    Core |   1048576 |      16384 | False |     1,652.44 us |      9.4633 us |      8.8520 us |     1,649.80 us |      3.9063 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |   1048576 |      16384 | False |       251.24 us |      1.0582 us |      0.9381 us |       250.95 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |      16384 | False |     1,650.37 us |     32.4220 us |     36.0369 us |     1,648.07 us |      3.9063 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |      16384 | False |       249.65 us |      2.9630 us |      2.6266 us |       248.69 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |      16384 |  True |     2,025.72 us |     21.0835 us |     19.7215 us |     2,033.82 us |      3.9063 |           - |           - |             25825 B |
|                   SyncFileStream |     Clr |   1048576 |      16384 |  True |       253.11 us |      1.1927 us |      1.0573 us |       252.83 us |           - |           - |           - |               308 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |      16384 |  True |     1,992.72 us |     14.2424 us |     12.6255 us |     1,988.45 us |      3.9063 |           - |           - |             25921 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |      16384 |  True |       255.55 us |      2.1888 us |      1.9404 us |       255.13 us |           - |           - |           - |               308 B |
|                  AsyncFileStream |    Core |   1048576 |      16384 |  True |     1,624.56 us |     20.7031 us |     19.3657 us |     1,621.77 us |      3.9063 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |   1048576 |      16384 |  True |       251.00 us |      2.6429 us |      2.4722 us |       250.20 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |      16384 |  True |     1,624.81 us |     16.0023 us |     14.9686 us |     1,627.86 us |      3.9063 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |      16384 |  True |       251.14 us |      0.7568 us |      0.6319 us |       250.86 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |      84975 | False |       552.24 us |     10.9842 us |     19.2379 us |       555.02 us |      0.9766 |           - |           - |              5944 B |
|                   SyncFileStream |     Clr |   1048576 |      84975 | False |       148.22 us |      1.7142 us |      1.6035 us |       148.20 us |           - |           - |           - |               306 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |      84975 | False |       535.93 us |      7.5186 us |      6.6650 us |       533.43 us |      0.9766 |           - |           - |              5952 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |      84975 | False |       145.28 us |      0.7926 us |      0.7414 us |       145.10 us |           - |           - |           - |               306 B |
|                  AsyncFileStream |    Core |   1048576 |      84975 | False |       484.98 us |      4.4078 us |      4.1231 us |       485.63 us |      0.9766 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |   1048576 |      84975 | False |       144.93 us |      0.3837 us |      0.3204 us |       144.84 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |      84975 | False |       463.08 us |      5.0092 us |      4.6856 us |       461.91 us |      0.9766 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |      84975 | False |       148.31 us |      0.9835 us |      0.8213 us |       148.47 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |      84975 |  True |       536.86 us |     11.1121 us |     15.5776 us |       539.70 us |      0.9766 |           - |           - |              5944 B |
|                   SyncFileStream |     Clr |   1048576 |      84975 |  True |       148.03 us |      1.5950 us |      1.4919 us |       147.80 us |           - |           - |           - |               306 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |      84975 |  True |       556.79 us |     10.1273 us |      9.4731 us |       555.87 us |      0.9766 |           - |           - |              5944 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |      84975 |  True |       144.88 us |      0.6389 us |      0.5664 us |       144.79 us |           - |           - |           - |               306 B |
|                  AsyncFileStream |    Core |   1048576 |      84975 |  True |       474.07 us |      6.7558 us |      6.3194 us |       475.12 us |      0.9766 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |   1048576 |      84975 |  True |       147.27 us |      0.5226 us |      0.4364 us |       147.32 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |      84975 |  True |       471.38 us |      6.1873 us |      5.7876 us |       471.49 us |      0.9766 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |      84975 |  True |       146.96 us |      1.2928 us |      1.2093 us |       146.76 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |    1048576 | False |       176.31 us |      1.9534 us |      1.8272 us |       176.59 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |   1048576 |    1048576 | False |       121.53 us |      0.3779 us |      0.3535 us |       121.50 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |    1048576 | False |       174.88 us |      2.1408 us |      2.0025 us |       174.60 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |    1048576 | False |       126.69 us |      1.1872 us |      0.9914 us |       126.84 us |           - |           - |           - |               306 B |
|                  AsyncFileStream |    Core |   1048576 |    1048576 | False |       173.73 us |      2.6133 us |      2.4445 us |       173.84 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |   1048576 |    1048576 | False |       119.60 us |      0.4906 us |      0.4096 us |       119.46 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |    1048576 | False |       173.83 us |      1.7791 us |      1.5771 us |       173.72 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |    1048576 | False |       119.92 us |      1.0359 us |      0.9183 us |       119.94 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr |   1048576 |    1048576 |  True |       175.79 us |      1.1826 us |      1.0483 us |       176.15 us |      0.2441 |           - |           - |              1204 B |
|                   SyncFileStream |     Clr |   1048576 |    1048576 |  True |       121.38 us |      0.4923 us |      0.4364 us |       121.28 us |           - |           - |           - |               305 B |
| AsyncFileStreamSequentialOptions |     Clr |   1048576 |    1048576 |  True |       174.77 us |      3.3201 us |      3.1056 us |       173.46 us |      0.2441 |           - |           - |              1204 B |
|  SyncFileStreamSequentialOptions |     Clr |   1048576 |    1048576 |  True |       127.11 us |      2.5036 us |      2.4588 us |       128.55 us |           - |           - |           - |               306 B |
|                  AsyncFileStream |    Core |   1048576 |    1048576 |  True |       166.16 us |      1.4558 us |      1.1366 us |       165.96 us |      0.2441 |           - |           - |              1048 B |
|                   SyncFileStream |    Core |   1048576 |    1048576 |  True |       120.11 us |      0.6715 us |      0.5242 us |       120.14 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core |   1048576 |    1048576 |  True |       169.88 us |      2.9349 us |      2.7453 us |       168.71 us |      0.2441 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core |   1048576 |    1048576 |  True |       123.02 us |      3.3123 us |      3.0983 us |       122.06 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |        512 | False | 5,642,547.67 us | 53,516.0466 us | 50,058.9432 us | 5,645,766.40 us |  16000.0000 |   8000.0000 |           - |          79546128 B |
|                   SyncFileStream |     Clr | 104857600 |        512 | False |   458,007.59 us |  2,493.9397 us |  2,332.8327 us |   457,997.90 us |           - |           - |           - |              8192 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |        512 | False | 5,647,922.72 us | 38,590.7363 us | 36,097.7987 us | 5,642,582.90 us |  16000.0000 |   8000.0000 |           - |          79603720 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |        512 | False |   460,617.80 us |  3,582.0937 us |  3,175.4327 us |   460,126.55 us |           - |           - |           - |              8192 B |
|                  AsyncFileStream |    Core | 104857600 |        512 | False | 4,324,020.13 us | 28,865.1451 us | 27,000.4747 us | 4,331,692.00 us |  15000.0000 |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |        512 | False |   455,913.89 us |  1,475.3928 us |  1,307.8973 us |   455,635.90 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |        512 | False | 4,450,135.62 us | 26,022.3168 us | 24,341.2913 us | 4,445,634.90 us |  15000.0000 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |        512 | False |   464,598.96 us |  5,387.9648 us |  5,039.9056 us |   466,320.00 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |        512 |  True | 1,307,276.38 us |  3,098.1044 us |  2,746.3888 us | 1,306,782.15 us |  11000.0000 |   1000.0000 |           - |          57621592 B |
|                   SyncFileStream |     Clr | 104857600 |        512 |  True |    81,196.59 us |    277.7045 us |    259.7649 us |    81,047.03 us |           - |           - |           - |              4681 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |        512 |  True | 1,317,803.18 us |  7,212.1920 us |  6,746.2889 us | 1,319,529.20 us |  11000.0000 |   1000.0000 |           - |          57592264 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |        512 |  True |    80,992.42 us |    293.6377 us |    260.3022 us |    80,895.54 us |           - |           - |           - |              4681 B |
|                  AsyncFileStream |    Core | 104857600 |        512 |  True |   446,397.13 us |  1,882.2521 us |  1,760.6598 us |   446,718.30 us |           - |           - |           - |           3694528 B |
|                   SyncFileStream |    Core | 104857600 |        512 |  True |    81,192.66 us |    351.7924 us |    329.0669 us |    81,125.36 us |           - |           - |           - |              4640 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |        512 |  True |   446,822.60 us |  3,359.1634 us |  3,142.1635 us |   446,028.50 us |           - |           - |           - |           3693760 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |        512 |  True |    81,623.18 us |    697.7131 us |    652.6413 us |    81,400.14 us |           - |           - |           - |              4640 B |
|                  AsyncFileStream |     Clr | 104857600 |       4096 | False |   727,767.76 us |  5,389.6308 us |  5,041.4640 us |   727,532.60 us |   2000.0000 |   1000.0000 |           - |           9945376 B |
|                   SyncFileStream |     Clr | 104857600 |       4096 | False |    75,881.98 us |    275.2010 us |    243.9585 us |    75,907.53 us |           - |           - |           - |              1170 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |       4096 | False |   723,927.68 us |  4,991.8922 us |  4,425.1824 us |   723,915.40 us |   2000.0000 |   1000.0000 |           - |           9978144 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |       4096 | False |    76,048.24 us |    232.9583 us |    206.5115 us |    76,011.56 us |           - |           - |           - |              1170 B |
|                  AsyncFileStream |    Core | 104857600 |       4096 | False |   579,925.33 us |  3,308.4365 us |  2,932.8428 us |   579,861.70 us |   1000.0000 |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |       4096 | False |    75,735.58 us |    214.3444 us |    178.9873 us |    75,709.60 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |       4096 | False |   578,686.84 us |  2,860.9269 us |  2,389.0035 us |   579,054.10 us |   1000.0000 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |       4096 | False |    75,948.54 us |    383.6058 us |    358.8251 us |    75,877.93 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |       4096 |  True |   724,242.26 us |  3,870.4348 us |  3,021.7809 us |   724,225.15 us |   2000.0000 |   1000.0000 |           - |           9994528 B |
|                   SyncFileStream |     Clr | 104857600 |       4096 |  True |    75,716.26 us |    224.2394 us |    187.2500 us |    75,696.20 us |           - |           - |           - |              1170 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |       4096 |  True |   737,364.04 us |  5,439.8737 us |  5,088.4612 us |   737,480.80 us |   2000.0000 |   1000.0000 |           - |           9978144 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |       4096 |  True |    76,655.84 us |    231.8670 us |    205.5440 us |    76,658.69 us |           - |           - |           - |              1170 B |
|                  AsyncFileStream |    Core | 104857600 |       4096 |  True |   573,560.62 us |  5,816.6137 us |  5,440.8641 us |   571,246.90 us |   1000.0000 |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |       4096 |  True |    76,453.17 us |    477.4542 us |    446.6109 us |    76,398.21 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |       4096 |  True |   587,062.63 us |  4,518.8644 us |  4,226.9485 us |   586,000.10 us |   1000.0000 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |       4096 |  True |    75,717.29 us |    271.2658 us |    240.4701 us |    75,644.99 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |      16384 | False |   199,239.10 us |    795.0835 us |    743.7216 us |   199,306.50 us |    333.3333 |           - |           - |           2504069 B |
|                   SyncFileStream |     Clr | 104857600 |      16384 | False |    30,095.66 us |     78.6950 us |     69.7610 us |    30,068.04 us |           - |           - |           - |               512 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |      16384 | False |   196,200.69 us |    442.1677 us |    391.9702 us |   196,037.70 us |    333.3333 |           - |           - |           2504069 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |      16384 | False |    30,340.36 us |    297.5267 us |    278.3067 us |    30,185.98 us |           - |           - |           - |               512 B |
|                  AsyncFileStream |    Core | 104857600 |      16384 | False |   159,922.42 us |  1,516.1470 us |  1,418.2048 us |   160,106.98 us |    250.0000 |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |      16384 | False |    30,180.38 us |     98.3657 us |     87.1986 us |    30,148.72 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |      16384 | False |   159,137.83 us |    717.3622 us |    671.0210 us |   159,403.40 us |    250.0000 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |      16384 | False |    30,263.93 us |    112.7526 us |    105.4688 us |    30,267.39 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |      16384 |  True |   198,021.51 us |  1,269.9713 us |  1,187.9319 us |   197,873.90 us |    333.3333 |           - |           - |           2504069 B |
|                   SyncFileStream |     Clr | 104857600 |      16384 |  True |    30,265.47 us |    178.2204 us |    157.9878 us |    30,220.76 us |           - |           - |           - |               512 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |      16384 |  True |   203,785.13 us |    529.7328 us |    413.5805 us |   203,780.72 us |    333.3333 |           - |           - |           2509531 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |      16384 |  True |    30,503.94 us |    297.6258 us |    278.3993 us |    30,554.73 us |           - |           - |           - |               512 B |
|                  AsyncFileStream |    Core | 104857600 |      16384 |  True |   160,953.59 us |  1,474.9005 us |  1,379.6228 us |   161,208.40 us |    250.0000 |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |      16384 |  True |    30,226.04 us |    124.8336 us |    116.7694 us |    30,218.09 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |      16384 |  True |   159,679.71 us |  1,629.2522 us |  1,524.0035 us |   159,372.38 us |    250.0000 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |      16384 |  True |    30,021.51 us |     85.3669 us |     79.8523 us |    30,024.28 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |      84975 | False |    51,757.45 us |    321.4764 us |    284.9805 us |    51,737.95 us |    100.0000 |           - |           - |            483342 B |
|                   SyncFileStream |     Clr | 104857600 |      84975 | False |    18,497.85 us |    205.6319 us |    182.2873 us |    18,545.69 us |           - |           - |           - |               512 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |      84975 | False |    51,121.14 us |    128.3501 us |    113.7790 us |    51,153.60 us |    100.0000 |           - |           - |            483342 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |      84975 | False |    18,149.23 us |    132.0703 us |    123.5387 us |    18,113.67 us |           - |           - |           - |               512 B |
|                  AsyncFileStream |    Core | 104857600 |      84975 | False |    42,696.88 us |    759.8701 us |    673.6051 us |    42,438.00 us |     83.3333 |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |      84975 | False |    18,161.36 us |     71.2822 us |     63.1898 us |    18,146.89 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |      84975 | False |    41,672.21 us |    224.1654 us |    209.6845 us |    41,651.68 us |     83.3333 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |      84975 | False |    18,230.49 us |     88.9093 us |     83.1658 us |    18,223.55 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |      84975 |  True |    51,433.35 us |    271.7881 us |    254.2308 us |    51,442.27 us |    100.0000 |           - |           - |            483342 B |
|                   SyncFileStream |     Clr | 104857600 |      84975 |  True |    18,289.38 us |    163.1430 us |    152.6040 us |    18,243.12 us |           - |           - |           - |               512 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |      84975 |  True |    51,824.53 us |    308.8006 us |    288.8523 us |    51,770.49 us |    100.0000 |           - |           - |            481704 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |      84975 |  True |    18,168.06 us |     72.6893 us |     67.9936 us |    18,155.41 us |           - |           - |           - |               512 B |
|                  AsyncFileStream |    Core | 104857600 |      84975 |  True |    41,680.35 us |    211.0633 us |    187.1021 us |    41,675.35 us |     83.3333 |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |      84975 |  True |    18,245.20 us |     94.9337 us |     84.1562 us |    18,243.28 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |      84975 |  True |    41,788.49 us |    313.9157 us |    293.6369 us |    41,737.52 us |     76.9231 |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |      84975 |  True |    18,171.04 us |     95.4997 us |     89.3305 us |    18,148.83 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |    1048576 | False |    20,785.51 us |    413.5539 us |  1,152.8231 us |    20,381.10 us |           - |           - |           - |             40192 B |
|                   SyncFileStream |     Clr | 104857600 |    1048576 | False |    15,267.18 us |     48.6267 us |     43.1063 us |    15,266.72 us |           - |           - |           - |               384 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |    1048576 | False |    20,923.25 us |    417.9825 us |  1,205.9752 us |    20,699.23 us |           - |           - |           - |             40192 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |    1048576 | False |    15,824.83 us |    301.0007 us |    309.1055 us |    16,003.76 us |           - |           - |           - |               384 B |
|                  AsyncFileStream |    Core | 104857600 |    1048576 | False |    17,676.05 us |    136.0771 us |    127.2866 us |    17,670.98 us |           - |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |    1048576 | False |    15,209.58 us |     40.8697 us |     36.2299 us |    15,203.86 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |    1048576 | False |    17,493.24 us |     79.9926 us |     70.9113 us |    17,499.19 us |           - |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |    1048576 | False |    15,388.26 us |     64.7338 us |     60.5520 us |    15,376.10 us |           - |           - |           - |               520 B |
|                  AsyncFileStream |     Clr | 104857600 |    1048576 |  True |    21,701.07 us |    423.6634 us |    550.8821 us |    21,745.71 us |           - |           - |           - |             40192 B |
|                   SyncFileStream |     Clr | 104857600 |    1048576 |  True |    15,380.15 us |     74.6257 us |     69.8049 us |    15,369.09 us |           - |           - |           - |               384 B |
| AsyncFileStreamSequentialOptions |     Clr | 104857600 |    1048576 |  True |    21,102.76 us |    435.6602 us |  1,284.5539 us |    20,483.34 us |           - |           - |           - |             40192 B |
|  SyncFileStreamSequentialOptions |     Clr | 104857600 |    1048576 |  True |    15,252.89 us |     53.5086 us |     50.0520 us |    15,252.18 us |           - |           - |           - |               384 B |
|                  AsyncFileStream |    Core | 104857600 |    1048576 |  True |    17,529.14 us |     99.9007 us |     93.4472 us |    17,532.21 us |           - |           - |           - |              1048 B |
|                   SyncFileStream |    Core | 104857600 |    1048576 |  True |    15,396.82 us |     50.6477 us |     44.8979 us |    15,389.92 us |           - |           - |           - |               520 B |
| AsyncFileStreamSequentialOptions |    Core | 104857600 |    1048576 |  True |    17,467.88 us |    104.8846 us |     98.1092 us |    17,507.31 us |           - |           - |           - |              1048 B |
|  SyncFileStreamSequentialOptions |    Core | 104857600 |    1048576 |  True |    15,354.01 us |     83.7713 us |     74.2611 us |    15,339.77 us |           - |           - |           - |               520 B |


// * Warnings *
MultimodalDistribution
  FileStreamBenchmark.AsyncFileStream: Clr                  -> It seems that the distribution can have several modes (mValue = 2.89)
  FileStreamBenchmark.AsyncFileStreamSequentialOptions: Clr -> It seems that the distribution is bimodal (mValue = 3.24)
  FileStreamBenchmark.AsyncFileStreamSequentialOptions: Clr -> It seems that the distribution can have several modes (mValue = 2.98)

// * Hints *
Outliers
  FileStreamBenchmark.AsyncFileStream: Clr                   -> 1 outlier  was  removed
  FileStreamBenchmark.SyncFileStreamSequentialOptions: Core  -> 2 outliers were removed
  FileStreamBenchmark.AsyncFileStream: Clr                   -> 3 outliers were removed
  FileStreamBenchmark.AsyncFileStream: Core                  -> 1 outlier  was  removed, 2 outliers were detected
  FileStreamBenchmark.AsyncFileStreamSequentialOptions: Core -> 1 outlier  was  detected
  FileStreamBenchmark.SyncFileStream: Clr                    -> 4 outliers were removed
  FileStreamBenchmark.SyncFileStreamSequentialOptions: Core  -> 2 outliers were detected
  FileStreamBenchmark.AsyncFileStream: Clr                   -> 3 outliers were detected
  FileStreamBenchmark.SyncFileStreamSequentialOptions: Core  -> 2 outliers were removed, 3 outliers were detected
  FileStreamBenchmark.AsyncFileStream: Clr                   -> 1 outlier  was  removed, 3 outliers were detected
