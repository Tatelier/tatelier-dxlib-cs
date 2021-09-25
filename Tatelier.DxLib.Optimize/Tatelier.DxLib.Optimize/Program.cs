using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tatelier.DxLib.Optimize
{

    class Program
    {
        const string outputFolderPath = "Output";
        const string dxlibHeaderFilePath = "DxLib.h";

        static readonly string outputConstFilePath = $"{outputFolderPath}/Tatelier.DxLib.Const.cs";
        static readonly string outputStructManualFilePath = $"{outputFolderPath}/Tatelier.DxLib.Struct.Manual.cs";
        static readonly string outputStructAutoFilePath = $"{outputFolderPath}/Tatelier.DxLib.Struct.Auto.cs";

        // 定数関連
        static readonly IReadOnlyList<string> excludeConstList = new string[]
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
        static void WriteConst(StreamReader source, CSSourceStream a)
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

        // 構造体関連
        static bool CheckExcludeStruct(StringBuilder sb)
        {
            string a = sb.ToString();

            if (a.Contains("["))
            {
                return true;
            }

            if(a.Contains("MV1_REF_POLYGONLIST"))
            {
                return true;
            }

            if (a.Contains("STREAMDATASHRED"))
            {
                return true;
            }

            if (a.Contains("union"))
            {
                return true;
            }

            return false;
        }
        static List<(string, string, int)> cppToCSMap = new List<(string, string, int)>()
        {
            ("int", "int", 4),
            ("float", "float", 4),
            ("double", "double", 8),

            ("unsigned char", "byte", 1),
            ("unsigned int", "uint", 4),
            ("BYTE", "byte", 1),
            ("WORD", "ushort", 2),
            ("DWORD", "uint", 4),
            ("TCHAR", "byte", 1),

            ("void*", "uint", 4),
            ("const TCHAR*", "uint", 4),
            ("const TCHAR**", "uint", 4),
            ("const char*", "uint", 4),
            ("const IMEINPUTCLAUSEDATA*", "uint", 4),

            ("VECTOR", "VECTOR", 12),
            ("VECTOR_D", "VECTOR_D", 24),
            ("COLOR_U8", "COLOR_U8", 4),
            ("COLOR_F", "COLOR_F", 16),
            ("FLOAT4", "FLOAT4", 16),
            ("MV1_COLL_RESULT_POLY*", "uint", 4),

            ("COLORDATA", "COLORDATA", 1064),

        };
        static bool TryGetTypeForCS(string inputType, out string type, out int size)
        {
            var item = cppToCSMap.FirstOrDefault(v => v.Item1 == inputType);

            if (item != default)
            {
                type = item.Item2;
                size = item.Item3;

                return true;
            }
            else
            {
                type = null;
                size = -1;

                return false;
            }
        }

        static void WriteStructOnce(StringBuilder sb, CSSourceStream a)
        {
            if (CheckExcludeStruct(sb))
            {
                return;
            }

            var ta = new TextAnalyzer();
            ta.inputText = sb;

            int fieldOffset = 0;
            StringBuilder type = new StringBuilder();
            StringBuilder name = new StringBuilder();

            string res;


            res = ta.GetStr();

            if ("typedef" == res)
            {
                res = ta.GetStr();
                if ("struct" == res)
                {
                    if (ta.GetStr() == "{")
                    {
                    }
                    else if (ta.GetStr() == "{")
                    {

                    }

                    string tmp = "";
                    List<string> contentList = new List<string>();
                    while (tmp != "}")
                    {
                        tmp = ta.GetStr();

                        if ("}" == tmp)
                        {
                            break;
                        }

                        type.Append(tmp);

                        if ("const" == tmp)
                        {
                            tmp = ta.GetStr();
                            type.Append(" ");
                            type.Append(tmp);
                        }

                        if ("unsigned" == tmp)
                        {
                            tmp = ta.GetStr();
                            type.Append(" ");
                            type.Append(tmp);
                        }

                        tmp = ta.GetStr();
                        if (tmp == "*")
                        {
                            type.Append(tmp);

                            tmp = ta.GetStr();
                            while (tmp == "*")
                            {
                                type.Append(tmp);

                                tmp = ta.GetStr();
                            }
                        }


                        name.Append(tmp);

                        tmp = ta.GetStr();

                        string typeCS;
                        int size;

                        while (tmp == ",")
                        {
                            if (!TryGetTypeForCS($"{type}", out typeCS, out size))
                            {
                                throw new Exception("型変換に失敗しました。");
                            }

                            //contentList.Add($"[FieldOffset({fieldOffset})]");
                            //contentList.Add($"public {typeCS} {name};");
                            contentList.Add($"[FieldOffset({fieldOffset})] public {typeCS} {name};");
                            fieldOffset += size;

                            name.Clear();

                            tmp = ta.GetStr();

                            name.Append(tmp);

                            tmp = ta.GetStr();
                        }

                        if (!TryGetTypeForCS($"{type}", out typeCS, out size))
                        {
                            throw new Exception("型変換に失敗しました。");
                        }

                        //contentList.Add($"[FieldOffset({fieldOffset})]");
                        //contentList.Add($"public {typeCS} {name};");
                        contentList.Add($"[FieldOffset({fieldOffset})] public {typeCS} {name};");
                        fieldOffset += size;

                        type.Clear();
                        name.Clear();
                    }
                    contentList.RemoveAt(contentList.Count - 1);
                    var structName = ta.GetStr();

                    a.WriteLine("[StructLayout(LayoutKind.Explicit)]");
                    a.WriteLine($"public struct {structName}");
                    a.WriteLine("{");
                    a.IndentCount++;
                    foreach (var item in contentList)
                    {
                        a.WriteLine(item);
                    }
                    a.IndentCount--;
                    a.WriteLine("}");
                    a.WriteLine("");
                }
            }


        }
        static void WriteStruct(StreamReader source, CSSourceStream a)
        {
            var sb = new StringBuilder();

            while (!source.EndOfStream)
            {
                string line = source.ReadLine();

                if (line.Contains("DX_STRUCT_END"))
                {
                    break;
                }

                var textAnalyzer = new TextAnalyzer();

                if (line.Contains("typedef struct"))
                {
                    sb.AppendLine(line);

                    while (!source.EndOfStream)
                    {
                        var line2 = source.ReadLine();

                        int commentStartIndex = line2.IndexOf("//");
                        if (commentStartIndex > 0)
                        {
                            line2 = line2.Substring(0, commentStartIndex);
                        }

                        sb.AppendLine(line2);

                        string line3 = line2.Replace("\t", "").Replace(" ", "");
                        if (line3.StartsWith("}"))
                        {
                            break;
                        }
                    }
                    WriteStructOnce(sb, a);
                    sb.Clear();
                    continue;
                }
            }
        }


        public static void Main(string[] args)
        {
            if (Directory.Exists(outputFolderPath))
            {
                Directory.Delete(outputFolderPath, true);
            }
            Directory.CreateDirectory(outputFolderPath);

            var sr = new StreamReader(dxlibHeaderFilePath);

            // 定数
            using (var a = new CSSourceStream(outputConstFilePath))
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

            // 手動で入力する構造体
            using (var a = new CSSourceStream(outputStructManualFilePath))
            {
                const string manualFilePath = "Tatelier.DxLib.Struct.Manual.txt";

                // 手動入力部を書き込み
                foreach(var l in File.ReadAllLines(manualFilePath))
                {
                    a.WriteLine(l);
                }
            }

            // 構造体
            using (var a = new CSSourceStream(outputStructAutoFilePath))
            {
                sr.BaseStream.Seek(0, SeekOrigin.Begin);

                var sb = new StringBuilder();

                while(!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    if (line.Contains("DX_STRUCT_START"))
                    {
                        WriteStruct(sr, a);
                    }
                }

            }
        }
    }
}
