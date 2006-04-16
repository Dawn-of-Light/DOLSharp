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
using DOL.GS;
using log4net;

namespace DOL.AI
{
	/// <summary>
	/// This class is the base template of all arteficial intelligence in game objects
	/// </summary>
	public abstract class ABrainTemplate
	{
		/// <summary>
		/// The unique id of this brain
		/// </summary>
		protected int m_id;

		/// <summary>
		/// Returns the unique ID of this brain
		/// </summary>
		public int ABrainTemplateID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The body of this brain
		/// </summary>
		protected GameNPCTemplate m_bodyTemplate;

		/// <summary>
		/// Gets/sets the body of this brain
		/// </summary>
		public GameNPCTemplate BodyTemplate
		{
			get { return m_bodyTemplate; }
			set { m_bodyTemplate = value; }
		}

		/// <summary>
		/// Create a brain usable by npc
		/// </summary>
		public abstract ABrain CreateInstance();
	}
}
