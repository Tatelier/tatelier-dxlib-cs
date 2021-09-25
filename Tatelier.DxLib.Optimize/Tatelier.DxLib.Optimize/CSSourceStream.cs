using System;
using System.IO;

namespace Tatelier.DxLib.Optimize
{
    class CSSourceStream : IDisposable
    {
        const string baseTopFilePath = "Tatelier.DxLib.Base.Top.txt";
        const string baseBottomFilePath = "Tatelier.DxLib.Base.Bottom.txt";

        bool disposed = false;

        StreamWriter sw;

        public int IndentCount { get; set; } = 2;

        public void WriteLine(string content)
        {
            sw.WriteLine($"{new string('\t', IndentCount)}{content}");
        }

        public void WriteLineConstInt(string name, object value)
        {
            WriteLine($"public const int {name} = {value};");
        }

        public CSSourceStream(string filePath)
        {

            sw = new StreamWriter(filePath);

            sw.Write(File.ReadAllText(baseTopFilePath));
            sw.Flush();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                sw.Write(File.ReadAllText(baseBottomFilePath));
                sw.Flush();
                disposed = true;
            }
        }
    }
}
