// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to
// this project. Project-level suppressions either have no target or are given a specific target and
// scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:Opening parenthesis should be spaced correctly", Justification = "Causes false positive for String.Substring's range operator. CodeMaid ensures that this rule is not violated.", Scope = "member", Target = "~M:Toimik.UrlNormalization.HttpUrlNormalizer.NormalizeQuery(System.String)~System.String")]