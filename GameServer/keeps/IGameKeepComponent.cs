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

using System.Collections.Generic;

namespace DOL.GS.Keeps
{

    /// <summary>
    /// Interface implementing needed methods for sending Game Keep Component over network.
    /// </summary>
    public interface IGameKeepComponent
	{
		/// <summary>
		/// Reference to owning Keep
		/// </summary>
		IGameKeep Keep { get; }
		
		/// <summary>
		/// Hook point Collection
		/// </summary>
		IDictionary<int, DOL.GS.Keeps.GameKeepHookPoint> HookPoints { get; }
		
		/// <summary>
		/// Keep Component ID.
		/// </summary>
		int ID { get; }
		
		/// <summary>
		/// GameObject ObjectID.
		/// </summary>
		int ObjectID { get; }
		
		/// <summary>
		/// Keep Component Skin ID.
		/// </summary>
		int Skin { get; }
		
		/// <summary>
		/// Keep Component X.
		/// </summary>
		int ComponentX { get; }
		
		/// <summary>
		/// Keep Component Y.
		/// </summary>
		int ComponentY { get; }
		
		/// <summary>
		/// Keep Component Heading.
		/// </summary>
		int ComponentHeading { get; }
		
		/// <summary>
		/// Keep Component Height
		/// </summary>
		int Height { get; }
		
		/// <summary>
		/// GameLiving Health Percent
		/// </summary>
		byte HealthPercent { get; }
		
		/// <summary>
		/// Status of GameComponent (Flag)
		/// </summary>
		byte Status { get; }
		
		/// <summary>
		/// Is Tower Componetn Raized ?
		/// </summary>
		bool IsRaized { get; }
		
		/// <summary>
		/// Enable component Climbing.
		/// </summary>
		bool Climbing { get; }
		
		/// <summary>
		/// GameObject AddToWorld Method
		/// </summary>
		/// <returns></returns>
		bool AddToWorld();
		
		/// <summary>
		/// GameLiving IsAlive.
		/// </summary>
		bool IsAlive { get; }
		
	}
}