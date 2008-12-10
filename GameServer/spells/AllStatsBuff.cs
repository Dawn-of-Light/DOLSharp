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
            living.AbilityBonus[(int)eProperty.ArmorAbsorbtion] += (int)m_spell.Value; 
            living.AbilityBonus[(int)eProperty.MagicAbsorbtion] += (int)m_spell.Value; 
            
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
            living.AbilityBonus[(int)eProperty.ArmorAbsorbtion] -= (int)m_spell.Value; 
            living.AbilityBonus[(int)eProperty.MagicAbsorbtion] -= (int)m_spell.Value; 
 
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
				IAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, (int)Spell.Value);
			}
		}		
        public AllStatsBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
 }