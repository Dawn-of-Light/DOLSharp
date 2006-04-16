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
using System.Collections;
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a GenericArmor
	/// </summary> 
	public class NPCArmor : NPCEquipment
	{
		#region Constructor
		/// <summary>
		/// No args constructor
		/// </summary>
		public NPCArmor() 
		{
		}

		/// <summary>
		/// Construtor to use in scripts
		/// </summary>
		public NPCArmor(int model) : base(model)
		{
			ModelExtension = 0;
		}

		/// <summary>
		/// Construtor to use in scripts
		/// </summary>
		public NPCArmor(int model, int color, byte modelExtension) : base(model, color)
		{
			ModelExtension = modelExtension;
		}

		#endregion

		#region Declaration

		/// <summary>
		/// The model extension of this armor (it is often the relief level of the item)
		/// </summary>
		private byte m_modelExtension;

		/// <summary>
		/// Gets or sets the model extension of this armor (it is often the relief level of the item)
		/// </summary>
		public byte ModelExtension
		{
			get { return m_modelExtension; }
			set	{ m_modelExtension = value; }
		}
		#endregion
	}
}
