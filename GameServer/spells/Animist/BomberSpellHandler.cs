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
/*
 * [Ganrod] Nidel 2008-07-08
 * - Corrections for Bomber actions.
 */
using System;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("Bomber")]
    public class BomberSpellHandler : SummonSpellHandler
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		const string BOMBERTARGET = "bombertarget";

		public BomberSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { m_isSilent = true; }

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
            m_pet.Level = Caster.Level; // No bomber class to override SetPetLevel() in, so set level here
            m_pet.TempProperties.setProperty(BOMBERTARGET, target);
            m_pet.Name = Spell.Name;
			m_pet.Flags ^= GameNPC.eFlags.DONTSHOWNAME;
			m_pet.FixedSpeed = true;
            m_pet.Follow(target, 5, Spell.Range * 5); // with Toa bonus, if the bomber was fired > Spell.Range base, it didnt move..
        }

        protected override void AddHandlers()
        {
            GameEventMgr.AddHandler(m_pet, GameNPCEvent.ArriveAtTarget, BomberArriveAtTarget);
        }

        protected override void RemoveHandlers()
        {
            GameEventMgr.RemoveHandler(m_pet, GameNPCEvent.ArriveAtTarget, BomberArriveAtTarget);
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

            //[Ganrod] Nidel: Prevent NPE
            if (bomber == null || m_pet == null || bomber != m_pet)
                return;

            //[Ganrod] Nidel: Abort and delete bomber if Spell or Target is NULL
            Spell subspell = SkillBase.GetSpellByID(m_spell.SubSpellID);
            GameLiving living = m_pet.TempProperties.getProperty<object>(BOMBERTARGET, null) as GameLiving;

            if (subspell == null || living == null)
            {
                if (log.IsErrorEnabled && subspell == null)
                    log.Error("Bomber SubspellID for Bomber SpellID: " + m_spell.ID + " is not implemented yet");
                bomber.Health = 0;
                bomber.Delete();
                return;
            }

            //Andraste
            subspell.Level = m_spell.Level;
            if (living.IsWithinRadius(bomber, 350))
            {
				ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, subspell, SkillBase.GetSpellLine(SpellLine.KeyName));
                spellhandler.StartSpell(living);
            }

            //[Ganrod] Nidel: Delete Bomber after all actions.
            bomber.Health = 0;
            bomber.Delete();
        }
		
		/// <summary>
		/// Do not trigger SubSpells
		/// </summary>
		/// <param name="target"></param>
		public override void CastSubSpells(GameLiving target)
		{
		}
    }
}