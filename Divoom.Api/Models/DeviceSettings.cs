namespace Divoom.Api.Models;

public class DeviceSettings
{
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

	public Channel CurrentChannel { get; }

	public int BrightnessPercent { get; }

	public bool Byte5Is7f { get; }
}
