# Deucarian UI Binding Core State Integration

## What this is

`com.deucarian.ui-binding.core-state-integration` is a small Unity UPM package that connects two standalone packages:

- `com.deucarian.ui-binding`
- `com.deucarian.core-state`

The package exists so UI Binding can stay focused on UI item presentation, and Core State can stay focused on repository and selection state. Neither core package references this integration package or each other.

Current package version: `1.0.3`.

## When to use it

- Your project already uses UI Binding and Core State.
- You want a reusable adapter from `IReadOnlyRepository<TKey, T>` to `IUIBindingContainer<T, TKey>`.
- You want Core State selection reflected into UI Binding selection visuals.

## When not to use it

- Do not use this package without both target packages installed.
- Do not put repository ownership, UI item rendering, networking, persistence, MVVM, pooling, or virtualization here.
- Do not use this package as an app-specific UI architecture layer.

Migration note: replace old manifest entries for `com.deucarian.ui-binding.core-state-bridge` with `com.deucarian.ui-binding.core-state-integration`. Current installs use the `UIBinding-CoreState-Integration.git` repository.

## Install

Install this package when a project uses both UI Binding and Core State and wants a reusable integration between repositories, selection services, and UI containers.

Stable:

```json
"com.deucarian.ui-binding.core-state-integration": "https://github.com/Deucarian/UIBinding-CoreState-Integration.git#main"
```

Development:

```json
"com.deucarian.ui-binding.core-state-integration": "https://github.com/Deucarian/UIBinding-CoreState-Integration.git#develop"
```

For local development, reference all three packages by file path from a separate Unity project:

```json
{
  "dependencies": {
    "com.deucarian.ui-binding": "file:C:/Repositories/UI-Binding",
    "com.deucarian.core-state": "file:C:/Repositories/Core-State",
    "com.deucarian.ui-binding.core-state-integration": "file:C:/Repositories/UIBinding-CoreState-Integration"
  }
}
```

## Dependencies

- `com.deucarian.ui-binding` `1.1.0` supplies the UI container and item contracts.
- `com.deucarian.core-state` `1.0.1` supplies repository and selection-service contracts.

Neither UI Binding nor Core State depends on this integration package.

## Unity compatibility

Requires Unity 2021.3 or newer.

## Repository Binding

`RepositoryUIBinding<TKey, T>` keeps an `IReadOnlyRepository<TKey, T>` synchronized with an `IUIBindingContainer<T, TKey>`.

```csharp
using Deucarian.CoreState;
using Deucarian.UIBinding;
using Deucarian.UIBinding.CoreStateIntegration;

IReadOnlyRepository<string, ProjectData> repository = projectRepository;
IUIBindingContainer<ProjectData, string> container = projectContainer;

var binding = new RepositoryUIBinding<string, ProjectData>(
    repository,
    container);

binding.Bind();

// Later, when the owning view is destroyed or disabled:
binding.Dispose();
```

Binding behavior:

- `Bind` subscribes to repository events and performs an initial `ReplaceAll(repository.Items)`.
- `ItemAdded` adds the UI item, or updates it if the container already has the key.
- `ItemUpdated` updates the UI item, or adds it if the container is missing the key.
- `ItemRemoved` removes the UI item by key.
- `Cleared` clears the UI container.
- `Unbind` and `Dispose` are idempotent.

The caller owns binding lifetime. There are no static caches, global discovery hooks, service locators, or hidden state.

## Selection Binding

`SelectionUIBinding<TKey, T>` reflects `ISelectionService<TKey, T>` state into UI Binding selection visuals when the container has an `IUIBindingItemVisual<TKey, T>` configured. If no visual strategy is configured, it falls back to UI item components that implement `ISelectableUIItem`.

```csharp
using Deucarian.UIBinding.CoreStateIntegration;

public sealed class ProjectItem : GenericItem<ProjectData>, ISelectableUIItem
{
    public void SetSelected(bool selected)
    {
        // Update highlight, checkmark, or selected visual state.
    }
}

var selectionBinding = new SelectionUIBinding<string, ProjectData>(
    selectionService,
    container);

selectionBinding.Bind();
```

With the visual strategy API, item components no longer need to own selection visuals:

```csharp
container.SetItemVisual(new GraphicTintUIBindingItemVisual<string, ProjectData>(
    normalColor,
    selectedColor,
    hoveredColor));

selectionBinding.Bind();
```

Bind repository UI before selection UI when both are used:

```csharp
repositoryBinding.Bind();
selectionBinding.Bind();
```

Click-to-select behavior belongs in the item or view code that owns the concrete UI. The sample demonstrates this with a local `Button` listener that calls `selection.TrySelect(data.Id)`.

## Public API

- `RepositoryUIBinding<TKey, T>`: binds Core State repository changes to a UI Binding container.
- `SelectionUIBinding<TKey, T>`: applies Core State selection changes to UI Binding visual containers or selectable UI items.
- `ISelectableUIItem`: optional UI item contract for selected visual state.

## Samples

The package contains one sample entry:

- `Basic Usage`: sample scripts for a repository-backed UI list with add, update, remove, clear, reset, and click-to-select behavior.

The sample uses fake local data only and does not use API, networking, global state, or app-specific services.

## Validation

Run the shared package validator from the repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

For Unity EditMode tests, create or open a separate Unity project that references the three local packages by file path, then run tests for `Deucarian.UIBinding.CoreStateIntegration.Tests`.

Package tests are compiled only when the project manifest marks this package as testable:

```json
"testables": [
  "com.deucarian.ui-binding.core-state-integration"
]
```

Recommended playground path:

```text
C:/Repositories/UIBinding-CoreState-Integration-TestProject
```

Do not copy source code between repositories. Consume the packages through Unity Package Manager file references.

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Limitations

- This package is integration code only.
- It does not provide networking, persistence, MVVM, reactive frameworks, service location, pooling, virtualization, or app-specific UI architecture.
- Selection visuals can be handled by a UI Binding visual strategy or, for older item prefabs, by item components that implement `ISelectableUIItem`.

## Architecture / Contributor Notes

- [AGENTS.md](AGENTS.md) contains repository-specific ownership and Codex guidance.
- Deucarian architecture rules live in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md).
- Capability ownership is tracked in [CAPABILITY_OWNERSHIP.md](https://github.com/Deucarian/Package-Registry/blob/develop/CAPABILITY_OWNERSHIP.md).

## License

See [LICENSE.md](LICENSE.md).

## Quick Start

1. Install the package through Deucarian Package Installer or Unity Package Manager using the URL above.
2. Let Unity finish resolving packages and compiling assemblies.
3. Import the `Basic Usage` sample if you want a working reference scene or setup.
4. Start from the package README sections above and the public runtime/editor APIs in this repository.

## Troubleshooting

- Package does not resolve: confirm the stable or development Git URL matches the Package Registry entry and that required Deucarian dependencies are installed.
- Unity compile errors after install: let Package Manager finish resolving dependencies, then check asmdef references against `package.json` dependencies.
- Behavior appears to belong in another package: consult `AGENTS.md` and the Package Registry governance docs before moving or duplicating code.
