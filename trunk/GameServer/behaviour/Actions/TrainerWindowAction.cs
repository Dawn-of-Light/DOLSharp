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
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.Behaviour.Attributes;
using DOL.GS.Behaviour;
using DOL.Database;
using DOL.AI.Brain;
using log4net;
using System.Reflection;

namespace DOL.GS.Behaviour.Actions
{
	[ActionAttribute(ActionType = eActionType.TrainerWindow, IsNullableP = true)]
	public class TrainerWindowAction : AbstractAction<Nullable<Int32>, GameNPC>
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public TrainerWindowAction(GameNPC defaultNPC)
			: base(defaultNPC, eActionType.TrainerWindow)
		{
		}
		public override void Perform(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);
			if (player != null)
				player.Out.SendTrainerWindow();
		}
	}
}
