using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Class representing an error occurring in the InfluxClient.
   /// </summary>
   [Serializable]
   public class InfluxException : Exception
   {
      internal InfluxException() { }
      internal InfluxException( string message ) : base( message ) { }
      internal InfluxException( string message, Exception inner ) : base( message, inner ) { }
   }
}
