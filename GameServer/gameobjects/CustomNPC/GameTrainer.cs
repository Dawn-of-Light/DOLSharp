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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Database;

namespace DOL.GS
{
	/// <summary>
	/// The mother class for all class trainers
	/// </summary>
	public class GameTrainer : GameMob
	{

		#region GetExamineMessages/GetAggroLevelString
		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
//			IList list = base.GetExamineMessages(player, messageList);
			list.Add("You target ["+GetName(0, false)+"]");
			list.Add("You examine "+GetName(0, false)+".  "+GetPronoun(0, true)+" is "+GetAggroLevelString(player, false)+" and trains members of the "+TrainerClassName+" class.");
//			list.Add("[Right click to display a train window]");
			return list;
		}
		/// <summary>
		/// How friendly this NPC is to player
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <param name="firstLetterUppercase"></param>
		/// <returns>aggro state as string</returns>
		public override string GetAggroLevelString(GamePlayer player, bool firstLetterUppercase)
		{
			// TODO: findout if trainers can be aggro at all

			if(GameServer.ServerRules.IsSameRealm(this, player, true))
			{
				if(firstLetterUppercase) return "Friendly";
				else return "friendly";
			}
			IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
			if(aggroBrain != null && aggroBrain.AggroLevel > 0)
			{
				if(firstLetterUppercase) return "Aggressive";
				else return "aggressive";
			}

			if(firstLetterUppercase) return "Neutral";
			else return "neutral";

		}
		#endregion

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player) 
		{
			player.GainExperience(0, 0, 0, false); // leveup
 			if (!base.Interact(player)) return false;

			// Turn to face player
			TurnTo(player, 10000);

			return true;
		}

		/// <summary>
		/// Talk to trainer
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text)) return false;
			GamePlayer player = source as GamePlayer;
			if (player==null) return false;

			//Now we turn the npc into the direction of the person
			TurnTo(player, 10000);

			return true;
		}

		/// <summary>
		/// For Recieving Respec Stones. 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, GenericItem item)
		{
			if(source==null || item==null) return false;

			GamePlayer player = source as GamePlayer;
			if(player != null && item != null && item is RespecStone)
			{
				if(((RespecStone)item).RespecType == eRespecType.Full)
					player.RespecAmountAllSkill++;
				else
					player.RespecAmountSingleSkill++;
				
				player.Out.SendMessage("Thanks, I'll add an extra respec to your account.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Inventory.RemoveItem(item);
				return true;
			}

			return base.ReceiveItem(source, item);
		}

		/// <summary>
		/// Called to promote a player
		/// </summary>
		/// <param name="player">the player to promote</param>
		/// <param name="classid">the new classid</param>
		/// <param name="messageToPlayer">the message for the player</param>
		/// <param name="gifts">Array of inventory items as promotion gifts</param>
		/// <returns>true if successfull</returns>
		public bool PromotePlayer(GamePlayer player, int classid, string messageToPlayer, GenericItemTemplate[] gifts) {

			if(player == null) return false;

			IClassSpec oldClass = player.CharacterClass;

			// Player was promoted
			if (player.SetCharacterClass(classid)) {
				player.Out.SendMessage(this.Name +" says, \""+ messageToPlayer +"\"",eChatType.CT_System,eChatLoc.CL_PopupWindow);
				player.Out.SendMessage("You have been upgraded to the "+player.CharacterClass.Name+" class!",eChatType.CT_Important,eChatLoc.CL_SystemWindow);

				player.CharacterClass.OnLevelUp(player);
				player.UpdateSpellLineLevels(true);
				player.RefreshSpecDependendSkills(true);
				player.StartPowerRegeneration();
				//player.Out.SendUpdatePlayerSpells();
				player.Out.SendUpdatePlayerSkills();
				player.Out.SendUpdatePlayer();	
				
				// Initiate equipment
				if (gifts!=null && gifts.Length > 0) 
				{
					for (int i=0; i < gifts.Length; i++) 
					{
						if(gifts[i] != null) player.ReceiveItem(this, gifts[i].CreateInstance());
					}
				}

				// after gifts
				player.Out.SendMessage("You have been accepted by the "+player.CharacterClass.Profession+"!",eChatType.CT_Important,eChatLoc.CL_SystemWindow);

				Notify(GameTrainerEvent.PlayerPromoted, this, new PlayerPromotedEventArgs(player, oldClass));

				player.SaveIntoDatabase();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Holds trainer classname
		/// </summary>
		private string m_trainerClassName = "";

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public string TrainerClassName
		{
			get { return m_trainerClassName; }
		}
		
		/// <summary>
		/// Get class name from guild name
		/// </summary>
		/// <param name="obj"></param>
		public override void LoadFromDatabase(object obj)
		{
			base.LoadFromDatabase(obj);
			// "Fighter Trainer"
			int index = -1;
			if (GuildName.Length > 0)
				index = GuildName.IndexOf(" Trainer");

			if (index >= 0)
				m_trainerClassName = GuildName.Substring(0, index);
		}
	}
}
