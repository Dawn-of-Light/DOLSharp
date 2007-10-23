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
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Scripts;
using DOL.GS.Spells;
using DOL.Language;

namespace DOL.GS
{
	public class NecromancerPet : GameNPC
	{
		private RegionTimer	distanceTimer;

		public NecromancerPet(INpcTemplate template) : base(template)
		{}

		// Get Owner Spell Critical chance
		public override int SpellCriticalChance
		{
			get
			{
				if(Brain != null && Brain is ControlledNpc)
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						return ((ControlledNpc)Brain).GetPlayerOwner().SpellCriticalChance;
				
				return base.SpellCriticalChance;
			}
		}

		/// Ability bonus property
		public override IPropertyIndexer AbilityBonus
		{
			get 
			{
				if(Brain != null && Brain is ControlledNpc)
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						return ((ControlledNpc)Brain).GetPlayerOwner().AbilityBonus;
				
				return base.AbilityBonus;				
			}
		}

		/// Property Item Bonus field
		public override IPropertyIndexer ItemBonus
		{
			get 
			{
				if(Brain != null && Brain is ControlledNpc)
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						return ((ControlledNpc)Brain).GetPlayerOwner().ItemBonus;
				
				return base.ItemBonus;					
			}
		}
		
		// Get/Set Owner mana
		public override int Mana
		{
			get
			{
				if(Brain != null && Brain is ControlledNpc)
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						return ((ControlledNpc)Brain).GetPlayerOwner().Mana;
				
				return base.Mana;
			}
			set
			{
				if(Brain != null && Brain is ControlledNpc)
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						((ControlledNpc)Brain).GetPlayerOwner().Mana=value;
				
				base.Mana = value;
			}
		}
		
		// Get Owner Max mana
		public override int MaxMana
		{
			get
			{
				if(Brain != null && Brain is ControlledNpc)
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						return ((ControlledNpc)Brain).GetPlayerOwner().MaxMana;
				
				return base.MaxMana;
			}
		}

		// Get Owner modified spec level
		public override int GetModifiedSpecLevel(string keyName)
		{
			if(Brain != null && Brain is ControlledNpc) // necropet shared spellskill							
				if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
					return ((ControlledNpc)Brain).GetPlayerOwner().GetModifiedSpecLevel(keyName);

			return base.GetModifiedSpecLevel(keyName);
		}
	
		public override bool AddToWorld()
		{
			if (Brain!=null && Brain is ControlledNpc)
			{
				if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
					((ControlledNpc)Brain).GetPlayerOwner().Shade(true);
				
				if (((ControlledNpc)Brain).Owner!=null)
				{
					//Add handler to catch necromancer spell cast
					GameEventMgr.AddHandler(((ControlledNpc)Brain).Owner, GameLivingEvent.StartSpell, new DOLEventHandler(OnSpellCast));
	
					//Distance timer
					distanceTimer = new RegionTimer(((ControlledNpc)Brain).Owner, new RegionTimerCallback(CheckDistance),1000);			
				}
			}			
			return base.AddToWorld();
		}
		
		public override void Die(GameObject killer)
		{
			if (distanceTimer!=null) {distanceTimer.Stop(); distanceTimer=null;}
			if (Brain!=null && Brain is ControlledNpc)
			{
				if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
					((ControlledNpc)Brain).GetPlayerOwner().Shade(false);
				
				if (((ControlledNpc)Brain).Owner!=null)
				{
					//Remove handler that catch necromancer spell cast
					GameEventMgr.RemoveHandler(((ControlledNpc)Brain).Owner, GameLivingEvent.StartSpell, new DOLEventHandler(OnSpellCast));
					
					//If pet has less than 100% HP, reduce Necromancer life to 10%
					if(HealthPercent!=100)
						((ControlledNpc)Brain).Owner.Health = (int)(0.1 * (((ControlledNpc)Brain).Owner.MaxHealth));
				}
			}
			base.Die(killer);		
		}		

