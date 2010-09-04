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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
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
    public abstract class BaseProcSpellHandler : SpellHandler
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		//VaNaTiC->
        /*
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        */
        //VaNaTiC<-

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
        }

        /// <summary>
        /// The event type to hook on
        /// </summary>
        protected abstract DOLEvent EventType { get; }

        /// <summary>
        /// The spell line name of the proc spell
        /// </summary>
        protected abstract string SubSpellLineName { get; }

        /// <summary>
        /// The event handler of given event type
        /// </summary>
        protected abstract void EventHandler(DOLEvent e, object sender, EventArgs arguments);

        /// <summary>
        /// Holds the proc spell
        /// </summary>
        protected Spell m_procSpell;

        /// <summary>
        /// Holds the proc spell line
        /// </summary>
        protected SpellLine m_procSpellLine;

        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
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
            // "Your weapon is blessed by the gods!"
            // "{0}'s weapon glows with the power of the gods!"
            eChatType chatType = eChatType.CT_SpellPulse;
            if (Spell.Pulse == 0)
            {
                chatType = eChatType.CT_Spell;
            }
            MessageToLiving(effect.Owner, Spell.Message1, chatType);
            Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), chatType, effect.Owner);
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
            if (!noMessages)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);
            }
            GameEventMgr.RemoveHandler(effect.Owner, EventType, new DOLEventHandler(EventHandler));
            return 0;
        }

        /// <summary>
        /// Determines wether this spell is better than given one
        /// </summary>
        /// <returns>true if this spell is better version than compare spell</returns>
        public override bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
        {
            Spell oldProcSpell = SkillBase.GetSpellByID((int)oldeffect.Spell.Value);
            Spell newProcSpell = SkillBase.GetSpellByID((int)neweffect.Spell.Value);

            if (oldProcSpell == null || newProcSpell == null)
                return true;

            // do not replace active proc with different type proc
            if (oldProcSpell.SpellType != newProcSpell.SpellType) return false;

            if (oldProcSpell.Concentration > 0) return false;

            // if the new spell does less damage return false
            if (oldProcSpell.Damage > newProcSpell.Damage) return false;

            // if the new spell is lower than the old one return false
            if (oldProcSpell.Value > newProcSpell.Value) return false;

            //makes problems for immunity effects
            if (oldeffect is GameSpellAndImmunityEffect == false || ((GameSpellAndImmunityEffect)oldeffect).ImmunityState == false)
            {
                if (neweffect.Duration <= oldeffect.RemainingTime) return false;
            }

            return true;
        }
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            if (Spell.Group != 0)
                return Spell.Group == compare.Spell.Group;
            if (compare.Spell.SpellType != Spell.SpellType)
                return false;
            Spell oldProcSpell = SkillBase.GetSpellByID((int)Spell.Value);
            Spell newProcSpell = SkillBase.GetSpellByID((int)compare.Spell.Value);
            if (oldProcSpell == null || newProcSpell == null)
                return true;
            if (oldProcSpell.SpellType != newProcSpell.SpellType)
                return false;
            return true;
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
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "ProcSpellHandler.DelveInfo.Function", (string)(Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType)));

//                list.Add("Target: " + Spell.Target);
                list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Target", Spell.Target));

//                if (Spell.Range != 0) list.Add("Range: " + Spell.Range);
                if (Spell.Range != 0)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Range", Spell.Range));

//                if (Spell.Duration >= ushort.MaxValue * 1000) list.Add("Duration: Permanent.");
                if (Spell.Duration >= ushort.MaxValue * 1000)
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + " Permanent.");

