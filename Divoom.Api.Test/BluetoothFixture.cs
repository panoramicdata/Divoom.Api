using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Divoom.Api.Test;

/// <summary>
/// Shared fixture for all Bluetooth tests that provides a single DivoomClient instance.
/// This ensures all tests in the collection use the same Bluetooth connection.
/// </summary>
public class BluetoothFixture : IDisposable
{
	private bool _disposedValue;

	public DivoomClient Client { get; }

	public DivoomClientOptions Options { get; }

	public BluetoothFixture()
	{
		Options = LoadDivoomClientOptions();
		Client = new DivoomClient(Options, NullLogger.Instance);
	}

	private static DivoomClientOptions LoadDivoomClientOptions()
	{
		var fileInfo = new FileInfo("../../../appsettings.json");

		if (!fileInfo.Exists)
		{
			throw new InvalidOperationException(
				"Missing appsettings.json. Please copy the appsettings.example.json in the project root folder and set the various values appropriately.");
		}

		var options = JsonConvert.DeserializeObject<DivoomClientOptions>(File.ReadAllText(fileInfo.FullName));

		return options ?? throw new InvalidOperationException("Configuration did not deserialize");
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Client.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
