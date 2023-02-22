using System.IO;

using NUnit.Framework;

using DOL.GS;
using DOL.UnitTests.Gameserver;

namespace DOL.Integration.Gameserver
{
    [TestFixture, Explicit]
    public class Test_DOLScriptCompiler
    {
        [OneTimeSetUp]
        public void Init()
        {
            var server = new FakeServer();
            GameServer.LoadTestDouble(server);
        }

        [Test]
        public void CompileFromText_FooBarReturnsHello_IndirectInvokationReturnsHello()
        {
            var compiler = new DOLScriptCompiler();
            var codeText = "public class Foo{\npublic static string Bar => \"Hello\";}";

            var assembly = compiler.CompileFromText(new GameClient(null), codeText);

            var actual = assembly.GetType("Foo").GetProperty("Bar").GetGetMethod().Invoke(null, new object[] { });
            Assert.That(actual, Is.EqualTo("Hello"));
        }

        [Test]
        public void CompileFromText_Twice_SecondAssemblyIsNotNull()
        {
            var compiler = new DOLScriptCompiler();
            var codeText = "public class Foo{\npublic static void foo(){\nvar bar = 0; System.Console.WriteLine(bar);}}";
            var compiler2 = new DOLScriptCompiler();
            var codeText2 = "public class Foo2{\npublic void foo(){\nSystem.Console.WriteLine();}}";

            _ = compiler.CompileFromText(new GameClient(null), codeText);
            var actual = compiler2.CompileFromText(new GameClient(null), codeText2);

            Assert.That(actual, Is.Not.Null);
        }

        [Test]
        public void CompileFromText_usingDolGs_WithGameServerDllAsReference_AssemblyIsNotNull()
        {
            var compiler = new DOLScriptCompiler();
            var codeText = "using DOL.GS;";

            var actual = compiler.CompileFromText(new GameClient(null), codeText);

            Assert.That(actual, Is.Not.Null);
        }

        [Test]
        public void Compile_FileContainingFooBarHello_IndirectInvokationReturnsHello()
        {
            var compiler = new DOLScriptCompiler();
            var codeText = "class Foo{public static string Bar => \"Hello\";}";
            var sourceFile = new FileInfo("test.cs");
            File.WriteAllText(sourceFile.FullName, codeText);

            File.Delete("test.dll");
            var assembly = compiler.Compile(new FileInfo("test.dll"), new[] { sourceFile });

            var actual = assembly.GetType("Foo").GetProperty("Bar").GetGetMethod().Invoke(null, new object[] { });
            Assert.That(actual, Is.Not.Null);
        }
    }
}