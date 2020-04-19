using System;

namespace Vibrant.InfluxDB.Client.Tests
{
    internal class SimpleJsonRow
    {
        [InfluxTimestamp]
        internal DateTime? Timestamp { get; set; }

        [InfluxField("value")]
        internal string Value { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            var other = obj as SimpleJsonRow;
            if (other == null)
                return false;

            return Timestamp == other.Timestamp
               && Value == other.Value;
        }
    }
}
