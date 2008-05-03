using System;
using System.Collections.Generic;
using System.Text;
using DOL.Database2;

namespace DOL.GS.Housing
{
	/// <summary>
	/// House item interface.
	/// </summary>
	/// <author>Aredhel</author>
	public interface IHouseHookpointItem
	{
		bool Attach(House house, uint hookpointID, ushort heading);
		bool Attach(House house, DBHousepointItem hookedItem);
		bool Detach();
		int Index { get; }
		String TemplateID { get; }
	}
}
