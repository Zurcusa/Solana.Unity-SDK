// Runtime/codebase/SolanaMobileStack/Internal/IsExternalInit.cs
#if !NET5_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Polyfill enabling C# 9 <c>init</c>-only setters on .NET Standard 2.0 (Unity 2022.3).
    /// Declared <c>internal</c> so consumer assemblies are unaffected and Unity can ship its own
    /// without a type-clash when the toolchain advances.
    /// </summary>
    internal static class IsExternalInit { }
}
#endif
