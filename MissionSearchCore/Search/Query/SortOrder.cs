using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class SortOrder
    {
       public enum SortOption
       {
           Ascending = 0,
           Descending = 1,
       }
               
       public string SortField { get; set; }
       public string DisplayName { get; set; }
       public SortOption Order { get; set; }

       public string SortDir
       {
           get { return (Order == SortOption.Descending) ? "desc" : "asc";  }
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="name"></param>
       /// <param name="option"></param>
       public SortOrder(string name, SortOption option)
       {
           SortField = name;
           Order = option;
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="name"></param>
       /// <param name="option"></param>
       public SortOrder(string name, string displayName, SortOption option)
       {
           SortField = name;
           Order = option;
           DisplayName = displayName;
       }

        

    }
}
