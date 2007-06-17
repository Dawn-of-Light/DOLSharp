using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Camouflage
	/// </summary>
	public class CamouflageEffect : StaticEffect, IGameEffect
	{
		
		public override void Start(GameLiving target)
		{
			base.Start(target);
			if (target is GamePlayer)
				(target as GamePlayer).Out.SendMessage("You are now camouflaged!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
		}

		public override void Stop()
		{
			base.Stop();
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage("Your camouflage is gone.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		
		public override string Name
		{
			get { return "Camouflage"; }
		}
		
		public override ushort Icon
		{
			get { return 476; }
		}
		
		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(1);
				delveInfoList.Add("Available to the archer classes. This ability will make the archer undectable to the Mastery of Stealth Realm Ability for a period of time. Drawing a weapon or engaging in combat will cancel the effect and cause the player to be visible. This does not modify stealth skill.");
				return delveInfoList;
			}
		}
	}
}