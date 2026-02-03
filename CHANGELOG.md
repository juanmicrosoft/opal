# Changelog

All notable changes to this project will be documented in this file.

## [0.1.4] - 2025-02-03

### Added
- **Multi-AI support**: Added support for GitHub Copilot, OpenAI Codex, and Google Gemini CLI
  - `opalc init --ai github` for GitHub Copilot
  - `opalc init --ai codex` for OpenAI Codex
  - `opalc init --ai gemini` for Google Gemini
- **Solution-level initialization**: `opalc init` now works on solution folders, initializing all projects
- Enum support for C# to OPAL conversion
- Support for explicit enum values and underlying types
- OPAL syntax: `§ENUM{id:Name}` and `§ENUM{id:Name:underlyingType}`
- Type mappings for DateTime, Guid, and read-only collections (ReadList, ReadDict)
- Comprehensive NuGet package metadata (authors, tags, repository URL, license)
- CHANGELOG.md for tracking version history

### Changed
- Renamed to "Optimized Programming for Agents Language"
- Documentation links now point to https://juanrivera.github.io/opal
- Updated documentation to reflect current feature support status
- Fixed Claude skills directory structure to match Codex/Gemini pattern

### Fixed
- Clarified that `opalc init` should be run in a folder with a C# project or solution

## [0.1.3] - Previous Release
- Claude Code hooks for OPAL-first enforcement
- Initial AI integration with Claude

## [0.1.0] - Initial Release
- Initial public release of OPAL compiler
