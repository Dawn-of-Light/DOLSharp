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
using DOL.Language;
using DOL.GS.Effects;

namespace DOL.GS.Commands
{
    [CmdAttribute(
      "&cast",
      ePrivLevel.GM,
      "GMCommands.Cast.Description",
      "GMCommands.Cast.Usage")]
    public class CastCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            string type = args[1].ToLower();

            int id = 0;
            try
            {
                id = Convert.ToInt32(args[2]);
            }
            catch
            {
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Cast.InvalidId"));
                return;
            }
            if (id < 0)
            {
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Cast.IdNegative"));
                return;
            }

            GameLiving target = client.Player.TargetObject as GameLiving;
            if (target == null)
                target = client.Player as GameLiving;

            switch (type)
            {
                #region Effect
                case "effect":
                    {
                        DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Cast.EffectExecuted", id.ToString()));

                        DummyEffect effect = new DummyEffect((ushort)id);
                        effect.Start(client.Player);

                        foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            player.Out.SendSpellEffectAnimation(client.Player, target, (ushort)id, 0, false, 1);

                        break;
                    }
                #endregion Effect
                #region Cast
                case "cast":
                    {
                        DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Cast.CastExecuted", id.ToString()));
                        foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                            player.Out.SendSpellCastAnimation(client.Player, (ushort)id, 30);
                        break;
                    }
                #endregion Cast
                #region Spell
                case "spell":
                    {
                        Spell spell = SkillBase.GetSpellByID(id);
                        SpellLine line = new SpellLine("GMCast", "GM Cast", "unknown", false);
                        if (spell != null)
                        {
                            if ((target is GamePlayer) && (target != client.Player) && (spell.Target.ToLower() != "self"))
                            {
                                DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Cast.Spell.CastOnLiving", spell.Name, target.Name));
                                DisplayMessage(((GamePlayer)target).Client, LanguageMgr.GetTranslation(((GamePlayer)target).Client, "GMCommands.Cast.Spell.GMCastOnYou", ((client.Account.PrivLevel == 2) ? "GM" : "Admin"), client.Player.Name));
                            }
                            else if ((target == client.Player) || (spell.Target.ToLower() == "self"))
                                DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Cast.Spell.CastOnSelf", spell.Name));

                            ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(client.Player, spell, line);
                            if (spellHandler != null)
                                spellHandler.StartSpell(target);
                        }
                        else
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Cast.Spell.Inexistent", id.ToString()));
                        }
                        break;
                    }
                #endregion Spell
                #region Sound
                case "sound":
                    DisplayMessage(client,
                        LanguageMgr.GetTranslation(client, "GMCommands.Cast.SoundPlayed", id.ToString()));
                    client.Player.Out.SendSoundEffect((ushort)id, 0, 0, 0, 0, 0);
                    break;
                #endregion
                #region Default
                default:
                    {
                        DisplaySyntax(client);
                        break;
                    }
                #endregion Default
            }
        }
    }
}
