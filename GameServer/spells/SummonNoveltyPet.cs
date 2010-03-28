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

// Original code by Dinberg

using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.GS.Spells;
using DOL.AI.Brain;


namespace DOL.GS.Spells
{
	/// <summary>
    /// This pet is purely aesthetic and can't be cast in RvR zones
    /// </summary>
    [SpellHandler("SummonNoveltyPet")]
    public class SummonNoveltyPet : SummonSpellHandler
    {
        /// <summary>
        /// Constructs the spell handler
        /// </summary>
		public SummonNoveltyPet(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);

			if (pet != null)
			{
				pet.Flags |= (uint)GameNPC.eFlags.PEACE; //must be peace!

				//No brain for now, so just follow owner.
				pet.Follow(Caster, 100, WorldMgr.VISIBILITY_DISTANCE);

				Caster.TempProperties.setProperty(NoveltyPetBrain.HAS_PET, true);
			}
                        
        }

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster.CurrentRegion.IsRvR)
            {
                MessageToCaster("You cannot cast this spell in an rvr zone!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted);
                return false;
            }

			if (Caster.TempProperties.getProperty<bool>(NoveltyPetBrain.HAS_PET, false))
			{
				// no message
				return false;
			}

            return base.CheckBeginCast(selectedTarget);
        }

        /// <summary>
        /// These pets aren't controllable!
        /// </summary>
        /// <param name="brain"></param>
        protected override void SetBrainToOwner(IControlledBrain brain)
        {
        }

        protected override IControlledBrain GetPetBrain(GameLiving owner)
        {
            return new NoveltyPetBrain(owner as GamePlayer);
        }

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add(string.Format("  {0}", Spell.Description));

				return list;
			}
		}
    }
}
