using AwesomeAssertions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Divoom.Api.Test;

public sealed class ReadOnlyOptionTests : IDisposable
{
	private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
	private readonly List<string> _requests = [];
	private readonly CancellationTokenSource _stop = new();

	public ReadOnlyOptionTests()
	{
		_listener.Start();
		_ = Task.Run(ServeAsync);
	}

	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

	private DivoomClient CreateClient(bool isReadOnly) => new(
		new DivoomClientOptions
		{
			DeviceIp = $"127.0.0.1:{Port}",
			IsReadOnly = isReadOnly,
			HttpMaxAttemptCount = 1
		},
		NullLogger.Instance);

	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> new DivoomClientOptions().IsReadOnly.Should().BeFalse();

	[Fact]
	public async Task LocalWriteCommand_WhenReadOnly_ThrowsAndSendsNothing()
	{
		using var client = CreateClient(isReadOnly: true);

		var act = () => client.Channel.SetAsync(new SetChannelRequest { Index = 1 }, CancellationToken);

		// Refit wraps handler exceptions in ApiRequestException
		(await act.Should().ThrowAsync<Refit.ApiRequestException>()).WithInnerException<InvalidOperationException>();
		_requests.Should().BeEmpty();
	}

	[Fact]
	public async Task LocalWriteCommand_WhenNotReadOnly_IsSent()
	{
		using var client = CreateClient(isReadOnly: false);

		var response = await client.Channel.SetDialAsync(new SetDialRequest { DialId = 1 }, CancellationToken);

		response.Should().NotBeNull();
		_requests.Should().ContainSingle().Which.Should().Contain("Channel/SetClockSelectId");
	}

	[Fact]
	public async Task LocalGetCommand_WhenReadOnly_IsStillSent()
	{
		using var client = CreateClient(isReadOnly: true);

		var response = await client.Channel.GetDialInfoAsync(new(), CancellationToken);

		response.Brightness.Should().Be(42);
		_requests.Should().ContainSingle().Which.Should().Contain("Channel/GetClockInfo");
	}

	[Fact]
	public async Task BluetoothWriteCommand_WhenReadOnly_Throws()
	{
		using var client = CreateClient(isReadOnly: true);

		var act = () => client.Bluetooth.Settings.SetBrightnessAsync(new DivoomBluetoothDevice(null!), 50, CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
	}

	public void Dispose()
	{
		_stop.Cancel();
		_listener.Stop();
		_stop.Dispose();
		GC.SuppressFinalize(this);
	}

	private async Task ServeAsync()
	{
		try
		{
			while (!_stop.IsCancellationRequested)
			{
				using var tcp = await _listener.AcceptTcpClientAsync(_stop.Token);
				await HandleAsync(tcp);
			}
		}
		catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or SocketException)
		{
			// Listener stopped
		}
	}

	private async Task HandleAsync(TcpClient tcp)
	{
		var stream = tcp.GetStream();
		var buffer = new byte[8192];
		var received = new StringBuilder();
		while (true)
		{
			var read = await stream.ReadAsync(buffer, _stop.Token);
			received.Append(Encoding.UTF8.GetString(buffer, 0, read));
			var text = received.ToString();
			var headerEnd = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
			if (read == 0 || (headerEnd >= 0 && text.Length - headerEnd - 4 >= ContentLength(text)))
			{
				break;
			}
		}

		lock (_requests)
		{
			_requests.Add(received.ToString());
		}

		const string body = "{\"error_code\":0,\"ClockId\":1,\"Brightness\":42}";
		var response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n{body}";
		await stream.WriteAsync(Encoding.UTF8.GetBytes(response), _stop.Token);
	}

	private static int ContentLength(string text)
	{
		const string header = "Content-Length:";
		var index = text.IndexOf(header, StringComparison.OrdinalIgnoreCase);
		if (index < 0)
		{
			return 0;
		}

		var end = text.IndexOf("\r\n", index, StringComparison.Ordinal);
		return int.Parse(text[(index + header.Length)..end].Trim(), System.Globalization.CultureInfo.InvariantCulture);
	}
}
