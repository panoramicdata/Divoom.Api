using System;

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
						if (@byte != 0x4a)
						{
							throw new FormatException($"This does not appear to be a valid response because the byte at index {byteIndex - 1} is not 0x4a.");
						}

						break;
					case 5:
						if (@byte != 0x7f && @byte != 0x00)
						{
							throw new FormatException($"This does not appear to be a valid response because the byte at index {byteIndex - 1} is not 0x7f or 0x00.");
						}

						Byte5Is7f = @byte == 0x7f;

						break;
					case 6 or 10:
						BrightnessPercent = @byte;
						break;
					default:
						//017f007f00010001000100
						break;
				}
			}
		}
	}

	public Channel CurrentChannel { get; }

	public int BrightnessPercent { get; }

	public bool Byte5Is7f { get; }
}
