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

using DOL.GS.Spells;

namespace DOL.GS
{
	/// <summary>
	/// CustomizableGamePet is a class to handle GamePet that take orders through keywords such as Taunt abilities or Parry/Block.
	/// This needs to be extended to handle further keywords or custom abilities.
	/// </summary>
	public class CustomizableGamePet : GamePet
	{
		#region Block
		/// <summary>
		/// Block Toggle
		/// </summary>
		protected bool m_isBlockEnabled;
		
		/// <summary>
		/// Block Toggle Getter
		/// </summary>
		public bool IsBlockEnabled 
		{
			get { return m_isBlockEnabled; }
		}
		
		/// <summary>
		/// Check if the Pet has the ability to Block regardless of Toggle
		/// </summary>
		public virtual bool HasBlock
		{
			get { return base.BlockChance > 0; }
		}
		
		/// <summary>
		/// Override Block Chance Based on Toggle.
		/// </summary>
		public override byte BlockChance
		{
			get
			{
				if (IsBlockEnabled)
				{
					return base.BlockChance;
				}
				else
				{
					return 0;
				}
			}
			set { base.BlockChance = value; }
		}
		#endregion
		
		#region Parry
		/// <summary>
		/// Parry Toggle
		/// </summary>
		protected bool m_isParryEnabled;
		
		/// <summary>
		/// Parry Toggle Getter
		/// </summary>
		public virtual bool IsParryEnabled
		{
			get { return m_isParryEnabled; }
		}
		
		/// <summary>
		/// Check if the Pet has the ability to Parry regardless of Toggle
		/// </summary>
		public virtual bool HasParry
		{
			get { return base.ParryChance > 0; }
		}
		
		/// <summary>
		/// Override Parry Chance based on Toggle.
		/// </summary>
		public override byte ParryChance
		{
			get 
			{
				if (IsParryEnabled)
				{
					return base.ParryChance; 
				}
				else
				{
					return 0;
				}
			}
			set { base.ParryChance = value; }
		}
		#endregion
		
		// Constructor
		public CustomizableGamePet(INpcTemplate template)
			: base(template)
		{
		}
	}
}
