// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Assertion", "NUnit2003:Consider using Assert.That(expr, Is.True) instead of Assert.IsTrue(expr)", Justification = "Following Frends guidelines", Scope = "namespaceanddescendants", Target = "~N:Frends.MySQL.ExecuteQuery.Tests")]
[assembly: SuppressMessage("Assertion", "NUnit2011:Use ContainsConstraint for better assertion messages in case of failure", Justification = "Following Frends guidelines", Scope = "namespaceanddescendants", Target = "~N:Frends.MySQL.ExecuteQuery.Tests")]
[assembly: SuppressMessage("Assertion", "NUnit2005:Consider using Assert.That(actual, Is.EqualTo(expected)) instead of Assert.AreEqual(expected, actual)", Justification = "Following Frends guidelines", Scope = "namespaceanddescendants", Target = "~N:Frends.MySQL.ExecuteQuery.Tests")]
