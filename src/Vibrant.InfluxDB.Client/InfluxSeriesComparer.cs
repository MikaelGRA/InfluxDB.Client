using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public static class InfluxSeriesComparer
   {
      public static bool Compare<TInfluxRow>( InfluxSeries<TInfluxRow> left, InfluxSeries<TInfluxRow> right )
      {
         return Compare( left, right.GroupedTags );
      }

      public static bool Compare<TInfluxRow>( InfluxSeries<TInfluxRow> left, IEnumerable<KeyValuePair<string, object>> rightTags )
      {
         if( left.GroupedTags == null && ( rightTags == null || rightTags.Count() == 0 ) )
         {
            return true;
         }

         foreach( var tag in rightTags )
         {
            object tagValue;
            if( left.GroupedTags.TryGetValue( tag.Key, out tagValue ) )
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
   }
}
