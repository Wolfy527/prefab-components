# Fallback Installer

Build:

```powershell
./.github/scripts/build-fallback-installer.ps1
```

The generated installer Unity package is for product authoring. Import it into
a project that has Prefab Builder installed, then include this folder in the
product export:

```text
Assets/Wolfy_527/~ Supporting Files/Prefab Components Installer
```

Do not include either Components package folder in the product export:

```text
Packages/com.wolfy527.prefab-components
Packages/com.wolfy527.prefab-components.fallback
```

Prefab Builder keeps the installer staged. In a customer project, the bootstrap
keeps the VPM package when present. Otherwise, it safely moves recognized legacy
scripts aside and installs or updates the fallback under Supporting Files without
downgrading a newer version. VCC removes that fallback before installing the
managed package. Temporary installer files are removed when Unity closes.
