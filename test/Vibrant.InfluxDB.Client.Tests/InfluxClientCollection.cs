using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Vibrant.InfluxDB.Client.Tests
{
   [CollectionDefinition( "InfluxClient collection" )]
   public class InfluxClientCollection : ICollectionFixture<InfluxClientFixture>
   {
   }
}