		private int CheckDistance(RegionTimer timer)
        {
			if (Brain == null 
			    || !(Brain is ControlledNpc) 
			    || ((ControlledNpc)Brain).Owner == null 
			    || !(((ControlledNpc)Brain).Owner.IsAlive))
            {
				timer.Stop();
				timer=null;
				return 0;
			}
			if(WorldMgr.GetDistance(((ControlledNpc)Brain).Owner,this) > 2000)
			{
				// Release pet if too far away from necromancer
				((ControlledNpc)Brain).Owner.CommandNpcRelease();
				timer.Stop();
				timer=null;
				return 0;				
			}
			return 1000;
		}
		/// <summary>
		/// Called when owner cast a spell
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnSpellCast(DOLEvent e, object sender, EventArgs arguments)
		{
			GameLiving owner = sender as GameLiving;
			CastSpellEventArgs spell = arguments as CastSpellEventArgs;
			if (Brain == null || !(Brain is ControlledNpc)) return;
			if (((ControlledNpc)Brain).Owner == null) return;
			if (owner == null) return;
			if (spell == null) return;

			if(spell.SpellHandler.Spell.SpellType.ToLower()!="necromancerpet")
			{
				// Build a new spell
				Spell ModifiedSpell = BuildNecroSpell(spell.SpellHandler.Spell);
				// Set the target
				if(TargetObject!=owner.TargetObject) TargetObject = owner.TargetObject;
				// Make the pet cast this spell
				CastSpell(ModifiedSpell,spell.SpellHandler.SpellLine);				
			}
		}		
		protected virtual Spell BuildNecroSpell(Spell OriginalSpell)
		{
            DBSpell dbs = new DBSpell();
            dbs.AutoSave = false;
            dbs.Name = OriginalSpell.Name;
            dbs.Icon = OriginalSpell.Icon;
            dbs.ClientEffect = OriginalSpell.ClientEffect;
            dbs.Description = OriginalSpell.Description;
            dbs.Damage = OriginalSpell.Damage;
            dbs.DamageType = (int)OriginalSpell.DamageType;
            dbs.Radius = OriginalSpell.Radius;
            dbs.Type = OriginalSpell.SpellType;
            dbs.Value = OriginalSpell.Value;
            dbs.Duration = OriginalSpell.Duration / 1000;
            dbs.Frequency = OriginalSpell.Frequency / 100;
            dbs.Pulse = OriginalSpell.Pulse;
            dbs.PulsePower = OriginalSpell.PulsePower;
            dbs.Radius = OriginalSpell.Radius;
            dbs.LifeDrainReturn = OriginalSpell.LifeDrainReturn;
            dbs.Message1 = OriginalSpell.Message1;
            dbs.Message2 = OriginalSpell.Message2;
            dbs.Message3 = OriginalSpell.Message3;
            dbs.Message4 = OriginalSpell.Message4;
            dbs.Range = OriginalSpell.Range;
            // Modify target to self if it was pet
            if(OriginalSpell.Target.ToLower()=="pet")
            	dbs.Target = "Self";
            else
            	dbs.Target = OriginalSpell.Target;  
            // Set Spell powercost to 0 since it is calculated on necromancer spell cast
            dbs.Power = 0;
            // Spell pet cast time stored in ResurrectHealth
            dbs.CastTime = OriginalSpell.ResurrectHealth;
            Spell NewSpell = new Spell(dbs, 1);
            dbs = null;
            return NewSpell;
 		}
		
		#region Necro Spell cast
		//Added spell queue for necro pet
		/// <summary>
		/// The next spell
		/// </summary>
		protected Spell m_nextSpell;
		/// <summary>
		/// The next spell line
		/// </summary>
		protected SpellLine m_nextSpellLine;
		/// <summary>
		/// A lock for the spellqueue
		/// </summary>
		protected object m_spellQueueAccessMonitor = new object();

