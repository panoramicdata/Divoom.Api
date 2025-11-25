using InTheHand.Net.Sockets;

namespace Divoom.Api.Models;

public class DivoomBluetoothDevice(BluetoothDeviceInfo x)
{
	internal BluetoothDeviceInfo DeviceInfo { get; private set; } = x;
}
