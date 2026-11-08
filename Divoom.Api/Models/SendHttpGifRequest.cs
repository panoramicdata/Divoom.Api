using Divoom.Api.Interfaces;
using System;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A request to send one frame of an animation over HTTP
/// </summary>
public class SendHttpGifRequest : ICommand
{
	/// <summary>
	/// The command
	/// </summary>
	[JsonPropertyName("Command")]
	public string Command => "Draw/SendHttpGif";

	/// <summary>
	/// The number of frames in the animation
	/// </summary>
	[JsonPropertyName("PicNum")]
	public int FrameCount { get; set; } = 1;

	/// <summary>
	/// The width (and height) in pixels: 16, 32 or 64
	/// </summary>
	[JsonPropertyName("PicWidth")]
	public int Width { get; set; } = 64;

	/// <summary>
	/// The zero-based index of this frame
	/// </summary>
	[JsonPropertyName("PicOffset")]
	public int FrameIndex { get; set; }

	/// <summary>
	/// The animation id, which must be higher than the last one sent (see <see cref="GetHttpGifIdResponse"/>)
	/// </summary>
	[JsonPropertyName("PicID")]
	public int PicId { get; set; }

	/// <summary>
	/// Milliseconds per frame
	/// </summary>
	[JsonPropertyName("PicSpeed")]
	public int FrameDurationMs { get; set; } = 1000;

	/// <summary>
	/// Base64 of Width x Width RGB bytes, row by row
	/// </summary>
	[JsonPropertyName("PicData")]
	public string PicData { get; set; } = string.Empty;

	/// <summary>
	/// Creates a single-frame animation filled with one colour.
	/// </summary>
	/// <param name="picId">The animation id.</param>
	/// <param name="hexColor">The colour, as #RRGGBB.</param>
	/// <param name="width">The width in pixels.</param>
	public static SendHttpGifRequest SolidColor(int picId, string hexColor, int width = 64)
	{
		var (red, green, blue) = ParseHexColor(hexColor);
		var pixels = new byte[width * width * 3];
		for (var index = 0; index < pixels.Length; index += 3)
		{
			pixels[index] = red;
			pixels[index + 1] = green;
			pixels[index + 2] = blue;
		}

		return new SendHttpGifRequest
		{
			PicId = picId,
			Width = width,
			PicData = Convert.ToBase64String(pixels)
		};
	}

	internal static (byte Red, byte Green, byte Blue) ParseHexColor(string hexColor)
	{
		ArgumentNullException.ThrowIfNull(hexColor);
		var hex = hexColor.TrimStart('#');
		if (hex.Length != 6)
		{
			throw new ArgumentException($"'{hexColor}' is not a #RRGGBB colour.", nameof(hexColor));
		}

		var value = Convert.ToInt32(hex, 16);
		return ((byte)(value >> 16), (byte)(value >> 8), (byte)value);
	}
}
