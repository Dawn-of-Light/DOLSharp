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
using DOL.GS.Effects;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("TraitorsDaggerProc")]
    public class TraitorsDaggerProc : DefensiveProcSpellHandler
    {    	
   		public override void OnEffectStart(GameSpellEffect effect)
		{
            base.OnEffectStart(effect);
            if(effect.Owner is GamePlayer)
            {	
            	GamePlayer player = effect.Owner as GamePlayer;
   				player.Shade(true);
   			}
   		}   		
  		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
            if(effect.Owner is GamePlayer)
            {	
            	GamePlayer player = effect.Owner as GamePlayer;
  				player.Shade(false);
  			}
  			return base.OnEffectExpires(effect,noMessages);
  		}        
        public TraitorsDaggerProc(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

	[SpellHandlerAttribute("TraitorsDaggerSummon")]
    public class TraitorsDaggerSummon : SummonSpellHandler
    {    
		private DBSpell dbs;
		private Spell   s;
		private SpellLine sl;
		private ISpellHandler trap;
		
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			GamePlayer player = Caster as GamePlayer;
			if (player == null)	return;
			INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.AmnesiaChance);
			if (template == null)
			{
				String errorMessage = String.Format("NPC template {0} is missing, spell ID = {1}", Spell.AmnesiaChance, Spell.ID);
				if (log.IsWarnEnabled) log.Warn(errorMessage);
				if (player.Client.Account.PrivLevel > 1) MessageToCaster(errorMessage, eChatType.CT_Skill);
				return;
			}
			int x, y, z;
			Caster.GetSpotFromHeading(64, out x, out y);
			z = Caster.Z;
			ControlledNpc controlledBrain = new ControlledNpc(Caster);
			summoned = new TraitorDaggerPet(template);
			summoned.SetOwnBrain(new ProcPetBrain(player));
			summoned.HealthMultiplicator = true;
			summoned.X = x;
			summoned.Y = y;
			summoned.Z = z;
			summoned.GuildName=Caster.Name; // Andraste
			summoned.CurrentRegion = Caster.CurrentRegion;
			summoned.Heading = Caster.Heading;
			summoned.Realm = Caster.Realm;
			summoned.CurrentSpeed = 0;
			if (Spell.ResurrectHealth < 0) summoned.Level = (byte)(Caster.Level * Spell.ResurrectHealth * -0.01);
			else summoned.Level = (byte)Spell.ResurrectHealth;
			if (summoned.Level > Spell.ResurrectMana) summoned.Level = (byte)Spell.ResurrectMana;
			GameSpellEffect effect = CreateSpellEffect(target, effectiveness);
			summoned.AddToWorld();
			(summoned.Brain as IAggressiveBrain).AddToAggroList(target, 1);
			effect.Start(summoned);
			if(summoned!=null)
				GameEventMgr.AddHandler(summoned, GameLivingEvent.AttackFinished, new DOLEventHandler(EventHandler));
		}
		
		protected void EventHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
			if (args == null || args.AttackData == null)
				return;
			
			// Spirit procs lifetap when hitting ennemy
			if(trap!=null)
				trap.StartSpell(args.AttackData.Target);
		}	
		
        public TraitorsDaggerSummon(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) 
        {
			dbs = new DBSpell();
			dbs.Name = "Increased Essence Consumption";
			dbs.Icon = 661;
			dbs.ClientEffect = 661;
			dbs.DamageType = 10;
			dbs.Target = "Enemy";
			dbs.Radius = 0;
			dbs.Type = "Lifedrain";
			dbs.Damage = 50;
			dbs.LifeDrainReturn = 100;
			dbs.Value = -100;
			dbs.Duration = 0;
			dbs.Frequency = 0;
			dbs.Pulse = 0;
			dbs.PulsePower = 0;
			dbs.Power = 0;
			dbs.CastTime = 0;
			dbs.Range = 350;
			s = new Spell(dbs,1);
			sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);		
			trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);	
        }
    }
}

namespace DOL.GS
{
	public class TraitorDaggerPet : GameNPC
	{
		public override int MaxHealth
        {
            get { return Level*25; }
        }
		public override void OnAttackedByEnemy(AttackData ad) { }
		public TraitorDaggerPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
	}
}