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

namespace DOL.GS.Commands
{
	[CmdAttribute("&bang", ePrivLevel.Player, "Bang on your shield", "/bang")]
	[CmdAttribute("&beckon", ePrivLevel.Player, "Makes a beckoning gesture with character's hand", "/beckon")]
	[CmdAttribute("&beg", ePrivLevel.Player, "Plea for items or money", "/beg")]
	[CmdAttribute("&blush", ePrivLevel.Player, "Act in a shy manner", "/blush")]
	[CmdAttribute("&bow", ePrivLevel.Player, "Give an honorable bow", "/bow")]
	[CmdAttribute("&charge", ePrivLevel.Player, "Onward!", "/charge")]
	[CmdAttribute("&cheer", ePrivLevel.Player, "Cheer with both hands in the air", "/cheer")]
	[CmdAttribute("&clap", ePrivLevel.Player, "Make a clapping gesture", "/clap")]
	[CmdAttribute("&cry", ePrivLevel.Player, "Sob pathetically", "/cry")]
	[CmdAttribute("&curtsey", ePrivLevel.Player, "Give a low curtsey", "/curtsey")]
	[CmdAttribute("&dance", ePrivLevel.Player, "Dance a little jig", "/dance")]
	[CmdAttribute("&dismiss", ePrivLevel.Player, "Make a dismissing gesture", "/dismiss")]
	[CmdAttribute("&flex", ePrivLevel.Player, "Makes a flexing gesture", "/flex")]
	[CmdAttribute("&hug", ePrivLevel.Player, "Makes a hugging gesture", "/hug")]
	[CmdAttribute("&induct", ePrivLevel.Player, "Make a ceremonial induction gesture", "/induct")]
	[CmdAttribute("&kiss", ePrivLevel.Player, "Blow a kiss", "/kiss")]
	[CmdAttribute("&laugh", ePrivLevel.Player, "Makes a laughing gesture", "/bang")]
	[CmdAttribute("&military", ePrivLevel.Player, "A military salute", "/military")]
	[CmdAttribute("&no", ePrivLevel.Player, "Shake your head", "/no")]
	[CmdAttribute("&point", ePrivLevel.Player, "Points to something in front of you", "/point")]
	[CmdAttribute("&ponder", ePrivLevel.Player, "For when you just want to go \"hmmmmmmm\"", "/ponder")]
	[CmdAttribute("&present", ePrivLevel.Player, "To present someone with something", "/present")]
	[CmdAttribute("&raise", ePrivLevel.Player, "Raise your hand, as in volunteering or getting attention", "/raise")]
	[CmdAttribute("&rude", ePrivLevel.Player, "A rude gesture", "/rude")]
	[CmdAttribute("&salute", ePrivLevel.Player, "Makes a stiff salute", "/salute")]
	[CmdAttribute("&shrug", ePrivLevel.Player, "Shrug your shoulders", "/shrug")]
	[CmdAttribute("&slap", ePrivLevel.Player, "Slap someone", "/slap")]
	[CmdAttribute("&slit", ePrivLevel.Player, "Let your enemy know what you want to do to him", "/slit")]
	[CmdAttribute("&surrender", ePrivLevel.Player, "I give up!", "/surrender")]
	[CmdAttribute("&taunt", ePrivLevel.Player, "A very mean gesture when you really want to insult someone", "/taunt")]
	[CmdAttribute("&victory", ePrivLevel.Player, "Make a victory cheer", "/victory")]
	[CmdAttribute("&wave", ePrivLevel.Player, "Makes a waving gesture", "/wave")]
	[CmdAttribute("&yes", ePrivLevel.Player, "Nod your head", "/yes")]
	//New
	[CmdAttribute("&sweat", ePrivLevel.Player, "sweat", "/sweat")]
	[CmdAttribute("&stagger", ePrivLevel.Player, "stagger", "/stagger")]
	[CmdAttribute("&yawn", ePrivLevel.Player, "yawn", "/yawn")]
	[CmdAttribute("&doh", ePrivLevel.Player, "doh", "/doh")]
	[CmdAttribute("&confuse", ePrivLevel.Player, "Confused", "/confuse")]
	[CmdAttribute("&shiver", ePrivLevel.Player, "shiver", "/shiver")]
	[CmdAttribute("&rofl", ePrivLevel.Player, "rofl", "/rofl")]
	[CmdAttribute("&mememe", ePrivLevel.Player, "mememe", "/mememe")]
	[CmdAttribute("&worship", ePrivLevel.Player, "worship", "/worship")]
	[CmdAttribute("&drink", ePrivLevel.Player, "drink", "/drink")]
	[CmdAttribute("&angry", ePrivLevel.Player, "Look angrily around", "/angry")]
	[CmdAttribute("&lookfar", ePrivLevel.Player, "Lets you look into the distance", "/lookfar")]
	[CmdAttribute("&smile", ePrivLevel.Player, "Make a big smile", "/smile")]
	[CmdAttribute("&stench", ePrivLevel.Player, "Wave away the local stench", "/stench")] 
	public class EmoteCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private const ushort EMOTE_RANGE_TO_TARGET = 2048; // 2064 was out of range and 2020 in range;
		private const ushort EMOTE_RANGE_TO_OTHERS = 512; // 519 was out of range and 504 in range;

