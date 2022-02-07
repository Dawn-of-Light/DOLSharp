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
#if NETFRAMEWORK
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using log4net;
using DOL.Language;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    public class DOLScriptCompiler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private CodeDomProvider compiler;
        private CompilerErrorCollection lastCompilationErrors;
        private bool HasErrors => lastCompilationErrors.HasErrors;
        private static List<string> referencedAssemblies = new List<string>();

        static DOLScriptCompiler()
        {
            var libDirectory = new DirectoryInfo(Path.Combine(GameServer.Instance.Configuration.RootDirectory, "lib"));
            referencedAssemblies.AddRange(libDirectory.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Select(f => f.Name));
            referencedAssemblies.Add("System.dll");
            referencedAssemblies.Add("System.Xml.dll");
            referencedAssemblies.Add("System.Core.dll");
            referencedAssemblies.Add("System.Net.Http.dll");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                referencedAssemblies.Add("netstandard.dll");
            }

            referencedAssemblies.Remove("testcentric.engine.metadata.dll"); //NUnit3TestAdapter 4.x dependency conflict with mscorlib.dll
            referencedAssemblies.AddRange(GameServer.Instance.Configuration.AdditionalScriptAssemblies);
        }

        public DOLScriptCompiler()
        {
            compiler = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
        }

        public void SetToVisualBasicNet()
        {
            compiler = new VBCodeProvider();
        }

        public Assembly Compile(FileInfo outputFile, IEnumerable<FileInfo> sourceFiles)
        {
            var sourceFilePaths = sourceFiles.Select(file => file.FullName).ToArray();
            var compilerParameters = new CompilerParameters(referencedAssemblies.ToArray())
            {
#if DEBUG
                IncludeDebugInformation = true,
#else
		        IncludeDebugInformation = false,
#endif
                GenerateExecutable = false,
                GenerateInMemory = false,
                WarningLevel = 2,
                CompilerOptions = string.Format($"/optimize /lib:{Path.Combine(".", "lib")}")
            };
            compilerParameters.ReferencedAssemblies.Remove(outputFile.Name);
            compilerParameters.OutputAssembly = outputFile.FullName;

            var compilerResults = compiler.CompileAssemblyFromFile(compilerParameters, sourceFilePaths);
            lastCompilationErrors = compilerResults.Errors;
            GC.Collect();
            if (HasErrors)
            {
                PrintErrorMessagesToConsole();
                throw new ApplicationException("Scripts compilation was unsuccessful. Abort startup!");
            }
            return compilerResults.CompiledAssembly;
        }

        public Assembly CompileFromText(GameClient client, string code)
        {
            var compilerParameters = new CompilerParameters(referencedAssemblies.ToArray())
            {
                GenerateInMemory = true,
                WarningLevel = 2,
                CompilerOptions = string.Format($"/lib:{Path.Combine(".", "lib")}")
            };
            compilerParameters.GenerateInMemory = true;

            var compilerResults = compiler.CompileAssemblyFromSource(compilerParameters, code);
            lastCompilationErrors = compilerResults.Errors;

            if (HasErrors)
            {
                PrintErrorMessagesTo(client);
                return null;
            }
            return compilerResults.CompiledAssembly;
        }

        private void PrintErrorMessagesToConsole()
        {
            foreach (CompilerError error in lastCompilationErrors)
            {
                if (error.IsWarning) continue;

                var errorMessage = $"   {error.FileName} Line:{error.Line} Col:{error.Column}";
                if (log.IsErrorEnabled)
                {
                    errorMessage = $"Script compilation failed because: \n{error.ErrorText}\n" + errorMessage;
                }
                log.Error(errorMessage);
            }
        }

        private void PrintErrorMessagesTo(GameClient client)
        {
            if (client.Player != null)
            {
                client.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "AdminCommands.Code.ErrorCompiling"), eChatType.CT_System, eChatLoc.CL_PopupWindow);

                foreach (CompilerError error in lastCompilationErrors)
                    client.Out.SendMessage(error.ErrorText, eChatType.CT_System, eChatLoc.CL_PopupWindow);
            }
            else
            {
                log.Debug("Error compiling code.");
            }
        }
    }
}
#endif
