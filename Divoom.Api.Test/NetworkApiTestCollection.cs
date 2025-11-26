namespace Divoom.Api.Test;

/// <summary>
/// Collection definition for network-based API tests (GZ API, Channel API).
/// These tests use HTTP and can run in parallel with each other but not with Bluetooth tests.
/// </summary>
[CollectionDefinition("Network API Tests", DisableParallelization = false)]
public class NetworkApiTestCollection
{
	// This class has no code, and is never created. Its purpose is simply
	// to be the place to apply [CollectionDefinition].
}
