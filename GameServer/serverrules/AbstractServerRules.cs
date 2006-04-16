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
using System.Reflection;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.GS;
using DOL.GS.Loot;
using DOL.GS.PacketHandler;
using System.Net;
using NHibernate.Expression;
using log4net;

namespace DOL.GS.ServerRules
{
	public abstract class AbstractServerRules : IServerRules
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Allows or denies a client from connecting to the server ...
		/// NOTE: The client has not been fully initialized when this method is called.
		/// For example, no account or character data has been loaded yet.
		/// </summary>
		/// <param name="client">The client that sent the login request</param>
		/// <param name="username">The username of the client wanting to connect</param>
		/// <returns>true if connection allowed, false if connection should be terminated</returns>
		/// <remarks>You can only send ONE packet to the client and this is the
		/// LoginDenied packet before returning false. Trying to send any other packet
		/// might result in unexpected behaviour on server and client!</remarks>
		public virtual bool IsAllowedToConnect(GameClient client, string username)
		{
			if (!client.Socket.Connected)
				return false;

			/* Example to limit the connections to a certain client version!
			if(client.Version != GameClient.eClientVersion.Version169)
			{
				client.Out.SendLoginDenied(eLoginError.ClientVersionTooLow);
				return false;
			}
			*/

			/* Example to limit the connections from a certain IP range!
			if(client.Socket.RemoteEndPoint.ToString().StartsWith("192.168.0."))
			{
				client.Out.SendLoginDenied(eLoginError.AccountNoAccessAnyGame);
				return false;
			}
			*/


			/* Example to deny new connections on saturdays
			if(DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
			{
				client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
				return false;
			}
			*/

			/* Example to deny new connections between 10am and 12am
			if(DateTime.Now.Hour >= 10 && DateTime.Now.Hour <= 12)
			{
				client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
				return false;
			}
			*/

			return true;
		}

		public abstract string RulesDescription();

		public virtual bool IsAllowedCharsInAllRealms(GameClient client)
		{
			return true;
		}
		
