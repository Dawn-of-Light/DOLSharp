using System;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
    [SpellHandlerAttribute("Ereine")]
    public class Ereine : AllStatsBuff
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {    
     		base.OnEffectStart(effect);
			if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;			
				GameEventMgr.AddHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			}
		}
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {  
            if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
				Spell subspell = SkillBase.GetSpellByID(m_spell.ResurrectMana);
				if (subspell != null)
				{
					subspell.Level=m_spell.Level;
					ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(Caster, subspell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
					if(spellhandler!=null )spellhandler.StartSpell(Caster);
                }
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            }
			return base.OnEffectExpires(effect, noMessages);
        }
		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;
            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null) ad = attackedByEnemy.AttackData;
            if((int)ad.DamageType>=1&&(int)ad.DamageType<=3)
			{
				int absorb=(int)Math.Round((double)ad.Damage*(double)Spell.Value/100);
				int critical=(int)Math.Round((double)ad.CriticalDamage*(double)Spell.Value/100);
				ad.Damage-=absorb;
				ad.CriticalDamage-=critical;
				if(ad.Attacker is GamePlayer) (ad.Attacker as GamePlayer).Out.SendMessage("Your target's Ereine charge absorb "+(absorb+critical)+" damages", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				if(ad.Target is GamePlayer) (ad.Target as GamePlayer).Out.SendMessage("Your Ereine charge absorb "+(absorb+critical)+" damages", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			}
        }
        public Ereine(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
	[SpellHandlerAttribute("Ereine2")]
    public class Ereine2 : AllStatsDebuff
    {
		public override bool HasPositiveEffect { get { return false; } }
        public override bool IsUnPurgeAble { get { return true; } }
		public Ereine2(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	[SpellHandlerAttribute("EreineProc")]
    public class EreineProc : SpellHandler
    {
		public override void OnEffectStart(GameSpellEffect effect)
        {    
     		base.OnEffectStart(effect);
			if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
				GameEventMgr.AddHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
			}
		}
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {  
            if(effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            }
			return base.OnEffectExpires(effect, noMessages);
        }
		private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
        {
            GameLiving living = sender as GameLiving;
            if (living == null) return;
            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            AttackData ad = null;
            if (attackedByEnemy != null) ad = attackedByEnemy.AttackData;
            if((int)ad.DamageType>=10&&(int)ad.DamageType<=15)
			{
				if(ad.Attacker is GamePlayer) (ad.Attacker as GamePlayer).Out.SendMessage("Your target' Ereine Proc absorb "+(ad.Damage+ad.CriticalDamage)+" damages", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				if(ad.Target is GamePlayer) (ad.Target as GamePlayer).Out.SendMessage("Your Ereine Proc absorb "+(ad.Damage+ad.CriticalDamage)+" damages", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				ad.Damage=0; ad.CriticalDamage=0;
				GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, this);
				if(effect != null) effect.Cancel(false);
			}
        }		
		public EreineProc(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}