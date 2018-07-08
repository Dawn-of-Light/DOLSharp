using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.Effects;
using DOL.AI.Brain;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    ///
    /// </summary>
    public class DivineInterventionEffect : TimedEffect
    {
        private const int PoolDuration = 1200000; // 20 minutes

        public DivineInterventionEffect(int value)
            : base(PoolDuration)
        {
            _poolValue = value;
        }

        private readonly ArrayList _affected = new ArrayList();
        private Group _group;
        private int _poolValue;
        private GamePlayer _playerOwner;

        /// <summary>
        /// Start the effect on a living target
        /// </summary>
        /// <param name="living"></param>
        public override void Start(GameLiving living)
        {
            _playerOwner = living as GamePlayer;

            if (_playerOwner == null)
            {
                return;
            }

            _group = _playerOwner.Group;

            if (_group == null)
            {
                return;
            }

            base.Start(living);

            _playerOwner.Out.SendMessage("You group is protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            GameEventMgr.AddHandler(_group, GroupEvent.MemberJoined, new DOLEventHandler(PlayerJoinedGroup));
            GameEventMgr.AddHandler(_group, GroupEvent.MemberDisbanded, new DOLEventHandler(PlayerDisbandedGroup));

            foreach (GamePlayer gp in _group.GetPlayersInTheGroup())
            {
                foreach (GamePlayer p in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (!p.IsAlive)
                    {
                        continue;
                    }

                    p.Out.SendSpellEffectAnimation(living, gp, 7036, 0, false, 1);
                }

                if (gp == _playerOwner)
                {
                    continue;
                }

                gp.Out.SendMessage("You are protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                _affected.Add(gp);
                GameEventMgr.AddHandler(gp, GameObjectEvent.TakeDamage, new DOLEventHandler(TakeDamage));
                if (gp.CharacterClass.ID == (int)eCharacterClass.Necromancer)
                {
                    if (gp.ControlledBrain != null)
                    {
                        _affected.Add(gp.ControlledBrain.Body);
                        GameEventMgr.AddHandler(gp.ControlledBrain.Body, GameLivingEvent.TakeDamage, new DOLEventHandler(TakeDamageNPC));
                    }
                }
            }
        }

        protected void PlayerJoinedGroup(DOLEvent e, object sender, EventArgs args)
        {
            if (!(args is MemberJoinedEventArgs pjargs))
            {
                return;
            }

            _affected.Add(pjargs.Member);
            GameEventMgr.AddHandler(pjargs.Member, GameObjectEvent.TakeDamage, new DOLEventHandler(TakeDamage));

            if (pjargs.Member is GamePlayer player)
            {
                player.Out.SendMessage("You are protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                if (player.CharacterClass.ID == (int)eCharacterClass.Necromancer)
                {
                    if (player.ControlledBrain != null)
                    {
                        _affected.Add(player.ControlledBrain.Body);
                        GameEventMgr.AddHandler(player.ControlledBrain.Body, GameObjectEvent.TakeDamage, new DOLEventHandler(TakeDamageNPC));
                    }
                }
            }
        }

        protected void PlayerDisbandedGroup(DOLEvent e, object sender, EventArgs args)
        {
            if (!(args is MemberDisbandedEventArgs pdargs))
            {
                return;
            }

            _affected.Remove(pdargs.Member);
            GameEventMgr.RemoveHandler(pdargs.Member, GameObjectEvent.TakeDamage, new DOLEventHandler(TakeDamage));
            if (pdargs.Member is GamePlayer player)
            {
                player.Out.SendMessage("You are no longer protected by a pool of healing!", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                if (player.CharacterClass.ID == (int)eCharacterClass.Necromancer)
                {
                    if (player.ControlledBrain != null)
                    {
                        _affected.Remove(player.ControlledBrain.Body);
                        GameEventMgr.RemoveHandler(player.ControlledBrain.Body, GameObjectEvent.TakeDamage, new DOLEventHandler(TakeDamageNPC));
                    }
                }
            }

            if (_group == null)
            {
                Cancel(false);
            }
        }

        protected void TakeDamageNPC(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC npc = sender as GameNPC;

            if (!npc.IsWithinRadius(m_owner, 2300))
            {
                return;
            }

            if (!npc.IsAlive)
            {
                return;
            }

            int dmgamount = npc.MaxHealth - npc.Health;

            if (dmgamount <= 0 || npc.HealthPercent >= 75)
            {
                return;
            }

            int healamount;

            if (_poolValue <= 0)
            {
                Cancel(false);
            }

            if (_poolValue - dmgamount > 0)
            {
                healamount = dmgamount;
            }
            else
            {
                healamount = dmgamount - _poolValue;
            }

            foreach (GamePlayer tPlayer in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (!tPlayer.IsAlive)
                {
                    continue;
                }

                tPlayer.Out.SendSpellEffectAnimation(m_owner, npc, 8051, 0, false, 1);
            }

            var petOwner = (npc.Brain as IControlledBrain)?.Owner as GamePlayer;
            petOwner?.Out.SendMessage($"Your {npc.Name} was healed by the pool of healing for {healamount}!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            _playerOwner.Out.SendMessage($"Your pool of healing heals the {npc.Name} of {petOwner?.Name} for {healamount}!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

            npc.ChangeHealth(m_owner, GameLiving.eHealthChangeType.Spell, healamount);
            _poolValue -= dmgamount;

            if (_poolValue <= 0)
            {
                Cancel(false);
            }
        }

        protected void TakeDamage(DOLEvent e, object sender, EventArgs args)
        {
            TakeDamageEventArgs targs = args as TakeDamageEventArgs;

            if (!(sender is GamePlayer player) || !player.IsWithinRadius(m_owner, 2300))
            {
                return;
            }

            if (targs == null || targs.DamageType == eDamageType.Falling)
            {
                return;
            }

            if (!player.IsAlive)
            {
                return;
            }

            int dmgamount = player.MaxHealth - player.Health;

            if (dmgamount <= 0 || player.HealthPercent >= 75)
            {
                return;
            }

            int healamount;

            if (_poolValue <= 0)
            {
                Cancel(false);
            }

            if (!player.IsAlive)
            {
                return;
            }

            if (_poolValue - dmgamount > 0)
            {
                healamount = dmgamount;
            }
            else
            {
                healamount = dmgamount - _poolValue;
            }

            foreach (GamePlayer tPlayer in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (!tPlayer.IsAlive)
                {
                    continue;
                }

                tPlayer.Out.SendSpellEffectAnimation(m_owner, player, 8051, 0, false, 1);
            }

            player.Out.SendMessage($"You are healed by the pool of healing for {healamount}!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            _playerOwner.Out.SendMessage($"Your pool of healing heals {player.Name} for {healamount}!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            player.ChangeHealth(m_owner, GameLiving.eHealthChangeType.Spell, healamount);
            _poolValue -= dmgamount;

            if (_poolValue <= 0)
            {
                Cancel(false);
            }
        }

        public override void Stop()
        {
            base.Stop();
            if (_group != null)
            {
                GameEventMgr.RemoveHandler(_group, GroupEvent.MemberDisbanded, new DOLEventHandler(PlayerDisbandedGroup));
                GameEventMgr.RemoveHandler(_group, GroupEvent.MemberJoined, new DOLEventHandler(PlayerJoinedGroup));
            }

            foreach (GameLiving l in _affected)
            {
                if (l is GamePlayer)
                {
                    (l as GamePlayer).Out.SendMessage("You are no longer protected by a pool of healing!", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                    GameEventMgr.RemoveHandler(l, GameObjectEvent.TakeDamage, new DOLEventHandler(TakeDamage));
                }
                else
                {
                    GameEventMgr.RemoveHandler(l, GameObjectEvent.TakeDamage, new DOLEventHandler(TakeDamageNPC));
                }
            }

            _affected.Clear();
            _group = null;
        }

        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name => "Divine Intervention";

        /// <summary>
        /// Icon to show on players, can be id
        /// </summary>
        public override ushort Icon => 3035;

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList<string> DelveInfo
        {
            get
            {
                var delveInfoList = new List<string>
                {
                    "This ability creates a pool of healing on the user, instantly healing any member of the caster's group when they go below 75% hp until it is used up.",
                    " ",
                    "Target: Group",
                    "Duration: 15:00 min",
                    " ",
                    $"Value: {_poolValue}"
                };

                int seconds = RemainingTime / 1000;
                if (seconds > 0)
                {
                    delveInfoList.Add(" ");
                    delveInfoList.Add(seconds > 60
                        ? $"- {seconds / 60}:{seconds % 60:00} minutes remaining."
                        : $"- {seconds} seconds remaining.");
                }

                return delveInfoList;
            }
        }
    }
}
