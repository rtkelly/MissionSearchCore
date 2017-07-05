using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.LuceneClient
{
    public class LuceneDoc : DynamicObject
    {
        Document InnerLuceneDoc;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public LuceneDoc(Document doc)
        {
            InnerLuceneDoc = doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder,  out object result)
        {
            result = GetPropertyValue(binder.Name);

            return result != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object GetPropertyValue(string propertyName)
        {
            var result = InnerLuceneDoc.GetField(propertyName);
            
            return result.StringValue;
        }
    }
}
