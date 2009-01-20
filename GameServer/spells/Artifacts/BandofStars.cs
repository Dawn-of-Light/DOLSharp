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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.SkillHandler;

using DOL.Events;
using DOL.GS;


namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("StarsProc")]
	public class StarsProc : SpellHandler
	{		
		public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            return base.CheckBeginCast(selectedTarget);
        }
		public override void StartSpell(GameLiving target)
		{
			foreach (GameLiving targ in SelectTargets(target))
			{
				DealDamage(targ);
			}
		}
		
		private void DealDamage(GameLiving target)
		{
			int ticksToTarget = WorldMgr.GetDistance(m_caster, target) * 100 / 85; // 85 units per 1/10s
			int delay = 1 + ticksToTarget / 100;
			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, (ushort)(delay), false, 1);
			}
			BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target, this);
			bolt.Start(1 + ticksToTarget);
		}
		
		public override void FinishSpellCast(GameLiving target)
		{
			if (target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent)
			{
				MessageToCaster("Your spell has no effect on the keep component!", eChatType.CT_SpellResisted);
				return;
			}
			base.FinishSpellCast(target);
		}
		
		protected class BoltOnTargetAction : RegionAction
		{
			protected readonly GameLiving m_boltTarget;
			protected readonly StarsProc m_handler;
			
			public BoltOnTargetAction(GameLiving actionSource, GameLiving boltTarget, StarsProc spellHandler) : base(actionSource)
			{
				if (boltTarget == null)
					throw new ArgumentNullException("boltTarget");
				if (spellHandler == null)
					throw new ArgumentNullException("spellHandler");
				m_boltTarget = boltTarget;
				m_handler = spellHandler;
			}

			protected override void OnTick()
			{
				GameLiving target = m_boltTarget;
				GameLiving caster = (GameLiving)m_actionSource;
				if (target == null) return;
				if (target.CurrentRegion.ID != caster.CurrentRegion.ID) return;
				if (target.ObjectState != GameObject.eObjectState.Active) return;
				if (!target.IsAlive) return;
				if (target == null) return;
				if (!target.IsAlive || target.ObjectState!=GameLiving.eObjectState.Active) return;
				
				AttackData ad = m_handler.CalculateDamageToTarget(target, 1);
				ad.Damage = (int)m_handler.Spell.Damage;
				m_handler.SendDamageMessages(ad);
				m_handler.DamageTarget(ad, false);
				
				//if (m_handler.Spell.SubSpellID != 0) Spell subspell = m_handler.SkillBase.GetSpellByID(m_handler.Spell.SubSpellID);
                if (m_handler.Spell.SubSpellID != 0 && SkillBase.GetSpellByID(m_handler.Spell.SubSpellID) != null)
				{
					ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(caster, SkillBase.GetSpellByID(m_handler.Spell.SubSpellID), SkillBase.GetSpellLine("Mob Spells"));
					spellhandler.StartSpell(target);
				}
					
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, caster);
			}
		}

		public StarsProc(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
	
	[SpellHandlerAttribute("StarsProc2")]
    public class StarsProc2 : SpellHandler
    {
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}
        public override void OnEffectStart(GameSpellEffect effect)
        {    
     		base.OnEffectStart(effect);            
            effect.Owner.DebuffCategory[(int)eProperty.Dexterity] += (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Strength] += (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Constitution] += (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Acuity] += (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Piety] += (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Empathy] += (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Quickness] += (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Intelligence] += (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Charisma] += (int)m_spell.Value;   
            effect.Owner.DebuffCategory[(int)eProperty.ArmorAbsorbtion] += (int)m_spell.Value; 
            effect.Owner.DebuffCategory[(int)eProperty.MagicAbsorbtion] += (int)m_spell.Value; 
            
            if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;  
				if(m_spell.LifeDrainReturn>0) if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer) player.Model=(ushort)m_spell.LifeDrainReturn;
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();             	
            }
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {  
            effect.Owner.DebuffCategory[(int)eProperty.Dexterity] -= (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Strength] -= (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Constitution] -= (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Acuity] -= (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Piety] -= (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Empathy] -= (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Quickness] -= (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Intelligence] -= (int)m_spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Charisma] -= (int)m_spell.Value;        
            effect.Owner.DebuffCategory[(int)eProperty.ArmorAbsorbtion] -= (int)m_spell.Value; 
            effect.Owner.DebuffCategory[(int)eProperty.MagicAbsorbtion] -= (int)m_spell.Value; 
 
            if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;  
				if(player.CharacterClass.ID!=(byte)eCharacterClass.Necromancer) player.Model=(ushort)player.Client.Account.Characters[player.Client.ActiveCharIndex].CreationModel;
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();  
            }                       
            return base.OnEffectExpires(effect, noMessages);
        }

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			if (target.Realm == 0 || Caster.Realm == 0)
			{
				target.LastAttackedByEnemyTickPvE = target.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}
			if(target is GameNPC) 
			{
				IAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
			}
		}		
        public StarsProc2(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
 
}
