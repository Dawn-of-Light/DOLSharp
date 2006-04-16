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
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using DOL.GS.Database;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using NHibernate.Expression;
using log4net;

namespace DOL.GS.Loot
{
	/// <summary>
	/// This class is used to represent all extra money loot
	/// </summary>
	public class ExtraMoneyLoot : BaseLoot
	{
		/// <summary>
		/// This method is used to return the object to add to the world base on this loot
		/// </summary>	
		public override GameObjectTimed GetLoot(GameMob killedMob, GameLiving killerPlayer)
		{
			GameMoney money = new GameMoney();
			money.Model = 82;
			money.Realm = killerPlayer.Realm;

			int lvl = killedMob.Level+1;
			if (lvl < 1) lvl = 1;
			int minLoot = 2+((lvl*lvl*lvl)>>3);
			long moneyCount = minLoot+Util.Random(minLoot>>1);

			if(Util.Chance(40))
			{
				money.Name = "large chest";
				money.TotalCopper = moneyCount * 2;
			}
			else
			{
				money.Name = "small chest";
				money.TotalCopper = moneyCount / 2;
			}

			return money;
		}
	}
}
