# Basic Usage

This sample shows why the bridge package exists:

- Core State owns repository and selection data.
- UI Binding owns UI item creation and synchronization.
- The bridge package owns the small bridge between them.

Add `CoreStateUIBindingSample` to an empty GameObject in a scene. If no UI references are assigned, the sample creates a simple UGUI layout at runtime with fake local data.

The sample supports:

- Adding repository items.
- Updating the selected repository item.
- Removing the selected repository item.
- Clearing and resetting the repository.
- Clicking a UI item to call `SelectionService.TrySelect`.

No networking, API, service locator, or app-specific state is used.
