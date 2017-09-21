# Changelog
## 3.0.4
 * Fixed issue #12
 * Fixed internal tests so they no longer collide

## 3.0.3
 * Fixed chunking support. #11 
 * Changed newtonsoft json dependency to actually existing version.
 * Change InfluxException throwing behaviour such that they are only throw in case of non-200 status code or other internal exception

## 3.0.2 
 * Added support for RP in write operations. Issue #9.

## 3.0.1
 * Fixed issue #7

## 3.0.0
 * Added support for influxdb 1.0

## 2.1.0
 * Added support for DELETE queries

## 2.0.0
 * Support for influxdb v 0.13

## 1.0.7
 * Improved performance for reading IInfluxRows from the database.
 * Improved perfomance by following guidelines for ordering of tags/fields when sending to data to influxdb
 * Changed Newtonsoft.Json requirement to 7.0.1

## 1.0.6
 * Additional target frameworks

## 1.0.5
 * Added new overloads of ShowMeasurements by splitting them into two different methods

## 1.0.4
 * Nuget package metadata update
 * AssemblyInfo fixes

## 1.0.2
 * Fixes to DateTime handling. It could occur that strings formatted as DateTimes, when read from InfluxDB, would be treated as DateTimes
 * Fixes a bug that could cause an unexpected exception when using IInfluxRow, if no fields/tags were present for a type of measurement
 * Now allows use of internal classes/properties for POCO classes used for queries
 * Fixes a lot of issues with the XML documentation

## 1.0.1
 * Much improved error messages
 * Fixed an issue that could cause a deadlock in certain situations

## 1.0.0 - Initial release
 * Initial InfluxClient implementation.
