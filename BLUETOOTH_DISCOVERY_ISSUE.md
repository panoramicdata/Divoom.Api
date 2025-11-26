# Bluetooth Discovery Issue - Root Cause Analysis

## Problem
`BluetoothClient.DiscoverDevices()` returns 0 devices even though devices are paired in Windows.

## Evidence
1. ? Devices visible in Windows: `Get-PnpDevice` shows TimeBox-Evo and Pixoo-Max
2. ? InTheHand library can't discover: `DiscoverDevices()` returns empty collection
3. ?? Intermittent: Earlier it found 1 device, now finds 0

## Root Cause
The `InTheHand.Net.Bluetooth` library's `DiscoverDevices()` method relies on the Windows Bluetooth stack's discovery API, which:
- Only finds devices in **discoverable mode** (not just paired)
- Can be blocked by Windows Bluetooth settings
- Is unreliable for finding already-paired devices
- Requires devices to be actively advertising

## Why It Worked Before Then Stopped
- Device went to sleep/low-power mode
- Device stopped advertising (paired but not discoverable)
- Windows Bluetooth cache cleared
- Another Bluetooth app is using the radio

## Solution Options

### Option 1: Use Device Address Directly (Recommended for Tests)
Since you know your device address (`1175580CAD93`), connect directly without discovery:

```csharp
// In appsettings.json
{
  "DeviceMac": "1175580CAD93",  // TimeBox-Evo-audio address
  "DeviceIp": "192.168.1.123"
}

// In code - skip discovery, use known address
var address = BluetoothAddress.Parse("1175580CAD93");
var device = new DivoomBluetoothDevice(new BluetoothDeviceInfo(address));
```

### Option 2: Make Device Discoverable
Put your TimeBox-Evo into **pairing/discoverable mode**:
1. Turn off device
2. Turn on while holding pairing button
3. Device should start blinking/advertising
4. Then run tests

### Option 3: Windows Bluetooth Reset
```powershell
# Restart Bluetooth service
Restart-Service bthserv

# Or toggle Bluetooth off/on in Windows Settings
```

### Option 4: Use Paired Device List (Windows API)
Instead of discovery, enumerate already-paired devices using Windows APIs.

## Recommended Fix for Tests

Skip discovery entirely and use the known device address from configuration:

```csharp
public List<DivoomBluetoothDevice> GetDevices()
{
    // Try discovery first
    var discovered = TryDiscoverDevices();
    if (discovered.Any())
        return discovered;
    
    // Fallback: Use configured device address if discovery fails
    if (!string.IsNullOrEmpty(_configuredDeviceMac))
    {
        try
        {
            var address = BluetoothAddress.Parse(_configuredDeviceMac);
            var deviceInfo = new BluetoothDeviceInfo(address);
            return new List<DivoomBluetoothDevice> { new(deviceInfo) };
        }
        catch
        {
            // Fallback failed
        }
    }
    
    return new List<DivoomBluetoothDevice>();
}
```

## Why This Happens
Bluetooth Classic (not BLE) discovery is **inherently unreliable** on Windows because:
1. Devices must be in discoverable mode to be found
2. Paired devices don't automatically advertise
3. Windows caches device information inconsistently
4. Multiple Bluetooth stacks can interfere

## Immediate Action
For tests to pass reliably, either:
1. ? Put device in discoverable mode before each test run
2. ? Use known device address (skip discovery)
3. ? Accept that some test runs will fail due to discovery issues
