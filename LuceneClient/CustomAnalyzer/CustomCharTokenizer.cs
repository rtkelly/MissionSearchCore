using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.LuceneClient.CustomAnalyzer
{
    public class CustomCharTokenizer : CharTokenizer
    {
        public CustomCharTokenizer(TextReader input) : base(input)
        {

        }

        protected override char Normalize(char c)
        {
            return char.ToLower(c);
        }

        protected override bool IsTokenChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '"' || c == ' '; //c == '/' || 
        }


    }
}
