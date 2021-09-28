using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tatelier.DxLib.Optimize
{
    class CSV
    {
        string[] headers;

        string[][] contents;

        public string GetName(int column)
        {
            if(0 <= column && column < headers.Length)
            {
                return headers[column];
            }
            else
            {
                return null;
            }
        }

        public IReadOnlyList<IReadOnlyList<string>> Contents => contents;

        public int GetIndex(string name)
        {
            return Array.IndexOf(headers, name);
        }
        public string GetValue(string name, IReadOnlyList<string> values)
        {
            int index = GetIndex(name);

            if(index != -1)
            {
                return values[index];
            }

            return null;
        }

        public string[] GetSearch(Func<string[], bool> searchMethod)
        {
            foreach (var content in contents)
            {
                if (searchMethod(content))
                {
                    return content;
                }    
            }

            return null;
        }

        public CSV(string filePath, char splitChar = ',')
        {
            var lines = File.ReadAllLines(filePath);
            headers = lines[0].Split(splitChar);

            List<string[]> list2 = new List<string[]>();

            foreach (var line in lines.Skip(1))
            {
                var split = line.Split(splitChar);
                if (split.Length > 0)
                {
                    list2.Add(split);
                }
            }

            contents = list2.ToArray();
        }
    }
}
