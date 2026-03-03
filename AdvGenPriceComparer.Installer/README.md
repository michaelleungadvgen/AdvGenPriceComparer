# AdvGenPriceComparer Installer

This WiX v4 installer project creates a Windows Installer (MSI) package for the AdvGen Price Comparer application.

## Prerequisites

- .NET 9.0 SDK or later
- WiX Toolset v4 (installed automatically via NuGet)

## Building the Installer

### From Command Line

```powershell
# Build the installer
cd AdvGenPriceComparer.Installer
dotnet build -c Release

# Or build for specific platform
dotnet build -c Release -p:Platform=x64
```

### Output Location

The MSI file will be generated at:
```
AdvGenPriceComparer.Installer/bin/x64/Release/AdvGenPriceComparer.msi
```

## Features

- **Per-machine installation**: Installs for all users (requires admin privileges)
- **Start Menu shortcut**: Creates shortcut in Start Menu
- **Desktop shortcut** (optional): User can choose to create desktop shortcut
- **Major upgrade support**: Automatically upgrades previous versions
- **Customizable install location**: User can choose installation directory

## Installer Properties

| Property | Description |
|----------|-------------|
| `INSTALLFOLDER` | Installation directory (default: Program Files\AdvGen Price Comparer) |
| `INSTALLDESKTOPSHORTCUT` | Set to 1 to install desktop shortcut |

## Silent Installation

```powershell
# Silent install
msiexec /i AdvGenPriceComparer.msi /qn

# Silent install with desktop shortcut
msiexec /i AdvGenPriceComparer.msi INSTALLDESKTOPSHORTCUT=1 /qn

# Silent uninstall
msiexec /x AdvGenPriceComparer.msi /qn
```

## Customization

To modify the installer:

1. Edit `Package.wxs` to change:
   - Product name, version, or manufacturer
   - Installation directories
   - Shortcuts and registry entries
   - UI customization

2. Add additional files:
   - Update the `Files` element in `ApplicationFiles` component group

3. Add new components:
   - Create new `ComponentGroup` fragments
   - Reference them in the `MainFeature` feature

## Troubleshooting

### Build Errors

1. **"WixToolset.Sdk not found"**: Ensure you have .NET 9.0+ SDK installed
2. **"Project reference not resolved"**: Build the WPF project first

### Installation Errors

1. **"Another version is already installed"**: Uninstall the previous version first, or use MajorUpgrade
2. **"Access denied"**: Run installer as Administrator
