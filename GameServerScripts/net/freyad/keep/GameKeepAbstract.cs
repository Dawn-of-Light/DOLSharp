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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using DOL.Database;
using DOL.GS;
using DOL.GS.Keeps;

using log4net;

namespace net.freyad.keep
{
	/// <summary>
	/// GameKeepAbstract is a Simple Implementation of IGameKeepData extending basic IGameKeep interface.
	/// </summary>
	public abstract class GameKeepAbstract : IGameKeepData
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private List<IGameKeepComponent> m_components = new List<IGameKeepComponent>();
		
		/// <summary>
		/// List of IGameKeepComponents for Network Sending Purpose.
		/// </summary>
		public List<IGameKeepComponent> SentKeepComponents
		{ 
			get 
			{
				return m_components;
			}
		}
		
		private List<IGameKeepObject> m_keepObjects = new List<IGameKeepObject>();
		
		/// <summary>
		/// List of Objects Attached to Keep through Position Data
		/// </summary>
		public List<IGameKeepObject> KeepObjects {
			get { return m_keepObjects; }
		}
		
		
		
		public Hashtable Guards { get { return null;}}
		public Hashtable Banners { get {return null;}}
		public void SaveIntoDatabase() {}
		
		private string m_name;
		
		public string Name {
			get { return m_name; }
			set { m_name = value; }
		}
		
		private ushort m_keepID;
		
		/// <summary>
		/// Keep ID
		/// </summary>
		public virtual ushort KeepID {
			get { return m_keepID; }
			set { m_keepID = value; }
		}
		
		private int m_x;
		
		/// <summary>
		/// Keep X
		/// </summary>
		public virtual int X {
			get { return m_x; }
			set { m_x = value; }
		}
		
		private int m_y;
		
		/// <summary>
		/// Keep Y
		/// </summary>
		public virtual int Y {
			get { return m_y; }
			set { m_y = value; }
		}
		
		private int m_z;
		
		/// <summary>
		/// Keep Z
		/// </summary>
		public virtual int Z {
			get { return m_z; }
			set { m_z = value; }
		}
		
		private ushort m_heading;
		
		/// <summary>
		/// Keep Heading
		/// </summary>
		public virtual ushort Heading {
			get { return m_heading; }
			set { m_heading = value; }
		}
		
		private eRealm m_realm;
		
		/// <summary>
		/// Keep Current Realm.
		/// </summary>
		public virtual eRealm Realm {
			get { return m_realm; }
			set { m_realm = value; }
		}
		
		private byte m_level;
		
		/// <summary>
		/// Keep Current Level
		/// </summary>
		public virtual byte Level {
			get { return m_level; }
			set { m_level = value; }
		}
		
		private ushort m_regionID;
		
		/// <summary>
		/// Keep RegionID
		/// </summary>
		public virtual ushort RegionID {
			get { return m_regionID; }
			set { m_regionID = value; }
		}
		
		/// <summary>
		/// Keep CurrentRegion
		/// </summary>
		public virtual Region CurrentRegion {
			get { return WorldMgr.GetRegion(RegionID); }
			set { RegionID = value.ID; }
		}
		
		/// <summary>
		/// Keep Current Claimed Guild
		/// </summary>
		public virtual Guild Guild {
			get { return null; }
			set { return; }
		}
		
		/// <summary>
		/// Keep Effective Level.
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public byte EffectiveLevel(byte level)
		{
			return Level;
		}
		
		/// <summary>
		/// Load From KeepData DataObject.
		/// </summary>
		/// <param name="keep"></param>
		public virtual void LoadFromDatabase(DataObject obj)
		{
			KeepData keep = (KeepData)obj;
			
			// Load Keep Data...
			m_name = keep.Name;
			m_keepID = keep.KeepID;
			m_x = keep.X;
			m_y = keep.Y;
			m_z = keep.Z;
			m_heading = keep.Heading;
			m_level = keep.CurrentLevel;
			m_regionID = (ushort)keep.Region;
			
			foreach(KeepDataComponent component in keep.Component)
			{
				m_components.Add(new GameKeepDataComponent(component, this));
			}
			
			// Load Positions...
			if (keep.Template != null)
			{
				foreach (KeepDataXKeepDataPosition template in keep.Template)
				{
					if (template != null && template.Position != null)
					{
						foreach (KeepDataPosition pos in template.Position)
						{
							// For each Position in all Templates
							// Try to create Object Class
							// Then Load from Database
							// Finally Add to World and Keep Object Collection.
							try
							{
								IGameKeepObject keepObj = null;
								// Search our assemblies for Position Classtype
								// Must Implement IGameKeepObject
								foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
								{
									try
									{
										// This include all kind of possible objects
										// Door, Hookpoint, SiegeEngines, Guard, NPC, Patrols
										// Can be extensively used to add anything related to a Keep.
										// The Interface is pretty much compatible with any GameObject/GameLiving.
										keepObj = (IGameKeepObject)assembly.CreateInstance(pos.ClassType);
										keepObj.Keep = this;
										break;
									}
									catch// (Exception cex)
									{
										//if (log.IsDebugEnabled)
										//	log.DebugFormat("Could not instanciate IGameKeepObject {0}({1}) From Class '{2}' in assembly {3}, ex : {4}", pos.KeepData_PositionID, pos.PositionTemplateName, pos.ClassType, assembly.ToString(), cex);
										continue;
									}
								}
								
								if (keepObj == null)
								{
									if (log.IsWarnEnabled)
										log.WarnFormat("Could not instanciate Keep Object ({0}) Id : {1}, Name {2}", pos.ClassType, pos.KeepData_PositionID, pos.PositionTemplateName);
									
									continue;
								}
								
								keepObj.LoadFromDatabase(pos);
								
								if (!keepObj.AddToWorld())
								{
									if (log.IsWarnEnabled)
										log.WarnFormat("Could not add Keep Object To World ({0}) id : {1}, Name {2}, ObjectID {3}", pos.ClassType, pos.KeepData_PositionID, pos.PositionTemplateName, keepObj);
									continue;
								}
								
								m_keepObjects.Add(keepObj);

							}
							catch (Exception ex)
							{
								if (log.IsErrorEnabled)
									log.ErrorFormat("Error While Loading Components From Keep Data {0} ({1}) - Template {2}, ex : {3}", keep.KeepDataID, keep.Name, keep.TemplateName, ex);
							}
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Default behavior to send player packets when he enters the region hosting this keep.
		/// </summary>
		/// <param name="player"></param>
		public void OnPlayerEnterRegion(GamePlayer player)
		{			
			player.Out.SendKeepInfo(this);
			
			foreach (GameKeepDataComponent dataComp in m_components)
			{
				player.Out.SendKeepComponentInfo(dataComp);
			}
			
		}
	}
}
