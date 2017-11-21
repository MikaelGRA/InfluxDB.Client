using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Resources
{
   internal static class Errors
   {
      internal static readonly string CouldNotParseEnum = "Could not parse an incoming value to an enum for the property {0} on the type {1}. The value was {2}.";
      internal static readonly string CountNotConvertEnumToString = "Could not convert the incominng value {0} to the enum on the property {1} on the type {2}.";
      internal static readonly string IndeterminateColumns = "Could not determine which columns in the returned data are tags and which are fields.";
      internal static readonly string InvalidFieldType = "The property {0} on the type {1} which is used as an InfluxField must be one of the following types: string, double, long, int, short, byte, ulong, uint, ushort, sbyte, bool, DateTime, Nullable<double>, Nullable<long>, Nullable<int>, Nullable<short>, Nullable<byte>, Nullable<ulong>, Nullable<uint>, Nullable<ushort>, Nullable<sbyte>, Nullable<bool>, Nullable<DateTime> or a user-defined enum.";
      internal static readonly string InvalidComputedType = "The property {0} on the type {1} which is used as an InfluxComputed must be one of the following types: string, double, long, bool, DateTime, Nullable<double>, Nullable<long>, Nullable<bool>, Nullable<DateTime> or a user-defined enum.";
      internal static readonly string InvalidNameProperty = "The property {0} on the type {1} must specify a non-empty name for either an InfluxField, InfluxTag or InfluxComputed.";
      internal static readonly string InvalidTagType = "The property {0} on the type {1} which is used as an InfluxTag must be either a string or a user-defined enum.";
      internal static readonly string MultipleAttributesOnSingleProperty = "The property {0} on the type {1} has multiple InfluxAttributes. This is not allowed. Please specify only InfluxTimestamp, InfluxTag or InfluxField.";
      internal static readonly string ParsingError = "An error occurred while parsing the error response after an unsuccessful request.";
      internal static readonly string UnexpectedQueryResult = "No measurements were returned in the query. Likely because the measurement does not exist, no data exists for the queried period or because there was an error in the identifiers used in the query.";
      internal static readonly string UnknownError = "An unknown error occurred. Please inspect the inner exception.";
      internal static readonly string InvalidColumn = "Could not determine whether the column '{0}' is a tag, field or a timestamp.";
      internal static readonly string InvalidTagOrFieldName = "The name of a field or tag cannot contain newline characters.";
      internal static readonly string MultiInterfaceImplementations = "The class '{0}' implements the interface 'IIinfluxRow<>' multiple times with different generic parameters.";
      internal static readonly string CouldNotFindTimestampParser = "Could not find a registered timestamp parser for the type '{0}'.";
      internal static readonly string CouldNotParseTimestamp = "Could not parse the timestamp '{0}'.";
      internal static readonly string MissingOffsetInEpochTime = "Cannot parse an epoch time to a DateTimeOffset. Consider not specifying a TimestampPrecision in the InfluxQueryOptions when using DateTimeOffset.";
   }
}
