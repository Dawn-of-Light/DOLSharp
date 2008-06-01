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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
    /// <summary>
    /// All stats debuff spell handler
    /// </summary>
    [SpellHandlerAttribute("AllStatsDebuff")]
    public class AllStatsDebuff : SpellHandler
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
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();  
            }                       
            return base.OnEffectExpires(effect, noMessages);
        }
        
		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
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
        public AllStatsDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
 
    /// <summary>
    /// Lore debuff spell handler (Magic resist debuff)
    /// </summary>
    [SpellHandlerAttribute("LoreDebuff")]
    public class LoreDebuff : SpellHandler
    {
 		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}
        public override void OnEffectStart(GameSpellEffect effect)
        {
        	base.OnEffectStart(effect);      
        	effect.Owner.DebuffCategory[(int)eProperty.SpellDamage] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Heat] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Cold] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Matter] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Spirit] += (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Energy] += (int)Spell.Value;
            
            if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
             	player.Out.SendCharResistsUpdate(); 
             	player.UpdatePlayerStatus();
            }                       
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.DebuffCategory[(int)eProperty.SpellDamage] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Heat] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Cold] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Matter] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Spirit] -= (int)Spell.Value;
            effect.Owner.DebuffCategory[(int)eProperty.Resist_Energy] -= (int)Spell.Value;
            
            if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
             	player.Out.SendCharResistsUpdate(); 
             	player.UpdatePlayerStatus();
            }           
            
            return base.OnEffectExpires(effect, noMessages);
        }
		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
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
        public LoreDebuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// Strength/Constitution drain spell handler
    /// </summary>
    [SpellHandlerAttribute("StrengthConstitutionDrain")]
    public class StrengthConstitutionDrain : StrengthConDebuff
    {  	
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}
        public override void OnEffectStart(GameSpellEffect effect)
        {
        	base.OnEffectStart(effect);         
            Caster.BuffBonusCategory1[(int)eProperty.Strength] += (int)m_spell.Value;
            Caster.BuffBonusCategory1[(int)eProperty.Constitution] += (int)m_spell.Value;
 
            if(Caster is GamePlayer)
            {
            	GamePlayer player = Caster as GamePlayer;          	
             	player.Out.SendCharStatsUpdate(); 
             	player.UpdateEncumberance();
             	player.UpdatePlayerStatus();
            } 
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {           
            Caster.BuffBonusCategory1[(int)eProperty.Strength] -= (int)m_spell.Value;
            Caster.BuffBonusCategory1[(int)eProperty.Constitution] -= (int)m_spell.Value;          
 
            if(Caster is GamePlayer)
            {
            	GamePlayer player = Caster as GamePlayer;          	
             	player.Out.SendCharStatsUpdate(); 
             	player.UpdateEncumberance();
             	player.UpdatePlayerStatus();
            } 
            return base.OnEffectExpires(effect,noMessages);
        } 
        
        public StrengthConstitutionDrain(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    
    /// <summary>
    /// ABS Damage shield spell handler
    /// </summary>
    [SpellHandlerAttribute("ABSDamageShield")]
    public class ABSDamageShield : AblativeArmorSpellHandler
    {
  		protected override void OnDamageAbsorbed(AttackData ad, int DamageAmount)
		{
            AttackData newad = new AttackData();
            newad.Attacker = ad.Target;
            newad.Target = ad.Attacker;
            newad.Damage = DamageAmount;
            newad.DamageType = Spell.DamageType;
            newad.AttackType = AttackData.eAttackType.Spell;
            newad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
            ad.Target.OnAttackedByEnemy(newad);
            ad.Attacker.DealDamage(newad); 			
		}       
        public ABSDamageShield(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    
    /// <summary>
    /// Morph spell handler
    /// </summary>
    [SpellHandlerAttribute("Morph")]
    public class Morph : SpellHandler
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {    
           if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;  
            	player.Model = (ushort)Spell.LifeDrainReturn;     
            	player.Out.SendUpdatePlayer();  
            }       	
     		base.OnEffectStart(effect); 
        }
        
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
           if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;    
 				player.Model = (ushort)player.Client.Account.Characters[player.Client.ActiveCharIndex].CreationModel;            	
 				player.Out.SendUpdatePlayer();  
            }                       
            return base.OnEffectExpires(effect, noMessages);         	
        }    	
    	public Morph(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }   
 
    /// <summary>
    /// Arcane leadership spell handler (range+resist pierce)
    /// </summary>
    [SpellHandlerAttribute("ArcaneLeadership")]
    public class ArcaneLeadership : DualStatBuff
    {
		public override eProperty Property1 { get { return eProperty.SpellRange; } }	
		public override eProperty Property2 { get { return eProperty.ResistPierce; } }			
    	
    	public ArcaneLeadership(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }   
}
