using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   internal static class InfluxSeriesComparer
   {
      public static bool Compare<TInfluxRow>( InfluxSeries<TInfluxRow> left, InfluxSeries<TInfluxRow> right )
      {
         return Compare( left.GroupedTags, right.GroupedTags );
      }

      public static bool Compare( IReadOnlyDictionary<string, object> leftTags, IEnumerable<KeyValuePair<string, object>> rightTags )
      {
         if( leftTags != null && rightTags != null )
         {
            if( leftTags.Count == 0 && rightTags.Count() == 0 )
            {
               return true;
            }

            foreach( var tag in rightTags )
            {
               object tagValue;
               if( leftTags.TryGetValue( tag.Key, out tagValue ) )
               {
                  if( tagValue != null )
                  {
                     if( !tagValue.Equals( tag.Value ) )
                     {
                        return false;
                     }
                  }
                  else
                  {
                     // tagValue is null, so only continue if tag.Value is also null
                     if( tag.Value != null )
                     {
                        return false;
                     }
                  }
               }
            }
            return true;
         }

         return false;

      }
   }
}
