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
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Commands
{
	/// <summary>
	/// Providing some basic command handler functionality
	/// </summary>
	public abstract class AbstractCommandHandler
	{
		public virtual void DisplayMessage(GamePlayer player, string message)
		{
			DisplayMessage(player.Client, message, new object[] { });
		}


		public virtual void DisplayMessage(GameClient client, string message)
		{
			DisplayMessage(client, message, new object[] { });
		}


		public virtual void DisplayMessage(GameClient client, string message, params object[] objs)
		{
			if (client == null || !client.IsPlaying)
				return;
			client.Out.SendMessage(String.Format(message, objs), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return;
		}


		public virtual void DisplaySyntax(GameClient client)
		{
			if (client == null || !client.IsPlaying)
				return;

			CmdAttribute[] attrib = (CmdAttribute[])this.GetType().GetCustomAttributes(typeof(CmdAttribute), false);
			if (attrib.Length == 0)
				return;

			client.Out.SendMessage(LanguageMgr.GetTranslation(client, attrib[0].Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			foreach (string sentence in attrib[0].Usage)
				client.Out.SendMessage(LanguageMgr.GetTranslation(client, sentence), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return;
		}


        public virtual void DisplaySyntax( GameClient client, string subcommand )
        {
            if ( client == null || !client.IsPlaying )
                return;

            CmdAttribute[] attrib = (CmdAttribute[])this.GetType().GetCustomAttributes( typeof( CmdAttribute ), false );

            if ( attrib.Length == 0 )
                return;

            foreach ( string sentence in attrib[0].Usage )
            {
                string[] words = sentence.Split( new char[] { ' ' }, 2 );

                if ( words.Length >= 2 && words[1].Equals( subcommand ) )
                {
                    client.Out.SendMessage( LanguageMgr.GetTranslation( client, sentence ), eChatType.CT_System, eChatLoc.CL_SystemWindow );
                }
            }

            return;
        }


        public virtual void DisplaySyntax( GameClient client, string subcommand1, string subcommand2 )
        {
            if ( client == null || !client.IsPlaying )
                return;

            CmdAttribute[] attrib = (CmdAttribute[])this.GetType().GetCustomAttributes( typeof( CmdAttribute ), false );

            if ( attrib.Length == 0 )
                return;

            foreach ( string sentence in attrib[0].Usage )
            {
                string[] words = sentence.Split( new char[] { ' ' }, 3 );

                if ( words.Length >= 3 && words[1].Equals( subcommand1 ) && words[2].Equals( subcommand2 ) )
                {
                    client.Out.SendMessage( LanguageMgr.GetTranslation( client, sentence ), eChatType.CT_System, eChatLoc.CL_SystemWindow );
                }
            }

            return;

        }
	}
}
