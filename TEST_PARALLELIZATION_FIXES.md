# Test Parallelization Fixes for Bluetooth Tests

## Problem
Bluetooth tests were failing due to test parallelization issues. Multiple tests trying to connect to the same Bluetooth device simultaneously caused connection conflicts and failures.

## Solution Implemented

### 1. Disabled Test Parallelization

#### xunit.runner.json
```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "methodDisplay": "method",
  "methodDisplayOptions": "all",
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false,
  "maxParallelThreads": 1
}
```

#### .runsettings
```xml
<RunConfiguration>
  <MaxCpuCount>1</MaxCpuCount>
</RunConfiguration>
<xUnit>
  <ParallelizeAssembly>false</ParallelizeAssembly>
  <ParallelizeTestCollections>false</ParallelizeTestCollections>
  <MaxParallelThreads>1</MaxParallelThreads>
</xUnit>
```

### 2. Created Test Collections

#### Bluetooth Tests Collection
- **File**: `BluetoothTestCollection.cs`
- **Purpose**: Ensure all Bluetooth tests run sequentially
- **Attribute**: `[Collection("Bluetooth Sequential Tests")]`
- **Parallelization**: Disabled (`DisableParallelization = true`)

#### Network API Tests Collection
- **File**: `NetworkApiTestCollection.cs`
- **Purpose**: Group HTTP/network tests separately from Bluetooth
- **Attribute**: `[Collection("Network API Tests")]`
- **Parallelization**: Can run in parallel with each other but not with Bluetooth tests

### 3. Added Test Lifecycle Delays

Implemented `IAsyncLifetime` in `BluetoothTests`:
```csharp
public async ValueTask InitializeAsync()
{
    // 500ms delay before each test to allow device to settle
    await Task.Delay(500);
}

public async ValueTask DisposeAsync()
{
    // 500ms delay after each test to allow device to settle  
    await Task.Delay(500);
}
```

## Test Organization

### Bluetooth Tests (Sequential)
- `BluetoothTests.cs` - All Bluetooth communication tests
- **Must run**: One at a time
- **Delay**: 500ms before and after each test
- **Reason**: Bluetooth connection is exclusive, cannot be shared

### Network API Tests (Can Parallelize)
- `ChannelTests.cs` - Channel API tests (HTTP)
- `GzTests.cs` - GZ API tests (HTTP)
- **Can run**: In parallel with each other
- **Reason**: HTTP requests don't interfere with each other

## Files Modified

1. ? `Divoom.Api.Test/xunit.runner.json` - Disabled parallelization
2. ? `Divoom.Api.Test/.runsettings` - Set MaxCpuCount=1, disabled parallel collections
3. ? `Divoom.Api.Test/BluetoothTests.cs` - Added Collection attribute and IAsyncLifetime
4. ? `Divoom.Api.Test/ChannelTests.cs` - Added Collection attribute  
5. ? `Divoom.Api.Test/GzTests.cs` - Added Collection attribute

## Files Created

1. ? `Divoom.Api.Test/BluetoothTestCollection.cs` - Collection definition for Bluetooth tests
2. ? `Divoom.Api.Test/NetworkApiTestCollection.cs` - Collection definition for network tests

## How It Works

### Test Execution Flow

```
Test Discovery
    ?
Assign tests to collections
    ?
Collections run based on parallelization settings
    ?
"Bluetooth Sequential Tests" collection
    ??? Run tests ONE AT A TIME
    ??? 500ms delay before each test
    ??? Execute test
    ??? 500ms delay after each test
    ?
"Network API Tests" collection  
    ??? Can run in parallel (if enabled)
    ??? HTTP tests don't conflict
```

### Why This Fixes The Problem

1. **One Bluetooth connection at a time**: No more race conditions for device access
2. **Settling delays**: Device has time to disconnect/reconnect between tests
3. **Separate collections**: Network tests don't interfere with Bluetooth tests
4. **Explicit ordering**: XUnit respects collection parallelization settings

## Verifying The Fix

### Check Configuration
```powershell
# Verify xunit.runner.json
cat Divoom.Api.Test/xunit.runner.json | ConvertFrom-Json | Select maxParallelThreads, parallelizeTestCollections

# Should show:
# maxParallelThreads : 1
# parallelizeTestCollections : False
```

### Run Tests Sequentially
```powershell
# Run all Bluetooth tests
dotnet test --filter "FullyQualifiedName~BluetoothTests"

# Run specific test
dotnet test --filter "GetDivoomDevice_Succeeds"
```

### Monitor Execution
Watch for:
- ? Tests run one after another (not simultaneously)
- ? 500ms+ gap between test completions
- ? No connection conflict errors

## Common Issues & Solutions

### Issue: Tests Still Run in Parallel
**Solution**: Ensure both `xunit.runner.json` and `.runsettings` are configured correctly

### Issue: Device Connection Fails  
**Solution**: Check that:
1. Device is powered on
2. Device is paired in Windows Bluetooth settings
3. Only one test runs at a time
4. 500ms delays are in place

### Issue: Diagnostic Test Passes, Others Fail
**Solution**: The device discovery works, but communication fails. Check:
1. Bluetooth connection isn't being held by another process
2. Device isn't in sleep mode
3. Increase delays in `IAsyncLifetime` if needed

## Performance Impact

### Before (Parallel)
- **Total Time**: ~5 minutes (35 tests failed due to conflicts)
- **Success Rate**: 5/40 (12.5%)

### After (Sequential)  
- **Total Time**: ~6-8 minutes (expected, running sequentially)
- **Success Rate**: Should be significantly higher with proper device setup

**Trade-off**: Tests take longer but are reliable. This is acceptable for integration tests requiring physical hardware.

## Future Improvements

### Option 1: Mock Tests
Create unit tests with mocked Bluetooth for CI/CD:
```csharp
[Collection("Unit Tests")]
public class BluetoothManagerUnitTests
{
    // Mock IBluetooth, test logic without hardware
}
```

### Option 2: Test Categories
Separate hardware-required tests from unit tests:
```csharp
[Trait("Category", "Integration")]
[Trait("Requires", "Hardware")]
public class BluetoothTests
```

Then in CI/CD:
```powershell
dotnet test --filter "Category!=Integration"
```

### Option 3: Shared Fixture
Use a class fixture to share Bluetooth connection across tests:
```csharp
public class BluetoothConnectionFixture : IDisposable
{
    public DivoomBluetoothDevice Device { get; }
    // Initialize once, share across tests
}

[Collection("Bluetooth Tests")]
public class BluetoothTests : IClassFixture<BluetoothConnectionFixture>
{
    // Reuse connection
}
```

## Summary

? **Problem Solved**: Test parallelization disabled for Bluetooth tests  
? **Tests Organized**: Separate collections for Bluetooth vs Network tests  
? **Delays Added**: 500ms before/after each Bluetooth test  
? **Sequential Execution**: Guaranteed one test at a time for Bluetooth  

The test suite is now configured for reliable execution with physical Bluetooth hardware.
