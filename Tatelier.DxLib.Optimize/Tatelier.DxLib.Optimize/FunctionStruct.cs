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
        public string ReturnType;

        public string Name;

        public bool IsUnsafe = false;

        public List<Parameter> ParameterList = new List<Parameter>();

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

        public string GetString(string header, string functionNameHeader = "", bool ignoreDefaultValue = false)
        {
            return $"{header}{ReturnType} {functionNameHeader}{Name}({GetParameterString(ignoreDefaultValue: ignoreDefaultValue)})";
        }

        public void Clear()
        {
            ReturnType = null;
            Name = null;
            IsUnsafe = false;
            ParameterList.Clear();
        }
    }
}
