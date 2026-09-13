using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

/// <summary>
/// Sends images and animations to a device for display.
/// </summary>
public interface IBluetoothImages
{
	/// <summary>
	/// Views a static image.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="image">The image to display.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewImageAsync(
		DivoomBluetoothDevice device,
		DivoomImage image,
		CancellationToken cancellationToken);

	/// <summary>
	/// Views an animation.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="divoomAnimation">The animation.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewAnimationAsync(
		DivoomBluetoothDevice device,
		DivoomAnimation divoomAnimation,
		CancellationToken cancellationToken);
}
