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
      internal readonly IReadOnlyList<PropertyExpressionInfo<TInfluxRow>> Tags;
      internal readonly IReadOnlyList<PropertyExpressionInfo<TInfluxRow>> Fields;
      internal readonly IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> All;
      internal readonly IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> PropertiesByClrName;

      internal InfluxRowTypeInfo(
         PropertyExpressionInfo<TInfluxRow> timestamp,
         List<PropertyExpressionInfo<TInfluxRow>> tags,
         List<PropertyExpressionInfo<TInfluxRow>> fields,
         List<PropertyExpressionInfo<TInfluxRow>> all )
      {
         Timestamp = timestamp;
         Tags = new List<PropertyExpressionInfo<TInfluxRow>>( tags.OrderBy( x => x.Key, StringComparer.Ordinal ) );
         Fields = new List<PropertyExpressionInfo<TInfluxRow>>( fields.OrderBy( x => x.Key, StringComparer.Ordinal ) );
         All = new ReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>>( all.ToDictionary( x => x.Key, x => x ) );
         PropertiesByClrName = All.ToDictionary( x => x.Value.Property.Name, x => x.Value );

         var newLambda = Expression.Lambda<Func<TInfluxRow>>( Expression.New( typeof( TInfluxRow ) ), true );
         New = newLambda.Compile();
      }
   }
}
