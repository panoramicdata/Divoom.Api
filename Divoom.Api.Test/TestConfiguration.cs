using Microsoft.Extensions.Configuration;

namespace Divoom.Api.Test;

/// <summary>
/// Loads the <see cref="DivoomClientOptions"/> that the integration tests run against.
/// </summary>
/// <remarks>
/// The device details identify a developer's own hardware, so they live in user secrets
/// and never reach source control. An <c>appsettings.json</c> beside the project is still
/// honoured for anyone who prefers a file, but user secrets win where both set a value.
/// </remarks>
internal static class TestConfiguration
{
	/// <summary>
	/// Builds the client options from user secrets, falling back to an optional
	/// <c>appsettings.json</c> in the project root.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Thrown when no device has been configured, with the commands needed to set one.
	/// </exception>
	internal static DivoomClientOptions LoadDivoomClientOptions()
	{
		var configuration = new ConfigurationBuilder()
			.AddJsonFile("../../../appsettings.json", optional: true)
			.AddUserSecrets(typeof(TestConfiguration).Assembly, optional: true)
			.Build();

		var deviceIp = configuration["DeviceIp"];
		var deviceMac = configuration["DeviceMac"];

		if (string.IsNullOrWhiteSpace(deviceIp) || string.IsNullOrWhiteSpace(deviceMac))
		{
			throw new InvalidOperationException(
				"No Divoom device is configured. Set your device's details in user secrets:\n" +
				"  dotnet user-secrets set \"DeviceId\" \"300012345\" --project Divoom.Api.Test\n" +
				"  dotnet user-secrets set \"DeviceIp\" \"192.168.1.123\" --project Divoom.Api.Test\n" +
				"  dotnet user-secrets set \"DeviceMac\" \"01234567890A\" --project Divoom.Api.Test\n" +
				"See Divoom.Api.Test/UserSecretsGuide.md for how to find those values.");
		}

		return new DivoomClientOptions
		{
			DeviceId = int.TryParse(configuration["DeviceId"], out var deviceId) ? deviceId : 0,
			DeviceIp = deviceIp,
			DeviceMac = deviceMac
		};
	}
}
