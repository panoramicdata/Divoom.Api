using Divoom.Api.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

/// <summary>
/// Sends a command to a device and returns what it answers with.
/// </summary>
internal interface IDeviceTransport
{
	/// <summary>
	/// Sends a command made up of a fixed sequence of bytes, in the order given.
	/// </summary>
	Task<DeviceResponseSet> SendCommandAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken,
		params byte[] commandBytes);
}