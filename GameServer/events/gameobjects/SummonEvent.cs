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
using System.Collections.Generic;
using System.Text;

namespace DOL.Events
{
    /// <summary>
    /// Event for summoning NPCs (e.g. ancient bound djinns).
    /// </summary>
    /// <author>Aredhel</author>
    public class SummonEvent : GameLivingEvent
    {
        protected SummonEvent(String name)
            : base(name) { }

        public static readonly SummonEvent SummonStarted = new SummonEvent("Summon.Started");
        public static readonly SummonEvent SummonCompleted = new SummonEvent("Summon.Completed");
        public static readonly SummonEvent BanishStarted = new SummonEvent("Banish.Started");
        public static readonly SummonEvent BanishCompleted = new SummonEvent("Banish.Completed");
    }
}
