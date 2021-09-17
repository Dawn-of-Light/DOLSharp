﻿/*
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
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using System;
using System.Collections.Generic;

namespace DOL.GS.Delve
{
    public abstract class SkillDelve
    {
        protected string DelveType { get; set; }
        protected short Index { get; set; }

        public static SkillDelve Create(GameClient client, Skill skill)
        {
            if (skill is Style) return new StyleDelve(client, skill.InternalID);
            if (skill is Song) return new SongDelve(skill.InternalID);
            if (skill is Spell spell) return new SpellDelve(spell);
            if (skill is RealmAbility) return new RealmAbilityDelve(client, skill.InternalID);
            if (skill is Ability) return new AbilityDelve(client, skill.InternalID);
            throw new ArgumentException($"{skill.GetType()} has no Delve class.");
        }

        public abstract ClientDelve GetClientDelve();

        public virtual IEnumerable<ClientDelve> GetClientDelves()
            => new List<ClientDelve>() { GetClientDelve() };

        protected ClientDelve NotFoundClientDelve
        {
            get
            {
                var clientDelve = new ClientDelve(DelveType);
                clientDelve.AddElement("Index", Index);
                clientDelve.AddElement("Name", "(not found)");
                return clientDelve;
            }
        }
    }
}
