using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tatelier.DxLib.Optimize
{
    [DebuggerDisplay("{inputText}")]
    public class TextAnalyzer
    {

        public char[] splitIgnoreCharList = new char[]
        {
            ' ',
            '\r',
            '\n',
            '\t',
        };

        public char[] splitCharList2 = new char[]
        {
            '*',
            '-',
            '+',
            '/',
            '%',
            '(',
            ')',
            ',',
            '#',
        };

        public StringBuilder inputText;

        public int index;

        public string[] ToArrayStr()
        {
            int temp = index;

            var result = EnumerateStr2().ToArray();

            index = temp;

            return result;
        }

        IEnumerable<string> EnumerateStr2()
		{
            string str = GetStr();
            while(str != null)
			{
                yield return str;
                str = GetStr();
            }
		}

        public IEnumerable<string> EnumerateStr()
        {
            return ToArrayStr();
        }

        public string GetStr()
        {
            var outputText = new StringBuilder(32);

            for (int i = index; i < inputText.Length; i++)
            {
                // 文字
                if(inputText[i] == '\'')
                {
                    if(i + 2 < inputText.Length
                        && inputText[i+2] == '\'')
                    {
                        outputText.Append(inputText[i]);
                        outputText.Append(inputText[i + 1]);
                        outputText.Append(inputText[i + 2]);
                        index += 2;
                        return $"{outputText}";
                    }
                    else
                    {
                        throw new System.Exception("文字がシングルコーテーションで囲まれていません。");
                    }
                }
                // 文字列
                // "と"で囲まれたテキストは文字列とする。
                else if (inputText[i] == '\"')
                {
                    outputText.Append(inputText[i]);

                    for (; i < inputText.Length; i++)
                    {
                        outputText.Append(inputText[i]);
                        if (inputText[i] == '\"')
                        {
                            index = i + 1;
                            return $"{outputText}";
                        }
                    }
                }
                // コメント
                // //以降の文字列はその行に限り無視する
                else if ('/' == inputText[i]
                    && (i + 1) < inputText.Length
                    && '/' == inputText[i + 1])
                {
                    for (; i < inputText.Length; i++)
                    {
                        if (inputText[i] == '\n')
                        {
                            index = i + 1;
                            return $"{outputText}";
                        }
                    }
                }
                // コメント
                // /*以降の文字列は*/が現れるまで行をまたいで無視する。
                else if('/' == inputText[i]
                    && (i + 1) < inputText.Length
                    && '*' == inputText[i + 1])
                {
                    if (outputText.Length > 0)
                    {
                        return $"{outputText}";
                    }
                    else
                    {
                        for (; i < inputText.Length; i++)
                        {
                            if (inputText[i] == '*'
                                && (i + 1) < inputText.Length
                                && '/' == inputText[i + 1])
                            {
                                index = i + 2;
                                i = index;
                                break;
                            }
                        }
                    }
                }
                // 無視リストの文字の場合、そこで
                else if (splitIgnoreCharList.Any(v => v == inputText[i]))
                {
                    if (outputText.Length > 0)
                    {
                        index = i + 1;
                        return $"{outputText}";
                    }
                }
                // 分割文字リストの文字の場合、そこまで積み上げた文字列を返し、
                // その次に分岐文字を返す
                else if (splitCharList2.Any(v => v == inputText[i]))
                {
                    if (outputText.Length > 0)
                    {
                        index = i;
                        return $"{outputText}";
                    }
                    else
                    {
                        index = i + 1;
                        return $"{inputText[i]}";
                    }
                }
                else
                {
                    outputText.Append(inputText[i]);
                }
            }

            index = inputText.Length;

            return null;
        }

        public void SetText(string text)
        {
            inputText = new StringBuilder(text);
            index = 0;
        }
    }
}
