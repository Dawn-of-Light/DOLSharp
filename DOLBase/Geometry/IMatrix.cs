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

namespace DOL.Geometry
{
	public interface IMatrix
	{
		float _11
			{ get; set;}
		float _12
			{ get; set;}
		float _13
			{ get; set;}
		float _14
			{ get; set;}
		float _21
			{ get; set;}
		float _22
			{ get; set;}
		float _23
			{ get; set;}
		float _24
			{ get; set;}
		float _31
			{ get; set;}
		float _32
			{ get; set;}
		float _33
			{ get; set;}
		float _34
			{ get; set;}
		float _41
			{ get; set;}
		float _42
			{ get; set;}
		float _43
			{ get; set;}
		float _44
			{ get; set;}
		
		void SetCell(int x, int y, float value);
		float GetCell(int x, int y); 

		void Inverse();

	}
}
