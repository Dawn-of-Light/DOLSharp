using System;
using System.Collections.Generic;
using System.Text;

namespace DOL.Database
{
	/// <summary>
	/// Thrown when requested row is not found.
	/// </summary>
	public class RowNotFoundException : DolDatabaseException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:RowNotFoundException"/> class.
		/// </summary>
		public RowNotFoundException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:RowNotFoundException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public RowNotFoundException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:RowNotFoundException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public RowNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
