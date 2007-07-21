//Create by La Grande Bataille french freeshard
//Free Scripts for DOL-France and DOLserver
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&horse", (uint)ePrivLevel.Player, "Emotes pour les chevaux", "/horse <emote>")]
	public class HorseEmoteCommandHandler : ICommandHandler
	{
		private const ushort EMOTE_RANGE_TO_TARGET = 2048;
		private const ushort EMOTE_RANGE_TO_OTHERS = 512;

		public int OnCommand(GameClient client, string[] args)
		{
			if (!client.Player.IsOnHorse)
			{
				client.Out.SendMessage("Vous devez chevaucher votre monture pour utiliser cette commande!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			if (client.Player.TargetObject != null)
			{
				int distanceToTarget = WorldMgr.GetDistance((GameObject)client.Player, (GameObject)client.Player.TargetObject);

				// la cible n'est pas à portée
				if (distanceToTarget > EMOTE_RANGE_TO_TARGET || distanceToTarget < 0)
				{
					client.Out.SendMessage("Vous ne voyez pas votre cible aux alentours.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
			}

			if (args.Length < 2)
			{
				Usage(client);
				return 0;
			}

			eEmote emoteID;
			string[] emoteMessages;


			switch (args[1])
			{
				case "halt":
					emoteID = eEmote.Rider_Halt;
					emoteMessages = EMOTE_MESSAGES_HALT;
					break;
				case "pet":
					emoteID = eEmote.Rider_pet;
					emoteMessages = EMOTE_MESSAGES_PET;
					break;
				case "trick":
					emoteID = eEmote.Rider_Trick;
					emoteMessages = EMOTE_MESSAGES_TRICK;
					break;
				case "courbette":
					emoteID = eEmote.Horse_Courbette;
					emoteMessages = EMOTE_MESSAGES_COURBETTE;
					break;
				case "startle":
					emoteID = eEmote.Horse_Startle;
					emoteMessages = EMOTE_MESSAGES_STARTLE;
					break;
				case "nod":
					emoteID = eEmote.Horse_Nod;
					emoteMessages = EMOTE_MESSAGES_NOD;
					break;
				case "graze":
					emoteID = eEmote.Horse_Graze;
					emoteMessages = EMOTE_MESSAGES_GRAZE;
					break;
				case "rear":
					emoteID = eEmote.Horse_rear;
					emoteMessages = EMOTE_MESSAGES_REAR;
					break;
				default:
					return 0;
			}

			SendEmote(client.Player, client.Player.TargetObject, emoteID, emoteMessages);
			return 1;
		}


		private void SendEmote(GamePlayer sourcePlayer, GameObject targetObject, eEmote emoteID, string[] emoteMessages)
		{
			string messageToSource = null;
			string messageToTarget = null;
			string messageToOthers = null;

			if (targetObject == null)
			{
				messageToSource = emoteMessages[EMOTE_NOTARGET_TO_SOURCE];
				messageToOthers = string.Format(emoteMessages[EMOTE_NOTARGET_TO_OTHERS], sourcePlayer.Name);
			}
			else
			{
				messageToSource = string.Format(emoteMessages[EMOTE_TO_SOURCE], targetObject.GetName(0, false));
				messageToOthers = string.Format(emoteMessages[EMOTE_TO_OTHERS], sourcePlayer.Name, targetObject.GetName(0, false));

				if (targetObject is GamePlayer)
					messageToTarget = string.Format(emoteMessages[EMOTE_TO_OTHERS], sourcePlayer.Name, YOU);
			}

			foreach (GamePlayer player in sourcePlayer.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendEmoteAnimation(sourcePlayer, emoteID);

			SendEmoteMessages(sourcePlayer, targetObject as GamePlayer, messageToSource, messageToTarget, messageToOthers);

			return;
		}


		private void SendEmoteMessages(GamePlayer sourcePlayer, GamePlayer targetPlayer, string messageToSource, string messageToTarget, string messageToOthers)
		{
			SendEmoteMessage(sourcePlayer, messageToSource);

			if (targetPlayer != null)
				SendEmoteMessage(targetPlayer, messageToTarget);

			foreach (GamePlayer player in sourcePlayer.GetPlayersInRadius(EMOTE_RANGE_TO_OTHERS))
				if (player != sourcePlayer && player != targetPlayer)
					SendEmoteMessage(player, messageToOthers);

			return;
		}


		private void SendEmoteMessage(GamePlayer player, string message)
		{
			player.Out.SendMessage(message, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
		}

		private const byte EMOTE_NOTARGET_TO_SOURCE = 0;
		private const byte EMOTE_NOTARGET_TO_OTHERS = 1;
		private const byte EMOTE_TO_SOURCE = 2;
		private const byte EMOTE_TO_OTHERS = 3;
		private const string YOU = "vous";

		private readonly string[] EMOTE_MESSAGES_HALT = {
         "Vous faites signe à tout le monde de s'arrêter.",
         "{0} fait signe à tout le monde de s'arrêter.",
         "Vous faites signe à {0} de s'arrêter.",
         "{0} fait signe à {1} de s'arrêter.",
      };
		private readonly string[] EMOTE_MESSAGES_PET = {
         "Vous caressez votre monture.",
         "{0} caresse sa monture.",
         "",
         "",
      };
		private readonly string[] EMOTE_MESSAGES_TRICK = {
         "Vous vous tenez debout sur votre monture pour impressionner l'assistance.",
         "{0} se tient debout sur sa monture pour impressionner l'assistance.",
         "Vous vous tenez debout sur votre monture pour impressionner {0}.",
         "{0} se tient debout sur sa monture pour impressionner {1}!",
      };
		private readonly string[] EMOTE_MESSAGES_COURBETTE = {
         "Votre monture effectue une courbette.",
         "La monture de {0} effectue une courbette.",
         "",
         "",
      };
		private readonly string[] EMOTE_MESSAGES_STARTLE = {
         "Votre monture a un mouvement de crainte.",
         "La monture de {0} a un mouvement de crainte.",
         "Votre monture a un mouvement de crainte face à {0}.",
         "La monture de {0} a un mouvement de crainte face à {1}!",
      };
		private readonly string[] EMOTE_MESSAGES_NOD = {
         "Votre monture incline la tête.",
         "La monture de {0} incline la tête.",
         "Votre monture incline la tête devant {0}.",
         "La monture de {0} incline la tête devant {1}!",
      };
		private readonly string[] EMOTE_MESSAGES_GRAZE = {
         "Vous laissez votre monture brouter.",
         "{0} laisse sa monture brouter.",
         "",
         "",
      };
		private readonly string[] EMOTE_MESSAGES_REAR = {
         "Votre monture se cabre.",
         "La monture de {0} se cabre.",
         "Votre monture se cabre face à {0}.",
         "La monture de {0} se cabre face à {1}!",
      };

		public static void Usage(GameClient client)
		{
			client.Out.SendMessage("Usage de la commande /horse :", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			client.Out.SendMessage("/horse <courbette>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}