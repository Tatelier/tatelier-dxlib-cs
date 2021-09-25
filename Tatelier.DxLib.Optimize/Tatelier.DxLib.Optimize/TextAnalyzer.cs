using System.Linq;
using System.Text;

namespace Tatelier.DxLib.Optimize
{
    class TextAnalyzer
    {
        static char[] splitCharList = new char[]
        {
            '\t',
            ' ',
            '\r',
            '\n',
        };

        public StringBuilder inputText;
        public int index;

        public string GetStr()
        {
            var outputText = new StringBuilder(64);

            for (int i = index; i < inputText.Length; i++)
            {
                if (inputText[i] == '\"')
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
                else if (splitCharList.Any(v => v == inputText[i]))
                {
                    if (outputText.Length > 0)
                    {
                        index = i + 1;
                        return $"{outputText}";
                    }
                }
                else if (',' == inputText[i]
                    || '*' == inputText[i])
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

            return $"{outputText}";
        }
    }
}
