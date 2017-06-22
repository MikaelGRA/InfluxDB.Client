using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// InfluxQueryOptions are applied to queries made to InfluxDB.
   /// </summary>
   public sealed class InfluxQueryOptions
   {
      /// <summary>
      /// Constructs an InfluxQueryOptions with default values.
      /// </summary>
      public InfluxQueryOptions()
      {
         Precision = null;
         ChunkSize = null;
         MetadataExpiration = TimeSpan.FromHours( 1 );
      }

      /// <summary>
      /// Gets or sets the Precision. Default is nanoseconds.
      /// </summary>
      public TimestampPrecision? Precision { get; set; }

      /// <summary>
      /// Gets or sets the ChunkSize. Default is null. Which means
      /// it uses the InfluxDB default of 10000.
      /// </summary>
      public int? ChunkSize { get; set; }

      /// <summary>
      /// Gets or sets how long before retrieved metadata about measurements
      /// takes to expire and must be retrieved again.
      /// 
      /// This is only used when querying data based on the IInfluxRow interface
      /// because this interface. This is because the interface has no way to 
      /// know which retrieved columns are fields or tags. It therefore makes an
      /// implicit query to get this information from the database.
      /// 
      /// A value of null means it never expires. Default is 1 hour.
      /// </summary>
      public TimeSpan? MetadataExpiration { get; set; }
   }
}
