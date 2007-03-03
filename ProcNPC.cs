//Dawn of Light Version 1.7.48
//12/13/2004
//Written by Gavinius
//based on Nardin and Zjovaz previous script
//08/18/2005
//by sirru
//completely rewrote SetEffect, no duel item things whatsoever left
//compatible with dol 1.7 and added some nice things like more 
//smalltalk, equip, emotes, changing of itemnames and spellcast at the end of process
//plus i added changing of speed and color
//what could be done is trimming the prefixes from the name instead of looking at the db, but i dont know how to do that :)

using System;
using DOL;
using DOL.GS;
using DOL.Events;
using DOL.Database;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[NPCGuildScript("Proc NPC")]
	public class ProcNPC : GameNPC
	{
		private string PROCNPC_ITEM_WEAK = "DOL.GS.Scripts.ProcNPC_Item_Manipulation";//used to store the item in the player

		public override bool Interact(GamePlayer player)
		{
			if(base.Interact(player))
			{
				TurnTo(player,250);
				SendReply(player, 	"Greetings Adventurer!\n\n"+
				          			"I add a proc to weapons or armor for you!...\n"+
				          			"All I want is your soul, if you don't have one then I'll also accept 200bp.\n\n"+
				          			"Simply give me the item and i will start my work.");
				return true;
			}
			return false;
		}

		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			GamePlayer t = source as GamePlayer;
			if(t == null || item == null) return false;
			if(WorldMgr.GetDistance(this,t) > WorldMgr.INTERACT_DISTANCE)
			{
				t.Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if(t.BountyPoints < 200)
			{
				t.Out.SendMessage("You need more bounty points to use this service!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (item.Object_Type > 1 &&  item.Object_Type < 27) //Item is a weapon
				SendReply(t, "Very good, select the type of proc you would like to add:\n"+
								"[Direct Damage]\n"+
								"[Damage over Time]\n"+
								"[Damage Add]\n"+
								"[Lifetap]\n"+
								"[Haste]\n"+
								"[Str/Con Debuff]\n"+
								"[Dex/Quick Debuff]\n");
			else if ((item.Object_Type > 31 && item.Object_Type < 39) || item.Object_Type == 42) //Item is armor or a shield.
				SendReply(t, "Very good, select the type of proc you would like to add:\n"+
								"[Direct Damage]\n"+
								"[Damage over Time]\n"+
								"[Damage Add]\n"+
								"[Lifetap]\n"+
								"[Haste]\n"+
								"[Alblative]\n"+
								"[Armor Factor Buff]\n"+
								"[Damage Shield]\n");
			t.TempProperties.setProperty(PROCNPC_ITEM_WEAK, new WeakReference(item));
			return false;
		}
		
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if(!base.WhisperReceive(source,str)) return false;

			if(!(source is GamePlayer)) return false;
		
			GamePlayer player = (GamePlayer)source;

			TurnTo(player.X,player.Y);

			switch(str)
			{
				case "Direct Damage":
					SendReply(player, 
					        	"Select a damage type : \n" +
								"[Heat DD]\n" +
								"[Spirit DD]\n" +
								"[Cold DD]\n" +
								"[Energy DD]\n");
					break;
				case "Heat DD":
					SetProc(player, 32124); break;					
				case "Spirit DD":
					SetProc(player, 32127); break;
				case "Cold DD":
					SetProc(player, 32125); break;
				case "Energy DD":
					SetProc(player, 32126); break;
				case "Damage over Time":
					SendReply(player, 
					        	"Select a damage type : \n" +
								"[Heat DoT]\n" +
								"[Matter DoT]\n" +
								"[Energy DoT]\n");
					break;
				case "Heat DoT":
					SetProc(player, 32006); break;
				case "Matter DoT":
					SetProc(player, 32013); break;
				case "Energy DoT":
					SetProc(player, 32012); break;


				case "Damage Add":
					SetProc(player, 32200); break;
				case "Lifetap":
					SetProc(player, 32210); break;
				case "Haste":
					SetProc(player, 32170); break;
				case "Str/Con Debuff":
					SetProc(player, 32220); break;
				case "Dex/Quick Debuff":
					SetProc(player, 32230); break;
				case "Alblative":
					SetProc(player, 32151); break;
				case "Armor Factor Buff":
					SetProc(player, 32161); break;
				case "Damage Shield":
					SetProc(player, 32180); break;
			}

			return true;
		}

		public void SendReply(GamePlayer player, string msg)
		{
			player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
		}
		public void SetProc(GamePlayer player, int proc)
		{
			WeakReference itemWeak = (WeakReference) player.TempProperties.getObjectProperty(PROCNPC_ITEM_WEAK, new WeakRef(null));
			
			player.TempProperties.removeProperty(PROCNPC_ITEM_WEAK);
			
			InventoryItem item = (InventoryItem) itemWeak.Target;

			item.ProcSpellID = proc;
			player.RemoveBountyPoints(200);
			player.Out.SendInventoryItemsUpdate(new InventoryItem[] {item});
			SendReply(player, "My work upon "+item.Name+" is complete. Farewell adventurer.");
		}
	}
}


