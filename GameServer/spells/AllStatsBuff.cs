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
    [SpellHandlerAttribute("AllStatsBuff")]
    public class AllStatsBuff : SpellHandler
    {
		public override int CalculateSpellResistChance(GameLiving target) { return 0; }
		
        public override void OnEffectStart(GameSpellEffect effect)
        {    
     		base.OnEffectStart(effect);
			GameLiving living = effect.Owner as GameLiving;
            living.AbilityBonus[(int)eProperty.Dexterity] += (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Strength] += (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Constitution] += (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Acuity] += (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Piety] += (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Empathy] += (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Quickness] += (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Intelligence] += (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Charisma] += (int)m_spell.Value;   
            living.AbilityBonus[(int)eProperty.ArmorAbsorption] += (int)m_spell.Value; 
            living.AbilityBonus[(int)eProperty.MagicAbsorption] += (int)m_spell.Value; 
            
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
			GameLiving living = effect.Owner as GameLiving;
            living.AbilityBonus[(int)eProperty.Dexterity] -= (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Strength] -= (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Constitution] -= (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Acuity] -= (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Piety] -= (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Empathy] -= (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Quickness] -= (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Intelligence] -= (int)m_spell.Value;
            living.AbilityBonus[(int)eProperty.Charisma] -= (int)m_spell.Value;        
            living.AbilityBonus[(int)eProperty.ArmorAbsorption] -= (int)m_spell.Value; 
            living.AbilityBonus[(int)eProperty.MagicAbsorption] -= (int)m_spell.Value; 
 
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
				IOldAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
			}
		}		
        public AllStatsBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
 }