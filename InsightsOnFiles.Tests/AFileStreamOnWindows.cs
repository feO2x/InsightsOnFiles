using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace InsightsOnFiles.Tests
{
    public static class AFileStreamOnWindows
    {
        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(2000)]
        [InlineData(3000)]
        [InlineData(4000)]
        [InlineData(4010)]
        [InlineData(4050)]
        [InlineData(4095)]
        [InlineData(4096)]
        [InlineData(4250)]
        [InlineData(4500)]
        [InlineData(4750)]
        [InlineData(5000)]
        [InlineData(6000)]
        [InlineData(7000)]
        [InlineData(8000)]
        [InlineData(9000)]
        [InlineData(20_000)]
        [InlineData(50_000)]
        [InlineData(63_999)]
        [InlineData(64_000)]
        [InlineData(80_000)]
        public static async Task CompletesSynchronouslyWhenTheInternalBufferSizeIsLargerThanTheFile(int fileSize)
        {
            var fileName = $"Test file with {fileSize} bytes.blob";
            try
            {
                var writeBuffer = new byte[fileSize];
                for (var i = 0; i < fileSize; ++i)
                {
                    writeBuffer[i] = (byte) (i % byte.MaxValue);
                }

                using (var writeStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
                    await writeStream.WriteAsync(writeBuffer);

                var readBuffer = new byte[fileSize];
                const int largeStreamBufferSize = 84075; // This is the largest array size that will fit on the SOH
                using (var readStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, largeStreamBufferSize, FileOptions.Asynchronous))
                {
                    var task = readStream.ReadAsync(readBuffer, 0, fileSize);
                    task.IsCompleted.Should().BeTrue();
                    task.Result.Should().Be(fileSize);
                    readBuffer.Should().Equal(writeBuffer);
                }
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }
    }
}