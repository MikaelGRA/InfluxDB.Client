using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Helpers
{
   internal class PropertyExpressionInfo<TInfluxRow>
   {
      internal readonly Action<TInfluxRow, object> SetValue;
      internal readonly Func<TInfluxRow, object> GetValue;
      internal readonly Type Type;
      internal readonly Dictionary<Enum, string> EnumToString;
      internal readonly Dictionary<string, Enum> StringToEnum;

      internal PropertyExpressionInfo( PropertyInfo property )
      {
         // Instance type of target entity class 
         ParameterExpression instanceParam = Expression.Parameter( typeof( TInfluxRow ), "x" );

         // Instance type of target entity class 
         ParameterExpression valueParam = Expression.Parameter( typeof( object ), "y" );

         // instance.Property 
         MemberExpression getProperty = Expression.Property( instanceParam, property );

         // instance.Property = row[property] 
         BinaryExpression assignProperty = Expression.Assign( getProperty, Expression.Convert( valueParam, property.PropertyType ) );

         var getterLambda = Expression.Lambda<Func<TInfluxRow, object>>( Expression.Convert( getProperty, typeof( object ) ), true, instanceParam );
         GetValue = getterLambda.Compile();

         var setterLambda = Expression.Lambda<Action<TInfluxRow, object>>( assignProperty, true, instanceParam, valueParam );
         SetValue = setterLambda.Compile();

         var type = property.PropertyType;
         if ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
         {
            // unwrap the nullable type
            Type = type.GetGenericArguments()[ 0 ];
         }
         else
         {
            Type = type;
         }

         if ( Type.IsEnum )
         {
            EnumToString = new Dictionary<Enum, string>();
            StringToEnum = new Dictionary<string, Enum>();

            var values = Enum.GetValues( Type );
            foreach ( Enum value in values )
            {
               string stringValue = value.ToString();
               var memberInfo = Type.GetMember( stringValue );
               if ( memberInfo != null && memberInfo.Length > 0 )
               {
                  var attribute = memberInfo[ 0 ].GetCustomAttribute<EnumMemberAttribute>();
                  if ( attribute != null )
                  {
                     stringValue = attribute.Value;
                  }
               }

               EnumToString.Add( value, stringValue );
               StringToEnum.Add( stringValue, value );
            }
         }
      }

      internal Enum GetEnumValue( object value )
      {
         // attempt converions
         var valueAsString = value as string;
         if ( valueAsString == null )
         {
            throw new InfluxException( $"Could not parse the value for the enum {Type.Name} because the retrieved value was not a string." );
         }

         // attempt parsing
         Enum valueAsEnum;
         if ( !StringToEnum.TryGetValue( valueAsString, out valueAsEnum ) )
         {
            throw new InfluxException( $"Could not parse the value '{valueAsString}' as the enum {Type.Name}." );
         }

         return valueAsEnum;
      }

      internal string GetStringValue( object value )
      {
         var valueAsString = value as string;
         if ( valueAsString != null )
         {
            return valueAsString;
         }

         var valueAsEnum = value as Enum;
         if ( valueAsEnum == null )
         {
            throw new InfluxException( $"Could not cast the value {value} to an enum." );
         }

         if ( !EnumToString.TryGetValue( valueAsEnum, out valueAsString ) )
         {
            throw new InfluxException( $"Could not convert the value {value} to its string representation for the enum {Type.Name}." );
         }

         return valueAsString;
      }
   }
}
