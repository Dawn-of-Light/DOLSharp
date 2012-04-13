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
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    #region PrimerSpell
    public class PrimerSpellHandler : SpellHandler
	{
        public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, Spell.Duration, 0, effectiveness);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{			
			GameEventMgr.AddHandler(effect.Owner, GamePlayerEvent.Moving, new DOLEventHandler(OnMove));
			SendEffectAnimation(effect.Owner, 0, false, 1);			
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if(effect.Owner is GamePlayer && !noMessages)
				((GamePlayer)effect.Owner).Out.SendMessage("You modification spell effect has expired.", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
			GameEventMgr.RemoveHandler(effect.Owner, GamePlayerEvent.Moving, new DOLEventHandler(OnMove));
			return base.OnEffectExpires (effect, false);
		}

		private void OnMove(DOLEvent e, object sender, EventArgs arguments)
		{
            GameLiving living = sender as GameLiving;
            if (living == null) return;
            if (living.IsMoving)
            {
                GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
                if (effect != null)
                {
                    effect.Cancel(false);
                    ((GamePlayer)living).Out.SendMessage("You move and break your modification spell.", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
            }            
		}

        public PrimerSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
    }
    #endregion PrimerSpell

    #region Powerless
    [SpellHandlerAttribute("Powerless")]
    public class PowerlessSpellHandler : PrimerSpellHandler
    {
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (!base.CheckBeginCast(selectedTarget))
                return false;

            bool powerless = Caster.TempProperties.getProperty("Warlock-Powerless", false);
            bool range = Caster.TempProperties.getProperty("Warlock-Range", false);
            bool uninterruptable = Caster.TempProperties.getProperty("Warlock-Uninterruptable", false);

            if (range)
            {
                MessageToCaster("You already preparing a Range spell", eChatType.CT_System);
                return false;
            }
            if (uninterruptable)
            {
                MessageToCaster("You already preparing a Uninterruptable spell", eChatType.CT_System);
                return false;
            }
            if (powerless)
            {
                MessageToCaster("You already preparing this effect", eChatType.CT_System);
                return false;
            }

            Caster.TempProperties.setProperty("Warlock-CastingPowerless", true);
            return true;
        }

        public override bool CheckEndCast(GameLiving target)
        {
            Caster.TempProperties.setProperty("Warlock-CastingPowerless", false);
            return base.CheckEndCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            Caster.TempProperties.setProperty("Warlock-Powerless", true);
            base.OnEffectStart(effect);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            Caster.TempProperties.setProperty("Warlock-Powerless", false);
            return base.OnEffectExpires(effect, noMessages);
        }

        public PowerlessSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion Powerless

    #region Range
    [SpellHandlerAttribute("Range")]
    public class RangeSpellHandler : PrimerSpellHandler
    {
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (!base.CheckBeginCast(selectedTarget))
                return false;

            bool powerless = Caster.TempProperties.getProperty("Warlock-Powerless", false);
            bool range = Caster.TempProperties.getProperty("Warlock-Range", false);
            bool uninterruptable = Caster.TempProperties.getProperty("Warlock-Uninterruptable", false);

            if (powerless)
            {
                MessageToCaster("You already preparing a Powerless spell", eChatType.CT_System);
                return false;
            }
            if (uninterruptable)
            {
                MessageToCaster("You already preparing a Uninterruptable spell", eChatType.CT_System);
                return false;
            }
            if (range)
            {
                MessageToCaster("You already preparing this effect", eChatType.CT_System);
                return false;
            }

            Caster.TempProperties.setProperty("Warlock-CastingRange", true);
            Caster.TempProperties.setProperty("Warlock-RangeValue", (int)Spell.Range);
            return true;
        }

        public override bool CheckEndCast(GameLiving target)
        {
            Caster.TempProperties.setProperty("Warlock-CastingRange", false);
            return base.CheckEndCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            Caster.TempProperties.setProperty("Warlock-Range", true);
            Caster.TempProperties.setProperty("Warlock-RangeValue", (int)Spell.Range);
            base.OnEffectStart(effect);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            Caster.TempProperties.setProperty("Warlock-Range", false);
            Caster.TempProperties.setProperty("Warlock-RangeValue", 0);
            return base.OnEffectExpires(effect, noMessages);
        }

        public RangeSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion Range

    #region Uninterruptable
    [SpellHandlerAttribute("Uninterruptable")]
    public class UninterruptableSpellHandler : PrimerSpellHandler
    {
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (!base.CheckBeginCast(selectedTarget))
                return false;

            bool powerless = Caster.TempProperties.getProperty("Warlock-Powerless", false);
            bool range = Caster.TempProperties.getProperty("Warlock-Range", false);
            bool uninterruptable = Caster.TempProperties.getProperty("Warlock-Uninterruptable", false);

            if (powerless)
            {
                MessageToCaster("You already preparing a Powerless spell", eChatType.CT_System);
                return false;
            }
            if (range)
            {
                MessageToCaster("You already preparing a Range spell", eChatType.CT_System);
                return false;
            }
            if (uninterruptable)
            {
                MessageToCaster("You already preparing this effect", eChatType.CT_System);
                return false;
            }

            Caster.TempProperties.setProperty("Warlock-CastingUninterruptable", true);
            Caster.TempProperties.setProperty("Warlock-UninterruptableValue", (double)Spell.Value);
            return true;
        }

        public override bool CheckEndCast(GameLiving target)
        {
            Caster.TempProperties.setProperty("Warlock-CastingUninterruptable", false);
            return base.CheckEndCast(target);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            Caster.TempProperties.setProperty("Warlock-Uninterruptable", true);
            base.OnEffectStart(effect);
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            Caster.TempProperties.setProperty("Warlock-Uninterruptable", false);
            return base.OnEffectExpires(effect, noMessages);
        }

        public UninterruptableSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion Uninterruptable
}
