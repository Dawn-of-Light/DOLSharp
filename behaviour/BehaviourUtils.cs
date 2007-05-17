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
using System.Collections.Generic;
using System.Text;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using DOL.Database;
using DOL.GS.Movement;
using DOL.Events;
using DOL.GS.Quests;

namespace DOL.GS.Behaviour
{
    public enum eDefaultValueConstants
    {        
        NPC
    }

    class BehaviourUtils
    {

        /// <summary>
        /// Player Constant will be replaced by players name in output messages.
        /// </summary>
        const string PLAYER = "<Player>";
        /// <summary>
        /// @deprecated
        /// </summary>
        const string PLAYER_OLD = "{Player}";
        /// <summary>
        /// Constant will be replaced by the players class in the outputmessages.
        /// </summary>
        const string CLASS = "<Class>";
        /// <summary>
        /// Constant will be replaced by the players race in the outputmessages.
        /// </summary>
        const string RACE = "<Race>";

        public static object ConvertObject(object obj, Type destinationType)
        {
            return ConvertObject(obj, null, destinationType);
        }        

        public static object ConvertObject(object obj,object defaultValue, Type destinationType)
        {
            object result = null;
            if (destinationType == typeof(Type))
            {
                if (obj is Type)
                {
                    result = obj;
                }
                else if (!String.IsNullOrEmpty(Convert.ToString(obj)))
                {
                    result = ScriptMgr.GetType(Convert.ToString(obj));
                }
            }
            else if (destinationType == typeof(Int32))
            {
                result = Convert.ToInt32(obj);
            }
            else if (destinationType == typeof(Nullable<Int32>))
            {
                result = new Nullable<Int32>(Convert.ToInt32(obj));
            }
            else if (destinationType == typeof(Int64))
            {
                result = Convert.ToInt64(obj);
            }
            else if (destinationType == typeof(eEmote))
            {
                if (obj is eEmote)
                {
                    result = (eEmote)obj;
                }
                else
                {
                    result = (eEmote)Enum.Parse(typeof(eEmote), Convert.ToString(obj), true);
                }
            }
            else if (destinationType == typeof(GameLiving))
            {
                result = QuestMgr.ResolveLiving(obj);
            }
            else if (destinationType == typeof(ItemTemplate))
            {
                if (obj is ItemTemplate)
                    result = obj;
                else
                {
                    result = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), Convert.ToString(obj));
                }
            }
            else if (destinationType == typeof(CustomDialogResponse))
            {
                result = (CustomDialogResponse)obj;
            }
            else if (destinationType == typeof(GameLocation))
            {
                result = (GameLocation)obj;
            }
            else if (destinationType == typeof(IPoint3D))
            {
                result = (IPoint3D)obj;
            }
            else if (destinationType == typeof(PathPoint))
            {
                result = (PathPoint)obj;
            }                  
            else
            {
                result = obj;
            }

            // handle default value if result == null
            if (result == null && defaultValue != null)
            {
                result = defaultValue;
            }

            return result;
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

            int playerIndex = message.IndexOf(PLAYER);
            if (playerIndex == 0)
            {
                message = message.Replace(PLAYER, player.GetName(0, true));
            }
            else if (playerIndex > 0)
            {
                message = message.Replace(PLAYER, player.GetName(0, false));
            }

            // {PLAYER} is deprecated use <PLAYER> instead
            playerIndex = message.IndexOf(PLAYER_OLD);
            if (playerIndex == 0)
            {
                message = message.Replace(PLAYER_OLD, player.GetName(0, true));
            }
            else if (playerIndex > 0)
            {
                message = message.Replace(PLAYER_OLD, player.GetName(0, false));
            }

            message = message.Replace(RACE, player.RaceName);
            message = message.Replace(CLASS, player.CharacterClass.Name);

            return message;
        }

        public static GamePlayer GuessGamePlayerFromNotify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = null;
            if (sender is GamePlayer)
                player = sender as GamePlayer;
            else if (e == GameLivingEvent.WhisperReceive || e == GameObjectEvent.Interact)
            {
                player = ((SourceEventArgs)args).Source as GamePlayer;
            }
            else if (e == GameLivingEvent.ReceiveItem || e == GameObjectEvent.ReceiveMoney)
            {
                player = ((SourceEventArgs)args).Source as GamePlayer;
            }
            else if (e == AreaEvent.PlayerEnter || e == AreaEvent.PlayerLeave)
            {
                AreaEventArgs aArgs = (AreaEventArgs)args;
                player = aArgs.GameObject as GamePlayer;
            }

            return player;
        }
    }
}
