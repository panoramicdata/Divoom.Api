namespace Divoom.Api.Models
{
	public enum Command
	{
		Radio = 0x05,
		SetVolume = 0x08,
		GetVolume = 0x09,
		SetMuteState = 0x0a,
		GetMuteState = 0x0b,
		SetDateTime = 0x18,
		SetStaticImage = 0x44,
		SetChannel = 0x45,
		SetAnimationFrame = 0x49,
		GetTemperature = 0x59,
		SetTemperatureAndWeather = 0x5F,
		GetRadioFrequency = 0x60,
		SetRadioFrequency = 0x61,
		SetBrightness = 0x74,
	}
}
