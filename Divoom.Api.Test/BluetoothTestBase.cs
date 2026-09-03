using AwesomeAssertions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging;

namespace Divoom.Api.Test;

/// <summary>
/// The shared plumbing for the Bluetooth test classes: the client drawn from the
/// collection fixture, a logger routed to the test output, the settling delays each
/// test needs around it, and device discovery.
/// </summary>
/// <remarks>
/// Every derived class carries <c>[Collection("Bluetooth")]</c> so that they all share
/// one <see cref="BluetoothFixture"/>, and therefore one connection to the device.
/// </remarks>
public abstract class BluetoothTestBase : IAsyncLifetime
{
	private readonly BluetoothFixture _fixture;

	protected BluetoothTestBase(ITestOutputHelper testOutputHelper, BluetoothFixture fixture)
	{
		_fixture = fixture;
		Logger = LoggerFactory
			.Create(builder => builder.AddProvider(new XunitLoggerProvider(testOutputHelper)))
			.CreateLogger(GetType().Name);
	}

	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	protected DivoomClient Client => _fixture.Client;

	protected ILogger Logger { get; }

	public async ValueTask InitializeAsync() =>
		// Small delay before each test to allow device to settle
		await Task.Delay(500);

	public async ValueTask DisposeAsync()
	{
		// Small delay after each test to allow device to settle
		await Task.Delay(500);
		GC.SuppressFinalize(this);
	}

	protected async Task<DivoomBluetoothDevice> GetFirstDeviceAsync(CancellationToken cancellationToken)
	{
		var devices = await Client
			.Bluetooth
			.GetDevicesAsync(cancellationToken);

		devices.Should().NotBeNull("Bluetooth discovery should not return null");

		if (devices.Count == 0)
		{
			// Provide helpful diagnostic information
			Logger.LogError("No Divoom devices found during Bluetooth discovery.");
			Logger.LogError("Troubleshooting steps:");
			Logger.LogError("  1. Ensure your Divoom device (TimeBox/PIXOO) is powered ON");
			Logger.LogError("  2. Pair the device in Windows Settings → Bluetooth & devices");
			Logger.LogError("  3. Ensure Bluetooth is enabled on your PC");
			Logger.LogError("  4. Move the device closer (within 10 meters)");
			Logger.LogError("  5. Run the DiagnoseBluetooth_ListsAllDevices test to see all discovered devices");
			Logger.LogError("");
			Logger.LogError("To run diagnostics: Remove [Skip] attribute from DiagnoseBluetooth_ListsAllDevices test");
		}

		devices.Should().BeOfType<List<DivoomBluetoothDevice>>();
		devices.Should().HaveCountGreaterThan(0, "at least one Divoom/TimeBox/PIXOO device should be discovered. See log output above for troubleshooting steps.");

		var device = devices.FirstOrDefault(d => d.DeviceInfo.Connected) ?? throw new InvalidOperationException("No connected devices found");

		if (Logger.IsEnabled(LogLevel.Information))
		{
			Logger.LogInformation("Using device: {Device}", device);
		}

		return device;
	}
}
