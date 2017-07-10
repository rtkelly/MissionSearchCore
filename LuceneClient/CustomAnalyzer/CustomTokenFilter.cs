using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.LuceneClient.CustomAnalyzer
{
    public class CustomTokenFilter : TokenFilter
    {

        public CustomTokenFilter(TokenStream input) : base(input)
        {
            
        }
        
        public override bool IncrementToken()
        {
            throw new NotImplementedException();
        }
    }
}
