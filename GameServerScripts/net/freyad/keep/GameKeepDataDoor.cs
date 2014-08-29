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

using DOL.GS;
using DOL.Database;

namespace net.freyad.keep
{
	/// <summary>
	/// Description of GameKeepDataDoor.
	/// </summary>
	public class GameKeepDataDoor : GameObject, IDoor, IGameKeepObject
	{
		public GameKeepDataDoor()
		{
		}
		
		public override int X {
			get { return Keep.X+XOff; }
			set { base.X = value; }
		}
		
		public override int Y {
			get { return Keep.Y+YOff; }
			set { base.Y = value; }
		}
		
		public override int Z {
			get { return Keep.Z+ZOff; }
			set { base.Z = value; }
		}
		
		public override Region CurrentRegion {
			get { return Keep.CurrentRegion; }
			set { base.CurrentRegion = value; }
		}
		
		private IGameKeepData m_keep;
		
		public IGameKeepData Keep {
			get { return m_keep; }
			set { m_keep = value; CurrentRegion = value.CurrentRegion; }
		}
		
		private int m_xOff;
		
		public int XOff {
			get { return m_xOff; }
			set { m_xOff = value; }
		}
		
		private int m_yOff;
		
		public int YOff {
			get { return m_yOff; }
			set { m_yOff = value; }
		}
		
		private int m_zOff;
		
		public int ZOff {
			get { return m_zOff; }
			set { m_zOff = value; }
		}
		
		public override string Name	{get{return "";}}
		public virtual uint Flag {get{return 0;}}
		public override ushort Heading	{get{return 0;}}
		public virtual ushort ZoneID { get{return 0;}}
		public override eRealm Realm {get{return eRealm.None;}}
		public virtual int DoorID	{get{return 0;}}
		public virtual eDoorState State {get{return eDoorState.Open;} set{return;}}
		public virtual void Open(GameLiving opener = null)
		{
			
		}
		public virtual void Close(GameLiving closer = null)
		{
			
		}
		public virtual void NPCManipulateDoorRequest(GameNPC npc, bool open)
		{
			
		}
		public override void LoadFromDatabase(DataObject obj)
		{
			if (obj is DBDoor)
				LoadFromDatabase((DBDoor)obj);
			
			if (obj is KeepDataPosition)
				LoadFromDatabase((KeepDataPosition)obj);
		}
		
		public virtual void LoadFromDatabase(DBDoor door)
		{			
		}
		
		public virtual void LoadFromDatabase(KeepDataPosition pos)
		{
			XOff = pos.Xoff;
			YOff = pos.Yoff;
			ZOff = pos.Zoff;
		}
	}
}
