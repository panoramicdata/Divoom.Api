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

		Bytes = outputBytes.ToArray();
	}

	public Command Command { get; private set; }

	public byte[] Bytes { get; private set; }

	public bool IsEmpty => Bytes.Length == 0;

	public bool IsOk { get; }

	public override string ToString() =>
		$"{Command} {(IsOk ? "OK" : "Not OK")}: " +
		(Command == Command.SetChannel
			? ((Channel)Bytes[0]).ToString()
			: string.Join(":", Bytes.Select(x => x.ToString("X2", CultureInfo.InvariantCulture))));
}
