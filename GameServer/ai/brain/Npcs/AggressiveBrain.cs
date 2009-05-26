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
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using System.Collections;
using log4net;
using System.Reflection;

namespace DOL.AI.Brain
{
    /// <summary>
    /// A brain that can aggro on other GameLiving objects.
    /// We derive from APlayerVicinityBrain to save CPU in case there
    /// aren't any players around anyway.
    /// </summary>
    /// <author>Aredhel</author>
    public class AggressiveBrain : APlayerVicinityBrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Mobs will all use a 1000ms think interval; aggro level is a chance
        /// to aggro on a living, it won't change the think interval anymore.
        /// </summary>
        public override int ThinkInterval
        {
            get { return 1000; }
        }

        /// <summary>
        /// Make some decisions.
        /// </summary>
        public override void Think()
        {
            if (Body.IsIncapacitated || Body.IsReturningToSpawnPoint)
                return;

            if (IsEngaged)
                PickTarget();
            else
                OnIdle();
        }

        #region Idle handler.

        /// <summary>
        /// The NPC has nothing to do.
        /// </summary>
        protected virtual void OnIdle()
        {
            if (Body == null)
                return;

            foreach (GamePlayer player in Body.GetPlayersInRadius(AggressionRange))
            {
                if (Util.Chance(GetChanceToAggro(player)))
                {
                    Aggression.Raise(player, InternalAggression.Initial);
                    return;
                }
            }
        }

        /// <summary>
        /// Range at which this brain will aggro at all.
        /// </summary>
        public virtual ushort AggressionRange
        {
            get { return 2000; }
        }

        /// <summary>
        /// Level of aggression.
        /// </summary>
        public virtual ushort AggressionLevel
        {
            get { return 100; }
        }

        /// <summary>
        /// Chance to aggro on this living; the default implementation
        /// is a flat chance if the living is attackable. A custom implementation 
        /// could make this dependent on distance, faction, whatever.
        /// </summary>
        /// <param name="living"></param>
        /// <returns></returns>
        protected virtual ushort GetChanceToAggro(GameLiving living)
        {
            return living.IsAttackable
                ? AggressionLevel
                : (ushort)0;
        }

        #endregion

        #region Aggression handler.

        /// <summary>
        /// Pick the next target.
        /// </summary>
        private void PickTarget()
        {
            GameLiving target = Aggression.PrimaryTarget;

            if (target == null)
            {
                Body.StopAttack();
                Aggression.Clear();
                Body.TargetObject = null;
                Body.WalkToSpawn();
            }
            else
            {
                if (target != Body.TargetObject)
                    SwitchTarget(target);
            }
        }

        /// <summary>
        /// Switch the target.
        /// </summary>
        /// <param name="living"></param>
        private void SwitchTarget(GameLiving target)
        {
            Body.StartAttack(target);
        }

        /// <summary>
        /// Body has been attacked.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="attackData"></param>
        protected virtual void OnAttacked(AttackData attackData)
        {
            if (attackData == null)
                return;

            if (attackData.Target == Body)
            {
                if (Body.IsReturningToSpawnPoint)
                    Body.CancelWalkToSpawn();

                if (!attackData.IsMeleeAttack)
                {
                    ISpellHandler spellhandler = attackData.SpellHandler;

                    if (spellhandler != null && spellhandler is TauntSpellHandler)
                    {
                        OnTaunted(attackData);
                        return;
                    }
                }

                Aggression.Raise(attackData.Attacker, attackData.Damage);

                // TODO: Process attack data and change the amount of aggro
                //       accordingly.
            }
            else
            {
                OnLivingAttacked(attackData.Target, attackData.Attacker);
            }
        }

        /// <summary>
        /// Another living has been attacked; this is were you hook
        /// BAF (Bring-A-Friend) in.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="attacker"></param>
        protected virtual void OnLivingAttacked(GameLiving target, GameLiving attacker)
        {
        }

