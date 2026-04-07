using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Divoom.Api.Models;

/// <summary>
/// See https://github.com/RomRider/node-divoom-timebox-evo/blob/master/PROTOCOL.md#receiving-messages for information
/// </summary>
public class DeviceResponse
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DeviceResponse"/> class.
	/// </summary>
	/// <param name="bytes">The raw response bytes.</param>
	public DeviceResponse(IEnumerable<byte> bytes)
	{
		var byteIndex = 0;

		var outputBytes = new List<byte>();

		// Enumerate the bytes
		foreach (var @byte in bytes)
		{
			switch (byteIndex++)
			{
				case 0:
					var command = (Command)@byte;
					if (command != Command.Response)
					{
						throw new DataException("This does not appear to be a valid response.");
					}

					break;
				case 1:
					Command = (Command)@byte;
					break;
				case 2:
					IsOk = @byte == 0x55;
					break;
				default:
					outputBytes.Add(@byte);
					break;
			}
		}

		Bytes = [.. outputBytes];
	}

	/// <summary>
	/// The command
	/// </summary>
	public Command Command { get; private set; }

	/// <summary>
	/// The response bytes
	/// </summary>
	public byte[] Bytes { get; private set; }

	/// <summary>
	/// Whether the response is empty
	/// </summary>
	public bool IsEmpty => Bytes.Length == 0;

	/// <summary>
	/// Whether the response is OK
	/// </summary>
	public bool IsOk { get; }

	/// <inheritdoc />
	public override string ToString() =>
		$"{Command} {(IsOk ? "OK" : "Not OK")}: " +
		(Command == Command.SetChannel
			? ((Channel)Bytes[0]).ToString()
			: string.Join(":", Bytes.Select(x => x.ToString("X2", CultureInfo.InvariantCulture))));
}