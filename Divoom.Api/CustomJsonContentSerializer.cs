using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api;

/// <summary>
/// A custom IHttpContentSerializer based on SystemTextJsonContentSerializer
/// to handle retries after missing members are observed
/// </summary>
public class CustomJsonContentSerializer : IHttpContentSerializer
{
	private readonly DivoomClientOptions _options;
	private readonly ILogger _logger;
	private readonly JsonSerializerOptions _jsonSerializerOptionsWithIgnore;
	private readonly JsonSerializerOptions _jsonSerializerOptionsWithError;
	private readonly SystemTextJsonContentSerializer _serializerIgnore;

	/// <summary>
	/// Initializes a new instance of the <see cref="CustomJsonContentSerializer"/> class.
	/// </summary>
	/// <param name="options">The client options.</param>
	/// <param name="logger">The logger.</param>
	public CustomJsonContentSerializer(DivoomClientOptions options, ILogger logger)
	{
		_options = options;
		_logger = logger;
		_jsonSerializerOptionsWithIgnore = new JsonSerializerOptions
		{
			// By default nulls should not be rendered out, this will allow the receiving API to apply any defaults.
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
			Converters = { new JsonStringEnumConverter() }
		};
		_jsonSerializerOptionsWithError = new JsonSerializerOptions
		{
			// By default nulls should not be rendered out, this will allow the receiving API to apply any defaults.
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
			Converters = { new JsonStringEnumConverter() }
		};

		_serializerIgnore = new SystemTextJsonContentSerializer(_jsonSerializerOptionsWithIgnore);
	}

	/// <inheritdoc />
	public async Task<T?> FromHttpContentAsync<T>(HttpContent content)
		=> await FromHttpContentAsync<T>(content, CancellationToken.None);

	/// <inheritdoc />
	public async Task<T?> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken)
		=> _options.JsonMissingMemberHandling switch
		{
			JsonMissingMemberHandling.Ignore => await _serializerIgnore.FromHttpContentAsync<T>(content, cancellationToken).ConfigureAwait(false),
			JsonMissingMemberHandling.ThrowOnError => await LogOnErrorAndThrowFromHttpContentAsync<T>(content, cancellationToken).ConfigureAwait(false),
			JsonMissingMemberHandling.LogWarningOnErrorAndContinue => await LogWarningOnErrorAndContinueFromHttpContentAsync<T>(content, cancellationToken).ConfigureAwait(false),
			_ => throw new NotSupportedException()
		};

	private async Task<T?> LogWarningOnErrorAndContinueFromHttpContentAsync<T>(
		HttpContent content,
		CancellationToken cancellationToken)
	{
		// This code has to read the content all at once into a stream
		// as we might re-use it in the second DeserializeObject call
		var sourceJson = await content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		try
		{
			return JsonSerializer.Deserialize<T>(sourceJson, _jsonSerializerOptionsWithError);
		}
		catch (JsonException ex)
		{
			_logger.LogWarning(ex, "{Message}", ex.Message);

			if (_options.JsonMissingMemberResponseLogLevel != LogLevel.None
				&& _logger.IsEnabled(_options.JsonMissingMemberResponseLogLevel))
			{
				_logger.Log(_options.JsonMissingMemberResponseLogLevel, "Missing Member Response JSON:\n{SourceJson}", sourceJson);
			}

			// Execute the action if one was provided
			_options.JsonMissingMemberAction?.Invoke(typeof(T), ex, sourceJson);

			return JsonSerializer.Deserialize<T>(sourceJson, _jsonSerializerOptionsWithIgnore);
		}
	}

	private async Task<T?> LogOnErrorAndThrowFromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken)
	{
		// This code has to read the content all at once into a stream
		// as we might re-use it in the second DeserializeObject call
		var sourceJson = await content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		try
		{
			return JsonSerializer.Deserialize<T>(sourceJson, _jsonSerializerOptionsWithError);
		}
		catch (JsonException ex)
		{
			if (_options.JsonMissingMemberResponseLogLevel != LogLevel.None
				&& _logger.IsEnabled(_options.JsonMissingMemberResponseLogLevel))
			{
				_logger.Log(_options.JsonMissingMemberResponseLogLevel, "Missing Member Response JSON:\n{SourceJson}", sourceJson);
			}

			// Execute the action if one was provided
			_options.JsonMissingMemberAction?.Invoke(typeof(T), ex, sourceJson);

			throw;
		}
	}

	/// <inheritdoc />
	public string? GetFieldNameForProperty(PropertyInfo propertyInfo)
		=> _serializerIgnore.GetFieldNameForProperty(propertyInfo);

	/// <inheritdoc />
	public HttpContent ToHttpContent<T>(T item)
		=> _serializerIgnore.ToHttpContent<T>(item);
}
