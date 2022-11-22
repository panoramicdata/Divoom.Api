using Divoom.Api.Models;
using Refit;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

/// <summary>
/// All calls that are returned from https://app.divoom-gz.com/ are served by this interface.
/// </summary>
public interface IGz
{
	/// <summary>
	/// Gets a list of supported dial types.
	/// </summary>
	[Get("/Channel/GetDialType")]
	Task<GetDialTypesResponse> GetDialTypesAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a list of devices on the current LAN.
	/// It's not clear how this is achieved.
	/// Perhaps the device is scanning the local network and reporting them back?
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[Get("/Device/ReturnSameLANDevice")]
	Task<GetLanDevicesResponse> GetLanDevicesAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a list of devices on the current LAN.
	/// It's not clear how this is achieved.
	/// Perhaps the device is scanning the local network and reporting them back?
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[Get("/Device/GetTimeDialFontList")]
	Task<GetDialFontsResponse> GetDialFontsAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a list of dials for a particular channel
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[Get("/Channel/GetDialList")]
	Task<GetChannelDialsResponse> GetChannelDialsAsync(
		[Body] GetChannelDialsRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a list of dials for a particular channel
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[Post("/Device/GetImgUploadList")]
	Task<GetImagesResponse> GetImagesAsync(
		[Body] GetImagesRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a list of dials for a particular channel
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	[Post("/Device/GetImgLikeList")]
	Task<GetImagesResponse> GetLikedImagesAsync(
		[Body] GetImagesRequest request,
		CancellationToken cancellationToken);
}
