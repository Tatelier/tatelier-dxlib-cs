using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Tatelier.DxLib.Optimize;

namespace TestTextAnalyzer
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var textAnalyzer = new TextAnalyzer();
            textAnalyzer.inputText = new System.Text.StringBuilder();

            var array = textAnalyzer.ToArrayStr();

            File.WriteAllLines(nameof(TestMethod1), array);
        }
    }
}