		public virtual bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet)
		{
			if(source == null || target == null) return false;
			
			return true;
		}
		
		public virtual bool IsAllowedToTrade(GamePlayer source, GamePlayer target, bool quiet)
		{
			if(source == null || target == null) return false;
			
			return true;
		}

		public virtual bool IsAllowedToUnderstand(GameLiving source, GamePlayer target)
		{
			if(source == null || target == null) return false;
			
			return true;
		}
		
		public virtual bool IsSameRealm(GameLiving source, GameLivingBase target, bool quiet)
		{
			if(source == null || target == null) return false;
			
			// clients with priv level > 1 are considered friendly by anyone
			if(target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > ePrivLevel.Player) return true;

			// npc with peace brain are always of the same realm
			if (source is GameNPC && ((GameNPC)source).OwnBrain is PeaceBrain
			 || target is GameNPC && ((GameNPC)target).OwnBrain is PeaceBrain)
				return true;
			
			return false;
		}
		
		/// <summary>
		/// Is attacker allowed to attack defender.
		/// </summary>
		/// <param name="attacker">living that makes attack</param>
		/// <param name="defender">attacker's target</param>
		/// <param name="quiet">should messages be sent</param>
		/// <returns>true if attack is allowed</returns>
		public virtual bool IsAllowedToAttack(GameLiving attacker, GameLivingBase defender, bool quiet)
		{
			if(attacker == null || defender == null) return false;

			// can't attack self
			if(attacker == defender)
			{
				if (quiet == false) MessageToLiving(attacker, "You can't attack yourself!");
				return false;
			}

			// npc with peace brain can't attack and can't be attacked
			if (attacker is GameNPC && ((GameNPC)attacker).OwnBrain is PeaceBrain
			 || defender is GameNPC && ((GameNPC)defender).OwnBrain is PeaceBrain)
				return false;

			GamePlayer playerAttacker = attacker as GamePlayer;
			GamePlayer playerDefender = defender as GamePlayer;

			if (playerDefender != null && playerDefender.Client.ClientState != GameClient.eClientState.Playing)
			{
				if (!quiet)
					MessageToLiving(attacker, defender.Name + " is entering the game and is temporarily immune to PvP attacks!");
				return false;
			}

			if (playerAttacker != null && playerDefender != null)
			{
				// Attacker immunity
				if(playerAttacker.IsPvPInvulnerability)
				{
					if(quiet == false) MessageToLiving(attacker, "You can't attack players until your PvP invulnerability timer wears off!");
					return false;
				}

				// Defender immunity
				if(playerDefender.IsPvPInvulnerability)
				{
					if(quiet == false) MessageToLiving(attacker, defender.Name + " is temporarily immune to PvP attacks!");
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Is caster allowed to cast a spell
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="target"></param>
		/// <param name="spell"></param>
		/// <param name="spellLine"></param>
		/// <returns>true if allowed</returns>
		public virtual bool IsAllowedToCastSpell(GameLiving caster, GameLivingBase target, Spell spell, SpellLine spellLine)
		{
			return true;
		}

		public virtual bool IsAllowedToSpeak(GamePlayer source, string communicationType)
		{
			if(source.Alive == false)
			{
				MessageToLiving(source, "Hmmmm...you can't " + communicationType + " while dead!");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Is player allowed to bind
		/// </summary>
		/// <param name="player"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public virtual bool IsAllowedToBind(GamePlayer player, BindPoint point)
		{
			return true;
		}

		/// <summary>
		/// Is player allowed to make the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public virtual bool IsAllowedToCraft(GamePlayer player, GenericItemTemplate item)
		{
			return true;
		}

		public virtual bool CanTakeFallDamage(GamePlayer player)
		{
			if(player.Client.Account.PrivLevel > ePrivLevel.Player)
				return false;
			return true;
		}

		public virtual long GetExperienceForLevel(int level)
		{
			if (level > GamePlayer.MAX_LEVEL)
				return GamePlayer.XPLevel[GamePlayer.MAX_LEVEL]; // exp for level 51, needed to get exp after 50
			if (level <= 0)
				return GamePlayer.XPLevel[0];
			return GamePlayer.XPLevel[level - 1];
		}

		public virtual long GetExperienceForLiving(int level)
		{
			level = (level < 0) ? 0 : level;

			// use exp table
			if(level < GameLiving.XPForLiving.Length)
				return GameLiving.XPForLiving[level];

			// use formula if level is not in exp table
			// long can hold values up to level 238
			if(level > 238)
				level = 238;

			double k1, k1_inc, k1_lvl;

			// noret: using these rules i was able to reproduce table from
			// http://www.daocweave.com/daoc/general/experience_table.htm
			if(level>=35)
			{
				k1_lvl=35;
				k1_inc=0.2;
				k1=20;
			}
			else if(level>=20)
			{
				k1_lvl=20;
				k1_inc=0.3334;
				k1=15;
			}
			else if(level>=10)
			{
				k1_lvl=10;
				k1_inc=0.5;
				k1=10;
			}
			else
			{
				k1_lvl=0;
				k1_inc=1;
				k1=0;
			}

			long exp = (long)(Math.Pow(2, k1+(level-k1_lvl)*k1_inc) * 5);
			if (exp < 0)
			{
				exp = 0;
			}

			return exp;
		}

		public virtual bool CheckAbilityToUseItem(GamePlayer player, EquipableItem item)
		{
			if(player == null)
				return false;

			if(item.AllowedClass.Count > 0 && !item.AllowedClass.Contains((eCharacterClass)player.CharacterClassID))
			{
				return false;
			}

			//armor
			if (item.ObjectType >= eObjectType._FirstArmor && item.ObjectType <= eObjectType._LastArmor)
			{
				int bestLevel = -1;
				bestLevel = Math.Max(bestLevel, player.GetAbilityLevel(Abilities.AlbArmor));
				bestLevel = Math.Max(bestLevel, player.GetAbilityLevel(Abilities.HibArmor));
				bestLevel = Math.Max(bestLevel, player.GetAbilityLevel(Abilities.MidArmor));

				switch (item.ObjectType)
				{
					case eObjectType.Cloth      : return bestLevel >= 1;
					case eObjectType.Leather    : return bestLevel >= 2;
					case eObjectType.Reinforced :
					case eObjectType.Studded    : return bestLevel >= 3;
					case eObjectType.Scale      :
					case eObjectType.Chain      : return bestLevel >= 4;
					case eObjectType.Plate      : return bestLevel >= 5;
					default: return false;
				}
			}

			string[] check = new string[0];
			
			//http://dol.kitchenhost.de/files/dol/Info/itemtable.txt
			//http://support.darkageofcamelot.com/cgi-bin/support.cfg/php/enduser/std_adp.php?p_sid=frxnPUjg&p_lva=&p_refno=020709-000000&p_created=1026248996&p_sp=cF9ncmlkc29ydD0mcF9yb3dfY250PTE0JnBfc2VhcmNoX3RleHQ9JnBfc2VhcmNoX3R5cGU9MyZwX2NhdF9sdmwxPTI2JnBfY2F0X2x2bDI9fmFueX4mcF9zb3J0X2J5PWRmbHQmcF9wYWdlPTE*&p_li
			switch(item.ObjectType)
			{
				case eObjectType.GenericItem     : return true;
				case eObjectType.GenericArmor    : return true;
				case eObjectType.GenericWeapon   : return true;
				case eObjectType.Staff           : check = new string[] { Abilities.Weapon_Staves }; break;
				case eObjectType.ShortBow        : check = new string[] { Abilities.Weapon_Shortbows }; break;

					//alb
				case eObjectType.CrushingWeapon  : check = new string[] { Abilities.Weapon_Crushing, Abilities.Weapon_Blunt, Abilities.Weapon_Hammers }; break;
				case eObjectType.SlashingWeapon  : check = new string[] { Abilities.Weapon_Slashing, Abilities.Weapon_Blades, Abilities.Weapon_Swords, Abilities.Weapon_Axes }; break;
				case eObjectType.ThrustWeapon    : check = new string[] { Abilities.Weapon_Thrusting, Abilities.Weapon_Piercing }; break;
				case eObjectType.FlexibleWeapon  : check = new string[] { Abilities.Weapon_Flexible }; break;
				case eObjectType.TwoHandedWeapon : check = new string[] { Abilities.Weapon_TwoHanded, Abilities.Weapon_LargeWeapons }; break;
				case eObjectType.PolearmWeapon   : check = new string[] { Abilities.Weapon_Polearms, Abilities.Weapon_CelticSpear, Abilities.Weapon_Spears }; break;
				case eObjectType.Longbow         : check = new string[] { Abilities.Weapon_Longbows }; break;
				case eObjectType.Crossbow        : check = new string[] { Abilities.Weapon_Crossbow }; break;

					//mid
				case eObjectType.Sword           : check = new string[] { Abilities.Weapon_Swords, Abilities.Weapon_Slashing, Abilities.Weapon_Blades }; check = new string[] { Abilities.Weapon_Swords }; break;
				case eObjectType.Hammer          : check = new string[] { Abilities.Weapon_Hammers, Abilities.Weapon_Crushing, Abilities.Weapon_Blunt }; check = new string[] { Abilities.Weapon_Hammers }; break;
				case eObjectType.LeftAxe:
				case eObjectType.Axe             : check = new string[] { Abilities.Weapon_Axes, Abilities.Weapon_Slashing, Abilities.Weapon_Blades }; check = new string[] { Abilities.Weapon_Axes }; break;
				case eObjectType.HandToHand      : check = new string[] { Abilities.Weapon_HandToHand }; break;
				case eObjectType.Spear           : check = new string[] { Abilities.Weapon_Spears, Abilities.Weapon_CelticSpear, Abilities.Weapon_Polearms }; break;
				case eObjectType.CompositeBow    : check = new string[] { Abilities.Weapon_CompositeBows }; break;
				case eObjectType.ThrownWeapon    : check = new string[] { Abilities.Weapon_Thrown }; break;

					//hib
				case eObjectType.Blades          : check = new string[] { Abilities.Weapon_Blades, Abilities.Weapon_Slashing, Abilities.Weapon_Swords, Abilities.Weapon_Axes }; break;
				case eObjectType.Blunt           : check = new string[] { Abilities.Weapon_Blunt, Abilities.Weapon_Crushing, Abilities.Weapon_Hammers }; break;
				case eObjectType.Piercing        : check = new string[] { Abilities.Weapon_Piercing, Abilities.Weapon_Thrusting }; break;
				case eObjectType.LargeWeapon     : check = new string[] { Abilities.Weapon_LargeWeapons, Abilities.Weapon_TwoHanded }; break;
				case eObjectType.CelticSpear     : check = new string[] { Abilities.Weapon_CelticSpear, Abilities.Weapon_Spears, Abilities.Weapon_Polearms }; break;
				case eObjectType.Scythe          : check = new string[] { Abilities.Weapon_Scythe }; break;
				case eObjectType.RecurvedBow     : check = new string[] { Abilities.Weapon_RecurvedBows }; break;

					//misc
				//case eObjectType.Magical         : return true;
				case eObjectType.Shield          : return player.GetAbilityLevel(Abilities.Shield) >= (byte)(((Shield)item).Size);
				//case eObjectType.Arrow           : check = new string[] { Abilities.Weapon_CompositeBows, Abilities.Weapon_Longbows, Abilities.Weapon_RecurvedBows, Abilities.Weapon_Shortbows }; break;
				//case eObjectType.Bolt            : check = new string[] { Abilities.Weapon_Crossbow }; break;
				//case eObjectType.Poison          : return player.GetModifiedSpecLevel(Specs.Envenom) > 0;
				case eObjectType.Instrument      : return player.HasAbility(Abilities.Weapon_Instruments);
			}

			foreach(string ch in check)
				if(player.HasAbility(ch))
					return true;

			return false;
		}

		/// <summary>
		/// Get object specialization level based on server type
		/// </summary>
		/// <param name="player">player whom specializations are checked</param>
		/// <param name="objectType">object type</param>
		/// <returns>specialization in object or 0</returns>
		public virtual int GetObjectSpecLevel(GamePlayer player, eObjectType objectType)
		{
			int res = 0;

			foreach(eObjectType obj in GetCompatibleObjectTypes(objectType))
			{
				int spec = player.GetModifiedSpecLevel(SkillBase.ObjectTypeToSpec(obj));
				if(res < spec)
					res = spec;
			}
			return res;
		}

		/// <summary>
		/// Checks whether one object type is equal to another
		/// based on server type
		/// </summary>
		/// <param name="type1"></param>
		/// <param name="type2"></param>
		/// <returns>true if equals</returns>
		public virtual bool IsObjectTypesEqual(eObjectType type1, eObjectType type2)
		{
			foreach(eObjectType obj in GetCompatibleObjectTypes(type1))
			{
				if(obj == type2)
					return true;
			}
			return false;
		}

		#region GetCompatibleObjectTypes

		/// <summary>
		/// Holds arrays of compatible object types
		/// </summary>
		protected Hashtable m_compatibleObjectTypes = null;

		/// <summary>
		/// Translates object type to compatible object types based on server type
		/// </summary>
		/// <param name="objectType">The object type</param>
		/// <returns>An array of compatible object types</returns>
		protected virtual eObjectType[] GetCompatibleObjectTypes(eObjectType objectType)
		{
			if(m_compatibleObjectTypes == null)
			{
				m_compatibleObjectTypes = new Hashtable();
				m_compatibleObjectTypes[(int)eObjectType.Staff] = new eObjectType[] { eObjectType.Staff };
				m_compatibleObjectTypes[(int)eObjectType.ShortBow] = new eObjectType[] { eObjectType.ShortBow };

				//alb
				m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon]  = new eObjectType[] { eObjectType.CrushingWeapon, eObjectType.Blunt, eObjectType.Hammer };
				m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon]  = new eObjectType[] { eObjectType.SlashingWeapon, eObjectType.Blades, eObjectType.Sword, eObjectType.Axe };
				m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon]    = new eObjectType[] { eObjectType.ThrustWeapon, eObjectType.Piercing };
				m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon, eObjectType.LargeWeapon };
				m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon]   = new eObjectType[] { eObjectType.PolearmWeapon, eObjectType.CelticSpear, eObjectType.Spear };
				m_compatibleObjectTypes[(int)eObjectType.FlexibleWeapon]  = new eObjectType[] { eObjectType.FlexibleWeapon };
				m_compatibleObjectTypes[(int)eObjectType.Longbow]         = new eObjectType[] { eObjectType.Longbow };
				m_compatibleObjectTypes[(int)eObjectType.Crossbow]        = new eObjectType[] { eObjectType.Crossbow };
				//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;

				//mid
				m_compatibleObjectTypes[(int)eObjectType.Hammer]       = new eObjectType[] { eObjectType.Hammer, eObjectType.CrushingWeapon, eObjectType.Blunt };
				m_compatibleObjectTypes[(int)eObjectType.Sword]        = new eObjectType[] { eObjectType.Sword, eObjectType.SlashingWeapon, eObjectType.Blades };
				m_compatibleObjectTypes[(int)eObjectType.LeftAxe]      = new eObjectType[] { eObjectType.LeftAxe };
				m_compatibleObjectTypes[(int)eObjectType.Axe]          = new eObjectType[] { eObjectType.Axe, eObjectType.SlashingWeapon, eObjectType.Blades };
				m_compatibleObjectTypes[(int)eObjectType.HandToHand]   = new eObjectType[] { eObjectType.HandToHand };
				m_compatibleObjectTypes[(int)eObjectType.Spear]        = new eObjectType[] { eObjectType.Spear, eObjectType.CelticSpear, eObjectType.PolearmWeapon };
				m_compatibleObjectTypes[(int)eObjectType.CompositeBow] = new eObjectType[] { eObjectType.CompositeBow };
				m_compatibleObjectTypes[(int)eObjectType.ThrownWeapon] = new eObjectType[] { eObjectType.ThrownWeapon };

				//hib
				m_compatibleObjectTypes[(int)eObjectType.Blunt]        = new eObjectType[] { eObjectType.Blunt, eObjectType.CrushingWeapon, eObjectType.Hammer };
				m_compatibleObjectTypes[(int)eObjectType.Blades]       = new eObjectType[] { eObjectType.Blades, eObjectType.SlashingWeapon, eObjectType.Sword, eObjectType.Axe };
				m_compatibleObjectTypes[(int)eObjectType.Piercing]     = new eObjectType[] { eObjectType.Piercing, eObjectType.ThrustWeapon };
				m_compatibleObjectTypes[(int)eObjectType.LargeWeapon]  = new eObjectType[] { eObjectType.LargeWeapon, eObjectType.TwoHandedWeapon };
				m_compatibleObjectTypes[(int)eObjectType.CelticSpear]  = new eObjectType[] { eObjectType.CelticSpear, eObjectType.Spear, eObjectType.PolearmWeapon };
				m_compatibleObjectTypes[(int)eObjectType.Scythe]       = new eObjectType[] { eObjectType.Scythe };
				m_compatibleObjectTypes[(int)eObjectType.RecurvedBow]  = new eObjectType[] { eObjectType.RecurvedBow };

				m_compatibleObjectTypes[(int)eObjectType.Shield]       = new eObjectType[] { eObjectType.Shield };
				m_compatibleObjectTypes[(int)eObjectType.Poison]       = new eObjectType[] { eObjectType.Poison };
				//TODO: case 45: abilityCheck = Abilities.instruments; break;
			}

			eObjectType[] res = (eObjectType[])m_compatibleObjectTypes[(int)objectType];
			if(res == null)
				return new eObjectType[0];
			return res;
		}

		#endregion

		/// <summary>
		/// Invoked on NPC death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedNpc">npc that died</param>
		/// <param name="killer">killer</param>
		public virtual void OnNPCKilled(GameNPC killedNpc, GameLiving killer)
		{
			bool isWorthReward = true;

			string message = "You gain no experience from this kill!";
			if (killedNpc is GameSummonedPet || killedNpc.Region.Time - GameMob.CHARMED_NOEXP_TIMEOUT < killedNpc.TempProperties.getLongProperty(GameMob.CHARMED_TICK_PROP, long.MinValue))
			{
				message = "This monster has been charmed recently and is worth no experience.";
				isWorthReward = false;
			}
			else		
			{
				lock(killedNpc.XPGainers.SyncRoot)
				{
					if(killedNpc.XPGainers.Count == 0)
					{
						isWorthReward = false;
					}
					else
					{
						// we need at least 1 player attacker to deal xp and loot
						bool playerAttacker = false;
						foreach(GameLiving gainer in killedNpc.XPGainers.Keys)
						{
							GamePlayer gainerPlayer = gainer as GamePlayer;
							if(gainerPlayer != null)
							{
								playerAttacker = true;

								//If a gameplayer with privlevel > 1 attacked the
								//mob, then the players won't gain xp nor loot...
								if(gainerPlayer.Client.Account.PrivLevel > ePrivLevel.Player)
								{
									isWorthReward = false;
									break;
								}
							}
							else
							{
								GameNPC gainerNPC = gainer as GameNPC;
								if(gainerNPC != null)
								{
									IControlledBrain brain = gainerNPC.Brain as IControlledBrain;
									if (brain != null && brain.Owner != null)
									{
										playerAttacker = true;
									}
								}
							}

							//If the killed npc is gray for one of the xpGainer (no matter if player or another npc)
							//it is't worth anything either
							if(gainer.IsObjectGreyCon(killedNpc)) 
							{
								isWorthReward = false;
								break;
							}
						}

						if(!playerAttacker) isWorthReward = false;
					}
				}
			}

			if(!isWorthReward)
			{
				foreach(DictionaryEntry de in killedNpc.XPGainers)
				{
					GamePlayer player = de.Key as GamePlayer;
					if(player!=null)
						player.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return;
			}

			lock(killedNpc.XPGainers.SyncRoot)
			{
				ArrayList playerGainers = new ArrayList();
				foreach(DictionaryEntry de in killedNpc.XPGainers)
				{
					GamePlayer player = de.Key as GamePlayer;
					if (player == null) continue;
					if (player.ObjectState != eObjectState.Active) continue;
					if (!player.Alive) continue;
					if (!player.Position.CheckSquareDistance(killedNpc.Position, (uint) (WorldMgr.MAX_EXPFORKILL_DISTANCE*WorldMgr.MAX_EXPFORKILL_DISTANCE))) continue;
					
					playerGainers.Add(player);
				}

				#region XP

				float totalDamage = 0;
				foreach(GamePlayer player in playerGainers)
					totalDamage += (float)killedNpc.XPGainers[player];

				long npcExpValue = killedNpc.ExperienceValue;

				//Now deal the XP to all livings
				foreach(GamePlayer player in playerGainers)
				{
					double damagePercent = (float)killedNpc.XPGainers[player] / totalDamage;
					long xpReward = (long)(npcExpValue * damagePercent); // exp for damage percent

					// exp cap
					long expCap = (long)(player.ExperienceValue * 1.25);
					if(xpReward > expCap) xpReward = expCap;

					// camp bonus
					const double fullCampBonus = 0.3;
					const double fullCampBonusTicks = (1000 * 60 * 60); //1 hour (in ms) = full 30%
					long livingLifeSpan = killedNpc.Region.Time - killedNpc.SpawnTick;

					double campBonus = fullCampBonus * (livingLifeSpan / fullCampBonusTicks);

					if(campBonus < 0.01)
						campBonus = 0;
					else if(campBonus > fullCampBonus)
						campBonus = fullCampBonus;

                    //group bonus
                    //TODO

                    player.GainExperience(xpReward, (long)(xpReward * campBonus), 0, true);
				}
				#endregion

				#region Loot

				// now generate loot
				GameMob killedMob = killedNpc as GameMob;
				if(killedMob != null) // only GameMob can loot
				{
					ArrayList allLoots = new ArrayList();

					LootList mobLootList = GameServer.Database.SelectObject(typeof(LootList), Expression.Eq("LootListID", killedMob.LootListID)) as LootList;
					if(mobLootList != null)
					{
						foreach(ILoot loot in mobLootList.AllLoots)
						{
							if(Util.Chance(loot.Chance))
							{
								GameObjectTimed obj = loot.GetLoot(killedMob, killer);
								if(obj != null) allLoots.Add(obj);
							}
						}
					}

					// when no item loot always drop a bag of coin else 20% chance
					if(allLoots.Count <= 0 || Util.Chance(20)) 
					{
						GameMoney money = new GameMoney();
						money.Name = "bag of coin";
						money.Model = 82;
						money.Realm = 0;
						
						int lvl = killedMob.Level+1;
						if (lvl < 1) lvl = 1;
						int minLoot = 2+((lvl*lvl*lvl)>>3);
						money.TotalCopper = minLoot+Util.Random(minLoot>>1);

						allLoots.Add(money);
					}
							
					//Now add all owner to all loots
					foreach(GamePlayer player in playerGainers)
					{
						foreach(GameObjectTimed loot in allLoots)
						{
							loot.AddOwner(player);
						}
					}

					// add all loots to the world
					foreach(GameObjectTimed loot in allLoots)
					{
						loot.Position = killedMob.Position;
						loot.Heading = killedMob.Heading;
						loot.Region = killedMob.Region;

						loot.AddToWorld();
					}

					// send all loot message
					foreach(GamePlayer player in killedMob.GetInRadius(typeof(GamePlayer), WorldMgr.INFO_DISTANCE))
					{
						foreach(GameObjectTimed loot in allLoots)
						{
							player.Out.SendMessage(killedMob.GetName(0, true) +" drops "+ loot.GetName(1, false) +".", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
						}
					}
				}
				#endregion
			}
		}

		/// <summary>
		/// Invoked on Player death and deals out
		/// experience/realm points if needed
		/// </summary>
		/// <param name="killedPlayer">player that died</param>
		/// <param name="killer">killer</param>
		public virtual void OnPlayerKilled(GamePlayer killedPlayer, GameObject killer)
		{
			killedPlayer.LastDeathRealmPoints = 0;
			// "player has been killed recently"
			const long noExpSeconds = 5 * 60;
			if(killedPlayer.DeathTime + noExpSeconds > killedPlayer.PlayedTime)
			{
				lock(killedPlayer.XPGainers.SyncRoot)
				{
					foreach(DictionaryEntry de in killedPlayer.XPGainers)
						if(de.Key is GamePlayer)
						{
							((GamePlayer)de.Key).Out.SendMessage(killedPlayer.Name + " has been killed recently and is worth no realm points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							((GamePlayer)de.Key).Out.SendMessage(killedPlayer.Name + " has been killed recently and is worth no experience!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						}
				}
				return;
			}

			lock(killedPlayer.XPGainers.SyncRoot)
			{
				bool dealNoXP = false;
				float totalDamage = 0;
				//Collect the total damage
				foreach(DictionaryEntry de in killedPlayer.XPGainers)
				{
					GameObject obj = (GameObject)de.Key;
					if(obj is GamePlayer)
					{
						//If a gameplayer with privlevel > 1 attacked the
						//mob, then the players won't gain xp ...
						if(((GamePlayer)obj).Client.Account.PrivLevel > ePrivLevel.Player)
						{
							dealNoXP = true;
							break;
						}
					}
					else
					{
						//If object is no gameplayer and realm is != none
						//then it means that a npc has hit this living and
						//it is not worth any xp ...
						if(obj.Realm != (byte)eRealm.None)
						{
							dealNoXP = true;
							break;
						}
					}
					totalDamage += (float)de.Value;
				}

				if(dealNoXP)
				{
					foreach(DictionaryEntry de in killedPlayer.XPGainers)
					{
						GamePlayer player = de.Key as GamePlayer;
						if(player!=null)
							player.Out.SendMessage("You gain no experience from this kill!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return;
				}


				long playerExpValue = killedPlayer.ExperienceValue;
				int playerRPValue = killedPlayer.RealmPointsValue;
				int playerBPValue = killedPlayer.BountyPointsValue;

				//Now deal the XP and RPs to all livings
				foreach(DictionaryEntry de in killedPlayer.XPGainers)
				{
					GamePlayer expGainer = de.Key as GamePlayer;
					if (expGainer == null) continue;
					if (expGainer.ObjectState != eObjectState.Active) continue;
					if (!expGainer.Alive) continue;
					if (!expGainer.Position.CheckSquareDistance(killedPlayer.Position, (uint) (WorldMgr.MAX_EXPFORKILL_DISTANCE*WorldMgr.MAX_EXPFORKILL_DISTANCE))) continue;


					double damagePercent = (float)de.Value / totalDamage;

					// realm points
					int rpCap = expGainer.RealmPointsValue * 2;
					int realmPoints = (int)(playerRPValue * damagePercent);
					if(realmPoints > rpCap)
						realmPoints = rpCap;
					if(realmPoints != 0)
					{
						killedPlayer.LastDeathRealmPoints += realmPoints;
						expGainer.GainRealmPoints(realmPoints);
					}

					// bounty points
					int bpCap = expGainer.BountyPointsValue * 2;
					int bountyPoints = (int)(playerBPValue * damagePercent);
					if(bountyPoints > bpCap)
						bountyPoints = bpCap;
					if(bountyPoints != 0)
					{
						expGainer.GainBountyPoints(bountyPoints);
					}

					// experience
					long xpReward = (long)(playerExpValue * damagePercent); // exp for damage percent

					long expCap = (long)(expGainer.ExperienceValue * 1.25);
					if(xpReward > expCap)
						xpReward = expCap;

					expGainer.GainExperience(xpReward);
					
					if (killedPlayer.ReleaseType != GamePlayer.eReleaseType.Duel)
					{
						switch ((eRealm)killedPlayer.Realm)
						{
							case eRealm.Albion:
								expGainer.KillsAlbionPlayers++;
								if (expGainer == killer)
								{
									expGainer.KillsAlbionDeathBlows++;
									if ((float)de.Value == totalDamage)
										expGainer.KillsAlbionSolo++;
								}
								break;
								
							case eRealm.Hibernia:
								expGainer.KillsHiberniaPlayers++;
								if (expGainer == killer)
								{
									expGainer.KillsHiberniaDeathBlows++;
									if ((float)de.Value == totalDamage)
										expGainer.KillsHiberniaSolo++;
								}
								break;
								
							case eRealm.Midgard:
								expGainer.KillsMidgardPlayers++;
								if (expGainer == killer)
								{
									expGainer.KillsMidgardDeathBlows++;
									if ((float)de.Value == totalDamage)
										expGainer.KillsMidgardSolo++;
								}
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the Realm of an living for name text coloring
		/// </summary>
		/// <param name="player"></param>
		/// <param name="target"></param>
		/// <returns>byte code of realm</returns>
		public virtual byte GetLivingRealm(GamePlayer player, GameLiving target)
		{
			if (player == null || target == null) return 0;

			// clients with priv level > 1 are considered friendly by anyone
			GamePlayer playerTarget = target as GamePlayer;
			if(playerTarget != null && playerTarget.Client.Account.PrivLevel > ePrivLevel.Player) return player.Realm;

			return target.Realm;
		}

		/// <summary>
		/// Gets the player name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The name of the target</returns>
		public virtual string GetPlayerName(GamePlayer source, GamePlayer target)
		{
			return target.Name;
		}

		/// <summary>
		/// Gets the player last name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The last name of the target</returns>
		public virtual string GetPlayerLastName(GamePlayer source, GamePlayer target)
		{
			return target.LastName;
		}

		/// <summary>
		/// Gets the player guild name based on server type
		/// </summary>
		/// <param name="source">The "looking" player</param>
		/// <param name="target">The considered player</param>
		/// <returns>The guild name of the target</returns>
		public virtual string GetPlayerGuildName(GamePlayer source, GamePlayer target)
		{
			return target.Guild != null ? target.Guild.GuildName : String.Empty;
		}

		/// <summary>
		/// Gets the server type color handling scheme
		///
		/// ColorHandling: this byte tells the client how to handle color for PC and NPC names (over the head)
		/// 0: standard way, other realm PC appear red, our realm NPC appear light green
		/// 1: standard PvP way, all PC appear red, all NPC appear with their level color
		/// 2: Same realm livings are friendly, other realm livings are enemy; nearest friend/enemy buttons work
		/// 3: standard PvE way, all PC friendly, realm 0 NPC enemy rest NPC appear light green
		/// 4: All NPC are enemy, all players are friendly; nearest friend button selects self, nearest enemy don't work at all
		/// </summary>
		/// <param name="client">The client asking for color handling</param>
		/// <returns>The color handling</returns>
		public virtual byte GetColorHandling(GameClient client)
		{
			return 0;
		}

		/// <summary>
		/// Formats player statistics.
		/// </summary>
		/// <param name="player">The player to read statistics from.</param>
		/// <returns>List of strings.</returns>
		public IList FormatPlayerStatistics(GamePlayer player)
		{
			ArrayList stat = new ArrayList();
			int total = 0;
			stat.Add("Kill report");
			switch((eRealm)player.Realm)
			{
				case eRealm.Albion:
					stat.Add("Midgard Players Killed: " + player.KillsMidgardPlayers.ToString("N0"));
					stat.Add("Hibernia Players Killed: " + player.KillsHiberniaPlayers.ToString("N0"));
					total = player.KillsMidgardPlayers + player.KillsHiberniaPlayers;
					break;
				case eRealm.Midgard:
					stat.Add("Albion Players Killed: " + player.KillsAlbionPlayers.ToString("N0"));
					stat.Add("Hibernia Players Killed: " + player.KillsHiberniaPlayers.ToString("N0"));
					total = player.KillsAlbionPlayers + player.KillsHiberniaPlayers;
					break;
				case eRealm.Hibernia:
					stat.Add("Albion Players Killed: " + player.KillsAlbionPlayers.ToString("N0"));
					stat.Add("Midgard Players Killed: " + player.KillsMidgardPlayers.ToString("N0"));
					total = player.KillsMidgardPlayers + player.KillsAlbionPlayers;
					break;
			}
			stat.Add("Total Players Killed: " + total.ToString("N0"));
			stat.Add(" ");
			stat.Add("Capture Report");
			stat.Add("Keeps Captured: " + player.CapturedKeeps.ToString("N0"));
			stat.Add("Towers Captured: " + player.CapturedTowers.ToString("N0"));
			
			return stat;
		}

		#region MessageToLiving
		/// <summary>
		/// Send system text message to system window
		/// </summary>
		/// <param name="living"></param>
		/// <param name="message"></param>
		public virtual void MessageToLiving(GameLiving living, string message)
		{
			MessageToLiving(living, message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		/// <summary>
		/// Send custom text message to system window
		/// </summary>
		/// <param name="living"></param>
		/// <param name="message"></param>
		/// <param name="type"></param>
		public virtual void MessageToLiving(GameLiving living, string message, eChatType type)
		{
			MessageToLiving(living, message, type, eChatLoc.CL_SystemWindow);
		}
		/// <summary>
		/// Send custom text message to GameLiving
		/// </summary>
		/// <param name="living"></param>
		/// <param name="message"></param>
		/// <param name="type"></param>
		/// <param name="loc"></param>
		public virtual void MessageToLiving(GameLiving living, string message, eChatType type, eChatLoc loc)
		{
			if(living is GamePlayer)
				((GamePlayer)living).Out.SendMessage(message, type, loc);
		}
		#endregion
	}
}
