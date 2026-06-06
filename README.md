# JorisHoef Generic UI Items Core State Bridge

## Overview

JorisHoef Generic UI Items Core State Bridge is a small Unity UPM package that connects two standalone packages:

- `com.jorishoef.generic-ui-items`
- `com.jorishoef.core.state`

The package exists so Generic UI Items can stay focused on UI item presentation, and Core State can stay focused on repository and selection state. Neither core package references this bridge package or each other.

Package ID: `com.jorishoef.generic-ui-items.core-state-bridge`

## Installation

Install this package when a project uses both Generic UI Items and Core State and wants a reusable bridge between repositories, selection services, and UI containers.

For local development, reference all three packages by file path from a separate Unity project:

```json
{
  "dependencies": {
    "com.jorishoef.generic-ui-items": "file:C:/Repositories/GenericUIItems",
    "com.jorishoef.core.state": "file:C:/Repositories/Core-State",
    "com.jorishoef.generic-ui-items.core-state-bridge": "https://github.com/JorisHoef/GenericUIItems-CoreState-Bridge.git#main"
  }
}
```

The package requires Unity `2021.3` or newer.

## Repository Binding

`RepositoryUIBinding<TKey, T>` keeps an `IReadOnlyRepository<TKey, T>` synchronized with an `IGenericUIContainer<T, TKey>`.

```csharp
using JorisHoef.Core.State;
using JorisHoef.GenericUIItems;
using JorisHoef.GenericUIItems.CoreState;

IReadOnlyRepository<string, ProjectData> repository = projectRepository;
IGenericUIContainer<ProjectData, string> container = projectContainer;

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

`SelectionUIBinding<TKey, T>` reflects `ISelectionService<TKey, T>` state into Generic UI Items selection visuals when the container has an `IGenericUIItemVisual<TKey, T>` configured. If no visual strategy is configured, it falls back to UI item components that implement `ISelectableUIItem`.

```csharp
using JorisHoef.GenericUIItems.CoreState;

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
container.SetItemVisual(new GraphicTintGenericUIItemVisual<string, ProjectData>(
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

- `RepositoryUIBinding<TKey, T>`: binds Core State repository changes to a Generic UI Items container.
- `SelectionUIBinding<TKey, T>`: applies Core State selection changes to Generic UI Items visual containers or selectable UI items.
- `ISelectableUIItem`: optional UI item contract for selected visual state.

## Samples

The package contains one sample entry:

- `Basic Usage`: sample scripts for a repository-backed UI list with add, update, remove, clear, reset, and click-to-select behavior.

The sample uses fake local data only and does not use APIHelper, networking, global state, or app-specific services.

## Local Validation

Run structural validation from the package root:

```powershell
pwsh ./Tools/Validate-Package.ps1
```

For Unity EditMode tests, create or open a separate Unity project that references the three local packages by file path, then run tests for `GenericUIItems.CoreState.Bridge.Tests`.

Package tests are compiled only when the project manifest marks this package as testable:

```json
"testables": [
  "com.jorishoef.generic-ui-items.core-state-bridge"
]
```

Recommended playground path:

```text
C:/Repositories/GenericUIItems-CoreState-Bridge-TestProject
```

Do not copy source code between repositories. Consume the packages through Unity Package Manager file references.

## Documentation Policy

Public API changes require README updates, changelog entries, and focused EditMode tests.

## Limitations

- This package is bridge code only.
- It does not provide networking, persistence, MVVM, reactive frameworks, service location, pooling, virtualization, or app-specific UI architecture.
- Selection visuals can be handled by a Generic UI Items visual strategy or, for older item prefabs, by item components that implement `ISelectableUIItem`.
