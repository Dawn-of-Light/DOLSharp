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
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Spells
{
	[SpellHandler("TurretsRelease")]
	public class TurretsReleaseSpellHandler : SpellHandler
	{
		public TurretsReleaseSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
		{
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if(!(Caster is GamePlayer))
			{
				return false;
			}

			if(selectedTarget == null)
			{
                if (Caster is GamePlayer)
                    MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "TurretsRelease.CheckBeginCast.NoSelectedTarget"), eChatType.CT_SpellResisted);
                return false;
			}
			GameNPC target = selectedTarget as GameNPC;
			if(target == null || !(target.Brain is TurretBrain) || !Caster.IsControlledNPC(target))
			{
                if (Caster is GamePlayer)
                    MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "TurretsRelease.CheckBeginCast.NoSelectedTarget"), eChatType.CT_SpellResisted);
                return false;
			}
			if((target.Brain is TurretBrain) && !Caster.IsControlledNPC(target))
			{
                if (Caster is GamePlayer)
                    MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "TurretsRelease.CheckBeginCast.NoSelectedTarget"), eChatType.CT_SpellResisted);
                return false;
			}
            if ( !Caster.IsWithinRadius( target, Spell.Range ) )
			{
                if (Caster is GamePlayer)
                    MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "TurretsRelease.CheckBeginCast.TargetTooFarAway"), eChatType.CT_SpellResisted);
                return false;
			}
			return base.CheckBeginCast(selectedTarget);
		}

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (target != null && target.CurrentRegion != null)
			{
				foreach (GameNPC npc in target.CurrentRegion.GetNPCsInRadius(target.Coordinate, (ushort)Spell.Radius, false, true))
				{
					if (npc == null || !npc.IsAlive)
					{
						continue;
					}
					if (!(npc is TurretPet))
					{
						continue;
					}
					if (Caster.IsControlledNPC(npc))
					{
						//PetCounter is decremented when pet die.
						npc.Die(Caster);
					}
				}
			}
		}
	}
}