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
using System.Collections.Generic;
using System.Text;

namespace DOL.Database
{
	/// <summary>
	/// interface for all business object wich are load from transfer object
	/// </summary>
	/// <typeparam name="Persistentobject"></typeparam>
	public interface IPersistentBusinessObject<Persistentobject>
	{
		/// <summary>
		/// load data from an transfer object and put it in the business object
		/// </summary>
		/// <param name="obj"></param>
		void LoadFromTransferObject(Persistentobject obj);

		/// <summary>
		/// get  the tranfer object from the business object to save/update/delete/... it in DB
		/// </summary>
		/// <returns></returns>
		Persistentobject GetTransferObject();
	}
}
