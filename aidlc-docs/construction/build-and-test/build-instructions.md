# Build Instructions

## Prerequisites
- **Build Tool**: .NET SDK 10.0
- **Dependencies**: NuGet packages restored via `dotnet restore`
- **Environment Variables**:
  - `POLICY_CONFIG_PATH` (optional for local runs; set to `config/policy.template.json` when needed)
- **System Requirements**:
  - macOS/Linux/Windows with .NET 10 SDK
  - ~2 GB free RAM for local build and test execution
  - writable workspace and temporary directory

## Build Steps

### 1. Restore Dependencies
```bash
dotnet restore AuthModule.slnx
```

### 2. Configure Environment
```bash
export POLICY_CONFIG_PATH=config/policy.template.json
```

### 3. Build All Units
```bash
dotnet build AuthModule.slnx --no-restore
```

### 4. Verify Build Success
- **Expected Output**: all projects build successfully with 0 errors
- **Build Artifacts**:
  - `src/AuthModule/*/bin/Debug/net10.0/`
  - `tests/*/bin/Debug/net10.0/`
- **Common Warnings**: analyzer or nullable warnings may appear; errors are blocking

## Troubleshooting

### Build fails with restore errors
- **Cause**: stale cache / temporary network issue
- **Solution**:
  1. run `dotnet nuget locals all --clear`
  2. rerun `dotnet restore AuthModule.slnx`

### Build fails with compilation errors
- **Cause**: interface mismatch or missing reference across units
- **Solution**:
  1. build failing project directly
  2. resolve compile errors in reported files
  3. rerun full solution build
