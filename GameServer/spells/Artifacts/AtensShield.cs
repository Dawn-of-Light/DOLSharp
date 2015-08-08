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
namespace DOL.GS.Spells
{
	using System;
	using Database;
	using Events;

	[SpellHandler("AtensShield")]
	public class AtensShield : SummonItemSpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private ItemTemplate m_goldenTridentofFlame;

		public AtensShield(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			if (caster.Realm == eRealm.Albion)
			{
				m_goldenTridentofFlame = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Golden_Trident_of_Flame'") ?? Javelin;
				items.Add (GameInventoryItem.Create(m_goldenTridentofFlame));
				return;
			}

			if (caster.Realm == eRealm.Midgard)
			{
				m_goldenTridentofFlame = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Golden_Trident_of_Flame'") ?? JavelinM;
				items.Add (GameInventoryItem.Create(m_goldenTridentofFlame));
				return;
			}
			if (caster.Realm == eRealm.Hibernia)
			{
				m_goldenTridentofFlame = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Golden_Trident_of_Flame'") ?? JavelinH;
				items.Add (GameInventoryItem.Create(m_goldenTridentofFlame));
				return;
			}
		}

		private ItemTemplate Javelin
		{
			get
			{
				m_goldenTridentofFlame = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_Flame");
				if (m_goldenTridentofFlame == null)
				{
					if (log.IsWarnEnabled) log.Warn("Could not find Golden_Trident_of_Flame, loading it ...");
					m_goldenTridentofFlame = new ItemTemplate();
					m_goldenTridentofFlame.Id_nb = "Golden_Trident_of_Flame";
					m_goldenTridentofFlame.Name = "Golden Triden of Flame";
					m_goldenTridentofFlame.Level = 45;
					m_goldenTridentofFlame.Durability = 50000;
					m_goldenTridentofFlame.MaxDurability = 50000;
					m_goldenTridentofFlame.Condition = 50000;
					m_goldenTridentofFlame.MaxCondition = 50000;
					m_goldenTridentofFlame.Quality = 100;
					m_goldenTridentofFlame.DPS_AF = 150;
					m_goldenTridentofFlame.SPD_ABS = 36;
					m_goldenTridentofFlame.Type_Damage = 2;
					m_goldenTridentofFlame.Object_Type = 3;
					m_goldenTridentofFlame.Item_Type = 10;
					m_goldenTridentofFlame.Color = 48;
					m_goldenTridentofFlame.Effect = 77;
					m_goldenTridentofFlame.Model = 1004;
					m_goldenTridentofFlame.Bonus = 35;
					m_goldenTridentofFlame.Bonus1 = 60;
					m_goldenTridentofFlame.Bonus2 = 19;
					m_goldenTridentofFlame.Bonus3 = 20;
					m_goldenTridentofFlame.Bonus4 = 3;
					m_goldenTridentofFlame.Bonus5 = 3;
					m_goldenTridentofFlame.Bonus1Type = 10;
					m_goldenTridentofFlame.Bonus2Type = 1;
					m_goldenTridentofFlame.Bonus3Type = 148;
					m_goldenTridentofFlame.Bonus4Type = 155;
					m_goldenTridentofFlame.Bonus5Type = 200;
					m_goldenTridentofFlame.IsPickable = false;
					m_goldenTridentofFlame.IsDropable = false;
					m_goldenTridentofFlame.CanDropAsLoot = false;
					m_goldenTridentofFlame.IsTradable = false;
					m_goldenTridentofFlame.MaxCount = 1;
					m_goldenTridentofFlame.PackSize = 1;
					m_goldenTridentofFlame.ProcSpellID = 32116;

				}
				return m_goldenTridentofFlame;
			}
		}

