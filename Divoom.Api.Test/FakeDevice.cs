using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Divoom.Api.Test;

/// <summary>
/// A loopback HTTP server standing in for a device's local API, recording each raw request.
/// </summary>
public sealed class FakeDevice : IDisposable
{
	private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
	private readonly List<string> _requests = [];
	private readonly CancellationTokenSource _stop = new();

	public FakeDevice()
	{
		_listener.Start();
		_ = Task.Run(ServeAsync);
	}

	public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

	/// <summary>
	/// Returns the response body for a request body; the default answers every command with success.
	/// </summary>
	public Func<string, string> Respond { get; set; } = _ => "{\"error_code\":0,\"ClockId\":1,\"Brightness\":42}";

	public IReadOnlyList<string> Requests
	{
		get
		{
			lock (_requests)
			{
				return [.. _requests];
			}
		}
	}

	/// <summary>
	/// The JSON body of each request, in order.
	/// </summary>
	public IReadOnlyList<string> Bodies => [.. Requests.Select(BodyOf)];

	public void Dispose()
	{
		_stop.Cancel();
		_listener.Stop();
		_stop.Dispose();
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
		var buffer = new byte[65536];
		var received = new StringBuilder();
		while (true)
		{
			var read = await stream.ReadAsync(buffer, _stop.Token);
			received.Append(Encoding.UTF8.GetString(buffer, 0, read));
			var text = received.ToString();
			if (read == 0 || IsComplete(text))
			{
				break;
			}
		}

		var request = received.ToString();
		lock (_requests)
		{
			_requests.Add(request);
		}

		// A Pixoo 64 answers a chunked body this way, whatever it contains
		var body = IsChunked(request)
			? "{\"error_code\": \"Request data illegal json\"}"
			: Respond(BodyOf(request));
		var response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {Encoding.UTF8.GetByteCount(body)}\r\nConnection: close\r\n\r\n{body}";
		await stream.WriteAsync(Encoding.UTF8.GetBytes(response), _stop.Token);
	}

	private static bool IsChunked(string text)
		=> text.Contains("Transfer-Encoding: chunked", StringComparison.OrdinalIgnoreCase);

	private static bool IsComplete(string text)
	{
		var headerEnd = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
		if (headerEnd < 0)
		{
			return false;
		}

		return IsChunked(text)
			? text.EndsWith("0\r\n\r\n", StringComparison.Ordinal)
			: text.Length - headerEnd - 4 >= ContentLength(text);
	}

	private static string BodyOf(string request)
	{
		var headerEnd = request.IndexOf("\r\n\r\n", StringComparison.Ordinal);
		if (headerEnd < 0)
		{
			return string.Empty;
		}

		var body = request[(headerEnd + 4)..];
		if (!IsChunked(request))
		{
			return body;
		}

		var decoded = new StringBuilder();
		var position = 0;
		while (true)
		{
			var lineEnd = body.IndexOf("\r\n", position, StringComparison.Ordinal);
			var size = Convert.ToInt32(body[position..lineEnd], 16);
			if (size == 0)
			{
				return decoded.ToString();
			}

			decoded.Append(body, lineEnd + 2, size);
			position = lineEnd + 2 + size + 2;
		}
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
