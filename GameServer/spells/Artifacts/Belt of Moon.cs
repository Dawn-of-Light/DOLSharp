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
    using PacketHandler;

    [SpellHandler("BeltOfMoon")]
    public class BeltOfMoon : SummonItemSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ItemTemplate m_MoonMace;
        private ItemTemplate m_MoonStaff;

        public BeltOfMoon(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            if (caster.CurrentRegion.IsNightTime)
            {
                if (caster.Realm == eRealm.Albion)
                {
                    m_MoonMace = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_Mace") ?? Mace;
                    items.Add(GameInventoryItem.Create(m_MoonMace));

                    m_MoonStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_Staff") ?? Staff;
                    items.Add(GameInventoryItem.Create(m_MoonStaff));
                    return;
                }

                if (caster.Realm == eRealm.Midgard)
                {
                    m_MoonMace = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_MaceM") ?? MaceM;
                    items.Add(GameInventoryItem.Create(m_MoonMace));

                    m_MoonStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_Staff") ?? Staff;
                    items.Add(GameInventoryItem.Create(m_MoonStaff));
                    return;
                }

                if (caster.Realm == eRealm.Hibernia)
                {
                    m_MoonMace = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_MaceH") ?? MaceH;
                    items.Add(GameInventoryItem.Create(m_MoonMace));

                    m_MoonStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_Staff") ?? Staff;
                    items.Add(GameInventoryItem.Create(m_MoonStaff));
                    return;
                }
            }

            else
            {
                MessageToCaster("The powers of the Belt of Moon, can only be Summon under the Moon light!", eChatType.CT_SpellResisted);
            }
        }

        private ItemTemplate Mace
        {
            get
            {
                m_MoonMace = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_Mace");
                if (m_MoonMace == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Moon_Mace, loading it ...");
                    }

                    m_MoonMace = new ItemTemplate
                    {
                        Id_nb = "Moon_Mace",
                        Name = "Moon Mace",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 2,
                        Item_Type = 10,
                        Color = 43,
                        Effect = 45,
                        Model = 647,
                        Bonus1 = 24,
                        Bonus2 = 3,
                        Bonus3 = 10,
                        Bonus4 = 4,
                        Bonus1Type = 156,
                        Bonus2Type = 163,
                        Bonus3Type = 196,
                        Bonus4Type = 191,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        SpellID = 65511,
                        Charges = 5,
                        MaxCharges = 5
                    };
                }

                return m_MoonMace;
            }
        }

        private ItemTemplate MaceM
        {
            get
            {
                m_MoonMace = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_Mace");
                if (m_MoonMace == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Moon_Mace, loading it ...");
                    }

                    m_MoonMace = new ItemTemplate
                    {
                        Id_nb = "Moon_Mace",
                        Name = "Moon Warhammer",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 12,
                        Item_Type = 10,
                        Color = 43,
                        Effect = 45,
                        Model = 320,
                        Bonus1 = 24,
                        Bonus2 = 3,
                        Bonus3 = 10,
                        Bonus4 = 4,
                        Bonus1Type = 156,
                        Bonus2Type = 163,
                        Bonus3Type = 196,
                        Bonus4Type = 191,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        SpellID = 65511,
                        Charges = 5,
                        MaxCharges = 5
                    };
                }

                return m_MoonMace;
            }
        }

        private ItemTemplate MaceH
        {
            get
            {
                m_MoonMace = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_Mace");
                if (m_MoonMace == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Moon_Mace, loading it ...");
                    }

                    m_MoonMace = new ItemTemplate
                    {
                        Id_nb = "Moon_Mace",
                        Name = "Moon Hammer",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 20,
                        Item_Type = 10,
                        Color = 43,
                        Effect = 45,
                        Model = 461,
                        Bonus1 = 24,
                        Bonus2 = 3,
                        Bonus3 = 10,
                        Bonus4 = 4,
                        Bonus1Type = 156,
                        Bonus2Type = 163,
                        Bonus3Type = 196,
                        Bonus4Type = 191,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        PackSize = 1,
                        SpellID = 65511,
                        Charges = 5,
                        MaxCharges = 5
                    };
                }

                return m_MoonMace;
            }
        }

        private ItemTemplate Staff
        {
            get
            {
                m_MoonStaff = GameServer.Database.FindObjectByKey<ItemTemplate>("Moon_Staff");
                if (m_MoonStaff == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("Could not find Moon_Staff, loading it ...");
                    }

                    m_MoonStaff = new ItemTemplate
                    {
                        Id_nb = "Moon_Staff",
                        Name = "Moon Staff",
                        Level = 50,
                        Durability = 50000,
                        MaxDurability = 50000,
                        Condition = 50000,
                        MaxCondition = 50000,
                        Quality = 100,
                        DPS_AF = 150,
                        SPD_ABS = 35,
                        Type_Damage = 0,
                        Object_Type = 8,
                        Item_Type = 12,
                        Hand = 1,
                        Color = 43,
                        Effect = 45,
                        Model = 566,
                        Bonus1 = 24,
                        Bonus2 = 3,
                        Bonus3 = 10,
                        Bonus4 = 4,
                        Bonus1Type = 156,
                        Bonus2Type = 163,
                        Bonus3Type = 196,
                        Bonus4Type = 191,
                        IsPickable = false,
                        IsDropable = false,
                        CanDropAsLoot = false,
                        IsTradable = false,
                        MaxCount = 1,
                        SpellID = 65511,
                        Charges = 5,
                        MaxCharges = 5
                    };
                }

                return m_MoonStaff;
            }
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Released, OnPlayerReleased);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
        }

        private static void OnPlayerReleased(DOLEvent e, object sender, EventArgs arguments)
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
                    if (player.CurrentRegion.IsNightTime)
                    {
                        return;
                    }

                    if (invItem.Id_nb.Equals("Moon_Mace"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Moon_MaceM"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Moon_MaceH"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Moon_Staff"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    player.Out.SendMessage("The Power of Belt of Moon, has left you!",eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
            }

            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Released, OnPlayerReleased);
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
                    if (invItem.Id_nb.Equals("Moon_Mace"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Moon_MaceM"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Moon_MaceH"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }

                    if (invItem.Id_nb.Equals("Moon_Staff"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }
                }
            }

            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}