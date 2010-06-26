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
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DOL.AI.Brain;
using DOL.Config;
using DOL.GS.PacketHandler;
using DOL.GS.ServerRules;
using DOL.GS.Spells;
using DOL.GS.Commands;
using log4net;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace DOL.GS
{
	public class ScriptMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ArrayList m_scripts = new ArrayList(4);
		private static readonly Hashtable m_spellhandlerConstructorCache = new Hashtable();

		/// <summary>
		/// This class will hold all info about a gamecommand
		/// </summary>
		public class GameCommand
		{
            public String[] Usage { get; set; }
			public string m_cmd;
			public uint m_lvl;
			public string m_desc;
			public ICommandHandler m_cmdHandler;
		}

		/// <summary>
		/// All commands will be stored in this hashtable
		/// </summary>
		private static Hashtable m_cmds =
			//VaNaTiC->
			//new Hashtable(CaseInsensitiveHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);
			new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		//VaNaTiC<-


		/// <summary>
		/// Gets the scripts assemblies
		/// </summary>
		public static Assembly[] Scripts
		{
			get { return (Assembly[])m_scripts.ToArray(typeof(Assembly)); }
		}

		/// <summary>
		/// Gets the requested command if it exists
		/// </summary>
		/// <param name="cmd">The command to retrieve</param>
		/// <returns>Returns the command if it exists, otherwise the return value is null</returns>
		public static GameCommand GetCommand(string cmd)
		{
			return m_cmds[cmd] as GameCommand;
		}

		/// <summary>
		/// Looking for exact match first, then, if nothing found, trying to guess command using first letters
		/// </summary>
		/// <param name="cmd">The command to retrieve</param>
		/// <returns>Returns the command if it exists, otherwise the return value is null</returns>
		public static GameCommand GuessCommand(string cmd)
		{
			GameCommand myCommand = GetCommand(cmd);
			if (myCommand != null) return myCommand;

			// Trying to guess the command
			string compareCmdStr = cmd.ToLower();
			IDictionaryEnumerator iter = m_cmds.GetEnumerator();

			while (iter.MoveNext())
			{
				GameCommand currentCommand = iter.Value as GameCommand;
				string currentCommandStr = iter.Key as string;

				if (currentCommand == null) continue;

				if (currentCommandStr.ToLower().StartsWith(compareCmdStr))
				{
					myCommand = currentCommand;
					break;
				}
			}
			return myCommand;
		}

		/// <summary>
		/// Returns an array of all the available commands with the specified plvl and their descriptions
		/// </summary>
		/// <param name="plvl">plvl of the commands to get</param>
		/// <param name="addDesc"></param>
		/// <returns></returns>
		public static string[] GetCommandList(ePrivLevel plvl, bool addDesc)
		{
			IDictionaryEnumerator iter = m_cmds.GetEnumerator();

			ArrayList list = new ArrayList();

			while (iter.MoveNext())
			{
				GameCommand cmd = iter.Value as GameCommand;
				string cmdString = iter.Key as string;

				if (cmd == null || cmdString == null)
				{
					continue;
				}

				if (cmdString[0] == '&')
					cmdString = '/' + cmdString.Remove(0, 1);
				if ((uint)plvl >= cmd.m_lvl)
				{
					if (addDesc)
					{
						list.Add(cmdString + " - " + cmd.m_desc);
					}
					else
					{
						list.Add(cmd.m_cmd);
					}
				}
			}

			return (string[])list.ToArray(typeof(string));
		}

		/// <summary>
		/// Parses a directory for all source files
		/// </summary>
		/// <param name="path">The root directory to start the search in</param>
		/// <param name="filter">A filter representing the types of files to search for</param>
		/// <param name="deep">True if subdirectories should be included</param>
		/// <returns>An ArrayList containing FileInfo's for all files in the path</returns>
		private static ArrayList ParseDirectory(DirectoryInfo path, string filter, bool deep)
		{
			ArrayList files = new ArrayList();

			if (!path.Exists)
				return files;

			files.AddRange(path.GetFiles(filter));

			if (deep)
			{
				foreach (DirectoryInfo subdir in path.GetDirectories())
					files.AddRange(ParseDirectory(subdir, filter, deep));
			}

			return files;
		}

		/// <summary>
		/// Searches the script assembly for all command handlers
		/// </summary>
		/// <returns>True if succeeded</returns>
		private static bool LoadCommands()
		{
			m_cmds.Clear();

			ArrayList asms = new ArrayList(Scripts);
			asms.Add(typeof(GameServer).Assembly);

			//build array of disabled commands
			string[] disabledarray = ServerProperties.Properties.DISABLED_COMMANDS.Split(';');

			foreach (Assembly script in asms)
			{
				if (log.IsDebugEnabled)
					log.Debug("ScriptMgr: Searching for commands in " + script.GetName());
				// Walk through each type in the assembly
				foreach (Type type in script.GetTypes())
				{
					// Pick up a class
					if (type.IsClass != true) continue;
					if (type.GetInterface("DOL.GS.Commands.ICommandHandler") == null) continue;

					try
					{
						object[] objs = type.GetCustomAttributes(typeof(CmdAttribute), false);
						foreach (CmdAttribute attrib in objs)
						{
							bool disabled = false;
							foreach (string str in disabledarray)
							{
								if (attrib.Cmd.Replace('&', '/') == str)
								{
									disabled = true;
									log.Info("Will not load command " + attrib.Cmd + " as it is disabled in server properties");
									break;
								}
							}
							if (disabled)
								continue;
							if (m_cmds.ContainsKey(attrib.Cmd))
							{
								log.Info(attrib.Cmd + " from " + script.GetName() + " has been suppressed, a command of that type already exists!");
								continue;
							}
							if (log.IsDebugEnabled)
								log.Debug("ScriptMgr: Command - '" + attrib.Cmd + "' - (" + attrib.Description + ") required plvl:" + attrib.Level);
							GameCommand cmd = new GameCommand();

							cmd.Usage = attrib.Usage;
							cmd.m_cmd = attrib.Cmd;
							cmd.m_lvl = attrib.Level;
							cmd.m_desc = attrib.Description;
							cmd.m_cmdHandler = (ICommandHandler)Activator.CreateInstance(type);
							m_cmds.Add(attrib.Cmd, cmd);
							if (attrib.Aliases != null)
								foreach (string alias in attrib.Aliases)
								{
									m_cmds.Add(alias, cmd);
								}
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("LoadCommands", e);
					}
				}
			}
			log.Info("Loaded " + m_cmds.Count + " commands!");
			return true;
		}

		/// <summary>
		/// Called when a command needs to be handled
		/// </summary>
		/// <param name="client">Client executing the command</param>
		/// <param name="cmdLine">Args for the command</param>
		/// <returns>True if succeeded</returns>
		public static bool HandleCommand(GameClient client, string cmdLine)
		{
			try
			{
				// parse args
				string[] pars = ParseCmdLine(cmdLine);
				GameCommand myCommand = GuessCommand(pars[0]);

				//If there is no such command, return false
				if (myCommand == null) return false;

				if (client.Account.PrivLevel < myCommand.m_lvl)
				{
					if (!SinglePermission.HasPermission(client.Player, pars[0].Substring(1, pars[0].Length - 1)))
					{
						if (pars[0][0] == '&')
							pars[0] = '/' + pars[0].Remove(0, 1);
						//client.Out.SendMessage("You do not have enough priveleges to use " + pars[0], eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						//why should a player know the existing commands..
						client.Out.SendMessage("No such command ("+pars[0]+")",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return true;
					}
					//else execute the command
				}

				ExecuteCommand(client, myCommand, pars);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("HandleCommand", e);
			}
			return true;
		}

		/// <summary>
		/// Called when a command needs to be handled without plvl being taken into consideration
		/// </summary>
		/// <param name="client">Client executing the command</param>
		/// <param name="cmdLine">Args for the command</param>
		/// <returns>True if succeeded</returns>
		public static bool HandleCommandNoPlvl(GameClient client, string cmdLine)
		{
			try
			{
				string[] pars = ParseCmdLine(cmdLine);
				GameCommand myCommand = GuessCommand(pars[0]);

				//If there is no such command, return false
				if (myCommand == null) return false;

				ExecuteCommand(client, myCommand, pars);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("HandleCommandNoPlvl", e);
			}
			return true;
		}

		/// <summary>
		/// Splits string to substrings
		/// </summary>
		/// <param name="cmdLine">string that should be split</param>
		/// <returns>Array of substrings</returns>
		private static string[] ParseCmdLine(string cmdLine)
		{
			if (cmdLine == null)
			{
				throw new ArgumentNullException("cmdLine");
			}

			List<string> args = new List<string>();
			int state = 0;
			StringBuilder arg = new StringBuilder(cmdLine.Length >> 1);

			for (int i = 0; i < cmdLine.Length; i++)
			{
				char c = cmdLine[i];
				switch (state)
				{
					case 0: // waiting for first arg char
						if (c == ' ') continue;
						arg.Length = 0;
						if (c == '"') state = 2;
						else
						{
							state = 1;
							i--;
						}
						break;
					case 1: // reading arg
						if (c == ' ')
						{
							args.Add(arg.ToString());
							state = 0;
						}
						arg.Append(c);
						break;
					case 2: // reading string
						if (c == '"')
						{
							args.Add(arg.ToString());
							state = 0;
						}
						arg.Append(c);
						break;
				}
			}
			if (state != 0) args.Add(arg.ToString());

			string[] pars = new string[args.Count];
			args.CopyTo(pars);

			return pars;
		}

		/// <summary>
		/// Checks for 'help' param and executes command
		/// </summary>
		/// <param name="client">Client executing the command</param>
		/// <param name="myCommand">command to be executed</param>
		/// <param name="pars">Args for the command</param>
		/// <returns>Command result</returns>
		private static void ExecuteCommand(GameClient client, GameCommand myCommand, string[] pars)
		{
			// what you type in script is what you get; needed for overloaded scripts,
			// like emotes, to handle case insensitive and guessed commands correctly
			pars[0] = myCommand.m_cmd;

			//Log the command usage
			if (client.Account == null || ((ServerProperties.Properties.LOG_ALL_GM_COMMANDS && client.Account.PrivLevel > 1) || myCommand.m_lvl > 1))
			{
				string commandText = String.Join(" ", pars);
				string targetName = "(no target)";
				string playerName = (client.Player == null) ? "(player is null)" : client.Player.Name;
				string accountName = (client.Account == null) ? "account is null" : client.Account.Name;

				if (client.Player == null)
				{
					targetName = "(player is null)";
				}
				else if (client.Player.TargetObject != null)
				{
					targetName = client.Player.TargetObject.Name;
					if (client.Player.TargetObject is GamePlayer)
						targetName += "(" + ((GamePlayer)client.Player.TargetObject).Client.Account.Name + ")";
				}
				GameServer.Instance.LogGMAction("Command: " + playerName + "(" + accountName + ") -> " + targetName + " - \"/" + commandText.Remove(0, 1) + "\"");

			}

			myCommand.m_cmdHandler.OnCommand(client, pars);
		}

		/// <summary>
		/// Compiles the scripts into an assembly
		/// </summary>
		/// <param name="compileVB">True if the source files will be in VB.NET</param>
		/// <param name="path">Path to the source files</param>
		/// <param name="dllName">Name of the assembly to be generated</param>
		/// <param name="asm_names">References to other assemblies</param>
		/// <returns>True if succeeded</returns>
		public static bool CompileScripts(bool compileVB, string path, string dllName, string[] asm_names)
		{
			if (!path.EndsWith(@"\") && !path.EndsWith(@"/"))
				path = path + "/";

			//Reset the assemblies
			m_scripts.Clear();

			//Check if there are any scripts, if no scripts exist, that is fine as well
			ArrayList files = ParseDirectory(new DirectoryInfo(path), compileVB ? "*.vb" : "*.cs", true);
			if (files.Count == 0)
			{
				return true;
			}

			//Recompile is required as standard
			bool recompileRequired = true;

			//This file should hold the script infos
			FileInfo configFile = new FileInfo(dllName + ".xml");

			//If the script assembly is missing, recompile is required
			if (!File.Exists(dllName))
			{
				if (log.IsDebugEnabled)
					log.Debug("Script assembly missing, recompile required!");
			}
			else
			{
				//Script assembly found, check if we have a file modify info
				if (configFile.Exists)
				{
					//Ok, we have a config file containing the script file sizes and dates
					//let's check if any script was modified since last compiling them
					if (log.IsDebugEnabled)
						log.Debug("Found script info file");

					try
					{
						XMLConfigFile config = XMLConfigFile.ParseXMLFile(configFile);

						//Assume no scripts changed
						recompileRequired = false;

						ArrayList precompiledScripts = new ArrayList(config.Children.Keys);

						//Now test the files
						foreach (FileInfo finfo in files)
						{
							if (config[finfo.FullName]["size"].GetInt(0) != finfo.Length
								|| config[finfo.FullName]["lastmodified"].GetLong(0) != finfo.LastWriteTime.ToFileTime())
							{
								//Recompile required
								recompileRequired = true;
								break;
							}
							precompiledScripts.Remove(finfo.FullName);
						}

						recompileRequired |= precompiledScripts.Count > 0; // some compiled script was removed

						if (recompileRequired && log.IsDebugEnabled)
						{
							log.Debug("At least one file was modified, recompile required!");
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Error during script info file to scripts compare", e);
					}
				}
				else
				{
					if (log.IsDebugEnabled)
						log.Debug("Script info file missing, recompile required!");
				}
			}

			//If we need no compiling, we load the existing assembly!
			if (!recompileRequired)
			{
				try
				{
					Assembly asm = Assembly.LoadFrom(dllName);
					if (!m_scripts.Contains(asm))
						m_scripts.Add(asm);

					if (log.IsDebugEnabled)
						log.Debug("Precompiled script assembly loaded");
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Error loading precompiled script assembly, recompile required!", e);
				}

				//Try loading the commands
				if (!LoadCommands())
					throw new Exception("Could not load class CommandHandler");

				//Return success!
				return true;
			}

			//We need a recompile, if the dll exists, delete it firsthand
			if (File.Exists(dllName))
				File.Delete(dllName);

			CompilerResults res = null;
			try
			{
				CodeDomProvider compiler;

				if (compileVB)
				{
					compiler = new VBCodeProvider();
				}
				else
				{
					compiler = new CSharpCodeProvider( new Dictionary<string, string> { { "CompilerVersion", "v3.5" } } );
				}
				
				// Graveen: allow script compilation in debug or release mode
				#if DEBUG
					CompilerParameters param = new CompilerParameters(asm_names, dllName, true);
				#else
					CompilerParameters param = new CompilerParameters(asm_names, dllName, false);
				#endif
				param.GenerateExecutable = false;
				param.GenerateInMemory = false;
				param.WarningLevel = 2;
				param.CompilerOptions = @"/lib:." + Path.DirectorySeparatorChar + "lib";

				string[] filepaths = new string[files.Count];
				for (int i = 0; i < files.Count; i++)
					filepaths[i] = ((FileInfo)files[i]).FullName;

				res = compiler.CompileAssemblyFromFile(param, filepaths);

				//After compiling, collect
				GC.Collect();

				if (res.Errors.HasErrors)
				{
					foreach (CompilerError err in res.Errors)
					{
						if (err.IsWarning) continue;

						StringBuilder builder = new StringBuilder();
						builder.Append("   ");
						builder.Append(err.FileName);
						builder.Append(" Line:");
						builder.Append(err.Line);
						builder.Append(" Col:");
						builder.Append(err.Column);
						if (log.IsErrorEnabled)
						{
							log.Error("Script compilation failed because: ");
							log.Error(err.ErrorText);
							log.Error(builder.ToString());
						}
					}

					return false;
				}

				if (!m_scripts.Contains(res.CompiledAssembly))
					m_scripts.Add(res.CompiledAssembly);

				if (!LoadCommands())
				{
					throw new Exception("Could not load class CommandHandler");
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("CompileScripts", e);
				m_scripts.Clear();
			}
			//now notify our callbacks
			bool ret = false;
			if (res != null)
			{
				ret = !res.Errors.HasErrors;
			}
			if (ret == false)
				return ret;

			XMLConfigFile newconfig = new XMLConfigFile();
			foreach (FileInfo finfo in files)
			{
				newconfig[finfo.FullName]["size"].Set(finfo.Length);
				newconfig[finfo.FullName]["lastmodified"].Set(finfo.LastWriteTime.ToFileTime());
			}
			if (log.IsDebugEnabled)
				log.Debug("Writing script info file");

			newconfig.Save(configFile);

			return true;
		}

		/// <summary>
		/// searches the given assembly for AbilityActionHandlers
		/// </summary>
		/// <param name="asm">The assembly to search through</param>
		/// <returns>Hashmap consisting of keyName => AbilityActionHandler Type</returns>
		public static Hashtable FindAllAbilityActionHandler(Assembly asm)
		{
			Hashtable abHandler = new Hashtable();
			if (asm != null)
			{
				foreach (Type type in asm.GetTypes())
				{
					if (!type.IsClass) continue;
					if (type.GetInterface("DOL.GS.IAbilityActionHandler") == null) continue;

					object[] objs = type.GetCustomAttributes(typeof(SkillHandlerAttribute), false);
					for (int i = 0; i < objs.Length; i++)
					{
						if (objs[i] is SkillHandlerAttribute)
						{
							SkillHandlerAttribute attr = objs[i] as SkillHandlerAttribute;
							abHandler[attr.KeyName] = type;
							//DOLConsole.LogLine("Found ability action handler "+attr.KeyName+": "+type);
							//									break;
						}
					}
				}
			}
			return abHandler;
		}

		/// <summary>
		/// searches the script directory for SpecActionHandlers
		/// </summary>
		/// <param name="asm">The assembly to search through</param>
		/// <returns>Hashmap consisting of keyName => SpecActionHandler Type</returns>
		public static Hashtable FindAllSpecActionHandler(Assembly asm)
		{
			Hashtable specHandler = new Hashtable();
			if (asm != null)
			{
				foreach (Type type in asm.GetTypes())
				{
					if (!type.IsClass) continue;
					if (type.GetInterface("DOL.GS.ISpecActionHandler") == null) continue;

					object[] objs = type.GetCustomAttributes(typeof(SkillHandlerAttribute), false);
					for (int i = 0; i < objs.Length; i++)
					{
						if (objs[i] is SkillHandlerAttribute)
						{
							SkillHandlerAttribute attr = objs[0] as SkillHandlerAttribute;
							specHandler[attr.KeyName] = type;
							//DOLConsole.LogLine("Found spec action handler "+attr.KeyName+": "+type);
							break;
						}
					}
				}
			}
			return specHandler;
		}


		/// <summary>
		/// Searches for ClassSpec's by id in a given assembly
		/// </summary>
		/// <param name="asm">the assembly to search through</param>
		/// <param name="id">the classid to search</param>
		/// <returns>ClassSpec that was found or null if not found</returns>
		public static ICharacterClass FindCharacterClass(int id)
		{
			ArrayList asms = new ArrayList(Scripts);
			asms.Add(typeof(GameServer).Assembly);
			foreach (Assembly asm in asms)
			{
				foreach (Type type in asm.GetTypes())
				{
					// Pick up a class
					if (type.IsClass != true) continue;
					if (type.GetInterface("DOL.GS.ICharacterClass") == null) continue;

					try
					{
						object[] objs = type.GetCustomAttributes(typeof(CharacterClassAttribute), false);
						foreach (CharacterClassAttribute attrib in objs)
						{
							if (attrib.ID == id)
							{
								return (ICharacterClass)Activator.CreateInstance(type);
							}
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("FindCharacterClass", e);
					}
				}
			}
			return null;
		}


		/// <summary>
		/// Searches for NPC guild scripts
		/// </summary>
		/// <param name="realm">Realm for searching handlers</param>
		/// <param name="asm">The assembly to search through</param>
		/// <returns>
		/// all handlers that were found, guildname(string) => classtype(Type)
		/// </returns>
		protected static Hashtable FindAllNPCGuildScriptClasses(eRealm realm, Assembly asm)
		{
			Hashtable ht = new Hashtable();
			if (asm != null)
			{
				foreach (Type type in asm.GetTypes())
				{
					// Pick up a class
					if (type.IsClass != true) continue;
					if (!type.IsSubclassOf(typeof(GameNPC))) continue;

					try
					{
						object[] objs = type.GetCustomAttributes(typeof(NPCGuildScriptAttribute), false);
						if (objs.Length == 0) continue;

						foreach (NPCGuildScriptAttribute attrib in objs)
						{
							if (attrib.Realm == eRealm.None || attrib.Realm == realm)
							{
								ht[attrib.GuildName] = type;
							}

						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("FindAllNPCGuildScriptClasses", e);
					}
				}
			}
			return ht;
		}

		protected static Hashtable[] m_gs_guilds = new Hashtable[(int)eRealm._Last + 1];
		protected static Hashtable[] m_script_guilds = new Hashtable[(int)eRealm._Last + 1];

		/// <summary>
		/// searches for a npc guild script
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="realm"></param>
		/// <returns>type of class for searched npc guild or null</returns>
		public static Type FindNPCGuildScriptClass(string guild, eRealm realm)
		{
			if (string.IsNullOrEmpty(guild)) return null;

			Type type = null;
			if (m_script_guilds[(int)realm] == null)
			{
				Hashtable allScriptGuilds = new Hashtable();
				ArrayList asms = new ArrayList(Scripts);
				asms.Add(typeof(GameServer).Assembly);
				foreach (Assembly asm in asms)
				{
					Hashtable scriptGuilds = FindAllNPCGuildScriptClasses(realm, asm);
					if (scriptGuilds == null) continue;
					foreach (DictionaryEntry entry in scriptGuilds)
					{
						if (allScriptGuilds.ContainsKey(entry.Key)) continue; // guild is already found
						allScriptGuilds.Add(entry.Key, entry.Value);
					}
				}
				m_script_guilds[(int)realm] = allScriptGuilds;
			}

			//SmallHorse: First test if no realm-guild hashmap is null, then test further
			//Also ... you can not use "nullobject as anytype" ... this crashes!
			//You have to test against NULL result before casting it... read msdn doku
			if (m_script_guilds[(int)realm] != null && m_script_guilds[(int)realm][guild] != null)
				type = m_script_guilds[(int)realm][guild] as Type;

			if (type == null)
			{
				if (m_gs_guilds[(int)realm] == null)
				{
					Assembly gasm = Assembly.GetAssembly(typeof(GameServer));
					m_gs_guilds[(int)realm] = FindAllNPCGuildScriptClasses(realm, gasm);
				}
			}

			//SmallHorse: First test if no realm-guild hashmap is null, then test further
			//Also ... you can not use "nullobject as anytype" ... this crashes!
			//You have to test against NULL result before casting it... read msdn doku
			if (m_gs_guilds[(int)realm] != null && m_gs_guilds[(int)realm][guild] != null)
				type = m_gs_guilds[(int)realm][guild] as Type;

			return type;
		}


		private static Type m_defaultControlledBrainType = typeof(ControlledNpcBrain);
		public static Type DefaultControlledBrainType
		{
			get { return m_defaultControlledBrainType; }
			set { m_defaultControlledBrainType = value; }
		}

		/// <summary>
		/// Constructs a new brain for player controlled npcs
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static IControlledBrain CreateControlledBrain(GamePlayer owner)
		{
			Type[] constructorParams = new Type[] { typeof(GamePlayer) };
			ConstructorInfo handlerConstructor = m_defaultControlledBrainType.GetConstructor(constructorParams);
			return (IControlledBrain)handlerConstructor.Invoke(new object[] { owner });
		}


		/// <summary>
		/// Create a spell handler for caster with given spell
		/// </summary>
		/// <param name="caster">caster that uses the spell</param>
		/// <param name="spell">the spell itself</param>
		/// <param name="line">the line that spell belongs to or null</param>
		/// <returns>spellhandler or null if not found</returns>
		public static ISpellHandler CreateSpellHandler(GameLiving caster, Spell spell, SpellLine line)
		{
			if (spell == null || spell.SpellType.Length == 0) return null;

			ConstructorInfo handlerConstructor = (ConstructorInfo)m_spellhandlerConstructorCache[spell.SpellType];

			// try to find it in assemblies when not in cache
			if (handlerConstructor == null)
			{
				Type[] constructorParams = new Type[] { typeof(GameLiving), typeof(Spell), typeof(SpellLine) };
				ArrayList asms = new ArrayList(Scripts);
				asms.Add(typeof(GameServer).Assembly);
				foreach (Assembly script in asms)
				{
					foreach (Type type in script.GetTypes())
					{
						if (type.IsClass != true) continue;
						if (type.GetInterface("DOL.GS.Spells.ISpellHandler") == null) continue;

						// look for attribute
						try
						{
							object[] objs = type.GetCustomAttributes(typeof(SpellHandlerAttribute), false);
							if (objs.Length == 0) continue;

							foreach (SpellHandlerAttribute attrib in objs)
							{
								if (attrib.SpellType == spell.SpellType)
								{
									handlerConstructor = type.GetConstructor(constructorParams);
									if (log.IsDebugEnabled)
										log.Debug("Found spell handler " + type);
									break;
								}
							}
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("CreateSpellHandler", e);
						}
						if (handlerConstructor != null) break;
					}
				}

				if (handlerConstructor != null)
				{
					m_spellhandlerConstructorCache[spell.SpellType] = handlerConstructor;
				}
			}

			if (handlerConstructor != null)
			{
				try
				{
					return (ISpellHandler)handlerConstructor.Invoke(new object[] { caster, spell, line });
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Failed to create spellhandler " + handlerConstructor, e);
				}
			}
			else
			{
				if (log.IsErrorEnabled)
					log.Error("Couldn't find spell handler for spell type " + spell.SpellType);
			}
			return null;
		}

		/// <summary>
		/// Create server rules handler for specified server type
		/// </summary>
		/// <param name="serverType">server type used to look for rules handler</param>
		/// <returns>server rules handler or normal server type handler if errors</returns>
		public static IServerRules CreateServerRules(eGameServerType serverType)
		{
			Type rules = null;

			// first search in scripts
			foreach (Assembly script in Scripts)
			{
				foreach (Type type in script.GetTypes())
				{
					if (type.IsClass == false) continue;
					if (type.GetInterface("DOL.GS.ServerRules.IServerRules") == null) continue;

					// look for attribute
					try
					{
						object[] objs = type.GetCustomAttributes(typeof(ServerRulesAttribute), false);
						if (objs.Length == 0) continue;

						foreach (ServerRulesAttribute attrib in objs)
						{
							if (attrib.ServerType == serverType)
							{
								rules = type;
								break;
							}
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("CreateServerRules", e);
					}
					if (rules != null) break;
				}
			}

			if (rules == null)
			{
				// second search in gameserver
				foreach (Type type in Assembly.GetAssembly(typeof(GameServer)).GetTypes())
				{
					if (type.IsClass == false) continue;
					if (type.GetInterface("DOL.GS.ServerRules.IServerRules") == null) continue;

					// look for attribute
					try
					{
						object[] objs = type.GetCustomAttributes(typeof(ServerRulesAttribute), false);
						if (objs.Length == 0) continue;

						foreach (ServerRulesAttribute attrib in objs)
						{
							if (attrib.ServerType == serverType)
							{
								rules = type;
								break;
							}
						}
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("CreateServerRules", e);
					}
					if (rules != null) break;
				}

			}

			if (rules != null)
			{
				try
				{
					IServerRules rls = (IServerRules)Activator.CreateInstance(rules, null);
					if (log.IsInfoEnabled)
						log.Info("Found server rules for " + serverType + " server type (" + rls.RulesDescription() + ").");
					return rls;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("CreateServerRules", e);
				}
			}
			if (log.IsWarnEnabled)
				log.Warn("Rules for " + serverType + " server type not found, using \"normal\" server type rules.");
			return new NormalServerRules();
		}

		/// <summary>
		/// Search for a type by name; first in GameServer assembly then in scripts assemblies
		/// </summary>
		/// <param name="name">The type name</param>
		/// <returns>Found type or null</returns>
		public static Type GetType(string name)
		{
			Type t = typeof(GameServer).Assembly.GetType(name);
			if (t == null)
			{
				foreach (Assembly asm in Scripts)
				{
					t = asm.GetType(name);
					if (t == null) continue;
					return t;
				}
			}
			else
			{
				return t;
			}
			return null;
		}

		/// <summary>
		/// Finds all classes that derive from given type.
		/// First check scripts then GameServer assembly.
		/// </summary>
		/// <param name="baseType">The base class type.</param>
		/// <returns>Array of types or empty array</returns>
		public static Type[] GetDerivedClasses(Type baseType)
		{
			if (baseType == null)
				return new Type[0];

			ArrayList types = new ArrayList();
			ArrayList asms = new ArrayList(Scripts);
			asms.Add(typeof(GameServer).Assembly);

			foreach (Assembly asm in asms)
				foreach (Type t in asm.GetTypes())
				{
					if (t.IsClass && baseType.IsAssignableFrom(t))
						types.Add(t);
				}

			return (Type[])types.ToArray(typeof(Type));
		}
	}
}
