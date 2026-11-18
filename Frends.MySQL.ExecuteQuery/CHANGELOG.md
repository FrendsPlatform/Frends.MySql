# Changelog

## [2.0.0] - 2026-11-11
### Changed
- **[Breaking Change]** Moved `ConnectionString` from `Input` to a new `Connection` parameter. The task now takes `Input`, `Connection` and `Options` parameters.
- **[Breaking Change]** Renamed the `QueryInput` parameter to `Input`.
- **[Breaking Change]** Errors are now handled according to the new `Options.ThrowErrorOnFailure` setting, and the original exception is thrown instead of a wrapped one.
- Added `Options.ThrowErrorOnFailure` (default `true`) and `Options.ErrorMessageOnFailure` for customizing error handling.
- Added `Error` to the task result, which is populated when the task fails and `ThrowErrorOnFailure` is `false`.
- Updated the task to target .NET 8.

## [1.4.0] - 2025-12-19
### Fixed
- Allow defining execute type.
- Fix the issue with respecting timeout.

## [1.3.0] - 2025-09-22
### Fixed
- Respect Options.TimeoutSeconds in MySql commands

## [1.2.0] - 2025-03-18
### Updated
- Update packages:
  Dapper          2.1.28 -> 2.1.66
  MySqlConnector  2.3.5  -> 2.4.0
  coverlet.collector      3.1.2  -> 6.0.4
  Microsoft.NET.Test.Sdk  17.1.0 -> 17.13.0
  NUnit                   3.13.3 -> 4.3.2
  NUnit.Analyzers         3.3.0  -> 4.6.0
  NUnit3TestAdapter       4.2.1  -> 5.0.0  

## [1.1.0] - 2024-03-07
### Updated
- Changed MySqlData -library to MySqlConnector -library.

## [1.0.1] - 2023-01-02
### Fixed
- Fixed issue with Task's result couldn't be referenced with dot notation by changing the ResultJtoken to dynamic.
- Changed the Newtonsoft.Json version to 12.0.1 to be same what Frends is using.

## [1.0.0] - 2022-10-12
### Added
- Initial implementation