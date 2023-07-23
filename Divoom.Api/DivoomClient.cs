using Divoom.Api.Implementations;
using Divoom.Api.Interfaces;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Net.Http;

namespace Divoom.Api;

public class DivoomClient : IDisposable
{
	public DivoomClientOptions Options { get; }

	private readonly ILogger _logger;
	private readonly RefitSettings _refitSettings;
	private readonly DivoomHttpClientHandler _httpClientHandler;
	private readonly HttpClient _httpClient;
	private readonly HttpClient _localHttpClient;
	private bool _disposedValue;

	public DivoomClient(DivoomClientOptions options, ILogger logger)
	{
		Options = options;

		_logger = logger;

		_refitSettings = new RefitSettings
		{
			ContentSerializer = new CustomNewtonsoftJsonContentSerializer(Options, _logger)
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

		Bluetooth = new BluetoothManager();
	}

	public IGz Gz { get; }

	public IChannel Channel { get; }

	public IBluetooth Bluetooth { get; }

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

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
