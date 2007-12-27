using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.ServerProperties;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    [CmdAttribute(
    "&centeranno",
       new string[] { "&center" },
      ePrivLevel.GM,
       "Dire quelquechose qui s'affichant au centre de la fenetre de jeu.",
       "/centeranno <message>")]
    public class CenterannoCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                client.Out.SendMessage("You must Centeranno something...", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
            }

            string message = string.Join(" ", args, 1, args.Length - 1);

            Centeranno(client.Player.Name, message);
        }

        private void Centeranno(string name, string message)
        {
            foreach (GameClient thisClient in WorldMgr.GetAllPlayingClients())
            {
                thisClient.Player.Out.SendMessage( "" + message, eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
            }
        }
    }
}
