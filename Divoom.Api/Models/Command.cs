namespace Divoom.Api.Models;

/// <summary>
/// Bluetooth command type
/// </summary>
public enum Command
{
	/// <summary>No command</summary>
	NoCommand = 0x00,
	/// <summary>Response</summary>
	Response = 0x04,
	/// <summary>Radio</summary>
	Radio = 0x05,
	/// <summary>Set volume</summary>
	SetVolume = 0x08,
	/// <summary>Get volume</summary>
	GetVolume = 0x09,
	/// <summary>Set mute state</summary>
	SetMuteState = 0x0a,
	/// <summary>Get mute state</summary>
	GetMuteState = 0x0b,
	/// <summary>Set date and time</summary>
	SetDateTime = 0x18,
	/// <summary>Info brightness 0x31</summary>
	InfoBrightness31 = 0x31,
	/// <summary>Info brightness 0x32</summary>
	InfoBrightness32 = 0x32,
	/// <summary>Set static image</summary>
	SetStaticImage = 0x44,
	/// <summary>Set channel</summary>
	SetChannel = 0x45,
	/// <summary>Get settings</summary>
	GetSettings = 0x46,
	/// <summary>Set color</summary>
	SetColor = 0x47,
	/// <summary>Set animation frame</summary>
	SetAnimationFrame = 0x49,
	/// <summary>Set temperature unit</summary>
	SetTemperatureUnit = 0x4c,
	/// <summary>Get weather</summary>
	GetWeather = 0x59,
	/// <summary>Set weather</summary>
	SetWeather = 0x5F,
	/// <summary>Get radio frequency</summary>
	GetRadioFrequency = 0x60,
	/// <summary>Set radio frequency</summary>
	SetRadioFrequency = 0x61,
	/// <summary>Set brightness</summary>
	SetBrightness = 0x74,
	/// <summary>Bad request</summary>
	BadRequest = 0xBD
}
