using DOL.Events;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Class for the Lord Guard
	/// </summary>
	public class GuardLord : GameKeepGuard
	{
		/// <summary>
		/// Lord needs more health at the moment
		/// </summary>
		public override int MaxHealth
		{
			get
			{
				return base.MaxHealth * 3;
			}
		}

		public override int RealmPointsValue
		{
			get
			{
				if (this.Component == null || this.Component.Keep == null)
					return 5000;
				else
				{
					int value = this.Component.Keep.BaseLevel * 4 * (this.Component.Keep.Level + 1);
					if (this.Component.Keep is GameKeep)
						value *= 4;
					return value;
				}
			}
		}

		public override int AttackRangeDistance
		{
			get
			{
				return 1200;
			}
		}

		/// <summary>
		/// When Lord dies, we update Area Mgr to call the various functions we need
		/// And update the player stats
		/// </summary>
		/// <param name="killer">The killer object</param>
		public override void Die(GameObject killer)
		{
			if (this.Component != null)
				GameServer.ServerRules.ResetKeep(this, killer);

			base.Die(killer);
		}

		/// <summary>
		/// When we interact with lord, we display all possible options
		/// </summary>
		/// <param name="player">The player object</param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (this.Component == null)
				return false;

            player.Out.SendMessage("Would you like to [Claim Keep] now? Or maybe [Release Keep]?", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            return true;
		}

		/// <summary>
		/// From a great distance, damage does not harm lord
		/// </summary>
		/// <param name="source">The source of the damage</param>
		/// <param name="damageType">The type of the damage</param>
		/// <param name="damageAmount">The amount of the damage</param>
		/// <param name="criticalAmount">The critical hit amount of damage</param>
		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			int distance = 0;
			if (this.Component != null && this.Component.Keep != null && this.Component.Keep is GameKeep)
				distance = 750;
			else distance = 350;
			if (WorldMgr.GetDistance(source, this) > distance)
			{
				if (source is GamePlayer)
					(source as GamePlayer).Out.SendMessage(this.Name + " is immune to damage from this range", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			base.TakeDamage(source, damageType, damageAmount, criticalAmount);
		}
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer player = (GamePlayer)source;
            byte flag = 0;
            switch (str)
            {
                case "Claim Keep":
                    {
                        if (PlayerMgr.IsAllowedToInteract(player, this.Component.Keep, eInteractType.Claim))
                        {
                            player.Out.SendDialogBox(eDialogCode.KeepClaim, (ushort)player.ObjectID, 0, 0, 0, eDialogType.YesNo, false, "Do you wish to claim\n" + this.Component.Keep.Name + "?");
                            return true;
                        }
                        break;
                    }
                case "Release Keep":
                    {
                        if (PlayerMgr.IsAllowedToInteract(player, this.Component.Keep, eInteractType.Release))
                        {
                            flag += 4;
                        }
                        break;
                    }
            }
            if (flag > 0)
                player.Out.SendKeepClaim(this.Component.Keep, flag);

            return true;
        }
    }
}
