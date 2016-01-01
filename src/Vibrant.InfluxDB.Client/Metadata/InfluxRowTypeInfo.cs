using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Metadata
{
   internal class InfluxRowTypeInfo<TInfluxRow>
   {
      internal readonly Func<TInfluxRow> New;
      internal readonly PropertyExpressionInfo<TInfluxRow> Timestamp;
      internal readonly IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> Tags;
      internal readonly IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> Fields;
      internal readonly IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> All;
      internal readonly IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> PropertiesByClrName;

      internal InfluxRowTypeInfo(
         PropertyExpressionInfo<TInfluxRow> timestamp,
         IDictionary<string, PropertyExpressionInfo<TInfluxRow>> tags, 
         IDictionary<string, PropertyExpressionInfo<TInfluxRow>> fields,
         IDictionary<string, PropertyExpressionInfo<TInfluxRow>> all )
      {
         Timestamp = timestamp;
         Tags = new ReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>>( tags );
         Fields = new ReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>>( fields );
         All = new ReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>>( all );


         PropertiesByClrName = All.ToDictionary( x => x.Value.Property.Name, x => x.Value );

         var newLambda = Expression.Lambda<Func<TInfluxRow>>( Expression.New( typeof( TInfluxRow ) ), true );
         New = newLambda.Compile();
      }
   }
}
