using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A response containing the device's settings
/// </summary>
public class GetAllConfigResponse : PixooResponse
{
	/// <summary>
	/// The brightness, 0 to 100
	/// </summary>
	[JsonPropertyName("Brightness")]
	public int Brightness { get; set; }

	/// <summary>
	/// The rotation flag
	/// </summary>
	[JsonPropertyName("RotationFlag")]
	public int RotationFlag { get; set; }

	/// <summary>
	/// The clock display time in seconds
	/// </summary>
	[JsonPropertyName("ClockTime")]
	public int ClockTime { get; set; }

	/// <summary>
	/// The gallery display time in seconds
	/// </summary>
	[JsonPropertyName("GalleryTime")]
	public int GalleryTime { get; set; }

	/// <summary>
	/// The single gallery display time (the device spells it "Galley")
	/// </summary>
	[JsonPropertyName("SingleGalleyTime")]
	public int SingleGalleryTime { get; set; }

	/// <summary>
	/// The channel shown at power on
	/// </summary>
	[JsonPropertyName("PowerOnChannelId")]
	public int PowerOnChannelId { get; set; }

	/// <summary>
	/// Whether the gallery shows the time
	/// </summary>
	[JsonPropertyName("GalleryShowTimeFlag")]
	public int GalleryShowTimeFlag { get; set; }

	/// <summary>
	/// The current dial (clock face) id
	/// </summary>
	[JsonPropertyName("CurClockId")]
	public int DialId { get; set; }

	/// <summary>
	/// 1 for a 24-hour clock
	/// </summary>
	[JsonPropertyName("Time24Flag")]
	public int Time24Flag { get; set; }

	/// <summary>
	/// 0 for Celsius, 1 for Fahrenheit
	/// </summary>
	[JsonPropertyName("TemperatureMode")]
	public int TemperatureMode { get; set; }

	/// <summary>
	/// The screen rotation angle
	/// </summary>
	[JsonPropertyName("GyrateAngle")]
	public int GyrateAngle { get; set; }

	/// <summary>
	/// 1 when the display is mirrored
	/// </summary>
	[JsonPropertyName("MirrorFlag")]
	public int MirrorFlag { get; set; }

	/// <summary>
	/// 1 when the screen is on
	/// </summary>
	[JsonPropertyName("LightSwitch")]
	public int LightSwitch { get; set; }
}
