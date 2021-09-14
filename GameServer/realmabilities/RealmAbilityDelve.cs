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
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS
{
    public class RealmAbilityDelve : SkillDelve
    {
		private Skill ra;

		public RealmAbilityDelve(GameClient client, int skillIndex)
        {
			var ra = client.Player.GetAllUsableSkills().Where(e => e.Item1.InternalID == skillIndex && e.Item1 is Ability).Select(e => e.Item1).FirstOrDefault();

			if (ra == null)
			{
				ra = SkillBase.GetAbilityByInternalID(skillIndex);
			}

			DelveType = "RealmAbility";
			Index = unchecked((short)skillIndex);
		}

        public override ClientDelve GetClientDelve()
        {
			var delve = new ClientDelve(DelveType);
			delve.AddElement("Index", Index);

			if (ra is RealmAbility realmAbility)
			{
				delve.AddElement("Name", realmAbility.Name);
				if (ra.Icon > 0)
					delve.AddElement("icon", realmAbility.Icon);

				for (int i = 0; i <= realmAbility.MaxLevel - 1; i++)
				{
					if (realmAbility.CostForUpgrade(i) > 0)
						delve.AddElement(string.Format("TrainingCost_{0}", (i + 1)), realmAbility.CostForUpgrade(i));
				}

				if (ra is TimedRealmAbility timedRealmAbility)
				{
					for (int i = 1; i <= timedRealmAbility.MaxLevel; i++)
					{
						delve.AddElement(string.Format("ReuseTimer_{0}", i), timedRealmAbility.GetReUseDelay(i));
					}
				}
			}
			else if (ra != null)
			{
				delve.AddElement("Name", ra.Name);
			}
			else
			{
				delve.AddElement("Name", "(not found)");
			}

			return delve;
		}

		public override IEnumerable<ClientDelve> GetClientDelves()
		{
			return new List<ClientDelve>() { GetClientDelve() };
		}
	}
}
