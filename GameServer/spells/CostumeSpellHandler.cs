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
using DOL.Events;
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Costume spell handler: Shape change on self
	/// </summary>
	/// <author>Luhz</author>
	[SpellHandler("Costume")]
	class CostumeSpellHandler : SpellHandler
	{
		/// <summary>
		/// Effect starting.
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			GamePlayer player = effect.Owner as GamePlayer;

			if (player != null)
				player.Model = (ushort)effect.Spell.Value;

			GameEventMgr.AddHandler(effect.Owner, GamePlayerEvent.RegionChanged, new DOLEventHandler(OnZone));
		}

		/// <summary>
		/// Cancels the effect if the player zones out of the allowed regions.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		private void OnZone(DOLEvent e, object sender, EventArgs arguments)
		{
			if (sender is GamePlayer)
			{
				switch ((sender as GamePlayer).CurrentRegionID)
				{
					case 10:  //City of Camelot
					case 101: //Jordheim
					case 201: //Tir Na Nog

					case 2:	  //Albion Housing
					case 102: //Midgard Housing
					case 202: //Hibernia Housing
						return;	
					default: 
						GameSpellEffect costume = FindEffectOnTarget(sender as GamePlayer, this);
						if (costume != null) costume.Cancel(false);
						break;
				}
			}
		}

		/// <summary>
		/// Effect expiring (duration spells only).
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="noMessages"></param>
		/// <returns>Immunity duration in milliseconds.</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GamePlayer player = effect.Owner as GamePlayer;

			if (player != null)
				player.Model = player.CreationModel;      

			return 0;
		}

		/// <summary>
		/// Creates a new ShadesOfMist spell handler.
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="spell"></param>
		/// <param name="line"></param>
		public CostumeSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}
}
