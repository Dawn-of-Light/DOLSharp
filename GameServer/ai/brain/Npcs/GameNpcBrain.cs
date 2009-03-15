using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.Events;

namespace DOL.AI.Brain
{
    /// <summary>
    /// Standard NPC brain implementation (complete rewrite).
    /// We derive from APlayerVicinityBrain to save CPU in case there
    /// aren't any players around anyway.
    /// </summary>
    /// <author>Aredhel</author>
    public class GameNpcBrain : APlayerVicinityBrain, IAggressiveBrain
    {
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
            if (Body.IsIncapacitated || Body.IsReturningHome)
                return;


        }

        #region Notify handlers.

        /// <summary>
        /// Process messages coming from the body.
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
                    
                    if (HateList.IsEnemy(healed.Enemy))
                        OnEnemyHealed(healed.HealSource, healed.HealAmount);
                }

                return;
            }

            if (e == GameLivingEvent.EnemyKilled)
            {
                if (args is EnemyKilledEventArgs)
                {
                    EnemyKilledEventArgs killed = args as EnemyKilledEventArgs;

                    if (HateList.IsEnemy(killed.Target))
                        OnEnemyKilled(killed.Target);
                }
            }
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

            OnTaunted(attackData.Attacker, HateList.InitialAggro);

            // TODO: Process attack data and change the amount of aggro
            //       accordingly.
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
                OnTaunted(source as GameLiving, HateList.InitialAggro);

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

            HateList.Release(living);
        }

        /// <summary>
        /// Move living up in the hate list.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="amount"></param>
        protected void OnTaunted(GameLiving living, long amount)
        {
            HateList.OnTaunted(living, amount);

            GameLiving primaryTarget = HateList.PrimaryTarget;

            if (primaryTarget != Body.TargetObject)
                SwitchTarget(primaryTarget);
        }

        /// <summary>
        /// Move living down in the hate list.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="amount"></param>
        protected void OnDetaunted(GameLiving living, long amount)
        {
            HateList.OnDetaunted(living, amount);

            GameLiving primaryTarget = HateList.PrimaryTarget;

            if (primaryTarget != Body.TargetObject)
                SwitchTarget(primaryTarget);
        }

        /// <summary>
        /// Switch the target.
        /// </summary>
        /// <param name="living"></param>
        protected void SwitchTarget(GameLiving living)
        {
        }

        #endregion

        #region Hate list.

        /// <summary>
        /// The initial aggro amount when this NPC is pulled, regardless of 
        /// how much damage has been inflicted.
        /// </summary>

        private InternalHateList m_hateList;
        
        /// <summary>
        /// This brain's hate list.
        /// </summary>
        private InternalHateList HateList
        {
            get
            {
                if (m_hateList == null)
                    m_hateList = new InternalHateList();

                return m_hateList;
            }
        }

        /// <summary>
        /// Encapsulated in order to achieve thread safety.
        /// </summary>
        private class InternalHateList
        {
            private IDictionary<GameLiving, long> m_amount = new Dictionary<GameLiving, long>();
            private object m_syncObject = new object();

            /// <summary>
            /// The initial amount of aggro generated without actually
            /// inflicting damage on an NPC. No matter how much detaunting
            /// is done, you can never drop below this value (unless you die,
            /// that is).
            /// </summary>
            public long InitialAggro
            {
                get { return 100; }
            }

            /// <summary>
            /// Increase hate by the given amount.
            /// </summary>
            /// <param name="living"></param>
            /// <param name="amount"></param>
            public void OnTaunted(GameLiving living, long amount)
            {
                if (living == null || amount <= 0)
                    return;

                lock (m_syncObject)
                {
                    if (m_amount.ContainsKey(living))
                        m_amount[living] += amount;
                    else
                        m_amount.Add(living, amount);
                }
            }

            /// <summary>
            /// Lower hate by the given amount, but not past the initial
            /// aggro cap.
            /// </summary>
            /// <param name="living"></param>
            /// <param name="amount"></param>
            public void OnDetaunted(GameLiving living, long amount)
            {
                if (living == null || amount <= 0)
                    return;

                lock (m_syncObject)
                {
                    if (m_amount.ContainsKey(living))
                    {
                        long current = m_amount[living];
                        m_amount[living] = (current >= amount + InitialAggro) ? current - amount : InitialAggro;
                    }
                }
            }

            /// <summary>
            /// Remove a living from the list.
            /// </summary>
            /// <param name="living"></param>
            public void Release(GameLiving living)
            {
                lock (m_syncObject)
                {
                    if (living != null && m_amount.ContainsKey(living))
                        m_amount.Remove(living);
                }
            }

            /// <summary>
            /// The living that has accumulated the most hate.
            /// </summary>
            public GameLiving PrimaryTarget
            {
                get
                {
                    GameLiving primaryTarget = null;
                    long maxAmount = 0;

                    lock (m_syncObject)
                    {
                        foreach (GameLiving living in m_amount.Keys)
                        {
                            if (m_amount[living] > maxAmount)
                            {
                                maxAmount = m_amount[living];
                                primaryTarget = living;
                            }
                        }
                    }

                    return primaryTarget;
                }
            }

            /// <summary>
            /// Check whether or not this living is an enemy.
            /// </summary>
            /// <param name="living"></param>
            /// <returns></returns>
            public bool IsEnemy(GameLiving living)
            {
                if (living == null)
                    return false;

                lock (m_syncObject)
                {
                    return m_amount.ContainsKey(living);
                }
            }
        }

        #endregion
    }
}
