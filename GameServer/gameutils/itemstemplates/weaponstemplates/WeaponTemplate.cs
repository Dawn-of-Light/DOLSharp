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
	public enum eHandNeeded : byte
	{
		RightHand	= 0,
		TwoHands	= 1,
		LeftHand	= 2,
	}

	/// <summary>
	/// Summary description for a WeaponTemplate
	/// </summary> 
	public abstract class WeaponTemplate : VisibleEquipmentTemplate
	{
		#region Declaraction
		/// <summary>
		/// The DPS of the weapon
		/// </summary>
		protected byte m_damagePerSecond;

		/// <summary>
		/// The SPD of the weapon (in milli sec)
		/// </summary>
		protected int m_speed;

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
		protected int m_glowEffect;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the DPS of this weapon template
		/// </summary>
		public byte DamagePerSecond
		{
			get { return m_damagePerSecond; }
			set	{ m_damagePerSecond = value; }
		}

		/// <summary>
		/// Gets or sets the SPD of this weapon template (in milli sec)
		/// </summary>
		public int Speed
		{
			get { return m_speed; }
			set	{ m_speed = value; }
		}

		/// <summary>
		/// Gets or sets the damage type of this weapon template
		/// </summary>
		public eDamageType DamageType
		{
			get { return m_damageType; }
			set	{ m_damageType = value; }
		}

		/// <summary>
		/// Gets or sets witch hands are needed to use this weapon
		/// </summary>
		public eHandNeeded HandNeeded
		{
			get { return m_handNeeded; }
			set	{ m_handNeeded = value; }
		}

		/// <summary>
		/// Gets or sets the glow effect to show with this weapon template
		/// </summary>
		public int GlowEffect
		{
			get { return m_glowEffect; }
			set	{ m_glowEffect = value; }
		}

		#endregion

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList GetDelveInfo(GamePlayer player)
		{
			ArrayList list = (ArrayList) base.GetDelveInfo(player);

			double itemDPS = DamagePerSecond/10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * player.Level);
			double itemSPD = Speed/1000.0;
			double effectiveDPS = itemDPS * Quality/100.0 * Condition/100;

			list.Add(" ");
			list.Add(" ");
			list.Add("Damage Modifiers:");
			list.Add("- " + itemDPS.ToString("0.0") + " Base DPS");
			list.Add("- " + clampedDPS.ToString("0.0") + " Clamped DPS");
			list.Add("- " + itemSPD.ToString("0.0") + " Weapon Speed");
			list.Add("- " + Quality + "% Quality");
			list.Add("- " + (byte)Condition + "% Condition");
			
			switch(DamageType)
			{
				case eDamageType.Crush:  list.Add("Damage Type: Crush"); break;
				case eDamageType.Slash:  list.Add("Damage Type: Slash"); break;
				case eDamageType.Thrust: list.Add("Damage Type: Thrust"); break;
				default: list.Add("Damage Type: Natural"); break;
			}

			list.Add(" ");
			list.Add("Effective Damage:");
			list.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
			
			return list;
		}
	}
}	