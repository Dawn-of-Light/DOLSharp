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
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using System.Reflection;
using DOL.Events;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Description résumée de GameKeepLord.
	/// </summary>
	public class GameKeepLord : GameKeepGuard
	{
		public GameKeepLord() : base()
		{

		}
		public GameKeepLord(AbstractGameKeep keep) : base(keep)
		{}
		public GameKeepLord(GameKeepGuard guard)
		{
			this.Realm = guard.Realm;
			this.Level = guard.Level;
			this.Region  = guard.Region;
			this.Position = guard.Position;
			this.Heading = guard.Heading;
			this.CurrentSpeed = guard.CurrentSpeed;
			this.MaxSpeedBase = guard.MaxSpeedBase;
			this.GuildName = guard.GuildName;
			this.Size = guard.Size;
		}

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public override bool AddToKeep(AbstractGameKeep keep)
		{
			this.Level+= 20;
			if (keep.Lord != null)
			{
				log.Error("the keep "+keep.Name + " have ever a lord");
				return false;
			}
			keep.Lord = this;
			GameEventMgr.AddHandler(this,GameNPCEvent.Dying,new DOLEventHandler((Brain as KeepGuardBrain).Keep.LordKilled));
			return true;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			KeepGuardBrain myBrain = this.Brain as KeepGuardBrain;
			if (myBrain == null)
				return false;

			this.TurnTo(player.Position);
			if ( myBrain.Keep.Guild == null)
				this.SayTo(player, eChatLoc.CL_PopupWindow, "Do you want to [Claim] this keep?");
			else
			{
				if (!(myBrain.Keep is GameKeepTower))
					this.SayTo(player, eChatLoc.CL_PopupWindow, "You can change the type of the keep and switch to [melee], [scout] or [caster].By default, it is melee.");
				this.SayTo(player, eChatLoc.CL_PopupWindow, "Do you want to [Upgrade] this keep?");
			}
			return true;
		}
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source,str))
				return false;

			KeepGuardBrain myBrain = this.Brain as KeepGuardBrain;
			if (myBrain == null)
				return false;

			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;
			if (str == "Claim")
			{
				if (!myBrain.Keep.CheckForClaim(player))
					return false;
				myBrain.Keep.Claim(player);
			}
			if (myBrain.Keep.Guild == null) return false;
			switch(str)
			{
				case "upgrade":	
				{
					
					string msg =  "what is the target level you want to upgradethis keep?";
					for (int i = myBrain.Keep.Level+1;i<=10;i++)
						msg += " ["+i+"]";
					this.SayTo(player, eChatLoc.CL_PopupWindow, "what is the target level you want to upgradethis keep?");
				}break;
				case "melee": 
				{
					myBrain.Keep.KeepType = GameKeep.eKeepType.Melee;
				}break;
				case "scout": 
				{
					myBrain.Keep.KeepType = GameKeep.eKeepType.Stealth;
				}break;
				case "caster": 
				{
					myBrain.Keep.KeepType = GameKeep.eKeepType.Magic;
				}break;
				case "1": 
				case "2":
				case "3":
				case "4":
				case "5":
				case "6":
				case "7":
				case "8":
				case "9":
				case "10":
				{
					int targetlevel = Convert.ToInt32(str);
					myBrain.Keep.Upgrade(targetlevel);
				}break;
			}
			return true;
		}
	}
}
