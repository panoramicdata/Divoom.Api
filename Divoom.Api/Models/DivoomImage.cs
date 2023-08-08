using System;
using System.Collections.Generic;
using System.Drawing;

namespace Divoom.Api.Models;

public class DivoomImage
{
	private List<Color> palette = new();
	private List<byte> encodedImage = new();

	public DivoomImage(Color[] image)
	{
		// See https://github.com/RomRider/node-divoom-timebox-evo/blob/master/PROTOCOL.md#animations-images-and-text

		foreach (var pixel in image)
		{
			var paletteIndex = palette.IndexOf(pixel);
			if (paletteIndex == -1)
			{
				palette.Add(pixel);
				if (palette.Count > 256)
				{
					throw new NotSupportedException("More than 256 colors not supported.  Less color variety must be pre-calculated.");
				}
			}
		}

		// Hack (inefficiency)
		// Pad the palette with black to ensure 8 bits per pixel.
		while (palette.Count < 256)
		{
			palette.Add(Color.Black);
		}

		var bitsPerPixel = (int)Math.Ceiling(Math.Log(palette.Count, 2));

		if (bitsPerPixel == 8)
		{
			foreach (var pixel in image)
			{
				encodedImage.Add((byte)palette.IndexOf(pixel));
			}
		}
		else
		{
			encodedImage.Add(0);

			var encodedImageByteIndex = 0;
			var encodedImageByteBitIndex = 0;
			foreach (var pixel in image)
			{
				var paletteIndex = (byte)palette.IndexOf(pixel);
				for (var bitIndex = 0; bitIndex < bitsPerPixel; bitIndex++)
				{
					if (encodedImageByteBitIndex == 8)
					{
						encodedImage.Add(0);
						encodedImageByteIndex++;
						encodedImageByteBitIndex = 0;
					}

					var bit = (paletteIndex & (1 << bitIndex)) != 0;
					if (bit)
					{
						encodedImage[encodedImageByteIndex] |= (byte)(1 << encodedImageByteBitIndex);
					}

					encodedImageByteBitIndex++;
				}
			}

			// Reverse the order of the bits in each byte of encoded image
			for (var i = 0; i < encodedImage.Count; i++)
			{
				encodedImage[i] = (byte)((encodedImage[i] * 0x0202020202UL & 0x010884422010UL) % 1023);
			}
		}
	}

	internal List<byte> GetImageBytes()
	{
		var imageLength = 7 + palette.Count * 3 + encodedImage.Count;

		var imageBytes = new List<byte>
		{
			// Image data
			0xaa, // Image start

			// Image length
			(byte)(imageLength & 0xff),
			(byte)(imageLength >> 8 & 0xff),

			// Image type
			0x00, // Fixed
			0x00, // Fixed
			0x00, // Fixed

			// Number of colors
			(byte)(
			palette.Count == 256
				? 0
				: palette.Count
			)
		};

		// Color data
		foreach (var color in palette)
		{
			imageBytes.Add(color.R);
			imageBytes.Add(color.G);
			imageBytes.Add(color.B);
		};

		// Pixel data
		foreach (var pixel in encodedImage)
		{
			imageBytes.Add(pixel);
		}

		return imageBytes;
	}
}
