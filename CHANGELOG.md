# Changelog

## 1.0.7 - Upcoming
 * Improved performance for reading IInfluxRows from the database.
 * Improved perfomance by following guidelines for ordering of tags/fields when sending to data to influxdb

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
