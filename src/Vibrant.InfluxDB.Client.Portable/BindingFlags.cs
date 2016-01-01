using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   [Flags]
   internal enum BindingFlags
   {
      None = 0,
      Instance = 1,
      Public = 2,
      Static = 4,
      FlattenHierarchy = 8,
      NonPublic = 16,
      SetProperty = 8192
   }
}
