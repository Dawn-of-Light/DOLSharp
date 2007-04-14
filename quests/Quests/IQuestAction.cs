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
using System.Text;
using DOL.Events;

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

    /// <summary>
    /// If one trigger and all requirements are fulfilled the corresponding actions of
    /// a QuestAction will we executed one after another. Actions can be more or less anything:
    /// at the moment there are: GiveItem, TakeItem, Talk, Give Quest, Increase Quest Step, FinishQuest,
    /// etc....
    /// </summary>
    public interface IQuestAction
    {
        /// <summary>
        /// Action performed 
        /// Can be used in subclasses to define special behaviour of actions
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>        
        void Perform(DOLEvent e, object sender, EventArgs args, GamePlayer player);
    }
}
