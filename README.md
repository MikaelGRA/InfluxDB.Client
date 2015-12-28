# InfluxDB.Client

This library makes it easy to be a client for InfluxDB on .NET!

## Usage

The library exposes all operations on InfluxDB and can be used for reading/writing data to/from in two primary ways:
 * Using your own POCO classes.
 * Using dynamic classes.

### Using your own POCO classes.

1. Start by defining a class that represents a row in InfluxDB that you want to store.

```c#
   public class ComputerState
   {
      [InfluxTimestamp]
      public DateTime Timestamp { get; set; }

      [InfluxTag( "host" )]
      public string Host { get; set; }

      [InfluxTag( "region" )]
      public string Region { get; set; }

      [InfluxField( "cpu" )]
      public double CPU { get; set; }

      [InfluxField( "ram" )]
      public long RAM { get; set; }
   }
```
