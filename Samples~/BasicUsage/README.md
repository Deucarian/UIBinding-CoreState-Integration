# Basic Usage

This sample shows why the integration package exists:

- Core State owns repository and selection data.
- UI Binding owns UI item creation and synchronization.
- The integration package owns the small integration between them.

Add `CoreStateUIBindingSample` to an empty GameObject in a scene. If no UI references are assigned, the sample creates a simple UGUI layout at runtime with fake local data.

The sample supports:

- Adding repository items.
- Updating the selected repository item.
- Removing the selected repository item.
- Clearing and resetting the repository.
- Clicking a UI item to call `SelectionService.TrySelect`.

No networking, API, service locator, or app-specific state is used.
