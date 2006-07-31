using System;
using System.Text;
using DOL.Events;
using DOL.Database;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests
{

    /// <summary>
    /// Triggertype defines a list of available triggers to use within questparts.
    /// Depending on triggertype triggerVariable and triggerkeyword will have special
    /// meaning look at documentation of each triggertype for details
    /// </summary>
    ///<remarks>
    /// Syntax: ... I:eEmote(eEmote.Yes) ... Parameter I must be of Type
    /// eEmote and has the default value of "eEmote.Yes" (used if no value is passed).
    /// If no default value is defined value must be passed along with trigger.
    /// </remarks>
    public enum eTriggerType : byte
    {
        /// <summary>
        /// No Triggertype at all, needed since triggertype cannot be null when passed as an argument
        /// </summary>
        None = 0x00,
        //ATTA : Monster acquires a target and initiate an attack                 
        /// <summary>
        /// AQST : player accepts quest I:Type
        /// </summary>       
        /// <remarks>Tested</remarks>
        AcceptQuest = 0x01,
        /// <summary>
        /// DEAT : Enemy I:GameLiving died, no matter who/what killed it
        /// </summary>
        EnemyDying = 0x02,
        /// <summary>
        /// DQST : Player declines quest I:Type
        /// </summary>
        /// <remarks>Tested</remarks>
        DeclineQuest = 0x03,
        /// <summary>
        /// Continue quest I:Type after abort quest offer
        /// </summary>
        /// <remarks>Tested</remarks>
        ContinueQuest = 0x04,
        /// <summary>
        /// Abort quest I:Type after abort quest offer
        /// </summary>
        /// <remarks>Tested</remarks>
        AbortQuest = 0x05,
        //INRA : player enters interaction radius (checked each second, please use timers)
        /// <summary>
        /// player enters area I:IArea        
        /// </summary>
        /// <remarks>Tested</remarks>
        EnterArea = 0x06,
        /// <summary>
        /// player leaves area I:IArea
        /// </summary>
        /// <remarks>Tested</remarks>
        LeaveArea = 0x07,
        /// <summary>
        /// INTE : player right-clicks NPC:GameNPC
        /// NPC to interact with is NPC of BaseQuestPart
        /// </summary>
        /// <remarks>Tested</remarks>
        Interact = 0x08,
        /// <summary>
        ///  KILL : monster I:GameLiving kills the quest player
        /// </summary>
        PlayerKilled = 0x09,
        /// <summary>
        /// PKIL : a player (and only a player) kills the monster defined by K:string od I:GameLiving
        /// if TriggerKeyword != null
        ///     triggerkeyword = name of monster
        /// else
        ///     I = monster object
        /// </summary>        
        EnemyKilled = 0x0A,
        /// <summary>
        /// TAKE : Player gives item I:ItemTemplate to NPC
        /// </summary>
        /// <remarks>Tested</remarks>
        GiveItem = 0x0B,
        /// <summary>
        /// WORD : player whispers the word K:string to NPC (from QuestPart)        
        /// </summary>
        /// <remarks>Tested</remarks>
        Whisper = 0x0C,

        //TIME : ingame time reaches I
        /// <summary>
        /// TMR# :  timer with id K:string has finished
        /// </summary>
        Timer = 0x0D,
        /*
        Description of item specific trigger types :
        BLOC : item blocks an attack
        BOUG : item is bought
        DROP : item is droped
        EREG : item's holder enters a region
        EVAD : item's holder evades an attack
        FUMB : item fumbles an attack
        GET : item is picked up
        HIT : item's holder is hit
        LREG : item's holder leaves a region
        MISS : item's holder is missed
        PARR : item parries an attack
        REMO : item is removed
        SHEA : item is sheathed
        SOLD : item is sold
        SPEL : item's holder casts a spell
        STRI : item hits a monster
        STYL : item's holder performs a style
         * */
        /// <summary>
        /// USED : Item I:ItemTemplate is used by player
        /// </summary>
        /// <remarks>Tested</remarks>
        ItemUsed = 0x0F
        /*
            WIEL : item is wield
            WORN : item is worn
            */
    }

    /// <summary>
    /// A trigger defines the circumstances under which a certain QuestAction is fired.
    /// This can be eTriggerAction.Interact, eTriggerAction.GiveItem, eTriggerAction.Attack, etc...
    /// Additional there are two variables to add the needed parameters for the triggertype (Item to give for GiveItem, NPC to interact for Interact, etc...). To fire a QuestAction at least one of the added triggers must be fulfilled. 
    /// </summary>        
    public class BaseQuestTrigger : AbstractQuestTrigger
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
        /// </summary>
        /// <param name="type">Triggertype</param>
        /// <param name="keyword">keyword (K), meaning depends on triggertype</param>
        /// <param name="var">variable (I), meaning depends on triggertype</param>
        public BaseQuestTrigger(BaseQuestPart questPart, eTriggerType type, String keyword, object var) : base(questPart, type,keyword,var)
        {            
            //Do some pre error checking for wrong parameters
            switch (TriggerType)
            {
                case eTriggerType.Interact:
                    {
                        if (K != null)
                            throw new ArgumentException("No Keyword with eTriggerType.Interact allowed :keyword" + K, "K");
                        break;
                    }
                case eTriggerType.Whisper:
                    {
                        if (K == null)
                            throw new ArgumentNullException("Keyword needed for eTriggerType.Whisper", "K");
                        break;
                    }
                case eTriggerType.GiveItem:
                    {
                        if (!(I is ItemTemplate))
                            throw new ArgumentException("Variable I must be of type ItemTemplate for item based triggertypes: " + TriggerType, "I");
                        break;
                    }
                case eTriggerType.AcceptQuest:
                case eTriggerType.DeclineQuest:
                case eTriggerType.AbortQuest:
                case eTriggerType.ContinueQuest:
                    {
                        if (!(I is Type))
                            throw new ArgumentException("Variable I must be type of quest for Quest based triggertypes: " + TriggerType, "I");
                        break;
                    }
                case eTriggerType.EnemyKilled:
                    {
                        if (!(I is GameLiving) && K == null)
                            throw new ArgumentException("Variable I must be GameLiving or K name of mob for triggertypes: " + TriggerType, "I");
                        break;
                    }
                case eTriggerType.EnemyDying:
                    {
                        if (!(I is GameLiving))
                            throw new ArgumentException("Variable I must be GameLiving for triggertypes: " + TriggerType, "I");
                        break;
                    }

                case eTriggerType.PlayerKilled:
                    {
                        break;
                    }
                case eTriggerType.EnterArea:
                case eTriggerType.LeaveArea:
                    {
                        if (!(I is IArea))
                            throw new ArgumentException("Variable I must be area for Area based triggertypes: " + TriggerType, "I");
                        break;
                    }
                case eTriggerType.Timer:
                    {
                        if (K == null)
                            throw new ArgumentException("Variable K must be name of timer for eTriggerType.Timer.", "K");
                        break;
                    }
                case eTriggerType.ItemUsed:
                    {
                        if (!(I is ItemTemplate))
                            throw new ArgumentException("Variable I must be ItemTemplate for eTriggerType.Timer.", "I");
                        break;
                    }
            }
        }

        /// <summary>
        /// Checks the trigger, this method is called whenever a event associated with this questparts quest
        /// or a manualy associated eventhandler is notified.
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>
        /// <returns>true if QuestPart should be executes, else false</returns>
        public override bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player)
        {
            bool result = false;

            switch (TriggerType)
            {
                case eTriggerType.Interact:
                    {
                        result |= (e == GameObjectEvent.Interact && sender == NPC && K == null);
                        break;
                    }
                case eTriggerType.Whisper:
                    {
                        if (e == GameLivingEvent.WhisperReceive && sender == NPC)
                        {
                            WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
                            result |= wArgs.Text != null && K == wArgs.Text;
                        }
                        break;
                    }
                case eTriggerType.GiveItem:
                    {
                        if (e == GamePlayerEvent.GiveItem)
                        {
                            GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
                            ItemTemplate item = I as ItemTemplate;
                            result |= (gArgs.Target == NPC &&
                                            item != null && gArgs.Item.Id_nb == item.Id_nb);
                        }
                        break;
                    }
                case eTriggerType.AcceptQuest:
                    {
                        if (e == GamePlayerEvent.AcceptQuest)
                        {
                            Type questType = (Type)I;

                            QuestEventArgs qArgs = (QuestEventArgs)args;
                            result |= (qArgs.Player.ObjectID == player.ObjectID && qArgs.QuestID == QuestMgr.GetIDForQuestType(questType));
                        }
                        break;
                    }
                case eTriggerType.DeclineQuest:
                    {
                        if (e == GamePlayerEvent.DeclineQuest)
                        {
                            Type questType = (Type)I;

                            QuestEventArgs qArgs = (QuestEventArgs)args;
                            result |= (qArgs.Player.ObjectID == player.ObjectID && qArgs.QuestID == QuestMgr.GetIDForQuestType(questType));
                        }
                        break;
                    }
                case eTriggerType.AbortQuest:
                    {
                        if (e == GamePlayerEvent.AbortQuest)
                        {
                            Type questType = (Type)I;

                            QuestEventArgs qArgs = (QuestEventArgs)args;
                            result |= (qArgs.Player.ObjectID == player.ObjectID && qArgs.QuestID == QuestMgr.GetIDForQuestType(questType));
                        }
                        break;
                    }
                case eTriggerType.ContinueQuest:
                    {
                        if (e == GamePlayerEvent.ContinueQuest)
                        {
                            Type questType = (Type)I;

                            QuestEventArgs qArgs = (QuestEventArgs)args;
                            result |= (qArgs.Player.ObjectID == player.ObjectID && qArgs.QuestID == QuestMgr.GetIDForQuestType(questType));
                        }
                        break;
                    }
                case eTriggerType.EnemyKilled:
                    {
                        if (e == GameLivingEvent.EnemyKilled && sender == player)
                        {

                            EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                            if (K != null)
                                result |= gArgs.Target.Name == K;
                            else
                            {
                                result |= gArgs.Target == (GameLiving)I;
                            }
                        }
                        break;
                    }
                case eTriggerType.EnemyDying:
                    {
                        if (e == GameLivingEvent.Dying)
                        {
                            //DyingEventArgs dArgs = (DyingEventArgs)args;
                            GameLiving living = (GameLiving)I;
                            result |= sender == living;
                        }
                        break;
                    }

                case eTriggerType.PlayerKilled:
                    {
                        if (e == GameLivingEvent.EnemyKilled && sender == NPC)
                        {
                            EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                            result |= gArgs.Target == player;
                        }
                        break;
                    }
                case eTriggerType.EnterArea:
                    {
                        if (e == AreaEvent.PlayerEnter)
                        {
                            IArea area = (IArea)I;
                            AreaEventArgs aArgs = (AreaEventArgs)args;
                            result |= aArgs.GameObject == player && area == aArgs.Area;
                        }
                        break;
                    }
                case eTriggerType.LeaveArea:
                    {
                        if (e == AreaEvent.PlayerLeave)
                        {
                            IArea area = (IArea)I;
                            AreaEventArgs aArgs = (AreaEventArgs)args;
                            result |= aArgs.GameObject == player && area == aArgs.Area;
                        }
                        break;
                    }
                case eTriggerType.Timer:
                    {
                        if (e == GameLivingEvent.Timer)
                        {
                            TimerEventArgs tArgs = (TimerEventArgs)args;
                            result |= K == tArgs.TimerID && tArgs.Source == player;
                        }
                        break;
                    }
                case eTriggerType.ItemUsed:
                    {
                        if (e == GamePlayerEvent.UseSlot)
                        {
                            ItemTemplate itemTemplate = (ItemTemplate)I;
                            UseSlotEventArgs uArgs = (UseSlotEventArgs)args;
                            InventoryItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot);
                            result |= (item != null && itemTemplate.Id_nb == item.Id_nb);
                        }
                        break;
                    }
            }
            return result;
        }

        public override void Register()
        {
            switch (TriggerType)
            {
                case eTriggerType.Interact:
                case eTriggerType.Whisper:
                    GameEventMgr.AddHandlerUnique(NPC, GameLivingEvent.WhisperReceive, new DOLEventHandler(QuestPart.Notify));
                    GameEventMgr.AddHandlerUnique(NPC, GameLivingEvent.Interact, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.AcceptQuest:
                    GameEventMgr.AddHandlerUnique(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.DeclineQuest:
                    GameEventMgr.AddHandlerUnique(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.EnemyDying:
                    GameEventMgr.AddHandlerUnique(NPC, GameLivingEvent.Dying, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.EnterArea:
                    //IArea area = (IArea)I;
                    //GameEventMgr.AddHandlerUnique(area, AreaEvent.PlayerEnter, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.LeaveArea:
                    //area = (IArea)I;
                    //GameEventMgr.AddHandlerUnique(area, AreaEvent.PlayerLeave, new DOLEventHandler(QuestPart.Notify));
                    break;
                default:
                    break;
            }
        }

        public override void Unregister()
        {
            switch (TriggerType)
            {
                case eTriggerType.Interact:
                case eTriggerType.Whisper:
                    GameEventMgr.RemoveHandler(NPC, GameLivingEvent.WhisperReceive, new DOLEventHandler(QuestPart.Notify));
                    GameEventMgr.RemoveHandler(NPC, GameLivingEvent.Interact, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.AcceptQuest:
                    GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.DeclineQuest:
                    GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.EnemyDying:
                    GameEventMgr.RemoveHandler(NPC, GameLivingEvent.Dying, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.EnterArea:
                    //IArea area = (IArea)I;
                    //GameEventMgr.RemoveHandler(area, AreaEvent.PlayerEnter, new DOLEventHandler(QuestPart.Notify));
                    break;
                case eTriggerType.LeaveArea:
                    //area = (IArea)I;
                    //GameEventMgr.RemoveHandler(area, AreaEvent.PlayerLeave, new DOLEventHandler(QuestPart.Notify));
                    break;
                default:
                    break;

            }
        }
    }
}
