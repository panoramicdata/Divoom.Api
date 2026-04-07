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

	private readonly ILogger _logger;
	private readonly RefitSettings _refitSettings;
	private readonly DivoomHttpClientHandler _httpClientHandler;
	private readonly HttpClient _httpClient;
	private readonly HttpClient _localHttpClient;
	private bool _disposedValue;

	/// <summary>
	/// Initializes a new instance of the <see cref="DivoomClient"/> class.
	/// </summary>
	/// <param name="options">The client options.</param>
	/// <param name="logger">The logger.</param>
	public DivoomClient(DivoomClientOptions options, ILogger logger)
	{
		Options = options;

		_logger = logger;

		_refitSettings = new RefitSettings
		{
			ContentSerializer = new CustomJsonContentSerializer(Options, _logger)
		};

		_httpClientHandler = new DivoomHttpClientHandler(options
			?? throw new ArgumentNullException(nameof(options)), this, _logger);

		_httpClient = new HttpClient(_httpClientHandler)
		{
			BaseAddress = new Uri("https://app.divoom-gz.com"),
			Timeout = TimeSpan.FromSeconds(options.HttpClientTimeoutSeconds)
		};

		_localHttpClient = new HttpClient(_httpClientHandler)
		{
			BaseAddress = new Uri($"http://{options.DeviceIp}"),
			Timeout = TimeSpan.FromSeconds(options.HttpClientTimeoutSeconds)
		};

		Gz = RestService.For<IGz>(_httpClient, _refitSettings);

		Channel = RestService.For<IChannel>(_localHttpClient, _refitSettings);

		Bluetooth = new BluetoothManager(_logger);
	}

	/// <summary>
	/// The Gz API interface
	/// </summary>
	public IGz Gz { get; }

	/// <summary>
	/// The Channel API interface
	/// </summary>
	public IChannel Channel { get; }

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
				_localHttpClient.Dispose();
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
