# Deucarian UI Binding Core State Integration

## Overview

Deucarian UI Binding Core State Integration is a small Unity UPM package that connects two standalone packages:

- `com.deucarian.ui-binding`
- `com.deucarian.core-state`

The package exists so UI Binding can stay focused on UI item presentation, and Core State can stay focused on repository and selection state. Neither core package references this integration package or each other.

Package ID: `com.deucarian.ui-binding.core-state-integration`

Migration note: replace old manifest entries for `com.deucarian.ui-binding.core-state-bridge` with `com.deucarian.ui-binding.core-state-integration`. Current installs use the `UIBinding-CoreState-Integration.git` repository.

## Installation

Install this package when a project uses both UI Binding and Core State and wants a reusable integration between repositories, selection services, and UI containers.

For local development, reference all three packages by file path from a separate Unity project:

```json
{
  "dependencies": {
    "com.deucarian.ui-binding": "file:C:/Repositories/UIBinding",
    "com.deucarian.core-state": "file:C:/Repositories/Core-State",
    "com.deucarian.ui-binding.core-state-integration": "https://github.com/Deucarian/UIBinding-CoreState-Integration.git#main"
  }
}
```

The package requires Unity `2021.3` or newer.

## Dependencies

- `com.deucarian.ui-binding` `1.1.0` supplies the UI container and item contracts.
- `com.deucarian.core-state` supplies repository and selection-service contracts.

Neither UI Binding nor Core State depends on this integration package.

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

## Local Validation

Run structural validation from the package root:

```powershell
pwsh ./Tools/Validate-Package.ps1
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

## Documentation Policy

Public API changes require README updates, changelog entries, and focused EditMode tests.

## Limitations

- This package is integration code only.
- It does not provide networking, persistence, MVVM, reactive frameworks, service location, pooling, virtualization, or app-specific UI architecture.
- Selection visuals can be handled by a UI Binding visual strategy or, for older item prefabs, by item components that implement `ISelectableUIItem`.
