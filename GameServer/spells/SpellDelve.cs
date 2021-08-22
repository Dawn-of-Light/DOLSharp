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
using DOL.GS.PacketHandler;
using System.Collections.Generic;

namespace DOL.GS.Spells
{
	public class SpellDelve
	{
		private ISpellHandler spellHandler;
		private Spell Spell => spellHandler.Spell;

		private int CastTime => Spell.CastTime;
		private eDamageType MagicType => Spell.DamageType;

		public SpellDelve(ISpellHandler spellHandler)
		{
			this.spellHandler = spellHandler;
		}

		public string GetClientMessage()
		{
			var clientDelve = new ClientDelve("Spell");
			clientDelve.AddElement("Function", "light");
			clientDelve.AddElement("Index", unchecked((ushort)Spell.InternalID));
			clientDelve.AddElement("Name", Spell.Name);
			if (CastTime >= 2000)
				clientDelve.AddElement("cast_timer", CastTime - 2000);
			if (CastTime == 0)
				clientDelve.AddElement("instant", "1");
			clientDelve.AddElement("damage_type", GetMagicTypeID());
			clientDelve.AddElement("level", Spell.Level);
			clientDelve.AddElement("power_cost", Spell.Power);
			clientDelve.AddElement("cost_type", GetCostTypeID());
			clientDelve.AddElement("range", Spell.Range);
			clientDelve.AddElement("duration", Spell.Duration / 1000);
			clientDelve.AddElement("dur_type", GetDurationType());
			clientDelve.AddElement("timer_value", Spell.RecastDelay / 1000);
			clientDelve.AddElement("target", GetSpellTargetType());
			clientDelve.AddElement("radius", Spell.Radius);
			clientDelve.AddElement("concentration_points", Spell.Concentration);
			clientDelve.AddElement("frequency", Spell.Frequency);

			clientDelve.AddElement("delve_string", spellHandler.ShortDescription);
			return clientDelve.ClientMessage;
		}

		private int GetSpellTargetType()
		{
			switch (Spell.Target)
			{
				case "Realm":
					return 7;
				case "Self":
					return 0;
				case "Enemy":
					return 1;
				case "Pet":
					return 6;
				case "Group":
					return 3;
				case "Area":
					return 9;
				case "Corpse":
					return 8;
				default:
					return 0;
			}
		}

		private int GetDurationType()
		{
			if (Spell.Duration > 0)
			{
				return 2;
			}
			if (Spell.Concentration > 0)
			{
				return 4;
			}

			return 0;
		}

		private int GetCostTypeID()
		{
			switch(spellHandler.CostType.ToLower())
            {
				case "health": return 2;
				case "endurance": return 3;
				default: return 0;
            }
		}

		private static Dictionary<eDamageType, int> damageTypeToIdLookup = new Dictionary<eDamageType, int>()
		{
			{eDamageType.Crush, 1 },
			{eDamageType.Slash, 2 },
			{eDamageType.Thrust, 3 },
			{eDamageType.Heat, 10 },
			{eDamageType.Spirit, 11 },
			{eDamageType.Cold, 12 },
			{eDamageType.Matter, 13 },
			{eDamageType.Body, 16 },
			{eDamageType.Energy, 20 },
		};

		private int GetMagicTypeID()
		{
			if (damageTypeToIdLookup.TryGetValue(MagicType, out int damageTypeID))
			{
				return damageTypeID;
			}
			return 0;
		}
	}
}
