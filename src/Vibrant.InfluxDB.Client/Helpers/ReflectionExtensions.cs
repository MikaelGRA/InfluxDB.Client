//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace Vibrant.InfluxDB.Client.Helpers
//{
//   internal static class ReflectionExtensions
//   {
//      public static object GetValue( this MemberInfo member, object instance )
//      {
//         if( member is PropertyInfo )
//         {
//            return ( (PropertyInfo)member ).GetValue( instance, null );
//         }
//         else if( member is FieldInfo )
//         {
//            return ( (FieldInfo)member ).GetValue( instance );
//         }
//         throw new InvalidOperationException();
//      }

//      public static void SetValue( this MemberInfo member, object instance, object value )
//      {
//         if( member is PropertyInfo )
//         {
//            var pi = (PropertyInfo)member;
//            pi.SetValue( instance, value, null );
//         }
//         else if( member is FieldInfo )
//         {
//            var fi = (FieldInfo)member;
//            fi.SetValue( instance, value );
//         }
//         else
//         {
//            throw new InvalidOperationException();
//         }
//      }
//   }
//}
