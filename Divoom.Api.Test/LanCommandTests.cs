using AwesomeAssertions;
using Divoom.Api.Exceptions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Divoom.Api.Test;

public sealed class LanCommandTests : IDisposable
{
	// Captured from a Pixoo 64 on 2026-11-08
	private const string CapturedAllConfig = "{\"error_code\":0,\"Brightness\":100,\"RotationFlag\":0,\"ClockTime\":60,\"GalleryTime\":60,\"SingleGalleyTime\":0,\"PowerOnChannelId\":0,\"GalleryShowTimeFlag\":0,\"CurClockId\":64,\"Time24Flag\":0,\"TemperatureMode\":0,\"GyrateAngle\":0,\"MirrorFlag\":0,\"LightSwitch\":1}";

	private readonly FakeDevice _device = new();

	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private DivoomClient CreateClient(bool isReadOnly = false) => new(
		new DivoomClientOptions
		{
			DeviceIp = $"127.0.0.1:{_device.Port}",
			IsReadOnly = isReadOnly,
			HttpMaxAttemptCount = 1
		},
		NullLogger.Instance);

	private JsonElement SingleBody()
		=> JsonDocument.Parse(_device.Bodies.Should().ContainSingle().Subject).RootElement;

	[Fact]
	public async Task GetAllConfigAsync_ParsesCapturedResponse()
	{
		_device.Respond = _ => CapturedAllConfig;
		using var client = CreateClient(isReadOnly: true);

		var config = await client.Channel.GetAllConfigAsync(new(), CancellationToken);

		config.Brightness.Should().Be(100);
		config.DialId.Should().Be(64);
		config.LightSwitch.Should().Be(1);
		config.ClockTime.Should().Be(60);
		SingleBody().GetProperty("Command").GetString().Should().Be("Channel/GetAllConf");
	}

	[Fact]
	public async Task Requests_AreSentWithContentLength_BecauseTheDeviceRejectsChunkedBodies()
	{
		using var client = CreateClient();

		var response = await client.Channel.SetBrightnessAsync(new SetBrightnessRequest { Brightness = 30 }, CancellationToken);

		ChannelExtensions.EnsureSuccess(response);
		_device.Requests.Should().ContainSingle().Which.Should().Contain("Content-Length:").And.NotContain("chunked");
	}

	[Fact]
	public void Client_WithoutDeviceIp_CanBeCreated_ButChannelExplains()
	{
		using var client = new DivoomClient(new DivoomClientOptions(), NullLogger.Instance);

		client.Gz.Should().NotBeNull();
		var act = () => client.Channel;
		act.Should().Throw<InvalidOperationException>().WithMessage("*DeviceIp*");
	}

	[Fact]
	public void LanDevice_ParsesCapturedDiscoveryResponse()
	{
		// Captured from https://app.divoom-gz.com/Device/ReturnSameLANDevice on 2026-11-08
		const string captured = "{\"ReturnCode\":0,\"ReturnMessage\":\"\",\"DeviceList\":[{\"DeviceName\":\"Office\",\"DeviceId\":300030325,\"DevicePrivateIP\":\"192.168.1.131\",\"DeviceMac\":\"94e6860c4370\",\"Hardware\":90}]}";

		var response = JsonSerializer.Deserialize<GetLanDevicesResponse>(captured)!;

		var device = response.Devices.Should().ContainSingle().Subject;
		device.Name.Should().Be("Office");
		device.PrivateIp.Should().Be("192.168.1.131");
		device.Hardware.Should().Be(90);
	}

	[Fact]
	public async Task GetDeviceTimeAsync_ParsesResponse()
	{
		_device.Respond = _ => "{\"error_code\":0,\"UTCTime\":1794151499,\"LocalTime\":\"2026-11-08 15:24:59\"}";
		using var client = CreateClient(isReadOnly: true);

		var time = await client.Channel.GetDeviceTimeAsync(new(), CancellationToken);

		time.UtcTime.Should().Be(1794151499);
		time.LocalTime.Should().Be("2026-11-08 15:24:59");
	}

	[Fact]
	public async Task SetBrightnessAsync_SendsBrightness()
	{
		using var client = CreateClient();

		await client.Channel.SetBrightnessAsync(new SetBrightnessRequest { Brightness = 30 }, CancellationToken);

		var body = SingleBody();
		body.GetProperty("Command").GetString().Should().Be("Channel/SetBrightness");
		body.GetProperty("Brightness").GetInt32().Should().Be(30);
	}

	[Theory]
	[InlineData(true, 1)]
	[InlineData(false, 0)]
	public async Task SetScreenPowerAsync_SendsOnOffAsNumber(bool isOn, int expected)
	{
		using var client = CreateClient();

		await client.Channel.SetScreenPowerAsync(new SetScreenPowerRequest { IsOn = isOn }, CancellationToken);

		var body = SingleBody();
		body.GetProperty("Command").GetString().Should().Be("Channel/OnOffScreen");
		body.GetProperty("OnOff").GetInt32().Should().Be(expected);
		body.TryGetProperty("IsOn", out _).Should().BeFalse();
	}

