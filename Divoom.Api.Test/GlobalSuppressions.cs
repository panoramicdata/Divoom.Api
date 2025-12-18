// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
	"Naming",
	"CA1707:Identifiers should not contain underscores",
	Justification = "Appropriate for Unit Tests"
)]
[assembly: SuppressMessage(
	"Performance",
	"CA1848:Use the LoggerMessage delegates",
	Justification = "Implementation effort not worth the performance gain"
)]
