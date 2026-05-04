// Tests~/TestHost/Assets/Tests/EditMode/TestHostBootstrapTest.cs
using System.Linq;
using NUnit.Framework;
using UnityEditor.Compilation;

namespace SolanaMobileStack.Tests.EditMode
{
    public class TestHostBootstrapTest
    {
        [Test]
        public void AdapterAssemblyLoads()
        {
            var assemblies = CompilationPipeline
                .GetAssemblies(AssembliesType.Editor)
                .Select(a => a.name)
                .ToArray();

            Assert.That(assemblies, Has.Some.EqualTo("com.solana.unity_sdk"),
                "Runtime asmdef 'com.solana.unity_sdk' must be visible to the test-host; " +
                "otherwise tests cannot reference SDK types.");
        }
    }
}
