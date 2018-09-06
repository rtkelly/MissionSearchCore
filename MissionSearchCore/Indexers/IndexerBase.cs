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
    public abstract class IndexerBase
    {
        protected ILogger _logger { get; set; }

        protected int _sourceId { get; set; }
        
        protected void LogError(string message)
        {
            if (_logger != null)
                _logger.Error(message);
        }

        protected void LogWarning(string message)
        {
            if (_logger != null)
                _logger.Warn(message);
        }


    }

    public abstract class IndexerBase<T> : IndexerBase  where T : ISearchDocument
    {
        
        /// <summary>
        /// This method is used to assign a value to a generic object. 
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
                case "String[]":
                    {
                        var array = docProp.GetValue(doc) as String[];
                        
                        var list = (array == null) ? new List<string>() : array.ToList();

                        var valueArray = value as String[];

                        foreach (var str in valueArray)
                        {
                            list.Add(str);
                        }

                        docProp.SetValue(doc, list.ToArray());
                    }
                    break;
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
        /// This method checks for any content properties decorated with the SearchIndex attribute.
        /// If the attribite contains a field name then the property value is assigned to that field in the index.
        /// If no field name is defined the content is added to the content field in the index.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="doc"></param>
        /// <param name="docProps"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        protected T AddSearchIndexProperties(ISearchableContent page, T doc, PropertyInfo[] docProps, Type baseType)
        {
            var pageProps = baseType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
                      
            // load indexed field
            foreach (var pageProp in pageProps)
            {
                //pageProp.GetCustomAttribute()
                var customAttr = Attribute.GetCustomAttributes(pageProp, typeof(SearchIndex));

                if (!customAttr.Any())
                    continue;

                var srchFieldMap = customAttr.First() as SearchIndex;

                if (srchFieldMap == null) 
                    continue;

                var fieldName = srchFieldMap.FieldName;

                var docProp = srchFieldMap.FieldName != null ? docProps.FirstOrDefault(p => p.Name == fieldName)
                    : docProps.FirstOrDefault(p => p.Name == Global.ContentField);

                if (docProp != null)
                {
                    var value = pageProp.GetValue(page);

                    var dateValue = value as DateTime?;

                    if (dateValue != null && dateValue is DateTime?)
                    {
                        if(dateValue.Value != DateTime.MinValue)
                        {
                            SetPropertyValue(doc, docProp, dateValue.Value);
                        }
                    }
                    else if (value != null)
                    {
                        SetPropertyValue(doc, docProp, value);
                    }
                }
            }

            return doc;
        }
        
    }
}
