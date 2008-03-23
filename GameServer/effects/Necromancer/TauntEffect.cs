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
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Pet taunt effect. While active, the pet will keep trying
	/// to taunt its target in case it is attacking someone else.
	/// </summary>
	/// <author>Aredhel</author>
	class TauntEffect : StaticEffect, IGameEffect
	{
		/// <summary>
		/// Creates a new taunt effect.
		/// </summary>
		public TauntEffect()
			: base() { }

		/// <summary>
		/// Start the effect.
		/// </summary>
		/// <param name="target"></param>
		public override void Start(GameLiving target)
		{
			base.Start(target);

			GamePlayer petOwner = null;
			if (target is GameNPC && (target as GameNPC).Brain is IControlledBrain)
				petOwner = ((target as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;

			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (player == null)
					continue;

				player.Out.SendSpellEffectAnimation(target, target, 1073, 0, false, 1);

				eChatType chatType = (player != null && player == petOwner)
					? eChatType.CT_Spell
					: eChatType.CT_System;

				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Effects.Necro.TauntEffect.SeemsChange", target.GetName(0, true)), chatType, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Stop the effect.
		/// </summary>
		public override void Stop()
		{
			base.Stop();

			GamePlayer petOwner = null;
			if (Owner is GameNPC && (Owner as GameNPC).Brain is IControlledBrain)
				petOwner = ((Owner as GameNPC).Brain as IControlledBrain).Owner as GamePlayer;

			foreach (GamePlayer player in Owner.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (player == null)
					continue;

				eChatType chatType = (player == petOwner)
					? eChatType.CT_SpellExpires
					: eChatType.CT_System;

				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Effects.Necro.TauntEffect.SeemsLessAgg", Owner.GetName(0, true)), chatType, eChatLoc.CL_SystemWindow);
			}
		}
	}
}
