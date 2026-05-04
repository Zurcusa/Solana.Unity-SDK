// Tests~/TestHost/Assets/Tests/EditMode/UpmCleanImportTest.cs
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace SolanaMobileStack.Tests.EditMode
{
    public class UpmCleanImportTest
    {
        [Test]
        public void NoTestAssetLeakage()
        {
            // When a package is imported via UPM, Tests~ is stripped.
            // From *within* the test-host, Tests~/TestHost IS present — so this test asserts
            // the tilde-folder convention is intact on the package side by walking the package root.
            var packageRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ".."));

            // Sentinel precondition: package.json must exist at the computed root. If the test-host
            // is ever relocated, the `..` walk above silently misfires — fail fast with an
            // actionable message instead of producing a misleading pass/fail on the layout checks.
            Assert.That(File.Exists(Path.Combine(packageRoot, "package.json")), Is.True,
                $"packageRoot resolution is broken — expected package.json at {packageRoot}. " +
                $"Did the test-host move? Update the `..` walk in {nameof(NoTestAssetLeakage)}.");

            var leaked = Path.Combine(packageRoot, "Assets", "Tests~");
            Assert.IsFalse(Directory.Exists(leaked),
                $"Tests~ must not appear under Assets/ in a UPM-imported package; found {leaked}.");

            // Extra guard: the tilde-suffixed folder exists at the package root, confirming UPM-exclusion convention.
            var excluded = Path.Combine(packageRoot, "Tests~");
            Assert.IsTrue(Directory.Exists(excluded),
                "Tests~ folder must exist at the package root (UPM will strip it on import).");
        }
    }
}
