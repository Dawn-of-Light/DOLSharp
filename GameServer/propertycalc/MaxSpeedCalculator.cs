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
using DOL.GS.Effects;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Max Speed calculator
	/// 
	/// BuffBonusCategory1 unused
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 used for all multiplicative speed bonuses
	/// </summary>
	[PropertyCalculator(eProperty.MaxSpeed)]
	public class MaxSpeedCalculator : PropertyCalculator
	{
		public static readonly double SPEED1 = 1.753;
		public static readonly double SPEED2 = 1.816;
		public static readonly double SPEED3 = 1.91;
		public static readonly double SPEED4 = 1.989;
		public static readonly double SPEED5 = 2.068;

		public override int CalcValue(GameLiving living, eProperty property)
		{
			if (living.IsMezzed || living.IsStunned) return 0;

			double speed = living.BuffBonusMultCategory1.Get((int)property);

			if (living is GamePlayer)
			{
				GamePlayer player = (GamePlayer)living;
				//				Since Dark Age of Camelot's launch, we have heard continuous feedback from our community about the movement speed in our game. The concerns over how slow
				//				our movement is has continued to grow as we have added more and more areas in which to travel. Because we believe these concerns are valid, we have decided
				//				to make a long requested change to the game, enhancing the movement speed of all players who are out of combat. This new run state allows the player to move
				//				faster than normal run speed, provided that the player is not in any form of combat. Along with this change, we have slightly increased the speed of all
				//				secondary speed buffs (see below for details). Both of these changes are noticeable but will not impinge upon the supremacy of the primary speed buffs available
				//				to the Bard, Skald and Minstrel.
				//				- The new run speed does not work if the player is in any form of combat. All combat timers must also be expired.
				//				- The new run speed will not stack with any other run speed spell or ability, except for Sprint.
				//				- Pets that are not in combat have also received the new run speed, only when they are following, to allow them to keep up with their owners.
				double horseSpeed = (player.IsOnHorse ? player.ActiveHorse.Speed * 0.01 : 1.0);
				if (speed > horseSpeed)
					horseSpeed = 1.0;
				if (speed == 1 && !player.InCombat && !player.IsStealthed && !player.CurrentRegion.IsRvR)
					speed *= 1.25; // new run speed is 125% when no buff

				if (player.IsOverencumbered && player.Client.Account.PrivLevel < 2)
				{
					double Enc = player.Encumberance; // calculating player.Encumberance is a bit slow with all those locks, don't call it much
					if (Enc > player.MaxEncumberance)
					{
						speed *= ((((player.MaxSpeedBase * 1.0 / GamePlayer.PLAYER_BASE_SPEED) * (-Enc)) / (player.MaxEncumberance * 0.35f)) + (player.MaxSpeedBase / GamePlayer.PLAYER_BASE_SPEED) + ((player.MaxSpeedBase / GamePlayer.PLAYER_BASE_SPEED) * player.MaxEncumberance / (player.MaxEncumberance * 0.35)));
						if (speed <= 0)
						{
							speed = 0;
							player.Out.SendMessage("You are encumbered and cannot move.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
					else
					{
						player.IsOverencumbered = false;
					}
				}
				if (player.IsStealthed)
				{
					double stealthSpec = player.GetModifiedSpecLevel(Specs.Stealth);
					if (stealthSpec > player.Level)
						stealthSpec = player.Level;
					speed *= 0.3 + (stealthSpec + 10) * 0.3 / (player.Level + 10);
					VanishEffect vanish = player.EffectList.GetOfType(typeof(VanishEffect)) as VanishEffect;
					if (vanish != null)
						speed *= vanish.SpeedBonus;
					MasteryOfStealthAbility mos = player.GetAbility(typeof(MasteryOfStealthAbility)) as MasteryOfStealthAbility;
					if (mos != null)
						speed *= 1 + MasteryOfStealthAbility.GetSpeedBonusForLevel(mos.Level);
				}
				if (player.IsSprinting) speed *= 1.3;
				speed *= horseSpeed;
			}
			else if (living is GameNPC)
			{
				//Ryan: edit for BD
				// 125% speed if following owner
				if (speed == 1.0 && !living.InCombat)
				{
					IControlledBrain brain = ((GameNPC)living).Brain as IControlledBrain;
					if (brain != null)
					{
						GamePlayer owner = brain.GetPlayerOwner();
						if (owner != null)
						{
							if (owner == brain.Body.CurrentFollowTarget)
								speed *= 1.25;
							if (owner.IsSprinting)
								speed *= 1.3;
							if (owner.IsOnHorse)
								speed *= 1.45;
							if (owner.IsOnHorse && owner.IsSprinting)
								speed *= 1.7;
						}
					}
				}
                else if (!living.InCombat)
                {
                    IControlledBrain brain = ((GameNPC)living).Brain as IControlledBrain;

					if (brain != null)
					{
						//Ryan: edit for BD
						GamePlayer owner = brain.GetPlayerOwner();
						if (owner != null)
						{
							if (brain != null && owner.IsSprinting)
								speed *= 1.3;
						}				
					}
                }
				double healthPercent = living.Health / (double)living.MaxHealth;
				if (healthPercent < 0.33)
				{
					speed *= 0.2 + healthPercent * (0.8 / 0.33); //33%hp=full speed 0%hp=20%speed
				}
			}

			speed = living.MaxSpeedBase * speed + 0.5; // 0.5 is to fix the rounding error when converting to int so root results in speed 2 (191*0.01=1.91+0.5=2.41)

            GameSpellEffect iConvokerEffect = SpellHandler.FindEffectOnTarget(living, "SpeedWrap");
            if (iConvokerEffect != null && speed > 191 && living.EffectList.GetOfType(typeof(ChargeEffect)) == null)
                return 191;

			if (speed < 0)
				return 0;
			return (int)speed;
		}
	}
}
