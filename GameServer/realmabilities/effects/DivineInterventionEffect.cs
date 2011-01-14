using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Events;
using DOL.GS.Effects;
using DOL.AI.Brain;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// 
	/// </summary>
	public class DivineInterventionEffect : TimedEffect, IGameEffect
	{
		public DivineInterventionEffect(int value)
			: base(DivineInterventionAbility.poolDuration)
		{
			PoolValue = value;
		}

		private Group m_group = null;
		public int PoolValue = 0;
		private ArrayList m_affected = new ArrayList();
        private GamePlayer m_playerOwner = null;

		/// <summary>
		/// Start the effect on a living target
		/// </summary>
		/// <param name="living"></param>
		public override void Start(GameLiving living)
		{
			m_playerOwner = living as GamePlayer;

			if (m_playerOwner == null)
				return;

			m_group = m_playerOwner.Group;

			if (m_group == null)
				return;

			base.Start(living);

			m_playerOwner.Out.SendMessage("You group is protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			GameEventMgr.AddHandler(m_group, GroupEvent.MemberJoined, new DOLEventHandler(PlayerJoinedGroup));
			GameEventMgr.AddHandler(m_group, GroupEvent.MemberDisbanded, new DOLEventHandler(PlayerDisbandedGroup));

			foreach (GamePlayer gp in m_group.GetPlayersInTheGroup())
			{
				foreach (GamePlayer p in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if(!p.IsAlive) continue;
					p.Out.SendSpellEffectAnimation(living, gp, 7036, 0, false, 1);
				}

				if (gp == m_playerOwner) 
					continue;

				gp.Out.SendMessage("You are protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				m_affected.Add(gp);
				GameEventMgr.AddHandler(gp, GamePlayerEvent.TakeDamage, new DOLEventHandler(TakeDamage));
                if (gp.CharacterClass.ID == (int)eCharacterClass.Necromancer)
                {
                    if (gp.ControlledBrain != null)
                    {
                        m_affected.Add(gp.ControlledBrain.Body);
                        GameEventMgr.AddHandler(gp.ControlledBrain.Body, GameLivingEvent.TakeDamage, new DOLEventHandler(TakeDamageNPC));
                    }
                }
			}
		}

		protected void PlayerJoinedGroup(DOLEvent e, object sender, EventArgs args)
		{
			MemberJoinedEventArgs pjargs = args as MemberJoinedEventArgs;
			if (pjargs == null) return;
			m_affected.Add(pjargs.Member);
			GameEventMgr.AddHandler(pjargs.Member, GamePlayerEvent.TakeDamage, new DOLEventHandler(TakeDamage));

            if (pjargs.Member is GamePlayer)
            {
                ((GamePlayer)pjargs.Member).Out.SendMessage("You are protected by a pool of healing!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                if (((GamePlayer)pjargs.Member).CharacterClass.ID == (int)eCharacterClass.Necromancer)
                {
                    if (((GamePlayer)pjargs.Member).ControlledBrain != null)
                    {
                        m_affected.Add(((GamePlayer)pjargs.Member).ControlledBrain.Body);
                        GameEventMgr.AddHandler(((GamePlayer)pjargs.Member).ControlledBrain.Body, GameLivingEvent.TakeDamage, new DOLEventHandler(TakeDamageNPC));
                    }
                }
            }
		}

		protected void PlayerDisbandedGroup(DOLEvent e, object sender, EventArgs args)
		{
			MemberDisbandedEventArgs pdargs = args as MemberDisbandedEventArgs;
			if (pdargs == null) return;
			m_affected.Remove(pdargs.Member);
			GameEventMgr.RemoveHandler(pdargs.Member, GamePlayerEvent.TakeDamage, new DOLEventHandler(TakeDamage));
            if (pdargs.Member is GamePlayer)
            {
                ((GamePlayer)pdargs.Member).Out.SendMessage("You are no longer protected by a pool of healing!", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                if (((GamePlayer)pdargs.Member).CharacterClass.ID == (int)eCharacterClass.Necromancer)
                {
                    if (((GamePlayer)pdargs.Member).ControlledBrain != null)
                    {
                        m_affected.Remove(((GamePlayer)pdargs.Member).ControlledBrain.Body);
                        GameEventMgr.RemoveHandler(((GamePlayer)pdargs.Member).ControlledBrain.Body, GameLivingEvent.TakeDamage, new DOLEventHandler(TakeDamageNPC));
                    }
                }
            }

			if (m_group == null)
				Cancel(false);
		}

        protected void TakeDamageNPC(DOLEvent e, object sender, EventArgs args)
        {
            TakeDamageEventArgs targs = args as TakeDamageEventArgs;
            GameNPC npc = sender as GameNPC;

            if (!npc.IsWithinRadius(m_owner, 2300))
                return;

            if (!npc.IsAlive) return;

            int dmgamount = npc.MaxHealth - npc.Health;

            if (dmgamount <= 0 || npc.HealthPercent >= 75) return;

            int healamount = 0;


            if (PoolValue <= 0)
                Cancel(false);

            if (PoolValue - dmgamount > 0)
            {
                healamount = dmgamount;
            }
            else
            {
                healamount = dmgamount - PoolValue;
            }
            foreach (GamePlayer t_player in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                if (!t_player.IsAlive) continue;
                t_player.Out.SendSpellEffectAnimation(m_owner, npc, 8051, 0, false, 1);
            }
            GamePlayer petOwner = null;
            petOwner = ((npc as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;
            if (petOwner != null)
                petOwner.Out.SendMessage("Your " + npc.Name + " was healed by the pool of healing for " + healamount + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

            m_playerOwner.Out.SendMessage("Your pool of healing heals the " + npc.Name + " of " + petOwner.Name + " for " + healamount + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

            npc.ChangeHealth(m_owner, GameLiving.eHealthChangeType.Spell, healamount);
            PoolValue -= dmgamount;

            if (PoolValue <= 0)
                Cancel(false);
        }

		protected void TakeDamage(DOLEvent e, object sender, EventArgs args)
		{
			TakeDamageEventArgs targs = args as TakeDamageEventArgs;
			GamePlayer player = sender as GamePlayer;

			if (!player.IsWithinRadius(m_owner, 2300))
				return;

			if (targs.DamageType == eDamageType.Falling) return;

			if (!player.IsAlive) return;

			int dmgamount = player.MaxHealth - player.Health;

			if (dmgamount <= 0 || player.HealthPercent >= 75) return;

			int healamount = 0;


			if (PoolValue <= 0)
				Cancel(false);

			if (!player.IsAlive)
				return;

			if (PoolValue - dmgamount > 0)
			{
				healamount = dmgamount;
			}
			else
			{
				healamount = dmgamount - PoolValue;
			}
			foreach (GamePlayer t_player in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if(!t_player.IsAlive) continue;
				t_player.Out.SendSpellEffectAnimation(m_owner, player, 8051, 0, false, 1);
			}
			player.Out.SendMessage("You are healed by the pool of healing for " + healamount + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            m_playerOwner.Out.SendMessage("Your pool of healing heals " + player.Name + " for " + healamount + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
			player.ChangeHealth(m_owner, GameLiving.eHealthChangeType.Spell, healamount);
			PoolValue -= dmgamount;

			if (PoolValue <= 0)
				Cancel(false);
		}

		public override void Stop()
		{
			base.Stop();
			if (m_group != null)
			{
				GameEventMgr.RemoveHandler(m_group, GroupEvent.MemberDisbanded, new DOLEventHandler(PlayerDisbandedGroup));
				GameEventMgr.RemoveHandler(m_group, GroupEvent.MemberJoined, new DOLEventHandler(PlayerJoinedGroup));
			}
			foreach (GameLiving l in m_affected)
			{
				if (l is GamePlayer)
				{
					(l as GamePlayer).Out.SendMessage("You are no longer protected by a pool of healing!", eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
					GameEventMgr.RemoveHandler(l, GamePlayerEvent.TakeDamage, new DOLEventHandler(TakeDamage));
				}
				else
				{
					GameEventMgr.RemoveHandler(l, GameLivingEvent.TakeDamage, new DOLEventHandler(TakeDamageNPC));
				}
			}
			m_affected.Clear();
			m_group = null;
		}
		
		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return "Divine Intervention"; } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 3035; } }
		
		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
				var delveInfoList = new List<string>();
				delveInfoList.Add("This ability creates a pool of healing on the user, instantly healing any member of the caster's group when they go below 75% hp until it is used up.");
				delveInfoList.Add(" ");
				delveInfoList.Add("Target: Group");
				delveInfoList.Add("Duration: 15:00 min");
				delveInfoList.Add(" ");
				delveInfoList.Add("Value: " + PoolValue);
				int seconds = (int)(RemainingTime / 1000);
				if (seconds > 0)
				{
					delveInfoList.Add(" ");
					if (seconds > 60)
						delveInfoList.Add("- " + seconds / 60 + ":" + (seconds % 60).ToString("00") + " minutes remaining.");
					else
						delveInfoList.Add("- " + seconds + " seconds remaining.");
				}
				return delveInfoList;
			}
		}
	}
}
