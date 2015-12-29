using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Metadata
{
   internal static class MetadataCache
   {
      private static readonly object _sync = new object();
      private static readonly Dictionary<Type, object> _typeCache = new Dictionary<Type, object>();
      private static readonly HashSet<Type> _validFieldTypes = new HashSet<Type> { typeof( string ), typeof( double ), typeof( long ), typeof( bool ), typeof( DateTime ) };

      internal static InfluxRowTypeInfo<TInfluxRow> GetOrCreate<TInfluxRow>()
         where TInfluxRow : new()
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
                     throw new InfluxException( "A property can only have one InfluxAttribute." );
                  }

                  if ( timestampAttribute != null )
                  {
                     timestamp = new PropertyExpressionInfo<TInfluxRow>( "time", propertyInfo );
                     if ( timestamp.Type != typeof( DateTime ) )
                     {
                        throw new InfluxException( $"The property {propertyInfo.Name} on the type {type.Name} must be a DateTime." );
                     }

                     all.Add( "time", timestamp );
                  }
                  else if ( fieldAttribute != null )
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>( fieldAttribute.Name, propertyInfo );
                     if ( !_validFieldTypes.Contains( expression.Type ) && !expression.Type.IsEnum )
                     {
                        throw new InfluxException( $"The property {propertyInfo.Name} on the type {type.Name} must be a string, double, long or bool." );
                     }

                     if ( string.IsNullOrEmpty( fieldAttribute.Name ) )
                     {
                        throw new InfluxException( $"The property {propertyInfo.Name} on the type {type.Name} cannot have an empty InfluxField name." );
                     }

                     fields.Add( fieldAttribute.Name, expression );
                     all.Add( fieldAttribute.Name, expression );
                  }
                  else if ( tagAttribute != null )
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>( tagAttribute.Name, propertyInfo );
                     if ( expression.Type != typeof( string ) && !expression.Type.IsEnum )
                     {
                        throw new InfluxException( $"The property {propertyInfo.Name} on the type {type.Name} must be a string or an enum." );
                     }

                     if ( string.IsNullOrEmpty( tagAttribute.Name ) )
                     {
                        throw new InfluxException( $"The property {propertyInfo.Name} on the type {type.Name} cannot have an empty InfluxTag name." );
                     }

                     tags.Add( tagAttribute.Name, expression );
                     all.Add( tagAttribute.Name, expression );
                  }
               }

               cache = new InfluxRowTypeInfo<TInfluxRow>( timestamp, tags, fields, all );

               _typeCache.Add( typeof( TInfluxRow ), cache );
            }
            return (InfluxRowTypeInfo<TInfluxRow>)cache;
         }
      }
   }
}