	[Fact]
	public async Task PlayBuzzerAsync_SendsCycleTimes()
	{
		using var client = CreateClient();

		await client.Channel.PlayBuzzerAsync(new PlayBuzzerRequest { PlayTotalTimeMs = 3000 }, CancellationToken);

		var body = SingleBody();
		body.GetProperty("Command").GetString().Should().Be("Device/PlayBuzzer");
		body.GetProperty("PlayTotalTime").GetInt32().Should().Be(3000);
		body.GetProperty("ActiveTimeInCycle").GetInt32().Should().Be(500);
	}

	[Fact]
	public async Task ShowTextAsync_SendsNextGifIdThenText()
	{
		_device.Respond = body => body.Contains("Draw/GetHttpGifId", StringComparison.Ordinal)
			? "{\"error_code\":0,\"PicId\":7}"
			: "{\"error_code\":0}";
		using var client = CreateClient();

		await client.Channel.ShowTextAsync("BUSY", "#FF0000", "#FFFF00", 64, CancellationToken);

		var bodies = _device.Bodies.Select(b => JsonDocument.Parse(b).RootElement).ToList();
		bodies.Select(b => b.GetProperty("Command").GetString())
			.Should().Equal("Draw/GetHttpGifId", "Draw/SendHttpGif", "Draw/SendHttpText");

		var gif = bodies[1];
		gif.GetProperty("PicID").GetInt32().Should().Be(8);
		gif.GetProperty("PicWidth").GetInt32().Should().Be(64);
		var pixels = Convert.FromBase64String(gif.GetProperty("PicData").GetString()!);
		pixels.Should().HaveCount(64 * 64 * 3);
		pixels.Take(3).Should().Equal(0xFF, 0x00, 0x00);

		// Geometry confirmed on a Pixoo 64: a narrower box left the scroll short of the right edge
		var text = bodies[2];
		text.GetProperty("TextString").GetString().Should().Be("BUSY");
		text.GetProperty("color").GetString().Should().Be("#FFFF00");
		text.GetProperty("y").GetInt32().Should().Be(24);
		text.GetProperty("TextWidth").GetInt32().Should().Be(64);
	}

	[Fact]
	public async Task ShowTextAsync_WhenDeviceRejects_Throws()
	{
		_device.Respond = _ => "{\"error_code\":1}";
		using var client = CreateClient();

		var act = () => client.Channel.ShowTextAsync("BUSY", "#FF0000", "#FFFFFF", 64, CancellationToken);

		await act.Should().ThrowAsync<DivoomBadRequestException>();
	}

	[Fact]
	public async Task ShowTextAsync_WithBadColour_ThrowsBeforeSending()
	{
		using var client = CreateClient();

		var act = () => client.Channel.ShowTextAsync("BUSY", "red", "#FFFFFF", 64, CancellationToken);

		await act.Should().ThrowAsync<ArgumentException>();
		_device.Requests.Should().BeEmpty();
	}

	[Fact]
	public async Task ReadCommands_WhenReadOnly_AreSent()
	{
		using var client = CreateClient(isReadOnly: true);

		await client.Channel.GetAllConfigAsync(new(), CancellationToken);
		await client.Channel.GetDeviceTimeAsync(new(), CancellationToken);
		await client.Channel.GetHttpGifIdAsync(new(), CancellationToken);

		_device.Requests.Should().HaveCount(3);
	}

	public static TheoryData<string> WriteCommands => ["brightness", "screen", "reset", "gif", "text", "clear", "buzzer"];

	[Theory]
	[MemberData(nameof(WriteCommands))]
	public async Task WriteCommands_WhenReadOnly_AreRefused(string command)
	{
		using var client = CreateClient(isReadOnly: true);
		var channel = client.Channel;
		Func<Task> act = command switch
		{
			"brightness" => () => channel.SetBrightnessAsync(new SetBrightnessRequest { Brightness = 1 }, CancellationToken),
			"screen" => () => channel.SetScreenPowerAsync(new SetScreenPowerRequest(), CancellationToken),
			"reset" => () => channel.ResetHttpGifIdAsync(new(), CancellationToken),
			"gif" => () => channel.SendHttpGifAsync(SendHttpGifRequest.SolidColor(1, "#000000"), CancellationToken),
			"text" => () => channel.SendHttpTextAsync(new SendHttpTextRequest { Text = "x" }, CancellationToken),
			"clear" => () => channel.ClearHttpTextAsync(new(), CancellationToken),
			_ => () => channel.PlayBuzzerAsync(new PlayBuzzerRequest(), CancellationToken)
		};

		(await act.Should().ThrowAsync<Refit.ApiRequestException>()).WithInnerException<InvalidOperationException>();
		_device.Requests.Should().BeEmpty();
	}

	public void Dispose() => _device.Dispose();
}
