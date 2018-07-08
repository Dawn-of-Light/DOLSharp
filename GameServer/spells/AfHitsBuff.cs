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
using DOL.Database;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{

    [SpellHandler("AfHitsBuff")]
    public class AfHitsBuffSpellHandler : SpellHandler
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);

            double playerAF = 0;
            double bonusAF;
            double bonusHP;

            if (effect?.Owner == null)
            {
                effect.Cancel(false);
                return;
            }

            foreach (InventoryItem item in effect.Owner.Inventory.EquippedItems)
            {
                if (item.Object_Type >= (int)eObjectType._FirstArmor && item.Object_Type <= (int)eObjectType._LastArmor)
                {
                    playerAF += item.DPS_AF;
                }
            }

            playerAF += effect.Owner.GetModifiedFromItems(eProperty.ArmorFactor);

            if (Spell.Value < 0)
            {
                bonusAF = Spell.Value * -1 * playerAF / 100;
                bonusHP = Spell.Value * -1 * effect.Owner.MaxHealth / 100;
            }
            else
            {
                bonusAF = Spell.Value;
                bonusHP = Spell.Value;
            }

            GameLiving living = effect.Owner;
            living.TempProperties.setProperty("BONUS_HP", bonusHP);
            living.TempProperties.setProperty("BONUS_AF", bonusAF);
            living.AbilityBonus[(int)eProperty.MaxHealth] += (int)bonusHP;
            living.ItemBonus[(int)eProperty.ArmorFactor] += (int)bonusAF;

            SendUpdates(effect.Owner);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            base.OnEffectExpires(effect, noMessages);

            GameLiving living = effect.Owner;
            double bonusAF = living.TempProperties.getProperty<double>("BONUS_AF");
            double bonusHP = living.TempProperties.getProperty<double>("BONUS_HP");

            living.ItemBonus[(int)eProperty.ArmorFactor] -= (int)bonusAF;
            living.AbilityBonus[(int)eProperty.MaxHealth] -= (int)bonusHP;

            living.TempProperties.removeProperty("BONUS_AF");
            living.TempProperties.removeProperty("BONUS_HP");

            SendUpdates(effect.Owner);
            return 0;
        }

        public void SendUpdates(GameLiving target)
        {
            if (target is GamePlayer player)
            {
                player.Out.SendUpdatePlayer();
                player.Out.SendCharStatsUpdate();
                player.Out.SendUpdateWeaponAndArmorStats();
                player.UpdatePlayerStatus();
            }
        }

        public AfHitsBuffSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }
    }
}