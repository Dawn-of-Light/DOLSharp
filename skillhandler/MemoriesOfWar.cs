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
using DOL.GS;
using DOL.Database;

namespace DOL.GS.SkillHandler
{
	//Memories of War: Upon reaching level 41, the Hero, Warrior and Armsman will begin to gain more magic resistance
	//(spell damage reduction only) as they progress towards level 50. At each level beyond 41 they gain 2%-3% extra
	//resistance per level. At level 50, they will have the full 15% benefit.
	[SkillHandlerAttribute(Abilities.MemoriesOfWar)]
	public class MemoriesOfWar : StatChangingAbility
	{
		public MemoriesOfWar(DBAbility dba, int level)
			: base(dba, 1, eProperty.Resist_Body
			       & eProperty.Resist_Cold
			       & eProperty.Resist_Energy
			       & eProperty.Resist_Heat
			       & eProperty.Resist_Matter
			       & eProperty.Resist_Spirit)
		{
		}
		public override int GetAmountForLevel(int level)
		{
			return 15;
		}
	}
}
