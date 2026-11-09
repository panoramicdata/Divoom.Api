using System;
using System.Collections.Generic;
using System.Drawing;

namespace Divoom.Api.Models;

/// <summary>
/// A Divoom image
/// </summary>
public class DivoomImage
{
	private readonly List<Color> _palette = [];
	private readonly List<byte> _encodedImage = [];
	private readonly bool _isLarge;

	/// <summary>
	/// The pixel count of a 32x32 image, such as a Pixoo Max's, which uses a wider frame format.
	/// </summary>
	public static int LargeImagePixelCount => 32 * 32;

	/// <summary>
	/// The frame time in milliseconds
	/// </summary>
	public int FrameTimeMs { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DivoomImage"/> class with no frame duration.
	/// </summary>
	/// <param name="image">The pixel colors.</param>
	public DivoomImage(Color[] image) : this(image, null)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DivoomImage"/> class.
	/// </summary>
	/// <param name="image">The pixel colors.</param>
	/// <param name="frameTime">The frame duration, or <c>null</c> for none.</param>
	public DivoomImage(Color[] image, TimeSpan? frameTime)
	{
		// See https://github.com/RomRider/node-divoom-timebox-evo/blob/master/PROTOCOL.md#animations-images-and-text

		FrameTimeMs = (int)(frameTime?.TotalMilliseconds ?? 0);

		if (image.Length == LargeImagePixelCount)
		{
			_isLarge = true;
			BuildPalette(image, padTo256: false);
			EncodeLeastSignificantFirst(image, Math.Max(1, (int)Math.Ceiling(Math.Log(_palette.Count, 2))));
			return;
		}

		BuildPalette(image, padTo256: true);

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
			EncodePacked(image, bitsPerPixel);
		}
	}

	private void EncodeLeastSignificantFirst(Color[] image, int bitsPerPixel)
	{
		var accumulator = 0;
		var bitCount = 0;
		foreach (var pixel in image)
		{
			accumulator |= _palette.IndexOf(pixel) << bitCount;
			bitCount += bitsPerPixel;
			while (bitCount >= 8)
			{
				_encodedImage.Add((byte)(accumulator & 0xff));
				accumulator >>= 8;
				bitCount -= 8;
			}
		}

		if (bitCount > 0)
		{
			_encodedImage.Add((byte)(accumulator & 0xff));
		}
	}

	private void BuildPalette(Color[] image, bool padTo256)
	{
		foreach (var pixel in image)
		{
			if (_palette.IndexOf(pixel) == -1)
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
		while (padTo256 && _palette.Count < 256)
		{
			_palette.Add(Color.Black);
		}
	}

	private void EncodePacked(Color[] image, int bitsPerPixel)
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

	/// <summary>
	/// The image size in bytes
	/// </summary>
	public int ImageSize => _palette.Count * 3 + _encodedImage.Count;

	internal List<byte> GetImageBytes()
	{
		if (_isLarge)
		{
			return GetLargeImageBytes();
		}

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
		}

		// Pixel data
		foreach (var pixel in _encodedImage)
		{
			imageBytes.Add(pixel);
		}

		return imageBytes;
	}

	// Pixoo Max: AA LLLL TTTT 03 NNNN COLOR_DATA PIXEL_DATA, as hass-divoom's pixoomax encoding.
	private List<byte> GetLargeImageBytes()
	{
		var imageLength = 8 + ImageSize;

		List<byte> imageBytes =
		[
			0xaa,
			(byte)(imageLength & 0xff),
			(byte)(imageLength >> 8 & 0xff),
			(byte)(FrameTimeMs & 0xff),
			(byte)(FrameTimeMs >> 8 & 0xff),
			0x03,
			(byte)(_palette.Count & 0xff),
			(byte)(_palette.Count >> 8 & 0xff),
		];

		foreach (var color in _palette)
		{
			imageBytes.Add(color.R);
			imageBytes.Add(color.G);
			imageBytes.Add(color.B);
		}

		imageBytes.AddRange(_encodedImage);
		return imageBytes;
	}
}
