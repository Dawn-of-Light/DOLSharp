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
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using log4net;

namespace DOL.GS.RealmAbilities
{
	//http://daocpedia.de/index.php/Kettenblitz
	public class ChainLightningAbilityHandler : RR5RealmAbility
	{
		public ChainLightningAbilityHandler(DBAbility dba, int level) : base(dba, level) { }

		const int m_reuseTime = 600;
		const int m_range = 1875;
		const ushort m_spellID = 0;

		/// <summary>
		/// Execute engage ability
		/// </summary>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			if (living.TargetObject == null || !(living.TargetObject is GameLiving) || !GameServer.ServerRules.IsAllowedToAttack(living, living.TargetObject as GameLiving, true))
			{
				if (living is GamePlayer)
					(living as GamePlayer).Out.SendMessage("You cannot use this ability on this target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			int maxRange = (int)(m_range * living.GetModified(eProperty.SpellRange) * 0.01);
			if (!WorldMgr.CheckDistance(living, living.TargetObject, maxRange))
			{
				if (living is GamePlayer)
					(living as GamePlayer).Out.SendMessage(living.TargetObject.Name + " is too far away!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			SpellLine abilitiesLine = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities);
			if (abilitiesLine != null)
			{
				IList spells = SkillBase.GetSpellList(abilitiesLine.KeyName);
				if (spells != null)
				{
					foreach (Spell spell in spells)
					{
						if (spell.ID == m_spellID)
						{
							living.CastSpell(spell, abilitiesLine);
							living.DisableSkill(this, m_reuseTime * 1000);
							break;
						}
					}
				}
				else
					log.Warn("Chain Lightning Spell not found");
			}
			else
				log.Warn("Chain Lightning SpellLine not found");


		}
	}
}
