using Divoom.Api.Interfaces;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Net.Http;

namespace Divoom.Api;

public class DivoomClient
{
	public DivoomClientOptions Options { get; }

	private readonly ILogger _logger;
	private readonly RefitSettings _refitSettings;
	private readonly DivoomHttpClientHandler _httpClientHandler;
	private readonly HttpClient _httpClient;

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

		// Create Gz against the refit interface IGz
		Gz = RestService.For<IGz>(_httpClient, _refitSettings);
	}

	public IGz Gz { get; }
}
