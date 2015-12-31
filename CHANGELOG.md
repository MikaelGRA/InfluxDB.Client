# Changelog

## 1.0.2 - Upcoming release
 * Fixes to DateTime handling. It could occur that strings formatted as DateTimes, when read from InfluxDB, would be treated as DateTimes
 * Fixes a bug that could cause an unexpected exception when using IInfluxRow, if no fields/tags were present for a type of measurement

## 1.0.1
 * Much improved error messages
 * Fixed an issue that could cause a deadlock in certain situations

## 1.0.0 - Initial release
 * Initial InfluxClient implementation.
