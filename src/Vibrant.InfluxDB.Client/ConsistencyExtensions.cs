using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
    internal static class ConsistencyExtensions
    {
      internal static string GetQueryParameter( this Consistency that )
      {
         switch ( that )
         {
            case Consistency.One:
               return "one";
            case Consistency.Quorum:
               return "quorum";
            case Consistency.All:
               return "all";
            case Consistency.Any:
               return "any";
            default:
               throw new ArgumentException( "Invalid parameter value.", nameof( that ) );
         }
      }
    }
}
