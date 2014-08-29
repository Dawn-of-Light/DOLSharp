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
 
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Summary description for TurretPBAoESpellHandler.
	/// </summary>
	[SpellHandler("TurretPBAoE")]
	public class PetPBAoEDamageHandler : DirectDamageSpellHandler
	{
		public PetPBAoEDamageHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
		}

		public override bool HasPositiveEffect {
			get { return false; }
		}
		
		/// <summary>
		/// Source for LoS Check Should be Pet
		/// </summary>
		protected override GameObject SourceCheckLoS 
		{
			get 
			{
				if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
					return Caster.ControlledBrain.Body;
				
				return base.SourceCheckLoS; 
			}
		}
		
		/// <summary>
		/// Check if we have a main pet for casting.
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <param name="quiet"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget, bool quiet)
		{
			if (!base.CheckBeginCast(selectedTarget, quiet))
				return false;
			
			// Need Main Turret under our control before casting.
			if (Caster.ControlledBrain == null || Caster.ControlledBrain.Body == null)
			{
				if(!quiet && Caster is GamePlayer)
					MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "PetPBAOE.CheckBeginCast.NoPet"), eChatType.CT_System);
                return false;
			}
			
			return true;
		}

		/// <summary>
		/// Target Selected From Pet PBAE
		/// </summary>
		/// <param name="castTarget"></param>
		/// <returns></returns>
		public override IList<GameLiving> SelectTargets(GameObject castTarget)
		{
			// Change Target for Selecting.
			if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
				return base.SelectTargets(Caster.ControlledBrain.Body);
			
			return new List<GameLiving>(1);
		}
		
		/// <summary>
		/// Damage Dealer Should be the Pet.
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="showEffectAnimation"></param>
		/// <param name="attackResult"></param>
		public override void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult)
		{
			// Change Damage Dealer to Pet before sending damage (for brains)
			if (Caster.ControlledBrain != null && Caster.ControlledBrain.Body != null)
				ad.Attacker = Caster.ControlledBrain.Body;
			
			base.DamageTarget(ad, showEffectAnimation, attackResult);
		}
	}
}