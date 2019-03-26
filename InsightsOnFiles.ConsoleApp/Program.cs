using System;

namespace InsightsOnFiles.ConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            Console.ReadLine();
            var byteArray = new byte[84_975];
            for (var i = 0; i < byteArray.Length; ++i)
            {
                byteArray[i] = (byte) (i % byte.MaxValue);
            }

            Console.ReadLine();
        }
    }
}