		/// <summary>
		/// Callback after spell execution finished and next spell can be processed
		/// </summary>
		/// <param name="handler"></param>
		
		public override void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			lock (m_spellQueueAccessMonitor)
			{
				Spell nextSpell = m_nextSpell;
				SpellLine nextSpellLine = m_nextSpellLine;

				m_runningSpellHandler = null;
				m_nextSpell = null;			// avoid restarting nextspell by reentrance from spellhandler
				m_nextSpellLine = null;

				if(nextSpell!=null)
					m_runningSpellHandler = ScriptMgr.CreateSpellHandler(this, nextSpell, nextSpellLine);
			}
			if(m_runningSpellHandler!=null)
			{
				m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
				m_runningSpellHandler.CastSpell();
			}
		}
		
		/// <summary>
		/// Cast a specific spell from given spell line
		/// </summary>
		/// <param name="spell">spell to cast</param>
		/// <param name="line">Spell line of the spell (for bonus calculations)</param>
		public override void CastSpell(Spell spell, SpellLine line)
		{			
			if (IsStunned)
			{
				if(Brain != null && Brain is ControlledNpc)
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						((ControlledNpc)Brain).GetPlayerOwner().Out.SendMessage(LanguageMgr.GetTranslation(((ControlledNpc)Brain).GetPlayerOwner().Client, "GamePlayer.CastSpell.CantCastStunned"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			if (IsMezzed)
			{
				if(Brain != null && Brain is ControlledNpc)
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						((ControlledNpc)Brain).GetPlayerOwner().Out.SendMessage(LanguageMgr.GetTranslation(((ControlledNpc)Brain).GetPlayerOwner().Client, "GamePlayer.CastSpell.CantCastMezzed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			double fumbleChance = GetModified(eProperty.SpellFumbleChance);
			fumbleChance *= 0.01;
			if (fumbleChance > 0)
			{
				if (Util.ChanceDouble(fumbleChance))
				{
                   	if(Brain != null && Brain is ControlledNpc)
						if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
                   			((ControlledNpc)Brain).GetPlayerOwner().Out.SendMessage("You are fumbling for your words, and cannot cast!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    	return;
				}
			}			

			if (TargetObject == null) return;
						
			if(Brain != null && Brain is ControlledNpc)
				if (!((ControlledNpc)Brain).Owner.TargetInView)
				{
					if (((ControlledNpc)Brain).GetPlayerOwner()!=null)
						((ControlledNpc)Brain).GetPlayerOwner().Out.SendMessage("Your target is not in view.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
						
			lock (m_spellQueueAccessMonitor)
			{
				if (m_runningSpellHandler!=null && spell.CastTime>0)
				{
					if(Brain != null && Brain is ControlledNpc)
						if(((ControlledNpc)Brain).GetPlayerOwner()!=null)
							((ControlledNpc)Brain).GetPlayerOwner().Out.SendMessage(GetName(0,true) + " will cast this spell after this action.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					m_nextSpell = spell;
					m_nextSpellLine = line;
					return;
				}					
			}
			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
			if (spellhandler != null)
			{			
				if (spell.CastTime > 0)
				{
					if (AttackState) StopAttack();
					if (IsMoving)
					{
						StopFollow();
						StopMoving();
					}
					TurnTo(TargetObject);
					
					m_runningSpellHandler = spellhandler;
					m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
					spellhandler.CastSpell();
				}
				else
				{
					TurnTo(TargetObject);
					// insta cast no queue and no callback
					spellhandler.CastSpell();
				}
			}
			else
			{
				if(Brain != null && Brain is ControlledNpc)
					if(((ControlledNpc)Brain).GetPlayerOwner()!=null)
						((ControlledNpc)Brain).GetPlayerOwner().Out.SendMessage(spell.Name + " error (" + spell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion
	}
}
