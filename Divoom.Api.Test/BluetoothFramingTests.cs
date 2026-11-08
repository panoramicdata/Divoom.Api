using AwesomeAssertions;
using Divoom.Api.Implementations;
using Divoom.Api.Models;
using System.Data;
using System.Net;
using System.Net.Sockets;

namespace Divoom.Api.Test;

/// <summary>
/// Framing per the timebox-evo protocol: 01 LLLL PAYLOAD CRCR 02, where LLLL is the payload length plus 2
/// and CRCR is the sum of LLLL and PAYLOAD, both least significant byte first.
/// </summary>
public class BluetoothFramingTests
{
	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	[Fact]
	public void CommandBuilder_FramesPayloadWithLengthChecksumAndEndByte()
	{
		var builder = new CommandBuilder();
		builder.Add((byte)Command.SetBrightness);
		builder.Add(50);

		// LL = 2 + 2 = 4; CRC = 0x04 + 0x00 + 0x74 + 0x32 = 0xAA
		builder.GetBytes().Should().Equal(0x01, 0x04, 0x00, 0x74, 0x32, 0xAA, 0x00, 0x02);
		builder.CommandByte.Should().Be((byte)Command.SetBrightness);
	}

	[Fact]
	public void CommandBuilder_ChecksumCarriesIntoHighByte()
	{
		var builder = new CommandBuilder();
		builder.Add(0xFF);
		builder.Add(0xFF);

		// CRC = 0x04 + 0xFF + 0xFF = 0x0202
		builder.GetBytes().Should().Equal(0x01, 0x04, 0x00, 0xFF, 0xFF, 0x02, 0x02, 0x02);
	}

	[Fact]
	public void CommandBuilder_WhenEmpty_HasNoCommandByte()
	{
		var builder = new CommandBuilder();

		builder.CommandByte.Should().BeNull();
		builder.GetBytes().Should().Equal(0x01, 0x02, 0x00, 0x02, 0x00, 0x02);
	}

	[Theory]
	[InlineData(0x01, 0x04)]
	[InlineData(0x02, 0x05)]
	[InlineData(0x03, 0x06)]
	public void CommandBuilder_AddWithEscaping_EscapesFramingBytes(byte value, byte escaped)
	{
		var builder = new CommandBuilder();
		builder.AddWithEscaping(value);

		// Length and checksum count the logical byte, not the escape prefix
		builder.GetBytes().Should().Equal(0x01, 0x03, 0x00, 0x03, escaped, (byte)(0x03 + value), 0x00, 0x02);
	}

	[Fact]
	public void CommandBuilder_AddWithEscaping_LeavesOtherBytesAlone()
	{
		var builder = new CommandBuilder();
		builder.AddWithEscaping(0x04);

		builder.GetBytes().Should().Equal(0x01, 0x03, 0x00, 0x04, 0x07, 0x00, 0x02);
	}

	[Fact]
	public async Task Reader_ReadsAFrame()
	{
		var frame = Frame(0x04, 0x46, 0x55, 0x05, 0x10);

		var responses = await ReadAllAsync(frame);

		var response = responses.Should().ContainSingle().Subject;
		response.Command.Should().Be(Command.GetSettings);
		response.IsOk.Should().BeTrue();
		response.Bytes.Should().Equal(0x05, 0x10);
	}

	[Fact]
	public async Task Reader_ReadsBackToBackFrames()
	{
		var responses = await ReadAllAsync([.. Frame(0x04, 0x08, 0x55, 0x07), .. Frame(0x04, 0x09, 0x55, 0x0A)]);

		responses.Select(r => r.Command).Should().Equal(Command.SetVolume, Command.GetVolume);
		responses[0].Bytes.Should().Equal(0x07);
		responses[1].Bytes.Should().Equal(0x0A);
	}

	[Fact]
	public async Task Reader_UnescapesPayloadBytes()
	{
		// Snapshot of the reader's convention: LL counts the bytes on the wire, escape prefix included
		byte[] wirePayload = [0x04, 0x09, 0x55, 0x03, 0x04];
		var length = wirePayload.Length + 2;
		var crc = length + 0x04 + 0x09 + 0x55 + 0x01;
		byte[] frame = [0x01, (byte)length, 0x00, .. wirePayload, (byte)(crc & 0xFF), (byte)(crc >> 8), 0x02];

		var responses = await ReadAllAsync(frame);

		responses.Should().ContainSingle().Which.Bytes.Should().Equal(0x01);
	}

	[Fact]
	public async Task Reader_WithBadChecksum_ReturnsEmpty()
	{
		var frame = Frame(0x04, 0x46, 0x55, 0x05);
		frame[^3] ^= 0xFF;

		(await ReadAllAsync(frame)).Should().BeEmpty();
	}

	[Fact]
	public async Task Reader_WithBadStartByte_ReturnsEmpty()
	{
		var frame = Frame(0x04, 0x46, 0x55, 0x05);
		frame[0] = 0x07;

		(await ReadAllAsync(frame)).Should().BeEmpty();
	}

