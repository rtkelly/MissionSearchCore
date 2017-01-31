using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Util
{
    public static class ReflectionUtil
    {
        public static List<System.Type> GetBaseTypes(object anObject)
        {
            var contentBaseTypes = new List<System.Type>();

            var baseType = anObject.GetType().BaseType;

            while (baseType != null)
            {
                contentBaseTypes.Add(baseType);
                baseType = baseType.BaseType;
            }

            contentBaseTypes.Reverse();

            return contentBaseTypes;
        }
                
    }
}
