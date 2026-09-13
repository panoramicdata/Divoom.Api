using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

/// <summary>
/// Sends images and animations to a device.
/// </summary>
internal sealed class BluetoothImageCommands(DeviceConnection connection) : IBluetoothImages
{
	private readonly DeviceConnection _connection = connection;

	/// <summary>
	/// Views an image
	/// </summary>
	/// <param name="device">The device</param>
	/// <param name="divoomImage">An array of 256 colors, one for each pixel starting top left, moving left to right, then top to bottom.</param>
	/// <param name="cancellationToken">The CancellationToken</param>
	/// <returns></returns>
	/// <exception cref="NotSupportedException"></exception>
	public async Task<DeviceResponseSet> ViewImageAsync(
		DivoomBluetoothDevice device,
		DivoomImage divoomImage,
		CancellationToken cancellationToken)
	{
		// 44000A0A04 AA LLLL 000000 NN COLOR_DATA PIXEL_DATA
		// |<-HEAD->| |<-----------IMAGE_DATA-------------->|

		var commandBuilder = new CommandBuilder();

		// HEAD
		commandBuilder.Add((byte)Command.SetStaticImage);
		commandBuilder.Add(0x00); // Fixed
		commandBuilder.Add(0x0a); // Fixed
		commandBuilder.Add(0x0a); // Fixed
		commandBuilder.Add(0x04); // Fixed

		var imageBytes = divoomImage.GetImageBytes();
		foreach (var imageByte in imageBytes)
		{
			commandBuilder.Add(imageByte);
		}

		return await _connection.SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public async Task<DeviceResponseSet> ViewAnimationAsync(
		DivoomBluetoothDevice device,
		DivoomAnimation animation,
		CancellationToken cancellationToken)
	{
		var animationLength = animation.TotalFrameLength;

		var packetIndex = 0;
		while (true)
		{
			var frameDataBytes = animation.GetPacket(packetIndex);

			if (frameDataBytes.Count == 0)
			{
				break;
			}

			var commandBuilder = BuildAnimationFrameCommand(animationLength, packetIndex++, frameDataBytes);

			_ = await _connection.SendCommandAsync(device, commandBuilder, cancellationToken);
		}

		// TODO
		return new DeviceResponseSet([]);
	}

	/// <summary>
	/// Builds the command for a single animation frame.
	/// </summary>
	private static CommandBuilder BuildAnimationFrameCommand(
		int animationLength,
		int packetIndex,
		List<byte> frameDataBytes)
	{
		var commandBuilder = new CommandBuilder();

		// HEAD
		commandBuilder.Add((byte)Command.SetAnimationFrame);

		// Animation length
		commandBuilder.Add((byte)(animationLength & 0xff));
		commandBuilder.Add((byte)(animationLength >> 8 & 0xff));

		commandBuilder.Add((byte)packetIndex);

		foreach (var frameDataByte in frameDataBytes)
		{
			commandBuilder.Add(frameDataByte);
		}

		return commandBuilder;
	}
}
