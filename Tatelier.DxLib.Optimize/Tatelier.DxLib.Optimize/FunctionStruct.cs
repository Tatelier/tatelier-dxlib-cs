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

        public string GetParameterString(bool ignoreType = false, bool ignoreDefaultValue = false)
        {
            string result = "";

            foreach(var item in ParameterList)
            {
                result += $"{(ignoreType ? "" : $"{item.Type} ")}{item.Name}{(ignoreDefaultValue ? "" : (item.defaultValue != null ? $" = {item.defaultValue}" : ""))}, ";
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
