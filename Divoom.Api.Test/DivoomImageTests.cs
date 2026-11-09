using AwesomeAssertions;
using Divoom.Api.Models;
using Color = System.Drawing.Color;

namespace Divoom.Api.Test;

/// <summary>
/// Image frames per the timebox-evo protocol: AA LLLL TTTT RR NN COLOR_DATA PIXEL_DATA.
/// </summary>
public class DivoomImageTests
{
	private static readonly Color Red = Color.FromArgb(255, 0, 0);
	private static readonly Color Blue = Color.FromArgb(0, 0, 255);

	private static Color[] HalfRedHalfBlue()
		=> [.. Enumerable.Repeat(Red, 128), .. Enumerable.Repeat(Blue, 128)];

	[Fact]
	public void GetImageBytes_LaysOutHeaderPaletteAndPixels()
	{
		var image = new DivoomImage(HalfRedHalfBlue(), TimeSpan.FromMilliseconds(300));

		var bytes = image.GetImageBytes();

		// The palette is always padded to 256 colours, which leaves one byte per pixel
		image.ImageSize.Should().Be(256 * 3 + 256);
		bytes.Should().HaveCount(7 + image.ImageSize);

		// 7 + 1024 = 0x0407 bytes, 300 ms = 0x012C, palette reset, 256 colours written as 0
		bytes.Take(7).Should().Equal(0xAA, 0x07, 0x04, 0x2C, 0x01, 0x00, 0x00);
		bytes.Skip(7).Take(6).Should().Equal(0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF);
		bytes.Skip(13).Take(254 * 3).Should().AllSatisfy(b => b.Should().Be(0));

		var pixels = bytes.Skip(7 + 768).ToList();
		pixels.Take(128).Should().AllSatisfy(b => b.Should().Be(0));
		pixels.Skip(128).Should().AllSatisfy(b => b.Should().Be(1));
	}

	[Fact]
	public void Constructor_WithoutFrameTime_HasZeroFrameTime()
	{
		var image = new DivoomImage(HalfRedHalfBlue());

		image.FrameTimeMs.Should().Be(0);
		image.GetImageBytes().Skip(3).Take(2).Should().Equal(0x00, 0x00);
	}

	[Fact]
	public void Constructor_WithEveryPaletteSlotUsed_IndexesEachPixel()
	{
		var pixels = Enumerable.Range(0, 256).Select(i => Color.FromArgb(i, 255 - i, 7)).ToArray();

		var bytes = new DivoomImage(pixels).GetImageBytes();

		bytes.Skip(7 + 768).Should().Equal(Enumerable.Range(0, 256).Select(i => (byte)i));
		bytes.Skip(7 + 3 * 200).Take(3).Should().Equal(200, 55, 7);
	}

	[Fact]
	public void Constructor_WithMoreThan256Colours_IsNotSupported()
	{
		var pixels = Enumerable.Range(0, 257).Select(i => Color.FromArgb(i % 256, i / 256, 0)).ToArray();

		var act = () => new DivoomImage(pixels);

		act.Should().Throw<NotSupportedException>();
	}

	// Layout matches the frame that drew on a Pixoo Max on 2026-11-09 (hass-divoom's pixoomax encoding).
	[Fact]
	public void GetImageBytes_For32x32_UsesPixooMaxFlagWideCountAndUnpaddedPalette()
	{
		var green = Color.FromArgb(0, 255, 0);
		Color[] pixels = [Red, Blue, green, .. Enumerable.Repeat(Red, 1021)];

		var bytes = new DivoomImage(pixels).GetImageBytes();

		// 3 colours need 2 bits per pixel: 1024 * 2 / 8 = 256 bytes; length = 8 + 9 + 256 = 0x0111
		bytes.Should().HaveCount(8 + 9 + 256);
		bytes.Take(8).Should().Equal(0xAA, 0x11, 0x01, 0x00, 0x00, 0x03, 0x03, 0x00);
		bytes.Skip(8).Take(9).Should().Equal(0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0xFF, 0x00);

		// Indices 0, 1, 2, 0 packed least significant bits first: 00 | 01<<2 | 10<<4 | 00<<6 = 0x24
		bytes[17].Should().Be(0x24);
		bytes.Skip(18).Should().AllSatisfy(b => b.Should().Be(0));
	}

	[Fact]
	public void GetImageBytes_For32x32SingleColour_UsesOneBitPerPixel()
	{
		var bytes = new DivoomImage([.. Enumerable.Repeat(Blue, 1024)]).GetImageBytes();

		bytes.Should().HaveCount(8 + 3 + 128);
		bytes.Skip(5).Take(3).Should().Equal(0x03, 0x01, 0x00);
	}

	[Fact]
	public void Animation_WithNoFrames_HasNoPackets()
	{
		var animation = new DivoomAnimation();

		animation.GetPacket(0).Should().BeEmpty();
		animation.TotalFrameLength.Should().Be(0);
	}

	[Fact]
	public void Animation_SplitsFrameDataIntoPacketsOfAtMost400Bytes()
	{
		var animation = new DivoomAnimation();
		var first = new DivoomImage(HalfRedHalfBlue(), TimeSpan.FromMilliseconds(100));
		var second = new DivoomImage([.. Enumerable.Repeat(Blue, 256)], TimeSpan.FromMilliseconds(200));
		animation.AddFrame(first);
		animation.AddFrame(second);

		var packets = new List<List<byte>>();
		for (var index = 0; animation.GetPacket(index) is { Count: > 0 } packet; index++)
		{
			packets.Add(packet);
		}

		packets.Should().HaveCountGreaterThan(1);
		packets.SkipLast(1).Should().AllSatisfy(p => p.Should().HaveCount(400));
		packets[^1].Should().HaveCountLessThanOrEqualTo(400);

		var all = packets.SelectMany(p => p).ToList();
		all[0].Should().Be(0xAA);
		all.Should().EndWith(second.GetImageBytes());
	}
}
