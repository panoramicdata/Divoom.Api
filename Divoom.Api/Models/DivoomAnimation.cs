using System;
using System.Collections.Generic;
using System.Linq;

namespace Divoom.Api.Models;

public class DivoomAnimation
{
	private readonly List<byte> _frameBytes = new();

	private readonly List<int> _millisecondTimings = new();

	public IReadOnlyList<int> MillisecondTiming => _millisecondTimings;

	public int FrameCount { get; private set; }

	public void AddFrame(
		DivoomImage image,
		TimeSpan timeSinceLastFrame)
	{
		if (FrameCount == 0 && timeSinceLastFrame != TimeSpan.Zero)
		{
			throw new ArgumentException(
				$"The first frame must have a {nameof(timeSinceLastFrame)} of zero.",
				nameof(timeSinceLastFrame));
		}

		_millisecondTimings.Add((int)timeSinceLastFrame.TotalMilliseconds
			+ FrameCount == 0 ? 0 : _millisecondTimings[^1]);

		AddFrame(image);

		FrameCount++;
	}

	private void AddFrame(DivoomImage image)
	{
		// AA LLLL TTTT RR NN COLOR_DATA PIXEL_DATA
		_frameBytes.Add(0xAA);
	}

	internal List<byte> GetPacket(int packetIndex)
		=> _frameBytes
			.Skip(packetIndex * 400)
			.Take(400)
			.ToList();

	internal int TotalFrameLength { get; private set; }
}
