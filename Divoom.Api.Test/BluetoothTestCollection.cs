namespace Divoom.Api.Test;

/// <summary>
/// Collection definition to ensure Bluetooth tests run sequentially.
/// Bluetooth connections cannot be shared across parallel tests.
/// </summary>
[CollectionDefinition("Bluetooth Sequential Tests", DisableParallelization = true)]
public class BluetoothTestCollection
{
	// This class has no code, and is never created. Its purpose is simply
	// to be the place to apply [CollectionDefinition] and all the
	// ICollectionFixture<> interfaces.
}
