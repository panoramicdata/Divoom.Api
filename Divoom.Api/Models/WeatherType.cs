namespace Divoom.Api.Models;

/// <summary>
/// The weather type
/// </summary>
public enum WeatherType
{
	/// <summary>Clear</summary>
	Clear = 0x01,
	/// <summary>Cloudy</summary>
	Cloudy = 0x03,
	/// <summary>Thunderstorm</summary>
	Thunderstorm = 0x05,
	/// <summary>Rain</summary>
	Rain = 0x06,
	/// <summary>Snow</summary>
	Snow = 0x08,
	/// <summary>Fog</summary>
	Fog = 0x09,
}
