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
using DOL.GS.Styles;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.UseSkill, ClientStatus.PlayerInGame)]
	public class UseSkillHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region IPacketHandler Members

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int flagSpeedData = packet.ReadShort();
			int index = packet.ReadByte();
			int type = packet.ReadByte();

			new UseSkillAction(client.Player, flagSpeedData, index, type).Start(1);

			return 1;
		}

		#endregion

		#region Nested type: UseSkillAction

		/// <summary>
		/// Handles player use skill actions
		/// </summary>
		protected class UseSkillAction : RegionAction
		{
			/// <summary>
			/// The speed and flags data
			/// </summary>
			protected readonly int m_flagSpeedData;

			/// <summary>
			/// The skill index
			/// </summary>
			protected readonly int m_index;

			/// <summary>
			/// The skill type
			/// </summary>
			protected readonly int m_type;

			/// <summary>
			/// Constructs a new UseSkillAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="flagSpeedData">The skill type</param>
			/// <param name="index">The skill index</param>
			/// <param name="type">The skill type</param>
			public UseSkillAction(GamePlayer actionSource, int flagSpeedData, int index, int type)
				: base(actionSource)
			{
				m_flagSpeedData = flagSpeedData;
				m_index = index;
				m_type = type;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;
				int index = m_index;

				if ((m_flagSpeedData & 0x200) != 0)
				{
					player.CurrentSpeed = (short)(-(m_flagSpeedData & 0x1ff)); // backward movement
				}
				else
				{
					player.CurrentSpeed = (short)(m_flagSpeedData & 0x1ff); // forwardmovement
				}

				player.IsStrafing = (m_flagSpeedData & 0x4000) != 0;
				player.TargetInView = (m_flagSpeedData & 0xa000) != 0; // why 2 bits? that has to be figured out
				player.GroundTargetInView = ((m_flagSpeedData & 0x1000) != 0);

				//DOLConsole.LogDump
				Skill sk = null;
				if (m_type > 0)
				{
					IList skillList = player.GetNonTrainableSkillList();
					if (index < skillList.Count)
					{
						sk = skillList[index] as Skill;
					}
					else
					{
						IList styles = player.GetStyleList();
						if (index < skillList.Count + styles.Count)
						{
							index -= skillList.Count;
							sk = styles[index] as Skill;
						}
						else
						{
							List<SpellLine> spelllines = player.GetSpellLines();
							if (index < skillList.Count + styles.Count + player.GetSpellCount())
							{
								index -= (skillList.Count + styles.Count);
								Spell spell = null;
								SpellLine spellline = null;

								lock (player.lockSpellLinesList)
								{
									var spelllist = player.GetUsableSpells(spelllines, false);

									if (index >= spelllist.Count)
									{
										index -= spelllist.Count;
									}
									else
									{
										var spellenum = spelllist.Values.GetEnumerator();
										int i = 0;
										while (spellenum.MoveNext())
										{
											if (i == index)
											{
												spell = spellenum.Current.Key;
												spellline = spellenum.Current.Value;
												break;
											}

											i++;
										}
									}
								}

								if (spell != null)
								{
									player.CastSpell(spell, spellline);
									return;
								}
							}
							// TODO   Song and RA
						}
					}
				}
				else
				{
					IList specs = player.GetSpecList();
					if (index < specs.Count)
					{
						sk = specs[index] as Skill;
					}
				}

				if (sk != null)
				{
					if (sk is Style)
					{
						player.ExecuteWeaponStyle((Style)sk);
						return;
					}
					//player.Out.SendMessage("you triggered skill "+sk.Name, eChatType.CT_Advise, eChatLoc.CL_SystemWindow);

					int reuseTime = player.GetSkillDisabledDuration(sk);
					if (reuseTime > 60000)
					{
						player.Out.SendMessage(
							string.Format("You must wait {0} minutes {1} seconds to use this ability!", reuseTime/60000, reuseTime%60000/1000),
							eChatType.CT_System, eChatLoc.CL_SystemWindow);
						if (player.Client.Account.PrivLevel < 2) return;
					}
					else if (reuseTime > 0)
					{
						player.Out.SendMessage(string.Format("You must wait {0} seconds to use this ability!", reuseTime/1000 + 1),
						                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
						
						if (player.Client.Account.PrivLevel < 2) 
							return;
					}

					if (sk is Ability)
					{
						var ab = sk as Ability;
						IAbilityActionHandler handler = SkillBase.GetAbilityActionHandler(ab.KeyName);
						if (handler != null)
						{
							handler.Execute(ab, player);
							return;
						}
						
						ab.Execute(player);
					}
					if (sk is Specialization)
					{
						var spec = sk as Specialization;
						ISpecActionHandler handler = SkillBase.GetSpecActionHandler(spec.KeyName);
						if (handler != null)
						{
							handler.Execute(spec, player);
							return;
						}
					}
				}
				else
				{
					if (Log.IsWarnEnabled)
						Log.Warn("skill not handled because it was not found on player, shouldn't happen");
				}

				if (sk == null)
				{
					player.Out.SendMessage("Skill is not implemented.", eChatType.CT_Advise, eChatLoc.CL_SystemWindow);
				}
			}
		}

		#endregion
	}
}