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
    using System.Collections;
    using Database;
    using Events;

    /// <summary>
    /// NOTE: PLEASE CHECK YOUR SPELL ID FOR JAVELIN OR CREATE YOUR OWN ITEM
    /// </summary>
    [SpellHandler("GoldenSpearJavelin")]
    public class GoldenSpearJavelin : SummonItemSpellHandler
    {
        private ItemTemplate _artefJavelin;

        public GoldenSpearJavelin(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            _artefJavelin = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Artef_Javelin'") ?? Javelin;
            item = new InventoryItem(_artefJavelin);
        }

        private ItemTemplate Javelin
        {
            get
            {
                _artefJavelin = (ItemTemplate) GameServer.Database.FindObjectByKey<ItemTemplate>("Artef_Javelin");
                if(_artefJavelin == null)
                {
                    if(log.IsWarnEnabled) log.Warn("Could not find Artef_Javelin, loading it ...");
                    _artefJavelin = new ItemTemplate();
                    _artefJavelin.Id_nb = "Artef_Javelin";
                    _artefJavelin.Name = "Golden Javelin";
                    _artefJavelin.Level = 50;
                    _artefJavelin.MaxDurability = 50000;
                    _artefJavelin.MaxCondition = 50000;
                    _artefJavelin.Quality = 100;
                    _artefJavelin.Object_Type = (int) eObjectType.Magical;
                    _artefJavelin.Item_Type = 41;
                    _artefJavelin.Model = 23;
                    _artefJavelin.IsPickable = false;
                    _artefJavelin.IsDropable = false;
                    _artefJavelin.CanDropAsLoot = false;
                    _artefJavelin.IsTradable = false;
                    _artefJavelin.MaxCount = 1;
                    _artefJavelin.PackSize = 1;
                    _artefJavelin.Charges = 5;
                    _artefJavelin.MaxCharges = 5;
                    _artefJavelin.SpellID = 38076;
                }
                return _artefJavelin;
            }
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
        }

        private static void OnPlayerLeft(DOLEvent e,object sender,EventArgs arguments)
        {
            if(!(sender is GamePlayer)) return;
            GamePlayer player = sender as GamePlayer;
            lock(player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                foreach(InventoryItem invItem in items)
                {
                    if(invItem.Id_nb.Equals("Artef_Javelin"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }
                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}