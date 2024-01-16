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
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.Database;
using DOL.GS.Styles;
using DOL.GS.Geometry;

namespace DOL.GS.Spells
{
    
   [SpellHandler("AstralPetSummon")]
    public class AstralPetSummon : SummonSpellHandler
    {
    	//Graveen: Not implemented property - can be interesting
        /* 
        public bool Controllable
        {
            get { return false; }
        }*/

        /// <summary>
        /// Summon the pet.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);

            m_pet.TempProperties.setProperty("target", target);
            (m_pet.Brain as IOldAggressiveBrain).AddToAggroList(target, 1);
            (m_pet.Brain as ProcPetBrain).Think();

        }

        protected override GamePet GetGamePet(INpcTemplate template) { return new AstralPet(template); }
        protected override IControlledBrain GetPetBrain(GameLiving owner) { return new ProcPetBrain(owner); }
        protected override void SetBrainToOwner(IControlledBrain brain) {}

        protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GameNPC) || !((sender as GameNPC).Brain is IControlledBrain))
                return;
            GameNPC pet = sender as GameNPC;
            IControlledBrain brain = pet.Brain as IControlledBrain;

            GameEventMgr.RemoveHandler(pet, GameLivingEvent.PetReleased, new DOLEventHandler(OnNpcReleaseCommand));

            DOL.GS.Effects.GameSpellEffect effect = FindEffectOnTarget(pet, this);
            if (effect != null)
                effect.Cancel(false);
        }

        protected override Position GetSummonPosition()
            => Caster.Position + Vector.Create(Caster.Orientation, length: 64);

         public AstralPetSummon(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }
    }
}

namespace DOL.GS
{
    public class AstralPet : GamePet
    {
        public override int MaxHealth
        {
            get { return Level * 10; }
        }

        public override void OnAttackedByEnemy(AttackData ad) { }
        public AstralPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
    }
}