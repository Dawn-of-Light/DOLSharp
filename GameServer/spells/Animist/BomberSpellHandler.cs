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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("Bomber")]
	public class BomberSpellHandler : SummonSpellHandler
	{
		const string BOMBERTARGET = "bombertarget";

		public BomberSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (Spell.SubSpellID == 0)
			{
				MessageToCaster("SPELL NOT IMPLEMENTED: CONTACT GM", eChatType.CT_Important);
				return false;
			}

			return base.CheckBeginCast(selectedTarget);
		}

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			pet.TempProperties.setProperty(BOMBERTARGET, Caster.TargetObject);
			pet.Follow(target, 10, Spell.Range * 5); // with Toa bonus, if the bomber was fired > Spell.Range base, it didnt move..
		}

		protected override void AddHandlers()
		{
			GameEventMgr.AddHandler(pet, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(BomberArriveAtTarget));
		}

		protected override void RemoveHandlers()
		{
			GameEventMgr.RemoveHandler(pet, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(BomberArriveAtTarget));
		}

		protected override byte GetPetLevel()
		{
			return Caster.Level;
		}

		protected override IControlledBrain GetPetBrain(GameLiving owner)
		{
			return new BomberBrain(owner);
		}

		protected override void SetBrainToOwner(IControlledBrain brain)
		{
		}

		protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
		}

		/// <summary>
		/// Called when the Bomber reaches his target
		/// </summary>
		private void BomberArriveAtTarget(DOLEvent e, object sender, EventArgs args)
		{
			GameNPC bomber = sender as GameNPC;

			if (bomber != pet)
				return;

			GameLiving living = pet.TempProperties.getObjectProperty(BOMBERTARGET, null) as GameLiving;

			bomber.Health = 0;
			bomber.Delete();

			if (living != null)
			{
				Spell subspell = SkillBase.GetSpellByID(m_spell.SubSpellID);
				if (subspell != null)
				{
					//Andraste
					subspell.Level = m_spell.Level;
					if (WorldMgr.CheckDistance(living, bomber, 500))
					{
						ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, subspell, SkillBase.GetSpellLine(this.SpellLine.KeyName));
						spellhandler.StartSpell(living);
					}
				}
				else
				{
					if (log.IsErrorEnabled)
						log.Error("Bomber Subspell: " + subspell.ID + " is not implemented yet");
					return;
				}
			}
		}
	}
}