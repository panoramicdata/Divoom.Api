using System;
using System.Collections.Generic;
using System.Drawing;

namespace Divoom.Api.Models;

public class DivoomImage
{
	private readonly List<Color> _palette = [];
	private readonly List<byte> _encodedImage = [];
	public int FrameTimeMs { get; }

	public DivoomImage(Color[] image, TimeSpan? frameTime = null)
	{
		// See https://github.com/RomRider/node-divoom-timebox-evo/blob/master/PROTOCOL.md#animations-images-and-text

		foreach (var pixel in image)
		{
			var paletteIndex = _palette.IndexOf(pixel);
			if (paletteIndex == -1)
			{
				_palette.Add(pixel);
				if (_palette.Count > 256)
				{
					throw new NotSupportedException("More than 256 colors not supported.  Less color variety must be pre-calculated.");
				}
			}
		}

		// Hack (inefficiency)
		// Pad the palette with black to ensure 8 bits per pixel.
		while (_palette.Count < 256)
		{
			_palette.Add(Color.Black);
		}

		var bitsPerPixel = (int)Math.Ceiling(Math.Log(_palette.Count, 2));

		if (bitsPerPixel == 8)
		{
			foreach (var pixel in image)
			{
				_encodedImage.Add((byte)_palette.IndexOf(pixel));
			}
		}
		else
		{
			_encodedImage.Add(0);

			var encodedImageByteIndex = 0;
			var encodedImageByteBitIndex = 0;
			foreach (var pixel in image)
			{
				var paletteIndex = (byte)_palette.IndexOf(pixel);
				for (var bitIndex = 0; bitIndex < bitsPerPixel; bitIndex++)
				{
					if (encodedImageByteBitIndex == 8)
					{
						_encodedImage.Add(0);
						encodedImageByteIndex++;
						encodedImageByteBitIndex = 0;
					}

					var bit = (paletteIndex & (1 << bitIndex)) != 0;
					if (bit)
					{
						_encodedImage[encodedImageByteIndex] |= (byte)(1 << encodedImageByteBitIndex);
					}

					encodedImageByteBitIndex++;
				}
			}

			// Reverse the order of the bits in each byte of encoded image
			for (var i = 0; i < _encodedImage.Count; i++)
			{
				_encodedImage[i] = (byte)((_encodedImage[i] * 0x0202020202UL & 0x010884422010UL) % 1023);
			}
		}

		FrameTimeMs = (int)(frameTime?.TotalMilliseconds ?? 0);
	}

	public int ImageSize => _palette.Count * 3 + _encodedImage.Count;

	internal List<byte> GetImageBytes()
	{
		var imageLength = 7 + ImageSize;

		var imageBytes = new List<byte>
		{
			// Image data
			0xaa, // Image start

			// Image length
			(byte)(imageLength & 0xff),
			(byte)(imageLength >> 8 & 0xff),

			// Frame duration
			(byte)(FrameTimeMs & 0xff),
			(byte)(FrameTimeMs >> 8 & 0xff),

			0x00, // Reset palette: 0, keep palette: 1

			// Number of colors
			(byte)(
			_palette.Count == 256
				? 0
				: _palette.Count
			)
		};

		// Color data
		foreach (var color in _palette)
		{
			imageBytes.Add(color.R);
			imageBytes.Add(color.G);
			imageBytes.Add(color.B);
		};

		// Pixel data
		foreach (var pixel in _encodedImage)
		{
			imageBytes.Add(pixel);
		}

		return imageBytes;
	}
}
