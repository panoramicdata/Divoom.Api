using Divoom.Api.Exceptions;
using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api;

/// <summary>
/// Higher-level operations built from <see cref="IChannel"/> commands
/// </summary>
public static class ChannelExtensions
{
	/// <summary>
	/// Fills the screen with one colour and draws text over it, e.g. a status such as "BUSY".
	/// </summary>
	/// <param name="channel">The channel.</param>
	/// <param name="text">The text; longer than the box, it scrolls.</param>
	/// <param name="backgroundColor">The background, as #RRGGBB.</param>
	/// <param name="textColor">The text colour, as #RRGGBB.</param>
	/// <param name="width">The panel width in pixels: 16, 32 or 64.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	public static async Task ShowTextAsync(
		this IChannel channel,
		string text,
		string backgroundColor,
		string textColor,
		int width,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(channel);
		SendHttpGifRequest.ParseHexColor(backgroundColor);
		SendHttpGifRequest.ParseHexColor(textColor);

		// The device ignores a frame whose id is not higher than the last one it drew.
		var gifId = await channel.GetHttpGifIdAsync(new(), cancellationToken).ConfigureAwait(false);
		EnsureSuccess(await channel
			.SendHttpGifAsync(SendHttpGifRequest.SolidColor(gifId.PicId + 1, backgroundColor, width), cancellationToken)
			.ConfigureAwait(false));

		EnsureSuccess(await channel
			.SendHttpTextAsync(new SendHttpTextRequest
			{
				Text = text,
				Color = textColor,
				Y = (width / 2) - 8,
				TextWidth = width
			}, cancellationToken)
			.ConfigureAwait(false));
	}

	/// <summary>
	/// Throws when the device reports a non-zero error code.
	/// </summary>
	/// <param name="response">The response.</param>
	public static void EnsureSuccess(PixooResponse response)
	{
		ArgumentNullException.ThrowIfNull(response);
		if (response.ReturnCode?.ToString() is not "0")
		{
			throw new DivoomBadRequestException($"The device returned error_code {response.ReturnCode}.");
		}
	}
}
