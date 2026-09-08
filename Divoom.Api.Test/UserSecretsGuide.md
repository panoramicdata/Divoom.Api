# User Secrets Configuration Guide

## What are User Secrets?

User Secrets is a secure way to store configuration data during development without
committing it to source control. The Divoom tests need the details of a device on your
own network, which should not be shared, so they are read from User Secrets.

## Setting Up User Secrets

### Quick Start
```bash
# Navigate to the test project directory
cd Divoom.Api.Test

# Set the details of your Divoom device
dotnet user-secrets set "DeviceId" "300012345"
dotnet user-secrets set "DeviceIp" "192.168.1.123"
dotnet user-secrets set "DeviceMac" "01234567890A"
```

### Finding Your Device's Details

With the device powered on and connected to the same network as your PC, ask Divoom's
cloud which devices it can see on your network:

```bash
curl https://app.divoom-gz.com/Device/ReturnSameLANDevice
```

The response lists each device it found:

```json
{
  "ReturnCode": 0,
  "ReturnMessage": "",
  "DeviceList": [
    {
      "DeviceName": "Pixoo64",
      "DeviceId": 300012345,
      "DevicePrivateIp": "192.168.1.123",
      "DeviceMac": "01234567890A"
    }
  ]
}
```

Map those onto the secrets above: `DeviceId` → `DeviceId`, `DevicePrivateIp` → `DeviceIp`,
and `DeviceMac` → `DeviceMac`.

An empty `DeviceList` means the cloud cannot see a device on your network. Check that the
device is powered on, is on the same network as your PC, and has finished connecting.

## Viewing Your Secrets

```bash
# List all secrets
dotnet user-secrets list --project Divoom.Api.Test
```

## Secrets File Location

Your secrets are stored at:
- Windows: `%APPDATA%\Microsoft\UserSecrets\4e7ca9bf-a583-4222-93a6-20bbf6c809e7\secrets.json`
- Linux/macOS: `~/.microsoft/usersecrets/4e7ca9bf-a583-4222-93a6-20bbf6c809e7/secrets.json`

## The appsettings.json Alternative

An `appsettings.json` in the test project root is still honoured, so you can copy
`appsettings.example.json` to `appsettings.json` and fill in the values instead. It is
gitignored. Where both set the same key, User Secrets wins.

User Secrets are preferred: they live outside the repository, so there is no file to
accidentally commit.

## Bluetooth Tests

The Bluetooth tests additionally need the device **paired with this PC** (Windows
Settings → Bluetooth & devices) and within range. They talk to the device directly over
a serial-port Bluetooth connection rather than over the network, so they do not use
`DeviceIp`. Run `DiagnoseBluetooth_ListsAllDevices` to list what Bluetooth can see.

## Benefits of User Secrets

- Keeps configuration out of source control
- Easy to configure and manage
- Per-developer configuration
- Works across different machines
