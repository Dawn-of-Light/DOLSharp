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
using System.Collections.Generic;
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

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
				(target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client, "Effects.CamouflageEffect.YouAreCamouflaged"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
		}

		public override void Stop()
		{
			base.Stop();
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Client, "Effects.CamouflageEffect.YourCFIsGone"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
		
		public override string Name
		{
			get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.CamouflageEffect.Name"); }
		}
		
		public override ushort Icon
		{
			get { return 476; }
		}
		
		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
				var delveInfoList = new List<string>();
				delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.CamouflageEffect.InfoEffect"));

				return delveInfoList;
			}
		}
	}
}