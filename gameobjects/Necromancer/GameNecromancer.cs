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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.Database;
using log4net;
using DOL.Events;
using DOL.GS.Effects;
using System.Collections;

namespace DOL.GS
{
	class GameNecromancer : GamePlayer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Pet Control

		String m_petName = "";

		/// <summary>
		/// Sets the controlled object for this player
		/// </summary>
		/// <param name="controlledNpc"></param>
		public override void SetControlledNpc(DOL.AI.Brain.IControlledBrain controlledNpc)
		{
			base.SetControlledNpc(controlledNpc);
			if (controlledNpc == null)
			{
				OnPetReleased();
				Out.SendMessage(String.Format("You lose control of the {0}.", m_petName),
					eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
			}
			else
				m_petName = controlledNpc.Body.Name;
		}

		/// <summary>
		/// Releases controlled object
		/// </summary>
		public override void CommandNpcRelease()
		{
			base.CommandNpcRelease();
			OnPetReleased();
		}

		/// <summary>
		/// Invoked when pet is released.
		/// </summary>
		protected virtual void OnPetReleased()
		{
			CasterForm();
		}

		#endregion

		/// <summary>
		/// Necromancer can only attack when it's not a shade.
		/// </summary>
		/// <param name="attackTarget"></param>
		public override void StartAttack(GameObject attackTarget)
		{
            if (!IsShade)
                base.StartAttack(attackTarget);
            else
                Out.SendMessage("You cannot enter combat while you are a shade!",
                    eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
		}

        /// <summary>
        /// If the pet is up, show the pet's health in the group window.
        /// </summary>
        public override byte HealthPercentGroupWindow
        {
            get
            {
                if (ControlledNpc == null)
                    return base.HealthPercentGroupWindow;
				log.Info(String.Format("GameNecromancer.HealthPercentGroupWindow({0}, {1})",
					HealthPercent, ControlledNpc.Body.HealthPercent));
                return ControlledNpc.Body.HealthPercent;
            }
        }

        /// <summary>
        /// Set the tether timer if pet gets out of range or comes back into range.
        /// </summary>
        /// <param name="seconds"></param>
        public void SetTetherTimer(int seconds)
        {
            ShadeEffect shadeEffect = EffectList.GetOfType(typeof(ShadeEffect)) as ShadeEffect;
            if (shadeEffect != null)
            {
                lock (shadeEffect)
                    shadeEffect.SetTetherTimer(seconds);
                ArrayList effectList = new ArrayList(1);
                effectList.Add(shadeEffect);
                int effectsCount = 1;
                Out.SendUpdateIcons(effectList, ref effectsCount);
            }
        }

		protected virtual void CasterForm()
		{
			if (IsShade)
			{
				Shade(false);
			}
		}

		public GameNecromancer(GameClient client, Character theChar)
			: base(client, theChar)
		{
			CasterForm();
		}
	}
}
