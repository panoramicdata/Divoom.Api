using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Divoom.Api.Implementations;

/// <summary>
/// Reads a single framed response off a device's stream.
/// </summary>
/// <remarks>
/// A frame is <c>0x01</c>, two little-endian length bytes, the payload, a two-byte
/// CRC and a <c>0x02</c> end byte. Within the payload <c>0x03</c> escapes the byte
/// that follows it, which was sent with 3 added to its value. The framing lives here,
/// apart from <see cref="BluetoothManager"/>'s command API, because the two change for
/// entirely different reasons.
/// </remarks>
internal static class DeviceResponseReader
{
	/// <summary>
	/// The start byte plus the two length bytes that precede a response payload.
	/// </summary>
	private const int HeaderByteCount = 3;

	public static DeviceResponse Read(NetworkStream stream)
	{
		try
		{
			// Read all available bytes from the stream
			var rawBytes = new List<byte>();
			uint length = 0;
			var byteIndex = 0;
			var nextByteIsEscaped = false;
			while (stream.DataAvailable)
			{
				var byteAsInt = stream.ReadByte();
				var index = byteIndex++;

				if (index < HeaderByteCount)
				{
					AppendHeaderByte(rawBytes, byteAsInt, index, ref length);
					continue;
				}

				if (byteAsInt == 2 && byteIndex == length + 4)
				{
					return CompleteResponse(rawBytes);
				}

				AppendPayloadByte(rawBytes, byteAsInt, ref nextByteIsEscaped);
			}

			return new([]);
		}
		catch
		{
			return new([]);
		}
	}

	/// <summary>
	/// Appends one of the three header bytes: the 0x01 start byte, then the two
	/// little-endian length bytes, which are kept because the CRC is summed over them.
	/// </summary>
	private static void AppendHeaderByte(List<byte> rawBytes, int byteAsInt, int index, ref uint length)
	{
		switch (index)
		{
			case 0:
				// The first byte should be 0x01
				if (byteAsInt != 0x01)
				{
					throw new FormatException("First byte should be 0x01");
				}

				// All is well
				return;
			case 1:
				length = (byte)(byteAsInt & 0xff);
				rawBytes.Add((byte)byteAsInt);
				return;
			default:
				length |= ((uint)((byte)(byteAsInt & 0xff))) << 8;
				rawBytes.Add((byte)byteAsInt);
				return;
		}
	}

	/// <summary>
	/// Builds the response from a complete frame, verifying and then stripping the
	/// trailing CRC and the leading length bytes, neither of which belong in the payload.
	/// </summary>
	private static DeviceResponse CompleteResponse(List<byte> rawBytes)
	{
		// Get the CRC
		var crc =
			rawBytes[^2]
			|
			(ushort)(rawBytes[^1] << 8);

		// Remove the CRC bytes
		rawBytes.RemoveRange(rawBytes.Count - 2, 2);

		// Sum the bytes
		var sum = rawBytes.Sum(x => x);
		if (sum != crc)
		{
			throw new FormatException("CRC does not match");
		}

		// Remove the Length bytes
		rawBytes.RemoveRange(0, 2);

		// Return a device response based on the raw bytes excluding length and CRC
		return new(rawBytes);
	}

	/// <summary>
	/// Appends a payload byte, honouring the 0x03 escape prefix: the byte that follows
	/// it was sent with 3 added to its value.
	/// </summary>
	private static void AppendPayloadByte(List<byte> rawBytes, int byteAsInt, ref bool nextByteIsEscaped)
	{
		if (byteAsInt == 3)
		{
			nextByteIsEscaped = true;
			return;
		}

		if (nextByteIsEscaped)
		{
			byteAsInt -= 3;
			nextByteIsEscaped = false;
		}

		rawBytes.Add((byte)byteAsInt);
	}
}
