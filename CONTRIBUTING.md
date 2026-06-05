# Contributing

## Scope

JorisHoef Generic UI Items Core State Bridge is bridge code between Generic UI Items and Core State.

Keep Generic UI Items independent from Core State. Keep Core State independent from Generic UI Items. Do not add references from either core package back to this bridge package.

Do not add networking, APIHelper, MVVM, reactive framework dependencies, service locators, hidden global state, or app-specific behavior.

## Local Validation

Run structural validation from the package root:

```powershell
pwsh ./Tools/Validate-Package.ps1
```

For Unity validation, use a separate Unity test project that references the local packages by file path:

```json
"com.jorishoef.generic-ui-items": "file:C:/Repositories/GenericUIItems",
"com.jorishoef.core.state": "file:C:/Repositories/Core-State",
"com.jorishoef.generic-ui-items.core-state-bridge": "https://github.com/JorisHoef/GenericUIItems-CoreState-Bridge.git#main"
```

Also add the bridge package to the project manifest `testables` array when running package tests:

```json
"testables": [
  "com.jorishoef.generic-ui-items.core-state-bridge"
]
```

Package source should stay in this repository. Do not copy package code into the test project.

## Pull Requests

- Keep runtime changes focused on bridge behavior.
- Add or update EditMode tests for behavior changes.
- Update README and CHANGELOG for public API changes.
- Keep runtime asmdef free of editor-only references.
- Do not add dependencies beyond the two core packages unless the bridge actually requires them.
