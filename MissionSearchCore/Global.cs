using MissionSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{

    public static class Global<T>
    {
        public delegate T IndexCallBack(T doc, ISearchableContent page);
        public delegate bool StatusCallBack(string msg = null);

    }


    public static class Global
    {
        public static string ContentField { get { return "content";  } }

       
        

    }
}
