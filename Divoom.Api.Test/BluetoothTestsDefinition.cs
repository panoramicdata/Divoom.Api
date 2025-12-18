namespace Divoom.Api.Test;

/// <summary>
/// Collection definition for Bluetooth tests.
/// All test classes with [Collection("Bluetooth")] will share the same BluetoothFixture instance.
/// </summary>
[CollectionDefinition("Bluetooth", DisableParallelization = true)]
public class BluetoothTestsDefinition : ICollectionFixture<BluetoothFixture>;
