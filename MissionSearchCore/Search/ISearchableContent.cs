using System;
using System.Collections.Generic;
using System.Globalization;

namespace MissionSearch
{

    public interface ISearchableContent 
    {
        String _ContentID { get; set; }

        String Name { get; set;  }

        bool NotSearchable { get; set; }
                
    }
}