//                else if (Spell.Duration > 60000) list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
                else if (Spell.Duration > 60000)
                    list.Add(string.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "DelveInfo.Duration") + Spell.Duration / 60000 + ":" + (Spell.Duration % 60000 / 1000).ToString("00") + "min"));

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
    [SpellHandler("OffensiveProc")]
    public class OffensiveProcSpellHandler : BaseProcSpellHandler
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        /// Handler fired whenever effect target is attacked
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
            if (ad.AttackResult != GameLiving.eAttackResult.HitUnstyled && ad.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;

            int baseChance = Spell.Frequency / 100;

            if (ad.IsMeleeAttack)
            {
                if (sender is GamePlayer)
                {
                    GamePlayer player = (GamePlayer)sender;
                    InventoryItem leftWeapon = player.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
                    // if we can use left weapon, we have currently a weapon in left hand and we still have endurance,
                    // we can assume that we are using the two weapons.
                    if (player.CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type != (int)eObjectType.Shield)
                    {
                        baseChance /= 2;
                    }
                }
            }

            if (baseChance < 1)
                baseChance = 1;

            if (Util.Chance(baseChance))
            {
                ISpellHandler handler = ScriptMgr.CreateSpellHandler((GameLiving)sender, m_procSpell, m_procSpellLine);
                if (handler != null)
                {
					if (m_procSpell.Target.ToLower() == "enemy")
					{
						handler.StartSpell(ad.Target);
					}
					else if (m_procSpell.Target.ToLower() == "self")
					{
						handler.StartSpell(ad.Attacker);
					}
					else if (m_procSpell.Target.ToLower() == "group")
					{
						GamePlayer player = Caster as GamePlayer;
						if (Caster is GamePlayer)
						{
							if (player.Group != null)
							{
								foreach (GameLiving groupPlayer in player.Group.GetMembersInTheGroup())
								{
									if (player.IsWithinRadius(groupPlayer, m_procSpell.Range))
									{
										handler.StartSpell(groupPlayer);
									}
								}
							}
							else
							{
								handler.StartSpell(player);
							}
						}
					}
					else if (m_procSpell.Target.ToLower() == "realm")
					{
						GamePlayer player = Caster as GamePlayer;
						if (player != null)
						{
							foreach (GameLiving realmPlayer in player.GetPlayersInRadius((ushort)m_procSpell.Radius))
							{
								if (GameServer.ServerRules.IsSameRealm(player, realmPlayer, true))
								{
									handler.StartSpell(realmPlayer);
								}
							}
						}
					}
					else
					{
						log.Warn("Unknown spell target; skipping " + m_procSpell.Target + " proc " + m_procSpell.Name + " on " + ad.Target.Name + "; Realm = " + ad.Target.Realm);
					}
                }
            }
        }

        // constructor
        public OffensiveProcSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }

    /// <summary>
    /// This class contains data for DefensiveProc spells
    /// </summary>
    [SpellHandler("DefensiveProc")]
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
            if (args == null) return;
            if (args.AttackData == null) return;
            if (args.AttackData.SpellHandler != null) return;
            if (args.AttackData.AttackResult != GameLiving.eAttackResult.HitUnstyled
                && args.AttackData.AttackResult != GameLiving.eAttackResult.HitStyle)
                return;

            int baseChance = Spell.Frequency / 100;

            if (baseChance < 1)
                baseChance = 1;

            if (Util.Chance(baseChance))
            {
                ISpellHandler handler = ScriptMgr.CreateSpellHandler((GameLiving)sender, m_procSpell, m_procSpellLine);
                if (handler != null)
                    handler.StartSpell(args.AttackData.Attacker);
            }
        }

        // constructor
        public DefensiveProcSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    
       [SpellHandler( "OffensiveProcPvE" )]
       public class OffensiveProcPvESpellHandler : OffensiveProcSpellHandler
       {
          /// <summary>
          /// The event type to hook on
          /// </summary>
          protected override DOLEvent EventType
          {
             get { return GameLivingEvent.AttackFinished; }
          }


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

             if( args.AttackData.Target.Realm == eRealm.None )
                base.EventHandler( e, sender, arguments );
          }

          public OffensiveProcPvESpellHandler( GameLiving caster, Spell spell, SpellLine line ) : base( caster, spell, line ) { }
       }
}
