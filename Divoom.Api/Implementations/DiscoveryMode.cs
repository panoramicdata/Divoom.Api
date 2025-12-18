namespace Divoom.Api.Implementations
{
	/// <summary>
	/// Bluetooth device discovery modes
	/// </summary>
	public enum DiscoveryMode
	{
		/// <summary>
		/// Only paired devices will be discovered
		/// </summary>
		PairedOnly,

		/// <summary>
		/// Only newly discovered devices will be discovered
		/// </summary>
		DiscoveredOnly,

		/// <summary>
		/// All devices will be discovered
		/// </summary>
		All,
	}
}