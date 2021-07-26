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
using DOL.GS.Effects;
using DOL.Database;

namespace DOL.GS.Spells
{
	[SpellHandler("SpeedOfTheRealm")]
	public class SpeedOfTheRealmHandler : SpeedEnhancementSpellHandler
	{
		private const ushort SECONDEFFECT = 2086;

		public override void FinishSpellCast(GameLiving target)
		{
			base.FinishSpellCast(target);
			foreach (GamePlayer player in Caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendSpellCastAnimation(Caster, SECONDEFFECT, 20);
		}

		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			return Spell.Duration;
		}

        public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
        {
            PlayerXEffect eff = new PlayerXEffect();
            eff.Var1 = Spell.ID;
            eff.Duration = e.RemainingTime;
            eff.IsHandler = true;
            eff.SpellLine = SpellLine.KeyName;
            return eff;
        }

        public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			OnEffectStart(effect);
		}

		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			return OnEffectExpires(effect, false);
		}		

		public SpeedOfTheRealmHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
