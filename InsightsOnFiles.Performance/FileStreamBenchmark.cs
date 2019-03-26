using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace InsightsOnFiles.Performance
{
    public class FileStreamBenchmark
    {
        private const string FileName = "test.blob";

        [Params(1024, 1024 * 64, 1024 * 1024, 1024 * 1024 * 100)]
        public int FileSize { get; set; }

        [Params(512, 4096, 1024 * 16, 84975, 1024 * 1024)] // last one is on LOH, second to last is the largest array on the SOH
        public int BufferSize { get; set; }

        [Params(true, false)]
        public bool UseStreamDefaultBufferSize { get; set; }

        private byte[] _buffer;
        private int _streamBufferSize;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var bytes = new byte[FileSize];
            new Random().NextBytes(bytes);
            File.WriteAllBytes(FileName, bytes);

            _buffer = new byte[BufferSize];
            _streamBufferSize = UseStreamDefaultBufferSize ? 4096 : BufferSize;
        }

        [Benchmark]
        public async Task<int> AsyncFileStream()
        {
            var totalNumberOfBytesRead = 0;
            var buffer = _buffer;
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None, _streamBufferSize, FileOptions.Asynchronous))
            {
                do
                {
#if NET
                    var numberOfBytesRead = await fileStream.ReadAsync(buffer, 0, BufferSize);
#else
                    var numberOfBytesRead = await fileStream.ReadAsync(buffer);
#endif
                    totalNumberOfBytesRead += numberOfBytesRead;
                } while (totalNumberOfBytesRead < FileSize);
            }

            return totalNumberOfBytesRead;
        }

        [Benchmark]
        public int SyncFileStream()
        {
            var totalNumberOfBytesRead = 0;
            var buffer = _buffer;
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None, _streamBufferSize, FileOptions.None))
            {
                do
                {
#if NET
                    var numberOfBytesRead = fileStream.Read(buffer, 0, BufferSize);
#else
                    var numberOfBytesRead = fileStream.Read(buffer);
#endif
                    totalNumberOfBytesRead += numberOfBytesRead;
                } while (totalNumberOfBytesRead < FileSize);
            }

            return totalNumberOfBytesRead;
        }

        [Benchmark]
        public async Task<int> AsyncFileStreamSequentialOptions()
        {
            var totalNumberOfBytesRead = 0;
            var buffer = _buffer;
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None, _streamBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                do
                {
#if NET
                    var numberOfBytesRead = await fileStream.ReadAsync(buffer, 0, BufferSize);
#else
                    var numberOfBytesRead = await fileStream.ReadAsync(buffer);
#endif
                    totalNumberOfBytesRead += numberOfBytesRead;
                } while (totalNumberOfBytesRead < FileSize);
            }

            return totalNumberOfBytesRead;
        }

        [Benchmark]
        public int SyncFileStreamSequentialOptions()
        {
            var totalNumberOfBytesRead = 0;
            var buffer = _buffer;
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None, _streamBufferSize, FileOptions.SequentialScan))
            {
                do
                {
#if NET
                    var numberOfBytesRead = fileStream.Read(buffer, 0, BufferSize);
#else
                    var numberOfBytesRead = fileStream.Read(buffer);
#endif
                    totalNumberOfBytesRead += numberOfBytesRead;
                } while (totalNumberOfBytesRead < FileSize);
            }

            return totalNumberOfBytesRead;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (File.Exists(FileName))
                File.Delete(FileName);
        }
    }
}