        /// <summary>
        /// An enemy was healed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="amount"></param>
        protected virtual void OnEnemyHealed(GameObject source, int amount)
        {
            if (source == null)
                return;

            if (source is GameLiving)
                Aggression.Raise(source as GameLiving, InternalAggression.Minimum);

            // TODO: Track the source of the heal, e.g. if the heal originated
            //       from an object, find out who the owner is.
            //       Adjust amount of aggro generated.
        }

        /// <summary>
        /// An enemy has been killed.
        /// </summary>
        /// <param name="living"></param>
        protected virtual void OnEnemyKilled(GameLiving living)
        {
            if (living == null)
                return;

            Aggression.Remove(living);
        }

        private const int TauntAggressionAmount = 1000;

        /// <summary>
        /// The NPC has been taunted (spell).
        /// </summary>
        /// <param name="attackData"></param>
        protected virtual void OnTaunted(AttackData attackData)
        {
            GameLiving attacker = attackData.Attacker;

            if (attackData.Target == Body)
            {
                Aggression.Raise(attackData.Attacker, attackData.IsSpellResisted
                    ? 0 : TauntAggressionAmount);
            }
            else
            {
                OnLivingAttacked(attackData.Target, attacker);
            }
        }

        #endregion

        #region Notify handler.

        /// <summary>
        /// Process game events.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {
            base.Notify(e, sender, args);

            if (e == GameLivingEvent.AttackedByEnemy)
            {
                if (args is AttackedByEnemyEventArgs)
                {
                    AttackData attackData = (args as AttackedByEnemyEventArgs).AttackData;
                    OnAttacked(attackData);
                }

                return;
            }

            if (e == GameLivingEvent.EnemyHealed)
            {
                if (args is EnemyHealedEventArgs)
                {
                    EnemyHealedEventArgs healed = args as EnemyHealedEventArgs;
                    
                    if (IsEnemy(healed.Enemy))
                        OnEnemyHealed(healed.HealSource, healed.HealAmount);
                }

                return;
            }

            if (e == GameLivingEvent.EnemyKilled)
            {
                if (args is EnemyKilledEventArgs)
                {
                    EnemyKilledEventArgs killed = args as EnemyKilledEventArgs;

                    if (IsEnemy(killed.Target))
                        OnEnemyKilled(killed.Target);
                }

                return;
            }

