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
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS
{
	/// <summary>
	/// Keep guard is gamemob with just different brain and load from other DB table
	/// </summary>
	public class GameKeepGuard : GameMob
	{
		public GameKeepGuard(AbstractGameKeep keep) : this()
		{		
			KeepGuardBrain brain = new KeepGuardBrain();
			brain.Keep = keep;
			brain.AggroLevel = 100;//todo : find good aggro level and range
			brain.AggroRange = 50;
            SetOwnBrain(brain);
		}
		public GameKeepGuard() : base()
		{		
			Model = 486;
			Size = 50;
			Level = 55;
			Name = "Blank Guard";
			if (this.Brain == null)
			{
				KeepGuardBrain brain= new KeepGuardBrain();
				brain.AggroLevel = 10;
				brain.AggroRange = 50;
				SetOwnBrain(brain);
			}
		}

		/// <summary>
		/// Save the keep guard to Db table
		/// only called when create in command
		/// </summary>
		public override void SaveIntoDatabase()
		{
			DBKeepObject dbKeepGuard = null;
			if(InternalID != null)
				dbKeepGuard = (DBKeepObject) GameServer.Database.FindObjectByKey(typeof(DBKeepObject), InternalID);
			if(dbKeepGuard == null)
			{
				dbKeepGuard = new DBKeepObject();
				dbKeepGuard.ClassType = this.GetType().ToString();
			}
			dbKeepGuard.Name = Name;
			dbKeepGuard.BaseLevel = Level;
			Point pos = Position;
			dbKeepGuard.X = pos.X;
			dbKeepGuard.Y = pos.Y;
			dbKeepGuard.Z = pos.Z;
			dbKeepGuard.Heading = Heading;
			dbKeepGuard.EquipmentID = EquipmentTemplateID;
			
			AbstractGameKeep mykeep = (Brain as KeepGuardBrain ).Keep;
			if (mykeep != null)
				dbKeepGuard.KeepID = mykeep.KeepID;

			dbKeepGuard.Model=Model;
			dbKeepGuard.Realm=Realm;

			if(InternalID == null)
			{
				GameServer.Database.AddNewObject(dbKeepGuard);
				InternalID = dbKeepGuard.KeepObjectID.ToString();;
			}
			else
				GameServer.Database.SaveObject(dbKeepGuard);
		}

		/// <summary>
		/// load keep guard from DB object
		/// </summary>
		/// <param name="obj"> the db object</param>
		public override void LoadFromDatabase(object obj)
		{
			base.LoadFromDatabase(obj);
			DBKeepObject dbkeepobj = obj as DBKeepObject;
			if (dbkeepobj == null)return;

			KeepGuardBrain brain = this.Brain as KeepGuardBrain;
			if (brain == null) return;
			
			this.Name = dbkeepobj.Name;
			this.Realm = (byte)dbkeepobj.Realm;
			this.Model = (ushort)dbkeepobj.Model;
			this.Size= 50;
			this.Position = new Point(dbkeepobj.X, dbkeepobj.Y, dbkeepobj.Z);
			this.Heading = (ushort)dbkeepobj.Heading;
			this.LoadEquipmentTemplateFromDatabase(dbkeepobj.EquipmentID);
			AbstractGameKeep keep = brain.Keep ;
			if (keep != null)
			{	
				this.Region = keep.Region;
				this.Level = (byte)(dbkeepobj.BaseLevel + keep.Level);
				if (!this.AddToKeep(keep))
					return;
				if (keep.Guild != null)
				{
					GameEventMgr.AddHandler(this,GameLivingEvent.AttackedByEnemy,new DOLEventHandler(WarnGuild));
					GameEventMgr.AddHandler(this,GameLivingEvent.Dying,new DOLEventHandler(WarnGuild));
					//emblem ever set before so is in db.
				}
				//because if keep not set the current region is not set so can not been add.
				this.AddToWorld();
			}
		}

		/// <summary>
		/// Loads the equipment template of this guard
		/// all guard have his own equipment because when we modified emblem it must be change only for guard of keep and
		/// no other else npc...so removed cache and close() system for guard inventory
		/// </summary>
		/// <param name="equipmentTemplateID">The template id</param>
		public override void LoadEquipmentTemplateFromDatabase(string equipmentTemplateID)
		{
			EquipmentTemplateID = equipmentTemplateID;
			if (EquipmentTemplateID != null && EquipmentTemplateID.Length > 0)
			{
				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				if (template.LoadFromDatabase(EquipmentTemplateID))
				{
					m_inventory = template;
				}
				else
				{
//					if (log.IsDebugEnabled)
//						log.Debug("Error loading NPC inventory: InventoryID="+npc.EquipmentTemplateID+", NPC name="+npc.Name+".");
				}
				if (Inventory != null)
				{
					// if the two handed slot isnt empty we use that
					// or if the distance slot isnt empty we use that
					if (Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null)
						SwitchWeapon(eActiveWeaponSlot.TwoHanded);
					else if (Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
						SwitchWeapon(eActiveWeaponSlot.Distance);
					else SwitchWeapon(eActiveWeaponSlot.Standard); // sets visible left and right hand slots
				}
			}	
		}

		/// <summary>
		/// Change guild of guard (emblem on equipment) when keep is claimed
		/// </summary>
		/// <param name="guild"> the guild owner of the keep</param>
		public void ChangeGuild(Guild guild)
		{
			GuildName = guild.GuildName;
			VisibleEquipment lefthand = (VisibleEquipment)Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			if (lefthand != null)
				lefthand.Color = guild.Emblem;
			VisibleEquipment cloak = (VisibleEquipment)Inventory.GetItem(eInventorySlot.Cloak);
			if (cloak != null)
				cloak.Color = guild.Emblem;
			this.UpdateNPCEquipmentAppearance();
			GameEventMgr.AddHandler(this,GameLivingEvent.AttackedByEnemy,new DOLEventHandler(WarnGuild));
			GameEventMgr.AddHandler(this,GameLivingEvent.Dying,new DOLEventHandler(WarnGuild));
		}

		/// <summary>
		/// Delete all event on guard when delete object
		/// </summary>
		//TODO : use event delete
		public override void Delete()
		{
			GameEventMgr.RemoveHandler(this,GameLivingEvent.AttackedByEnemy,new DOLEventHandler(WarnGuild));
			GameEventMgr.RemoveHandler(this,GameLivingEvent.Dying,new DOLEventHandler(WarnGuild));
			base.Delete ();
		}

		/// <summary>
		/// Warn guild when keep is claimed if guard is attaked or if keep is taken
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void WarnGuild(DOLEvent e, object sender, EventArgs args)
		{
			if (e == GameLivingEvent.AttackedByEnemy)
			{
				((KeepGuardBrain)Brain).Keep.Guild.SendMessageToGuildMembers(this.Name + "is under attack in " + this.CurrentZone.Description + "!",eChatType.CT_Advise,eChatLoc.CL_SystemWindow);//
			}
			if (e == GameLivingEvent.Dying)
			{
				int m_inArea = 0;
				foreach (GamePlayer NearbyPlayers in this.GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
				{
					if (!GameServer.ServerRules.IsSameRealm(this, NearbyPlayers, true))
					{
						m_inArea = m_inArea + 1;
					}
				}
				string message = "[Guild] [ " + this.Name + " has been killed in " + this.CurrentZone.Description + " with " + m_inArea + " enemy player(s) in the area! ]";
				((KeepGuardBrain)Brain).Keep.Guild.SendMessageToGuildMembers(message,eChatType.CT_Guild,eChatLoc.CL_ChatWindow);
			}
		}

		public virtual bool AddToKeep( AbstractGameKeep keep)
		{
			keep.Guards.Add(this);
			return true;
		}
	}
}
