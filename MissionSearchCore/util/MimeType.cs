using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Util
{
    public static class MimeType
    {
        public static string GetDisplayName(string mimetype)
        {
            switch (mimetype)
            {
                case "application/pdf":
                    return "PDF";
                
                case "audio/mp3":
                case "audio/mp4":
                    return "Audio";
                
                case "video/quicktime":
                case "application/vnd.rn-realmedia":
                case "video/mp4":
                case "application/mp4":
                case "video/x-f4v":
                case "video/x-flv":
                    return "Video";

                case "application/msword":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.template":
                case "application/vnd.ms-word.document.macroEnabled.12":
                case "application/vnd.ms-word.template.macroEnabled.12":
                    return "MS Word";
                 
                case "application/vnd.ms-excel":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.template":
                case "application/vnd.ms-excel.sheet.macroEnabled.12":
                case "application/vnd.ms-excel.template.macroEnabled.12":
                case "application/vnd.ms-excel.addin.macroEnabled.12":
                case "application/vnd.ms-excel.sheet.binary.macroEnabled.12":
                    return "MS Excel";

                case "application/vnd.ms-powerpoint":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                case "application/vnd.openxmlformats-officedocument.presentationml.template":
                case "application/vnd.openxmlformats-officedocument.presentationml.slideshow":
                case "application/vnd.ms-powerpoint.addin.macroEnabled.12":
                case "application/vnd.ms-powerpoint.presentation.macroEnabled.12":
                case "application/vnd.ms-powerpoint.template.macroEnabled.12":
                case "application/vnd.ms-powerpoint.slideshow.macroEnabled.12":
                    return "MS PowerPoint";

            }

            return "";
        }
    }
}
