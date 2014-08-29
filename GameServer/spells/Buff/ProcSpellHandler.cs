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
using System.Reflection;

using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

using log4net;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Base class for proc spell handler
    /// </summary>
    public abstract class BaseProcSpellHandler : BuffSpellHandler
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected abstract string SubSpellLineName { get; }

        /// <summary>
        /// The event type to hook on
        /// </summary>
        protected abstract DOLEvent EventType { get; }
  
        /// <summary>
        /// Holds the proc spell
        /// </summary>
        protected Spell m_procSpell;
        
        /// <summary>
        /// Get The Proc Spell
        /// </summary>
		public Spell ProcSpell 
		{
			get { return m_procSpell; }
		}

        /// <summary>
        /// Holds the proc spell line
        /// </summary>
        protected SpellLine m_procSpellLine;
        
        /// <summary>
        /// Holds the range Helper in case of limited range check
        /// </summary>
        protected EffectRangeCheckHelper m_rangeHelper;

        /// <summary>
        /// Constructs new proc spell handler
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="spellLine"></param>
        public BaseProcSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
            : base(caster, spell, spellLine)
        {
            m_procSpellLine = SkillBase.GetSpellLine(SubSpellLineName);
            m_procSpell = SkillBase.GetSpellByID((int)spell.Value);
            
            // limited range set on AmnesiaChance.
            if (Spell.AmnesiaChance > 0)
            {
            	m_rangeHelper = new EffectRangeCheckHelper(this, Spell.AmnesiaChance);
            }
        }

        /// <summary>
        /// The event handler of given event type
        /// </summary>
        protected abstract void EventHandler(DOLEvent e, object sender, EventArgs arguments);
        
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
        	Caster.ChangeMana(Caster, GameLiving.eManaChangeType.Spell, -1 * PowerCost(target, true));
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            
            if (m_rangeHelper != null)
            {
            	m_rangeHelper.OnEffectStart(effect);
            }
            
            // "Your weapon is blessed by the gods!"
            // "{0}'s weapon glows with the power of the gods!"
            eChatType chatType = eChatType.CT_SpellPulse;
            
            if (!Spell.IsPulsing)
            {
                chatType = eChatType.CT_Spell;
            }
            
            MessageToLiving(effect.Owner, Spell.Message1, chatType);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), chatType, effect.Owner);
            
            // don't listen to all world event.
            if (effect.Owner != null)
            	GameEventMgr.AddHandler(effect.Owner, EventType, new DOLEventHandler(EventHandler));
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
        	// if already Expired exit.
        	if (m_rangeHelper != null)
        	{
        		if (m_rangeHelper.IsAlreadyExpired(effect))
        			return 0;
        	}
        	
            if (!noMessages)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);
            }
            
            // prevent errors.
            if (effect.Owner != null)
            	GameEventMgr.RemoveHandler(effect.Owner, EventType, new DOLEventHandler(EventHandler));
            
            return base.OnEffectExpires(effect, noMessages);
        }

        /// <summary>
        /// Check if this proc spell is overwritable by another.
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        public override bool IsOverwritable(GameSpellEffect compare)
        {
        	if (base.IsOverwritable(compare))
        	{
        		// Check if it's an instance of same type. (don't override effect group)
        		if ((GetType().IsInstanceOfType(compare.SpellHandler) || compare.SpellHandler.GetType().IsInstanceOfType(this)) 
        		    && !(Spell.EffectGroup != 0 && compare.Spell.EffectGroup != 0 && Spell.EffectGroup != compare.Spell.EffectGroup))
        		{
        			Spell oldProcSpell = ProcSpell;
        			Spell newProcSpell = ((BaseProcSpellHandler)compare.SpellHandler).ProcSpell;
        			
        			if (!(oldProcSpell == null || newProcSpell == null) && oldProcSpell.SpellType != newProcSpell.SpellType)
        				return false;
        		}
        		
        		return true;
        	}
        	
        	return false;
        }

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>();

//                list.Add("Function: " + (string)(Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "ProcSpellHandler.DelveInfo.Function", (string)(Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType)));

//                list.Add("Target: " + Spell.Target);
                list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Target", Spell.Target));

//                if (Spell.Range != 0) list.Add("Range: " + Spell.Range);
                if (Spell.Range != 0)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Range", Spell.Range));

//                if (Spell.Duration >= ushort.MaxValue * 1000) list.Add("Duration: Permanent.");
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Duration") + " Permanent.");

