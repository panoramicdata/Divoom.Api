using Divoom.Api.Implementations;
using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api;

/// <summary>
/// Decides which requests are reads, so that <see cref="DivoomClientOptions.IsReadOnly"/> can refuse the rest.
/// </summary>
internal static class ReadOnlyGuard
{
	private const string LocalCommandPath = "/post";

	// POST endpoints on the Divoom cloud that only list data.
	private static readonly HashSet<string> ReadOnlyCloudPostPaths = new(StringComparer.OrdinalIgnoreCase)
	{
		"/Device/GetImgUploadList",
		"/Device/GetImgLikeList"
	};

	private static readonly HashSet<byte> ReadOnlyBluetoothCommands =
	[
		(byte)Command.GetVolume,
		(byte)Command.GetMuteState,
		(byte)Command.GetSettings,
		(byte)Command.GetWeather,
		(byte)Command.GetRadioFrequency
	];

	public static async Task EnsureHttpRequestIsReadAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		if (request.Method == HttpMethod.Get
			|| request.Method == HttpMethod.Head
			|| request.Method == HttpMethod.Options)
		{
			return;
		}

		if (request.Method == HttpMethod.Post && await IsReadOnlyPostAsync(request, cancellationToken).ConfigureAwait(false))
		{
			return;
		}

		throw new InvalidOperationException(
			$"The client is read-only, so {request.Method} {request.RequestUri?.AbsolutePath} was not sent.");
	}

	public static void EnsureBluetoothCommandIsRead(DivoomClientOptions options, CommandBuilder commandBuilder)
	{
		if (options.IsReadOnly
			&& (commandBuilder.CommandByte is not { } commandByte || !ReadOnlyBluetoothCommands.Contains(commandByte)))
		{
			throw new InvalidOperationException(
				"The client is read-only, so the Bluetooth command was not sent.");
		}
	}

	private static async Task<bool> IsReadOnlyPostAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var path = request.RequestUri?.AbsolutePath ?? string.Empty;
		if (ReadOnlyCloudPostPaths.Contains(path))
		{
			return true;
		}

		if (!string.Equals(path, LocalCommandPath, StringComparison.OrdinalIgnoreCase) || request.Content is null)
		{
			return false;
		}

		// Local device commands are all POST /post; the verb is the last segment of the Command, e.g. "Channel/GetClockInfo".
		var body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			using var document = JsonDocument.Parse(body);
			if (document.RootElement.TryGetProperty("Command", out var command)
				&& command.ValueKind == JsonValueKind.String)
			{
				var name = command.GetString() ?? string.Empty;
				var verb = name[(name.LastIndexOf('/') + 1)..];
				return verb.StartsWith("Get", StringComparison.Ordinal);
			}
		}
		catch (JsonException)
		{
			return false;
		}

		return false;
	}
}
