using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.LuceneClient.CustomAnalyzer
{
    public class CustomStandardAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var result = new CustomCharTokenizer(reader);

            //result = new TokenFilter(result);

            return new StopFilter(false, result, StopAnalyzer.ENGLISH_STOP_WORDS_SET, true);
           
        }
    }
}
