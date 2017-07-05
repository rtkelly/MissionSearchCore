using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.LuceneClient
{
    public class LuceneMapper<T> where T : ISearchDocument
    {
        PropertyInfo[] Properties;
        
        /// <summary>
        /// 
        /// </summary>
        public LuceneMapper()
        {
            Type t = typeof(T);
            Properties = t.GetProperties();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Document ToLuceneDocument(T doc)
        {
            var luceneDoc = new Document();
            //luceneDoc.Boost = (float) boost;

            string key = null;

            foreach (var property in Properties)
            {
                var propValue = property.GetValue(doc);

                if (propValue != null)
                {
                    switch (property.Name)
                    {
                        case "id":
                            key = propValue.ToString();
                            luceneDoc.Add(new Field(property.Name, propValue.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                            break;
                        case "title":
                            luceneDoc.Add(new Field(property.Name, propValue.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                            luceneDoc.Add(new Field(property.Name+"_sortable", propValue.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                            break;

                        default:
                            switch(property.PropertyType.Name)
                            {
                                case "List`1": 
                                    
                                    var list = propValue as List<string>;

                                    foreach (var value in list ?? new List<string>())
                                    {
                                        luceneDoc.Add(new Field(property.Name, value, Field.Store.YES, Field.Index.ANALYZED));
                                    }
                                    break;

                                default:
                                    luceneDoc.Add(new Field(property.Name, propValue.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                                    break;
                            }


                            break;
                    }
                }
            }

            return luceneDoc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scoreDoc"></param>
        /// <returns></returns>
        public T ToSearchDocument(Document scoreDoc)
        {
            var searchDoc = (T)Activator.CreateInstance(typeof(T), new object[] { });
                        
            var fields = scoreDoc.GetFields();

            foreach (var field in fields)
            {
                try
                {
                    var prop = Properties.FirstOrDefault(p => p.Name == field.Name);

                    if (prop != null)
                    {
                        switch (prop.PropertyType.Name)
                        {
                            case "DateTime":
                                DateTime date;

                                if (DateTime.TryParse(field.StringValue, out date))
                                    prop.SetValue(searchDoc, date);

                                break;

                            case "Int32":
                                int i;

                                if (int.TryParse(field.StringValue, out i))
                                    prop.SetValue(searchDoc, i);

                                break;

                            case "Int64":
                                long l;

                                if (long.TryParse(field.StringValue, out l))
                                    prop.SetValue(searchDoc, l);

                                break;

                            case "Boolean":
                                Boolean b;

                                if (Boolean.TryParse(field.StringValue, out b))
                                    prop.SetValue(searchDoc, b);

                                break;


                            case "List`1":

                                var list = prop.GetValue(searchDoc) as List<string>;

                                if (list == null)
                                    list = new List<string>();

                                list.Add(field.StringValue);

                                prop.SetValue(searchDoc, list);    

                                break;

                            default:
                                prop.SetValue(searchDoc, field.StringValue.Trim());
                                break;
                        }
                    }
                }
                catch
                {
                    // TO DO: Handle errors
                }
            }

            return searchDoc;
        }
    }
}
