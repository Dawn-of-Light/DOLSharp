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
using DOL.GS.PacketHandler;
namespace DOL.GS.Spells
{
	/// Costume spell handler: Shape change on self
	/// <author>Luhz</author>
	[SpellHandler("Costume")]
	class CostumeSpellHandler : SpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget, bool quiet)
		{
			GameLiving player = selectedTarget as GamePlayer;

			if (player == null)
				return false;

			switch (player.CurrentRegionID)
			{
				case 10: return true;// Camelot City
				case 101: return true; // Jordheim
				case 201: return true; // Tir Na Nog
				case 2: return true; // Alb Housing
				case 102: return true; // Mid Housing
				case 202: return true; // Hib Housing
				default: return false;
			}
		}

		/// Effect starting.		
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			GamePlayer player = effect.Owner as GamePlayer;

			if (player != null)
			{
				if (CheckBeginCast(player, true) == false)
				{
					if (effect != null)
						effect.Cancel(true);
					player.Model = player.CreationModel;
					return;
				}
				else
				{
					player.Model = (ushort)effect.Spell.Value;
				}
				GameEventMgr.AddHandler(effect.Owner, GamePlayerEvent.RegionChanged, new DOLEventHandler(OnZone));
			}
		}

		private void OnZone(DOLEvent e, object sender, EventArgs arguments)
		{
			if (sender is GamePlayer)
			{
				switch ((sender as GamePlayer).CurrentRegionID)
				{
					case 10: // Camelot City 
					case 101: //Jordheim
					case 201: //Tir Na Nog
					case 2:	  //Albion Housing
					case 102: //Midgard Housing
					case 202: //Hibernia Housing                        
					default:
						GameSpellEffect costume = FindEffectOnTarget(sender as GamePlayer, this);
						if (costume != null)
							costume.Cancel(true);
						return;
				}
			}
		}

		/// Effect expiring (duration spells only).		
		/// <returns>Immunity duration in milliseconds.</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GamePlayer player = effect.Owner as GamePlayer;

			if (player != null)
			{
				effect.Cancel(false);
				player.Model = player.CreationModel;

				GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.RegionChanged, new DOLEventHandler(OnZone));
			}
			return 0;

		}

		/// Creates a new Costume spell handler.		
		public CostumeSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}
}
