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
 *///made by DeMAN
using System;
using System.Reflection;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using log4net;


namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("WaterBreathing")]
	public class WaterBreathingSpellHandler : SpellHandler
	{
		public WaterBreathingSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override void OnEffectStart(GameSpellEffect effect)
		{

			GamePlayer player = effect.Owner as GamePlayer;
			if (player != null)
			{
				player.CanBreathUnderWater = true;
				player.BuffBonusCategory1[(int)eProperty.WaterSpeed] += (int)Spell.Value;
				player.Out.SendUpdateMaxSpeed();
			}

			eChatType toLiving = (Spell.Pulse == 0) ? eChatType.CT_Spell : eChatType.CT_SpellPulse;
			eChatType toOther = (Spell.Pulse == 0) ? eChatType.CT_System : eChatType.CT_SpellPulse;
			if (Spell.Message2 != "")
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, false)), toOther, effect.Owner);
			MessageToLiving(effect.Owner, Spell.Message1 == "" ? "You find yourself able to move freely and breathe water like air!" : Spell.Message1, toLiving);
			base.OnEffectStart(effect);
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GamePlayer player = effect.Owner as GamePlayer;
			if (player != null)
			{
				player.CanBreathUnderWater = false;
				player.BuffBonusCategory1[(int)eProperty.WaterSpeed] -= (int)Spell.Value;
				player.Out.SendUpdateMaxSpeed();
				if (player.IsDiving)
					MessageToLiving(effect.Owner, "With a gulp and a gasp you realize that you are unable to breathe underwater any longer!", eChatType.CT_SpellExpires);
			}
			return base.OnEffectExpires(effect, noMessages);
		}
	}
}
