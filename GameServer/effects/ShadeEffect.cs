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
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Effects
{
	public class ShadeEffect : StaticEffect, IGameEffect
	{
		protected GamePlayer player;

		public override void Start(GameLiving living)
		{
			if (living is GamePlayer p)
			{
				player = p;
				player.EffectList.Add(this);
                player.Out.SendUpdatePlayer();
				player.Model = player.ShadeModel;
			}
		}

		public override void Stop()
		{
            if (player != null)
            {
                player.EffectList.Remove(this);
                player.Out.SendUpdatePlayer();
            }
		}

		public override void Cancel(bool playerCancel) 
		{
            if (player != null)
			{
				if (player.ShadeEffect != null)
                {
                    // Drop shade form.
                    player.ShadeEffect.Stop();
                    player.ShadeEffect = null;
                }
                // Drop shade form.
                player.Model = player.CreationModel;
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.Shade.NoLongerShade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		public override string Name 
        { 
            get { return LanguageMgr.GetTranslation(player.Client, "Effects.ShadeEffect.Name"); } 
        }	

		public override ushort Icon 
        { 
            get { return 0x193; } 
        }

		public override IList<string> DelveInfo 
        { 
            get { return Array.Empty<string>(); } 
        }
	}
}
