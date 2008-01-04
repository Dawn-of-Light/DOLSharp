using System;
using System.Collections.Generic;
using System.Text;
using DOL.Database;

namespace DOL.GS.Housing
{
	/// <summary>
	/// House item interface.
	/// </summary>
	/// <author>Aredhel</author>
	public interface IHouseHookpointItem
	{
		bool Attach(House house, uint hookpointID);
		bool Attach(House house, DBHousepointItem hookedItem);
		bool Detach();
		int Index { get; }
		String TemplateID { get; }
	}
}
