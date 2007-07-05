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
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS.Keeps
{
	public class HookPointInventory
	{
		public HookPointInventory()
		{
			hookpointItemList = new ArrayList(MAX_ITEM);
		}
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private ArrayList hookpointItemList;
		private const int MAX_ITEM = 10;
		public static HookPointInventory BlueHPInventory = new HookPointInventory();
		public static HookPointInventory RedHPInventory = new HookPointInventory();
		public static HookPointInventory GreenHPInventory = new HookPointInventory();
		public static HookPointInventory LightGreenHPInventory = new HookPointInventory();
		public static HookPointInventory YellowHPInventory = new HookPointInventory();

		/// <summary>
		/// Get the item the slot
		/// </summary>
		/// <param name="slot">The item slot</param>
		/// <returns>Item template or null</returns>
		public virtual HookPointItem GetItem(int slot)
		{
			if (slot >= hookpointItemList.Count)
				return null;

			if (slot > MAX_ITEM)
			{
				if (log.IsErrorEnabled)
					log.Error("item add to slot already used");
				return null;
			}
			return hookpointItemList[slot] as HookPointItem;
		}

		public virtual HookPointItem GetItem(string classType)
		{
			foreach (HookPointItem item in hookpointItemList)
			{
				if (item.GameObjectType == classType)
					return item;
			}

			return null;
		}

		/// <summary>
		/// add the item to the slot
		/// </summary>
		/// <param name="item"></param>
		/// <param name="slot">The item slot</param>
		/// <returns>Item template or null</returns>
		public virtual void AddItem(HookPointItem item, int slot)
		{
			if (hookpointItemList[slot] != null)
			{
				if (log.IsErrorEnabled)
					log.Error("item add to slot already used");
				return;
			}
			if ((slot > MAX_ITEM) || (slot < 0))
			{
				if (log.IsErrorEnabled)
					log.Error("slot out of the inventory");
				return;
			}
			hookpointItemList[slot] = item;
		}

		/// <summary>
		/// add the item to the first free slot
		/// </summary>
		/// <returns>Item template or null</returns>
		public virtual void AddFirstFreeSlot(HookPointItem item)
		{
			if (hookpointItemList.Count < 10)
				hookpointItemList.Add(item);

			else if (log.IsErrorEnabled)
				log.Error("inventory is full");
		}

		/// <summary>
		/// Gets a copy of all intems
		/// </summary>
		/// <returns>A list where key is the slot position and value is the ItemTemplate</returns>
		public virtual ArrayList GetAllItems()
		{
			return hookpointItemList;
		}

	}
	public class HookPointItem
	{
		public HookPointItem()
		{
		}

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public HookPointItem(string name, byte gold, ushort icon, string objectType, ushort flag)
		{
			m_name = name;
			m_gold = gold;
			m_icon = icon;
			m_objectType = objectType;
			m_flag = flag;
		}

		protected short m_gold;
		protected byte m_silver;
		protected byte m_copper;
		private ushort m_icon;
		private string m_objectType;
		private string m_name;
		private ushort m_flag;

		public short Gold
		{
			get { return m_gold; }
			set { m_gold = value; }
		}

		public ushort Icon
		{
			get { return m_icon; }
			set { m_icon = value; }
		}

		public string GameObjectType
		{
			get { return m_objectType; }
			set { m_objectType = value; }
		}

		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		public ushort Flag
		{
			get { return m_flag; }
			set { m_flag = value; }
		}

		public void Invoke(GamePlayer player, int payType, GameKeepHookPoint hookpoint, GameKeepComponent component)
		{
			if (!hookpoint.IsFree)
			{
				player.Out.SendMessage("The hookpoint is already used!", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return;
			}
			//1=or 2=BP 3=GuildBP 4=contract
			//todo enum
			switch (payType)
			{
				case 1:
					{
						if (!player.RemoveMoney(Gold * 100 * 100, "You buy " + this.GetName(1, false) + "."))
						{
							player.Out.SendMessage("You dont have enough money!", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							return;
						}
					} break;
				case 2:
					{
						if (!player.RemoveBountyPoints(Gold, "You buy " + this.GetName(1, false) + "."))
						{
							player.Out.SendMessage("You dont have enough bounty point!", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							return;
						}
					} break;
				case 3:
					{
						if (player.Guild == null) return;
						if (!player.Guild.RemoveBountyPoints(Gold))
						{
							player.Out.SendMessage("You dont have enough bounty point!", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
							return;
						}
						else
							player.Out.SendMessage("You buy " + this.GetName(1, false) + ".", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);

					} break;
				case 4:
					{
						player.Out.SendMessage("NOT IMPLEMENTED YET, SORRY", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
						return;
					}

			}

			GameLiving hookPointObj = CreateHPInstance(this.GameObjectType);
			if (hookPointObj == null) return;
			//use default value so no need to load
			//hookPointObj.LoadFromDatabase(this.ObjectTemplate);
			hookPointObj.CurrentRegion = player.CurrentRegion;
			hookPointObj.Realm = (byte)hookpoint.Component.Keep.Realm;

			if (hookPointObj is GameSiegeWeapon)
				((GameSiegeWeapon)hookPointObj).EnableToMove = false;
			hookPointObj.X = hookpoint.X;
			hookPointObj.Y = hookpoint.Y;
			hookPointObj.Z = hookpoint.Z;
			hookPointObj.Heading = hookpoint.Heading;

			if (hookPointObj is GameSiegeWeapon)
				(hookPointObj as GameSiegeWeapon).HookPoint = hookpoint;
			if (hookPointObj is IKeepItem)
				(hookPointObj as IKeepItem).Component = component;
			if (hookPointObj is GameSiegeCauldron)
				(hookPointObj as GameSiegeCauldron).Component = component;
			if (hookPointObj is GameKeepGuard)
			{
				(hookPointObj as GameKeepGuard).HookPoint = hookpoint;
				Keeps.TemplateMgr.RefreshTemplate(hookPointObj as GameKeepGuard);
			}
			if (hookPointObj is GameNPC)
				((GameNPC)hookPointObj).RespawnInterval = -1;//do not respawn
			hookPointObj.AddToWorld();
			if (hookPointObj is GameKeepGuard)
				(hookPointObj as GameKeepGuard).Component.Keep.Guards.Add(hookPointObj.ObjectID, hookPointObj);
			hookpoint.Object = hookPointObj;

			//create the db entry
			Database.DBKeepHookPointItem item = new DOL.Database.DBKeepHookPointItem(component.Keep.KeepID, component.ID, hookpoint.ID, GameObjectType);
			GameServer.Database.AddNewObject(item);
		}

		public static void Invoke(GameKeepHookPoint hookpoint, string objectType)
		{
			if (!hookpoint.IsFree)
				return;

			GameLiving hookPointObj = CreateHPInstance(objectType);
			if (hookPointObj == null) return;
			//use default value so no need to load
			//hookPointObj.LoadFromDatabase(this.ObjectTemplate);
			hookPointObj.CurrentRegion = hookpoint.Component.CurrentRegion;
			hookPointObj.Realm = (byte)hookpoint.Component.Keep.Realm;

			if (hookPointObj is GameSiegeWeapon)
				((GameSiegeWeapon)hookPointObj).EnableToMove = false;
			hookPointObj.X = hookpoint.X;
			hookPointObj.Y = hookpoint.Y;
			hookPointObj.Z = hookpoint.Z;
			hookPointObj.Heading = hookpoint.Heading;

			if (hookPointObj is GameSiegeWeapon)
				(hookPointObj as GameSiegeWeapon).HookPoint = hookpoint;
			if (hookPointObj is IKeepItem)
				(hookPointObj as IKeepItem).Component = hookpoint.Component;
			if (hookPointObj is GameSiegeCauldron)
				(hookPointObj as GameSiegeCauldron).Component = hookpoint.Component;
			if (hookPointObj is GameKeepGuard)
			{
				(hookPointObj as GameKeepGuard).HookPoint = hookpoint;
				Keeps.TemplateMgr.RefreshTemplate(hookPointObj as GameKeepGuard);
			}
			if (hookPointObj is GameNPC)
				((GameNPC)hookPointObj).RespawnInterval = -1;//do not respawn
			hookPointObj.AddToWorld();
			if (hookPointObj is GameKeepGuard)
				(hookPointObj as GameKeepGuard).Component.Keep.Guards.Add(hookPointObj.ObjectID, hookPointObj);
			hookpoint.Object = hookPointObj;
		}

		private static GameLiving CreateHPInstance(string objTypeName)
		{
			if (objTypeName != "")
			{
				GameLiving hookPointObj = null;
				Type objType = null;
				foreach (Assembly asm in ScriptMgr.Scripts)
				{
					objType = asm.GetType(objTypeName);
					if (objType != null)
						break;
				}
				if (objType == null)
					objType = Assembly.GetAssembly(typeof(GameServer)).GetType(objTypeName);
				if (objType == null)
				{
					if (log.IsErrorEnabled)
						log.Error("Could not find keepobject: " + objTypeName + "!!!");
					return null;
				}
				try
				{
					hookPointObj = (Activator.CreateInstance(objType)) as GameLiving;
				}
				catch (Exception e)
				{
					if (log.IsWarnEnabled)
						log.Warn("Hookpoint object can not been instanciate :" + e.ToString());
				}

				if (hookPointObj == null)
				{
					if (log.IsWarnEnabled)
						log.Warn("Hookpoint object have a wrong class type which must inherite from GameLiving.");
				}
				return hookPointObj;
			}
			else
			{
				if (log.IsWarnEnabled)
					log.Warn("Hookpoint object have a wrong class type which must inherite from GameLiving.");
				return null;
			}

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
	}
}
