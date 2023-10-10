namespace Divoom.Api.Models;

public enum Command
{
	NoCommand = 0x00,
	Response = 0x04,
	Radio = 0x05,
	SetVolume = 0x08,
	GetVolume = 0x09,
	SetMuteState = 0x0a,
	GetMuteState = 0x0b,
	SetDateTime = 0x18,
	InfoBrightness31 = 0x31,
	InfoBrightness32 = 0x32,
	SetStaticImage = 0x44,
	SetChannel = 0x45,
	GetSettings = 0x46,
	SetColor = 0x47,
	SetAnimationFrame = 0x49,
	SetTemperatureUnit = 0x4c,
	GetWeather = 0x59,
	SetWeather = 0x5F,
	GetRadioFrequency = 0x60,
	SetRadioFrequency = 0x61,
	SetBrightness = 0x74,
	BadRequest = 0xBD
}
