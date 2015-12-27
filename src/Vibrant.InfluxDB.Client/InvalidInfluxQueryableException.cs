using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{

   [Serializable]
   public class InvalidInfluxQueryableException : InvalidOperationException
   {
      public InvalidInfluxQueryableException() { }
      public InvalidInfluxQueryableException( string message ) : base( message ) { }
      public InvalidInfluxQueryableException( string message, Exception inner ) : base( message, inner ) { }
      protected InvalidInfluxQueryableException(
       System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context ) : base( info, context )
      { }
   }
}