//                else if (Spell.Duration > 60000) list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
                else if (Spell.Duration > 60000)
                    list.Add(string.Format(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "DelveInfo.Duration") + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + "min"));

                else if (Spell.Duration != 0) list.Add("Duration: " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
                if (Spell.Power != 0) list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
                list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
                if (Spell.RecastDelay > 60000) list.Add("Recast time: " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
                else if (Spell.RecastDelay > 0) list.Add("Recast time: " + (Spell.RecastDelay / 1000).ToString() + " sec");
                if (Spell.Concentration != 0) list.Add("Concentration cost: " + Spell.Concentration);
                if (Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);

                // Recursion check
                byte nextDelveDepth = (byte)(DelveInfoDepth + 1);
                if (nextDelveDepth > MAX_DELVE_RECURSION)
                {
                    list.Add("(recursion - see server logs)");
                    log.ErrorFormat("Spell delve info recursion limit reached. Source spell ID: {0}, Sub-spell ID: {1}", m_spell.ID, m_procSpell.ID);
                }
                else
                {
                    // add subspell specific informations
                    list.Add(" "); //empty line
                    list.Add("Sub-spell informations: ");
                    list.Add(" "); //empty line
                    ISpellHandler subSpellHandler = ScriptMgr.CreateSpellHandler(Caster, m_procSpell, m_procSpellLine);
                    if (subSpellHandler == null)
                    {
                        list.Add("unable to create subspell handler: '" + SubSpellLineName + "', " + m_spell.Value);
                        return list;
                    }
                    subSpellHandler.DelveInfoDepth = nextDelveDepth;
                    // Get delve info of sub-spell
                    IList<string> subSpellDelve = subSpellHandler.DelveInfo;
                    if (subSpellDelve.Count > 0)
                    {
                        subSpellDelve.RemoveAt(0);
                        list.AddRange(subSpellDelve);
                    }
                }

                return list;
            }
        }
    }

    /// <summary>
    /// This class contains data for OffensiveProc spells
    /// </summary>
    [SpellHandlerAttribute("OffensiveProc")]
    public class OffensiveProcSpellHandler : BaseProcSpellHandler
    {
		/// <summary>
        /// The event type to hook on
        /// </summary>
        protected override DOLEvent EventType
        {
            get { return GameLivingEvent.AttackFinished; }
        }

        /// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected override string SubSpellLineName
        {
            get { return "OffensiveProc"; }
        }

        /// <summary>
        /// Handler fired whenever effect target is landing a successful attack.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
            
            if (args == null || args.AttackData == null)
            {
                return;
            }
            
            AttackData ad = args.AttackData;
            
            if (!args.AttackData.IsMeleeAttack || (args.AttackData.AttackResult != eAttackResult.HitUnstyled && args.AttackData.AttackResult != eAttackResult.HitStyle))
                return;

            int baseChance = Spell.Frequency / 100;

            // Halve chance for dual wield attacks.
            if (args.AttackData.AttackType == AttackData.eAttackType.MeleeDualWield)
            {
				baseChance /= 2;
            }
            
            // Adapt chances to attack speed
            if (args.AttackData.WeaponSpeed != 0)
            	baseChance = (int)(baseChance * Math.Max(0.3, args.AttackData.WeaponSpeed / 35.0));

            if (baseChance < 1)
                baseChance = 1;

            if (Util.Chance(baseChance))
            {
            	SpellHandler handler = (SpellHandler)ScriptMgr.CreateSpellHandler(args.AttackData.Attacker, m_procSpell, m_procSpellLine);
                
                if (handler == null)
                	return;
                
                // Benefic spell proc on self (pbae realm, group, or self targeted.) other proc on attack Target.
                if (handler.HasPositiveEffect)
                {
                	handler.StartSpell(args.AttackData.Attacker);
                }
                else
                {
                	if (args.AttackData.Target.IsWithinRadius(args.AttackData.Attacker, handler.CalculateSpellRange()))
                		handler.StartSpell(args.AttackData.Target);
                }
            }
        }

        // constructor
        public OffensiveProcSpellHandler(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }

    /// <summary>
    /// This class is similar to OffensiveProc except that it doesn't need a hit to land to have a chance to proc
    /// Mostly used for Mobs and Pet which can have low hit-rate or low chance of successful hits (evade, block, parry by a player character)
    /// </summary>
    [SpellHandlerAttribute("OnHitOffensiveProc")]    
    public class OnHitOffensiveSpellHandler : BaseProcSpellHandler
    {
    	/// <summary>
        /// The event type to hook on
        /// </summary>
        protected override DOLEvent EventType
        {
            get { return GameLivingEvent.AttackFinished; }
        }

        /// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected override string SubSpellLineName
        {
            get { return "OffensiveProc"; }
        }

    	/// <summary>
        /// Handler fired whenever effect target is trying to attack
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
            
            if (args == null || args.AttackData == null)
            {
                return;
            }
            
            AttackData ad = args.AttackData;
            
            if (!args.AttackData.IsMeleeAttack || !args.AttackData.IsHit)
                return;

            int baseChance = Spell.Frequency / 100;

            // Halve chance for dual wield attacks.
            if (args.AttackData.AttackType == AttackData.eAttackType.MeleeDualWield)
            {
				baseChance /= 2;
            }
            
            // Adapt chances to attack speed
            if (args.AttackData.WeaponSpeed != 0)
            	baseChance = (int)(baseChance * Math.Max(0.3, args.AttackData.WeaponSpeed / 35.0));

            if (baseChance < 1)
                baseChance = 1;

            if (Util.Chance(baseChance))
            {
            	SpellHandler handler = (SpellHandler)ScriptMgr.CreateSpellHandler(args.AttackData.Attacker, m_procSpell, m_procSpellLine);
                
                if (handler == null)
                	return;
                
                // Benefic spell proc on self (pbae realm, group, or self targeted.) other proc on attack Target.
                if (handler.HasPositiveEffect)
                {
                	handler.StartSpell(args.AttackData.Attacker);
                }
                else
                {
                	if (args.AttackData.Target.IsWithinRadius(args.AttackData.Attacker, handler.CalculateSpellRange()))
                		handler.StartSpell(args.AttackData.Target);
                }
            }
        }
    	
        // constructor
        public OnHitOffensiveSpellHandler(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
    
    /// <summary>
    /// This class contains data for DefensiveProc spells
    /// </summary>
    [SpellHandlerAttribute("DefensiveProc")]
    public class DefensiveProcSpellHandler : BaseProcSpellHandler
    {
        /// <summary>
        /// The event type to hook on
        /// </summary>
        protected override DOLEvent EventType
        {
            get { return GameLivingEvent.AttackedByEnemy; }
        }

        /// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected override string SubSpellLineName
        {
            get { return "DefensiveProc"; }
        }

        /// <summary>
        /// Handler fired whenever effect target is attacked
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void EventHandler(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
            if (args == null) 
            	return;
            if (args.AttackData == null) 
            	return;

            if (!args.AttackData.IsMeleeAttack || (args.AttackData.AttackResult != eAttackResult.HitUnstyled && args.AttackData.AttackResult != eAttackResult.HitStyle))
                return;

            int baseChance = Spell.Frequency / 100;

            // Lower chance for dual wield attacks.
            if (args.AttackData.AttackType == AttackData.eAttackType.MeleeDualWield)
            	baseChance /= 2;
            
            // Adapt chances to attack speed
            if (args.AttackData.WeaponSpeed != 0)
            	baseChance = (int)(baseChance * Math.Max(0.3, args.AttackData.WeaponSpeed / 35.0));
            
            if (baseChance < 1)
                baseChance = 1;

            if (Util.Chance(baseChance))
            {
            	SpellHandler handler = ((SpellHandler)ScriptMgr.CreateSpellHandler(args.AttackData.Target, m_procSpell, m_procSpellLine));
                if (handler == null)
                	return;
                
                // Benefic spell proc on self (pbae realm, group, or self targeted.) other proc on attack Target.
                if (handler.HasPositiveEffect)
                {
                	handler.StartSpell(args.AttackData.Target);
                }
                else
                {
                	if (args.AttackData.Attacker.IsWithinRadius(args.AttackData.Target, handler.CalculateSpellRange()))
                		handler.StartSpell(args.AttackData.Attacker);
                }
            }
        }

        // constructor
        public DefensiveProcSpellHandler(GameLiving caster, Spell spell, SpellLine line)
        	: base(caster, spell, line)
        {
        }
    }
   
    /// <summary>
    /// Offensive Proc Handler for PvE Targets Only
    /// </summary>
	[SpellHandlerAttribute( "OffensiveProcPvE" )]
	public class OffensiveProcPvESpellHandler : OffensiveProcSpellHandler
	{
		/// <summary>
		/// Handler fired whenever effect target is attacked
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected override void EventHandler( DOLEvent e, object sender, EventArgs arguments )
		{
			AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
			
			if ( args == null || args.AttackData == null )
			{
				return;
			}
			
			// This only work on PvE Targets !
			if (args.AttackData.Target.Realm == eRealm.None)
				base.EventHandler( e, sender, arguments );
		}
		
		public OffensiveProcPvESpellHandler( GameLiving caster, Spell spell, SpellLine line )
		: base( caster, spell, line )
		{
		}
	}
}
