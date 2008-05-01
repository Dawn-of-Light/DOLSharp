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
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Quests
{
	/// <summary>
	/// Declares a Kill Task	
	/// Kill Mob A
	/// </summary>
	public class KillTask : AbstractTask
	{				
		// Chance of npc having task for player
		protected new const ushort CHANCE = 75;

		protected const String MOB_NAME = "mobName";
		protected const String ITEM_INDEX = "itemIndex";

		//private static int[] XPReward = new int[20]{25,50,100,200,400,800,1600,3200,6400,12800,18100,25600,36200,51200,72450,102400,144825,204800,289625,300000};
		private static readonly int[] MoneyReward = new int[20] {28,57,77,105,140,190,257,347,470,632,735,852,987,1147,1330,1542,1790,2077,2407,2801 };					

		// used to build generic mob item
		private static readonly string[] StrFormat = {"{0}'s {1}","{1} of {0}"};
		private static readonly string[] TaskObjects = {"Skin","Meat","Bones","Tooth","Claw","Skin","Legs","Collar","Bone","Ear","Head","Hair","Carapace","Skull","Pile of dirt","dust","Slice","Wings","egg","heart","mandible"};
		private static readonly int[] ObjectModels = {629,102,105,106,106,629,108,109,497,501,503,506,517,540,541,541,548,551,587,595,614};
		/// <summary>
		/// Constructs a new task
		/// </summary>
		/// <param name="taskPlayer">The player doing this task</param>
		public KillTask(GamePlayer taskPlayer) : base(taskPlayer)
		{
		}				
		
		/// <summary>
		/// Constructs a new task from a database Object
		/// </summary>
		/// <param name="taskPlayer">The player doing the task</param>
		/// <param name="dbTask">The database object</param>
		public KillTask(GamePlayer taskPlayer, DBTask dbTask) : base(taskPlayer, dbTask)
		{
		}

		public override long RewardMoney
		{
			get 
			{
				const ushort Scarto = 3; // Add/Remove % to the Result
				
				int ValueScarto = ((MoneyReward[m_taskPlayer.Level-1]/100)*Scarto);
				return Util.Random(MoneyReward[m_taskPlayer.Level-1]-ValueScarto, MoneyReward[m_taskPlayer.Level-1]+ValueScarto);
			}
		}
/*
		public override long RewardXP
		{
			get
			{
				ushort Scarto = 3; // Add/Remove % to the Result
				
				int ValueScarto = ((XPReward[m_taskPlayer.Level-1]/100)*Scarto);
				return new Random().Next(XPReward[m_taskPlayer.Level-1]-ValueScarto,XPReward[m_taskPlayer.Level-1]+ValueScarto);
			}
		}
*/
		public override IList RewardItems
		{
			get {return null;}
		}
		
		/// <summary>
		/// Retrieves the name of the task
		/// </summary>
		public override string Name
		{
			get { return "Kill Task"; }
		}

		/// <summary>
		/// Retrieves the description 
		/// </summary>
		public override string Description
		{
			get { return "Find a "+MobName+", kill it and bring something to prove your valor to "+RecieverName+" "; }
		}		
			
		/// <summary>
		/// Item related to task stored in dbTask
		/// </summary>
		public String MobName
		{
			get { return GetCustomProperty(MOB_NAME);}
			set { SetCustomProperty(MOB_NAME,value);}
		}

		/// <summary>
		/// Item index
		/// </summary>
		public int ItemIndex
		{
			get { return int.Parse(GetCustomProperty(ITEM_INDEX));}
			set { SetCustomProperty(ITEM_INDEX,value.ToString());}
		}

		public override string ItemName
		{
			get
			{	
				return string.Format(StrFormat[0],MobName,TaskObjects[ItemIndex]);
			}
			set {}
		}

		/// <summary>
		/// Called to finish the task.
		/// Should be overridden and some rewards given etc.
		/// </summary>
		public override void FinishTask()
		{			
			base.FinishTask();
		}		

		/// <summary>
		/// This method needs to be implemented in each task.
		/// It is the core of the task. The global event hook of the GamePlayer.
		/// This method will be called whenever a GamePlayer with this task
		/// fires ANY event!
		/// </summary>
		/// <param name="e">The event type</param>
		/// <param name="sender">The sender of the event</param>
		/// <param name="args">The event arguments</param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			// Filter only the events from task owner
			if (sender != m_taskPlayer)
				return;
			
			if (CheckTaskExpired()) 
			{
				return;
			}
			
			GamePlayer player = m_taskPlayer;
			
			if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;				
				GameLiving target = gArgs.Target as GameLiving;
				InventoryItem item = gArgs.Item;
				
				if(player.Task.RecieverName == target.Name && item.Name == player.Task.ItemName)
				{
					player.Inventory.RemoveItem(item);
					FinishTask();
				}
			}
			
			// Check if Mob is the Target of a KillTask for each of XPGainers in the Killer Group and if true Drop the TaskItem
			else if (e == GameLivingEvent.EnemyKilled) 
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
				GameLiving target = gArgs.Target;
				
				// check wether the right mob was killed
				if (target.Name!=MobName)
					return;

				int lowestCon = int.MaxValue;
				lock(target.XPGainers.SyncRoot)
				{
					if(target.XPGainers.Keys.Count == 0)
					{
						return;
					}
					foreach(GameObject gainer in target.XPGainers.Keys)
					{
						if(gainer is GamePlayer)
						{
							if(gainer.Level <= GameLiving.NoXPForLevel.Length)
							{
								if(target.Level <= GameLiving.NoXPForLevel[gainer.Level])
								{
									lowestCon=-3;
								}
							}
							else
							{
								int con = (int)gainer.GetConLevel(target);
								if(con<lowestCon)
								{
									lowestCon=con;
								}
							}
						}
					}
				}

				// The following lines allow the GM to drop the task item
				if (player.Client.Account.PrivLevel != 1)
					lowestCon = 0;

				//Only add task Loot if not killing grays
				if(lowestCon >= -2)
				{
					ArrayList Owners = new ArrayList();
					if(player.Group == null)
					{
						Owners.Add(m_taskPlayer);
					}
					else
					{
						ICollection<GamePlayer> group = m_taskPlayer.Group.GetPlayersInTheGroup();
						foreach(GamePlayer p in group)
						{
							if(p.Task !=null && p.Task.GetType() == typeof(KillTask))
							{
								if (((KillTask)p.Task).MobName == target.Name)
									Owners.Add(p);
							}
						}
					}

					if (Owners.Count > 0)
					{
						ArrayList dropMessages = new ArrayList();
						InventoryItem itemdrop = GenerateItem(ItemName, 1, ObjectModels[ItemIndex]);
						GameInventoryItem droppeditem = new GameInventoryItem(itemdrop);
						for (int a = 0; a < Owners.Count; a++)
						{
							droppeditem.AddOwner((GameObject)Owners[a]);
						}
						droppeditem.Name = itemdrop.Name;
						droppeditem.Level = 1;
						droppeditem.X = target.X;
						droppeditem.Y = target.Y;
						droppeditem.Z = target.Z;
						droppeditem.CurrentRegion = target.CurrentRegion;
						droppeditem.AddToWorld();
						if(dropMessages.Count > 0)
						{
							foreach(GamePlayer visiblePlayer in target.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
							{
								foreach(string str in dropMessages)
								{
									visiblePlayer.Out.SendMessage(str, eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
								}
							}
						}
					}
				}
			}
		}		

		/// <summary>
        /// Search for a Mob to Kill and Give the KillTask to the Player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool BuildTask(GamePlayer player, GameLiving source)
		{
			if (source == null)
				return false;

			GameNPC Mob = GetRandomMob(player);
			if(Mob == null)
			{
				player.Out.SendMessage("I have no task for you, come back later",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				return false;
			}
			else
			{
				player.Task = new KillTask(player);
				player.Task.TimeOut = DateTime.Now.AddHours(2);
				((KillTask)player.Task).ItemIndex = Util.Random(0, TaskObjects.Length-1);
				((KillTask)player.Task).MobName = Mob.Name;
				player.Task.RecieverName = source.Name;

				player.Out.SendMessage("Find "+Mob.GetName(1,false)+", kill it and bring me something to prove your valor. I've heard there should be one roaming in "+Mob.CurrentZone.Description+", but it doesn't matter which "+Mob.Name+" you kill.",eChatType.CT_System,eChatLoc.CL_PopupWindow);
				//Player.Out.SendCustomDialog("", new CustomDialogResponse(TaskDialogResponse));
				return true;
			}
		}

		/// <summary>
		/// Find a Random Mob in Radius Distance
		/// </summary>
		/// <param name="Player">The GamePlayer Object</param>		
		/// <returns>The GameMob Searched</returns>
		public static GameNPC GetRandomMob(GamePlayer Player)
		{					
			int minLevel = GameLiving.NoXPForLevel[Player.Level]+1;			
			int maxLevel = (int) (2 * ( (double)(Player.Level / 10 + 1) )) + Player.Level;

			GameNPC npc = Player.CurrentZone.GetRandomNPC(eRealm.None, minLevel,maxLevel);
			return npc;
		}

		/// <summary>
		/// Idientifies Named Guards
		/// At the moment this is done by simple name comparison against some known name patterns:
		/// 
		/// +*Guard*
		///		-Guardian
		///		-Guardian Sergeant
		///		-*Guardian
		///		-Guardian of the*
		///		-Guard
		///		-*Guard
		///		-Guardsman
		///		-*Guardsman
		///		-Guard's Armorer
		///	+Sir *
		///	+Jarl *
		///	+Lady *
		///	+Soldier *
		///	+Soldat *
		///	+Sentinel *
		///		-*Runes
		///		-*Kynon 
		///	
		///	+*Viking*
		///		-*Archer
		///		-*Dreng
		///		-*Huscarl
		///		-*Jarl
		///		
		///	+Huntress *
		///		
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public static bool CheckNamedGuard(GameLiving living)
		{
			if (living.Realm==0)
				return false;

			String name = living.Name;

			if (name.IndexOf("Guard")>=0)
			{
				
				if (name =="Guardian") return false;
				if (name =="Guardian Sergeant") return false;
				if (name.EndsWith("Guardian")) return false;
				if (name.StartsWith("Guardian of the")) return false;

				if (name =="Guard") return false;
				if (name.EndsWith("Guard")) return false;

				if (name =="Guardsman") return false;
				if (name.EndsWith("Guardsman")) return false;

				if (name =="Guard's Armorer") return false;

				return true;
			}

			if (name.StartsWith("Sir ") && (living.GuildName==null || living.GuildName=="") ) 
			{
				return true;
			}

			if (name.StartsWith("Captain ") && (living.GuildName==null || living.GuildName=="") ) 
			{
				return true;
			}

			if (name.StartsWith("Jarl ")) 
			{
				return true;
			}

			if (name.StartsWith("Lady ") && (living.GuildName==null || living.GuildName=="") )
			{
				return true;
			}
			if (name.StartsWith("Soldier ") || name.StartsWith("Soldat "))
				return true;

			if (name.StartsWith("Sentinel "))
			{
				if (name.EndsWith("Runes")) return false;
				if (name.EndsWith("Kynon")) return false;

				return true;
			}


			if (name.IndexOf("Viking")>=0)
			{
				if (name.EndsWith("Archer")) return false;
				if (name.EndsWith("Dreng")) return false;
				if (name.EndsWith("Huscarl")) return false;
				if (name.EndsWith("Jarl")) return false;

				return true;
			}

			if (name.StartsWith("Huntress "))
			{
				return true;
			}

			return false;
		}		

		public new static bool CheckAvailability(GamePlayer player, GameLiving target)
		{
			if (target==null)
				return false;

			if (!CheckNamedGuard(target))			
				return false;

			return AbstractTask.CheckAvailability(player,target,CHANCE);			
		}
	}
}