            if (e == GameLivingEvent.LowHealth)
            {
                OnLowHealth(sender as GameLiving);
                return;
            }
        }

        #endregion

        #region Other handlers

        /// <summary>
        /// Living is low on health. This could be the body (decide to
        /// heal self, run away, go berserk...) or any other living in the
        /// game (heal if in radius and friendly etc.)
        /// </summary>
        /// <param name="living"></param>
        protected virtual void OnLowHealth(GameLiving living)
        {
        }

        #endregion

        #region Aggression management

        private InternalAggression m_aggression;
        
        /// <summary>
        /// This brain's aggression towards various targets.
        /// </summary>
        private InternalAggression Aggression
        {
            get
            {
                if (m_aggression == null)
                    m_aggression = new InternalAggression();

                return m_aggression;
            }

            set
            {
                m_aggression = value;
            }
        }

        /// <summary>
        /// Engage on the living.
        /// </summary>
        /// <param name="living"></param>
        protected void EngageOn(GameLiving living)
        {
            if (!IsEnemy(living))
                Aggression.Raise(living, InternalAggression.Initial);
        }

        /// <summary>
        /// Returns true if this brain has at least one target.
        /// </summary>
        private bool IsEngaged
        {
            get
            {
                return (m_aggression == null)
                    ? false
                    : (Aggression.Targets.Count > 0);
            }
        }

        /// <summary>
        /// Check whether or not this living is an enemy.
        /// </summary>
        /// <param name="living"></param>
        /// <returns></returns>
        protected bool IsEnemy(GameLiving living)
        {
            return (living == null)
                ? false
                : (Aggression.IsEnemy(living));
        }

        /// <summary>
        /// Encapsulated in order to achieve thread safety.
        /// </summary>
        private class InternalAggression
        {
            private IDictionary<GameLiving, long> m_aggression = new Dictionary<GameLiving, long>();
            private object m_syncObject = new object();

            /// <summary>
            /// Get current amount of aggression for this living.
            /// </summary>
            /// <param name="living"></param>
            /// <returns></returns>
            public bool IsEnemy(GameLiving living)
            {
                lock (m_syncObject)
                    return m_aggression.ContainsKey(living);
            }

            /// <summary>
            /// List of targets.
            /// </summary>
            public IList<GameLiving> Targets
            {
                get
                {
                    lock (m_syncObject)
                    {
                        IList<GameLiving> targets = new List<GameLiving>(m_aggression.Count);
                        foreach (GameLiving target in m_aggression.Keys)
                            targets.Add(target);

                        return targets;
                    }
                }
            }

            /// <summary>
            /// The amount of aggression gained from initially
            /// pulling an NPC; it takes some time to "outaggro"
            /// the puller.
            /// </summary>
            public const int Initial = 500;

            /// <summary>
            /// The minimum amount of aggression possible; no matter
            /// how much detaunting you do, the mob will still hate you
            /// till the end.
            /// </summary>
            public const int Minimum = 100;

            /// <summary>
            /// Raise aggression by the given amount.
            /// </summary>
            /// <param name="living"></param>
            /// <param name="amount"></param>
            public void Raise(GameLiving living, long amount)
            {
                if (living == null || amount < 0)
                    return;

                lock (m_syncObject)
                {
                    if (m_aggression.ContainsKey(living))
                        m_aggression[living] += amount;
                    else
                    {
                        m_aggression.Add(living, (amount < Initial) ? Initial : amount);

                        if (living is GamePlayer)
                        {
                            Group group = (living as GamePlayer).Group;

                            if (group != null)
                                foreach (GamePlayer player in group.GetPlayersInTheGroup())
                                    if (!m_aggression.ContainsKey(player))
                                        m_aggression.Add(player, Minimum);

                            // TODO: Pets.
                        }
                    }

                    if (living is GamePlayer)
                    {
                        (living as GamePlayer).Out.SendMessage(String.Format("Aggression = {0}", m_aggression[living]),
                            eChatType.CT_Alliance, eChatLoc.CL_SystemWindow);
                    }
                }
            }

            /// <summary>
            /// Lower aggression by the given amount, but not past the minimum.
            /// </summary>
            /// <param name="living"></param>
            /// <param name="amount"></param>
            public void Lower(GameLiving living, long amount)
            {
                if (living == null || amount <= 0)
                    return;

                lock (m_syncObject)
                {
                    if (m_aggression.ContainsKey(living))
                    {
                        long current = m_aggression[living];
                        m_aggression[living] = (current >= amount + Minimum) ? current - amount : Minimum;
                    }
                }
            }

            /// <summary>
            /// Remove a living from the list.
            /// </summary>
            /// <param name="living"></param>
            public void Remove(GameLiving living)
            {
                lock (m_syncObject)
                {
                    if (living != null && m_aggression.ContainsKey(living))
                        m_aggression.Remove(living);
                }
            }

            /// <summary>
            /// Clear all targets.
            /// </summary>
            public void Clear()
            {
                lock (m_syncObject)
                    m_aggression.Clear();
            }

            /// <summary>
            /// The living that has accumulated the highest amount of aggression.
            /// </summary>
            public GameLiving PrimaryTarget
            {
                get
                {
                    GameLiving primaryTarget = null;
                    long maxAmount = 0;

                    lock (m_syncObject)
                    {
                        foreach (GameLiving living in m_aggression.Keys)
                        {
                            if (living.IsAttackable && m_aggression[living] > maxAmount)
                            {
                                maxAmount = m_aggression[living];
                                primaryTarget = living;
                            }
                        }
                    }

                    return primaryTarget;
                }
            }
        }

        #endregion
    }
}
