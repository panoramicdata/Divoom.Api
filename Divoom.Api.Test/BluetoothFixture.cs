using Microsoft.Extensions.Logging.Abstractions;

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
		Options = TestConfiguration.LoadDivoomClientOptions();
		Client = new DivoomClient(Options, NullLogger.Instance);
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
