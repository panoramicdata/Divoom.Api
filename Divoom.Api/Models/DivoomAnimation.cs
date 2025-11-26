using System;
using System.Collections.Generic;
using System.Linq;

namespace Divoom.Api.Models;

public class DivoomAnimation
{
	private readonly List<byte> _frameBytes = [];

	public void AddFrame(DivoomImage image)
	{
		// AA LLLL TTTT RR NN COLOR_DATA PIXEL_DATA
		var frameTime = image.FrameTimeMs;
		var length = image.ImageSize + 6;

		// Frame start
		_frameBytes.Add(0xAA);

		// Frame length
		_frameBytes.Add((byte)(length & 0xFF));
		_frameBytes.Add((byte)((length >> 8) & 0xFF));


		_frameBytes.Add((byte)(frameTime & 0xFF));
		_frameBytes.Add((byte)((frameTime >> 8) & 0xFF));

		_frameBytes.AddRange(image.GetImageBytes());

		TotalFrameLength += length;
	}

	internal List<byte> GetPacket(int packetIndex)
		=> [.. _frameBytes
			.Skip(packetIndex * 400)
			.Take(400)];

	internal int TotalFrameLength { get; private set; }
}
