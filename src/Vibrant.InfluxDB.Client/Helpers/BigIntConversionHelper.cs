using System;
using System.Numerics;

namespace Vibrant.InfluxDB.Client.Helpers
{
    internal static class BigIntConversionHelper
    {
        public static object ChangeType(BigInteger value, Type conversionType)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException(nameof(conversionType));
            }

            switch (Type.GetTypeCode(conversionType))
            {
                case TypeCode.Boolean:
                    return value == 1;

                case TypeCode.Char:
                    return (char)value;

                case TypeCode.SByte:
                    return (sbyte)value;

                case TypeCode.Byte:
                    return (byte)value;

                case TypeCode.Int16:
                    return (short)value;

                case TypeCode.UInt16:
                    return (ushort)value;

                case TypeCode.Int32:
                    return (int)value;

                case TypeCode.UInt32:
                    return (uint)value;

                case TypeCode.Int64:
                    return (long)value;

                case TypeCode.UInt64:
                    return (ulong)value;

                case TypeCode.Single:
                    return (float)value;

                case TypeCode.Double:
                    return (double)value;

                case TypeCode.Decimal:
                    return (decimal)value;

                case TypeCode.String:
                    return value.ToString();

                default:
                    throw new NotSupportedException($"Not supported data type: {conversionType}");
            }
        }
    }
}