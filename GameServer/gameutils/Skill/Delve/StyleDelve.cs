﻿/*
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
using DOL.GS.PacketHandler;
using DOL.GS.Styles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS.Delve
{
	public class StyleDelve : SkillDelve
	{
		private GameClient clt;
		private Style style;

		private int LevelZeroDamage => (int)Math.Round(style.GrowthOffset / 0.295);
		private int DamageIncreasePerLevel => (int)Math.Round(style.GrowthRate / 0.295);

		public StyleDelve(GameClient clt, int id)
		{
			this.clt = clt;
			var sk = clt.Player.GetAllUsableSkills().Where(e => e.Item1.InternalID == id && e.Item1 is Style).FirstOrDefault();

			if (sk == null || sk.Item1 == null)
			{
				style = SkillBase.GetStyleByInternalID(id);
			}
			else if (sk.Item1 is Style)
			{
				style = (Style)sk.Item1;
			}

			DelveType = "Style";
			Index = unchecked((short)id);
		}

		public override ClientDelve GetClientDelve()
		{
			if (style == null) return NotFoundClientDelve;

			var styles = clt.Player.GetSpecList().SelectMany(e => e.PretendStylesForLiving(clt.Player, clt.Player.MaxLevel));

			var clientDelve = new ClientDelve("Style");
			clientDelve.Index = Index;

			if (style.OpeningRequirementType == Style.eOpening.Offensive && style.AttackResultRequirement == Style.eAttackResultRequirement.Style)
			{
				Style st = styles.Where(s => s.ID == style.OpeningRequirementValue).FirstOrDefault();
				if (st != null)
				{
					clientDelve.AddElement("OpeningStyle", st.Name);
				}
			}

			var followupStyles = styles
				.Where(s => (s.OpeningRequirementType == Style.eOpening.Offensive && s.AttackResultRequirement == Style.eAttackResultRequirement.Style && s.OpeningRequirementValue == style.ID))
				.Select(s => s.Name);
			clientDelve.AddElement("FollowupStyle", followupStyles);
			clientDelve.AddElement("Name", style.Name);
			clientDelve.AddElement("Icon", style.Icon);
			clientDelve.AddElement("Level", style.Level);
			clientDelve.AddElement("Fatigue", style.EnduranceCost);
			clientDelve.AddElement("DefensiveMod", style.BonusToDefense);
			clientDelve.AddElement("AttackMod", style.BonusToHit);
			clientDelve.AddElement("OpeningDamage", LevelZeroDamage + style.Level * DamageIncreasePerLevel);
			clientDelve.AddElement("LevelBonus", DamageIncreasePerLevel);
			clientDelve.AddElement("OpeningType", (int)style.OpeningRequirementType);
			if (style.OpeningRequirementType == Style.eOpening.Positional)
				clientDelve.AddElement("OpeningNumber", style.OpeningRequirementValue);
			if (style.WeaponTypeRequirement > 0)
				clientDelve.AddElement("Weapon", style.GetRequiredWeaponName());
			clientDelve.AddElement("OpeningResult", (int)style.AttackResultRequirement);
			clientDelve.AddElement("Hidden", style.StealthRequirement);
			foreach (var proc in style.Procs)
			{
				if (proc.Item2 == 0 || proc.Item2 == style.ClassID)
				{
					clientDelve.AddElement("SpecialNumber", proc.Item1.InternalID);
					clientDelve.AddElement("SpecialType", "1");
				}
			}

			return clientDelve;
		}

		public override IEnumerable<ClientDelve> GetClientDelves()
		{
			var result = new List<ClientDelve>() { };
			foreach (var proc in style.Procs)
			{
				if (proc.Item2 == 0 || proc.Item2 == style.ClassID)
				{
					var procDelve = new SpellDelve(proc.Item1).GetClientDelve();
					result.Add(procDelve);
				}
			}
			result.Add(GetClientDelve());
			return result;
		}
    }
}
