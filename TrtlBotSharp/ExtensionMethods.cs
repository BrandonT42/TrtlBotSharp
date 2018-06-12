using System;
using System.Collections.Generic;

namespace TrtlBotSharp
{
    public static class ExtensionMethods
    {
        public static IEnumerable<List<ulong>> SplitList(this List<ulong> locations, int nSize=30)  
        {        
            for (int i=0; i < locations.Count; i+= nSize) 
            { 
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i)); 
            }  
        }   
    }
}
