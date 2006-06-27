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
using DOL.GS;

namespace DOL.AI.Brain
{
	/// <summary>
	/// keep guard brain and lord brain
	/// </summary>
	public class KeepGuardBrain : GuardBrain
	{
        /// <summary>
        /// the constructor
        /// </summary>
		public KeepGuardBrain() : base()
		{
			
		}

		protected AbstractGameKeep m_keep;

        /// <summary>
        /// Gets or sets the keep.
        /// </summary>
        /// <value>The keep.</value>
		public AbstractGameKeep Keep 
		{ 
			get{return m_keep;}
			set{m_keep=value;} 
		}
	}
}
