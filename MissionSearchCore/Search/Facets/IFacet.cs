using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Search.Facets
{
    public interface IFacet
    {
        string FieldName { get; set; }
        string FieldLabel { get; set; }
        int Order { get; set; }
    }
}
