using System;
using System.Collections.Generic;
using System.Linq;

namespace Divoom.Api.Models;

public class DivoomAnimation
{
	private readonly List<byte> _frameBytes = new();

	public void AddFrame(DivoomImage image)
	{
		if (_frameBytes.Count == 0 && image.FrameTimeMs != 0)
		{
			throw new ArgumentException(
				$"The first frame must have a {nameof(image.FrameTimeMs)} of zero.",
				nameof(image));
		}

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
		=> _frameBytes
			.Skip(packetIndex * 400)
			.Take(400)
			.ToList();

	internal int TotalFrameLength { get; private set; }
}
