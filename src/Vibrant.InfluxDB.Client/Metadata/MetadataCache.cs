using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Resources;

namespace Vibrant.InfluxDB.Client.Metadata
{
   internal static class MetadataCache
   {
      private static readonly object _sync = new object();
      private static readonly Dictionary<Type, object> _typeCache = new Dictionary<Type, object>();
      private static readonly HashSet<Type> _validFieldTypes = new HashSet<Type> { typeof( string ), typeof( double ), typeof( float ), typeof( long ), typeof( int ), typeof( short ), typeof( byte ), typeof( ulong ), typeof( uint ), typeof( ushort ), typeof( sbyte ), typeof( bool ), typeof( DateTime ) };

      internal static InfluxRowTypeInfo<TInfluxRow> GetOrCreate<TInfluxRow>()
         where TInfluxRow : new()
      {
         lock ( _sync )
         {
            object cache;
            var type = typeof( TInfluxRow );

            if ( !_typeCache.TryGetValue( type, out cache ) )
            {
               var computed = new List<PropertyExpressionInfo<TInfluxRow>>();
               var tags = new List<PropertyExpressionInfo<TInfluxRow>>();
               var fields = new List<PropertyExpressionInfo<TInfluxRow>>();
               var all = new List<PropertyExpressionInfo<TInfluxRow>>();
               PropertyExpressionInfo<TInfluxRow> timestamp = null;
               foreach ( var propertyInfo in type.GetProperties( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ) )
               {
                  var fieldAttribute = propertyInfo.GetCustomAttribute<InfluxFieldAttribute>();
                  var tagAttribute = propertyInfo.GetCustomAttribute<InfluxTagAttribute>();
                  var computedAttribute = propertyInfo.GetCustomAttribute<InfluxComputedAttribute>();
                  var timestampAttribute = propertyInfo.GetCustomAttribute<InfluxTimestampAttribute>();

                  // list all attributes so we can ensure the attributes specified on a property are valid
                  var allAttributes = new Attribute[] { fieldAttribute, tagAttribute, timestampAttribute }
                     .Where( x => x != null )
                     .ToList();

                  if ( allAttributes.Count > 1 )
                  {
                     throw new InfluxException( string.Format( Errors.MultipleAttributesOnSingleProperty, propertyInfo.Name, type.Name ) );
                  }

                  if ( timestampAttribute != null )
                  {
                     timestamp = new PropertyExpressionInfo<TInfluxRow>( InfluxConstants.TimeColumn, propertyInfo );

                     all.Add( timestamp );
                  }
                  else if ( fieldAttribute != null )
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>( fieldAttribute.Name, propertyInfo );
                     if ( !_validFieldTypes.Contains( expression.Type ) && !expression.Type.GetTypeInfo().IsEnum )
                     {
                        throw new InfluxException( string.Format( Errors.InvalidFieldType, propertyInfo.Name, type.Name ) );
                     }

                     if ( string.IsNullOrEmpty( fieldAttribute.Name ) )
                     {
                        throw new InfluxException( string.Format( Errors.InvalidNameProperty, propertyInfo.Name, type.Name ) );
                     }

                     fields.Add( expression );
                     all.Add( expression );
                  }
                  else if ( tagAttribute != null )
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>( tagAttribute.Name, propertyInfo );
                     if ( !_validFieldTypes.Contains( expression.Type ) && !expression.Type.GetTypeInfo().IsEnum )
                     {
                        throw new InfluxException( string.Format( Errors.InvalidTagType, propertyInfo.Name, type.Name ) );
                     }

                     if ( string.IsNullOrEmpty( tagAttribute.Name ) )
                     {
                        throw new InfluxException( string.Format( Errors.InvalidNameProperty, propertyInfo.Name, type.Name ) );
                     }

                     tags.Add( expression );
                     all.Add( expression );
                  }
                  else if( computedAttribute != null )
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>( computedAttribute.Name, propertyInfo );
                     if( !_validFieldTypes.Contains( expression.Type ) && !expression.Type.GetTypeInfo().IsEnum )
                     {
                        throw new InfluxException( string.Format( Errors.InvalidComputedType, propertyInfo.Name, type.Name ) );
                     }

                     if( string.IsNullOrEmpty( computedAttribute.Name ) )
                     {
                        throw new InfluxException( string.Format( Errors.InvalidNameProperty, propertyInfo.Name, type.Name ) );
                     }

                     computed.Add( expression );
                     all.Add( expression );
                  }
               }

               cache = new InfluxRowTypeInfo<TInfluxRow>( timestamp, tags, fields, computed, all );

               _typeCache.Add( typeof( TInfluxRow ), cache );
            }
            return (InfluxRowTypeInfo<TInfluxRow>)cache;
         }
      }
   }
}
