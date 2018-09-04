using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vibrant.InfluxDB.Client.ConsoleApp
{
   [Table("computerInfo1")]
   public class ComputerInfo
   {
      [Key]
      internal DateTime Timestamp { get; set; }

      [Column("cpu")]
      internal double? CPU { get; set; }

      [Column("ram")]
      internal long RAM { get; set; }

      [NotMapped]
      internal long PrivateData { get; set; }
   }

   public class ComputerInfoResult
   {
      [InfluxTimestamp]
      internal DateTime Timestamp { get; set; }

      [InfluxTag("Host")]
      internal string Host { get; set; }

      [InfluxTag("Region")]
      internal string Region { get; set; }

      [Column("cpu")]
      internal double? CPU { get; set; }

      [InfluxField("ram")]
      internal long RAM { get; set; }
   }

   public class ComputerInfoMeta
   {
      [InfluxField("host")]
      internal string Host { get; set; }

      [InfluxField("region")]
      internal string Region { get; set; }
   }
}