	[Fact]
	public async Task Reader_WithTruncatedFrame_ReturnsEmpty()
	{
		var frame = Frame(0x04, 0x46, 0x55, 0x05);

		(await ReadAllAsync(frame[..^2])).Should().BeEmpty();
	}

	[Fact]
	public void DeviceResponse_ParsesCommandAndStatus()
	{
		var response = new DeviceResponse([0x04, 0x09, 0x55, 0x0A, 0xFF]);

		response.Command.Should().Be(Command.GetVolume);
		response.IsOk.Should().BeTrue();
		response.IsEmpty.Should().BeFalse();
		response.Bytes.Should().Equal(0x0A, 0xFF);
		response.ToString().Should().Be("GetVolume OK: 0A:FF");
	}

	[Fact]
	public void DeviceResponse_WithoutOkMarker_IsNotOk()
	{
		var response = new DeviceResponse([0x04, 0x74, 0x00]);

		response.IsOk.Should().BeFalse();
		response.IsEmpty.Should().BeTrue();
		response.ToString().Should().Be("SetBrightness Not OK: ");
	}

	[Fact]
	public void DeviceResponse_ForSetChannel_NamesTheChannel()
		=> new DeviceResponse([0x04, 0x45, 0x55, 0x06]).ToString().Should().Be("SetChannel OK: Scoreboard");

	[Fact]
	public void DeviceResponse_NotStartingWithResponseMarker_Throws()
	{
		var act = () => new DeviceResponse([0x05, 0x45, 0x55]);

		act.Should().Throw<DataException>();
	}

	[Fact]
	public void DeviceResponse_FromNoBytes_IsEmpty()
	{
		var response = new DeviceResponse([]);

		response.IsEmpty.Should().BeTrue();
		response.Command.Should().Be(Command.NoCommand);
	}

	[Fact]
	public void DeviceResponseSet_IsOkOnlyWhenEveryResponseIs()
	{
		var ok = new DeviceResponse([0x04, 0x46, 0x55]);
		var notOk = new DeviceResponse([0x04, 0x46, 0x00]);

		new DeviceResponseSet([ok, ok]).IsOk.Should().BeTrue();
		new DeviceResponseSet([ok, notOk]).IsOk.Should().BeFalse();
	}

	[Fact]
	public void DeviceSettings_ReadsChannelAndBrightnessAcrossResponses()
	{
		// Snapshot of the decoded offsets: 0 channel, 5 is 0x7f, 6 then 10 brightness, counted across responses
		var first = new DeviceResponse([0x04, 0x46, 0x55, (byte)Channel.Lighting, 0, 0, 0, 0, 0x7f, 30]);
		var second = new DeviceResponse([0x04, 0x46, 0x55, 0, 0, 0, 75, 0]);

		var settings = new DeviceSettings(new DeviceResponseSet([first, second]));

		settings.CurrentChannel.Should().Be(Channel.Lighting);
		settings.Byte5Is7f.Should().BeTrue();
		settings.BrightnessPercent.Should().Be(75);
	}

	[Fact]
	public void DeviceSettings_FromOneShortResponse_UsesByteSix()
	{
		var response = new DeviceResponse([0x04, 0x46, 0x55, (byte)Channel.Clock, 0, 0, 0, 0, 0x00, 40]);

		var settings = new DeviceSettings(new DeviceResponseSet([response]));

		settings.CurrentChannel.Should().Be(Channel.Clock);
		settings.Byte5Is7f.Should().BeFalse();
		settings.BrightnessPercent.Should().Be(40);
	}

	private static byte[] Frame(params byte[] payload)
	{
		var length = payload.Length + 2;
		var crc = (length & 0xFF) + (length >> 8) + payload.Sum(b => b);
		return [0x01, (byte)(length & 0xFF), (byte)(length >> 8), .. payload, (byte)(crc & 0xFF), (byte)(crc >> 8), 0x02];
	}

	private static async Task<List<DeviceResponse>> ReadAllAsync(byte[] bytes)
	{
		using var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		using var client = new TcpClient();
		await client.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port, CancellationToken);
		using var server = await listener.AcceptTcpClientAsync(CancellationToken);
		await server.GetStream().WriteAsync(bytes, CancellationToken);

		// Read stops at the first moment no data is available, so wait for every byte to arrive
		var deadline = DateTime.UtcNow.AddSeconds(5);
		while (client.Available < bytes.Length && DateTime.UtcNow < deadline)
		{
			await Task.Delay(10, CancellationToken);
		}

		client.Available.Should().Be(bytes.Length);

		var stream = client.GetStream();
		var responses = new List<DeviceResponse>();
		while (true)
		{
			var response = DeviceResponseReader.Read(stream);
			if (response.IsEmpty)
			{
				return responses;
			}

			responses.Add(response);
		}
	}
}
