# Divoom.Api

[![Nuget](https://img.shields.io/nuget/v/Divoom.Api)](https://www.nuget.org/packages/Divoom.Api/)
[![Nuget](https://img.shields.io/nuget/dt/Divoom.Api)](https://www.nuget.org/packages/Divoom.Api/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/aed1b9ece5aa4875a3b539a770a7d0ad)](https://www.codacy.com?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=panoramicdata/Divoom.Api&amp;utm_campaign=Badge_Grade)

This is a .NET library for controlling Divoom Timebox devices.

It is based on the [Divoom Timebox API](http://doc.divoom-gz.com/web/#/12?page_id=189).

We only have access to the following devices at present, so these are the only onces we can develop against:
* PIXOO64
* Timebox Evo (16x16)

## Local network (Pixoo 64)

```csharp
using var client = new DivoomClient(new DivoomClientOptions { DeviceIp = "192.168.1.131" }, logger);
var config = await client.Channel.GetAllConfigAsync(new(), cancellationToken);
await client.Channel.SetBrightnessAsync(new SetBrightnessRequest { Brightness = 80 }, cancellationToken);
await client.Channel.ShowTextAsync("BUSY", "#B40000", "#FFFFFF", 64, cancellationToken);
```

Find a device's IP with `client.Gz.GetLanDevicesAsync(cancellationToken)`.
A device that rejects a command still answers HTTP 200, so check `error_code` with `ChannelExtensions.EnsureSuccess(response)`.

Gratefully received:
* Documentation
* Devices (ship to Panoramic House, 46 Heywood Avenue, SL6 3JA, UK)
* Amazon UK vouchers to buy devices
* Hints
* Findings
* Bluetooth logs