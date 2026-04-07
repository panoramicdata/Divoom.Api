namespace Divoom.Api;

public enum JsonMissingMemberHandling
{
	/// <summary>
	/// Ignore unmapped members during deserialization
	/// </summary>
	Ignore = 1,

	/// <summary>
	/// Throw an error when unmapped members are encountered during deserialization
	/// </summary>
	ThrowOnError = 2,

	/// <summary>
	/// Log the issue when a missing member error occurs and then reattempt using the Ignore behavior
	/// </summary>
	LogWarningOnErrorAndContinue = 3
}
