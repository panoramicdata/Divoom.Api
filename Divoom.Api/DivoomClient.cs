using Divoom.Api.Implementations;
using Divoom.Api.Interfaces;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Net.Http;

namespace Divoom.Api;

/// <summary>
/// A Divoom API client
/// </summary>
public class DivoomClient : IDisposable
{
	/// <summary>
	/// The client options
	/// </summary>
	public DivoomClientOptions Options { get; }

	private readonly DivoomHttpClientHandler _httpClientHandler;
	private readonly HttpClient _httpClient;
	private readonly Lazy<IChannel> _channel;
	private HttpClient? _localHttpClient;
	private bool _disposedValue;

	/// <summary>
	/// Initializes a new instance of the <see cref="DivoomClient"/> class.
	/// </summary>
	/// <param name="options">The client options.</param>
	/// <param name="logger">The logger.</param>
	public DivoomClient(DivoomClientOptions options, ILogger logger)
	{
		Options = options;

		var refitSettings = new RefitSettings
		{
			ContentSerializer = new CustomJsonContentSerializer(Options, logger),
			// Devices answer a chunked body with "Request data illegal json"
			Buffered = true
		};

		_httpClientHandler = new DivoomHttpClientHandler(options
			?? throw new ArgumentNullException(nameof(options)), this, logger);

		_httpClient = new HttpClient(_httpClientHandler)
		{
			BaseAddress = new Uri("https://app.divoom-gz.com"),
			Timeout = TimeSpan.FromSeconds(options.HttpClientTimeoutSeconds)
		};

		Gz = RestService.For<IGz>(_httpClient, refitSettings);

		// Lazy so that a client with no DeviceIp can still discover devices through Gz
		_channel = new Lazy<IChannel>(() =>
		{
			if (string.IsNullOrWhiteSpace(options.DeviceIp))
			{
				throw new InvalidOperationException($"Set {nameof(DivoomClientOptions.DeviceIp)} to use the local device API.");
			}

			_localHttpClient = new HttpClient(_httpClientHandler, disposeHandler: false)
			{
				BaseAddress = new Uri($"http://{options.DeviceIp}"),
				Timeout = TimeSpan.FromSeconds(options.HttpClientTimeoutSeconds)
			};
			return RestService.For<IChannel>(_localHttpClient, refitSettings);
		});

		Bluetooth = new BluetoothManager(options, logger);
	}

	/// <summary>
	/// The Gz API interface
	/// </summary>
	public IGz Gz { get; }

	/// <summary>
	/// The local device API, which needs <see cref="DivoomClientOptions.DeviceIp"/>
	/// </summary>
	public IChannel Channel => _channel.Value;

	/// <summary>
	/// The Bluetooth interface
	/// </summary>
	public IBluetooth Bluetooth { get; }

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_localHttpClient?.Dispose();
				_httpClient.Dispose();
				_httpClientHandler.Dispose();
			}

			_disposedValue = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
