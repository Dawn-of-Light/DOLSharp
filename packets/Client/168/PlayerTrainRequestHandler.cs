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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS.Commands;
using DOL.GS.RealmAbilities;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// handles Train clicks from Trainer Window
	/// D4 is up to 1.104
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0xD4 ^ 168, "Handles Player Train Requests")]
	public class PlayerTrainRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			uint x = packet.ReadInt();
			uint y = packet.ReadInt();
			int idline = packet.ReadByte();
			int unk = packet.ReadByte();
			int row = packet.ReadByte();
			int skillindex = packet.ReadByte();

			// idline not null so this is a Champion level training window
			if (idline > 0)
			{
				ChampSpec spec = ChampSpecMgr.GetAbilityFromIndex(idline, row, skillindex);
				if (spec != null)
				{
					if (client.Player.HaveChampionSpell(spec.SpellID))
					{
						client.Out.SendMessage("You already have that ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (!client.Player.IsCSAvailable(idline, row, skillindex))
					{
						client.Out.SendMessage("You do not meet the requirements for that ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if ((client.Player.ChampionSpecialtyPoints - spec.Cost) < 0)
					{
						client.Out.SendMessage("You do not have enough champion specialty points for that ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					client.Player.ChampionSpecialtyPoints -= spec.Cost;
					SpellLine sl = SkillBase.GetSpellLine(GlobalSpellsLines.Champion_Spells + client.Player.Name);
					if (sl.Spec.StartsWith("?"))
					{
						SpellLine line = new SpellLine(GlobalSpellsLines.Champion_Spells + client.Player.Name, GlobalSpellsLines.Champion_Spells, GlobalSpellsLines.Champion_Spells, true);
						SkillBase.RegisterSpellLine(line);
					}
					SkillBase.AddSpellToList(GlobalSpellsLines.Champion_Spells + client.Player.Name, spec.SpellID);
					client.Player.ChampionSpells += spec.SpellID.ToString() + "|1;";

					client.Player.AddSpellLine(sl);
					client.Player.UpdateSpellLineLevels(false);
					client.Player.RefreshSpecDependantSkills(true);
					//client.Out.SendUpdatePoints();
					//client.Out.SendUpdatePlayer();
					client.Out.SendMessage("You gain an ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendChampionTrainerWindow(idline);
					client.Out.SendUpdatePlayerSkills();
					return;
				}
				else { client.Out.SendMessage("Didn't find spec!", eChatType.CT_System, eChatLoc.CL_SystemWindow); }
				return;
			}

			IList speclist = client.Player.GetSpecList();
			if (skillindex < speclist.Count)
			{
				Specialization spec = (Specialization)speclist[skillindex];
				if (spec.Level >= client.Player.Level)
				{
					client.Out.SendMessage("You can't train in this specialization again this level!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				// Graveen - autotrain 1.87 - allow players to train their AT specs even if no pts left
				client.Player.SkillSpecialtyPoints += client.Player.GetAutoTrainPoints(spec, 2);

				if (client.Player.SkillSpecialtyPoints >= spec.Level + 1)
				{
					client.Player.SkillSpecialtyPoints -= (ushort)(spec.Level + 1);
					spec.Level++;
					client.Player.OnSkillTrained(spec);

					client.Out.SendUpdatePoints();
					client.Out.SendTrainerWindow();
					return;
				}
				else
				{
					client.Out.SendMessage("That specialization costs " + (spec.Level + 1) + " specialization points!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("You don't have that many specialization points left for this level.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			}
			else if (skillindex >= 100)
			{
				IList offeredRA = (IList)client.Player.TempProperties.getProperty<object>("OFFERED_RA", null);
				if (offeredRA != null && skillindex < offeredRA.Count + 100)
				{
					RealmAbility ra = (RealmAbility)offeredRA[skillindex - 100];
					int cost = ra.CostForUpgrade(ra.Level - 1);
					if (client.Player.RealmSpecialtyPoints < cost)
					{
						client.Out.SendMessage(ra.Name + " costs " + (cost) + " realm ability points!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage("You don't have that many realm ability points left to get this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (!ra.CheckRequirement(client.Player))
					{
						client.Out.SendMessage("You are not experienced enough to get " + ra.Name + " now. Come back later.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					// get a copy of the ability since we use prototypes
					RealmAbility ability = SkillBase.GetAbility(ra.KeyName, ra.Level) as RealmAbility;
					if (ability != null)
					{
						client.Player.RealmSpecialtyPoints -= cost;
						client.Player.AddAbility(ability);
						client.Out.SendUpdatePoints();
						client.Out.SendUpdatePlayer();
						client.Out.SendUpdatePlayerSkills();
						client.Out.SendTrainerWindow();
					}
					else
					{
						client.Out.SendMessage("Unfortunately your training failed. Please report that to admins or game master. Thank you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						log.Error("Realm Ability " + ra.Name + "(" + ra.KeyName + ") unexpected not found");
					}
					return;
				}
			}

			if (log.IsErrorEnabled)
				log.Error("Player <" + client.Player.Name + "> requested to train incorrect skill index");
		}
	}
	
	/// <summary>
	/// Handles Train clicks from Trainer Window
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0xFB ^ 168, "Handles Player Train")]
	public class PlayerTrainHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			// A trainer of the appropriate class must be around (or global trainer, with TrainedClass = eCharacterClass.Unknow
			GameTrainer trainer = client.Player.TargetObject as DOL.GS.GameTrainer;
			if (trainer == null || (trainer.TrainedClass != (eCharacterClass)client.Player.CharacterClass.ID && trainer.TrainedClass !=  eCharacterClass.Unknown))
			{
				client.Out.SendMessage("You must select a valid trainer for your class.", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
				return;
			}
			
			//Specializations - 8 trainable specs max
			uint size = 8;
			long position = packet.Position;
			IList<uint> skills = new List<uint>();
			Dictionary<uint, uint> amounts = new Dictionary<uint, uint>();
			bool stop = false;
			for (uint i = 0; i < size; i++)
			{
				uint code = packet.ReadInt();
				if (!stop)
				{
					if (code == 0xFFFFFFFF) stop = true;
					else
					{
						if (!skills.Contains(code))
							skills.Add(code);
					}
				}
			}

			foreach (uint code in skills)
			{
				uint val = packet.ReadInt();

				if (!amounts.ContainsKey(code) && val > 1)
					amounts.Add(code, val);
			}

			IList specs = client.Player.GetSpecList();
			uint skillcount = 0;
			IList<string> done = new List<string>();
			bool trained = false;
			
			// Graveen: the trainline command is called
			foreach (Specialization spec in specs)
			{
				if (amounts.ContainsKey(skillcount))
				{
					if (spec.Level < amounts[skillcount])
					{
						TrainCommandHandler train = new TrainCommandHandler(true);
						train.OnCommand(client, new string[] { "&trainline", spec.KeyName, amounts[skillcount].ToString() });
						trained = true;
					}
				}
				skillcount++;
			}

			//RealmAbilities
			packet.Seek(position + 64, System.IO.SeekOrigin.Begin);
			size = 50;//50 RA's max?
			amounts.Clear();
			for (uint i = 0; i < size; i++)
			{
				uint val = packet.ReadInt();

				if (val > 0 && !amounts.ContainsKey(i))
				{
					amounts.Add(i, val);
				}
			}
			uint index = 0;
			if (amounts != null && amounts.Count > 0)
			{
				List<RealmAbility> ras = SkillBase.GetClassRealmAbilities(client.Player.CharacterClass.ID);
				foreach (RealmAbility ra in ras)
				{
					if (ra is RR5RealmAbility)
						continue;

					if (amounts.ContainsKey(index))
					{
						RealmAbility playerRA = (RealmAbility)client.Player.GetAbility(ra.KeyName);
						if (playerRA != null
						    && (playerRA.Level >= ra.MaxLevel || playerRA.Level >= amounts[index]))
						{
							index++;
							continue;
						}

						int cost = 0;
						for (int i = playerRA != null ? playerRA.Level : 0; i < amounts[index]; i++)
							cost += ra.CostForUpgrade(i);
						if (client.Player.RealmSpecialtyPoints < cost)
						{
							client.Out.SendMessage(ra.Name + " costs " + (cost) + " realm ability points!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							client.Out.SendMessage("You don't have that many realm ability points left to get this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							index++;
							continue;
						}
						if (!ra.CheckRequirement(client.Player))
						{
							client.Out.SendMessage("You are not experienced enough to get " + ra.Name + " now. Come back later.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							index++;
							continue;
						}

						bool valid = false;
						if (playerRA != null)
						{
							playerRA.Level = (int)amounts[index];
							valid = true;
						}
						else
						{
							RealmAbility ability = SkillBase.GetAbility(ra.KeyName, (int)amounts[index]) as RealmAbility;
							if (ability != null)
							{
								valid = true;
								client.Player.AddAbility(ability, false);
							}
						}
						if (valid)
						{
							client.Player.RealmSpecialtyPoints -= cost;
							client.Out.SendUpdatePoints();
							client.Out.SendUpdatePlayer();
							client.Out.SendCharResistsUpdate();
							client.Out.SendCharStatsUpdate();
							client.Out.SendUpdatePlayerSkills();
							client.Out.SendTrainerWindow();
							trained = true;
						}
						else
						{
							client.Out.SendMessage("Unfortunately your training failed. Please report that to admins or game master. Thank you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}

					index++;
				}
			}
			
			if (trained)
				client.Player.SaveIntoDatabase();
		}
		
		/// <summary>
		/// Summon trainer window
		/// </summary>
		[PacketHandlerAttribute(PacketHandlerType.TCP, 0xD3 ^ 168, "Call Player Train Window")]
		public class PlayerTrainWindowHandler : IPacketHandler
		{
			public void HandlePacket(GameClient client, GSPacketIn packet)
			{
				client.Out.SendTrainerWindow();
				//packet.ReadByte();
			}
		}
	}
}
