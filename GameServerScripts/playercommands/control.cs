/*using System;
using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
    [CmdAttribute(
         "&control",
         (uint)ePrivLevel.Player,
         "control a siegeweapon",
         "/control")]

    public class ControlCommandHandler : ICommandHandler
    {
        public int OnCommand(GameClient client, string[] args)
        {
            GamePlayer player = client.Player;
            GameSiegeWeapon siege;

            if (args.Length == 1)
            {
                if (player.TargetObject == null)
                {
                    player.Out.SendMessage("You must select a siegeweapon to control.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    return 1;
                }
                else
                {
                    if (player.TargetObject is GameSiegeWeapon)
                    {
                        siege = (GameSiegeWeapon)player.TargetObject;
                        if (player.SiegeWeapon == null)
                        {
                            siege.TakeControl(player);
                            return 1;
                        }
                        else
                        {
                            player.Out.SendMessage("You allready have a siege weapon under control.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        }
                    }
                    else
                    {
                        player.Out.SendMessage("You must select a valid siegeweapon.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                    }
                }
            }

                if (player.SiegeWeapon != null)
                {
                    switch (args[1])
                    {
                        case "release":
                            player.SiegeWeapon.ReleaseControl();
                            break;
                        case "aim":
                            player.SiegeWeapon.Aim();
                            break;
                        case "arm":
                            player.SiegeWeapon.Arm();
                            break;
                        case "fire":
                            player.SiegeWeapon.Fire();
                            break;
                            //commetend out untill fixed
                        case "move":
                            player.SiegeWeapon.Move();
                            break; 
                        case "repair":
                            player.SiegeWeapon.Repair();
                            break;
                        case "salvage":
                            player.SiegeWeapon.salvage();
                            break;
                        default:
                            player.Out.SendMessage("Invalid Command", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                            break;
                    }
                }
                else
                {
                    player.Out.SendMessage("You must first take control of the siegeweapon using /control", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                }
                return 1;
        }
    }
}*/