		public void OnCommand(GameClient client, string[] args)
		{
			// no emotes if dead
			if (!client.Player.IsAlive)
			{
				DisplayMessage(client, "You can't do that, you're dead!");
				return;
			}

			// no emotes in combat / mez / stun
			if (client.Player.AttackState || client.Player.IsMezzed || client.Player.IsStunned)
			{
				DisplayMessage(client, "You can't do that, you're busy!");
				return;
			}

			if (client.Player.TargetObject != null)
			{
				int distanceToTarget = WorldMgr.GetDistance(client.Player, client.Player.TargetObject);

				// target not in range
				if (distanceToTarget > EMOTE_RANGE_TO_TARGET || distanceToTarget < 0)
				{
					DisplayMessage(client, "You don't see your target around here.");
					return;
				}
			}


			eEmote emoteID;
			string[] emoteMessages;

			switch (args[0])
			{
				case "&angry":
					emoteID = eEmote.Angry;
					emoteMessages = EMOTE_MESSAGES_ANGRY;
					break; 
				case "&bang":
					emoteID = eEmote.BangOnShield;
					emoteMessages = EMOTE_MESSAGES_BANG;
					break;
				case "&beckon":
					emoteID = eEmote.Beckon;
					emoteMessages = EMOTE_MESSAGES_BECKON;
					break;
				case "&beg":
					emoteID = eEmote.Beg;
					emoteMessages = EMOTE_MESSAGES_BEG;
					break;
				case "&blush":
					emoteID = eEmote.Blush;
					emoteMessages = EMOTE_MESSAGES_BLUSH;
					break;
				case "&bow":
					emoteID = eEmote.Bow;
					emoteMessages = EMOTE_MESSAGES_BOW;
					break;
				case "&charge":
					emoteID = eEmote.LetsGo;
					emoteMessages = EMOTE_MESSAGES_CHARGE;
					break;
				case "&cheer":
					emoteID = eEmote.Cheer;
					emoteMessages = EMOTE_MESSAGES_CHEER;
					break;
				case "&clap":
					emoteID = eEmote.Clap;
					emoteMessages = EMOTE_MESSAGES_CLAP;
					break;
				case "&cry":
					emoteID = eEmote.Cry;
					emoteMessages = EMOTE_MESSAGES_CRY;
					break;
				case "&curtsey":
					emoteID = eEmote.Curtsey;
					emoteMessages = EMOTE_MESSAGES_CURTSEY;
					break;
				case "&dance":
					emoteID = eEmote.Dance;
					emoteMessages = EMOTE_MESSAGES_DANCE;
					break;
				case "&dismiss":
					emoteID = eEmote.Mercy;
					emoteMessages = EMOTE_MESSAGES_DISMISS;
					break;
				case "&flex":
					emoteID = eEmote.Flex;
					emoteMessages = EMOTE_MESSAGES_FLEX;
					break;
				case "&hug":
					emoteID = eEmote.Hug;
					emoteMessages = EMOTE_MESSAGES_HUG;
					break;
				case "&induct":
					emoteID = eEmote.Induct;
					emoteMessages = EMOTE_MESSAGES_INDUCT;
					break;
				case "&lookfar":
					emoteID = eEmote.Rider_LookFar;
					emoteMessages = EMOTE_MESSAGES_RIDER_LOOKFAR;
					break; 
				case "&kiss":
					emoteID = eEmote.BlowKiss;
					emoteMessages = EMOTE_MESSAGES_KISS;
					break;
				case "&laugh":
					emoteID = eEmote.Laugh;
					emoteMessages = EMOTE_MESSAGES_LAUGH;
					break;
				case "&military":
					emoteID = eEmote.Military;
					emoteMessages = EMOTE_MESSAGES_MILITARY;
					break;
				case "&no":
					emoteID = eEmote.No;
					emoteMessages = EMOTE_MESSAGES_NO;
					break;
				case "&point":
					emoteID = eEmote.Point;
					emoteMessages = EMOTE_MESSAGES_POINT;
					break;
				case "&ponder":
					emoteID = eEmote.Ponder;
					emoteMessages = EMOTE_MESSAGES_PONDER;
					break;
				case "&present":
					emoteID = eEmote.Present;
					emoteMessages = EMOTE_MESSAGES_PRESENT;
					break;
				case "&raise":
					emoteID = eEmote.Raise;
					emoteMessages = EMOTE_MESSAGES_RAISE;
					break;
				case "&rude":
					emoteID = eEmote.Rude;
					emoteMessages = EMOTE_MESSAGES_RUDE;
					break;
				case "&salute":
					emoteID = eEmote.Salute;
					emoteMessages = EMOTE_MESSAGES_SALUTE;
					break;
				case "&shrug":
					emoteID = eEmote.Shrug;
					emoteMessages = EMOTE_MESSAGES_SHRUG;
					break;
				case "&slap":
					emoteID = eEmote.Slap;
					emoteMessages = EMOTE_MESSAGES_SLAP;
					break;
				case "&slit":
					emoteID = eEmote.Slit;
					emoteMessages = EMOTE_MESSAGES_SLIT;
					break;
				case "&smile":
					emoteID = eEmote.Smile;
					emoteMessages = EMOTE_MESSAGES_SMILE;
					break; 
				case "&stench":
					emoteID = eEmote.Rider_Stench;
					emoteMessages = EMOTE_MESSAGES_RIDER_STENCH;
					break; 
				case "&surrender":
					emoteID = eEmote.Surrender;
					emoteMessages = EMOTE_MESSAGES_SURRENDER;
					break;
				case "&taunt":
					emoteID = eEmote.Taunt;
					emoteMessages = EMOTE_MESSAGES_TAUNT;
					break;
				case "&victory":
					emoteID = eEmote.Victory;
					emoteMessages = EMOTE_MESSAGES_VICTORY;
					break;
				case "&wave":
					emoteID = eEmote.Wave;
					emoteMessages = EMOTE_MESSAGES_WAVE;
					break;
				case "&yes":
					emoteID = eEmote.Yes;
					emoteMessages = EMOTE_MESSAGES_YES;
					break;
				case "&sweat":
					emoteID = eEmote.Sweat;
					emoteMessages = EMOTE_MESSAGES_SWEAT;
					break;
				case "&stagger":
					emoteID = eEmote.Stagger;
					emoteMessages = EMOTE_MESSAGES_STAGGER;
					break;
				case "&yawn":
					emoteID = eEmote.Yawn;
					emoteMessages = EMOTE_MESSAGES_YAWN;
					break;
				case "&doh":
					emoteID = eEmote.Doh;
					emoteMessages = EMOTE_MESSAGES_DOH;
					break;
				case "&confuse":
					emoteID = eEmote.Confused;
					emoteMessages = EMOTE_MESSAGES_CONFUSE;
					break;
				case "&shiver":
					emoteID = eEmote.Shiver;
					emoteMessages = EMOTE_MESSAGES_SHIVER;
					break;
				case "&rofl":
					emoteID = eEmote.Rofl;
					emoteMessages = EMOTE_MESSAGES_ROFL;
					break;
				case "&mememe":
					emoteID = eEmote.Mememe;
					emoteMessages = EMOTE_MESSAGES_MEMEME;
					break;
				case "&worship":
					emoteID = eEmote.Worship;
					emoteMessages = EMOTE_MESSAGES_WORSHIP;
					break;
				case "&drink":
					emoteID = eEmote.Drink;
					emoteMessages = EMOTE_MESSAGES_DRINK;
					break;
				default:
					return;
			}

			SendEmote(client.Player, client.Player.TargetObject, emoteID, emoteMessages);
		}


