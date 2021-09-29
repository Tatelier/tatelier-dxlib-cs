using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tatelier.DxLib.Optimize
{
    class Parameter
    {
        public string Type;

        public string Name;

        public string defaultValue;
    }

    class Function
    {
        /// <summary>
        /// 返り値型
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// 関数名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Unsafeかどうか
        /// </summary>
        public bool IsUnsafe { get; set; } = false;

        /// <summary>
        /// 仮引数リスト
        /// </summary>
        public List<Parameter> ParameterList { get; } = new List<Parameter>();

        bool IsDefaultValueOK(int index)
        {
            if(index == ParameterList.Count - 1)
            {
                return true;
            }

            for(int i = ParameterList.Count - 1; i > index; i--)
            {
                if (ParameterList[i].Type.StartsWith("out ")
                    || ParameterList[i].defaultValue == null
                    || ParameterList[i].defaultValue.Length == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 仮引数の文字列を取得する。
        /// </summary>
        /// <param name="ignoreType"></param>
        /// <param name="ignoreDefaultValue"></param>
        /// <returns></returns>
        public string GetParameterString(bool ignoreType = false, bool ignoreDefaultValue = false)
        {
            string result = "";

            for(int i=0;i<ParameterList.Count;i++)
            {
                var item = ParameterList[i];

                var typeSplit = item.Type?.Split(' ') ?? Array.Empty<string>();

                string t = "";

                if (ignoreDefaultValue)
                {
                    if (typeSplit.Length > 1
                        && (typeSplit[0] == "out"
                        || typeSplit[0] == "ref"))
                    {
                        string temp = "";

                        temp += typeSplit[0];

                        t = $"{(ignoreType ? $"{temp} " : $"{item.Type} ")}";
                    }
                    else
                    {
                        t = $"{(ignoreType ? "" : $"{item.Type} ")}";
                    }
                }
                else
                {
                    t = $"{(ignoreType ? "" : $"{item.Type} ")}";
                }

                string defaultValue;

                if ((item.Type?.StartsWith("out") ?? false)
                    || (item.Type?.StartsWith("ref") ?? false))
                {
                    defaultValue = "";
                }
                else
                {
                    if(item.defaultValue == "-1")
                    {

                    }

                    if (IsDefaultValueOK(i))
                    {
                        defaultValue = (ignoreDefaultValue ? "" : (item.defaultValue != null ? $" = {item.defaultValue}" : ""));
                    }
                    else
                    {
                        defaultValue = "";
                    }
                }

                result += $"{t}{item.Name}{defaultValue}, ";
            }

            if (result.Length > 0)
            {
                result = result.Substring(0, result.Length - 2);
            }
            return result;
        }

        /// <summary>
        /// C#関数に対応した文字列を取得する
        /// </summary>
        /// <param name="header"></param>
        /// <param name="functionNameHeader">関数名の前に付ける文字列</param>
        /// <param name="ignoreDefaultValue">仮引数のデフォルト値を無視する</param>
        /// <returns></returns>
        public string GetString(string header, string functionNameHeader = "", bool ignoreDefaultValue = false)
        {
            return $"{header}{ReturnType} {functionNameHeader}{Name}({GetParameterString(ignoreDefaultValue: ignoreDefaultValue)})";
        }

        /// <summary>
        /// クリアする
        /// </summary>
        public void Clear()
        {
            ReturnType = null;
            Name = null;
            IsUnsafe = false;
            ParameterList.Clear();
        }
    }
}
