using System;
using System.Collections.Generic;

namespace Divoom.Api.Implementations;

internal class CommandBuilder
{
	readonly List<byte> _bytes = new();
	uint _checksum;
	uint _count;

	public byte[] GetBytes()
	{
		var checksum = _checksum;

		var byteList = new List<byte>
		{
			// Start byte
			0x01
		};

		// Add the length of the command
		var length = _count + 2;
		var byte1 = Convert.ToByte(length & 0xFF);
		byteList.Add(byte1);
		checksum += byte1;
		var byte2 = Convert.ToByte((length >> 8) & 0xFF);
		byteList.Add(byte2);
		checksum += byte2;

		// Add the payload
		byteList.AddRange(_bytes);

		// Add two bytes, representing the length of the command
		byteList.Add(Convert.ToByte(checksum & 0xFF));
		byteList.Add(Convert.ToByte((checksum >> 8) & 0xFF));

		// Add the end byte
		byteList.Add(0x02);

		return byteList.ToArray();
	}

	public void Add(byte @byte)
	{
		_count++;
		_checksum += @byte;
		_bytes.Add(@byte);
	}

	public void AddWithEscaping(byte @byte)
	{
		_count++;
		_checksum += @byte;

		// 0x01, 0x02 or 0x03?
		if (@byte is 0x01 or 0x02 or 0x03)
		{
			// Yes, escape it
			_bytes.Add(0x03);
			_bytes.Add((byte)(@byte + 0x03));
			return;
		}

		// No, just add it
		_bytes.Add(@byte);
	}
}
