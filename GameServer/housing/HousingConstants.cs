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

namespace DOL.GS.Housing
{
	public enum PermissionType
	{
		Player = 1,
		Guild = 2,
		GuildRank = 3,
		Account = 4,
		All = 5,
		Class = 6,
		Race = 7
	}

	[Flags]
	public enum VaultPermissions : byte
	{
		None = 0x00,
		Add = 0x01,
		Remove = 0x02,
		View = 0x04
	}

	[Flags]
	public enum DecorationPermissions : byte
	{
		None = 0x00,
		Add = 0x01,
		Remove = 0x02
	}

	[Flags]
	public enum ConsignmentPermissions : byte
	{
		AddRemove = 0x03,
		Withdraw = 0x10,
		Any = AddRemove | Withdraw
	}

	public static class HousingConstants
	{
		public const string BPsForHouseRent = "BPsForHouseRent";
		public const string HouseForHouseRent = "HouseForHouseRent";
		public const int HouseViewingDistance = 10120; //guessed, but i'm sure its > vis dist.
		public const int MaxHookpointLocations = 40;
		public const int MaxHouseModel = 12;
		public const int MaxPermissionLevel = 9;
		public const int MinPermissionLevel = 1;
		public const string MoneyForHouseRent = "MoneyForHouseRent";
		public const string AllowAddHouseHookpoint = "housing_add_hookpoints";

		/// <summary>
		/// Number of items a single house vault can hold.
		/// </summary>
		public const int VaultSize = 100;

		/// <summary>
		/// Multi-dimensional array of consignment positioning primitives.
		/// </summary>
		/// <remarks>First dimensions is the model of the house.  Second dimension is a 4 value wide array, consisting of
		/// multiplier, range, Z addition, and realm, in that order.  Second dimension index is one-based, not zero-based.</remarks>
		public static readonly float[][] ConsignmentPositioning = new[]
		                                                          	{
		                                                          		null,
		                                                          		new[] {0.55f, 630, 40, 1}, // model 1
		                                                          		new[] {0.55f, 630, 40, 1}, // model 2
		                                                          		new[] {-0.55f, 613, 100, 1}, // model 3
		                                                          		new[] {0.53f, 620, 100, 1}, // model 4
		                                                          		new[] {-0.47f, 755, 40, 2}, // model 5
		                                                          		new[] {-0.5f, 630, 40, 2}, // model 6
		                                                          		new[] {0.48f, 695, 100, 2}, // model 7
		                                                          		new[] {-0.505f, 680, 100, 2}, // model 8
		                                                          		new[] {0.475f, 693, 40, 3}, // model 9
		                                                          		new[] {0.47f, 688, 40, 3}, // model 10
		                                                          		new[] {-0.65f, 603, 100, 3}, // model 11
		                                                          		new[] {-0.58f, 638, 100, 3} // model 12
		                                                          	};

		/// <summary>
		/// Housing hookpoint coordinates offset relative to a house.
		/// </summary>
		/// <remarks>Index is one-based, not zero-based.</remarks>
		public static readonly int[][][] RelativeHookpointsCoords = new[]
		                                                            	{
		                                                            		// NOTHING : Lot
		                                                            		null,
		                                                            		// ALB Cottage (model 1)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// ALB (model 2)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// ALB Villa(model 3)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// ALB Mansion(model 4)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// MID Cottage (model 5)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// MID (model 6)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// MID (model 7)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// MID (model 8)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// MID (model 9)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// MID (model 10)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// MID (model 11)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            		// MID (model 12)
		                                                            		new int[MaxHookpointLocations + 1][],
		                                                            	};
	}
}