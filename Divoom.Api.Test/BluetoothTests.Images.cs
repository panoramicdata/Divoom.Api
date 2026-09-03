using AwesomeAssertions;
using Divoom.Api.Models;
using SkiaSharp;
using Color = System.Drawing.Color;

namespace Divoom.Api.Test;

/// <summary>
/// <see cref="BluetoothTests"/> covering displaying images and animations.
/// </summary>
public partial class BluetoothTests
{
	[Fact]
	public async Task ViewImage_Constructed_Succeeds()
	{
		var imageBytes = new Color[256];
		var pixelIndex = 0;
		for (var x = 0; x < 16; x++)
		{
			for (var y = 0; y < 16; y++)
			{
				var r = pixelIndex;
				var g = x * 16;
				var b = y * 16;
				imageBytes[pixelIndex++] = Color.FromArgb(r, g, b);
			}
		}

		var device = await GetFirstDeviceAsync(CancellationToken);

		var deviceResponse = await Client
			.Bluetooth
			.ViewImageAsync(
				device,
				new DivoomImage(imageBytes),
				CancellationToken
			);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewAnimation_FromFile_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		var divoomAnimation = GetDivoomAnimation(new FileInfo("../../../Animations/ReportMagic.gif"));

		var deviceResponse = await Client
			.Bluetooth
			.ViewAnimationAsync(
				device,
				divoomAnimation,
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	private static DivoomAnimation GetDivoomAnimation(FileInfo fileInfo)
	{
		var animation = new DivoomAnimation();
		var frameTime = TimeSpan.Zero;

		using var stream = File.OpenRead(fileInfo.FullName);
		using var codec = SKCodec.Create(stream) ?? throw new InvalidOperationException($"Failed to load animation from {fileInfo.FullName}");
		var frameCount = codec.FrameCount;
		var info = codec.Info;

		for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
		{
			var frameInfo = codec.FrameInfo[frameIndex];
			using var bitmap = new SKBitmap(info);

			var options = new SKCodecOptions(frameIndex);
			codec.GetPixels(bitmap.Info, bitmap.GetPixels(), options);

			// Resize to 16x16 if needed
			SKBitmap resizedBitmap;
			if (bitmap.Width != 16 || bitmap.Height != 16)
			{
				resizedBitmap = bitmap.Resize(new SKImageInfo(16, 16), SKSamplingOptions.Default);
			}
			else
			{
				resizedBitmap = bitmap;
			}

			var frameImageBytes = new Color[256];
			var pixelIndex = 0;

			for (var y = 0; y < 16; y++)
			{
				for (var x = 0; x < 16; x++)
				{
					var pixel = resizedBitmap.GetPixel(x, y);
					frameImageBytes[pixelIndex++] = Color.FromArgb(pixel.Red, pixel.Green, pixel.Blue);
				}
			}

			if (resizedBitmap != bitmap)
			{
				resizedBitmap.Dispose();
			}

			var frameDelay = TimeSpan.FromMilliseconds(frameInfo.Duration);
			frameTime += frameDelay;

			animation.AddFrame(new DivoomImage(frameImageBytes, frameTime));
		}

		return animation;
	}

	[Fact]
	public async Task ViewImage_FromFile_Succeeds()
	{
		var imageBytes = new Color[256];
		var pixelIndex = 0;

		using var bitmap = SKBitmap.Decode("../../../Images/Panoramic Data.png");

		// Resize to 16x16 if needed
		SKBitmap resizedBitmap;
		if (bitmap.Width != 16 || bitmap.Height != 16)
		{
			resizedBitmap = bitmap.Resize(new SKImageInfo(16, 16), SKSamplingOptions.Default);
		}
		else
		{
			resizedBitmap = bitmap;
		}

		for (var y = 0; y < 16; y++)
		{
			for (var x = 0; x < 16; x++)
			{
				var pixel = resizedBitmap.GetPixel(x, y);
				imageBytes[pixelIndex++] = Color.FromArgb(pixel.Red, pixel.Green, pixel.Blue);
			}
		}

		if (resizedBitmap != bitmap)
		{
			resizedBitmap.Dispose();
		}

		var device = await GetFirstDeviceAsync(CancellationToken);

		var deviceResponse = await Client
			.Bluetooth
			.ViewImageAsync(
				device,
				new DivoomImage(imageBytes),
				CancellationToken
			);

		deviceResponse.IsOk.Should().BeTrue();
	}
}
