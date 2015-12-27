using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Helpers
{
   internal static class TypeCache
   {
      private static readonly object _sync = new object();
      private static readonly Dictionary<Type, object> _typeCache = new Dictionary<Type, object>();

      internal static DataPointTypeInfo<TInfluxRow> GetOrCreateTypeCache<TInfluxRow>()
         where TInfluxRow : IInfluxRow, new()
      {
         lock ( _sync )
         {
            object cache;
            var type = typeof( TInfluxRow );

            if ( !_typeCache.TryGetValue( type, out cache ) )
            {
               var tags = new Dictionary<string, PropertyExpressionInfo<TInfluxRow>>( StringComparer.InvariantCultureIgnoreCase );
               var fields = new Dictionary<string, PropertyExpressionInfo<TInfluxRow>>( StringComparer.InvariantCultureIgnoreCase );
               var all = new Dictionary<string, PropertyExpressionInfo<TInfluxRow>>( StringComparer.InvariantCultureIgnoreCase );
               PropertyExpressionInfo<TInfluxRow> timestamp = null;
               foreach ( var propertyInfo in type.GetProperties() )
               {
                  var fieldAttribute = propertyInfo.GetCustomAttribute<InfluxFieldAttribute>();
                  var tagAttribute = propertyInfo.GetCustomAttribute<InfluxTagAttribute>();
                  var timestampAttribute = propertyInfo.GetCustomAttribute<InfluxTimestampAttribute>();
                  var allAttributes = new Attribute[] { fieldAttribute, tagAttribute, timestampAttribute }
                     .Where( x => x != null )
                     .ToList();

                  if ( allAttributes.Count > 1 )
                  {
                     throw new InvalidInfluxQueryableException( "An implementation of IQueryableDataPoint properties can only have one InfluxAttribute." );
                  }

                  if ( timestampAttribute != null )
                  {
                     timestamp = new PropertyExpressionInfo<TInfluxRow>( propertyInfo );
                     all.Add( "time", timestamp );
                  }
                  else if ( fieldAttribute != null )
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>( propertyInfo );
                     fields.Add( fieldAttribute.Name, expression );
                     all.Add( fieldAttribute.Name, expression );
                  }
                  else if ( tagAttribute != null )
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>( propertyInfo );
                     tags.Add( tagAttribute.Name, expression );
                     all.Add( tagAttribute.Name, expression );
                  }
               }

               cache = new DataPointTypeInfo<TInfluxRow>( timestamp, tags, fields, all );

               _typeCache.Add( typeof( TInfluxRow ), cache );
            }
            return (DataPointTypeInfo<TInfluxRow>)cache;
         }
      }
   }
}
