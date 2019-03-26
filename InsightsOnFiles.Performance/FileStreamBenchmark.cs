using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace InsightsOnFiles.Performance
{
    public class FileStreamBenchmark
    {
        private const string FileName = "test.blob";

        [Params(100, 512, 1024, 2048, 4096, 8192, 1024 * 16, 1024 * 32, 1024 * 64, 1024 * 128, 1024 * 512, 1024 * 1024, 1024 * 1024 * 10)]
        public int FileSize { get; set; }

        [Params(512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 84975)]
        public int BufferSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var bytes = new byte[FileSize];
            for (var i = 0; i < FileSize; ++i)
            {
                bytes[i] = (byte) (i % byte.MaxValue);
            }

            File.WriteAllBytes(FileName, bytes);
        }

        [Benchmark]
        public async Task<int> AsyncFileStream()
        {
            var totalNumberOfBytesRead = 0;
            var buffer = new byte[BufferSize];
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None, BufferSize, FileOptions.Asynchronous))
            {
                do
                {
                    var numberOfBytesRead = await fileStream.ReadAsync(buffer, 0, BufferSize);
                    totalNumberOfBytesRead += numberOfBytesRead;
                } while (totalNumberOfBytesRead < FileSize);
            }

            return totalNumberOfBytesRead;
        }

        [Benchmark]
        public int SyncFileStream()
        {
            var totalNumberOfBytesRead = 0;
            var buffer = new byte[BufferSize];
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None, BufferSize, FileOptions.None))
            {
                do
                {
                    var numberOfBytesRead = fileStream.Read(buffer, 0, BufferSize);
                    totalNumberOfBytesRead += numberOfBytesRead;
                } while (totalNumberOfBytesRead < FileSize);
            }

            return totalNumberOfBytesRead;
        }

        [Benchmark]
        public async Task<int> AsyncFileStreamSequentialOptions()
        {
            var totalNumberOfBytesRead = 0;
            var buffer = new byte[BufferSize];
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                do
                {
                    var numberOfBytesRead = await fileStream.ReadAsync(buffer, 0, BufferSize);
                    totalNumberOfBytesRead += numberOfBytesRead;
                } while (totalNumberOfBytesRead < FileSize);
            }

            return totalNumberOfBytesRead;
        }

        [Benchmark]
        public int SyncFileStreamSequentialOptions()
        {
            var totalNumberOfBytesRead = 0;
            var buffer = new byte[BufferSize];
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None, BufferSize, FileOptions.SequentialScan))
            {
                do
                {
                    var numberOfBytesRead = fileStream.Read(buffer, 0, BufferSize);
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