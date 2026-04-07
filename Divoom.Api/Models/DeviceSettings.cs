namespace Divoom.Api.Models;

/// <summary>
/// Device settings
/// </summary>
public class DeviceSettings
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DeviceSettings"/> class.
	/// </summary>
	/// <param name="deviceResponseSet">The device response set.</param>
	public DeviceSettings(DeviceResponseSet deviceResponseSet)
	{
		var byteIndex = 0;
		foreach (var deviceResponse in deviceResponseSet.Responses)
		{
			foreach (var @byte in deviceResponse.Bytes)
			{
				switch (byteIndex++)
				{
					case 0:
						CurrentChannel = (Channel)@byte;
						break;
					case 1 or 2 or 4:
						break;
					case 3:
						break;
					case 5:
						Byte5Is7f = @byte == 0x7f;

						break;
					case 6 or 10:
						BrightnessPercent = @byte;
						break;
					default:
						//017f007f00010001000100
						//00:00:00:FF:00:FF:64:00:01:0B:64:01:FF:FF:00:00:01:01:01:01
						break;
				}
			}
		}
	}

	/// <summary>
	/// The current channel
	/// </summary>
	public Channel CurrentChannel { get; }

	/// <summary>
	/// The brightness percentage
	/// </summary>
	public int BrightnessPercent { get; }

	/// <summary>
	/// Whether byte 5 is 0x7f
	/// </summary>
	public bool Byte5Is7f { get; }
}
