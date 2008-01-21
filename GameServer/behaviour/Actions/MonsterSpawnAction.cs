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
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.Behaviour.Attributes;using DOL.GS.Behaviour;
using DOL.Database2;

namespace DOL.GS.Behaviour.Actions
{
    // NOTE it is important that we look into the database for the npc because since it's not spawn at the moment the WorldMgr cant find it!!!
    [ActionAttribute(ActionType = eActionType.MonsterSpawn,DefaultValueP=eDefaultValueConstants.NPC)]
    public class MonsterSpawnAction : AbstractAction<GameLiving,Unused>
    {               

        public MonsterSpawnAction(GameNPC defaultNPC,  Object p, Object q)
            : base(defaultNPC, eActionType.MonsterSpawn, p, q)
        {                
        }


        public MonsterSpawnAction(GameNPC defaultNPC,  GameLiving npcToSpawn)
            : this(defaultNPC,  (object)npcToSpawn, (object)null) { }

        public override void Perform(DOLEvent e, object sender, EventArgs args)
        {            
            if (P.AddToWorld())
            {
                // appear with a big buff of magic
                foreach (GamePlayer visPlayer in P.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    visPlayer.Out.SendSpellCastAnimation(P, 1, 20);
                }
                
            }
        }
    }
}
