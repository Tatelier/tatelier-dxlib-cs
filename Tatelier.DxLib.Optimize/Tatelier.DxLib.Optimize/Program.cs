using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tatelier.DxLib.Optimize
{
    class A : IDisposable
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

        public A(string filePath)
        {
            var outputFolderPath = Path.GetDirectoryName(filePath);
            if (Directory.Exists(outputFolderPath))
            {
                Directory.Delete(outputFolderPath, true);
            }
            Directory.CreateDirectory(outputFolderPath);

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


    class MainClass
    {
        static string[] excludeConstList = new string[]
        {
            "STTELL(", "STSEEK(", "STREAD(", "STWRITE(", "STEOF(", "STCLOSE(",
            "STREAM_SEEKTYPE_SET", "STREAM_SEEKTYPE_END", "STREAM_SEEKTYPE_CUR",
            "DEFAULT_FOV", "DEFAULT_TAN_FOV_HALF", "DEFAULT_NEAR", "DEFAULT_FAR",
            "DEFAULT_FONT_SIZE", "DEFAULT_FONT_THINCK", "DEFAULT_FONT_TYPE", "DEFAULT_FONT_EDGESIZE",
        };

        static bool CheckExcludeConst(StringBuilder sb)
        {
            string text = $"{sb}";

            return excludeConstList.Any(v => v == text);
        }

        static void WriteConst(StreamReader source, A a)
        {
            a.WriteLineConstInt("TRUE", 1);
            a.WriteLineConstInt("FALSE", 0);


            StringBuilder name = new StringBuilder(256);
            StringBuilder value = new StringBuilder(256);

            var defineReplace = new Regex("#define\\s");

            while (!source.EndOfStream)
            {
                string line = source.ReadLine();

                if (line.Contains("DX_DEFINE_END"))
                {
                    break;
                }

                name.Clear();
                value.Clear();

                if (!line.StartsWith("#define"))
                {
                    continue;
                }

                line = defineReplace.Replace(line, "", 1);

                int i = 0;

                // 定義名を取得
                for (; i < line.Length; i++)
                {
                    if (line[i] == ' '
                        || line[i] == '\t')
                    {
                        break;
                    }
                    name.Append(line[i]);
                }

                if (CheckExcludeConst(name))
                {
                    continue;
                }

                // 定義名と値の間にあるスペースを無視
                for (; i < line.Length; i++)
                {
                    if (line[i] != ' '
                        && line[i] != '\t')
                    {
                        break;
                    }
                }

                // 値を取得
                for (; i < line.Length; i++)
                {
                    if(line[i]=='/'
                        && (i+1 < line.Length)
                        && line[i + 1] == '/')
                    {
                        break;
                    }
                    if (line[i] == '\t')
                    {
                        break;
                    }
                    value.Append(line[i]);
                }

                a.WriteLineConstInt($"{name}", $"{value}");
            }
        }
        public static void Main(string[] args)
        {
            string dxlibHeaderFilePath = "DxLib.h";

            var sr = new StreamReader(dxlibHeaderFilePath);

            using (var a = new A("Output/Tatelier.DxLib.Const.cs"))
            {
                while(!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    if (line.Contains("DX_DEFINE_START"))
                    {
                        WriteConst(sr, a);
                    }
                }
            }
        }
    }
}
