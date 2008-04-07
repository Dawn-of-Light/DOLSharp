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
using DOL.GS.PacketHandler;
using DOL.GS;
using DOL.GS.Effects;

namespace DOL.GS.SkillHandler
{
    /// <summary>
    /// Handler for Sprint Ability clicks
    /// </summary>
    [SkillHandlerAttribute(Abilities.BolsteringRoar)]
    public class BolsteringRoarAbilityHandler : SpellCastingAbilityHandler
    {
		public override long Preconditions
		{
			get
			{
				return DEAD | SITTING | MEZZED | STUNNED | NOTINGROUP;
			}
		}
 		public override ushort SpellID
		{
			get
			{
				return 14376;
			}
		}  
 		public override bool CheckPreconditions(GameLiving living, long bitmask)
 		{ 			 
             lock (living.EffectList)
             {
                foreach (IGameEffect effect in living.EffectList)
                {
                    if (effect is GameSpellEffect)
                    {
                        GameSpellEffect oEffect = (GameSpellEffect)effect;
                        if (oEffect.Spell.SpellType.ToLower().IndexOf("speeddecrease") != -1 && oEffect.Spell.Value != 99)
                        {            
                        	GamePlayer player = living as GamePlayer;
                        	if(player!=null) player.Out.SendMessage("You may not use this ability while snared!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return true;
                        }
                    }
                }
            }
             return base.CheckPreconditions(living, bitmask);
 		}
    }
}
