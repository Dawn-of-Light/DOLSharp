using System;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using log4net;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.Keeps
{
	public class GuardLord : GameKeepGuard
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private eRealm m_lastRealm = eRealm.None;
		private long m_lastKillTime = 0;

		public override double GetArmorAbsorb(eArmorSlot slot)
		{
			return base.GetArmorAbsorb(slot) + 0.05;
		}

		public override int MaxHealth => base.MaxHealth * 3;

		public override int RealmPointsValue
		{
			get
			{
				// PvE Lords drop stacks of dreaded seals instead of giving RP directly
                if (Realm == eRealm.None && GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvE)
                    return 0;

                long duration = (CurrentRegion.Time - m_lastKillTime) / 1000L;

				if (duration < Properties.LORD_RP_WORTH_SECONDS)
				{
					return 0;
				}

				if (Component == null || Component.Keep == null)
				{
					return 5000;
				}
				else
				{
					return Component.Keep.RealmPointsValue();
				}
			}
		}

		public override int BountyPointsValue
		{
			get
			{
				// PvE Lords drop stacks of dreaded seals instead of giving RP directly
				if (Realm == eRealm.None && GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvE)
					return 0;

				long duration = (CurrentRegion.Time - m_lastKillTime) / 1000L;
				if (duration < Properties.LORD_RP_WORTH_SECONDS)
				{
					return 0;
				}

				if (Component != null && Component.Keep != null)
				{
					return Component.Keep.BountyPointsValue();
				}

				return base.BountyPointsValue;
			}
		}

		public override long ExperienceValue
		{
			get
			{
				long duration = (CurrentRegion.Time - m_lastKillTime) / 1000L;
				if (duration < Properties.LORD_RP_WORTH_SECONDS)
				{
					return 0;
				}

				if (Component != null && Component.Keep != null)
				{
					return Component.Keep.ExperiencePointsValue();
				}

				return base.ExperienceValue;
			}
		}

		public override double ExceedXPCapAmount
		{
			get
			{
				if (Component != null && Component.Keep != null)
				{
					return Component.Keep.ExceedXPCapAmount();
				}

				return base.ExceedXPCapAmount;
			}
		}

		public override long MoneyValue
		{
			get
			{
				long duration = (CurrentRegion.Time - m_lastKillTime) / 1000L;
				if (duration < Properties.LORD_RP_WORTH_SECONDS)
				{
					return 0;
				}

				if (Component != null && Component.Keep != null)
				{
					return Component.Keep.MoneyValue();
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
		/// Keep lords are narcissitic; they don't assist themselves or anyone else
		/// </summary>
		/// <param name="lord"></param>
		/// <returns>Whether or not we are responding</returns>
		public override bool AssistLord(GuardLord lord)
		{
			return false;
		}

		/// <summary>
		/// When Lord dies, we update Area Mgr to call the various functions we need
		/// And update the player stats
		/// </summary>
		/// <param name="killer">The killer object</param>
		public override void Die(GameObject killer)
		{
			m_lastRealm = eRealm.None;

			if (Properties.LOG_KEEP_CAPTURES)
			{
				try
				{
					if (this.Component != null)
					{
						Database.KeepCaptureLog keeplog = new Database.KeepCaptureLog();
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

			if (Component != null)
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

			if (Component == null)
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
			int distance;
			if (Component != null && Component.Keep != null && Component.Keep is GameKeep)
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

			if (attacker != null && Component != null && Component.Keep != null && IsAlive && !GameServer.ServerRules.IsSameRealm(this, attacker, true))
			{
				if (Realm == m_lastRealm && m_lastRealm != eRealm.None)
					Component.Keep.LastAttackedByEnemyTick = CurrentRegion.Time; // light up the keep/tower
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
                        if (PlayerMgr.IsAllowedToInteract(player, Component.Keep, eInteractType.Claim))
                        {
                            player.Out.SendDialogBox(eDialogCode.KeepClaim, (ushort)player.ObjectID, 0, 0, 0, eDialogType.YesNo, false, "Do you wish to claim\n" + Component.Keep.Name + "?");
                            return true;
                        }
                        break;
                    }
                case "Release Keep":
                    {
                        if (PlayerMgr.IsAllowedToInteract(player, Component.Keep, eInteractType.Release))
                        {
                            flag += 4;
                        }
                        break;
                    }
            }
            if (flag > 0)
                player.Out.SendKeepClaim(Component.Keep, flag);

            return true;
        }

		protected override CharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return CharacterClass.Armsman;
			else if (ModelRealm == eRealm.Midgard) return CharacterClass.Warrior;
			else if (ModelRealm == eRealm.Hibernia) return CharacterClass.Hero;
			return CharacterClass.None;
		}

		protected override void SetBlockEvadeParryChance()
		{
			base.SetBlockEvadeParryChance();

			BlockChance = 15;
			ParryChance = 15;

			if (ModelRealm != eRealm.Albion)
			{
				EvadeChance = 10;
				ParryChance = 5;
			}
		}

		protected override KeepGuardBrain GetBrain() => new LordBrain();

		protected override void SetStats()
		{
			Strength = (short)(Properties.LORD_AUTOSET_STR_BASE + (10 * Level * Properties.LORD_AUTOSET_STR_MULTIPLIER));
			Dexterity = (short)(Properties.LORD_AUTOSET_DEX_BASE + (Level * Properties.LORD_AUTOSET_DEX_MULTIPLIER));
			Constitution = (short)(Properties.LORD_AUTOSET_CON_BASE + (Level * Properties.LORD_AUTOSET_CON_MULTIPLIER));
			Quickness = (short)(Properties.LORD_AUTOSET_QUI_BASE + (Level * Properties.LORD_AUTOSET_QUI_MULTIPLIER));
			Intelligence = (short)(Properties.LORD_AUTOSET_INT_BASE + (Level * Properties.LORD_AUTOSET_INT_MULTIPLIER));
		}

		protected override void SetRespawnTime()
		{
			if (Component != null)
			{
				RespawnInterval = Component.Keep.LordRespawnTime;
			}
			else
			{
				if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvE
					|| GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
				{
					// In PvE & PvP servers, lords are really just mobs farmed for seals.
					int iVariance = 1000 * Math.Abs(Properties.GUARD_RESPAWN_VARIANCE);
					int iRespawn = 60 * ((Math.Abs(Properties.GUARD_RESPAWN) * 1000) +
						(Util.Random(-iVariance, iVariance)));

					RespawnInterval = (iRespawn > 1000) ? iRespawn : 1000; // Make sure we don't end up with an impossibly low respawn interval.
				}
				else
					RespawnInterval = 10000; // 10 seconds
			}
		}

		protected override void SetSpeed()
		{
			base.SetSpeed();
			if (Component != null)
			{
				MaxSpeedBase = 0;
			}
		}

		private string GetKeepShortName(string KeepName)
		{
			string ShortName;
			if (KeepName.StartsWith("Caer"))//Albion
			{
				ShortName = KeepName.Substring(5);
			}
			else if (KeepName.StartsWith("Fort"))
			{
				ShortName = KeepName.Substring(5);
			}
			else if (KeepName.StartsWith("Dun"))//Hibernia
			{
				if (KeepName == "Dun nGed")
				{
					ShortName = "Ged";
				}
				else if (KeepName == "Dun da Behn")
				{
					ShortName = "Behn";
				}
				else
				{
					ShortName = KeepName.Substring(4);
				}
			}
			else if (KeepName.StartsWith("Castle"))// Albion Relic
			{
				ShortName = KeepName.Substring(7);
			}
			else//Midgard
			{
				if (KeepName.Contains(" "))
					ShortName = KeepName.Substring(0, KeepName.IndexOf(" ", 0));
				else
					ShortName = KeepName;
			}
			return ShortName;
		}

		protected override void SetName()
		{
			if (Component == null)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Commander", CurrentZone.Description);
				return;
			}
			else if (IsTowerGuard)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.TowerCaptain");
				return;
			}

			switch (ModelRealm)
			{
				case eRealm.None:
				case eRealm.Albion:
					if (Gender == eGender.Male)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Lord", GetKeepShortName(Component.Keep.Name));
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Lady", GetKeepShortName(Component.Keep.Name));
					break;
				case eRealm.Midgard:
					Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Jarl", GetKeepShortName(Component.Keep.Name));
					break;
				case eRealm.Hibernia:
					if (Gender == eGender.Male)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Chieftain", GetKeepShortName(Component.Keep.Name));
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Chieftess", GetKeepShortName(Component.Keep.Name));
					break;
			}

			if (Realm == eRealm.None)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Renegade", Name);
			}
		}
	}
}
