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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.Language;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Spell handler to summon a bonedancer pet.
	/// </summary>
	/// <author>IST</author>
	[SpellHandler("SummonCommander")]
	public class SummonCommanderPet : SummonSpellHandler
	{
		public SummonCommanderPet(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (Caster is GamePlayer && ((GamePlayer)Caster).ControlledBrain != null)
			{
                MessageToCaster(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SummonCommanderPet.CheckBeginCast.Text"), eChatType.CT_SpellResisted);
                return false;
            }
			return base.CheckBeginCast(selectedTarget);
		}

		protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
			if (!(sender is CommanderPet))
				return;

			CommanderPet pet = sender as CommanderPet;

			if (pet.ControlledNpcList != null)
			{
				foreach (BDPetBrain cnpc in pet.ControlledNpcList)
				{
					if (cnpc != null)
						GameEventMgr.Notify(GameLivingEvent.PetReleased, cnpc.Body);
				}
			}
			base.OnNpcReleaseCommand(e, sender, arguments);
		}

		protected override IControlledBrain GetPetBrain(GameLiving owner)
		{
			return new CommanderBrain(owner);
		}

		protected override GamePet GetGamePet(INpcTemplate template)
		{
			return new CommanderPet(template);
		}

		/// <summary>
		/// Delve info string.
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
                var delve = new List<string>();
                delve.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SummonCommanderPet.DelveInfo.Text1"));
                delve.Add("");
                delve.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SummonCommanderPet.DelveInfo.Text2"));
                delve.Add("");
                delve.Add(String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SummonCommanderPet.DelveInfo.Text3", Spell.Target)));
                delve.Add(String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SummonCommanderPet.DelveInfo.Text4", Math.Abs(Spell.Power))));
                delve.Add(String.Format(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "SummonCommanderPet.DelveInfo.Text5", (Spell.CastTime / 1000).ToString("0.0## Sekunden"))));
                return delve;
            }
		}
	}
}
