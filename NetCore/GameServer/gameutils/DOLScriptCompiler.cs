/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
#if NET
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DOL.GS
{
    public class DOLScriptCompiler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Compilation compiler;
        private Microsoft.CodeAnalysis.Emit.EmitResult lastEmitResult;
        private static List<PortableExecutableReference> referencedAssemblies;

        static DOLScriptCompiler()
        {
            referencedAssemblies = AppDomain.CurrentDomain
                                .GetAssemblies()
                                .Where(a => !a.IsDynamic)
                                .Select(a => a.Location)
                                .Where(s => !string.IsNullOrEmpty(s))
                                .Select(s => MetadataReference.CreateFromFile(s))
                                .ToList();
            var gameServerScriptAdditionalReferences = new string[] { 
                "System.Security.Cryptography.Algorithms", //for SHA256 in AutoXMLDatabaseUpdate
                "System.Security.Cryptography.Primitives", //for SHA256 in AutoXMLDatabaseUpdate
                "System.Net.Http"
            };
            var additionalReferences = GameServer.Instance.Configuration.AdditionalScriptAssemblies.ToList();
            additionalReferences.AddRange(gameServerScriptAdditionalReferences);

            foreach (var additionalReference in additionalReferences)
            {
                var dllName = additionalReference.EndsWith(".dll") ? additionalReference : additionalReference + ".dll";
                var probingPaths = new[] { ".", "lib", Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location) };
                var foundReference = false;
                foreach (var probingPath in probingPaths)
                {
                    var potentialReferenceFilePath = Path.Combine(probingPath, dllName);
                    if (File.Exists(potentialReferenceFilePath))
                    {
                        referencedAssemblies.Add(MetadataReference.CreateFromFile(potentialReferenceFilePath));
                        foundReference = true;
                        break;
                    }
                }
                if (foundReference == false) log.Error($"Reference not found: {additionalReference}");
            }
        }

        public bool HasErrors => !lastEmitResult.Success;

        public void SetToVisualBasicNet()
        {
            throw new NotSupportedException("Please migrate your scripts to C#.");
        }

        public Assembly Compile(FileInfo outputFile, IEnumerable<FileInfo> sourceFiles)
        {
            var syntaxTrees = sourceFiles.Where(file => file.Name != "AssemblyInfo.cs")
                .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file.FullName)));

            Compile(outputFile, syntaxTrees);

            if (HasErrors)
            {
                PrintErrorMessagesToConsole();
                throw new ApplicationException("Scripts compilation was unsuccessful. Abort startup!");
            }
            return Assembly.LoadFrom(outputFile.FullName);
        }

        public Assembly CompileFromText(GameClient client, string code)
        {
            var outputFile = new FileInfo("code_"+Guid.NewGuid()+".dll");
            var syntaxTrees = new List<SyntaxTree>() { CSharpSyntaxTree.ParseText(code) };

            Compile(outputFile, syntaxTrees);

            if (HasErrors)
            {
                PrintErrorMessagesTo(client);
                File.Delete(outputFile.FullName);
                return null;
            }
            var assembly = Assembly.Load(File.ReadAllBytes(outputFile.FullName));
            File.Delete(outputFile.FullName);
            return assembly;
        }

        private void Compile(FileInfo outputFile, IEnumerable<SyntaxTree> syntaxTrees)
        {
            var compilerParameters = new CSharpCompilationOptions(
                    outputKind: OutputKind.DynamicallyLinkedLibrary,
                    warningLevel: 2);
            compiler = CSharpCompilation.Create(
                outputFile.Name,
                options: compilerParameters,
                references: referencedAssemblies,
                syntaxTrees: syntaxTrees);
            var emitResult = compiler.Emit(outputFile.FullName);
            GC.Collect();

            lastEmitResult = emitResult;
        }

        private void PrintErrorMessagesToConsole()
        {
            foreach (var diag in ErrorDiagnostics)
            {
                log.Error($"\t{diag.Location} {diag.Id}: {diag.GetMessage()}");
            }
        }

        private void PrintErrorMessagesTo(GameClient client)
        {
            if (client.Player != null)
            {
                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Code.ErrorCompiling"), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                foreach (var diag in ErrorDiagnostics)
                    client.Out.SendMessage(diag.GetMessage(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
            }
            else
            {
                log.Debug("Error compiling code.");
            }
        }

        private IEnumerable<Diagnostic> ErrorDiagnostics
            => lastEmitResult.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
    }
}
#endif
