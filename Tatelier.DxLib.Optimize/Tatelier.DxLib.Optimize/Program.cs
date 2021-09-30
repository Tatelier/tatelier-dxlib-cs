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
        const string dxlibFuncWinHeaderFilePath = "DxFunctionWin.h";
        const string dllNameVariableValue = "DxLibDLLFileName";

        static readonly string outputExtensionsFilePath = $"{outputFolderPath}/Tatelier.DxLib.Extensions.cs";
        static readonly string outputConstFilePath = $"{outputFolderPath}/Tatelier.DxLib.Const.cs";
        static readonly string outputStructManualFilePath = $"{outputFolderPath}/Tatelier.DxLib.Struct.Manual.cs";
        static readonly string outputStructAutoFilePath = $"{outputFolderPath}/Tatelier.DxLib.Struct.Auto.cs";
        static readonly string outputFunctionManualFilePath = $"{outputFolderPath}/Tatelier.DxLib.Function.Manual.cs";
        static readonly string outputFunctionAutoFilePath = $"{outputFolderPath}/Tatelier.DxLib.Function.Auto.cs";

        static List<(string, string, int)> cppToCSMap = new List<(string, string, int)>()
        {
            ("int", "int", 4),
            ("float", "float", 4),
            ("double", "double", 8),
            ("unsigned char", "byte", 1),
            ("unsigned int", "uint", 4),

            ("size_t", "ulong", 8),

            ("TCHAR", "byte", 1),
            ("BYTE", "byte", 1),
            ("LONGLONG", "long", 8),
            ("ULONGLONG", "ulong", 8),
            ("WORD", "ushort", 2),
            ("DWORD", "uint", 4),
            ("BOOL", "int", 4),
            ("VERTEX", null, 12),
            ("VECTOR", null, 12),
            ("VECTOR_D", null, 24),
            ("COLOR_U8", null, 4),
            ("COLOR_F", null, 16),
            ("FLOAT4", null, 16),
            ("INT4", null, 16),

            ("void*", "System.IntPtr", 4),
            ("int*", "out int", 4),
            ("float*", "out float", 4),
            ("double*", "out double", 4),
            ("TOUCHINPUTDATA*", "out TOUCHINPUTDATA", 4),
            ("TOUCHINPUTPOINT*", "out TOUCHINPUTPOINT", 4),
            ("IPDATA*", "out IPDATA", 4),
            ("IPDATA_IPv6*", "out IPDATA_IPv6", 4),
            ("DINPUT_JOYSTATE*", "out DINPUT_JOYSTATE", 4),
            ("XINPUT_STATE*", "out XINPUT_STATE", 4),
            ("LONGLONG*", "out long", 8),
            ("ULONGLONG*", "out ulong", 8),

            ("DWORD_PTR", "uint", 4),

            ("TCHAR*", "System.Text.StringBuilder", 8),
            ("BYTE*", "out byte", 0),

            ("const void*", "System.IntPtr", 8),
            ("const TCHAR*", "string", 4),
            ("const TCHAR**", "uint", 4),
            ("const char*", "uint", 4),
            ("const IMEINPUTCLAUSEDATA*", "uint", 4),
            ("const COLORDATA*", "uint", 4),
            ("const IMAGEFORMATDESC*", "out IMAGEFORMATDESC", 0),
            ("MV1_COLL_RESULT_POLY*", "uint", 4),

            ("const BOOL*", "out int", 4),
            ("const unsigned short*", "out ushort", 8),

            ("DX_CHAR*", "out byte", 8),

            ("HANDLE", "System.IntPtr", 8),
            ("HWND", "System.IntPtr", 8),
            ("HRGN", "System.IntPtr", 8), // 不明
            ("HICON", "System.IntPtr", 8),
            ("HINSTANCE", "System.IntPtr", 8),

            ("DATEDATA*", "out DATEDATA", 8),

            ("COLORDATA", null, 1064),

        };

        static readonly IReadOnlyList<string> csKeywordList = new string[]
        {
            "sbyte",
            "short",
            "int",
            "long",
            "byte",
            "ushort",
            "uint",
            "ulong",
        };

        // 定数関連
        static readonly IReadOnlyList<string> excludeConstList = new string[]
        {
            "STTELL(", "STSEEK(", "STREAD(", "STWRITE(", "STEOF(", "STCLOSE(",
            "STREAM_SEEKTYPE_SET", "STREAM_SEEKTYPE_END", "STREAM_SEEKTYPE_CUR",
            "DEFAULT_FOV", "DEFAULT_TAN_FOV_HALF", "DEFAULT_NEAR", "DEFAULT_FAR",
            "DEFAULT_FONT_SIZE", "DEFAULT_FONT_THINCK", "DEFAULT_FONT_TYPE", "DEFAULT_FONT_EDGESIZE",
        };

        static readonly IReadOnlyList<string> excludeFunctionList = new string[]
        {
            "GraphFilter", "GraphFilterBlt", "GraphFilterRectBlt",
            "GraphBlend", "GraphBlendBlt", "GraphBlendRectBlt",
            "MV1SetMaterialTypeParamAll", "MV1SetMaterialTypeParam",
            "sprintfDx", "snprintfDx", "sscanfDx",
            "SetBlendGraphParam", "MailApp_Send", "MailApp_SendWithStrLen",
            "SetBeepFrequency", "PlayBeep", "StopBeep",
            "ClearDrawScreen", "ClearDrawScreenZBuffer",
            "GetTexPixelFormat", "GetTexColorData", "LoadGraphToResource", "GetWindowSizeChangeEnableFlag",
            "DrawChipMap", "BltBaseImage", "CreateGraphFromGraphImage",
            "ReCreateGraphFromGraphImage", "CreateDivGraphFromGraphImage", "ReCreateDivGraphFromGraphImage",
            "MemStreamOpen", "MemStreamClose", "vsprintfDx", "vsnprintfDx", "vsscanfDx",
            "GetDrawTargetSurface", "GetPrimarySurface", "GetBackSurface",
            "GetWorkSurface", "GetUseDDrawObj", "GetPixelFormat",
            "GetOverlayPixelFormat", "GetDirectDrawCaps", "GetDrawScreenDC",
            "GetDrawStringCharInfo", "GetDrawExtendStringCharInfo",
            "GetDrawStringCharInfoToHandle", "GetDrawExtendStringCharInfoToHandle",
            "GetDrawNStringCharInfo", "GetDrawExtendNStringCharInfo",
            "GetDrawNStringCharInfoToHandle", "GetDrawExtendNStringCharInfoToHandle",
            "GetDrawFormatStringCharInfo", "GetDrawExtendFormatStringCharInfo", "GetDrawFormatStringCharInfoToHandle", "GetDrawExtendFormatStringCharInfoToHandle",
            "GetDirectDrawDeviceGUID", "GetUseD3DDevObj", "GetVertexBuffer",
            "GetTexPixelFormat", "GetTexColorData", "GetTexPixelFormat",
            "GetTexColorData", "GetTexPixelFormat", "GetTexColorData",
            "GetZBufferPixelFormat", "GraphColorMatchBltVer2", "GetFullColorImage",
            "GetResourceIDString", "CreateDIBGraphVer2", "SetHookWinProc",
            "FileRead_getInfo", "FileRead_findFirst", "FileRead_findFirst_WithStrLen", "FileRead_findNext", "FileRead_findClose",
            "SetKeyInputStringColor", "Paint",
            
            "SetActiveStateChangeCallBackFunction", "SetUseASyncChangeWindowModeFunction", 
            "SetMenuItemSelectCallBackFunction", "SetWindowMenu", "SetRestoreShredPoint", 
            "SetRestoreGraphCallback", 

            "SetGraphicsDeviceRestoreCallbackFunction", "SetGraphicsDeviceLostCallbackFunction",
            "AddUserGraphLoadFunction4", "SubUserGraphLoadFunction4",
        };

        static readonly IReadOnlyList<string> excludeVariableTypeList = new string[]
        {
            "HMODULE",
            "HDC",
            "STREAMDATASHREDTYPE2",
            "BASEIMAGE",
            "GUID",
            "HBITMAP",
            "BITMAPINFO",
            "WAVEFORMATEX",
            "STREAMDATA",
            "void**",
            "IMEINPUTDATA",
        };

        static bool CheckExcludeConst(StringBuilder sb)
        {
            string text = $"{sb}";

            return excludeConstList.Any(v => v == text);
        }

        static string GetConvertHexToDec(StringBuilder sb)
		{
            var textAnalyzer = new TextAnalyzer();
            textAnalyzer.splitIgnoreCharList = new char[]
            {
            };
            textAnalyzer.splitCharList2 = new char[]
            {
                ' ',
                '\t',
                '\r',
                '\n',
                '*',
                '-',
                '+',
                '/',
                '%',
                '(',
                ')',
                ',',
            };
            textAnalyzer.inputText = sb;

            var outputSB = new StringBuilder();


            var iterator = textAnalyzer.EnumerateStr();

            foreach(var item in iterator)
			{
                if (item.StartsWith("0x"))
                {
                    outputSB.Append($"unchecked((int){item})");
                }
                else
                {
                    outputSB.Append(item);
                }
			}

            return $"{outputSB}";
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

                var valueStr = $"{value}";
                valueStr = GetConvertHexToDec(value);

                a.WriteLineConstInt($"{name}", $"{valueStr}");
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
        static bool TryGetTypeForCS(string inputType, bool isReturnType, out string type, out int size)
        {
            var item = cppToCSMap.FirstOrDefault(v => v.Item1 == inputType);

            if (item != default)
            {
                if (item.Item2 != null)
                {
                    type = item.Item2;
                }
				else
				{
                    type = inputType;
				}
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
                            if (!TryGetTypeForCS($"{type}", false, out typeCS, out size))
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

                        if (!TryGetTypeForCS($"{type}", false, out typeCS, out size))
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
                    try
                    {
                        WriteStructOnce(sb, a);
                    }
                    catch(Exception e)
					{
                        Console.WriteLine($"{e}");
					}

                    sb.Clear();
                    continue;
                }
            }
        }

        static string GetTypeString(string[] typeStringArray, int startIndex, int endIndex)
        {
            string tt = "";
            for (int k = startIndex; k <= endIndex; k++)
            {
                if (typeStringArray[k] == "*")
                {
                    tt = tt.Remove(tt.Length - 1);
                }
                tt += typeStringArray[k] + " ";
            }
            if (tt.Length > 0)
            {
                tt = tt.Remove(tt.Length - 1);
            }

            return tt;
        }

        static bool TryGetVariable(string[] array, int startIndex, int endIndex, out string variableType, out string variableName, out string variableDefaultValue)
        {
            if(startIndex >= endIndex)
            {
                variableType = null;
                variableName = null;
                variableDefaultValue = null;
                return false;
            }

            int substitutionIndex = Array.IndexOf(array, "=", startIndex, endIndex - startIndex);

            if (substitutionIndex == -1)
            {
                variableType = GetTypeString(array, startIndex, endIndex - 1);
                variableName = array[endIndex];
                variableDefaultValue = null;

                return true;
            }
            else
            {
                variableType = GetTypeString(array, startIndex, substitutionIndex - 2);
                variableName = array[substitutionIndex - 1];

                variableDefaultValue = "";
                for(int i= substitutionIndex + 1; i <= endIndex; i++)
                {
                    variableDefaultValue += array[i];
                }

                return true;
            }
        }

        static void WriteFunction(StreamReader source, CSSourceStream a)
        {
            var f = new Function();

            var csv = new CSV("TypeConvert.csv");
            bool isNowComment = false;
            while (!source.EndOfStream)
            {
                bool ignore = false;
                var line = source.ReadLine();

                if (line.Contains("DX_FUNCTION_END"))
                {
                    break;
                }

                // (/*)コメント内の場合、(*/)を見つけるまでカット
                if (isNowComment)
                {
                    int endCommendIndex = line.IndexOf("*/");
                    if (endCommendIndex != -1)
                    {
                        line = line.Substring(endCommendIndex + 2);
                        isNowComment = false;
                    }
                    else
                    {
                        Console.WriteLine($"now comment. [{line}]");
                        continue;
                    }
                }

                // 空行は何もしない
                if(line.Length == 0)
                {
                    Console.WriteLine($"empty line. [{line}]");
                    continue;
                }

                // Win版以外は無視
                if (line.Contains("#if defined( __APPLE__ ) || defined( __ANDROID__ )"))
                {
                    while (!source.EndOfStream)
                    {
                        line = source.ReadLine();

                        Console.WriteLine(line);
                        if (line.Contains("#endif // defined( __APPLE__ ) || defined( __ANDROID__ )"))
                        {
                            break;
                        }
                    }
                    line = source.ReadLine();
                }

                // コメント(//)以降を無視
                int commentIndex = line.IndexOf("//");

                if (commentIndex != -1)
                {
                    if (commentIndex == 0)
                    {
                        Console.WriteLine($"now comment. [{line}]");
                        continue;
                    }
                    line = line.Substring(0, commentIndex);
                }


                // コメント(/*)は、(*/)が現れるまでその分を無視
                commentIndex = line.IndexOf("/*");

                if (commentIndex != -1)
                {
                    int endCommentIndex = line.Substring(commentIndex + 2).IndexOf("*/");

					if (endCommentIndex != -1)
					{
                        endCommentIndex += commentIndex + 2;
                        line = line.Substring(0, commentIndex) + line.Substring(endCommentIndex + 2);
					}
                    else
					{
                        line = line.Substring(0, commentIndex);
                        isNowComment = true;
					}
                }

                // 外部公開されていない関数は処理しない
                if (!line.Contains("extern"))
                {
                    Console.WriteLine($"not extern. [{line}]");
                    continue;
                }

                var textAnalyzer = new TextAnalyzer();
                textAnalyzer.SetText(line);

                var array = textAnalyzer.ToArrayStr();

                f.Clear();

                if (array.Any(v => v == "va_list"))
                {
                    Console.WriteLine($"va_list is ignore target. [{line}]");
                    continue;
                }

                if(array[0] == "extern")
                {
                    int argOpenParenthesisIndex = Array.IndexOf(array, "(");
                    
                    // 
                    if (argOpenParenthesisIndex == -1)
                    {
                        Console.WriteLine($"not found open parenthesis [{line}]");
                        continue;
                    }

                    //　
                    if (excludeFunctionList.Any(v => v == array[argOpenParenthesisIndex - 1]))
                    {
                        continue;
                    }

                    f.Name = array[argOpenParenthesisIndex - 1];

                    string tt = GetTypeString(array, 1, argOpenParenthesisIndex - 2);


                    if (excludeVariableTypeList.Any(v => tt.Contains(v)))
                    {
                        Console.WriteLine($"return type exclude. [{line}]");
                        goto IgnoreLine;
                    }

                    f.ReturnType = TryGetTypeForCS(tt, true, out var t, out _) ? t : tt;

                    int argCloseParenthesisIndex = Array.LastIndexOf(array, ")");

                    int argOnceStartIndex = argOpenParenthesisIndex + 1;
                    int argOnceEndIndex;

                    // 仮引数セット
                    for (int i = argOpenParenthesisIndex + 1; i <= argCloseParenthesisIndex; i++)
                    {
                        if (array[i] == ","
                            || array[i] == ")")
                        {
                            argOnceEndIndex = i - 1;

                            if(argOnceStartIndex == argOnceEndIndex)
                            {
                                Console.WriteLine("not variable.");
                                break;
                            }

                            if(!TryGetVariable(array, argOnceStartIndex, argOnceEndIndex, out var vType, out var vName, out var vDefaultValue))
                            {
                                Console.WriteLine($"variable error. [{line}]");
                                goto IgnoreLine;
                            }

                            argOnceStartIndex = i + 1;

                            if(excludeVariableTypeList.Any(v => vType.Contains(v)))
                            {
                                Console.WriteLine($"variable exclude. [{line}]");
                                goto IgnoreLine;
                            }

                            if(TryGetTypeForCS(vType, false, out var vcsType, out int size))
                            {

                            }
                            else
                            {
                                if (vType.EndsWith("*"))
                                {
                                    vcsType = $"out {vType.Replace("*", "").Replace("const ", "")}";
                                }
                                else
                                {
                                    vcsType = $"{vType.Replace("const ", "")}";
                                }
                            }

                            if (!(csKeywordList.Any(v=>vcsType.Contains(v)))
                                && vDefaultValue?.Length > 0)
                            {
                                vDefaultValue = "default";
                            }

                            // 配列の先頭ポインタの場合は、型を変更する
                            if (vcsType.StartsWith("out")
                                && vName.EndsWith("Array"))
                            {
                                var split = vcsType.Split(' ');

                                vcsType = $"[In, Out] {string.Join(" ", split.Skip(1))}[]";
                                vDefaultValue = null;
                            }

                            if(vDefaultValue == "NULL")
                            {
                                vDefaultValue = "0";
                            }

                            if (vName?.Length > 0
                               && vType?.Length > 0)
                            {
                                f.ParameterList.Add(new Parameter()
                                {
                                    Name = vName,
                                    Type = vcsType,
                                    defaultValue = vDefaultValue,
                                });
                            }
                            else
                            {

                            }
                        }
                    }

                    a.WriteLine($"[DllImport({dllNameVariableValue}, EntryPoint=\"dx_{f.Name}\", CallingConvention=CallingConvention.StdCall)]");
                    a.WriteLine($"{f.GetString($"extern {(f.IsUnsafe ? "unsafe " : "")}static ", "dx_", true)};");
                    a.WriteLine($"{f.GetString($"public {(f.IsUnsafe ? "unsafe " : "")}static ")} => dx_{f.Name}({f.GetParameterString(true, true)});");
                    a.WriteLine("");

                IgnoreLine:;
                }

#if false
                if((array[0] == "extern"
                    && array[3] == "(")
                    || (array[0] == "extern"
                    && array[1] == "unsigned"
                    && array[4] == "(")
                    || (array[0] == "extern"
                    && array[1] == "const"
                    && array[5] == "("))
                {
                    int IndexName = 2;
                    int IndexArgsFirst = 4;

                    if (array[4] == "(")
                    {
                        IndexName++;
                        IndexArgsFirst++;
                    }
                    else if (array[5] == "(")
                    {
                        IndexName+=2;
                        IndexArgsFirst+=2;
                    }

                    if (excludeFunctionList.Any(v => v == array[IndexName]))
                    {
                        continue;
                    }
                    int bracketCount = 1;
                    for (int i = IndexArgsFirst; i < array.Length; i++)
                    {
                        if(array[i] == "(")
						{
                            bracketCount++;
						}

                        if(array[i] == ")")
                        {
                            bracketCount--;

                            // カッコが閉じられたらそこまでを仮引数として処理する
							if (bracketCount == 0)
                            {
                                for (int j = IndexArgsFirst; j < i; j++)
                                {
                                    string type = array[j];

                                    // "," の場合はその次の文字列を型で取得する。
                                    if (type == ",")
                                    {
                                        j++;
                                        type = array[j];
                                    }


                                    if (type == "const")
                                    {
                                        if (j + 1 < array.Length)
                                        {
                                            j++;
                                            type += " " + array[j];
                                        }
                                    }

                                    if (type.EndsWith("unsigned"))
                                    {
                                        if (j + 1 < array.Length)
                                        {
                                            j++;
                                            type += (" " + array[j]);
                                        }
                                    }


                                    while (j + 1 < array.Length
                                        && array[j + 1] == "*")
                                    {
                                        j++;
                                        type += "*";
                                    }

                                    string name = null;

                                    string dv = null;

                                    if (j + 1 < array.Length)
                                    {
                                        j++;
                                        if (array[j] != ")")
                                        {
                                            name = array[j];
                                            if (j + 1 < array.Length)
                                            {
                                                if (array[j + 1] == "=")
                                                {
                                                    j += 2;

                                                    // C# に NULLはないため、0に変換する
                                                    if (array[j] == "NULL")
                                                    {
                                                        dv = "0";
                                                    }
                                                    else if (array[j] == "-")
                                                    {
                                                        dv = $"{array[j]}";
                                                        j++;
                                                        dv += $"{array[j]}";
                                                    }
                                                    else
                                                    {
                                                        dv = $"{array[j]}";
                                                    }

                                                    while (array[j + 1] != ","
                                                        && array[j + 1] != ")")
                                                    {
                                                        if (array[j + 1] == "NULL")
                                                        {
                                                            dv += " 0";
                                                        }
                                                        else if (array[j + 1] == "-")
                                                        {
                                                            dv = $" {array[j + 1]}";
                                                            j++;
                                                            dv += $"{array[j + 1]}";
                                                        }
                                                        else
                                                        {
                                                            dv += $" {array[j + 1]}";
                                                        }
                                                        dv += $" {array[j + 1]}";
                                                        j++;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (j + 1 < array.Length)
                                            {
                                                j++;
                                                if (array[j] != ")"
                                                    && array[j] != ";")
                                                {
                                                    name = array[j];
                                                }
                                            }
                                        }
                                    }


                                    if (type?.Length > 0
                                        && name?.Length > 0)
                                    {
                                        if (type.EndsWith("**"))
                                        {
                                            Console.WriteLine($"ignore double pointer: {array[IndexName]}");
                                            continue;
                                        }

                                        var temp = type;

                                        ignore = excludeVariableTypeList.Any(v => type.Contains(v));

                                        if (!ignore)
                                        {
                                            if (!TryGetTypeForCS(type, true, out type, out var s))
                                            {
                                                try
                                                {
                                                    throw new Exception(temp);
                                                }
                                                catch
                                                {

                                                }
                                            }

                                            if ((type == "System.IntPtr"
                                                || (type?.Contains("VECTOR") ?? false))
                                                && dv?.Length > 0)
                                            {
                                                dv = "default";
                                            }

                                            if (type != null
                                                && type.StartsWith("out")
                                                && name.EndsWith("Array"))
                                            {
                                                var split = type.Split(' ');

                                                type = $"[In, Out] {string.Join(" ", split.Skip(1))}[]";
                                                dv = null;
                                            }

                                            if ((type?.StartsWith("out") ?? false)
                                                || (type?.StartsWith("ref") ?? false)
                                                || (type?.StartsWith("string") ?? false))
                                            {
                                                dv = null;
                                            }



                                            f.ParameterList.Add(new Parameter()
                                            {
                                                Type = type,
                                                Name = name,
                                                defaultValue = dv,
                                            });
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (!ignore)
                                {
                                    string tt = "";
                                    for(int k = 1; k < IndexArgsFirst - 2; k++)
									{
										if (array[k] == "*")
										{
                                            tt = tt.Remove(tt.Length - 1);
                                        }
                                        tt += array[k] + " ";
									}
									if (tt.Length > 0)
									{
                                        tt = tt.Remove(tt.Length - 1);
									}

                                    if (TryGetTypeForCS(tt, true, out var t, out _))
                                    {
                                        f.ReturnType = t;
                                    }
                                    else
                                    {
                                        f.ReturnType = tt;
                                    }
                                    f.Name = array[IndexName];

                                    a.WriteLine($"[DllImport({dllNameVariableValue}, EntryPoint=\"dx_{f.Name}\", CallingConvention=CallingConvention.StdCall)]");
                                    a.WriteLine($"{f.GetString($"extern {(f.IsUnsafe ? "unsafe " : "")}static ", "dx_", true)};");
                                    a.WriteLine($"{f.GetString($"public {(f.IsUnsafe ? "unsafe " : "")}static ")} => dx_{f.Name}({f.GetParameterString(true, true)});");
                                    a.WriteLine("");
                                }
                                f.Clear();
                                if (ignore)
                                {
                                    break;
                                }
                            }
                            break;
                        }
                    }


                }
#endif

            }
        }

        public static void Main(string[] args)
        {
            // 既存の出力先フォルダを削除し、再作成する。
            if (Directory.Exists(outputFolderPath))
            {
                Directory.Delete(outputFolderPath, true);
            }
            Directory.CreateDirectory(outputFolderPath);

            var sr = new StreamReader(dxlibHeaderFilePath);

            var srFuncWin = new StreamReader(dxlibFuncWinHeaderFilePath);

            // 関数
            using (var a = new CSSourceStream(outputExtensionsFilePath))
            {
                a.WriteLine("#if _WIN32");
                a.WriteLine($"const string {dllNameVariableValue} = \"DxLib.dll\";");
                a.WriteLine("#elif _WIN64");
                a.WriteLine($"const string {dllNameVariableValue} = \"DxLib_x64.dll\";");
                a.WriteLine("#else");
                a.WriteLine($"const string {dllNameVariableValue} = \"DxLib.dll\";");
                a.WriteLine("#endif");
                a.WriteLineConstInt("DX_TRUE", 1);
                a.WriteLineConstInt("DX_FALSE", 0);
            }

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

            // 関数
            using (var a = new CSSourceStream(outputFunctionManualFilePath))
			{
                const string manualFilePath = "Tatelier.DxLib.Function.Manual.txt";

                // 手動入力部を書き込み
                foreach(var l in File.ReadAllLines(manualFilePath))
                {
                    a.WriteLine(l);
                }
			}

            using (var a = new CSSourceStream(outputFunctionAutoFilePath))
            {
                sr.BaseStream.Seek(0, SeekOrigin.Begin);

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Contains("DX_FUNCTION_START"))
                    {
                        WriteFunction(sr, a);
                    }
                }


                srFuncWin.BaseStream.Seek(0, SeekOrigin.Begin);


                while (!srFuncWin.EndOfStream)
                {
                    var line = srFuncWin.ReadLine();
                    if (line.Contains("DX_FUNCTION_START"))
                    {
                        WriteFunction(srFuncWin, a);
                    }
                }
            }
        }
    }
}
