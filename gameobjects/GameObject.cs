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
using System.Text;
using DOL.Events;
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// This class holds all information that
	/// EVERY object in the game world needs!
	/// </summary>
	public abstract class GameObject : GeometryEngineNode
	{
		#region Name / Realm / Model

		/// <summary>
		/// The name of the Object
		/// </summary>
		protected string m_Name;

		/// <summary>
		/// Holds the realm of this object
		/// </summary>
		protected byte m_Realm;

		/// <summary>
		/// The model of the Object
		/// </summary>
		protected int m_Model;

		/// <summary>
		/// Gets or Sets the current Name of the Object
		/// </summary>
		public virtual string Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		/// <summary>
		/// Gets or Sets the current Realm of the Object
		/// </summary>
		public virtual byte Realm
		{
			get { return m_Realm; }
			set { m_Realm = value; }
		}

		/// <summary>
		/// Gets or Sets the current Model of the Object
		/// </summary>
		public virtual int Model
		{
			get { return m_Model; }
			set { m_Model = value; }
		}

		#endregion

		#region ExamineMessage / Interact

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public virtual IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList(4);
			list.Add("You target [" + GetName(0, false) + "]");
			return list;
		}

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public virtual bool Interact(GamePlayer player)
		{
			if (!Position.CheckSquareDistance(player.Position, (uint)(WorldMgr.INTERACT_DISTANCE*WorldMgr.INTERACT_DISTANCE)))
			{
				player.Out.SendMessage("You are too far away to interact with " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			Notify(GameObjectEvent.Interact, this, new InteractEventArgs(player));
			player.Notify(GameObjectEvent.InteractWith, player, new InteractWithEventArgs(this));

			return true;
		}

		private const string m_vowels = "aeuio";

		/// <summary>
		/// Returns name with article for nouns
		/// </summary>
		/// <param name="article">0=definite, 1=indefinite</param>
		/// <param name="firstLetterUppercase"></param>
		/// <returns>name of this object (includes article if needed)</returns>
		public virtual string GetName(int article, bool firstLetterUppercase)
		{
			/*
			 * http://www.camelotherald.com/more/888.shtml
			 * - All monsters names whose names begin with a vowel should now use the article 'an' instead of 'a'.
			 * 
			 * http://www.camelotherald.com/more/865.shtml
			 * - Instances where objects that began with a vowel but were prefixed by the article "a" (a orb of animation) have been corrected.
			 */


			// actually this should be only for Named mobs (like dragon, legion) but there is no way to find that out
			if (char.IsUpper(Name[0])) // proper noun
			{
				return Name;
			}
			else // common noun
				if (article == 0)
			{
				if (firstLetterUppercase)
					return "The " + Name;
				else
					return "the " + Name;
			}
			else
			{
				// if first letter is a vowel
				if (m_vowels.IndexOf(Name[0]) != -1)
				{
					if (firstLetterUppercase)
						return "An " + Name;
					else
						return "an " + Name;
				}
				else
				{
					if (firstLetterUppercase)
						return "A " + Name;
					else
						return "a " + Name;
				}
			}
		}

		/// <summary>
		/// Pronoun of this object in case you need to refer it in 3rd person
		/// http://webster.commnet.edu/grammar/cases.htm
		/// </summary>
		/// <param name="firstLetterUppercase"></param>
		/// <param name="form">0=Subjective, 1=Possessive, 2=Objective</param>
		/// <returns>pronoun of this object</returns>
		public virtual string GetPronoun(int form, bool firstLetterUppercase)
		{
			switch (form)
			{
				default: // Subjective
					if (firstLetterUppercase)
						return "It";
					else
						return "it";
				case 1: // Possessive
					if (firstLetterUppercase)
						return "Its";
					else
						return "its";
				case 2: // Objective
					if (firstLetterUppercase)
						return "It";
					else
						return "it";
			}
		}

		#endregion

		#region Receive Item/Money

		/// <summary>
		/// Called when the object is about to get an item from someone
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="item">Item to get</param>
		/// <returns>true if the item was successfully received</returns>
		public virtual bool ReceiveItem(GameLiving source, GenericItem item)
		{
			Notify(GameObjectEvent.ReceiveItem, this, new ReceiveItemEventArgs(source, this, item));
			return false;
		}

		/// <summary>
		/// Called when the object is about to get money from someone
		/// </summary>
		/// <param name="source">Source from where to get the money</param>
		/// <param name="money">array of money to get</param>
		/// <returns>true if the money was successfully received</returns>
		public virtual bool ReceiveMoney(GameLiving source, long money)
		{
			Notify(GameObjectEvent.ReceiveMoney, this, new ReceiveMoneyEventArgs(source, this, money));
			return false;
		}

		#endregion

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" name=").Append(Name)
				.Append(" realm=").Append(Realm)
				.Append(" model=").Append(Model)
				.ToString();
		}
	}
}