using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MissionSearch
{
    public interface IBoostSettings
    {
        int TitleBoost { get; set; }

        int ContentBoost { get; set; }

        int SummaryBoost { get; set; }

        int DocumentsBoost { get; set; }

        int DateBoost { get; set; }

    }
}