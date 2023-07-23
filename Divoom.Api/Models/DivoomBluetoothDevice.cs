using InTheHand.Net.Sockets;

namespace Divoom.Api.Models;

public class DivoomBluetoothDevice
{
	internal BluetoothDeviceInfo DeviceInfo { get; private set; }

	public DivoomBluetoothDevice(BluetoothDeviceInfo x)
	{
		DeviceInfo = x;
	}
}