		private ItemTemplate JavelinM
		{
			get
			{
				m_goldenTridentofFlame = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_FlameM");
				if (m_goldenTridentofFlame == null)
				{
					if (log.IsWarnEnabled) log.Warn("Could not find Golden_Trident_of_Flame, loading it ...");
					m_goldenTridentofFlame = new ItemTemplate();
					m_goldenTridentofFlame.Id_nb = "Golden_Trident_of_FlameM";
					m_goldenTridentofFlame.Name = "Golden Triden of Flame";
					m_goldenTridentofFlame.Level = 45;
					m_goldenTridentofFlame.Durability = 50000;
					m_goldenTridentofFlame.MaxDurability = 50000;
					m_goldenTridentofFlame.Condition = 50000;
					m_goldenTridentofFlame.MaxCondition = 50000;
					m_goldenTridentofFlame.Quality = 100;
					m_goldenTridentofFlame.DPS_AF = 150;
					m_goldenTridentofFlame.SPD_ABS = 36;
					m_goldenTridentofFlame.Type_Damage = 2;
					m_goldenTridentofFlame.Object_Type = 11;
					m_goldenTridentofFlame.Item_Type = 10;
					m_goldenTridentofFlame.Color = 48;
					m_goldenTridentofFlame.Effect = 77;
					m_goldenTridentofFlame.Model = 1004;
					m_goldenTridentofFlame.Bonus = 35;
					m_goldenTridentofFlame.Bonus1 = 60;
					m_goldenTridentofFlame.Bonus2 = 19;
					m_goldenTridentofFlame.Bonus3 = 20;
					m_goldenTridentofFlame.Bonus4 = 3;
					m_goldenTridentofFlame.Bonus5 = 3;
					m_goldenTridentofFlame.Bonus1Type = 10;
					m_goldenTridentofFlame.Bonus2Type = 1;
					m_goldenTridentofFlame.Bonus3Type = 148;
					m_goldenTridentofFlame.Bonus4Type = 155;
					m_goldenTridentofFlame.Bonus5Type = 200;
					m_goldenTridentofFlame.IsPickable = false;
					m_goldenTridentofFlame.IsDropable = false;
					m_goldenTridentofFlame.CanDropAsLoot = false;
					m_goldenTridentofFlame.IsTradable = false;
					m_goldenTridentofFlame.MaxCount = 1;
					m_goldenTridentofFlame.PackSize = 1;
					m_goldenTridentofFlame.ProcSpellID = 32116;

				}
				return m_goldenTridentofFlame;
			}
		}

		private ItemTemplate JavelinH
		{
			get
			{
				m_goldenTridentofFlame = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_FlameH");
				if (m_goldenTridentofFlame == null)
				{
					if (log.IsWarnEnabled) log.Warn("Could not find Golden_Trident_of_Flame, loading it ...");
					m_goldenTridentofFlame = new ItemTemplate();
					m_goldenTridentofFlame.Id_nb = "Golden_Trident_of_FlameH";
					m_goldenTridentofFlame.Name = "Golden Triden of Flame";
					m_goldenTridentofFlame.Level = 45;
					m_goldenTridentofFlame.Durability = 50000;
					m_goldenTridentofFlame.MaxDurability = 50000;
					m_goldenTridentofFlame.Condition = 50000;
					m_goldenTridentofFlame.MaxCondition = 50000;
					m_goldenTridentofFlame.Quality = 100;
					m_goldenTridentofFlame.DPS_AF = 150;
					m_goldenTridentofFlame.SPD_ABS = 36;
					m_goldenTridentofFlame.Type_Damage = 2;
					m_goldenTridentofFlame.Object_Type = 19;
					m_goldenTridentofFlame.Item_Type = 10;
					m_goldenTridentofFlame.Color = 48;
					m_goldenTridentofFlame.Effect = 77;
					m_goldenTridentofFlame.Model = 1004;
					m_goldenTridentofFlame.Bonus = 35;
					m_goldenTridentofFlame.Bonus1 = 60;
					m_goldenTridentofFlame.Bonus2 = 19;
					m_goldenTridentofFlame.Bonus3 = 20;
					m_goldenTridentofFlame.Bonus4 = 3;
					m_goldenTridentofFlame.Bonus5 = 3;
					m_goldenTridentofFlame.Bonus1Type = 10;
					m_goldenTridentofFlame.Bonus2Type = 1;
					m_goldenTridentofFlame.Bonus3Type = 148;
					m_goldenTridentofFlame.Bonus4Type = 155;
					m_goldenTridentofFlame.Bonus5Type = 200;
					m_goldenTridentofFlame.IsPickable = false;
					m_goldenTridentofFlame.IsDropable = false;
					m_goldenTridentofFlame.CanDropAsLoot = false;
					m_goldenTridentofFlame.IsTradable = false;
					m_goldenTridentofFlame.MaxCount = 1;
					m_goldenTridentofFlame.PackSize = 1;
					m_goldenTridentofFlame.ProcSpellID = 32116;

				}
				return m_goldenTridentofFlame;
			}
		}

		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			base.OnDirectEffect(target, effectiveness);
			GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
		}

		private static void OnPlayerLeft(DOLEvent e, object sender, EventArgs arguments)
		{
			if (!(sender is GamePlayer))
				return;

			GamePlayer player = sender as GamePlayer;
			lock (player.Inventory)
			{
				var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
				foreach (InventoryItem invItem in items)
				{
					if (invItem.Id_nb.Equals("Golden_Trident_of_Flame"))
						player.Inventory.RemoveItem(invItem);

					if (invItem.Id_nb.Equals("Golden_Trident_of_FlameM"))
						player.Inventory.RemoveItem(invItem);

					if (invItem.Id_nb.Equals("Golden_Trident_of_FlameH"))
						player.Inventory.RemoveItem(invItem);

				}
			}
			GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
		}
	}
}