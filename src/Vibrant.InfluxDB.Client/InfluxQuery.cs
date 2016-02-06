//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;
//using Vibrant.InfluxDB.Client.IQToolkit;

//namespace Vibrant.InfluxDB.Client
//{
//   public class InfluxQuery<TData> : Query<TData>
//   {
//      public InfluxQuery( InfluxClient client, Type type, string db, string seriesId )
//         : base( new InfluxQueryProvider<TData>( client, db, seriesId ) )
//      {
//      }

//      public InfluxQuery( InfluxQueryProvider<TData> provider, Expression expression )
//         : base( provider, expression )
//      {

//      }

//      public string GetQueryText( Expression expression )
//      {
//         return ( (IQueryText)Provider ).GetQueryText( expression );
//      }

//      public override string ToString()
//      {
//         return GetQueryText( Expression );
//      }
//   }
//}
