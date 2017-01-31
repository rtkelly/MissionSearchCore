using MissionSearch.Attributes;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MissionSearch.Indexers
{
    public abstract class IndexerBase<T> where T : ISearchDocument
    {
        protected ILogger _logger { get; set; }

        protected void LogError(string message)
        {
            if(_logger != null)
                _logger.Error(message);
        }

        protected void LogWarning(string message)
        {
            if(_logger != null)
                _logger.Warn(message);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="docProp"></param>
        /// <param name="value"></param>
        protected void SetPropertyValue(T doc, PropertyInfo docProp,  object value)
        {
            if (value == null)
               return;

            switch (docProp.PropertyType.Name)
            {
                case "List`1":

                    if (value.GetType().Name == docProp.PropertyType.Name)
                    {
                        docProp.SetValue(doc, value);
                    }
                    else
                    {
                        if (docProp.PropertyType.GenericTypeArguments.Any())
                        {
                            var list = docProp.GetValue(doc) as List<string>;

                            if (list == null)
                                list = new List<string>();

                            switch (docProp.PropertyType.GenericTypeArguments.First().Name)
                            {
                                case "String":

                                    list.Add(HtmlParser.StripHTML(value.ToString()));
                                    break;
                                                                   

                            }

                            docProp.SetValue(doc, list);    
                        }
                    }

                    break;

                case "string":
                case "String":
                    
                    if (value.GetType().Name == "XhtmlString")
                    {
                       docProp.SetValue(doc, HtmlParser.StripHTML(value.ToString()));
                    }
                    else
                    {
                        docProp.SetValue(doc, value);
                    }
                    break;

                default:
                    docProp.SetValue(doc, value);
                    break;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="doc"></param>
        /// <param name="docProps"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        protected T GetBaseProperties(ISearchableContent page, T doc, PropertyInfo[] docProps, System.Type baseType)
        {
            var pageProps = baseType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

            // load indexed field
            foreach (var pageProp in pageProps)
            {
                var customAttr = Attribute.GetCustomAttributes(pageProp, typeof(SearchIndex));

                if (customAttr == null || !customAttr.Any())
                    continue;

                var srchFieldMap = customAttr.First() as SearchIndex;

                var fieldName = srchFieldMap.FieldName;

                var docProp = srchFieldMap.FieldName != null ? docProps.FirstOrDefault(p => p.Name == fieldName)
                    : docProps.FirstOrDefault(p => p.Name == Global.ContentField);

                if (docProp != null)
                {
                    var value = pageProp.GetValue(page);

                    if (value != null)
                        SetPropertyValue(doc, docProp, value);
                }
            }

            return doc;
        }
        
    }
}
