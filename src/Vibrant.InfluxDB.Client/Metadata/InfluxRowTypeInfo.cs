using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Http;
using Vibrant.InfluxDB.Client.Resources;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Metadata
{
   internal abstract class InfluxRowTypeInfo
   {
      public abstract Type GetTimestampType();

      public abstract bool IsBasedOnInterface();

      public abstract HttpContent CreateHttpContentFor( InfluxClient client, IEnumerable rows, string measurementName, InfluxWriteOptions options );
   }

   internal abstract class InfluxRowTypeInfo<TInfluxRow> : InfluxRowTypeInfo
   {
      internal readonly Func<TInfluxRow> New;
      internal readonly PropertyExpressionInfo<TInfluxRow> Timestamp;
      internal readonly PropertyExpressionInfo<TInfluxRow> InfluxMeasurement;
      internal readonly IReadOnlyList<PropertyExpressionInfo<TInfluxRow>> Tags;
      internal readonly IReadOnlyList<PropertyExpressionInfo<TInfluxRow>> Fields;
      internal readonly IReadOnlyList<PropertyExpressionInfo<TInfluxRow>> Computed;
      internal readonly IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> All;
      internal readonly IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> PropertiesByClrName;
      internal readonly bool ImplementsIHaveMeasurementName;
      internal readonly string ImplicitMeasurementName;
      internal readonly Func<TInfluxRow, string> GetFallbackMeasurementName;

      internal InfluxRowTypeInfo(
         PropertyExpressionInfo<TInfluxRow> timestamp,
         List<PropertyExpressionInfo<TInfluxRow>> tags,
         List<PropertyExpressionInfo<TInfluxRow>> fields,
         List<PropertyExpressionInfo<TInfluxRow>> computed,
         List<PropertyExpressionInfo<TInfluxRow>> all,
         PropertyExpressionInfo<TInfluxRow> influxMeasurement )
      {
         Timestamp = timestamp;
         Tags = new List<PropertyExpressionInfo<TInfluxRow>>( tags.OrderBy( x => x.Key, StringComparer.Ordinal ) );
         Fields = new List<PropertyExpressionInfo<TInfluxRow>>( fields.OrderBy( x => x.Key, StringComparer.Ordinal ) );
         Computed = new List<PropertyExpressionInfo<TInfluxRow>>( computed.OrderBy( x => x.Key, StringComparer.Ordinal ) );
         All = new ReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>>( all.ToDictionary( x => x.Key, x => x ) );
         PropertiesByClrName = All.ToDictionary( x => x.Value.Property.Name, x => x.Value );
         InfluxMeasurement = influxMeasurement;

         var newLambda = Expression.Lambda<Func<TInfluxRow>>( Expression.New( typeof( TInfluxRow ) ), true );
         New = newLambda.Compile();

         ImplementsIHaveMeasurementName = typeof( TInfluxRow ).GetInterfaces().Any( x => x == typeof( IHaveMeasurementName ) );
         var attr = typeof( TInfluxRow ).GetTypeInfo().GetCustomAttribute<InfluxMeasurementAttribute>();
         if( attr != null )
         {
            ImplicitMeasurementName = attr.Name;
         }

         // interface
         Func<TInfluxRow, string> try1 = null;
         if( ImplementsIHaveMeasurementName )
         {
            try1 = row => ( (IHaveMeasurementName)row ).MeasurementName;
         }

         // property with InfluxMeasurementAttribute?
         Func<TInfluxRow, string> try2 = null;
         if( InfluxMeasurement != null )
         {
            try2 = row => (string)InfluxMeasurement.GetValue( row );
         }

         // class with InfluxMeasurementAttribute?
         Func<TInfluxRow, string> try3 = null;
         if( ImplicitMeasurementName != null )
         {
            try3 = row => ImplicitMeasurementName;
         }

         GetFallbackMeasurementName = row => try1?.Invoke( row ) ?? try2?.Invoke( row ) ?? try3?.Invoke( row );
      }

      public void SetMeasurementName( string measurementName, TInfluxRow row )
      {
         if( ImplementsIHaveMeasurementName )
         {
            ( (IHaveMeasurementName)row ).MeasurementName = measurementName;
         }
         if( InfluxMeasurement?.SetValue != null )
         {
            InfluxMeasurement.SetValue( row, measurementName );
         }
      }

      public Func<TInfluxRow, string> CreateGetMeasurementNameFunction( string measurementName )
      {
         if( measurementName != null )
         {
            return new Func<TInfluxRow, string>( row => measurementName );
         }
         else
         {
            return new Func<TInfluxRow, string>( row => GetFallbackMeasurementName( row ) ?? throw new InfluxException( Errors.CouldNotDetermineMeasurementName ) );
         }
      }
   }

   internal class InfluxRowTypeInfo<TInfluxRow, TTimestamp> : InfluxRowTypeInfo<TInfluxRow>
      where TInfluxRow : new()
   {
      private bool _isBasedOnInterface;

      internal InfluxRowTypeInfo( bool isBasedOnInterface, PropertyExpressionInfo<TInfluxRow> timestamp, List<PropertyExpressionInfo<TInfluxRow>> tags, List<PropertyExpressionInfo<TInfluxRow>> fields, List<PropertyExpressionInfo<TInfluxRow>> computed, List<PropertyExpressionInfo<TInfluxRow>> all, PropertyExpressionInfo<TInfluxRow> influxMeasurement ) : base( timestamp, tags, fields, computed, all, influxMeasurement )
      {
         _isBasedOnInterface = isBasedOnInterface;
      }

      public override HttpContent CreateHttpContentFor( InfluxClient client, IEnumerable rows, string measurementName, InfluxWriteOptions options )
      {
         return new InfluxRowContent<TInfluxRow, TTimestamp>( client, _isBasedOnInterface, rows, CreateGetMeasurementNameFunction( measurementName ), options );
      }

      public override bool IsBasedOnInterface()
      {
         return _isBasedOnInterface;
      }

      public override Type GetTimestampType()
      {
         return typeof( TTimestamp );
      }
   }
}
