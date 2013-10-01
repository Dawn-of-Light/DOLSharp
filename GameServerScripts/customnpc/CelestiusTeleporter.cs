/*
 * Created by SharpDevelop.
 * User: PJSR05841
 * Date: 13/09/2013
 * Time: 11:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

using DOL.GS;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Description of CelestiusTeleporter.
	/// </summary>
	public class CelestiusTeleporter : GameTeleporter
	{

		public override ushort Model {
			get { return 0x4aa; }
		}
		
		protected override string Type {
			get { return GetType().FullName; }
		}
		
		public override bool Interact(GamePlayer player)
		{
			if(!base.Interact(player))
				return false;
			
			SayTo(player, "Hello I can port you to and from Celestius Lobby !\n\nJump to Celestius [lobby]\nGet [back] to City of Aerus");
			return true;
		}
	}
	
	
}
