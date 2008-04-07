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
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    [CmdAttribute(
       "&cast",
       ePrivLevel.GM,
         "cast a spell",
         "/cast <spellid> : cast the effect from <spellid>",
        "/cast <whatever you want> <spellid> : cast the spell <spellid>")]
    public class CastCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                client.Out.SendMessage("Usage: /cast <spellid> cast the visual effect associated with <spellid>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("Usage: /cast <whateveryouwant> <spellid> cast the spell <spellid> (/cast dol 10)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            ushort spellID = 0;
            try
            {

                if (args.Length >= 3)
                {
					ushort.TryParse(args[2], out spellID);
                    Spell spell = SkillBase.GetSpellByID(spellID);
                    SpellLine line = new SpellLine("GMCast", "GM Cast", "unknown", false);
                    GameLiving caster = client.Player;
                    GameObject obj = client.Player.TargetObject;
                    GameLiving target = null;
                    if (obj == null)
                        target = client.Player;
                    else if (obj is GameLiving)
                        target = (GameLiving)obj;
                    if (spell != null)
                    {
                        ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(caster, spell, line);
                        if (spellHandler != null)
                            spellHandler.StartSpell(target);
                    }
                }
                else
                {
					ushort.TryParse(args[1], out spellID);
                    Spell spell = SkillBase.GetSpellByID(spellID);
                    GameObject obj = client.Player.TargetObject;
                    GameLiving target = null;
                    if (obj == null)
                        target = client.Player;
                    else if (obj is GameLiving)
                        target = (GameLiving)obj;
                    foreach (GamePlayer plr in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        plr.Out.SendSpellEffectAnimation(client.Player, target, (ushort)spellID, 0, false, 1);
                }
            }
            catch
            {
                client.Out.SendMessage("Usage: /cast <spellid> cast the visual effect associated with <spellid>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                client.Out.SendMessage("Usage: /cast <whateveryouwant> <spellid> cast the spell <spellid> (/cast dol 10)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }
    }
}