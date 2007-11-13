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

namespace DOL.GS
{
    /// <summary>
    /// The Atlantis arbiter.
    /// </summary>
    /// <author>Aredhel</author>
    public class Arbiter : Researcher
    {
        public Arbiter()
            : base() { }

        /// <summary>
        /// Address the arbiter.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) 
                return false;

            String intro = String.Format("Ah, {0}, welcome to what is left of Atlatis! ",
                player.CharacterClass.Name);
            intro += "What a grand find this is! ";
            intro += "We don't know why we received messages from Atlantis, but when we did, ";
            intro += "we set forth right away. We had not heard of Atlantis for years. ";
            intro += "More years than our land has a history. ";
            intro += "But we set forth, and do you know why? The [promises] of Atlantis!";

            SayTo(player, eChatLoc.CL_PopupWindow, intro);

            // TODO: This appears to be level-dependent. Get the proper message
            // for all the other cases (high enough level when starting the trials
            // high enough level and trials already started).
            
            intro = String.Format("{0}, welcome to Atlantis. When you are ready, you may begin the trials. ",
                player.Name);
            intro += "Come to me then.";
            
            SayTo(player, eChatLoc.CL_PopupWindow, intro);
            return true;
        }

        /// <summary>
		/// Talk to the arbiter.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
        public override bool WhisperReceive(GameLiving source, string text)
        {
            if (!base.WhisperReceive(source, text)) return false;
            GamePlayer player = source as GamePlayer;

            switch (text.ToLower())
            {
                case "promises" :
                    String reply = "Ah they spoke of these traits they value above all others, ";
                    reply += "and of trials that tested other cultures for those traits. ";
                    reply += "Those who pass the trials are promised greater skills in their trained arts. ";
                    reply += "We don't know how, but that is what we are studying. ";
                    reply += "See here, behind me, this stone with these glyphs? ";
                    reply += "We call it the [tablet] of destiny.";
                    SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    break;
                case "tablet":
                    reply = "Aye, this tablet and its magic, they are the only source of knowledge ";
                    reply += "we have on the trials and the planes. We're trying to translate it, ";
                    reply += "and we have found some other tablets and scrolls that tell us ";
                    reply += "a bit about this [place].";
                    SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    break;
                case "place":
                    reply = "It is the Hall of Heroes. As the Atlanteans refer to it. ";
                    reply += "It's the entrance to the Planes of Trials. Now, I can glean a bit off ";
                    reply += "of these tablets. We know that beyond this, through that portal there, ";
                    reply += "lie the Planes of the Trials. There are certain trials you must defeat ";
                    reply += "if you wish to be considered worthy of Atlantis. To fail is to die, ";
                    reply += "for the most part, though it does mention something about survivors ";
                    reply += "being sent away in disgrace. I can't tell you much about the trials, ";
                    reply += "or even how to pass them. You see, Atlantis has fallen, and of the first ";
                    reply += "ones we sent into the planes, only a few [survived].";
                    SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    break;
                case "survived":
                    reply = "Here, we are at a true disadvantage. Not only do we not know what the ";
                    reply += "original trials were supposed to be like, what was necessary for passing ";
                    reply += "them, and all the ceremonial steps involved, but it seems the ";
                    reply += "[inhabitants] of the planes have forgotten as well.";
                    SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    break;
                case "inhabitants":
                    reply = "Oh yes, the inhabitants. It seems, well, at one point, there were no ";
                    reply += "societies of creatures in the planes. That was our first mistake. ";
                    reply += "We assumed there would only be the trials. It turns out that somehow, ";
                    reply += "the creatures of the Planes, after Atlantis' fall, found the means to ";
                    reply += "survive in the planes. We think some creatures may have even escaped ";
                    reply += "to the planes, though we can't be sure. All we know is that when ";
                    reply += "Atlantis fell, it fell quickly. But, as for what lies beyond that portal, ";
                    reply += "all I can say is the trials still [exist].";
                    SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    break;
                case "exist":
                    reply = "Yes, but in some corrupted, degraded form. Passing them will involve more ";
                    reply += "than any one noble skill alone. We did not realize it was not merely ";
                    reply += "a matter of heroics, strength and skill. It seems the beasts of the planes ";
                    reply += "have developed a culture of their own, often influenced by the ancient ";
                    reply += "trials, but evolved so much over time that there really is no resemblance. ";
                    reply += "If you want the skills, the glory and the riches beyond this portal, ";
                    reply += String.Format("my dear {0}, ", player.CharacterClass.Name);
                    reply += "you are going to have to enter the planes, and be prepared for anything, ";
                    reply += "for I know too little to [guide] you.";
                    SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    break;
                case "guide":
                    reply = "I will do my best though. You will know if you pass each of the [trials]. ";
                    reply += "When you have completed a trial, return to me and I will aid you best I can. ";
                    reply += String.Format("Good luck {0}.", player.CharacterClass.Name);
                    SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    break;
                case "trials":
                    reply = "Yes, much of these planes were created for the trials. You will be rewarded ";
                    reply += "with the abilities of the ancient Atlanteans if you can complete these trials.";
                    SayTo(player, eChatLoc.CL_PopupWindow, reply);
                    break;
            }
            return true;
        }
    }
}
