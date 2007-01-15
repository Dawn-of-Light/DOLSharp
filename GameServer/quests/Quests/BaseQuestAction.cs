using System;
using System.Collections;
using DOL.Events;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using log4net;
using System.Reflection;
using DOL.GS.Scripts;

namespace DOL.GS.Quests
{
	/// <summary>
	/// Type of textoutput this one is used for general text messages within questpart.   
	/// </summary>
	public enum eTextType : byte
	{
		/// <summary>
		/// No output at all
		/// </summary>
		None = 0x00,
		/// <summary>
		/// EMOT : display the text localy without monster's name (local channel)
		/// </summary>
		/// <remarks>Tested</remarks>
		Emote = 0x01,
		/// <summary>
		/// BROA : broadcast the text in the entire zone (broadcast channel)
		/// </summary>
		Broadcast = 0x02,
		/// <summary>
		/// DIAG : display the text in a dialog box with an OK button
		/// </summary>
		Dialog = 0x03,		
		/// <summary>
		/// READ : open a description (bracket) windows saying what is written on the item
		/// </summary>
		Read = 0x05,		
	}

	/// <summary>
	/// Actiontype defines a list of actiontypes to be used qith questparts.
	/// Depending on actiontype P and Q will have special
	/// meaning look at documentation of each actiontype for details       
	/// </summary>
	///<remarks>
	/// Syntax: ... P:eEmote(eEmote.Yes) ... Parameter P must be of Type
	/// eEmote and has the default value of "eEmote.Yes" (used if no value is passed).
	/// If no default value is defined value must be passed along with action.
	/// </remarks>
	public enum eActionType : byte
	{
		/// <summary>
		/// ANIM : emote P:eEmote is performed by GameLiving:Q(Player)[NPC's ID:string]
		/// </summary>
		/// <remarks>TO let player perform animation Q must be null.
		/// Tested</remarks>
		Animation,
		/// <summary>
		/// ATTA : Player is attacked with aggroamount P:int(Player.Level/2)[string] by monster Q:GameNPC(NPC)[NPC's ID:string]
		/// </summary>
		/// <remarks>Tested</remarks>
		Attack,
		/// <summary>
		/// GameNPC Q:GameNPC(NPC)[NPC's ID:string] walks to point P:GameLocation(player)
		/// </summary>        
		WalkTo,
		/// <summary>
		/// GameNPC Q:GameNPC(NPC)[NPC's ID:string] walks to spawnpoint
		/// </summary>        
		WalkToSpawn,
		/// <summary>
		/// GameLiving Q:GameLiving(NPC)[NPC's ID:string] jumps immediatly to point P:GameLocation(player)
		/// </summary>        
		MoveTo,		
		/*
		CAST : monster's p## spell is casted, or entire q## monster index casts its        
		CINV : p## item is created and placed in player's backback  // q## (functional ?)         
		CLAS : class is set to p##
		DEGE : p## monster index degenerates
		 * */		
		/// <summary>
		/// Displays a custom dialog with message P:string and CustomDialogresponse Q:CustomDialogResponse
		/// To Accept/Abort Quests use OfferQuest/OfferQuestAbort
		/// </summary>
		/// <remarks>Tested</remarks>
		CustomDialog,
		/// <summary>
		/// DINV : destroys Q:int(1)[string] instances of item P:ItemTemplate[Item's ID_nb:string] in inventory        
		/// </summary>
		DestroyItem,
		//DORM : enters dormant mode (unused)        
		/// <summary>
		/// DROP : item P:ItemTemplate[Item's ID_nb:string] is dropped on the ground
		/// </summary>
		DropItem,		
		// FGEN : forces monster index p## to spawn one monster from slot q## (uncheck max)        
		/// <summary>
		/// FQST : quest P:Type[Typename:string](Current Quest) is set as completed for player
		/// </summary>
		/// <remarks>Tested</remarks>
		FinishQuest,
		// // <summary>
		// /// GCAP : gives cap experience p## times (q## should be set to 1 to avoid bugs)                         
		// /// </summary>
		// GiveXPCap,
		/// <summary>
		/// GCPR : gives P:long[string] coppers
		/// </summary>
		GiveGold,
		/// <summary>
		/// TCPR : takes P:long[string] coppers
		/// </summary>
		TakeGold,
		/// <summary>
		/// GEXP : gives P:long[string] experience points
		/// </summary>
		GiveXP,
		/// <summary>
		/// GIVE : NPC Q gives item P:ItemTemplate[Item's ID_nb:string] to player
		/// if NPC!= null NPC will give item to player, otherwise it appears mysteriously :)
		/// </summary>
		/// <remarks>Tested</remarks>
		GiveItem,
		/// <summary>
		/// NPC takes Q:int[string] instances of item P:ItemTemplate[Item's ID_nb:string] from player
		/// default for q## is 1
		/// </summary>
		/// <remarks>Tested</remarks>
		TakeItem,
		/// <summary>
		/// QST : Q:GameNPC(NPC) assigns quest P:Type[Typename:string](Current Quest) to player        
		/// </summary>
		/// <remarks>Tested</remarks>
		GiveQuest,
		/// <summary>
		/// Quest P:Type[Typename:string](Current Quest) is offered to player via customdialog with message Q:string ny NPC
		/// if player accepts GamePlayerEvent.AcceptQuest is fired, else GamePlayerEvent.RejectQuest
		/// </summary>
		/// <remarks>Tested</remarks>
		OfferQuest,
		/// <summary>
		/// Quest P:Type[Typename:string](Current Quest) abort is offered to player via customdialog with message Q:string by NPC
		/// if player accepts GamePlayerEvent.AbortQuest is fired, else GamePlayerEvent.ContinueQuest
		/// </summary>
		/// <remarks>Tested</remarks>
		OfferQuestAbort,
		/*
			GSPE : monster index p## speed is set to q##            
			IATK : monster index p## attacks player, or first monster from monster index q##
			INFC : reduces faction p## by q## points
			IPFC : increases faction p## by q## points            
		 * */
		/// <summary>
		/// GUIL : guild of Q:GameLiving(NPC)[NPC's ID:string] is set to P:string (not player's guilds)
		/// </summary>
		SetGuildName,
		/// <summary>
		/// IPTH : monster Q:GameNPC(NPC)[NPC's ID:string] is assigned to path P:PathPoint
		/// </summary>
		SetMonsterPath,
		/// <summary>
		/// IQST : Quest P:Type[Typename:string](Current Quest) is set to step Q:int[string]
		/// </summary>
		/// <remarks>Tested</remarks>
		SetQuestStep,
		/// <summary>
		/// KQST : Aborts quest P:Type[Typename:string](Current Quest)
		/// </summary>  
		/// <remarks>Tested</remarks>
		AbortQuest,
		/// <summary>
		/// MES : Displays a message P:string of Texttype Q:TextType(Emote)
		/// </summary>
		Message,
		//    LIST : activates action list p## (action lists described in a following section)
		/// <summary>
		/// MGEN : Monster P:GameLiving[NPC's ID:string] will spawn considering all its requirements
		/// </summary>
		/// <remarks>Tested</remarks>
		MonsterSpawn,
		/// <summary>
		/// Monster P:GameLiving[NPC's ID:string] will unspawn considering all its requirements
		/// </summary>
		/// <remarks>Tested</remarks>
		MonsterUnspawn,
		/*
			NFAC : sets faction p## to a negative value specified by q##
			ORDE : changes trade order to p## one (???)
			PATH : path is set to p##
			PFAC : sets faction p## to a positive value specified by q##		
			RUMO : says a random quest teaser from zones p## and q##
			UOUT : set the fort p## to the realm q##
			SGEN : monster index p## will spawn slot q## up to its max        
		 * */
		/// <summary>
		/// SINV : Item P:ItemTemplate[Item's Id_nb:string] is replaceb by item Q:ItemTemplate[Item's Id_nb:string] in inventory of player
		/// </summary>
		/// <remarks>Tested</remarks>
		ReplaceItem,
		/// <summary>
		/// SQST : Increments queststep of quest P:Type[TypeName:string](Current Quest)
		/// </summary>
		/// <remarks>Tested</remarks>
		IncQuestStep,
		/// <summary>
		/// TALK : Q:GameLiving(NPC) says message P:string locally (local channel)
		/// </summary>
		/// <remarks>Tested</remarks>
		Talk,
		/// <summary>
		/// TELE : teleports player to destination P:GameLocation with a random distance of Q:int(0)[string]
		/// </summary>
		Teleport,
		/// <summary>
		/// TIMR : regiontimer P:RegionTimer starts to count Q:int[string] milliseconds        
		/// </summary>
		CustomTimer,
		/// <summary>
		/// TMR# :  timer P:string starts to count Q:int[string] milliseconds
		/// </summary>
		/// <remarks>Tested</remarks>
		Timer,                        
		//TREA : drops treasure index p##        
		/// <summary>
		/// WHIS : Q:GameLiving(NPC) whispers message P:string to player
		/// </summary>
		Whisper
		//XFER : changes to talk index p##. MUST be placed in entry 19 !            
	}