		// send emote animation to all visible players and format messages
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


		// send emote messages to all players in range
		private void SendEmoteMessages(GamePlayer sourcePlayer, GamePlayer targetPlayer, string messageToSource, string messageToTarget, string messageToOthers)
		{
			SendEmoteMessage(sourcePlayer, messageToSource);

			if (targetPlayer != null)
				SendEmoteMessage(targetPlayer, messageToTarget);

			foreach (GamePlayer player in sourcePlayer.GetPlayersInRadius(EMOTE_RANGE_TO_OTHERS))
				if (player != sourcePlayer && player != targetPlayer) // client and target gets unique messages
					SendEmoteMessage(player, messageToOthers);

			return;
		}


		// send emote chat type message to GamePlayer
		private void SendEmoteMessage(GamePlayer player, string message)
		{
			player.Out.SendMessage(message, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
		}


		private const byte EMOTE_NOTARGET_TO_SOURCE = 0; // nothing selected; send to client
		private const byte EMOTE_NOTARGET_TO_OTHERS = 1; // nothing selected; send to others
		private const byte EMOTE_TO_SOURCE = 2; // target selected; send to client; arg0 is target.name
		private const byte EMOTE_TO_OTHERS = 3; // target selected; send to others; arg0 is client.name; arg1 is target.name

		// ("{0} bangs on his shield at {1}!", Player.Name, YOU); sent to target
		private const string YOU = "you";

		private readonly string[] EMOTE_MESSAGES_BANG = {
			"You bang on your shield!",
			"{0} bangs on his shield!",
			"You bang on your shield at {0}!",
			"{0} bangs on his shield at {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_BECKON = {
			"You make a beckoning motion.",
			"{0} beckons at nothing in particular.",
			"You beckon {0}.",
			"{0} beckons {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_BEG = {
			"You beg everyone.",
			"{0} begs everyone.",
			"You beg {0}.",
			"{0} begs {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_BLUSH = {
			"You blush.",
			"{0} blushes.",
			"You blush at {0}.",
			"{0} blushes at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_BOW = {
			"You bow.",
			"{0} bows.",
			"You bow to {0}.",
			"{0} bows to {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_CHARGE = {
			"You motion everyone onward.",
			"{0} motions everyone onward.",
			"You motion {0} onward.",
			"{0} motions {1} onward.",
		};

		private readonly string[] EMOTE_MESSAGES_CHEER = {
			"You cheer wildly!",
			"{0} cheers!",
			"You cheer at {0}!",
			"{0} cheers at {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_CLAP = {
			"You give a round of applause!",
			"{0} claps!",
			"You clap at {0}!",
			"{0} claps at {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_CRY = {
			"You begin sobbing!",
			"{0} cries!",
			"You cry at {0}!",
			"{0} cries at {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_CURTSEY = {
			"You curtsey gracefully.",
			"{0} curtseys.",
			"You curtsey to {0}.",
			"{0} curtseys to {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_DANCE = {
			"You begin dancing!",
			"{0} dances!",
			"You dance with {0}!",
			"{0} dances with {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_DISMISS = {
			"You dismiss everyone.",
			"{0} dismisses everyone.",
			"You dismiss {0}.",
			"{0} dismisses {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_FLEX = {
			"You flex!",
			"{0} flexes!",
			"You flex at {0}!",
			"{0} flexes at {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_HUG = {
			"You hug yourself.",
			"{0} hugs himself.",
			"You hug {0}.",
			"{0} hugs {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_INDUCT = {
			"You induct everyone.",
			"{0} inducts everyone.",
			"You induct {0}.",
			"{0} inducts {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_KISS = {
			"You blow a kiss!",
			"{0} blows a kiss!",
			"You blow a kiss to {0}!",
			"{0} blows a kiss to {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_LAUGH = {
			"You burst into laughter!",
			"{0} laughs!",
			"You laugh at {0}!",
			"{0} laughs at {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_MILITARY = {
			"You salute.",
			"{0} salutes.",
			"You salute {0}.",
			"{0} salutes {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_NO = {
			"You shake your head no.",
			"{0} shakes his head no.",
			"You shake your head no at {0}.",
			"{0} shakes his head no at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_POINT = {
			"You point into the distance.",
			"{0} points.",
			"You point at {0}.",
			"{0} points at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_PONDER = {
			"You ponder.",
			"{0} ponders.",
			"You ponder at {0}.",
			"{0} ponders at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_PRESENT = {
			"You present.",
			"{0} presents.",
			"You present to {0}.",
			"{0} presents to {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_RAISE = {
			"You raise your hand.",
			"{0} raises his hand.",
			"You raise your hand at {0}.",
			"{0} raises his hand at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_RUDE = {
			"You make a rude gesture.",
			"{0} makes a rude gesture.",
			"You make a rude gesture at {0}.",
			"{0} makes a rude gesture at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_SALUTE = {
			"You give a firm salute.",
			"{0} gives a firm salute.",
			"You salute {0}.",
			"{0} salutes {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_SHRUG = {
			"You shrug.",
			"{0} shrugs.",
			"You shrug at {0}.",
			"{0} shrugs at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_SLAP = {
			"You make a slapping motion.",
			"{0} makes a slapping motion.",
			"You slap {0}!",
			"{0} slaps {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_SLIT = {
			"You make a throat slit motion.",
			"{0} makes a throat slit motion.",
			"You make a throat slit motion at {0}.",
			"{0} makes a throat slit motion at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_SURRENDER = {
			"You make a surrendering gesture.",
			"{0} makes a surrendering gesture.",
			"You make a surrendering gesture at {0}.",
			"{0} makes a surrendering gesture at {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_TAUNT = {
			"You make a taunting gesture.",
			"{0} makes a taunting gesture.",
			"You taunt {0}.",
			"{0} taunts {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_VICTORY = {
			"You howl in victory!",
			"{0} howls in victory!",
			"You howl in victory to {0}!",
			"{0} howls in victory to {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_WAVE = {
			"You wave at everyone.",
			"{0} waves at everyone.",
			"You wave to {0}.",
			"{0} waves to {1}.",
		};

		private readonly string[] EMOTE_MESSAGES_YES = {
			"You nod your head yes.",
			"{0} nods his head yes.",
			"You nod your head yes at {0}.",
			"{0} nods his head yes at {1}.",
		};
		private readonly string[] EMOTE_MESSAGES_SWEAT = {
			"You break into a sweat.",
			"{0} breaks into a sweat.",
			"You break into a sweat at the sight of {0}.",
			"{0} breaks into a sweat at the sight of {1}.",
		};
		private readonly string[] EMOTE_MESSAGES_STAGGER = {
			"You stagger.",
			"{0} staggers.",
			"You stagger towards {0}.",
			"{0} staggers towards {1}.",
		};
		private readonly string[] EMOTE_MESSAGES_YAWN = {
			"You yawn.",
			"{0} yanws.",
			"You yawn at {0}.",
			"{0} yawns at {1}.",
		};
		private readonly string[] EMOTE_MESSAGES_DOH = {
			"You slap your head in confusion.",
			"{0} slaps your head in confusion.",
			"You slap your head in confusion at {0}.",
			"{0} slaps his head in confusion at {1}.",
		};
		private readonly string[] EMOTE_MESSAGES_CONFUSE = {
			"You look at yourself, clearly confused.",
			"{0} looks at you, clearly confused.",
			"You look at {0}, clearly confused.",
			"{0} looks at {1}, clearly confused.",
		};
		private readonly string[] EMOTE_MESSAGES_SHIVER = {
			"You shiver.",
			"{0} shivers.",
			"You shiver at the sight of {0}.",
			"{0} shivers at the sight of {1}.",
		};
		private readonly string[] EMOTE_MESSAGES_ROFL = {
			"You roll on the floor laughing.",
			"{0} rolls on the floor laughing at.",
			"You roll on the floor laughing at {0}.",
			"{0} rolls on the floor laughing at {1}.",
		};
		private readonly string[] EMOTE_MESSAGES_MEMEME = {
			"You beg to pick you.",
			"{0} begs to pick him.",
			"You beg {0} to pick you.",
			"{0} begs {1} to pick them.",
		};
		private readonly string[] EMOTE_MESSAGES_WORSHIP = {
			"You worship everything, and yet nothing.",
			"{0} worships nothing in particular.",
			"You worship {0}!",
			"{0} worships {1}!",
		};
		private readonly string[] EMOTE_MESSAGES_DRINK = {
			"You take a drink.",
			"{0} takes a drink.",
			"You toast {0} and take a drink.",
			"{0} toasts {1}!",
		};

		private readonly string[] EMOTE_MESSAGES_ANGRY = {
                                              "You look angrily around.",
                                              "{0} looks angrily.",
                                              "You look angrily at {0}.",
                                              "{0} looks angrily at {1}.",
      };

		private readonly string[] EMOTE_MESSAGES_RIDER_LOOKFAR = {
                                                    "You look into the distance.",
                                                    "{0} looks into the distance.",
                                                    "You look into the distance.",
                                                    "{0} looks into the distance.",
      };

		private readonly string[] EMOTE_MESSAGES_RIDER_STENCH = {
                                                   "You wave away the local stench.",
                                                   "{0} waves away the local stench.",
                                                   "You wave away the stench of {0}.",
                                                   "{0} waves away the stench of {1}.",
      };
		private readonly string[] EMOTE_MESSAGES_SMILE = {
                                               "You smile happily.",
                                               "{0} smiles happily.",
                                               "You smile at {0}.",
                                               "{0} smiles at {1}.",
      }; 
	}
}