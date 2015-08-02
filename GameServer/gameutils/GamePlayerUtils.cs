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
using System.Linq;

using DOL.Language;
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// GamePlayer Utils Extension Class
	/// </summary>
	public static class GamePlayerUtils
	{
		/// <summary>
		/// Get Spot Description Checking Any Area with Description or Zone Description
		/// </summary>
		/// <param name="reg"></param>
		/// <param name="spot"></param>
		/// <returns></returns>
		public static string GetSpotDescription(this Region reg, IPoint3D spot)
		{
			return reg.GetSpotDescription(spot.X, spot.Y, spot.Z);
		}
		
		/// <summary>
		/// Get Spot Description Checking Any Area with Description or Zone Description
		/// </summary>
		/// <param name="reg"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static string GetSpotDescription(this Region reg, int x, int y, int z)
		{
			if (reg != null)
			{
				var area = reg.GetAreasOfSpot(x, y, z).OfType<AbstractArea>().FirstOrDefault(a => a.DisplayMessage && !string.IsNullOrEmpty(a.Description));
				
				if (area != null)
					return area.Description;
				
				var zone = reg.GetZone(x, y);
				
				if (zone != null)
					return zone.Description;
				
				return reg.Description;
			}
			
			return string.Empty;
		}

		/// <summary>
		/// Get Spot Description Checking Any Area with Description or Zone Description and Try Translating it
		/// </summary>
		/// <param name="reg"></param>
		/// <param name="client"></param>
		/// <param name="spot"></param>
		/// <returns></returns>
		public static string GetTranslatedSpotDescription(this Region reg, GameClient client, IPoint3D spot)
		{
			return reg.GetTranslatedSpotDescription(client, spot.X, spot.Y, spot.Z);
		}
		
		/// <summary>
		/// Get Spot Description Checking Any Area with Description or Zone Description and Try Translating it
		/// </summary>
		/// <param name="reg"></param>
		/// <param name="client"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static string GetTranslatedSpotDescription(this Region reg, GameClient client, int x, int y, int z)
		{
			if (reg != null)
			{
				var area = reg.GetAreasOfSpot(x, y, z).OfType<AbstractArea>().FirstOrDefault(a => a.DisplayMessage);
				
				// Try Translate Area First
				if (area != null)
				{
					var lng = client.GetTranslation(area) as DBLanguageArea;
					
					if (lng != null && !Util.IsEmpty(lng.ScreenDescription))
						return lng.ScreenDescription;
							
					return area.Description;
				}
				
				var zone = reg.GetZone(x, y);
				
				// Try Translate Zone
				if (zone != null)
				{
					var lng = client.GetTranslation(zone) as DBLanguageZone;
					if (lng != null)
						return lng.ScreenDescription;
					
					return zone.Description;
				}
				
				return reg.Description;
			}
			
			return string.Empty;			
		}
		
		/// <summary>
		/// Get Player Spot Description Checking Any Area with Description or Zone Description and Try Translating it
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string GetTranslatedSpotDescription(this GamePlayer player)
		{
			return player.GetTranslatedSpotDescription(player.CurrentRegion, player.X, player.Y, player.Z);
		}
		
		/// <summary>
		/// Get Player Spot Description Checking Any Area with Description or Zone Description and Try Translating it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static string GetTranslatedSpotDescription(this GamePlayer player, Region region, int x, int y, int z)
		{
			return player.Client.GetTranslatedSpotDescription(region, x, y, z);
		}
		
		/// <summary>
		/// Get Client Spot Description Checking Any Area with Description or Zone Description and Try Translating it
		/// </summary>
		/// <param name="client"></param>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static string GetTranslatedSpotDescription(this GameClient client, Region region, int x, int y, int z)
		{
			return region.GetTranslatedSpotDescription(client, x, y, z);
		}
		
		/// <summary>
		/// Get Player Spot Description Checking Any Area with Description or Zone Description 
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string GetSpotDescription(this GamePlayer player)
		{
			return player.GetTranslatedSpotDescription();
		}
		
		/// <summary>
		/// Get Player's Bind Spot Description Checking Any Area with Description or Zone Description 
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string GetBindSpotDescription(this GamePlayer player)
		{
			return player.GetTranslatedSpotDescription(WorldMgr.GetRegion((ushort)player.BindRegion), player.BindXpos, player.BindYpos, player.BindZpos);
		}
	}
}
