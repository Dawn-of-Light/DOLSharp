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
using System.Collections.Generic;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    [SpellHandler("VampiirArmorDebuff")]
    public class VampiirArmorDebuff : SpellHandler
    {
        private static eArmorSlot[] slots = {eArmorSlot.HEAD, eArmorSlot.TORSO, eArmorSlot.LEGS};

        public eArmorSlot Slot { get; private set; } = eArmorSlot.NOTSET;

        private int old_item_af;
        private int old_item_abs;
        private InventoryItem item;
        protected GamePlayer player;

        public override void FinishSpellCast(GameLiving target)
        {
            Caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
            player = effect.Owner as GamePlayer;
            if (player == null)
            {
                return;
            }

            int slot = Util.Random(0, 2);
            Slot = slots[slot];
            string msg = GlobalConstants.SlotToName((int)Slot);
            MessageToCaster($"You debuff {effect.Owner.Name}\'s {msg}{string.Empty}", eChatType.CT_Spell);
            foreach (GamePlayer visPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                visPlayer.Out.SendSpellEffectAnimation(player, player, (ushort)(13180 + slot), 0, false, 0x01);
            }

            item = player.Inventory.GetItem((eInventorySlot)Slot);

            if (item != null)
            {
                old_item_af = item.DPS_AF;
                old_item_abs = item.SPD_ABS;
                item.DPS_AF -= (int)Spell.Value;
                item.SPD_ABS -= Spell.ResurrectMana;
                if (item.DPS_AF < 0)
                {
                    item.DPS_AF = 0;
                }

                if (item.SPD_ABS < 0)
                {
                    item.SPD_ABS = 0;
                }

                player.Client.Out.SendInventoryItemsUpdate(new[] { item });
                player.Out.SendCharStatsUpdate();
                player.UpdatePlayerStatus();
                player.Out.SendUpdatePlayer();
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendCharResistsUpdate();

                GameEventMgr.AddHandler(player,GamePlayerEvent.Linkdeath, new DOLEventHandler(EventAction));
                GameEventMgr.AddHandler(player,GamePlayerEvent.Quit, new DOLEventHandler(EventAction));
                GameEventMgr.AddHandler(player,GamePlayerEvent.RegionChanged, new DOLEventHandler(EventAction));
                GameEventMgr.AddHandler(player,GameLivingEvent.Dying, new DOLEventHandler(EventAction));
            }

            base.OnEffectStart(effect);
        }

        public void EventAction(DOLEvent e, object sender, EventArgs arguments)
        {
            if (player == null)
            {
                return;
            }

            RemoveEffect();
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            RemoveEffect();
            return base.OnEffectExpires(effect,noMessages);
        }

        public void RemoveEffect()
        {
            if (player == null)
            {
                return;
            }

            GameSpellEffect effect = FindEffectOnTarget(player,this);
            effect?.Cancel(false);

            if (item == null)
            {
                return;
            }

            item.DPS_AF = old_item_af;
            item.SPD_ABS = old_item_abs;

            player.Client.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
            player.Out.SendCharStatsUpdate();
            player.UpdatePlayerStatus();
            player.Out.SendUpdatePlayer();
            player.Out.SendUpdateWeaponAndArmorStats();
            player.Out.SendCharResistsUpdate();

            GameEventMgr.RemoveHandler(player,GamePlayerEvent.Linkdeath, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(player,GamePlayerEvent.Quit, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(player,GamePlayerEvent.RegionChanged, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(player,GameLivingEvent.Dying, new DOLEventHandler(EventAction));
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>(16);
                list.Add($"Name: {Spell.Name}\n");
                list.Add($"Description: {Spell.Description}\n");
                list.Add($"Target: {Spell.Target}");
                list.Add($"Casting time: {Spell.CastTime * 0.001:0.0## sec;-0.0## sec;'instant'}");
                if (Spell.Duration >= ushort.MaxValue * 1000)
                {
                    list.Add("Duration: Permanent.");
                }
                else if (Spell.Duration > 60000)
                {
                    list.Add($"Duration: {Spell.Duration / 60000}:{(Spell.Duration % 60000 / 1000):00} min");
                }
                else if (Spell.Duration != 0)
                {
                    list.Add($"Duration: {Spell.Duration / 1000:0' sec';'Permanent.';'Permanent.'}");
                }

                if (Spell.RecastDelay > 60000)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 60000}:{Spell.RecastDelay % 60000 / 1000:00} min");
                }
                else if (Spell.RecastDelay > 0)
                {
                    list.Add($"Recast time: {Spell.RecastDelay / 1000} sec");
                }

                if (Spell.Range != 0)
                {
                    list.Add($"Range: {Spell.Range}");
                }

                if (Spell.Power != 0)
                {
                    list.Add($"Power cost: {Spell.Power:0;0'%'}");
                }

                list.Add($"Debuff Absorption : {Spell.ResurrectMana}");
                list.Add($"Debuff Armor Factor : {Spell.Value}");
                return list;
            }
        }

        public VampiirArmorDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}