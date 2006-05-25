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
using DOL.Database.DataAccessInterfaces;
using DOL.Database.DataTransferObjects;
using NHibernate.Expression;

namespace DOL.Database.NHibernate
{
	/// <summary>
	/// NHibernate implementation of <see cref="IAccountDao"/> interface.
	/// </summary>
	public class AccountDao : GenericDao<AccountTO, int>, IAccountDao
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountDao"/> class.
		/// </summary>
		/// <param name="state">The state.</param>
		public AccountDao(NHState state) : base(state)
		{
		}

		/// <summary>
		/// Finds an <see cref="AccountTO"/> by its name.
		/// </summary>
		/// <param name="accountName">The account name to find.</param>
		/// <returns>The found object or null.</returns>
		public AccountTO FindByName(string accountName)
		{
			return (AccountTO) Database.SelectObject(typeof(AccountTO), Expression.Eq("AccountName", accountName));
		}
	}
}