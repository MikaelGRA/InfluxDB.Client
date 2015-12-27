using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{

   [Serializable]
   public class InfluxException : Exception
   {
      public InfluxException() { }
      public InfluxException( string message ) : base( message ) { }
      public InfluxException( string message, Exception inner ) : base( message, inner ) { }
      protected InfluxException(
       System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context ) : base( info, context )
      { }
   }
}
