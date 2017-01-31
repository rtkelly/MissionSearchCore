using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public interface ISearchableAsset : ISearchableContent
    {   
        byte[] AssetBlob { get; set; }

        bool DisableExtract { get; set; }

        
    }
}
