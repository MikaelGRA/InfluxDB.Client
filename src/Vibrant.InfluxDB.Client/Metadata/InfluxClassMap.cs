using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Vibrant.InfluxDB.Client.Helpers;

namespace Vibrant.InfluxDB.Client.Metadata
{
    public class InfluxClassMap
    {
        private static Dictionary<Type, InfluxClassMap> __registry;

        protected InfluxMeasurementAttribute _measurementAttribute = null;
        protected Dictionary<PropertyInfo, Attribute> _propertyMappings;
        static InfluxClassMap()
        {
            __registry = new Dictionary<Type, InfluxClassMap>();
        }

        public InfluxClassMap()
        {
            _propertyMappings = new Dictionary<PropertyInfo, Attribute>();
        }

        public static void Register<TClass>(Action<InfluxClassMap<TClass>> mapper)
        {
            var classMap = new InfluxClassMap<TClass>();
            mapper(classMap);
            __registry.Add(typeof(TClass), classMap);
        }
        public static InfluxMeasurementAttribute GetMeasurementAttribute(Type classType)
        {
            try
            {
                var classMap = __registry[classType];
                return classMap._measurementAttribute;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
        public static TAttribute GetMappedAttribute<TAttribute>(PropertyInfo propertyInfo) where TAttribute : class
        {
            try
            {
                var classMap = __registry[propertyInfo.DeclaringType];
                var attribute = classMap._propertyMappings[propertyInfo];
                return attribute as TAttribute;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public virtual void SetMeasurementName(string name)
        {
            _measurementAttribute = new InfluxMeasurementAttribute(name);
        }

        // public virtual void MapTimestamp<T>(Expression<Func<T, object>> property)
        // {
        //     var properyInfo = ExpressionHelpers.GetPropertyInfo(property);
        //     _propertyMappings.Add(properyInfo, new InfluxTimestampAttribute());
        // }

        // public virtual void MapTag<T>(Expression<Func<T, object>> property, string tag)
        // {
        //     var properyInfo = ExpressionHelpers.GetPropertyInfo(property);
        //     _propertyMappings.Add(properyInfo, new InfluxTagAttribute(tag));
        // }
        // private virtual void MapField<T>(Expression<Func<T, object>> property, string field)
        // {
        //     var properyInfo = ExpressionHelpers.GetPropertyInfo(property);
        //     _propertyMappings.Add(properyInfo, new InfluxFieldAttribute(field));
        // }
    }

    public class InfluxClassMap<T> : InfluxClassMap
    {
        public void MapTimestamp(Expression<Func<T, object>> property)
        {
            var properyInfo = ExpressionHelpers.GetPropertyInfo(property);
            _propertyMappings.Add(properyInfo, new InfluxTimestampAttribute());
        }
        public void MapTag(Expression<Func<T, object>> property, string name)
        {
            var properyInfo = ExpressionHelpers.GetPropertyInfo(property);
            _propertyMappings.Add(properyInfo, new InfluxTagAttribute(name));
        }
        public void MapField(Expression<Func<T, object>> property, string name)
        {
            var properyInfo = ExpressionHelpers.GetPropertyInfo(property);
            _propertyMappings.Add(properyInfo, new InfluxFieldAttribute(name));
        }
        public void MapComputed(Expression<Func<T, object>> property, string name)
        {
            var properyInfo = ExpressionHelpers.GetPropertyInfo(property);
            _propertyMappings.Add(properyInfo, new InfluxComputedAttribute(name));
        }
        public void MapMeasurement(Expression<Func<T, object>> property)
        {
            var properyInfo = ExpressionHelpers.GetPropertyInfo(property);
            _propertyMappings.Add(properyInfo, new InfluxMeasurementAttribute());
        }
    }
}