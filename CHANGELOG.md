# Changelog

## 1.0.1 - 2026-06-17

- Renamed the package identity from `com.deucarian.ui-binding.core-state-bridge` to `com.deucarian.ui-binding.core-state-integration`.
- Renamed CoreStateBridge assemblies, namespaces, tests, tools, and samples to CoreStateIntegration.
- Migration: remove the old bridge package ID from Unity manifests and add `com.deucarian.ui-binding.core-state-integration`.

## Unreleased

- Removed the hard compile-time dependency on the new Generic UI Items visual container interface so older cached GenericUIItems installs can still fall back to `ISelectableUIItem`.
- Updated `SelectionUIBinding<TKey, T>` to drive Generic UI Items visual-aware containers when an item visual strategy is configured.
- Kept the existing `ISelectableUIItem` fallback for item-prefab-owned selected visuals.
- Added EditMode coverage for the visual-aware selection binding path.

## 1.0.0

- Initial integration package.
- Added repository-to-container binding.
- Added optional selection-to-UI binding.
- Added basic usage sample scripts.
- Added focused EditMode tests for repository and selection binding.