	public enum eSelectorType : byte {
		QuestType,
		GameLiving,
		GameNPC,
		Item,
		Location,
		Area
	}		

    /// <summary>
    /// If one trigger and all requirements are fulfilled the corresponding actions of
    /// a QuestAction will we executed one after another. Actions can be more or less anything:
    /// at the moment there are: GiveItem, TakeItem, Talk, Give Quest, Increase Quest Step, FinishQuest,
    /// etc....
    /// </summary>
    public class BaseQuestAction : AbstractQuestAction
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Player Constant will be replaced by players name in output messages.
		/// </summary>
		const string PLAYER = "{Player}";
        /// <summary>
        /// Constant used to store timerid in RegionTimer.Properties
        /// </summary>
        const string TIMER_ID = "timerid";
        /// <summary>
        /// Constant used to store GameLiving Source in RegionTimer.Properties
        /// </summary>
        const string TIMER_SOURCE = "timersource";
		
		/// 
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseQuestAction"/> class.
		/// </summary>
		/// <param name="questPart">Parent QuestPart of this Action</param>
		/// <param name="actionType">Type of action</param>
		/// <param name="p">Action Parameter</param>
		/// <param name="q">Action Parameter</param>
        public BaseQuestAction(BaseQuestPart questPart,eActionType actionType, Object p, Object q): base(questPart, actionType, p, q)
        {            
            switch (ActionType)
            {
                case eActionType.SetQuestStep:
                    {
                        if (P is string && ((string)P).Length > 0)
							P = ScriptMgr.GetType(Convert.ToString(P));

						if (!(P is Type))
							P = QuestType;

						if (Q is string)
							Q = Convert.ToInt32(Q);

                        if (!(Q is int))
                            throw new ArgumentException("Variable Q must be queststep for quest based actions. ActionType:" + ActionType, "Q");
                        break;
                    }
                case eActionType.IncQuestStep:
                case eActionType.FinishQuest:
                case eActionType.AbortQuest:                
                    {
                        if (P is string && ((string)P).Length > 0)
							P = ScriptMgr.GetType(Convert.ToString(P));

                        if (!(P is Type))
							P = QuestType;
                        break;
                    }
				case eActionType.GiveQuest:
					{
                        if (P is string && ((string)P).Length > 0)
							P = ScriptMgr.GetType(Convert.ToString(P));

						if (!(P is Type))
							P = QuestType;

						Q = QuestMgr.ResolveNPC(Q, NPC);

						break;
					}
                case eActionType.OfferQuest:
                case eActionType.OfferQuestAbort:
                    {
                        if (P is string && ((string)P).Length > 0)
							P = ScriptMgr.GetType(Convert.ToString(P));

                        if (!(P is Type))
							P = QuestType;

                        if (!(Q is string))
                            throw new ArgumentException("Variable Q must be message(string) for quest based actions. ActionType:" + ActionType, "Q");
                        break;
                    }
                case eActionType.GiveItem:
                case eActionType.TakeItem:
                case eActionType.DropItem:
                case eActionType.DestroyItem:
                    {
						if (P is string)
							P = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), Convert.ToString(P));

                        if (!(P is ItemTemplate))
                            throw new ArgumentException("Variable P must be itemtemplate for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.ReplaceItem:
                    {
						if (P is string)
							P = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), Convert.ToString(P));

                        if (!(P is ItemTemplate))
                            throw new ArgumentException("Variable P must be itemtemplate for actionType:" + ActionType, "P");

						if (Q is string)
							Q = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), Convert.ToString(Q));

						if (!(Q is ItemTemplate))
                            throw new ArgumentException("Variable Q must be itemtemplate for actionType:" + ActionType, "Q");
                        break;
                    }
                case eActionType.GiveXP:
                    {
						if (P is string)
							P = Convert.ToInt64(P);

                        if (!(P is Int64))
                            throw new ArgumentException("Variable P must be xp(long) for actionType:" + ActionType, "P");
                        break;
                    }

                case eActionType.GiveGold:
                case eActionType.TakeGold:
                    {
						if (P is string)
							P = Convert.ToInt64(P);

                        if (!(P is Int64))
                            throw new ArgumentException("Variable P must be copper(long) for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.Talk:
                    {
						Q = QuestMgr.ResolveLiving(Q, NPC);

                        if (!(P is string))
                            throw new ArgumentException("Variable P must be string for actionType:" + ActionType, "P");
                        break;
                    }

                case eActionType.CustomDialog:
                    {
                        if (!(P is string))
                            throw new ArgumentException("Variable P must be string for actionType:" + ActionType, "P");

                        if (!(Q is CustomDialogResponse))
                            throw new ArgumentException("Variable Q must be CustomDialogResponse for actionType:" + ActionType, "Q");

                        break;
                    }
                case eActionType.Message:                
                    {
						if (!(Q is eTextType))
							throw new ArgumentException("Variable Q must be eTextType for actionType:" + ActionType, "Q");

                        if (!(P is string))
                            throw new ArgumentException("Variable P must be string for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.Animation:
                    {
						if (P is string)
							P = (eEmote) Convert.ToInt32(P);

                        if (!(P is eEmote))
                            throw new ArgumentException("Variable P must be eEmote for actionType:" + ActionType, "P");

						Q = QuestMgr.ResolveLiving(Q);						

						break;
                    }                
                case eActionType.Teleport:
                    {
                        if (!(P is GameLocation))
                            throw new ArgumentException("Variable P must be GameLocation for actionType:" + ActionType, "P");
                        
						if (Q is string)
							Q = Convert.ToInt32(Q);
						break;

                    }
                case eActionType.CustomTimer:
                    {
                        if (!(P is RegionTimer))
                            throw new ArgumentException("Variable P must be RegionTimer for actionType:" + ActionType, "P");

						if (Q is string)
							Q = Convert.ToInt32(Q);
                        if (!(Q is int))
                            throw new ArgumentException("Variable Q must be delay(int) for actionType:" + ActionType, "Q");

                        break;
                    }
                case eActionType.Timer:
                    {
						if (Q is string)
							Q = Convert.ToInt32(Q);

                        if (!(Q is int))
                            throw new ArgumentException("Variable Q must be delay(int) for actionType:" + ActionType, "Q");
                        if (!(P is string))
                            throw new ArgumentException("Variable P must be timername(string) for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.MonsterSpawn:					
					// we have to fetch the mob from database since it doesn't exist in the region yet.
					P = QuestMgr.ResolveLiving(P,NPC,true);
					break;
                case eActionType.MonsterUnspawn:					
					P = QuestMgr.ResolveLiving(P,NPC);
					break;
				case eActionType.SetMonsterPath:
					//if (!(P is PathPoint))
					//	throw new ArgumentException("Variable P must be PathPoint for actionType:" + ActionType, "P");

					Q = QuestMgr.ResolveNPC(Q,NPC);
					break;
				case eActionType.SetGuildName:									
					if (!(P is string))
						throw new ArgumentException("Variable P must be string for actionType:" + ActionType, "P");

					Q = QuestMgr.ResolveLiving(Q,NPC);
					break;
				case eActionType.Whisper:
					
					Q = QuestMgr.ResolveLiving(Q, NPC);

					if (!(P is string))
						throw new ArgumentException("Variable P must be string for actionType:" + ActionType, "P");
					break;
            }
        }
		
		public static bool IncreaseQuestStep(GamePlayer player, Type questType)
		{
			AbstractQuest playerQuest = player.IsDoingQuest(questType) as AbstractQuest;
			if (playerQuest != null)
			{
				playerQuest.Step = playerQuest.Step++;
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool SetQuestStep(GamePlayer player, Type questType, byte step)
		{
			AbstractQuest playerQuest = player.IsDoingQuest(questType) as AbstractQuest;
			if (playerQuest != null)
			{
				playerQuest.Step = step;
				return true;
			}
			else
				return false;
		}

		public static bool GiveItem(GamePlayer player, ItemTemplate item, GameNPC npc)
		{
            InventoryItem inventoryItem = new InventoryItem(item);
			player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, inventoryItem);
			if (npc == null)
			{
				player.Out.SendMessage("You receive " + item.Name + ".", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
			}
			else
			{
				player.Out.SendMessage("You receive " + item.Name + " from " + npc.GetName(0, false) + ".", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
			}
			return true;
		}

		public static bool ReplaceItem(GamePlayer player, ItemTemplate oldItem, ItemTemplate newItem)
		{
            if (player.Inventory.RemoveTemplate(oldItem.Id_nb, 1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                InventoryItem inventoryItem = new InventoryItem(newItem);
                return player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, inventoryItem);                
            }
            else
                return false;
		}

		public static bool TakeItem(GamePlayer player, ItemTemplate itemToRemove, int count, GameNPC npc)
		{
			Hashtable dataSlots = new Hashtable(10);
			lock(player.Inventory)
			{
				ICollection allBackpackItems = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				
				bool result = false;
				foreach (InventoryItem item in allBackpackItems)
				{
					if (item.Name == itemToRemove.Name)
					{
						
						if(item.IsStackable) // is the item is stackable
						{
							if(item.Count >= count)
							{
								if(item.Count == count)
								{
									dataSlots.Add(item, null);
								}
								else
								{
									dataSlots.Add(item, count);
								}
								result = true;
								break;
							}
							else
							{
								dataSlots.Add(item, null);
								count-= item.Count;
							}
						}
						else
						{
							dataSlots.Add(item, null);
							if(count <= 1)
							{
								result = true;
								break;
							}
							else
							{
								count--;
							}
						}
					}
				}
				if(result == false)
				{
					return false;
				}
			}

			GamePlayerInventory playerInventory = player.Inventory as GamePlayerInventory;
			playerInventory.BeginChanges();
			foreach(DictionaryEntry de in dataSlots)
			{
				if(de.Value == null)
				{
					playerInventory.RemoveItem((InventoryItem)de.Key);
				}
				else
				{
					playerInventory.RemoveCountFromStack((InventoryItem)de.Key, (int)de.Value);
				}
			}
			playerInventory.CommitChanges();
			
			if (npc != null)
			{
				player.Out.SendMessage("You give " + itemToRemove.Name + " to " + npc.GetName(0, false) + ".", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
			}
			else
			{
				player.Out.SendMessage("You give " + itemToRemove.Name + ".", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
			}
			
			return true;
		}

		public static bool DropItem(GamePlayer player, ItemTemplate item)
		{
            InventoryItem inventoryItem = new InventoryItem(item);
			player.CreateItemOnTheGround(inventoryItem);
			player.Out.SendMessage(item.Name + " drops in front of you.", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
			return true;
		}

		public static bool DestroyItem(GamePlayer player, ItemTemplate itemToDestroy, int count)
		{	
			Hashtable dataSlots = new Hashtable(10);
			lock(player.Inventory)
			{
				ICollection allBackpackItems = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				
				bool result = false;
				foreach (InventoryItem item in allBackpackItems)
				{
					if (item.Name == itemToDestroy.Name)
					{

						if(item.IsStackable) // is the item is stackable
						{
							if(item.Count >= count)
							{
								if(item.Count == count)
								{
									dataSlots.Add(item, null);
								}
								else
								{
									dataSlots.Add(item, count);
								}
								result = true;
								break;
							}
							else
							{
								dataSlots.Add(item, null);
								count-= item.Count;
							}
						}
						else
						{
							dataSlots.Add(item, null);
							if(count <= 1)
							{
								result = true;
								break;
							}
							else
							{
								count--;
							}
						}
					}
				}
				if(result == false)
				{
					return false;
				}
			}

			GamePlayerInventory playerInventory = player.Inventory as GamePlayerInventory;
			playerInventory.BeginChanges();
			foreach(DictionaryEntry de in dataSlots)
			{
				if(de.Value == null)
				{
					playerInventory.RemoveItem((InventoryItem)de.Key);
				}
				else
				{
					playerInventory.RemoveCountFromStack((InventoryItem)de.Key, (int)de.Value);
				}
			}
			playerInventory.CommitChanges();


			player.Out.SendMessage(itemToDestroy.Name + " is destroyed.", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
			
			return true;
		}

		public static bool SendMessage(GamePlayer player, string message, eTextType textType)
		{
			message = GetPersonalizedMessage(message, player);
			switch (textType)
			{
				case eTextType.Dialog:
					player.Out.SendCustomDialog(message, null);
					break;
				case eTextType.Emote:
					player.Out.SendMessage(message, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
					break;
				case eTextType.Broadcast:
					foreach (GameClient clientz in WorldMgr.GetAllPlayingClients())
					{
						clientz.Player.Out.SendMessage(message, eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
					}
					break;
				case eTextType.Read:
					player.Out.SendMessage("You read: \"" + message + "\"", eChatType.CT_Emote, eChatLoc.CL_PopupWindow);
					break;
				default:
					return false;
			}
			return true;
		}

		public static bool Animation(GameLiving actor, eEmote emote)
		{
			foreach (GamePlayer nearPlayer in actor.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				nearPlayer.Out.SendEmoteAnimation(actor, emote);
			}
			return true;
		}

		public static bool Attack(GamePlayer player, GameNPC attacker, int aggroAmount)
		{
			if (attacker.Brain is IAggressiveBrain)
			{
				IAggressiveBrain brain = (IAggressiveBrain)attacker.Brain;
				brain.AddToAggroList(player, aggroAmount);
				return true;
			}
			else
			{
				log.Warn("Non agressive mob " + attacker.Name + " was order to attack player. This goes against the first directive and will not happen");
				return false;
			}
		}

		public static bool Teleport(GamePlayer player, GameLocation location, int radius) {
			if (location.Name != null)
            {
                player.Out.SendMessage(player + " is being teleported to " + location.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        	
        	location.X += Util.Random(-radius, radius);
        	location.Y += Util.Random(-radius, radius);
            return player.MoveTo(location.RegionID, location.X,location.Y,location.Z, location.Heading);			
		}

		public static bool Talk(GamePlayer player, GameNPC npc, string message)
		{
			message = GetPersonalizedMessage(message, player);
			npc.TurnTo(player);
			npc.SayTo(player, message);
			return true;
		}

		public static bool Whisper(GamePlayer player, GameNPC npc, string message)
		{
			message = GetPersonalizedMessage(message, player);
			npc.TurnTo(player);
			npc.Whisper(player, message);
			return true;
		}

		/*public static bool SetMovementPath(GameNPC npc, PathPoint p)
		{
			if (npc.Brain is RoundsBrain)
			{
				npc.CurrentWayPoint = p;
				MovementMgr.Instance.MoveOnPath(npc, npc.MaxSpeed);
				return true;
			}
			else
			{
				log.Warn("Mob without RoundsBrain was assigned to walk along Path");
				return false;
			}
		}*/

		public static bool MonsterSpawn(GameLiving living)
		{
			if (living.AddToWorld())
			{
				// appear with a big buff of magic
				foreach (GamePlayer visPlayer in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					visPlayer.Out.SendSpellCastAnimation(living, 1, 20);
				}
				return true;
			}
			else
				return false;

		}

		public static bool MonsterUnspawn(GameLiving living)
		{
			return living.RemoveFromWorld();			
		}

		public static bool Timer(GamePlayer player, string timername, int delay)
		{
			RegionTimer timer = new RegionTimer(player, new RegionTimerCallback(QuestTimerCallBack));
			timer.Properties.setProperty(TIMER_ID, timername);
			timer.Properties.setProperty(TIMER_SOURCE, player);
			timer.Start(delay);
			return true;
		}

        /// <summary>
        /// Action performed 
        /// Can be used in subclasses to define special behaviour of actions
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>        
        public override void Perform(DOLEvent e, object sender, EventArgs args, GamePlayer player)
        {
            switch (ActionType)
            {
                case eActionType.IncQuestStep:
                    {                        
						IncreaseQuestStep(player, (Type)P);                        
                        break;
                    }
                case eActionType.SetQuestStep:
                    {
						SetQuestStep(player, (Type)P, Convert.ToByte(Q));                        
                        break;
                    }
                case eActionType.FinishQuest:
                    {                        
						AbstractQuest quest = player.IsDoingQuest((Type)P);
						if (quest != null)
							quest.FinishQuest();						
                        break;
                    }
                case eActionType.GiveQuest:
                    {                        						
						QuestMgr.GiveQuestToPlayer((Type)P, player, (GameNPC)Q);
                        break;
                    }
                case eActionType.OfferQuest:
                    {
                        string message = GetPersonalizedMessage(Convert.ToString(Q),player);                        
						QuestMgr.ProposeQuestToPlayer((Type)P, message, player,NPC);
                        break;
                    }
                case eActionType.OfferQuestAbort:
                    {
                        string message = GetPersonalizedMessage(Convert.ToString(Q),player);                        
						QuestMgr.AbortQuestToPlayer((Type)P, message, player, NPC);
                        break;
                    }
                case eActionType.AbortQuest:
                    {                        
						AbstractQuest quest = player.IsDoingQuest((Type)P);
						if (quest != null)
							quest.AbortQuest();
                        break;
                    }
                case eActionType.GiveItem:
                    {
                        ItemTemplate item = (ItemTemplate)P;
						GameNPC npc = Q is GameNPC ? (GameNPC) Q : NPC;
						GiveItem(player, item, npc);
                        break;
                    }

                case eActionType.ReplaceItem:
                    {
                        ItemTemplate newItem = (ItemTemplate)Q;
                        ItemTemplate oldItem = (ItemTemplate)P;
					   ReplaceItem(player, oldItem, newItem);
                  
                       break;
                    }
                case eActionType.TakeItem:
                    {
                        ItemTemplate itemToRemove = (ItemTemplate)P;
                        int amount = Q != null ? Convert.ToInt32(Q) : 1;
						TakeItem(player, itemToRemove, amount,NPC);                        
                        break;
                    }
                case eActionType.DropItem:
                    {
                        ItemTemplate item = (ItemTemplate)P;
						DropItem(player, item);                        
                        break;
                    }
                case eActionType.DestroyItem:
                    {
                        ItemTemplate itemToDestroy = (ItemTemplate)P;
                        int amount = Q != null ? Convert.ToInt32(Q) : 1;
						DestroyItem(player, itemToDestroy, amount);
                        break;
                    }
                case eActionType.GiveXP:
                    {
                        long xp = Convert.ToInt64(P);
                        player.GainExperience(xp);
                        break;
                    }

                case eActionType.GiveGold:
                    {
                        long copper = Convert.ToInt64(P);
                        player.AddMoney(copper);
                        break;
                    }
                case eActionType.TakeGold:
                    {
                        long copper = Convert.ToInt64(P);
                        player.RemoveMoney(copper);
                        break;
                    }
                case eActionType.Talk:
                    {						                
						Talk(player, (GameNPC)Q,Convert.ToString(P));
                        break;
                    }
				case eActionType.Whisper:
					{									
						Whisper(player, (GameNPC)Q, Convert.ToString(P));						
						break;
					}
                case eActionType.CustomDialog:
                    {
                        string message = GetPersonalizedMessage(Convert.ToString(P),player);
                        CustomDialogResponse response = Q as CustomDialogResponse;
                        player.Out.SendCustomDialog(message, response);
                        break;
                    }
                case eActionType.Message:
                    {						                        
						SendMessage(player,Convert.ToString(P), (eTextType) Q);
                        break;
                    }                
                case eActionType.Animation:
                    {                        
                        GameLiving actor = Q is GameLiving ? (GameLiving)Q : player;
                        Animation(actor, (eEmote)P);
                        break;
                    }
                case eActionType.Attack:
                    {
						GameNPC attacker = (GameNPC)Q;
						int aggroAmount = P != null ? Convert.ToInt32(P) : player.Level << 1;
						Attack(player, attacker, aggroAmount);
                        break;
                    }
                case eActionType.WalkTo:
                    {
						GameNPC npc = (GameNPC)Q;
                        IPoint3D location = (P is IPoint3D) ? (IPoint3D)P : player;
                        npc.WalkTo(location, npc.CurrentSpeed);
                        break;
                    }
				case eActionType.WalkToSpawn:
					{
						GameNPC npc = (GameNPC)Q;
						npc.WalkToSpawn();
						break;
					}
				case eActionType.MoveTo:
					{
						GameLiving npc = (GameLiving)Q;
						if (P is GameLocation)
						{
							GameLocation location = (GameLocation)P;
							npc.MoveTo(location.RegionID, location.X,location.Y,location.Z, location.Heading);
						}
						else
						{
							npc.MoveTo(player.CurrentRegionID, player.X,player.Y,player.Z, (ushort)player.Heading);							
						}						
						
						break;
					}                                
                case eActionType.Teleport:
                    {                        
                        int radius = Q != null ? Convert.ToInt32(Q) : 0;
                        Teleport(player, (GameLocation)P, radius);
                        break;
                    }
                case eActionType.CustomTimer:
                    {
                        RegionTimer timer = (RegionTimer)P;                        
                        timer.Start(Convert.ToInt32(Q));
                        break;
                    }
                case eActionType.Timer:
                    {                        
						Timer(player, Convert.ToString(P), Convert.ToInt32(Q));                        
                        break;
                    }
                case eActionType.MonsterSpawn:
                    {                        
						MonsterSpawn((GameLiving)P);
                        break;
                    }

                case eActionType.MonsterUnspawn:
                    {						
						MonsterUnspawn((GameLiving)P);
                        break;
                    }
				case eActionType.SetMonsterPath:
					{
						//SetMovementPath((GameNPC)Q,(PathPoint)P);
						break;
					}
				case eActionType.SetGuildName:
					{
						GameLiving npc = (GameLiving)Q;
						npc.GuildName = Convert.ToString(P);
						break;
					}
            }
        }

        /// <summary>
        /// Callback for quest internal timers used via eActionType.Timer and eTriggerType.Timer
        /// </summary>
        /// <param name="callingTimer"></param>
        /// <returns>0</returns>
        private static int QuestTimerCallBack(RegionTimer callingTimer)
        {
            string timerid = callingTimer.Properties.getObjectProperty(TIMER_ID, null) as string;
            if (timerid == null)
                throw new ArgumentNullException("TimerId out of Range", "timerid");

            GameLiving source = callingTimer.Properties.getObjectProperty(TIMER_SOURCE, null) as GameLiving;
            if (source == null)
                throw new ArgumentNullException("TimerSource null", "timersource");


            TimerEventArgs args = new TimerEventArgs(source, timerid);
            source.Notify(GameLivingEvent.Timer, source, args);

            return 0;
        }

		/// <summary>
		/// Personalizes the given message by replacing all instances of PLAYER with the actual name of the player
		/// </summary>
		/// <param name="message">message to personalize</param>
		/// <param name="player">Player's name to insert</param>
		/// <returns>message with actual name of player instead of PLAYER</returns>
		public static string GetPersonalizedMessage(string message, GamePlayer player)
		{
			if (message == null || player == null)
				return message;

			string playerMessage;
			int playerIndex = message.IndexOf(PLAYER);
			if (playerIndex == 0)
				playerMessage = message.Replace(PLAYER, player.GetName(0, true));
			else if (playerIndex > 0)
				playerMessage = message.Replace(PLAYER, player.GetName(0, false));
			else
				playerMessage = message;

			return playerMessage;
		}
    }
}
