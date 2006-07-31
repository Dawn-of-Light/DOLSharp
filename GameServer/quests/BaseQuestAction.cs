using System;
using System.Text;
using DOL.Events;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests
{    
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
        /// ANIM : emote P:eEmote is performed by GameLiving:Q(Player)
        /// </summary>
        /// <remarks>TO let player perform animation Q must be null.
        /// Tested</remarks>
        Animation,
        /// <summary>
        /// ATTA : Player is attacked with aggroamount P:int by monster Q:GameNPC(NPC)
        /// </summary>
        /// <remarks>Tested</remarks>
        Attack,
        /// <summary>
        /// GameNPC Q:GameNPC(NPC) walks to point P:GameLocation(player)
        /// </summary>        
        WalkTo,
        /// <summary>
        /// BROA : Message P:string is broadcasted to all players
        /// </summary>        
        Broadcast,
        /*
        CAST : monster's p## spell is casted, or entire q## monster index casts its        
        CINV : p## item is created and placed in player's backback  // q## (functional ?)         
        CLAS : class is set to p##
        DEGE : p## monster index degenerates
         * */
        /// <summary>
        /// DIAG : displays message P :string in a dialog box with an OK button
        /// </summary>
        Dialog,
        /// <summary>
        /// Displays a custom dialog with message P:string and CustomDialogresponse Q:CustomDialogResponse
        /// To Accept/Abort Quests use OfferQuest/OfferQuestAbort
        /// </summary>
        /// <remarks>Tested</remarks>
        CustomDialog,
        /// <summary>
        /// DINV : destroys Q:int(1) instances of item P:ItemTemplate in inventory        
        /// </summary>
        DestroyItem,
        //DORM : enters dormant mode (unused)        
        /// <summary>
        /// DROP : item P:ItemTemplate is dropped on the ground
        /// </summary>
        DropItem,
        /// <summary>
        ///  EMOT : displays message P:string localy without monster's name (local channel)
        /// </summary>
        /// <remarks>Tested</remarks>
        Emote,
        // FGEN : forces monster index p## to spawn one monster from slot q## (uncheck max)        
        /// <summary>
        /// FQST : quest P:Type is set as completed for player
        /// </summary>
        /// <remarks>Tested</remarks>
        FinishQuest,
        /// <summary>
        /// GCAP : gives cap experience p## times (q## should be set to 1 to avoid bugs)                         
        /// </summary>
        //GiveXPCap,
        /// <summary>
        /// GCPR : gives P:long coppers
        /// </summary>
        GiveGold,
        /// <summary>
        /// TCPR : takes P:long coppers
        /// </summary>
        TakeGold,
        /// <summary>
        /// GEXP : gives P:long experience points
        /// </summary>
        GiveXP,
        /// <summary>
        /// GIVE : NPC gives item P:ItemTemplate to player
        /// if NPC!= null NPC will give item to player, otherwise it appears mysteriously :)
        /// </summary>
        /// <remarks>Tested</remarks>
        GiveItem,
        /// <summary>
        /// NPC takes Q:int instances of item P:ItemTemplate from player
        /// default for q## is 1
        /// </summary>
        /// <remarks>Tested</remarks>
        TakeItem,
        /// <summary>
        /// QST : NPC assigns quest P:Type to player        
        /// </summary>
        /// <remarks>Tested</remarks>
        GiveQuest,
        /// <summary>
        /// Quest P:Type is offered to player via customdialog with message Q:string
        /// if player accepts GamePlayerEvent.AcceptQuest is fired, else GamePlayerEvent.RejectQuest
        /// </summary>
        /// <remarks>Tested</remarks>
        OfferQuest,
        /// <summary>
        /// Quest P:Type abort is offered to player via customdialog with message Q:string
        /// if player accepts GamePlayerEvent.AbortQuest is fired, else GamePlayerEvent.ContinueQuest
        /// </summary>
        /// <remarks>Tested</remarks>
        OfferQuestAbort,
        /*
            GSPE : monster index p## speed is set to q##
            GUIL : guild is set to p## (not player's guilds)
            IATK : monster index p## attacks player, or first monster from monster index q##
            INFC : reduces faction p## by q## points
            IPFC : increases faction p## by q## points
            IPTH : monster index p## is assigned to path q##
         * */
        /// <summary>
        /// IQST : Quest P:Type is set to step Q:int
        /// </summary>
        /// <remarks>Tested</remarks>
        SetQuestStep,
        /// <summary>
        /// KQST : Aborts quest P:Type
        /// </summary>  
        /// <remarks>Tested</remarks>
        AbortQuest,

        //    LIST : activates action list p## (action lists described in a following section)
        /// <summary>
        /// MGEN : Monster P:GameNPC will spawn considering all its requirements
        /// </summary>
        /// <remarks>Tested</remarks>
        MonsterSpawn,
        /// <summary>
        /// Monster P:GameNPC will unspawn considering all its requirements
        /// </summary>
        /// <remarks>Tested</remarks>
        MonsterUnspawn,
        /*
            NFAC : sets faction p## to a negative value specified by q##
            ORDE : changes trade order to p## one (???)
            PATH : path is set to p##
            PFAC : sets faction p## to a positive value specified by q##
         * */
        /// <summary>
        /// READ : open a description (bracket) windows reading message:P
        /// </summary>
        Read,
        /*
            RUMO : says a random quest teaser from zones p## and q##
            UOUT : set the fort p## to the realm q##
            SGEN : monster index p## will spawn slot q## up to its max        
         * */
        /// <summary>
        /// SINV : Item P:ItemTemplate is replaceb by item Q:ItemTemplate in inventory of player
        /// </summary>
        /// <remarks>Tested</remarks>
        ReplaceItem,
        /// <summary>
        /// SQST : Increments queststep of quest P:Type
        /// </summary>
        /// <remarks>Tested</remarks>
        IncQuestStep,
        /// <summary>
        /// TALK : says message P:string locally (local channel)
        /// </summary>
        /// <remarks>Tested</remarks>
        Talk,
        /// <summary>
        /// TELE : teleports player to destination P:GameLocation with a random distance of Q:int(0)
        /// Default for radius is 0
        /// </summary>
        Teleport,
        /// <summary>
        /// TIMR : regiontimer P:RegionTimer starts to count Q:int milliseconds        
        /// </summary>
        CustomTimer,
        /// <summary>
        /// TMR# :  timer P:string starts to count Q:int milliseconds
        /// </summary>
        /// <remarks>Tested</remarks>
        Timer,
        /*                
            TREA : drops treasure index p##
            * */
        /// <summary>
        /// WHIS : etalk p## appears in a window, words with brackets are keywords
        /// </summary>
        //Whisper        
        /*
            XFER : changes to talk index p##. MUST be placed in entry 19 !
            */
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
        /// Constant used to store timerid in RegionTimer.Properties
        /// </summary>
        const string TIMER_ID = "timerid";
        /// <summary>
        /// Constant used to store GameLiving Source in RegionTimer.Properties
        /// </summary>
        const string TIMER_SOURCE = "timersource";

        public BaseQuestAction(BaseQuestPart questPart,eActionType actionType, Object p, Object q): base(questPart, actionType, p, q)
        {            
            switch (ActionType)
            {
                case eActionType.SetQuestStep:
                    {
                        if (!(P is Type))
                            throw new ArgumentException("Variable P must be questType for quest based actions. ActionType:" + ActionType, "P");

                        if (!(Q is int))
                            throw new ArgumentException("Variable Q must be queststep for quest based actions. ActionType:" + ActionType, "Q");
                        break;
                    }
                case eActionType.IncQuestStep:
                case eActionType.FinishQuest:
                case eActionType.AbortQuest:
                case eActionType.GiveQuest:
                    {
                        if (!(P is Type))
                            throw new ArgumentException("Variable P must be questType for quest based actions. ActionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.OfferQuest:
                case eActionType.OfferQuestAbort:
                    {
                        if (!(P is Type))
                            throw new ArgumentException("Variable P must be questType for for quest based actions. ActionType:" + ActionType, "P");

                        if (!(Q is string))
                            throw new ArgumentException("Variable Q must be message(string) for quest based actions. ActionType:" + ActionType, "Q");
                        break;
                    }
                case eActionType.GiveItem:
                case eActionType.TakeItem:
                case eActionType.DropItem:
                case eActionType.DestroyItem:
                    {
                        if (!(P is ItemTemplate))
                            throw new ArgumentException("Variable P must be itemtemplate for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.ReplaceItem:
                    {
                        if (!(P is ItemTemplate))
                            throw new ArgumentException("Variable P must be itemtemplate for actionType:" + ActionType, "P");
                        if (!(Q is ItemTemplate))
                            throw new ArgumentException("Variable Q must be itemtemplate for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.GiveXP:
                    {
                        if (!(Q is Int64))
                            throw new ArgumentException("Variable Q must be xp(long) for actionType:" + ActionType, "Q");
                        break;
                    }

                case eActionType.GiveGold:
                case eActionType.TakeGold:
                    {
                        if (!(P is Int64))
                            throw new ArgumentException("Variable P must be copper(long) for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.Talk:
                    {
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
                case eActionType.Dialog:
                case eActionType.Emote:
                    {
                        if (!(P is string))
                            throw new ArgumentException("Variable P must be string for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.Animation:
                    {
                        if (!(P is eEmote))
                            throw new ArgumentException("Variable P must be eEmote for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.Attack:
                    {
                        if (!(P is int))
                            throw new ArgumentException("Variable P must be int for actionType:" + ActionType, "P");
                        break;
                    }                
                case eActionType.Broadcast:
                case eActionType.Read:
                    {
                        if (!(P is string))
                            throw new ArgumentException("Variable P must be string for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.Teleport:
                    {
                        if (!(P is GameLocation))
                            throw new ArgumentException("Variable P must be GameLocation for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.CustomTimer:
                    {
                        if (!(P is RegionTimer))
                            throw new ArgumentException("Variable P must be RegionTimer for actionType:" + ActionType, "P");

                        if (!(Q is int))
                            throw new ArgumentException("Variable Q must be delay(int) for actionType:" + ActionType, "Q");

                        break;
                    }
                case eActionType.Timer:
                    {
                        if (!(Q is int))
                            throw new ArgumentException("Variable Q must be delay(int) for actionType:" + ActionType, "Q");
                        if (!(P is string))
                            throw new ArgumentException("Variable P must be timername(string) for actionType:" + ActionType, "P");
                        break;
                    }
                case eActionType.MonsterSpawn:
                    if (!(P is GameNPC))
                        throw new ArgumentException("Variable P must be GameNPC for actionType:" + ActionType, "P");
                    break;
                case eActionType.MonsterUnspawn:
                    if (!(P is GameNPC))
                        throw new ArgumentException("Variable P must be GameNPC for actionType:" + ActionType, "P");
                    break;
            }            
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
                        Type requirementQuestType = (Type)P;
                        AbstractQuest playerQuest = player.IsDoingQuest(requirementQuestType) as AbstractQuest;
                        playerQuest.Step++;
                        break;
                    }
                case eActionType.SetQuestStep:
                    {
                        Type requirementQuestType = (Type)P;
                        AbstractQuest playerQuest = player.IsDoingQuest(requirementQuestType) as AbstractQuest;
                        playerQuest.Step = Convert.ToInt16(Q);
                        break;
                    }
                case eActionType.FinishQuest:
                    {
                        Type requirementQuestType = (Type)P;
                        AbstractQuest playerQuest = player.IsDoingQuest(requirementQuestType) as AbstractQuest;
                        if (playerQuest != null)
                        {
                            playerQuest.FinishQuest();
                        }
                        break;
                    }
                case eActionType.GiveQuest:
                    {
                        Type requirementQuestType = (Type)P;
                        if (NPC.CanGiveQuest(requirementQuestType, player) > 0)
                            NPC.GiveQuest(requirementQuestType, player, 1);
                        break;
                    }
                case eActionType.OfferQuest:
                    {
                        string message = BaseQuestPart.GetPersonalizedMessage(Convert.ToString(Q),player);
                        Type questType = (Type)P;
                        player.Out.SendQuestSubscribeCommand(NPC, QuestMgr.GetIDForQuestType(questType), message);
                        break;
                    }
                case eActionType.OfferQuestAbort:
                    {
                        string message = BaseQuestPart.GetPersonalizedMessage(Convert.ToString(Q),player);
                        Type questType = (Type)P;
                        player.Out.SendQuestAbortCommand(NPC, QuestMgr.GetIDForQuestType(questType), message);
                        break;
                    }
                case eActionType.AbortQuest:
                    {
                        Type questType = (Type)P;
                        AbstractQuest quest = player.IsDoingQuest(questType);
                        if (quest != null)
                        {
                            quest.AbortQuest();
                        }
                        break;
                    }
                case eActionType.GiveItem:
                    {
                        ItemTemplate itemTemplate = (ItemTemplate)P;

                        InventoryItem item = new InventoryItem(itemTemplate);
                        if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
                        {
                            if (NPC == null)
                            {
                                player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + " from " + NPC.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        else
                        {
                            player.CreateItemOnTheGround(item);
                            player.Out.SendMessage("Your Inventory is full. You couldn't recieve " + itemTemplate.GetName(0, false) + ", so it's been placed on the ground. Pick it up as soon as possible or it will vanish in a few minutes.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
                        }
                        break;
                    }

                case eActionType.ReplaceItem:
                    {
                        ItemTemplate newItemTemplate = (ItemTemplate)Q;
                        ItemTemplate oldItemTemplate = (ItemTemplate)P;

                        InventoryItem item = new InventoryItem(newItemTemplate);
                        lock (player.Inventory)
                        {
                            if (player.Inventory.RemoveTemplate(oldItemTemplate.Id_nb, 1, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv))
                            {
                                if (!player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
                                {
                                    //if inventory was full
                                    player.CreateItemOnTheGround(item);
                                    player.Out.SendMessage(item.GetName(1, true) + " drops in front of you.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
                                }
                            }
                        }
                        break;
                    }
                case eActionType.TakeItem:
                    {
                        ItemTemplate itemTemplate = (ItemTemplate)P;
                        int amount = Q != null ? Convert.ToInt32(Q) : 1;

                        lock (player.Inventory)
                        {
                            InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                            //remove item/s from player
                            while (amount > 0 && item != null)
                            {
                                if (item.Count > amount)
                                {
                                    player.Inventory.RemoveCountFromStack(item, amount);
                                    amount = 0;
                                }
                                else
                                {
                                    amount -= item.Count;
                                    player.Inventory.RemoveItem(item);
                                    item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                                }
                            }

                            if (NPC != null)
                            {
                                player.Out.SendMessage("You give " + itemTemplate.GetName(0, false) + " to " + NPC.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                            else
                            {
                                player.Out.SendMessage("You give " + itemTemplate.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }
                        break;
                    }
                case eActionType.DropItem:
                    {
                        ItemTemplate itemTemplate = (ItemTemplate)P;

                        InventoryItem item = new InventoryItem(itemTemplate);
                        player.CreateItemOnTheGround(item);
                        player.Out.SendMessage(itemTemplate.GetName(1, true) + " drops in front of you.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);

                        break;
                    }
                case eActionType.DestroyItem:
                    {
                        ItemTemplate itemTemplate = (ItemTemplate)P;
                        int amount = Q != null ? Convert.ToInt32(Q) : 1;

                        lock (player.Inventory)
                        {
                            InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                            while (amount > 0 && item != null)
                            {
                                if (item.Count > amount)
                                {
                                    player.Inventory.RemoveCountFromStack(item, amount);
                                    amount = 0;
                                }
                                else
                                {
                                    amount -= item.Count;
                                    player.Inventory.RemoveItem(item);
                                    item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                                }
                            }
                        }
                        player.Out.SendMessage(itemTemplate.GetName(0, true) + " is destroyed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
                        string message =BaseQuestPart.GetPersonalizedMessage(Convert.ToString(P),player);
                        NPC.TurnTo(player);
                        NPC.SayTo(player, message);
                        break;
                    }

                case eActionType.CustomDialog:
                    {
                        string message = BaseQuestPart.GetPersonalizedMessage(Convert.ToString(P),player);
                        CustomDialogResponse response = Q as CustomDialogResponse;
                        player.Out.SendCustomDialog(message, response);
                        break;
                    }
                case eActionType.Dialog:
                    {
                        string message = BaseQuestPart.GetPersonalizedMessage(Convert.ToString(P),player);
                        player.Out.SendCustomDialog(message, null);
                        break;
                    }
                case eActionType.Emote:
                    {
                        string message = BaseQuestPart.GetPersonalizedMessage(Convert.ToString(P),player);
                        player.Out.SendMessage(message, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                        break;
                    }
                case eActionType.Animation:
                    {
                        eEmote emote = (eEmote)P;
                        GameLiving actor = Q is GameLiving ? (GameLiving)Q : player;
                        foreach (GamePlayer nearPlayer in actor.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        {
                            nearPlayer.Out.SendEmoteAnimation(actor, emote);
                        }
                        break;
                    }
                case eActionType.Attack:
                    {
                        GameNPC attacker = (Q is GameNPC) ? (GameNPC)Q : NPC;
                        if (attacker.Brain is IAggressiveBrain)
                        {
                            int aggroAmount = P != null ? Convert.ToInt32(P) : 10;
                            IAggressiveBrain brain = (IAggressiveBrain)attacker.Brain;
                            brain.AddToAggroList(player, aggroAmount);
                        }
                        else
                        {
                            log.Warn("Non agressive mob " + attacker.Name + " was order to attack player in Quest " + QuestPart.QuestType + ". This goes against the first directive and will not happen");
                        }
                        break;
                    }
                case eActionType.WalkTo:
                    {
                        GameNPC npc = (Q is GameNPC) ? (GameNPC)Q : NPC;
                        IPoint3D location = (P is IPoint3D) ? (IPoint3D)P : new Point3D(player.X, player.Y, player.Z);
                        npc.WalkTo(location, npc.CurrentSpeed);
                        break;
                    }
                case eActionType.Broadcast:
                    {
                        string message =BaseQuestPart.GetPersonalizedMessage(Convert.ToString(P),player);
                        foreach (GameClient clientz in WorldMgr.GetAllPlayingClients())
                        {
                            clientz.Player.Out.SendMessage(message, eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    }
                case eActionType.Read:
                    {
                        string message = BaseQuestPart.GetPersonalizedMessage(Convert.ToString(P),player);
                        player.Out.SendMessage("[ " + message + " ]", eChatType.CT_Emote, eChatLoc.CL_PopupWindow);
                        break;
                    }
                case eActionType.Teleport:
                    {
                        GameLocation location = (GameLocation)P;
                        int radius = Q != null ? Convert.ToInt32(Q) : 0;

                        if (location.Name != null)
                        {
                            player.Out.SendMessage(player + " is being teleported to " + location.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        player.MoveTo(location.RegionID, location.X + Util.Random(0 - radius, radius), location.Y + Util.Random(0 - radius, radius), location.Z, location.Heading);
                        break;
                    }
                case eActionType.CustomTimer:
                    {
                        RegionTimer timer = (RegionTimer)P;
                        int delay = Convert.ToInt32(Q);
                        timer.Start(delay);
                        break;
                    }
                case eActionType.Timer:
                    {
                        int delay = Convert.ToInt32(Q);
                        string timername = Convert.ToString(P);
                        RegionTimer timer = new RegionTimer(player, new RegionTimerCallback(QuestTimerCallBack));
                        timer.Properties.setProperty(TIMER_ID, timername);
                        timer.Properties.setProperty(TIMER_SOURCE, player);
                        timer.Start(delay);
                        break;
                    }
                case eActionType.MonsterSpawn:
                    {
                        GameNPC npc = (GameNPC)P;
                        if (npc.ObjectState == GameObject.eObjectState.Inactive)
                        {
                            npc.AddToWorld();

                            // appear with a big buff of magic
                            foreach (GamePlayer visPlayer in npc.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            {
                                visPlayer.Out.SendSpellCastAnimation(npc, 1, 20);
                            }
                        }
                        break;
                    }

                case eActionType.MonsterUnspawn:
                    {
                        GameNPC npc = (GameNPC)P;
                        npc.RemoveFromWorld();
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

        
    }
}
