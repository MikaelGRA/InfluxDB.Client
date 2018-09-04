using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Vibrant.InfluxDB.Client.Resources;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Metadata
{
   internal static class MetadataCache
   {
      private static readonly MethodInfo GetOrCreateMethod = typeof(MetadataCache).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Single(x => x.Name == "GetOrCreate" && x.ContainsGenericParameters);

      private static readonly object Sync = new object();
      private static readonly Dictionary<Type, InfluxRowTypeInfo> TypeCache = new Dictionary<Type, InfluxRowTypeInfo>();
      private static readonly HashSet<Type> ValidFieldTypes = new HashSet<Type> { typeof(string), typeof(double), typeof(float), typeof(long), typeof(int), typeof(short), typeof(byte), typeof(ulong), typeof(uint), typeof(ushort), typeof(sbyte), typeof(bool), typeof(DateTime) };
      private static readonly ConcurrentDictionary<Type, Type> TypeDefinitionMap = new ConcurrentDictionary<Type, Type>();

      internal static Type GetGenericTypeDefinitionForImplementedInfluxInterface(Type type)
      {
         Type foundTypeGenericTypeDefinition = null;

         if (!TypeDefinitionMap.TryGetValue(type, out foundTypeGenericTypeDefinition))
         {
            foreach (var i in type.GetInterfaces())
            {
               if (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IInfluxRow<>))
               {
                  if (foundTypeGenericTypeDefinition != null)
                  {
                     throw new InfluxException(string.Format(Errors.MultiInterfaceImplementations, type.FullName));
                  }

                  foundTypeGenericTypeDefinition = i;
               }
            }

            TypeDefinitionMap[type] = foundTypeGenericTypeDefinition;
         }

         return foundTypeGenericTypeDefinition;
      }

      internal static Type GetGenericTypeDefinitionForImplementedInfluxInterface<TInfluxRow>()
      {
         return GetGenericTypeDefinitionForImplementedInfluxInterface(typeof(TInfluxRow));
      }

      internal static InfluxRowTypeInfo GetOrCreate(Type type)
      {
         lock (Sync)
         {
            if (!TypeCache.TryGetValue(type, out InfluxRowTypeInfo cache))
            {
               return (InfluxRowTypeInfo) GetOrCreateMethod.MakeGenericMethod(new[] { type }).Invoke(null, null);
            }

            return cache;
         }
      }

      internal static InfluxRowTypeInfo<TInfluxRow> GetOrCreate<TInfluxRow>()
         where TInfluxRow : new()
      {
         lock (Sync)
         {
            InfluxRowTypeInfo cache;
            var type = typeof(TInfluxRow);

            if (!TypeCache.TryGetValue(type, out cache))
            {
               Type timestampType = null;

               var computed = new List<PropertyExpressionInfo<TInfluxRow>>();
               var tags = new List<PropertyExpressionInfo<TInfluxRow>>();
               var fields = new List<PropertyExpressionInfo<TInfluxRow>>();

               var all = new List<PropertyExpressionInfo<TInfluxRow>>();
               PropertyExpressionInfo<TInfluxRow> timestamp = null;
               PropertyExpressionInfo<TInfluxRow> influxMeasurement = null;

               foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
               {
                  var fieldAttribute = propertyInfo.GetCustomAttribute<InfluxFieldAttribute>();
                  var tagAttribute = propertyInfo.GetCustomAttribute<InfluxTagAttribute>();
                  var computedAttribute = propertyInfo.GetCustomAttribute<InfluxComputedAttribute>();
                  var timestampAttribute = propertyInfo.GetCustomAttribute<InfluxTimestampAttribute>();
                  var influxMeasurementAttribute = propertyInfo.GetCustomAttribute<InfluxMeasurementAttribute>();

                  var notMappedAttribute = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
                  var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                  var keyAttribute = propertyInfo.GetCustomAttribute<KeyAttribute>();

                  // list all attributes so we can ensure the attributes specified on a property are valid
                  var allAttributes = new Attribute[] { fieldAttribute, tagAttribute, timestampAttribute, computedAttribute, notMappedAttribute, columnAttribute, keyAttribute }
                   .Where(x => x != null)
                   .ToList();

                  if (allAttributes.Count > 1)
                  {
                     throw new InfluxException(string.Format(Errors.MultipleAttributesOnSingleProperty, propertyInfo.Name, type.Name));
                  }

                  if (timestampAttribute != null || keyAttribute != null)
                  {
                     timestamp = new PropertyExpressionInfo<TInfluxRow>(InfluxConstants.TimeColumn, propertyInfo);

                     all.Add(timestamp);
                     timestampType = timestamp.RawType;
                  }
                  else if (tagAttribute != null)
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>(tagAttribute.Name, propertyInfo);
                     if (!ValidFieldTypes.Contains(expression.Type) && !expression.Type.GetTypeInfo().IsEnum)
                     {
                        throw new InfluxException(string.Format(Errors.InvalidTagType, propertyInfo.Name, type.Name));
                     }

                     if (string.IsNullOrEmpty(tagAttribute.Name))
                     {
                        throw new InfluxException(string.Format(Errors.InvalidNameProperty, propertyInfo.Name, type.Name));
                     }

                     tags.Add(expression);
                     all.Add(expression);
                  }
                  else if (computedAttribute != null)
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>(computedAttribute.Name, propertyInfo);
                     if (!ValidFieldTypes.Contains(expression.Type) && !expression.Type.GetTypeInfo().IsEnum)
                     {
                        throw new InfluxException(string.Format(Errors.InvalidComputedType, propertyInfo.Name, type.Name));
                     }

                     if (string.IsNullOrEmpty(computedAttribute.Name))
                     {
                        throw new InfluxException(string.Format(Errors.InvalidNameProperty, propertyInfo.Name, type.Name));
                     }

                     computed.Add(expression);
                     all.Add(expression);
                  }
                  else if (influxMeasurementAttribute != null)
                  {
                     var expression = new PropertyExpressionInfo<TInfluxRow>(null, propertyInfo);
                     if (expression.Type != typeof(string))
                     {
                        throw new InfluxException(string.Format(Errors.InvalidMeasurementNameType, propertyInfo.Name, type.Name));
                     }

                     influxMeasurement = expression;
                  }
                  else if (columnAttribute != null)
                  {
                     var name = !string.IsNullOrWhiteSpace(columnAttribute.Name) ? columnAttribute.Name : propertyInfo.Name;

                     var expression = new PropertyExpressionInfo<TInfluxRow>(name, propertyInfo);
                     if (!ValidFieldTypes.Contains(expression.Type) && !expression.Type.GetTypeInfo().IsEnum)
                     {
                        throw new InfluxException(string.Format(Errors.InvalidFieldType, propertyInfo.Name, type.Name));
                     }

                     fields.Add(expression);
                     all.Add(expression);
                  }
                  else if (fieldAttribute != null || notMappedAttribute == null)
                  {
                     if (fieldAttribute != null && string.IsNullOrEmpty(fieldAttribute.Name))
                     {
                        throw new InfluxException(string.Format(Errors.InvalidNameProperty, propertyInfo.Name, type.Name));
                     }

                     var name = fieldAttribute?.Name ?? propertyInfo.Name;

                     var expression = new PropertyExpressionInfo<TInfluxRow>(name, propertyInfo);
                     if (!ValidFieldTypes.Contains(expression.Type) && !expression.Type.GetTypeInfo().IsEnum)
                     {
                        throw new InfluxException(string.Format(Errors.InvalidFieldType, propertyInfo.Name, type.Name));
                     }

                     fields.Add(expression);
                     all.Add(expression);
                  }
               }

               bool isBasedOnInterface = false;
               var genericTypeDefinitionOfImplementedInterface = GetGenericTypeDefinitionForImplementedInfluxInterface(type);
               if (genericTypeDefinitionOfImplementedInterface != null)
               {
                  timestampType = genericTypeDefinitionOfImplementedInterface.GetGenericArguments()[0];
                  isBasedOnInterface = true;
               }
               else if (timestampType == null)
               {
                  timestampType = typeof(NullTimestamp);
               }

               cache = (InfluxRowTypeInfo<TInfluxRow>) typeof(InfluxRowTypeInfo<,>)
                  .MakeGenericType(new[] { typeof(TInfluxRow), timestampType })
                  .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0]
                  .Invoke(new object[] { isBasedOnInterface, timestamp, tags, fields, computed, all, influxMeasurement });
               TypeCache.Add(typeof(TInfluxRow), cache);
            }
            return (InfluxRowTypeInfo<TInfluxRow>) cache;
         }
      }
   }
}
