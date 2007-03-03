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
	[NPCGuildScript("Effect Master")]
	public class EffectNPC : GameNPC
	{
		private string EFFECTNPC_ITEM_WEAK = "DOL.GS.Scripts.EffectNPC_Item_Manipulation";//used to store the item in the player
		private string Prefix0 = "Purified";//Prefix for Effect Remover
		private string Prefix1 = "Glowing ";//25% Chance for each Prefix when effect is applied
		private string Prefix2 = "Sparkling ";//25% Chance for each Prefix when effect is applied
		private string Prefix3 = "Flaming ";//25% Chance for each Prefix when effect is applied
		private string Prefix4 = "Arcing ";//25% Chance for each Prefix when effect is applied
		private string Prefix5 = "Polished ";//Prefix for Speed change
		private string Prefix6 = "Dyed ";//Prefix for dyeing
		private ushort spell = 7215;//The spell which is casted
		private ushort duration = 3000;//3s, the duration the spell is cast
		private int Chance;//Chance for Prefixes
		private Random rnd = new Random();//Randomness 4 teh win
		private Queue m_timer = new Queue();//Gametimer for casting some spell at the end of the process
		private Queue castplayer = new Queue();//Used to hold the player who the spell gets cast on
		
		public override bool AddToWorld()
		{
			if(this.CurrentRegionID >= 1 || this.CurrentRegionID <= 800)
			{
				Name = "Master of Effects";
				Model = 52;
				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				template.AddNPCEquipment(eInventorySlot.Cloak,1720);
				template.AddNPCEquipment(eInventorySlot.TorsoArmor,2871);
				template.AddNPCEquipment(eInventorySlot.LegsArmor,2872);
				template.AddNPCEquipment(eInventorySlot.ArmsArmor,2873);
				template.AddNPCEquipment(eInventorySlot.HandsArmor,2876);
				template.AddNPCEquipment(eInventorySlot.FeetArmor,2875);
				template.AddNPCEquipment(eInventorySlot.TwoHandWeapon,649,72);
				Inventory = template.CloseTemplate();
                
			}
			GuildName = "Effect Master";
			Level = 50;
			SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			base.AddToWorld();
			return true;
		}
		
		public override bool Interact(GamePlayer player)
		{
			if(base.Interact(player))
			{
				SendReply(player, 	"Greetings Traveller!\n\n"+
				          			"I can change the effect or the color of your weapons and armors...\n"+
				          			"Simply give me the item and i will start my work.");
				          			//"On my countless journeys, i have mastered the art of"+ Didnt like the amount of talking
				          			//"focusing the etheral flows to a certain weapon.\n"+  so i slimmed it a  bit o.O
				          			//"Using this technique, i can make your weapon glow in"+
				          			//"every kind and color you can imagine.\n"+
				          			//"Just hand me the weapon and pay a small donation of "+PriceString+".");
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

			SendReply(t, "What service do you want to use ?\n"+
			          	"Shall I change the [effect] or [speed] of your weapon,"+
			          	"or do you want me to [dye] it ?.");
			t.TempProperties.setProperty(EFFECTNPC_ITEM_WEAK, new WeakReference(item));
			return false;
		}
		
		public override bool WhisperReceive(GameLiving source, string str)
		{
			bool speed = false;
			//coming soon bool color = false;
			
			if(!base.WhisperReceive(source,str)) return false;

			if(!(source is GamePlayer)) return false;
		
			GamePlayer player = (GamePlayer)source;

			TurnTo(player.X,player.Y);

			switch(str)
			{
				#region effects
				case "effect":
					SendReply(player,"Select a category of effect :\n" +
								"[Effect Remover]\n"+
								"[Longsword]\n"+
								"[2 hand/gr sword]\n"+
								"[2 hand/gr]\n" +
								"[1 hand hammer]\n" +
								"[1 hand crush]\n" + 
								"[1 hand axe]\n" +
								"[Shortsword]\n" + 
								"[BattleSpear]\n" + 
								"[Lugged spear]\n" + 
								"[Staff]\n");
				break;
				//Case EffectRemover
				case "Effect Remover":	SetEffect(player, 0); break;
				//Case Longsword
				case "Longsword":
					SendReply(player, 
						        "Choose a weapon effect : \n" +
								"[longsword - propane-style flame]\n" +
								"[longsword - regular flame]\n" +
								"[longsword - orange flame]\n" +
								"[longsword - rising flame]\n" +
								"[longsword - flame with smoke]\n" +
								"[longsword - flame with sparks]\n" +
								"[longsword - hot glow]\n" +
								"[longsword - hot aura]\n" +
								"[longsword - blue aura]\n" +
								"[longsword - hot blue glow]\n" +
								"[longsword - hot gold glow]\n" +
								"[longsword - hot red glow]\n" +
								"[longsword - red aura]\n" +
								"[longsword - cold aura with sparkles]\n" +
								"[longsword - cold aura with vapor]\n" +
								"[longsword - hilt wavering blue beam]\n" +
								"[longsword - hilt wavering green beam]\n" +
								"[longsword - hilt wavering red beam]\n" +
								"[longsword - hilt red/blue beam]\n" +
								"[longsword - hilt purple beam]\n");
				break;
				//SetEffect
				case "longsword - propane-style flame": SetEffect(player, 1); break;
				case "longsword - regular flame": SetEffect(player, 2); break;
				case "longsword - orange flame": SetEffect(player, 3); break;
				case "longsword - rising flame": SetEffect(player, 4); break;
				case "longsword - flame with smoke": SetEffect(player, 5); break;
				case "longsword - flame with sparks": SetEffect(player, 6); break;
				case "longsword - hot glow": SetEffect(player, 7); break;
				case "longsword - hot aura": SetEffect(player, 8); break;
				case "longsword - blue aura": SetEffect(player, 9); break;
				case "longsword - hot gold glow": SetEffect(player, 10); break;
				case "longsword - hot blue glow": SetEffect(player, 11); break;
				case "longsword - hot red glow": SetEffect(player, 12); break;
				case "longsword - red aura": SetEffect(player, 13); break;
				case "longsword - cold aura with sparkles": SetEffect(player, 14); break;
				case "longsword - cold aura with vapor": SetEffect(player, 15); break;
				case "longsword - hilt wavering blue beam": SetEffect(player, 16); break;
				case "longsword - hilt wavering green beam": SetEffect(player, 17); break;
				case "longsword - hilt wavering red beam": SetEffect(player, 18); break;
				case "longsword - hilt red/blue beam": SetEffect(player, 19); break;
				case "longsword - hilt purple beam": SetEffect(player, 20); break;
				//Case 2hand/gr Sword
				case "2 hand/gr sword":
					SendReply(player, 
						         "Choose a weapon effect : \n" +
								"[gr sword - yellow flames]\n" +
								"[gr sword - orange flames]\n" +
								"[gr sword - fire with smoke]\n" +
								"[gr sword - fire with sparks]\n");
				break;
				//SetEffect
				case "gr sword - yellow flames": SetEffect(player, 21); break;
				case "gr sword - orange flames": SetEffect(player, 22); break;
				case "gr sword - fire with smoke": SetEffect(player, 23); break;
				case "gr sword - fire with sparks": SetEffect(player, 24); break;
				//Case 2hand/gr
				case "2 hand/gr":
					SendReply(player, 
					        	"Choose a weapon effect : \n" +
								"[gr - blue glow with sparkles]\n" +
								"[gr - blue aura with cold vapor]\n" +
								"[gr - icy blue glow]\n" +
								"[gr - red aura]\n" +
								"[gr - strong crimson glow]\n" +
								"[gr - white core red glow]\n" +
								"[gr - silvery/white glow]\n" +
								"[gr - gold/yellow glow]\n" +
								"[gr - hot green glow]\n");
				break;
				//2hand/gr SetEffect
				case "gr - blue glow with sparkles": SetEffect(player, 25); break;
				case "gr - blue aura with cold vapor": SetEffect(player, 26); break;
				case "gr - icy blue glow": SetEffect(player, 27); break;
				case "gr - red aura": SetEffect(player, 28); break;
				case "gr - strong crimson glow": SetEffect(player, 29); break;
				case "gr - white core red glow": SetEffect(player, 30); break;
				case "gr - silvery/white glow": SetEffect(player, 31); break;
				case "gr - gold/yellow glow": SetEffect(player, 31); break;
				case "gr - hot green glow": SetEffect(player, 33); break;
				//Case 1h Hammer
				case "1 hand hammer":
					SendReply(player, 
					         	 "Choose a weapon effect : \n" +
								"[hammer - red aura]\n" +
								"[hammer - fiery glow]\n" +
								"[hammer - more intense fiery glow]\n" +
								"[hammer - flaming]\n" +
								"[hammer - torchlike flaming]\n" +
								"[hammer - silvery glow]\n" +
								"[hammer - purple glow]\n" +
								"[hammer - blue aura]\n" +
								"[hammer - blue glow]\n" +
								"[hammer - arcs from head to handle]\n");
				break;
				//1h Hammer SetEffect
				case "hammer - red aura": SetEffect(player, 34); break;
				case "hammer - fiery glow": SetEffect(player, 35); break;
				case "hammer - more intense fiery glow": SetEffect(player, 36); break;
				case "hammer - flaming": SetEffect(player, 37); break;
				case "hammer - torchlike flaming": SetEffect(player, 38); break;
				case "hammer - silvery glow": SetEffect(player, 39); break;
				case "hammer - purple glow": SetEffect(player, 40); break;
				case "hammer - blue aura": SetEffect(player, 41); break;
				case "hammer - blue glow": SetEffect(player, 42); break;
				case "hammer - arcs from head to handle": SetEffect(player, 43); break;
				//Case 1hand Crush
				case "1 hand crush":
					SendReply(player, 
					          	"Choose a weapon effect : \n" +
								"[crush - arcing halo]\n" +
								"[crush - center arcing]\n" +
								"[crush - smaller arcing halo]\n" +
								"[crush - hot orange core glow]\n" +
								"[crush - orange aura]\n" +
								"[crush - subtle aura with sparks]\n" +
								"[crush - yellow flame]\n" +
								"[crush - mana flame]\n" +
								"[crush - hot green glow]\n" +
								"[crush - hot red glow]\n" + 
								"[crush - hot purple glow]\n" +
								"[crush - cold vapor]\n");
				break;
				//1h Crush SetEffect
				case "crush - arcing halo": SetEffect(player, 44); break;
				case "crush - center arcing": SetEffect(player, 45); break;
				case "crush - smaller arcing halo": SetEffect(player, 46); break;
				case "crush - hot orange core glow": SetEffect(player, 47); break;
				case "crush - orange aura": SetEffect(player, 48); break;
				case "crush - subtle aura with sparks": SetEffect(player, 49); break;
				case "crush - yellow flame": SetEffect(player, 50); break;
				case "crush - mana flame": SetEffect(player, 51); break;
				case "crush - hot green glow": SetEffect(player, 52); break;
				case "crush - hot red glow": SetEffect(player, 53); break;
				case "crush - hot purple glow": SetEffect(player, 54); break;
				case "crush - cold vapor": SetEffect(player, 55); break;
				//Case 1hand Axe
				case "1 hand axe":
					SendReply(player, 
					          	"Choose a weapon effect : \n" +
								"[axe - basic flame]\n" +
								"[axe - orange flame]\n" +
								"[axe - slow orange flame with sparks]\n" +
								"[axe - fiery/trailing flame]\n" +
								"[axe - cold vapor]\n" +
								"[axe - blue aura with twinkles]\n" +
								"[axe - hot green glow]\n" +
								"[axe - hot blue glow]\n" +
								"[axe - hot cyan glow\n" +
								"[axe - hot purple glow]\n" + 
								"[axe - blue->purple->orange glow]\n");
				break;
				//Axe SetEffect
				case "axe - basic flame": SetEffect(player, 56); break;
				case "axe - orange flame": SetEffect(player, 57); break;
				case "axe - slow orange flame with sparks": SetEffect(player, 58); break;
				case "axe - fiery/trailing flame": SetEffect(player, 59); break;
				case "axe - cold vapor": SetEffect(player, 60); break;
				case "axe - blue aura with twinkles": SetEffect(player, 61); break;
				case "axe - hot green glow": SetEffect(player, 62); break;
				case "axe - hot blue glow": SetEffect(player, 63); break;
				case "axe - hot cyan glow": SetEffect(player, 64); break;
				case "axe - hot purple glow": SetEffect(player, 65); break;
				case "axe - blue->purple->orange glow": SetEffect(player, 66); break;
				//Case Shortsword
				case "Shortsword":
					SendReply(player,
					          	"Choose a weapon effect : \n" +
								"[shortsword - propane flame]\n" +
								"[shortsword - orange flame with sparks]\n" +
								"[shortsword - blue aura with twinkles]\n" +
								"[shortsword - green cloud with bubbles]\n" +
								"[shortsword - red aura with blood bubbles]\n" +
								"[shortsword - evil green glow]\n" +
								"[shortsword - black glow]\n");
				break;
				//Shortsword SetEffect
				case "shortsword - propane flame": SetEffect(player, 67); break;
				case "shortsword - orange flame with sparks": SetEffect(player, 68); break;
				case "shortsword - blue aura with twinkles": SetEffect(player, 69); break;
				case "shortsword - green cloud with bubbles": SetEffect(player, 70); break;
				case "shortsword - red aura with blood bubbles": SetEffect(player, 71); break;
				case "shortsword - evil green glow": SetEffect(player, 72); break;
				case "shortsword - black glow": SetEffect(player, 73); break;
				//Case BattleSpear
				case "BattleSpear":
					SendReply(player, 
							    "Choose a weapon effect : \n" +
								"[battlespear - cold with twinkles]\n" +
								"[battlespear - evil green aura]\n" +
								"[battlespear - evil red aura]\n" +
								"[battlespear - flaming]\n" +
								"[battlespear - hot gold glow]\n" +
								"[battlespear - hot fire glow]\n" +
								"[battlespear - red aura]\n");
				break;
				//BattleSpear SetEffect
				case "battlespear - cold with twinkles": SetEffect(player, 74); break;
				case "battlespear - evil green aura": SetEffect(player, 75); break;
				case "battlespear - evil red aura": SetEffect(player, 76); break;
				case "battlespear - flaming": SetEffect(player, 77); break;
				case "battlespear - hot gold glow": SetEffect(player, 78); break;
				case "battlespear - hot fire glow": SetEffect(player, 79); break;
				case "battlespear - red aura": SetEffect(player, 80); break;
				//Case Lugged Spear
				case "Lugged spear":
					SendReply(player, 
							    "Choose a weapon effect : \n" +
								"[lugged spear - blue glow]\n" +
								"[lugged spear - hot blue glow]\n" +
								"[lugged spear - cold with twinkles]\n" +
								"[lugged spear - flaming]\n" +
								"[lugged spear - electric arcing]\n" +
								"[lugged spear - hot yellow flame]\n" +
								"[lugged spear - orange flame with sparks]\n" +
								"[lugged spear - orange to purple flame]\n" +
								"[lugged spear - hot purple flame]\n" +
								"[lugged spear - silvery glow]\n");
				break;
				//Spear SetEffect
				case "lugged spear - blue glow": SetEffect(player, 81); break;
				case "lugged spear - hot blue glow": SetEffect(player, 82); break;
				case "lugged spear - cold with twinkles": SetEffect(player, 83); break;
				case "lugged spear - flaming": SetEffect(player, 84); break;
				case "lugged spear - electric arcing": SetEffect(player, 85); break;
				case "lugged spear - hot yellow flame": SetEffect(player, 86); break;
				case "lugged spear - orange flame with sparks": SetEffect(player, 87); break;
				case "lugged spear - orange to purple flame": SetEffect(player, 88); break;
				case "lugged spear - hot purple flame": SetEffect(player, 89); break;
				case "lugged spear - silvery glow": SetEffect(player, 90);	break;
				//Case Staff
				case "Staff":
					SendReply(player,
						          "Choose a weapon effect : \n" +
						          "[staff - blue glow]\n" +
								  "[staff - blue glow with twinkles]\n" +
								  "[staff - gold glow]\n" +
								  "[staff - gold glow with twinkles]\n" +
								  "[staff - faint red glow]\n");  
				break;
				//Staff SetEffect
				case "staff - blue glow": SetEffect(player, 90); break;
				case "staff - blue glow with twinkles": SetEffect(player, 91); break;
				case "staff - gold glow": SetEffect(player, 92); break;
				case "staff - gold glow with twinkles": SetEffect(player, 93); break;
				case "staff - faint red glow": SetEffect(player, 94); break;
				#endregion effects
				
				#region speed
				case "speed":
				    SendReply(player, "What would you like to be the new speed "+
				              		"of your weapon ?\n"+
				              		"  [15]"+
				              		"  [20]"+
				              		"  [25]\n"+
				              		"  [30]"+
				              		"  [35]"+
				              		"  [40]\n"+
				              		"  [45]"+
				              		"  [50]"+
				              		"  [55]");
				break;
				case "15":
					SetSpeed(player, 15);
				break;
				case "20":
					SetSpeed(player, 20);
				break;
				case "25":
					SetSpeed(player, 25);
				break;
				case "30":
					SetSpeed(player, 30);
				break;
				case "35":
					SetSpeed(player, 35);
				break;
				case "40":
					SetSpeed(player, 40);
				break;
				case "45":
					SetSpeed(player, 45);
				break;
				case "50":
					SetSpeed(player, 50);
				break;
				case "55":
					SetSpeed(player, 55);
				break;
				#endregion speed
				
				#region dye
				case "dye":
					SendReply(player, "Since the color list is so big, i have to send it"+
					          "in two parts.\n [First Part]\n [Second Part]\n");
				break;
				case "First Part":
					SendReply(player, 
					        "[White]\n"+
							"[Old Red]\n"+
							"[Old Green]\n"+
							"[Old Blue]\n"+
							"[Old Yellow]\n"+
							"[Old Purple]\n"+
							"[Gray]\n"+
							"[Old Turquoise]\n"+
							"[Leather Yellow]\n"+
							"[Leather Red]\n"+
							"[Leather Green]\n"+
							"[Leather Orange]\n"+
							"[Leather Violet]\n"+
							"[Leather Forest Green]\n"+
							"[Leather Blue]\n"+
							"[Leather Purple]\n"+
							"[Bronze]\n"+
							"[Iron]\n"+
							"[Steel]\n"+
							"[Alloy]\n"+
							"[Fine Alloy]\n"+
							"[Mithril]\n"+
							"[Asterite]\n"+
							"[Eog]\n"+
							"[Xenium]\n"+
							"[Vaanum]\n"+
							"[Adamantium]\n"+
							"[Red Cloth]\n"+
							"[Orange Cloth]\n"+
							"[Yellow-Orange Cloth]\n"+
							"[Yellow Cloth]\n"+
							"[Yellow-Green Cloth]\n"+
							"[Green Cloth]\n"+
							"[Blue-Green Cloth]\n"+
							"[Turquoise Cloth]\n"+
							"[Light Blue Cloth]\n"+
							"[Blue Cloth]\n"+
							"[Blue-Violet Cloth]\n"+
							"[Violet Cloth]\n"+
							"[Bright Violet Cloth]\n"+
							"[Purple Cloth]\n"+
							"[Bright Purple Cloth]\n"+
							"[Purple-Red Cloth]\n"+
							"[Black Cloth]\n"+
							"[Brown Cloth]\n"+
							"[Blue Metal]\n"+
							"[Green Metal]\n"+
							"[Yellow Metal]\n"+
							"[Gold Metal]\n"+
							"[Red Metal]\n"+
							"[Purple Metal]\n"+
							"[Blue 1]\n"+
							"[Blue 2]\n"+
							"[Blue 3]\n"+
							"[Blue 4]\n"+
							"[Turquoise 1]\n"+
							"[Turquoise 2]\n"+
							"[Turquoise 3]\n"+
							"[Teal 1]\n"+
							"[Teal 2]\n"+
							"[Teal 3]\n"+
							"[Brown 1]\n"+
							"[Brown 2]\n"+
							"[Brown 3]\n"+
							"[Red 1]\n"+
							"[Red 2]\n"+
							"[Red 3]\n"+
							"[Red 4]\n"+
							"[Green 1]\n"+
							"[Green 2]\n"+
							"[Green 3]\n"+
							"[Green 4]\n");
				break;	
				case "Second Part":
					SendReply(player,"[Gray 1]\n"+
							"[Gray 2]\n"+
							"[Gray 3]\n"+
							"[Orange 1]\n"+
							"[Orange 2]\n"+
							"[Orange 3]\n"+
							"[Purple 1]\n"+
							"[Purple 2]\n"+
							"[Purple 3]\n"+
							"[Yellow 1]\n"+
							"[Yellow 2]\n"+
							"[Yelow 3]\n"+
							"[violet]\n"+
							"[mauve]\n"+
							"[Blue 4]\n"+
							"[Purple 4]\n"+
							"[Ship Red]\n"+
							"[Ship Red 2]\n"+
							"[Ship Orange]\n"+
							"[Ship Orange 2]\n"+
							"[Orange 3]\n"+
							"[Ship Yellow]\n"+
							"[Ship Lime Green]\n"+
							"[Ship Green]\n"+
							"[Ship Green 2]\n"+
							"[Ship Turquiose]\n"+
							"[Ship Turqiose 2]\n"+
							"[Ship Blue]\n"+
							"[Ship Blue 2]\n"+
							"[Ship Blue 3]\n"+
							"[Ship Purple]\n"+
							"[Ship Purple 2]\n"+
							"[Ship Purple 3]\n"+
							"[Ship Pink]\n"+
							"[Ship Charcoal]\n"+
							"[Ship Charcoal 2]\n"+
							"[red - crafter only]\n"+
							"[plum - crafter only]\n"+
							"[purple - crafter only]\n"+
							"[dark purple - crafter only]\n"+
							"[dusky purple - crafter only]\n"+
							"[light gold - crafter only]\n"+
							"[dark gold - crafter only]\n"+
							"[dirty orange - crafter only]\n"+
							"[dark tan - crafter only]\n"+
							"[brown - crafter only]\n"+
							"[light green - crafter only]\n"+
							"[olive green - crafter only]\n"+
							"[cornflower blue - crafter only]\n"+
							"[light gray - crafter only]\n"+
							"[hot pink - crafter only]\n"+
							"[dusky rose - crafter only]\n"+
							"[sage green - crafter only]\n"+
							"[lime green - crafter only]\n"+
							"[gray teal - crafter only]\n"+
							"[gray blue - crafter only]\n"+
							"[olive gray - crafter only]\n"+
							"[Navy Blue - crafter only]\n"+
							"[Forest Green - crafter only]\n"+
							"[Burgundy - crafter only]\n");
				break;	
				case "White":
				    SetColor(player, 0);
				break;
				case "Old Red":
				    SetColor(player, 1);
				break;
				case "Old Green":
				    SetColor(player, 2);
				break;
				case "Old Blue":
				    SetColor(player, 3);
				break;
				case "Old Yellow":
				    SetColor(player, 4);
				break;
				case "Old Purple":
				    SetColor(player, 5);
				break;
				case "Gray":
				    SetColor(player, 6);
				break;
				case "Old Turquoise":
				    SetColor(player, 7);
				break;
				case "Leather Yellow":
				    SetColor(player, 8);
				break;
				case "Leather Red":
				    SetColor(player, 9);
				break;
				case "Leather Green":
				    SetColor(player, 10);
				break;
				case "Leather Orange":
				    SetColor(player, 11);
				break;
				case "Leather Violet":
				    SetColor(player, 12);
				break;
				case "Leather Forest Green":
				    SetColor(player, 13);
				break;
				case "Leather Blue":
				    SetColor(player, 14);
				break;
				case "Leather Purple":
				    SetColor(player, 15);
				break;
				case "Bronze":
				    SetColor(player, 16);
				break;
				case "Iron":
				    SetColor(player, 17);
				break;
				case "Steel":
				    SetColor(player, 18);
				break;
				case "Alloy":
				    SetColor(player, 19);
				break;
				case "Fine Alloy":
				    SetColor(player, 20);
				break;
				case "Mithril":
				    SetColor(player, 21);
				break;
				case "Asterite":
				    SetColor(player, 22);
				break;
				case "Eog":
				    SetColor(player, 23);
				break;
				case "Xenium":
				    SetColor(player, 24);
				break;
				case "Vaanum":
				    SetColor(player, 25);
				break;
				case "Adamantium":
				    SetColor(player, 26);
				break;
				case "Red Cloth":
				    SetColor(player, 27);
				break;
				case "Orange Cloth":
				    SetColor(player, 28);
				break;
				case "Yellow-Orange Cloth":
				    SetColor(player, 29);
				break;
				case "Yellow Cloth":
				    SetColor(player, 30);
				break;
				case "Yellow-Green Cloth":
				    SetColor(player, 31);
				break;
				case "Green Cloth":
				    SetColor(player, 32);
				break;
				case "Blue-Green Cloth":
				    SetColor(player, 33);
				break;
				case "Turquoise Cloth":
				    SetColor(player, 34);
				break;
				case "Light Blue Cloth":
				    SetColor(player, 35);
				break;
				case "Blue Cloth":
				    SetColor(player, 36);
				break;
				case "Blue-Violet Cloth":
				    SetColor(player, 37);
				break;
				case "Violet Cloth":
				    SetColor(player, 38);
				break;
				case "Bright Violet Cloth":
				    SetColor(player, 39);
				break;
				case "Purple Cloth":
				    SetColor(player, 40);
				break;
				case "Bright Purple Cloth":
				    SetColor(player, 41);
				break;
				case "Purple-Red Cloth":
				    SetColor(player, 42);
				break;
				case "Black Cloth":
				    SetColor(player, 43);
				break;
				case "Brown Cloth":
				    SetColor(player, 44);
				break;
				case "Blue Metal":
				    SetColor(player, 45);
				break;
				case "Green Metal":
				    SetColor(player, 46);
				break;
				case "Yellow Metal":
				    SetColor(player, 47);
				break;
				case "Gold Metal":
				    SetColor(player, 48);
				break;
				case "Red Metal":
				    SetColor(player, 49);
				break;
				case "Purple Metal":
				    SetColor(player, 50);
				break;
				case "Blue 1":
				    SetColor(player, 51);
				break;
				case "Blue 2":
				    SetColor(player, 52);
				break;
				case "Blue 3":
				    SetColor(player, 53);
				break;
				case "Blue 4":
				    SetColor(player, 54);
				break;
				case "Turquoise 1":
				    SetColor(player, 55);
				break;
				case "Turquoise 2":
				    SetColor(player, 56);
				break;
				case "Turquoise 3":
				    SetColor(player, 57);
				break;
				case "Teal 1":
				    SetColor(player, 58);
				break;
				case "Teal 2":
				    SetColor(player, 59);
				break;
				case "Teal 3":
				    SetColor(player, 60);
				break;
				case "Brown 1":
				    SetColor(player, 61);
				break;
				case "Brown 2":
				    SetColor(player, 62);
				break;
				case "Brown 3":
				    SetColor(player, 63);
				break;
				case "Red 1":
				    SetColor(player, 64);
				break;
				case "Red 2":
				    SetColor(player, 65);
				break;
				case "Red 3":
				    SetColor(player, 66);
				break;
				case "Red 4":
				    SetColor(player, 67);
				break;
				case "Green 1":
				    SetColor(player, 68);
				break;
				case "Green 2":
				    SetColor(player, 69);
				break;
				case "Green 3":
				    SetColor(player, 70);
				break;
				case "Green 4":
				    SetColor(player, 71);
				break;
				case "Gray 1":
				    SetColor(player, 72);
				break;
				case "Gray 2":
				    SetColor(player, 73);
				break;
				case "Gray 3":
				    SetColor(player, 74);
				break;
				case "Orange 1":
				    SetColor(player, 75);
				break;
				case "Orange 2":
				    SetColor(player, 76);
				break;
				case "Orange 3":
				    SetColor(player, 77);
				break;
				case "Purple 1":
				    SetColor(player, 78);
				break;
				case "Purple 2":
				    SetColor(player, 79);
				break;
				case "Purple 3":
				    SetColor(player, 80);
				break;
				case "Yellow 1":
				    SetColor(player, 81);
				break;
				case "Yellow 2":
				    SetColor(player, 82);
				break;
				case "Yelow 3":
				    SetColor(player, 83);
				break;
				case "violet":
				    SetColor(player, 84);
				break;
				case "mauve":
				    SetColor(player, 85);
				break;
				case "Blue 5":
				    SetColor(player, 86);
				break;
				case "Purple 4":
				    SetColor(player, 87);
				break;
				case "Ship Red":
				    SetColor(player, 100);
				break;
				case "Ship Red 2":
				    SetColor(player, 101);
				break;
				case "Ship Orange":
				    SetColor(player, 102);
				break;
				case "Ship Orange 2":
				    SetColor(player, 103);
				break;
				case "Orange 4":
				    SetColor(player, 104);
				break;
				case "Ship Yellow":
				    SetColor(player, 105);
				break;
				case "Ship Lime Green":
				    SetColor(player, 106);
				break;
				case "Ship Green":
				    SetColor(player, 107);
				break;
				case "Ship Green 2":
				    SetColor(player, 108);
				break;
				case "Ship Turquiose":
				    SetColor(player, 109);
				break;
				case "Ship Turqiose 2":
				    SetColor(player, 110);
				break;
				case "Ship Blue":
				    SetColor(player, 111);
				break;
				case "Ship Blue 2":
				    SetColor(player, 112);
				break;
				case "Ship Blue 3":
				    SetColor(player, 113);
				break;
				case "Ship Purple":
				    SetColor(player, 114);
				break;
				case "Ship Purple 2":
				    SetColor(player, 115);
				break;
				case "Ship Purple 3":
				    SetColor(player, 116);
				break;
				case "Ship Pink":
				    SetColor(player, 117);
				break;
				case "Ship Charcoal":
				    SetColor(player, 118);
				break;
				case "Ship Charcoal 2":
				    SetColor(player, 119);
				break;
				case "red - crafter only":
				    SetColor(player, 120);
				break;
				case "plum - crafter only":
				    SetColor(player, 121);
				break;
				case "purple - crafter only":
				    SetColor(player, 122);
				break;
				case "dark purple - crafter only":
				    SetColor(player, 123);
				break;
				case "dusky purple - crafter only":
				    SetColor(player, 124);
				break;
				case "light gold - crafter only":
				    SetColor(player, 125);
				break;
				case "dark gold - crafter only":
				    SetColor(player, 126);
				break;
				case "dirty orange - crafter only":
				    SetColor(player, 127);
				break;
				case "dark tan - crafter only":
				    SetColor(player, 128);
				break;
				case "brown - crafter only":
				    SetColor(player, 129);
				break;
				case "light green - crafter only":
				    SetColor(player, 130);
				break;
				case "olive green - crafter only":
				    SetColor(player, 131);
				break;
				case "cornflower blue - crafter only":
				    SetColor(player, 132);
				break;
				case "light gray - crafter only":
				    SetColor(player, 133);
				break;
				case "hot pink - crafter only":
				    SetColor(player, 134);
				break;
				case "dusky rose - crafter only":
				    SetColor(player, 135);
				break;
				case "sage green - crafter only":
				    SetColor(player, 136);
				break;
				case "lime green - crafter only":
				    SetColor(player, 137);
				break;
				case "gray teal - crafter only":
				    SetColor(player, 138);
				break;
				case "gray blue - crafter only":
				    SetColor(player, 139);
				break;
				case "olive gray - crafter only":
				    SetColor(player, 140);
				break;
				case "Navy Blue - crafter only":
				    SetColor(player, 141);
				break;
				case "Forest Green - crafter only":
				    SetColor(player, 142);
				break;
				case "Burgundy - crafter only":
				    SetColor(player, 143);
				break;

				#endregion dye
			}

			return true;
		}

		public void SendReply(GamePlayer player, string msg)
		{
			player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
		}
		
		#region setcolor
		public void SetColor(GamePlayer player, int color)
		{
			WeakReference itemWeak = (WeakReference) player.TempProperties.getObjectProperty(EFFECTNPC_ITEM_WEAK,	new WeakRef(null));
			
			player.TempProperties.removeProperty(EFFECTNPC_ITEM_WEAK);
			
			InventoryItem item = (InventoryItem) itemWeak.Target;
			
			if(item.Object_Type == 41 || item.Object_Type == 43 || item.Object_Type == 44 || 
			   item.Object_Type == 46)
			{
			   		SendReply(player, "You can't dye that.");
			}
			
			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				return;
			}
			
			if(item.Name.StartsWith(Prefix0) || item.Name.StartsWith(Prefix1) || item.Name.StartsWith(Prefix2) ||
				item.Name.StartsWith(Prefix3) || item.Name.StartsWith(Prefix4) || item.Name.StartsWith(Prefix5) ||
				item.Name.StartsWith(Prefix6))
			{
					ItemTemplate item2 = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), item.Id_nb);
					if(item2 != null)
					{
						item.Name = item2.Name;
					}
			}
			
			m_timer.Enqueue ( new RegionTimer(this, new RegionTimerCallback(Effect), duration) );
			castplayer.Enqueue(player) ;
			
			item.Name = Prefix6 + item.Name;
			item.Color = color;
			item.CrafterName = this.Name;
			player.Out.SendInventoryItemsUpdate(new InventoryItem[] {item});
			SendReply(player, "Look at that, the color has come out beautifully. Wear it with pride.");
			
			foreach ( GamePlayer visplayer in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE) )
			{
				visplayer.Out.SendSpellCastAnimation ( this , spell , 30 ) ;
			}
		}
		#endregion setcolor
		
		#region setspeed
		public void SetSpeed(GamePlayer player, int speed)
		{
			WeakReference itemWeak = (WeakReference) player.TempProperties.getObjectProperty(EFFECTNPC_ITEM_WEAK,	new WeakRef(null));
			
			player.TempProperties.removeProperty(EFFECTNPC_ITEM_WEAK);
			
			InventoryItem item = (InventoryItem) itemWeak.Target;
			
			if(item.Object_Type < 1 || item.Object_Type > 26)
			{
				SendReply(player, "I cannot work on anything else than weapons.");
				return;
			}
			
			if(speed > 40 && item.Hand != 1 && item.Object_Type != 10 && item.Object_Type != 10 && item.Object_Type != 9
			   && item.Object_Type != 15 && item.Object_Type != 18)
			{
			   	SendReply(player, "Onehanded weapons cannot be that slow.");
				return;
			}
			
			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				return;
			}

			
			if(item.Name.StartsWith(Prefix0) || item.Name.StartsWith(Prefix1) || item.Name.StartsWith(Prefix2) ||
				item.Name.StartsWith(Prefix3) || item.Name.StartsWith(Prefix4) || item.Name.StartsWith(Prefix5) ||
				item.Name.StartsWith(Prefix6))
			{
					ItemTemplate item2 = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), item.Id_nb);
					if(item2 != null)
					{
						item.Name = item2.Name;
					}
			}
			
			m_timer.Enqueue ( new RegionTimer(this, new RegionTimerCallback(Effect), duration) );
			castplayer.Enqueue(player) ;
			
			item.Name = Prefix5 + item.Name;
			item.SPD_ABS = speed;
			item.CrafterName = this.Name;
			player.Out.SendInventoryItemsUpdate(new InventoryItem[] {item});
			SendReply(player, "It has been quite a piece of work but now it's done.\n May the "+item.Name+" aid you on your ways.");
			foreach ( GamePlayer visplayer in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE) )
			{
				visplayer.Out.SendSpellCastAnimation ( this , spell , 30 ) ;
			}
		}
		#endregion setspeed
		
		#region seteffect
		public void SetEffect(GamePlayer player, int effect)
		{
			
			WeakReference itemWeak = (WeakReference) player.TempProperties.getObjectProperty(EFFECTNPC_ITEM_WEAK,	new WeakRef(null));
			
			player.TempProperties.removeProperty(EFFECTNPC_ITEM_WEAK);
			
			InventoryItem item = (InventoryItem) itemWeak.Target;
			
			if(item.Object_Type < 1 || item.Object_Type > 26)
			{
				SendReply(player, "I cannot work on anything else than weapons.");
				return;
			}
			
			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				return;
			}

			if(item.Name.StartsWith(Prefix0) || item.Name.StartsWith(Prefix1) || item.Name.StartsWith(Prefix2) ||
				item.Name.StartsWith(Prefix3) || item.Name.StartsWith(Prefix4) || item.Name.StartsWith(Prefix5) ||
				item.Name.StartsWith(Prefix6))
			{
					ItemTemplate item2 = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), item.Id_nb);
					if(item2 != null)
					{
						item.Name = item2.Name;
					}
			}
			
			m_timer.Enqueue ( new RegionTimer(this, new RegionTimerCallback(Effect), duration) );
			castplayer.Enqueue(player) ;
			
			Chance = rnd.Next(1, 100);
			if(effect == 0)item.Name = Prefix0 + item.Name;
			if(Chance >= 0 && Chance <= 25 && effect != 0)item.Name = Prefix1 + item.Name;
			if(Chance >= 25 && Chance <= 50 && effect != 0)item.Name = Prefix2 + item.Name;
			if(Chance >= 50 && Chance <= 75 && effect != 0)item.Name = Prefix3 + item.Name;
			if(Chance >= 75 && Chance <= 100 && effect != 0)item.Name = Prefix4 + item.Name;
				
			item.Effect = effect;
			item.CrafterName = this.Name;
			player.Out.SendInventoryItemsUpdate(new InventoryItem[] {item});
			SendReply(player, "May the "+item.Name+" lead you to a bright future.");
			foreach ( GamePlayer visplayer in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE) )
			{
				visplayer.Out.SendSpellCastAnimation ( this , spell , 30 ) ;
			}
		}
		#endregion seteffect
		
		public int Effect(RegionTimer timer)
		{
			m_timer.Dequeue();
			GamePlayer player = (GamePlayer)castplayer.Dequeue() ;
			foreach(GamePlayer visplayer in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				visplayer.Out.SendSpellEffectAnimation(this, player, spell, 0, false, 1);
			}
			return 0;
		}
	}
}

