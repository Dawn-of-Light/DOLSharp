//Create by La Grande Bataille french freeshard
//Free Scripts for DOL-France and DOLserver
using DOL.GS.PacketHandler;
using System;
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute("&horse", ePrivLevel.Player, "Horse emotes", "/horse <emote>")]
	public class HorseEmoteCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private const ushort EMOTE_RANGE_TO_TARGET = 2048;
		private const ushort EMOTE_RANGE_TO_OTHERS = 512;

		public void OnCommand(GameClient client, string[] args)
		{
			if (!client.Player.IsOnHorse)
			{
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, "HorseEmote.MustBeOnMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (args.Length < 2)
				return;

			GameObject target = client.Player.TargetObject;

			if (target != null)
			{
				if ( client.Player.IsWithinRadius( target, EMOTE_RANGE_TO_TARGET ) == false )
					target = null;
			}

			eEmote emoteID;

			switch (args[1])
			{
				case "halt":
					emoteID = eEmote.Rider_Halt;
					break;
				case "pet":
					emoteID = eEmote.Rider_pet;
					break;
				case "trick":
					emoteID = eEmote.Rider_Trick;
					break;
				case "courbette":
					emoteID = eEmote.Horse_Courbette;
					break;
				case "startle":
					emoteID = eEmote.Horse_Startle;
					break;
				case "nod":
					emoteID = eEmote.Horse_Nod;
					break;
				case "graze":
					emoteID = eEmote.Horse_Graze;
					break;
				case "rear":
					emoteID = eEmote.Horse_rear;
					break;
				default:
					return;
			}

			SendEmote(client, target, emoteID, args[1]);
		}


		private void SendEmote(GameClient client, GameObject targetObject, eEmote emoteID, string emoteType)
		{
			string messageToSource = null;
			string messageToTarget = null;
			string messageToOthers = null;

			GamePlayer sourcePlayer = client.Player;

			bool targetMatters = false;
			if (targetObject != null)
			{
				messageToSource = LanguageMgr.GetTranslation(client, string.Format("HorseEmote.{0}.ToSource", emoteType), targetObject.GetName(0, false));
				messageToOthers = LanguageMgr.GetTranslation(client, string.Format("HorseEmote.{0}.ToOthers", emoteType), sourcePlayer.Name, targetObject.GetName(0, false), sourcePlayer.GetPronoun(1, false));

				if (targetObject is GamePlayer)
					messageToTarget = LanguageMgr.GetTranslation(client, string.Format("HorseEmote.{0}.ToOthers", emoteType), sourcePlayer.Name, LanguageMgr.GetTranslation(client, "HorseEmote.You"), sourcePlayer.GetPronoun(1, false));


				if (messageToSource != "-" && messageToOthers != "-")
					targetMatters = true;
			}
			
			if (!targetMatters)
			{
				targetObject = null;
				messageToSource = LanguageMgr.GetTranslation(client, string.Format("HorseEmote.{0}.NoTargetToSource", emoteType));
				messageToOthers = LanguageMgr.GetTranslation(client, string.Format("HorseEmote.{0}.NoTargetToOthers", emoteType), sourcePlayer.Name, sourcePlayer.GetPronoun(1, false));
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
	}
}