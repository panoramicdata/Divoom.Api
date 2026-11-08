using Divoom.Api.Models;
using Refit;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

/// <summary>
/// Commands sent to a local device's HTTP API (POST http://{DeviceIp}/post).
/// </summary>
public interface IChannel
{
	/// <summary>
	/// Sets the dial (clock face).
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> SetDialAsync(
		[Body] SetDialRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the current dial and brightness.
	/// </summary>
	[Post("/post")]
	Task<GetDialInfoResponse> GetDialInfoAsync(
		[Body] GetDialInfoRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Sets the channel.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> SetAsync(
		[Body] SetChannelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the device's settings.
	/// </summary>
	[Post("/post")]
	Task<GetAllConfigResponse> GetAllConfigAsync(
		[Body] GetAllConfigRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the device's clock.
	/// </summary>
	[Post("/post")]
	Task<GetDeviceTimeResponse> GetDeviceTimeAsync(
		[Body] GetDeviceTimeRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Sets the brightness.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> SetBrightnessAsync(
		[Body] SetBrightnessRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Switches the screen on or off.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> SetScreenPowerAsync(
		[Body] SetScreenPowerRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the id of the last animation sent over HTTP.
	/// </summary>
	[Post("/post")]
	Task<GetHttpGifIdResponse> GetHttpGifIdAsync(
		[Body] GetHttpGifIdRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Resets the HTTP animation id.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> ResetHttpGifIdAsync(
		[Body] ResetHttpGifIdRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Sends one frame of an animation; the device shows it once all frames have arrived.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> SendHttpGifAsync(
		[Body] SendHttpGifRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Draws text over the animation last sent with <see cref="SendHttpGifAsync"/>.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> SendHttpTextAsync(
		[Body] SendHttpTextRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Removes all text sent over HTTP.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> ClearHttpTextAsync(
		[Body] ClearHttpTextRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Sounds the buzzer.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> PlayBuzzerAsync(
		[Body] PlayBuzzerRequest request,
		CancellationToken cancellationToken);
}
