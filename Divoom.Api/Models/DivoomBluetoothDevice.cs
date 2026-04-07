using InTheHand.Net.Sockets;

namespace Divoom.Api.Models;

/// <summary>
/// A Divoom Bluetooth device
/// </summary>
/// <param name="x">The Bluetooth device info.</param>
public class DivoomBluetoothDevice(BluetoothDeviceInfo x)
{
	/// <summary>
	/// The Bluetooth device info
	/// </summary>
	public BluetoothDeviceInfo DeviceInfo { get; private set; } = x;
}
