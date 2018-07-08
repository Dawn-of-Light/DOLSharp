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
                m_goldenTridentofFlame = GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_Flame") ?? Javelin;
                items.Add(GameInventoryItem.Create(m_goldenTridentofFlame));
                return;
            }

            if (caster.Realm == eRealm.Midgard)
            {
                m_goldenTridentofFlame = GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_Flame") ?? JavelinM;
                items.Add(GameInventoryItem.Create(m_goldenTridentofFlame));
                return;
            }

            if (caster.Realm == eRealm.Hibernia)
            {
                m_goldenTridentofFlame = GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_Flame") ?? JavelinH;
                items.Add(GameInventoryItem.Create(m_goldenTridentofFlame));
            }
        }

        private ItemTemplate Javelin
        {
            get
            {
                m_goldenTridentofFlame = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_Flame");
                if (m_goldenTridentofFlame == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Golden_Trident_of_Flame, loading it ...");
                    }

                    m_goldenTridentofFlame = new ItemTemplate
                    {
                        Id_nb = "Golden_Trident_of_Flame",
                        Name = "Golden Triden of Flame",
                        Level = 45,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 36,
                        Type_Damage = 2,
                        Object_Type = 3,
                        Item_Type = 10,
                        Color = 48,
                        Effect = 77,
                        Model = 1004,
                        Bonus = 35,
                        Bonus1 = 60,
                        Bonus2 = 19,
                        Bonus3 = 20,
                        Bonus4 = 3,
                        Bonus5 = 3,
                        Bonus1Type = 10,
                        Bonus2Type = 1,
                        Bonus3Type = 148,
                        Bonus4Type = 155,
                        Bonus5Type = 200,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 32116
                    };
                }

                return m_goldenTridentofFlame;
            }
        }

        private ItemTemplate JavelinM
        {
            get
            {
                m_goldenTridentofFlame = GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_FlameM");
                if (m_goldenTridentofFlame == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Golden_Trident_of_Flame, loading it ...");
                    }

                    m_goldenTridentofFlame = new ItemTemplate
                    {
                        Id_nb = "Golden_Trident_of_FlameM",
                        Name = "Golden Triden of Flame",
                        Level = 45,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 36,
                        Type_Damage = 2,
                        Object_Type = 11,
                        Item_Type = 10,
                        Color = 48,
                        Effect = 77,
                        Model = 1004,
                        Bonus = 35,
                        Bonus1 = 60,
                        Bonus2 = 19,
                        Bonus3 = 20,
                        Bonus4 = 3,
                        Bonus5 = 3,
                        Bonus1Type = 10,
                        Bonus2Type = 1,
                        Bonus3Type = 148,
                        Bonus4Type = 155,
                        Bonus5Type = 200,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 32116
                    };
                }

                return m_goldenTridentofFlame;
            }
        }

        private ItemTemplate JavelinH
        {
            get
            {
                m_goldenTridentofFlame = GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Trident_of_FlameH");
                if (m_goldenTridentofFlame == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Golden_Trident_of_Flame, loading it ...");
                    }

                    m_goldenTridentofFlame = new ItemTemplate
                    {
                        Id_nb = "Golden_Trident_of_FlameH",
                        Name = "Golden Triden of Flame",
                        Level = 45,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 36,
                        Type_Damage = 2,
                        Object_Type = 19,
                        Item_Type = 10,
                        Color = 48,
                        Effect = 77,
                        Model = 1004,
                        Bonus = 35,
                        Bonus1 = 60,
                        Bonus2 = 19,
                        Bonus3 = 20,
                        Bonus4 = 3,
                        Bonus5 = 3,
                        Bonus1Type = 10,
                        Bonus2Type = 1,
                        Bonus3Type = 148,
                        Bonus4Type = 155,
                        Bonus5Type = 200,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        ProcSpellID = 32116
                    };
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
            {
                return;
            }

            GamePlayer player = sender as GamePlayer;
            lock (player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {
                    if (invItem.Id_nb.Equals("Golden_Trident_of_Flame"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Golden_Trident_of_FlameM"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Golden_Trident_of_FlameH"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }
                }
            }

            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}