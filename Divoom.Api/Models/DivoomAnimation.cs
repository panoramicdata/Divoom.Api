using System;
using System.Collections.Generic;
using System.Linq;

namespace Divoom.Api.Models;

/// <summary>
/// A Divoom animation consisting of multiple frames
/// </summary>
public class DivoomAnimation
{
	private readonly List<byte> _frameBytes = [];

	/// <summary>
	/// Adds a frame to the animation.
	/// </summary>
	/// <param name="image">The image frame to add.</param>
	public void AddFrame(DivoomImage image)
	{
		// A frame is already AA LLLL TTTT RR NN COLOR_DATA PIXEL_DATA, so it is appended as is
		var frameBytes = image.GetImageBytes();
		_frameBytes.AddRange(frameBytes);

		TotalFrameLength += frameBytes.Count;
	}

	internal List<byte> GetPacket(int packetIndex)
		=> [.. _frameBytes
			.Skip(packetIndex * 400)
			.Take(400)];

	internal int TotalFrameLength { get; private set; }
}
