using InTheHand.Net.Sockets;

namespace Divoom.Api.Models;

public class DivoomBluetoothDevice(BluetoothDeviceInfo x)
{
	public BluetoothDeviceInfo DeviceInfo { get; private set; } = x;
}
