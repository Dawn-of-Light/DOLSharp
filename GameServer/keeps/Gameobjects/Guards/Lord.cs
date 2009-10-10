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
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
				if (this.Component == null || this.Component.Keep == null)
					return 5000;
				else
				{
					int value = 0;

					if (this.Component.Keep is GameKeep)
					{
						value = ServerProperties.Properties.KEEP_RP_BASE + ((this.Component.Keep.BaseLevel - 50) * ServerProperties.Properties.KEEP_RP_MULTIPLIER);
					}
					else
					{
						value = ServerProperties.Properties.TOWER_RP_BASE + ((this.Component.Keep.BaseLevel - 50) * ServerProperties.Properties.TOWER_RP_MULTIPLIER);
					}

					value += (this.Component.Keep.Level * ServerProperties.Properties.UPGRADE_MULTIPLIER);

					return value;
				}
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
					DOL.Database.KeepCaptureLog keeplog = new DOL.Database.KeepCaptureLog();
					keeplog.KeepName = Component.Keep.Name;

					if (Component.Keep is GameKeep)
						keeplog.KeepType = "Keep";
					else
						keeplog.KeepType = "Tower";

					keeplog.NumEnemies = GetEnemyCountInArea();
					keeplog.RPReward = RealmPointsValue;

					if (Component.Keep.StartCombatTick > 0)
						keeplog.CombatTime = (int)((Component.Keep.CurrentRegion.Time - Component.Keep.StartCombatTick) / 1000 / 60);

					keeplog.CapturedBy = GlobalConstants.RealmToName(killer.Realm);

					GameServer.Database.AddNewObject(keeplog);
				}
				catch (System.Exception ex)
				{
					log.Error(ex);
				}
			}

			if (this.Component != null)
				GameServer.ServerRules.ResetKeep(this, killer);

			base.Die(killer);
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


		eRealm m_lastRealm = eRealm.None;

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
