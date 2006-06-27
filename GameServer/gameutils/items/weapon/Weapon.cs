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
	/// Summary description for a Weapon
	/// </summary> 
	public abstract class Weapon : VisibleEquipment
	{
		#region Declaraction
		/// <summary>
		/// The DPS of the weapon
		/// </summary>
		private byte m_damagePerSecond;

		/// <summary>
		/// The SPD of the weapon (in milli sec)
		/// </summary>
		private int m_speed;

		/// <summary>
		/// The damage type of the weapon
		/// </summary>
		protected eDamageType m_damageType;

		/// <summary>
		/// Witch hand(s) are needed to use this weapon
		/// </summary>
		protected eHandNeeded m_handNeeded;

		/// <summary>
		/// The glow effect id show with this weapon template
		/// </summary>
		private int m_glowEffect;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the DPS of this weapon
		/// </summary>
		public byte DamagePerSecond
		{
			get { return m_damagePerSecond; }
			set	{ m_damagePerSecond = value; }
		}

		/// <summary>
		/// Gets or sets the SPD of this weapon (in milli sec)
		/// </summary>
		public int Speed
		{
			get { return m_speed; }
			set	{ m_speed = value; }
		}

		/// <summary>
		/// Gets or sets the damage type of this weapon
		/// </summary>
		public virtual eDamageType DamageType
		{
			get { return m_damageType; }
			set	{ m_damageType = value; }
		}

		/// <summary>
		/// Gets or sets the glow effect to show with this weapon
		/// </summary>
		public int GlowEffect
		{
			get { return m_glowEffect; }
			set	{ m_glowEffect = value; }
		}

		/// <summary>
		/// Gets or sets witch hand(s) are needed to use this weapon
		/// </summary>
		public virtual eHandNeeded HandNeeded
		{
			get { return m_handNeeded; }
			set	{ m_handNeeded = value; }
		}

		#endregion
	}
}	