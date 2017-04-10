using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;


namespace MissionSearch.Util
{
    /// <summary>
    /// A Collection of methods for working with Ektron Smartforms.
    /// </summary>

    public class XmlParser
    {
        XDocument xDoc;
        // ReSharper disable once InconsistentNaming
        public string Xml { get; set; }

        XmlNamespaceManager NamespaceManager;

        /// <summary>
        /// Constructor for the SmartformHelper
        /// </summary>
        /// <param name="xml">The XML for the smarform data</param>
        public XmlParser(string xml)
        {
            xDoc = XDocument.Parse((!string.IsNullOrEmpty(xml)) ? xml : "<root></root>");
            Xml = xml;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="namespaceManager"></param>
        public XmlParser(string xml, XmlNamespaceManager namespaceManager)
        {
            xDoc = XDocument.Parse((!string.IsNullOrEmpty(xml)) ? xml : "<root></root>");
            
            NamespaceManager = namespaceManager;

            Xml = xml;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripXml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var str = Regex.Replace(input, "<\\?.*?>", String.Empty);
            str = Regex.Replace(str, "<.*?>", String.Empty);
            str = HtmlParser.StripFormating(str);
            str = HtmlParser.StripComments(str);

            return str;
        }

        /// <summary>
        /// Takes a given xPath of an XML document and returns a value
        /// </summary>
        /// <param name="xpath">The path of node to return a value for</param>
        /// <param name="defaultValue"></param>
        /// <returns>A string with the desired value</returns>
        public string ParseString(string xpath, string defaultValue = "")
        {
            if (xDoc == null)
                return "";

            XElement xstr = xDoc.XPathSelectElement(xpath);

            if (xstr != null && !string.IsNullOrEmpty(xstr.Value))
            {
                return xstr.Value;
            }

            return defaultValue;
        }
                

        /// <summary>
        /// Parses the given xPath and returns a string value
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <returns>A string value for the given xPath</returns>
        // ReSharper disable once InconsistentNaming
        public string ParseHTML(string xpath)
        {
            if (xDoc == null)
                return "";

            XElement xstr = (NamespaceManager == null) ? xDoc.XPathSelectElement(xpath) : xDoc.XPathSelectElement(xpath, NamespaceManager);

            if (xstr == null)
                return "";

            StringBuilder str = new StringBuilder();

            foreach (XNode element in xstr.Nodes())
            {
                str.Append(element);
            }
        
                
            return str.ToString();
        }


        public string ParseStripHTML(string xpath)
        {
            if (xDoc == null)
                return "";

            XElement xstr = (NamespaceManager == null) ? xDoc.XPathSelectElement(xpath) : xDoc.XPathSelectElement(xpath, NamespaceManager);

            if (xstr == null)
                return "";

            StringBuilder str = new StringBuilder();

            foreach (XNode element in xstr.Nodes())
            {
                str.Append(element);
            }


            return StripHTML(str.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        /// <summary>
        /// Return outer xml from xpath
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public string ParseOuterHTML(string xpath)
        {
            if (xDoc == null)
                return "";

            XElement xstr = xDoc.XPathSelectElement(xpath);

            if (xstr == null)
                return "";

            StringBuilder str = new StringBuilder();

            foreach (XNode element in xstr.Nodes())
            {
                str.Append(element);
            }

            return string.Format("<{0}>{1}</{0}>", xstr.Name, str);
        }

        /// <summary>
        /// Parses the given xElement and returns a string value
        /// </summary>
        /// <param name="xElement">The xElement to return a value for</param>
        /// <returns>A string value for the given xElement</returns>
        // ReSharper disable once InconsistentNaming
        public string ParseHTML(XElement xElement)
        {
            if (xDoc == null)
                return "";

            var str = new StringBuilder();

            if (xElement != null)
            {
                foreach (XNode element in xElement.Nodes())
                {
                    str.Append(element);
                }
            }

            return str.ToString();
        }

        public string ParseText(string xpath)
        {
            string value = ParseHTML(xpath);

            if (string.IsNullOrEmpty(value))
                value = ParseString(xpath);

            return value;

        }

        /// <summary>
        /// Parses the given xPath and returns a DateTime value
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <returns>A DateTime value for the given xPath</returns>
        public DateTime? ParseDate(string xpath)
        {
            if (xDoc == null)
                return null;

            XElement xdate = xDoc.XPathSelectElement(xpath);

            if (xdate != null)
            {
                DateTime date;

                if (DateTime.TryParse(xdate.Value, out date))
                {
                    return date;
                }

            }
            return null;
        }

        public string ParseDateStr(string xpath, string format)
        {
            if (xDoc == null)
                return "";

            XElement xdate = xDoc.XPathSelectElement(xpath);

            if (xdate != null)
            {
                DateTime date;

                if (DateTime.TryParse(xdate.Value, out date))
                {
                    return date.ToString(format);
                }

            }
            return "";
        }

        /// <summary>
        /// Parses the given xPath and returns a long value
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <param name="defaultValue"></param>
        /// <returns>A long value for the given xPath</returns>
        public long ParseLong(string xpath, long defaultValue = 0)
        {
            if (xDoc == null)
                return defaultValue;

            XElement xlong = xDoc.XPathSelectElement(xpath);

            if (xlong != null)
            {
                long value;

                if (long.TryParse(xlong.Value, out value))
                {
                    return value;
                }

            }
            return 0;
        }

        /// <summary>
        /// Parses the given xPath and returns a long value
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <param name="defaultValue"></param>
        /// <returns>A long value for the given xPath</returns>
        public int ParseInt(string xpath, int defaultValue=0)
        {
            if (xDoc == null)
                return defaultValue;

            XElement xlong = xDoc.XPathSelectElement(xpath);

            if (xlong != null)
            {
                int value;

                if (int.TryParse(xlong.Value, out value))
                {
                    return value;
                }

            }
            return defaultValue;
        }

        /// <summary>
        /// Parses the given xPath and returns a boolean value
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <returns>A boolean value for the given xPath</returns>
        public bool ParseBool(string xpath)
        {
            if (xDoc == null)
                return false;

            XElement xlong = xDoc.XPathSelectElement(xpath);

            if (xlong != null)
            {
                bool value;

                if (bool.TryParse(xlong.Value, out value))
                {
                    return value;
                }

            }
            return false;
        }

        /// <summary>
        /// Parses the given xPath and returns a integer representation of a boolean value
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <returns>An integer representation of a boolean value for the given xPath</returns>
        public int ParseBooltoInt(string xpath)
        {
            if (xDoc == null)
                return 0;

            XElement xbool = xDoc.XPathSelectElement(xpath);

            if (xbool != null)
            {
                bool value;

                if (bool.TryParse(xbool.Value, out value))
                {
                    return (value) ? 1 : 0;
                }
            }
            return 0;
        }

        
        
        
        /// <summary>
        /// Parses a Group in an Ektron Smartform and returns a list of SmartformHelpers
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <returns>A List of SmartformHelpers containing the parsed Group items</returns>
        public List<XmlParser> ParseGroup(string xpath)
        {
            var groups = new List<XmlParser>();
                
            if (xDoc == null || string.IsNullOrEmpty(xpath))
                return groups;

            groups = (from b in xDoc.XPathSelectElements(xpath)
                select new XmlParser(b.ToString())).ToList();

            return groups;
        }

        public string ParseGroupStr(string xpath)
        {
            StringBuilder str  = new StringBuilder();

            if (xDoc == null || string.IsNullOrEmpty(xpath))
                return str.ToString();

            (from b in xDoc.XPathSelectElements(xpath)
                select new XmlParser(b.ToString()))
                .ToList()
                .ForEach(sf => str.Append(sf.Xml));

            return str.ToString();

        }

        public List<string> ParseToList(string xpath)
        {
            var list = new List<string>();

            if (xDoc == null || string.IsNullOrEmpty(xpath))
                return list;

            (from b in xDoc.XPathSelectElements(xpath)
             select b.Value)
                .ToList()
                .ForEach(value => list.Add(value));

            return list;

        }

        public XmlParser ParseFirst(string xpath)
        {
            if (xDoc == null)
                return null;

            List<XmlParser> groups;
        
            groups = (from b in xDoc.XPathSelectElements(xpath)
                select new XmlParser(b.ToString())).ToList();

            return groups.FirstOrDefault();
        }

        /// <summary>
        /// Parses a Group in an Ektron Smartform and returns a list of SmartformHelpers
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public List<XmlParser> ParseGroupItems(string xpath)
        {
            List<XmlParser> html;

            html = (from b in xDoc.XPathSelectElements(xpath).Nodes()
                select new XmlParser(b.ToString())).ToList();

            return html;
        }

        /// <summary>
        /// Parses the given xPath and returns a xElement object
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <returns>A xElement object for the given xPath</returns>
        public XElement GetElement(string xpath)
        {
            return xDoc.XPathSelectElement(xpath);
        }

        /// <summary>
        /// Parses attribute
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <returns>A xElement object for the given xPath</returns>
        public string ParseLinkHref(string xpath)
        {
            XElement xLink = xDoc.XPathSelectElement(xpath);

            if (xLink != null)
            {
                XAttribute xHref = xLink.Attribute("href");
                if (xHref != null)
                    return xHref.Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// Parses attribute
        /// </summary>
        /// <param name="xpath">The xPath to return a value for</param>
        /// <param name="attributeName"></param>
        /// <param name="wrapper"></param>
        /// <returns>A xElement object for the given xPath</returns>
        public string ParseAttribute(string xpath, string attributeName, string wrapper=null)
        {
            if (xDoc == null)
                return "";

            XElement xLink = xDoc.XPathSelectElement(xpath);

            if (xLink != null)
            {
                XAttribute xHref = xLink.Attribute(attributeName);
                if (xHref != null)
                    return Wrapper(wrapper, xHref.Value);
            }

            return string.Empty;
        }

       
        
        public string Wrapper(string wrapper, string value)
        {
            if (wrapper == null)
                return value;

            if (String.IsNullOrEmpty(value))
                return "";

            return string.Format(wrapper, value);
        }


        public string Wrapper(string wrapper, string value, string value2)
        {
            if (wrapper == null)
                return value;

            if (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(value2))
                return "";
        
            return string.Format(wrapper, value, value2);
        }

        /// <summary>
        /// Get root for smart form
        /// </summary>
        /// <returns></returns>
        public string GetRoot()
        {
            return (xDoc.Root != null) ? xDoc.Root.Name.ToString() : "";
        }

        

        /// <summary>
        /// get total instances of xpath
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public int GetCount(string xpath)
        {
            if (xDoc == null)
                return 0;

            return xDoc.XPathSelectElements(xpath).Count();
        }

        
    }
}