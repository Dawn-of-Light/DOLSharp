using System;
using DOL.Database;
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.JumpPoints
{
	/// <summary>
	/// Description résumée de RealmCheckJumpPointTargetLocation.
	/// </summary>
	public class JumpPointMultiTargetLocation : JumpPointTargetLocation
	{
		#region Declaration
		
		/// <summary>
		/// The realm allowed to use this jump point
		/// </summary>
		protected eRealm m_realm;		

		/// <summary>
		/// Return the realm allowed to use this jump point
		/// </summary>
		public eRealm Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}

		#endregion
	}
}
