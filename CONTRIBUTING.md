# Contributing

## Scope

Deucarian UI Binding Core State Integration is integration code between UI Binding and Core State.

Keep UI Binding independent from Core State. Keep Core State independent from UI Binding. Do not add references from either core package back to this integration package.

Do not add networking, API, MVVM, reactive framework dependencies, service locators, hidden global state, or app-specific behavior.

## Local Validation

Run structural validation from the package root:

```powershell
pwsh ./Tools/Validate-Package.ps1
```

For Unity validation, use a separate Unity test project that references the local packages by file path:

```json
"com.deucarian.ui-binding": "file:C:/Repositories/UIBinding",
"com.deucarian.core-state": "file:C:/Repositories/Core-State",
"com.deucarian.ui-binding.core-state-integration": "https://github.com/Deucarian/UI-Binding-CoreState-Bridge.git#main"
```

Also add the integration package to the project manifest `testables` array when running package tests:

```json
"testables": [
  "com.deucarian.ui-binding.core-state-integration"
]
```

Package source should stay in this repository. Do not copy package code into the test project.

## Pull Requests

- Keep runtime changes focused on integration behavior.
- Add or update EditMode tests for behavior changes.
- Update README and CHANGELOG for public API changes.
- Keep runtime asmdef free of editor-only references.
- Do not add dependencies beyond the two core packages unless the integration actually requires them.
