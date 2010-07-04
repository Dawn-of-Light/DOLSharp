using System;
using DOL.Events;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Class for the Lord Guard
	/// </summary>
	public class GuardLord : GameKeepGuard
	{
		private eRealm m_lastRealm = eRealm.None;
		private long m_lastKillTime = 0;

		/// <summary>
		/// Lord needs more health at the moment
		/// </summary>
		public override int MaxHealth
		{
			get
			{
				return base.MaxHealth * 3;
			}
		}

		public override int RealmPointsValue
		{
			get
			{
				long duration = (CurrentRegion.Time - m_lastKillTime) / 1000L;

				if (duration < ServerProperties.Properties.LORD_RP_WORTH_SECONDS)
				{
					return 0;
				}

				if (this.Component == null || this.Component.Keep == null)
				{
					return 5000;
				}
				else
				{
					return this.Component.Keep.RealmPointsValue();
				}
			}
		}

		public override int BountyPointsValue
		{
			get
			{
				long duration = (CurrentRegion.Time - m_lastKillTime) / 1000L;
				if (duration < ServerProperties.Properties.LORD_RP_WORTH_SECONDS)
				{
					return 0;
				}

				if (this.Component != null && this.Component.Keep != null)
				{
					return this.Component.Keep.BountyPointsValue();
				}

				return base.BountyPointsValue;
			}
		}

		public override long ExperienceValue
		{
			get
			{
				long duration = (CurrentRegion.Time - m_lastKillTime) / 1000L;
				if (duration < ServerProperties.Properties.LORD_RP_WORTH_SECONDS)
				{
					return 0;
				}

				if (this.Component != null && this.Component.Keep != null)
				{
					return this.Component.Keep.ExperiencePointsValue();
				}

				return base.ExperienceValue;
			}
		}

		public override double ExceedXPCapAmount
		{
			get
			{
				if (this.Component != null && this.Component.Keep != null)
				{
					return this.Component.Keep.ExceedXPCapAmount();
				}

				return base.ExceedXPCapAmount;
			}
		}

		public override long MoneyValue
		{
			get
			{
				long duration = (CurrentRegion.Time - m_lastKillTime) / 1000L;
				if (duration < ServerProperties.Properties.LORD_RP_WORTH_SECONDS)
				{
					return 0;
				}

				if (this.Component != null && this.Component.Keep != null)
				{
					return this.Component.Keep.MoneyValue();
				}

				return base.MoneyValue;
			}
		}


		public override int AttackRangeDistance
		{
			get
			{
				return 1200;
			}
		}

		/// <summary>
		/// When Lord dies, we update Area Mgr to call the various functions we need
		/// And update the player stats
		/// </summary>
		/// <param name="killer">The killer object</param>
		public override void Die(GameObject killer)
		{
			m_lastRealm = eRealm.None;

			if (ServerProperties.Properties.LOG_KEEP_CAPTURES)
			{
				try
				{
					if (this.Component != null)
					{
						DOL.Database.KeepCaptureLog keeplog = new DOL.Database.KeepCaptureLog();
						keeplog.KeepName = Component.Keep.Name;

						if (Component.Keep is GameKeep)
							keeplog.KeepType = "Keep";
						else
							keeplog.KeepType = "Tower";

						keeplog.NumEnemies = GetEnemyCountInArea();
						keeplog.RPReward = RealmPointsValue;
						keeplog.BPReward = BountyPointsValue;
						keeplog.XPReward = ExperienceValue;
						keeplog.MoneyReward = MoneyValue;

						if (Component.Keep.StartCombatTick > 0)
						{
							keeplog.CombatTime = (int)((Component.Keep.CurrentRegion.Time - Component.Keep.StartCombatTick) / 1000 / 60);
						}

						keeplog.CapturedBy = GlobalConstants.RealmToName(killer.Realm);

						string listRPGainers = "";

						foreach (System.Collections.DictionaryEntry de in XPGainers)
						{
							GameLiving living = de.Key as GameLiving;
							if (living != null)
							{
								listRPGainers += living.Name + ";";
							}
						}

						keeplog.RPGainerList = listRPGainers.TrimEnd(';');

						GameServer.Database.AddObject(keeplog);
					}
					else
					{
						log.Error("Component null for Guard Lord " + Name);
					}
				}
				catch (System.Exception ex)
				{
					log.Error("KeepCaptureLog Exception", ex);
				}
			}

			base.Die(killer);

			if (this.Component != null)
			{
				GameServer.ServerRules.ResetKeep(this, killer);
			}

			m_lastKillTime = CurrentRegion.Time;
		}

		/// <summary>
		/// When we interact with lord, we display all possible options
		/// </summary>
		/// <param name="player">The player object</param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (this.Component == null)
				return false;

			if (InCombat || Component.Keep.InCombat)
			{
				player.Out.SendMessage("You can't talk to the lord while under siege.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				log.DebugFormat("KEEPWARNING: {0} attempted to interact with {1} of {2} while keep or lord in combat.", player.Name, Name, Component.Keep.Name);
				return false;
			}

			if (GameServer.ServerRules.IsAllowedToClaim(player, CurrentRegion))
			{
				player.Out.SendMessage("Would you like to [Claim Keep] now? Or maybe [Release Keep]?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}

            return true;
		}

		public override bool AddToWorld()
		{
			if (base.AddToWorld())
			{
				m_lastRealm = Realm;
				return true;
			}

			return false;
		}


		/// <summary>
		/// From a great distance, damage does not harm lord
		/// </summary>
		/// <param name="source">The source of the damage</param>
		/// <param name="damageType">The type of the damage</param>
		/// <param name="damageAmount">The amount of the damage</param>
		/// <param name="criticalAmount">The critical hit amount of damage</param>
		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			int distance = 0;
			if (this.Component != null && this.Component.Keep != null && this.Component.Keep is GameKeep)
				distance = 400;
			else 
				distance = 300;

			// check to make sure pets and pet casters are in range
			GamePlayer attacker = null;
			if (source is GamePlayer)
			{
				attacker = source as GamePlayer;
			}
			else if (source is GameNPC && (source as GameNPC).Brain != null && (source as GameNPC).Brain is IControlledBrain && (((source as GameNPC).Brain as IControlledBrain).Owner) is GamePlayer)
			{
				attacker = ((source as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;
			}

			if ((attacker != null && IsWithinRadius(attacker, distance) == false) || IsWithinRadius(source, distance) == false)
			{
				if (attacker != null)
					attacker.Out.SendMessage(this.Name + " is immune to damage from this range", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}

			if (attacker != null && this.Component != null && this.Component.Keep != null && IsAlive && !GameServer.ServerRules.IsSameRealm(this, attacker, true))
			{
				if (Realm == m_lastRealm && m_lastRealm != eRealm.None)
					this.Component.Keep.LastAttackedByEnemyTick = CurrentRegion.Time; // light up the keep/tower
			}

			base.TakeDamage(source, damageType, damageAmount, criticalAmount);
		}

        public override bool WhisperReceive(GameLiving source, string str)
        {
			if (InCombat) return false;
			if (Component == null) return false;
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer player = (GamePlayer)source;

			if (!GameServer.ServerRules.IsSameRealm(this, player, true) || !GameServer.ServerRules.IsAllowedToClaim(player, CurrentRegion))
			{
				return false;
			}

            byte flag = 0;
            switch (str)
            {
                case "Claim Keep":
                    {
                        if (PlayerMgr.IsAllowedToInteract(player, this.Component.Keep, eInteractType.Claim))
                        {
                            player.Out.SendDialogBox(eDialogCode.KeepClaim, (ushort)player.ObjectID, 0, 0, 0, eDialogType.YesNo, false, "Do you wish to claim\n" + this.Component.Keep.Name + "?");
                            return true;
                        }
                        break;
                    }
                case "Release Keep":
                    {
                        if (PlayerMgr.IsAllowedToInteract(player, this.Component.Keep, eInteractType.Release))
                        {
                            flag += 4;
                        }
                        break;
                    }
            }
            if (flag > 0)
                player.Out.SendKeepClaim(this.Component.Keep, flag);

            return true;
        }
    }
}
