# InfluxDB.Client

This library makes it easy to be a client for InfluxDB on .NET!

## Usage

The library exposes all operations on InfluxDB and can be used for reading/writing data to/from in two primary ways:
 * Using your own POCO classes.
 * Using dynamic classes.

### Using your own POCO classes.

1. Start by defining a class that represents a row in InfluxDB that you want to store.

