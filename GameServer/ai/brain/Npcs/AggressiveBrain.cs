using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;

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

            if (IsEngaged)
                PickTarget();
            else
                CheckForEnemies();
        }

        /// <summary>
        /// Pick the next target.
        /// </summary>
        private void PickTarget()
        {
            GameLiving primaryTarget = Aggression.PrimaryTarget;

            if (primaryTarget != Body.TargetObject)
                SwitchTarget(primaryTarget);
        }

        /// <summary>
        /// Switch the target.
        /// </summary>
        /// <param name="living"></param>
        private void SwitchTarget(GameLiving living)
        {
            Body.StartAttack(living);
        }

        /// <summary>
        /// Check for living objects within a certain radius to 
        /// aggro on.
        /// </summary>
        private void CheckForEnemies()
        {
            GamePlayer player = PickPlayerInRadius();

            if (player != null)
            {
                Aggression.Raise(player, InternalAggression.Min);

                // TODO: If player is grouped, add remaining players
                //       from the group as well.
            }
        }

        public ushort AggroLevel { get; set; }

        protected virtual ushort AggroRange
        {
            get { return 500; }
        }

        private GamePlayer PickPlayerInRadius()
        {
            foreach (GamePlayer player in Body.GetPlayersInRadius(AggroRange))
                if (Util.Chance(AggroLevel))
                    return player;

            return null;
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
                Aggression.Raise(source as GameLiving, InternalAggression.Min);

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

            Aggression.Clear(living);
        }

        private const int TauntAggressionAmount = 1000;

        /// <summary>
        /// The NPC has been taunted (spell).
        /// </summary>
        /// <param name="attackData"></param>
        protected virtual void OnTaunted(AttackData attackData)
        {
            GamePlayer player = attackData.Attacker as GamePlayer;

            if (player != null)
            {
                player.Out.SendMessage(String.Format("Taunted{0}", attackData.IsSpellResisted
                    ? ", but taunt was resisted!" : ""), eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
            }

            Aggression.Raise(attackData.Attacker, attackData.IsSpellResisted
                ? 0 : TauntAggressionAmount);
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
        public bool IsEnemy(GameLiving living)
        {
            return (living == null)
                ? false
                : (Aggression.GetAmountForLiving(living) > 0);
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
            public long GetAmountForLiving(GameLiving living)
            {
                lock (m_syncObject)
                {
                    return m_aggression.ContainsKey(living)
                        ? m_aggression[living]
                        : 0;
                }
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
            /// The minimum amount of aggression possible.
            /// </summary>
            public const int Min = 100;

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
                        m_aggression.Add(living, (amount < Min) ? Min : amount);

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
                        m_aggression[living] = (current >= amount + Min) ? current - amount : Min;
                    }
                }
            }

            /// <summary>
            /// Remove a living from the list.
            /// </summary>
            /// <param name="living"></param>
            public void Clear(GameLiving living)
            {
                lock (m_syncObject)
                {
                    if (living != null && m_aggression.ContainsKey(living))
                        m_aggression.Remove(living);
                }
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
