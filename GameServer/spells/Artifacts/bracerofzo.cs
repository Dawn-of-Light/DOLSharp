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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Zo' Arkat summoning
    /// </summary>
    [SpellHandlerAttribute("ZoSummon")]
    public class BracerOfZo : SpellHandler
    {
        private static GameNPC[] deamons = new GameNPC[4];
        private static ControlledNpc[] brains = new ControlledNpc[4]; 
        
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
 
            INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
            if (template == null)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
                MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
                return;
            }
            int x, y;
            int i = 0;
            Caster.GetSpotFromHeading(64, out x, out y);
            for(i=0;i<4;i++)
            {               
                brains[i] = new ControlledNpc(Caster);
                brains[i].WalkState = eWalkState.Stay;
                deamons[i] = new GameNPC(template);
                deamons[i].SetOwnBrain(brains[i]);
                deamons[i].X = x + Util.Random(20,40) - Util.Random(20,40);
                deamons[i].Y = y + Util.Random(20,40) - Util.Random(20,40);
                deamons[i].Z = Caster.Z;
                deamons[i].CurrentRegion = Caster.CurrentRegion;
                deamons[i].Heading = (ushort)((Caster.Heading + 2048) % 4096);
                deamons[i].Realm = Caster.Realm;
                deamons[i].CurrentSpeed = 0;
                deamons[i].Level = 40;
                deamons[i].AddToWorld();
                deamons[i].TargetObject = Caster.TargetObject;
                brains[i].Attack(Caster.TargetObject);
            }			
		}
         public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
        	int i = 0;
            for(i=0;i<4;i++)
            {
            	if(deamons[i]!=null)
            	{
            		deamons[i].Health = 0;
            		deamons[i].Delete();
            	}
            }
            return base.OnEffectExpires(effect,noMessages);
        }
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }
        public BracerOfZo(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    
    [SpellHandlerAttribute("Bedazzlement")]
    public class ZoDebuffSpellHandler : DualStatDebuff
    {
		public override eProperty Property1 { get { return eProperty.FumbleChance; } }
		public override eProperty Property2 { get { return eProperty.SpellFumbleChance; } }

 		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
		}
		
        public ZoDebuffSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
