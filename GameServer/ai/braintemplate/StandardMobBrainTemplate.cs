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
using DOL.GS.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.AI.Brain
{
	/// <summary>
	/// Standard brain for standard mobs
	/// </summary>
	public class StandardMobBrainTemplate : ABrainTemplate
	{
		/// <summary>
		/// Max Aggro range in that this npc searches for enemies
		/// </summary>
		protected int m_aggroRange = 0;
		/// <summary>
		/// Aggressive Level of this npc
		/// </summary>
		protected int m_aggroLevel = 0;
		
		/// <summary>
		/// Aggressive Level in % 0..100, 0 means not Aggressive
		/// </summary>
		public virtual int AggroLevel
		{
			get { return m_aggroLevel; }
			set { m_aggroLevel = value; }
		}

		/// <summary>
		/// Range in that this npc aggros
		/// </summary>
		public virtual int AggroRange
		{
			get { return m_aggroRange; }
			set { m_aggroRange = value; }
		}

		/// <summary>
		/// Create a standerdmobbrain usable by npc
		/// </summary>
		public override ABrain CreateInstance()
		{
			StandardMobBrain brain = new StandardMobBrain();
			brain.AggroLevel = m_aggroLevel;
			brain.AggroRange = m_aggroRange;
			return brain;
		}
	}
}
