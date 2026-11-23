using AwesomeAssertions;
using Divoom.Api.Models;
using Color = System.Drawing.Color;

namespace Divoom.Api.Test;

public class DivoomAnimationFrameTests
{
	[Fact]
	public void AddFrame_WritesTheFrameInTheImageFrameFormat()
	{
		// The protocol says a frame is always AA LLLL TTTT RR NN COLOR_DATA PIXEL_DATA, the layout DivoomImage writes
		var image = new DivoomImage([.. Enumerable.Repeat(Color.FromArgb(255, 0, 0), 256)], TimeSpan.FromMilliseconds(100));
		var animation = new DivoomAnimation();

		animation.AddFrame(image);

		var frame = new List<byte>();
		for (var index = 0; animation.GetPacket(index) is { Count: > 0 } packet; index++)
		{
			frame.AddRange(packet);
		}

		frame.Should().Equal(image.GetImageBytes());
		animation.TotalFrameLength.Should().Be(frame.Count);
	}
}
