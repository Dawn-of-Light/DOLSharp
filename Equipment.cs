//Since the PLVLr is redundant with the new DB, made this... All Epic/Weapons/Rewards/General Crap is done in here..
//Created by Overdriven of course... Who else would do this? =<

using System;
using DOL;
using DOL.GS;
using DOL.Events;
using DOL.GS.PacketHandler;
using System.Reflection;
using System.Collections;
using DOL.Database;
using log4net;

namespace DOL.GS.Scripts
{
	public class Equipment : GameNPC
	{
		public Equipment()
			: base()
		{
			Flags |= (uint)GameNPC.eFlags.PEACE;
		}

		#region Declarations

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Albion Epic Declarations
		private static ItemTemplate WizardEpicBoots = null; //Bernor's Numinous Boots 
		private static ItemTemplate WizardEpicHelm = null; //Bernor's Numinous Coif 
		private static ItemTemplate WizardEpicGloves = null; //Bernor's Numinous Gloves 
		private static ItemTemplate WizardEpicVest = null; //Bernor's Numinous Hauberk 
		private static ItemTemplate WizardEpicLegs = null; //Bernor's Numinous Legs 
		private static ItemTemplate WizardEpicArms = null; //Bernor's Numinous Sleeves 
		private static ItemTemplate MinstrelEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate MinstrelEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate MinstrelEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate MinstrelEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate MinstrelEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate MinstrelEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate SorcererEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate SorcererEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate SorcererEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate SorcererEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate SorcererEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate SorcererEpicArms = null; //Valhalla Touched Sleeves
		private static ItemTemplate ClericEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ClericEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ClericEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ClericEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ClericEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ClericEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate PaladinEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate PaladinEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate PaladinEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate PaladinEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate PaladinEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate PaladinEpicArms = null; //Valhalla Touched Sleeves
		private static ItemTemplate MercenaryEpicBoots = null; // of the Shadowy Embers  Boots 
		private static ItemTemplate MercenaryEpicHelm = null; // of the Shadowy Embers  Coif 
		private static ItemTemplate MercenaryEpicGloves = null; // of the Shadowy Embers  Gloves 
		private static ItemTemplate MercenaryEpicVest = null; // of the Shadowy Embers  Hauberk 
		private static ItemTemplate MercenaryEpicLegs = null; // of the Shadowy Embers  Legs 
		private static ItemTemplate MercenaryEpicArms = null; // of the Shadowy Embers  Sleeves 
		private static ItemTemplate ReaverEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ReaverEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ReaverEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ReaverEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ReaverEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ReaverEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate CabalistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate CabalistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate CabalistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate CabalistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate CabalistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate CabalistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate InfiltratorEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate InfiltratorEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate InfiltratorEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate InfiltratorEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate InfiltratorEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate InfiltratorEpicArms = null; //Subterranean Sleeves		
		private static ItemTemplate NecromancerEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate NecromancerEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate NecromancerEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate NecromancerEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate NecromancerEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate NecromancerEpicArms = null; //Subterranean Sleeves
		private static ItemTemplate ScoutEpicBoots = null; //Brigandine of Vigilant Defense  Boots 
		private static ItemTemplate ScoutEpicHelm = null; //Brigandine of Vigilant Defense  Coif 
		private static ItemTemplate ScoutEpicGloves = null; //Brigandine of Vigilant Defense  Gloves 
		private static ItemTemplate ScoutEpicVest = null; //Brigandine of Vigilant Defense  Hauberk 
		private static ItemTemplate ScoutEpicLegs = null; //Brigandine of Vigilant Defense  Legs 
		private static ItemTemplate ScoutEpicArms = null; //Brigandine of Vigilant Defense  Sleeves 
		private static ItemTemplate ArmsmanEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ArmsmanEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ArmsmanEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ArmsmanEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ArmsmanEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ArmsmanEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate TheurgistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate TheurgistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate TheurgistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate TheurgistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate TheurgistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate TheurgistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate FriarEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate FriarEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate FriarEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate FriarEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate FriarEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate FriarEpicArms = null; //Subterranean Sleeves    

		#endregion Albion Epic Declarations
		#region Midgard Epic Declaration

		private static ItemTemplate HunterEpicBoots = null; //Call of the Hunt Boots 
		private static ItemTemplate HunterEpicHelm = null; //Call of the Hunt Coif 
		private static ItemTemplate HunterEpicGloves = null; //Call of the Hunt Gloves 
		private static ItemTemplate HunterEpicVest = null; //Call of the Hunt Hauberk 
		private static ItemTemplate HunterEpicLegs = null; //Call of the Hunt Legs 
		private static ItemTemplate HunterEpicArms = null; //Call of the Hunt Sleeves 
		private static ItemTemplate ShadowbladeEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate ShadowbladeEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate ShadowbladeEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate ShadowbladeEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate ShadowbladeEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate ShadowbladeEpicArms = null; //Shadow Shrouded Sleeves
		private static ItemTemplate SpiritmasterEpicBoots = null;
		private static ItemTemplate SpiritmasterEpicHelm = null;
		private static ItemTemplate SpiritmasterEpicGloves = null;
		private static ItemTemplate SpiritmasterEpicLegs = null;
		private static ItemTemplate SpiritmasterEpicArms = null;
		private static ItemTemplate SpiritmasterEpicVest = null;
		private static ItemTemplate RunemasterEpicBoots = null;
		private static ItemTemplate RunemasterEpicHelm = null;
		private static ItemTemplate RunemasterEpicGloves = null;
		private static ItemTemplate RunemasterEpicLegs = null;
		private static ItemTemplate RunemasterEpicArms = null;
		private static ItemTemplate RunemasterEpicVest = null;
		private static ItemTemplate BonedancerEpicBoots = null;
		private static ItemTemplate BonedancerEpicHelm = null;
		private static ItemTemplate BonedancerEpicGloves = null;
		private static ItemTemplate BonedancerEpicLegs = null;
		private static ItemTemplate BonedancerEpicArms = null;
		private static ItemTemplate BonedancerEpicVest = null;
		private static ItemTemplate HealerEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate HealerEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate HealerEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate HealerEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate HealerEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate HealerEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate ShamanEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate ShamanEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate ShamanEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate ShamanEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate ShamanEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate ShamanEpicArms = null; //Subterranean Sleeves
		private static ItemTemplate WarriorEpicBoots = null;
		private static ItemTemplate WarriorEpicHelm = null;
		private static ItemTemplate WarriorEpicGloves = null;
		private static ItemTemplate WarriorEpicLegs = null;
		private static ItemTemplate WarriorEpicArms = null;
		private static ItemTemplate WarriorEpicVest = null;
		private static ItemTemplate BerserkerEpicBoots = null;
		private static ItemTemplate BerserkerEpicHelm = null;
		private static ItemTemplate BerserkerEpicGloves = null;
		private static ItemTemplate BerserkerEpicLegs = null;
		private static ItemTemplate BerserkerEpicArms = null;
		private static ItemTemplate BerserkerEpicVest = null;
		private static ItemTemplate ThaneEpicBoots = null;
		private static ItemTemplate ThaneEpicHelm = null;
		private static ItemTemplate ThaneEpicGloves = null;
		private static ItemTemplate ThaneEpicLegs = null;
		private static ItemTemplate ThaneEpicArms = null;
		private static ItemTemplate ThaneEpicVest = null;
		private static ItemTemplate SkaldEpicBoots = null;
		private static ItemTemplate SkaldEpicHelm = null;
		private static ItemTemplate SkaldEpicGloves = null;
		private static ItemTemplate SkaldEpicVest = null;
		private static ItemTemplate SkaldEpicLegs = null;
		private static ItemTemplate SkaldEpicArms = null;
		private static ItemTemplate SavageEpicBoots = null;
		private static ItemTemplate SavageEpicHelm = null;
		private static ItemTemplate SavageEpicGloves = null;
		private static ItemTemplate SavageEpicVest = null;
		private static ItemTemplate SavageEpicLegs = null;
		private static ItemTemplate SavageEpicArms = null;

		#endregion Midgard Epic Declaration
		#region Hibernia Epic Declaration

		private static ItemTemplate ChampionEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate ChampionEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate ChampionEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate ChampionEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate ChampionEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate ChampionEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate BardEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate BardEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate BardEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate BardEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate BardEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate BardEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate EnchanterEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate EnchanterEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate EnchanterEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate EnchanterEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate EnchanterEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate EnchanterEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate NightshadeEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate NightshadeEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate NightshadeEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate NightshadeEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate NightshadeEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate NightshadeEpicArms = null; //Subterranean Sleeves 
		private static ItemTemplate RangerEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate RangerEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate RangerEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate RangerEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate RangerEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate RangerEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate HeroEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate HeroEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate HeroEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate HeroEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate HeroEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate HeroEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate EldritchEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate EldritchEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate EldritchEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate EldritchEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate EldritchEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate EldritchEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate WardenEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate WardenEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate WardenEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate WardenEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate WardenEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate WardenEpicArms = null; //Subterranean Sleeves
		private static ItemTemplate BlademasterEpicBoots = null; //Mist Shrouded Boots 
		private static ItemTemplate BlademasterEpicHelm = null; //Mist Shrouded Coif 
		private static ItemTemplate BlademasterEpicGloves = null; //Mist Shrouded Gloves 
		private static ItemTemplate BlademasterEpicVest = null; //Mist Shrouded Hauberk 
		private static ItemTemplate BlademasterEpicLegs = null; //Mist Shrouded Legs 
		private static ItemTemplate BlademasterEpicArms = null; //Mist Shrouded Sleeves 
		private static ItemTemplate DruidEpicBoots = null; //Shadow Shrouded Boots 
		private static ItemTemplate DruidEpicHelm = null; //Shadow Shrouded Coif 
		private static ItemTemplate DruidEpicGloves = null; //Shadow Shrouded Gloves 
		private static ItemTemplate DruidEpicVest = null; //Shadow Shrouded Hauberk 
		private static ItemTemplate DruidEpicLegs = null; //Shadow Shrouded Legs 
		private static ItemTemplate DruidEpicArms = null; //Shadow Shrouded Sleeves 
		private static ItemTemplate MentalistEpicBoots = null; //Valhalla Touched Boots 
		private static ItemTemplate MentalistEpicHelm = null; //Valhalla Touched Coif 
		private static ItemTemplate MentalistEpicGloves = null; //Valhalla Touched Gloves 
		private static ItemTemplate MentalistEpicVest = null; //Valhalla Touched Hauberk 
		private static ItemTemplate MentalistEpicLegs = null; //Valhalla Touched Legs 
		private static ItemTemplate MentalistEpicArms = null; //Valhalla Touched Sleeves 
		private static ItemTemplate AnimistEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate AnimistEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate AnimistEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate AnimistEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate AnimistEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate AnimistEpicArms = null; //Subterranean Sleeves 
		private static ItemTemplate ValewalkerEpicBoots = null; //Subterranean Boots 
		private static ItemTemplate ValewalkerEpicHelm = null; //Subterranean Coif 
		private static ItemTemplate ValewalkerEpicGloves = null; //Subterranean Gloves 
		private static ItemTemplate ValewalkerEpicVest = null; //Subterranean Hauberk 
		private static ItemTemplate ValewalkerEpicLegs = null; //Subterranean Legs 
		private static ItemTemplate ValewalkerEpicArms = null; //Subterranean Sleeves

		#endregion Hibernia Epic Declaration
		#region Weapon Declarations
		private static ItemTemplate RangerEpicBow = null;
		private static ItemTemplate ScoutEpicBow = null;
		private static ItemTemplate HunterEpicBow = null;
		private static ItemTemplate EpicShortBow = null;
		private static ItemTemplate EpicCrossBow = null;
		private static ItemTemplate EpicCasterStaff = null;
		private static ItemTemplate EpicFriarStaff = null;
		private static ItemTemplate EpicHarp = null;
		private static ItemTemplate Midgard2HHammer = null;
		private static ItemTemplate Midgard2HSword = null;
		private static ItemTemplate Midgard2HAxe = null;
		private static ItemTemplate Midgard1HHammer = null;
		private static ItemTemplate Midgard1HSword = null;
		private static ItemTemplate Midgard1HAxe = null;
		private static ItemTemplate Midgard1HLeftAxe = null;
		private static ItemTemplate MidgardSlashClaw = null;
		private static ItemTemplate MidgardSlashLeftClaw = null;
		private static ItemTemplate MidgardThrustClaw = null;
		private static ItemTemplate MidgardThrustLeftClaw = null;
		private static ItemTemplate MidgardThrustSpear = null;
		private static ItemTemplate MidgardSlashSpear = null;
		private static ItemTemplate EpicSmallShield = null;
		private static ItemTemplate EpicMediumShield = null;
		private static ItemTemplate EpicLargeShield = null;
		private static ItemTemplate HiberniaTrustSpear = null;
		private static ItemTemplate HiberniaSlashSpear = null;
		private static ItemTemplate HiberniaSlashLargeWeapon = null;
		private static ItemTemplate HiberniaThrustLargeWeapon = null;
		private static ItemTemplate HiberniaCrushLargeWeapon = null;
		private static ItemTemplate HiberniaScythe = null;
		private static ItemTemplate HiberniaBlade = null;
		private static ItemTemplate HiberniaLeftBlade = null;
		private static ItemTemplate HiberniaBlunt = null;
		private static ItemTemplate HiberniaLeftBlunt = null;
		private static ItemTemplate HiberniaPierce = null;
		private static ItemTemplate HiberniaLeftPierce = null;
		private static ItemTemplate AlbionTwoHandedCrush = null;
		private static ItemTemplate AlbionTwoHandedThrust = null;
		private static ItemTemplate AlbionTwoHandedSlash = null;
		private static ItemTemplate AlbionCrushPole = null;
		private static ItemTemplate AlbionThrustPole = null;
		private static ItemTemplate AlbionSlashPole = null;
		private static ItemTemplate AlbionSlash = null;
		private static ItemTemplate AlbionLeftSlash = null;
		private static ItemTemplate AlbionCrush = null;
		private static ItemTemplate AlbionLeftCrush = null;
		private static ItemTemplate AlbionThrust = null;
		private static ItemTemplate AlbionLeftThrust = null;
		private static ItemTemplate AlbionCrushFlex = null;
		private static ItemTemplate AlbionSlashFlex = null;
		private static ItemTemplate AlbionThrustFlex = null;

		#endregion Weapon Declarations
		#region Item Declaration
		private static ItemTemplate EpicBracer1 = null;
		private static ItemTemplate EpicBracer2 = null;
		private static ItemTemplate EpicRing1 = null;
		private static ItemTemplate EpicRing2 = null;
		private static ItemTemplate EpicJewell = null;
		private static ItemTemplate EpicNecklace = null;
		private static ItemTemplate EpicCasterCloak = null;
		private static ItemTemplate EpicMeleeCloak = null;
		private static ItemTemplate EpicMount = null;
		#endregion Item Declaration
		#region GiveItem

		protected static void GiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			GiveItem(null, player, itemTemplate);
		}

		protected static void GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
		{
			InventoryItem item = new InventoryItem(itemTemplate);
			if (player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
			{
				if (source == null)
				{
					player.Out.SendMessage("You receive the " + itemTemplate.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					player.Out.SendMessage("You receive " + itemTemplate.GetName(0, false) + " from " + source.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				player.CreateItemOnTheGround(item);
				player.Out.SendMessage("Your Inventory is full. You couldn't recieve the " + itemTemplate.Name + ", so it's been placed on the ground. Pick it up as soon as possible or it will vanish in a few minutes.", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
			}
		}
		#endregion GiveItem
		#endregion Declarations
		#region Armour/Equip

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Equipment NPC initializing ...");

			#region Weapons
			#region Bows
			#region Ranger Bow

			ItemTemplate item = null;
			item = new ItemTemplate();
			item.Id_nb = "RangerEpicBow";
			item.Name = "Bow of the Ranger";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.DistanceWeapon;
			item.Model = 471;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 54;
			item.Object_Type = 18;
			item.Quality = 100;
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Cold;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			RangerEpicBow = item;

			#endregion Ranger Bow
			#region Scout Bow

			item = new ItemTemplate();
			item.Id_nb = "ScoutEpicBow";
			item.Name = "Bow of the Scout";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.DistanceWeapon;
			item.Model = 471;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 54;
			item.Object_Type = 9;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Cold;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			ScoutEpicBow = item;

			#endregion Scout Bow
			#region Hunter Bow

			item = new ItemTemplate();
			item.Id_nb = "HunterEpicBow";
			item.Name = "Bow of the Hunter";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.DistanceWeapon;
			item.Model = 564;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 44;
			item.Object_Type = 15;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Cold;
			item.Bonus2 = 22;




			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			HunterEpicBow = item;

			#endregion Hunter Bow
			#region Short Bow

			item = new ItemTemplate();
			item.Id_nb = "EpicShortBow";
			item.Name = "Epic Short Bow";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.DistanceWeapon;
			item.Model = 471;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 40;
			item.Object_Type = 5;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Cold;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			EpicShortBow = item;

			#endregion Short Bow
			#region Cross Bow

			item = new ItemTemplate();
			item.Id_nb = "EpicCrossBow";
			item.Name = "Epic Cross Bow";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.DistanceWeapon;
			item.Model = 892;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 40;
			item.Object_Type = 10;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Cold;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			EpicCrossBow = item;

			#endregion Cross Bow
			#endregion Bows
			#region Staffs
			#region Caster Staff
			item = new ItemTemplate();
			item.Id_nb = "EpicCasterStaff";
			item.Name = "Epic Staff of the Caster";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 883;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 44;
			item.Object_Type = 8;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 50;
			item.Bonus1Type = (int)eProperty.AllMagicSkills;
			item.Bonus2 = 50;
			item.Bonus2Type = (int)eProperty.AllFocusLevels;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eProperty.PowerPoolCapBonus;
			item.Bonus4 = 15;
			item.Bonus4Type = (int)eProperty.PowerPool;
			EpicCasterStaff = item;

			#endregion Caster Staff
			#region Friar Staff

			item = new ItemTemplate();
			item.Id_nb = "EpicFriarStaff";
			item.Name = "Epic Friar Quarterstaff";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 883;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 30;
			item.Object_Type = 8;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 50;
			item.Bonus1Type = (int)eProperty.AllMagicSkills;
			item.Bonus2 = 50;
			item.Bonus2Type = (int)eProperty.AllFocusLevels;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eProperty.PowerPoolCapBonus;
			item.Bonus4 = 15;
			item.Bonus4Type = (int)eProperty.PowerPool;
			EpicFriarStaff = item;

			#endregion Friar Staff
			#endregion Staffs
			#region 2 Handed Mid Weapons
			#region Midgard 2H Hammer
			item = new ItemTemplate();
			item.Id_nb = "Midgard2HHammer";
			item.Name = "Two Handed Hammer of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 576;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 48;
			item.Object_Type = 12;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			Midgard2HHammer = item;
			#endregion Midgard2HHammer
			#region Midgard 2H Sword
			item = new ItemTemplate();
			item.Id_nb = "Midgard2HSword";
			item.Name = "Two Handed Sword of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 314;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 56;
			item.Object_Type = 11;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			Midgard2HSword = item;
			#endregion Midgard2HSword
			#region Midgard 2H Axe
			item = new ItemTemplate();
			item.Id_nb = "Midgard2HAxe";
			item.Name = "Two Handed Axe of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 577;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 51;
			item.Object_Type = 13;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			Midgard2HAxe = item;
			#endregion Midgard2HAxe
			#region MidgardThrustSpear
			item = new ItemTemplate();
			item.Id_nb = "MidgardThrustSpear";
			item.Name = "Thrust Spear of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 332;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 40;
			item.Object_Type = 14;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			MidgardThrustSpear = item;
			#endregion MidgardThrustSpear
			#region MidgardSlashSpear
			item = new ItemTemplate();
			item.Id_nb = "MidgardSlashSpear";
			item.Name = "Slash Spear of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 332;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 160;
			item.SPD_ABS = 40;
			item.Object_Type = 14;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			MidgardSlashSpear = item;
			#endregion MidgardSlashSpear
			#endregion 2 Handed Mid Weapons
			#region 1 handed Mid Weapons
			#region Midgard1HHammer
			item = new ItemTemplate();
			item.Id_nb = "Midgard1HHammer";
			item.Name = "One Handed Hammer of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 323;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 36;
			item.Object_Type = 12;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			Midgard1HHammer = item;
			#endregion Midgard1HHammer
			#region Midgard1HSword
			item = new ItemTemplate();
			item.Id_nb = "Midgard1HSword";
			item.Name = "One Handed Sword of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 311;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 34;
			item.Object_Type = 11;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			Midgard1HSword = item;
			#endregion Midgard1HSword
			#region Midgard1HAxe
			item = new ItemTemplate();
			item.Id_nb = "Midgard1HAxe";
			item.Name = "One Handed Axe of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 573;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 35;
			item.Object_Type = 13;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			Midgard1HAxe = item;
			#endregion Midgard1HAxe
			#region Midgard1HLeftAxe
			item = new ItemTemplate();
			item.Id_nb = "Midgard1HLeftAxe";
			item.Name = "One Handed Left Axe of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 577;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 35;
			item.Object_Type = 17;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			Midgard1HLeftAxe = item;
			#endregion Midgard1HLeftAxe
			#region MidgardSlashClaw
			item = new ItemTemplate();
			item.Id_nb = "MidgardSlashClaw";
			item.Name = "One Handed Slash Claw of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 965;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 30;
			item.Object_Type = 25;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			MidgardSlashClaw = item;
			#endregion MidgardSlashClaw
			#region MidgardSlashLeftClaw
			item = new ItemTemplate();
			item.Id_nb = "MidgardSlashLeftClaw";
			item.Name = "One Handed Left Slash Claw of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 965;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 30;
			item.Object_Type = 25;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			MidgardSlashLeftClaw = item;
			#endregion MidgardSlashLeftClaw
			#region MidgardThrustClaw
			item = new ItemTemplate();
			item.Id_nb = "MidgardThrustClaw";
			item.Name = "One Handed Thrust Claw of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 965;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 31;
			item.Object_Type = 25;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			MidgardThrustClaw = item;
			#endregion MidgardThrustClaw
			#region MidgardThrustLeftClaw
			item = new ItemTemplate();
			item.Id_nb = "MidgardThrustLeftClaw";
			item.Name = "One Handed Left Thrust Claw of Midgard";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 965;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 31;
			item.Object_Type = 25;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			MidgardThrustLeftClaw = item;
			#endregion MidgardThrustLeftClaw
			#endregion 1 handed Mid Weapons
			#region Two-Handed Albion Weapons
			#region AlbionTwoHandedCrush
			item = new ItemTemplate();
			item.Id_nb = "AlbionTwoHandedCrush";
			item.Name = "Two Handed Crush of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2662;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 48;
			item.Object_Type = 06;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionTwoHandedCrush = item;
			#endregion AlbionTwoHandedCrush
			#region AlbionTwoHandedThrust
			item = new ItemTemplate();
			item.Id_nb = "AlbionTwoHandedThrust";
			item.Name = "Two Handed Thruster of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2662;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 37;
			item.Object_Type = 06;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionTwoHandedThrust = item;
			#endregion AlbionTwoHandedThrust
			#region AlbionTwoHandedSlash
			item = new ItemTemplate();
			item.Id_nb = "AlbionTwoHandedSlash";
			item.Name = "Two Handed Slasher of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2662;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 51;
			item.Object_Type = (int)eObjectType.TwoHandedWeapon;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionTwoHandedSlash = item;
			#endregion AlbionTwoHandedSlash
			#region AlbionCrushPole
			item = new ItemTemplate();
			item.Id_nb = "AlbionCrushPole";
			item.Name = "Crush Pole of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2664;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 56;
			item.Object_Type = 7;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionCrushPole = item;
			#endregion AlbionCrushPole
			#region AlbionThrustPole
			item = new ItemTemplate();
			item.Id_nb = "AlbionThrustPole";
			item.Name = "Thrust Pole of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2664;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 43;
			item.Object_Type = 7;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionThrustPole = item;
			#endregion AlbionThrustPole
			#region AlbionSlashPole
			item = new ItemTemplate();
			item.Id_nb = "AlbionSlashPole";
			item.Name = "Slash Pole of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2664;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 48;
			item.Object_Type = 07;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionSlashPole = item;
			#endregion AlbionSlashPole
			#endregion Two-Handed Albion Weapons
			#region 1 Handed Albion Weapons
			#region AlbionSlash
			item = new ItemTemplate();
			item.Id_nb = "AlbionSlash";
			item.Name = "One Handed Slasher of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 311;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 34;
			item.Object_Type = 03;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionSlash = item;
			#endregion AlbionSlash
			#region AlbionLeftSlash
			item = new ItemTemplate();
			item.Id_nb = "AlbionLeftSlash";
			item.Name = "One Left Handed Slasher of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 311;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 28;
			item.Object_Type = 03;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionLeftSlash = item;
			#endregion AlbionLeftSlash
			#region AlbionCrush
			item = new ItemTemplate();
			item.Id_nb = "AlbionCrush";
			item.Name = "One Handed Crusher of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 323;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 34;
			item.Object_Type = 02;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionCrush = item;
			#endregion AlbionCrush
			#region AlbionLeftCrush
			item = new ItemTemplate();
			item.Id_nb = "AlbionLeftCrush";
			item.Name = "Left Handed Crusher of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 323;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 28;
			item.Object_Type = 02;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionLeftCrush = item;
			#endregion AlbionLeftCrush
			#region AlbionThrust
			item = new ItemTemplate();
			item.Id_nb = "AlbionThrust";
			item.Name = "One Handed Thruster of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 2658;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 34;
			item.Object_Type = (int)eObjectType.ThrustWeapon;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionThrust = item;
			#endregion AlbionThrust
			#region AlbionLeftThrust
			item = new ItemTemplate();
			item.Id_nb = "AlbionLeftThrust";
			item.Name = "Left Handed Thruster of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 2658;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 28;
			item.Object_Type = (int)eObjectType.ThrustWeapon;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionLeftThrust = item;
			#endregion AlbionLeftThrust
			#region AlbionCrushFlex
			item = new ItemTemplate();
			item.Id_nb = "AlbionCrushFlex";
			item.Name = "One Handed Crush Flex of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 859;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 35;
			item.Object_Type = 24;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionCrushFlex = item;
			#endregion AlbionCrushFlex
			#region AlbionSlashFlex
			item = new ItemTemplate();
			item.Id_nb = "AlbionSlashFlex";
			item.Name = "One Handed Slash Flex of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 859;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 35;
			item.Object_Type = 24;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionSlashFlex = item;
			#endregion AlbionSlashFlex
			#region AlbionThrustFlex
			item = new ItemTemplate();
			item.Id_nb = "AlbionThrustFlex";
			item.Name = "One Handed Thrust Flex of Albion";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 859;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 35;
			item.Object_Type = 24;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			AlbionThrustFlex = item;
			#endregion AlbionThrustFlex
			#endregion 1 Handed Albion Weapons
			#region 2 handed Hib Weapons
			#region HiberniaThrustSpear
			item = new ItemTemplate();
			item.Id_nb = "HiberniaThrustSpear";
			item.Name = "Thrust Spear of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 332;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 51;
			item.Object_Type = 23;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaTrustSpear = item;
			#endregion HiberniaTrustSpear
			#region HiberniaSlashSpear
			item = new ItemTemplate();
			item.Id_nb = "HiberniaSlashSpear";
			item.Name = "Slash Spear of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 332;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 51;
			item.Object_Type = 23;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaSlashSpear = item;
			#endregion HiberniaSlashSpear
			#region HiberniaSlashLargeWeapon
			item = new ItemTemplate();
			item.Id_nb = "HiberniaSlashLargeWeapon";
			item.Name = "Slash Large Weapon of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2690;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Slash;
			item.Realm = 3;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 45;
			item.Object_Type = 22;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaSlashLargeWeapon = item;
			#endregion HiberniaSlashLargeWeapon
			#region HiberniaCrushLargeWeapon
			item = new ItemTemplate();
			item.Id_nb = "HiberniaCrushLargeWeapon";
			item.Name = "Thrust Large Weapon of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2690;
			item.Hand = 1;
			item.Realm = 3;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 45;
			item.Object_Type = 22;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaCrushLargeWeapon = item;
			#endregion HiberniaCrushLargeWeapon
			#region HiberniaThrustLargeWeapon
			item = new ItemTemplate();
			item.Id_nb = "HiberniaThrustLargeWeapon";
			item.Name = "Thrust Large Weapon of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2690;
			item.Hand = 1;
			item.Realm = 3;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 45;
			item.Object_Type = 22;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaThrustLargeWeapon = item;
			#endregion HiberniaThrustLargeWeapon
			#region HiberniaScythe
			item = new ItemTemplate();
			item.Id_nb = "HiberniaScythe";
			item.Name = "Slash Scythe of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 927;
			item.Hand = 1;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 37;
			item.Object_Type = 26;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaScythe = item;
			#endregion HiberniaScythe
			#endregion 2 Handed Hib Weapons
			#region 1 handed Hib Weaspons
			#region HibernaBlade
			item = new ItemTemplate();
			item.Id_nb = "HibernaBlade";
			item.Name = "One Handed Blade of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 311;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 34;
			item.Object_Type = 19;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaBlade = item;
			#endregion HiberniaBlade
			#region HiberniaLeftBlade
			item = new ItemTemplate();
			item.Id_nb = "HiberniaLeftBlade";
			item.Name = "Left Handed Blade of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 311;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Slash;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 28;
			item.Object_Type = 19;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaLeftBlade = item;
			#endregion HiberniaLeftBlade
			#region HiberniaBlunt
			item = new ItemTemplate();
			item.Id_nb = "HiberniaBlunt";
			item.Name = "One Handed Blunt of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 323;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 34;
			item.Object_Type = 20;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaBlunt = item;
			#endregion HiberniaBlunt
			#region HiberniaLeftBlunt
			item = new ItemTemplate();
			item.Id_nb = "HiberniaLeftBlunt";
			item.Name = "One Handed Left Blunt of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 323;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Crush;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 28;
			item.Object_Type = 20;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaLeftBlunt = item;
			#endregion HiberniaLeftBlunt
			#region HiberniaPierce
			item = new ItemTemplate();
			item.Id_nb = "HiberniaPierce";
			item.Name = "One Handed Pierce of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.RightHandWeapon;
			item.Model = 2684;
			item.Hand = 0;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 34;
			item.Object_Type = 21;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaPierce = item;
			#endregion HiberniaPierce
			#region HiberniaLeftPierce
			item = new ItemTemplate();
			item.Id_nb = "HiberniaLeftPierce";
			item.Name = "One Left Handed Piercer of Hibernia";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 323;
			item.Hand = 2;
			item.Type_Damage = (int)eDamageType.Thrust;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 28;
			item.Object_Type = 21;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			HiberniaLeftPierce = item;
			#endregion HiberniaLeftPierce
			#endregion 1 handed Hib weapons
			#region Instruments
			#region EpicHarp
			item = new ItemTemplate();
			item.Id_nb = "EpicHarp";
			item.Name = "Epic Harp";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TwoHandWeapon;
			item.Model = 2116;
			item.Hand = 1;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 4;
			item.SPD_ABS = 40;
			item.Object_Type = (int)eObjectType.Instrument;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			EpicHarp = item;
			#endregion EpicHarp
			#endregion Instruments
			#region Shields
			#region EpicSmallShield
			item = new ItemTemplate();
			item.Id_nb = "EpicSmallShield";
			item.Name = "Cross Realm Small Shield";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 59;
			item.Hand = 2;
			item.Type_Damage = 1;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 40;
			item.Object_Type = 42;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			EpicSmallShield = item;
			#endregion EpicSmallShield
			#region EpicMediumShield
			item = new ItemTemplate();
			item.Id_nb = "EpicMediumShield";
			item.Name = "Cross Realm Medium Shield";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 61;
			item.Hand = 2;
			item.Type_Damage = 2;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 40;
			item.Object_Type = 42;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			EpicMediumShield = item;
			#endregion EpicMediumShield
			#region EpicLargeShield
			item = new ItemTemplate();
			item.Id_nb = "EpicLargeShield";
			item.Name = "Cross Realm Large Shield";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LeftHandWeapon;
			item.Model = 60;
			item.Hand = 2;
			item.Type_Damage = 3;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 162;
			item.SPD_ABS = 40;
			item.Object_Type = 42;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			EpicLargeShield = item;
			#endregion EpicLargeShield
			#endregion Shields
			#endregion Weapons
			#region Armour
			#region Albion Epic
			#region Wizard Epic

			item = new ItemTemplate();
			item.Id_nb = "WizardEpicBoots";
			item.Name = "Bernor's Numinous Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 143;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Cold;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			WizardEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "WizardEpicHelm";
			item.Name = "Bernor's Numinous Cap";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 21;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Spirit;
			WizardEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "WizardEpicGloves";
			item.Name = "Bernor's Numinous Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 142;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			WizardEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "WizardEpicVest";
			item.Name = "Bernor's Numinous Robes";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 798;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eResist.Cold;
			item.Bonus2 = 14;
			item.Bonus2Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus3 = 24;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			WizardEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "WizardEpicLegs";
			item.Name = "Bernor's Numinous Pants";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 140;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Fire;
			item.Bonus2 = 8;
			item.Bonus2Type = (int)eResist.Cold;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Energy;
			WizardEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "WizardEpicArms";
			item.Name = "Bernor's Numinous Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 141;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Earth;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 16;
			item.Bonus3Type = (int)eStat.INT;
			WizardEpicArms = item;

			#endregion Wizard Epic
			#region Minstrel Epic

			item = new ItemTemplate();
			item.Id_nb = "MinstrelEpicBoots";
			item.Name = "Boots of Coruscating Harmony";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 727;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 7;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 27;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Cold;
			MinstrelEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "MinstrelEpicHelm";
			item.Name = "Coif of Coruscating Harmony";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.CHR;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			MinstrelEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "MinstrelEpicGloves";
			item.Name = "Gauntlets of Coruscating Harmony";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 726;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 19;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			MinstrelEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "MinstrelEpicVest";
			item.Name = "Habergeon of Coruscating Harmony";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 723;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 6;
			item.Bonus1Type = (int)eResist.Cold;
			item.Bonus2 = 8;
			item.Bonus2Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus3 = 39;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Energy;
			MinstrelEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "MinstrelEpicLegs";
			item.Name = "Chaussess of Coruscating Harmony";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 724;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 19;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			MinstrelEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "MinstrelEpicArms";
			item.Name = "Sleeves of Coruscating Harmony";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 725;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 21;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Body;
			MinstrelEpicArms = item;

			#endregion Minstrel Epic
			#region Sorcerer Epic

			item = new ItemTemplate();
			item.Id_nb = "SorcererEpicBoots";
			item.Name = "Boots of Mental Acuity";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 143;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Matter;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			SorcererEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "SorcererEpicHelm";
			item.Name = "Cap of Mental Acuity";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 21;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Thrust;
			SorcererEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "SorcererEpicGloves";
			item.Name = "Gloves of Mental Acuity";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 142;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			SorcererEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "SorcererEpicVest";
			item.Name = "Vest of Mental Acuity";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 804;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eResist.Spirit;
			item.Bonus2 = 14;
			item.Bonus2Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus3 = 24;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			SorcererEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "SorcererEpicLegs";
			item.Name = "Pants of Mental Acuity";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 140;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Mind;
			item.Bonus2 = 19;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Spirit;
			SorcererEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "SorcererEpicArms";
			item.Name = "Sleeves of Mental Acuity";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 141;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Body;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 18;
			item.Bonus3Type = (int)eStat.INT;
			SorcererEpicArms = item;

			#endregion Sorcerer Epic
			#region Cleric Epic

			item = new ItemTemplate();
			item.Id_nb = "ClericEpicBoots";
			item.Name = "Boots of Defiant Soul";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 717;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.QUI; ;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Spirit;
			ClericEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ClericEpicHelm";
			item.Name = "Coif of Defiant Soul";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Enchantments;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 19;
			item.Bonus3Type = (int)eStat.PIE;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			ClericEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ClericEpicGloves";
			item.Name = "Gauntlets of Defiant Soul";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 716;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Smiting;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			ClericEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ClericEpicVest";
			item.Name = "Habergeon of Defiant Soul";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 713;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eResist.Crush;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eResist.Spirit;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus4 = 27;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ClericEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ClericEpicLegs";
			item.Name = "Chaussess of Defiant Soul";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 714;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Rejuvenation;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Cold;
			ClericEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "ClericEpicArms";
			item.Name = "Sleeves of Defiant Soul";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 715;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			ClericEpicArms = item;

			#endregion Cleric Epic
			#region Paladin Epic

			item = new ItemTemplate();
			item.Id_nb = "PaladinEpicBoots";
			item.Name = "Sabaton of the Iron Will";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 697;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 19;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Energy;
			PaladinEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "PaladinEpicHelm";
			item.Name = "Hounskull of the Iron Will";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 19;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Matter;
			PaladinEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "PaladinEpicGloves";
			item.Name = "Gauntlets of the Iron Will";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 696;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Heat;
			PaladinEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "PaladinEpicVest";
			item.Name = "Curiass of the Iron Will";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 693;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eResist.Body;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			PaladinEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "PaladinEpicLegs";
			item.Name = "Greaves of the Iron Will";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 694;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Cold;
			PaladinEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "PaladinEpicArms";
			item.Name = "Spaulders of the Iron Will";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 695;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 9;
			item.Bonus3Type = (int)eStat.QUI; ;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Spirit;
			PaladinEpicArms = item;

			#endregion Paladin Epic
			#region Mercenary Epic

			item = new ItemTemplate();
			item.Id_nb = "MercenaryEpicBoots";
			item.Name = "Boots of the Shadowy Embers";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 722;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 9;
			item.Bonus4Type = (int)eStat.STR;
			MercenaryEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "MercenaryEpicHelm";
			item.Name = "Coif of the Shadowy Embers";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Thrust;
			MercenaryEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "MercenaryEpicGloves";
			item.Name = "Gauntlets of the Shadowy Embers";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 721;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			MercenaryEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "MercenaryEpicVest";
			item.Name = "Haurberk of the Shadowy Embers";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 718;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 48;
			item.Bonus2Type = (int)eProperty.MaxHealth;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Thrust;
			MercenaryEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "MercenaryEpicLegs";
			item.Name = "Chausses of the Shadowy Embers";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 719;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Slash;
			MercenaryEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "MercenaryEpicArms";
			item.Name = "Sleeves of the Shadowy Embers";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 720;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 12;
			item.Bonus4Type = (int)eStat.QUI; ;
			MercenaryEpicArms = item;

			#endregion Mercenary Epic
			#region Reaver Epic

			item = new ItemTemplate();
			item.Id_nb = "ReaverEpicBoots";
			item.Name = "Boots of Murky Secrets";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 1270;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 14;
			item.Bonus1Type = (int)eProperty.MaxMana;
			item.Bonus2 = 9;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			ReaverEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ReaverEpicHelm";
			item.Name = "Coif of Murky Secrets";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.PIE;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eProperty.Skill_Flexible_Weapon;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Thrust;
			ReaverEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ReaverEpicGloves";
			item.Name = "Gauntlets of Murky Secrets";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 1271;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Crush;
			ReaverEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ReaverEpicVest";
			item.Name = "Hauberk of Murky Secrets";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 1267;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 48;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Thrust;
			ReaverEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ReaverEpicLegs";
			item.Name = "Chausses of Murky Secrets";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 1268;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Slash;
			ReaverEpicLegs = item;


			item = new ItemTemplate();
			item.Id_nb = "ReaverEpicArms";
			item.Name = "Sleeves of Murky Secrets";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 1269;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.Skill_Slashing;
			ReaverEpicArms = item;

			#endregion Reaver Epic
			#region Cabalist Epic

			item = new ItemTemplate();
			item.Id_nb = "CabalistEpicBoots";
			item.Name = "Warm Boots of the Construct";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 143;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Matter;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Thrust;
			CabalistEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "CabalistEpicHelm";
			item.Name = "Warm Coif of the Construct";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 21;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			CabalistEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "CabalistEpicGloves";
			item.Name = "Warm Gloves of the Construct";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 142;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eProperty.MaxMana;
			CabalistEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "CabalistEpicVest";
			item.Name = "Warm Robe of the Construct";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 682;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 24;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 14;
			item.Bonus2Type = (int)eProperty.MaxMana;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			CabalistEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "CabalistEpicLegs";
			item.Name = "Warm Leggings of the Construct";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 140;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Spirit;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			CabalistEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "CabalistEpicArms";
			item.Name = "Warm Sleeves of the Construct";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 141;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Body;
			item.Bonus3 = 16;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			CabalistEpicArms = item;

			#endregion Cabalist Epic
			#region Infiltrator Epic

			item = new ItemTemplate();
			item.Id_nb = "InfiltratorEpicBoots";
			item.Name = "Shadow-Woven Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 796;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.QUI; ;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 13;
			item.Bonus4Type = (int)eStat.CON;
			InfiltratorEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "InfiltratorEpicHelm";
			item.Name = "Shadow-Woven Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 13;
			item.Bonus4Type = (int)eStat.STR;
			InfiltratorEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "InfiltratorEpicGloves";
			item.Name = "Shadow-Woven Gloves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 795;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 21;
			item.Bonus2Type = (int)eProperty.MaxHealth;
			item.Bonus3 = 3;
			item.Bonus3Type = (int)eProperty.Skill_Envenom;
			item.Bonus4 = 3;
			item.Bonus4Type = (int)eProperty.Skill_Critical_Strike;
			InfiltratorEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "InfiltratorEpicVest";
			item.Name = "Shadow-Woven Jerkin";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 792;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 36;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Body;
			InfiltratorEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "InfiltratorEpicLegs";
			item.Name = "Shadow-Woven Leggings";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 793;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 21;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Crush;
			InfiltratorEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "InfiltratorEpicArms";
			item.Name = "Shadow-Woven Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 794;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 21;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eResist.Slash;
			InfiltratorEpicArms = item;

			#endregion Infiltrator Epic
			#region Necromancer Epic

			item = new ItemTemplate();
			item.Id_nb = "NecromancerEpicBoots";
			item.Name = "Boots of Forbidden Rites";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 143;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Pain_working;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Thrust;
			NecromancerEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "NecromancerEpicHelm";
			item.Name = "Cap of Forbidden Rites";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 21;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			NecromancerEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "NecromancerEpicGloves";
			item.Name = "Gloves of Forbidden Rites";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 142;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eProperty.MaxMana;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			NecromancerEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "NecromancerEpicVest";
			item.Name = "Robe of Forbidden Rites";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 1266;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 24;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 14;
			item.Bonus2Type = (int)eProperty.MaxMana;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			NecromancerEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "NecromancerEpicLegs";
			item.Name = "Leggings of Forbidden Rites";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 140;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Death_Servant;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			NecromancerEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "NecromancerEpicArms";
			item.Name = "Sleeves of Forbidden Rites";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 141;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_DeathSight;
			item.Bonus3 = 16;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			NecromancerEpicArms = item;

			#endregion Necromancer Epic
			#region Scout Epic


			item = new ItemTemplate();
			item.Id_nb = "ScoutEpicBoots";
			item.Name = "Brigandine Boots of Vigilant Defense";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 731;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.QUI; ;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Spirit;
			ScoutEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ScoutEpicHelm";
			item.Name = "Brigandine Coif of Vigilant Defense";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			ScoutEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ScoutEpicGloves";
			item.Name = "Brigandine Gloves of Vigilant Defense";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 732;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 21;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 5;
			item.Bonus2Type = (int)eProperty.Skill_Long_bows;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Slash;
			ScoutEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ScoutEpicVest";
			item.Name = "Brigandine Jerkin of Vigilant Defense";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 728;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eResist.Thrust;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 45;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ScoutEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ScoutEpicLegs";
			item.Name = "Brigandine Legs of Vigilant Defense";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 729;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 7;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Spirit;
			ScoutEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "ScoutEpicArms";
			item.Name = "Brigandine Sleeves of Vigilant Defense";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 730;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Energy;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eResist.Slash;
			ScoutEpicArms = item;

			#endregion Scout Epic
			#region Armsman Epic

			item = new ItemTemplate();
			item.Id_nb = "ArmsmanEpicBoots";
			item.Name = "Sabaton of the Stalwart Arm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 692;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Spirit;
			ArmsmanEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ArmsmanEpicHelm";
			item.Name = "Coif of the Stalwart Arm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Crush;
			ArmsmanEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ArmsmanEpicGloves";
			item.Name = "Gloves of the Stalwart Arm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 691;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Slash;
			ArmsmanEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ArmsmanEpicVest";
			item.Name = "Jerkin of the Stalwart Arm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 688;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eResist.Slash;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Energy;
			item.Bonus4 = 45;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ArmsmanEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ArmsmanEpicLegs";
			item.Name = "Legs of the Stalwart Arm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 689;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 24;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Crush;
			ArmsmanEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "ArmsmanEpicArms";
			item.Name = "Sleeves of the Stalwart Arm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 690;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 34;
			item.Object_Type = 36;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Thrust;
			ArmsmanEpicArms = item;

			#endregion Armsman Epic
			#region Friar Epic

			item = new ItemTemplate();
			item.Id_nb = "FriarEpicBoots";
			item.Name = "Prayer-bound Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 40;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.QUI; ;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.CON;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Spirit;
			FriarEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "FriarEpicHelm";
			item.Name = "Prayer-bound Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.CON;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.Skill_Enhancement; //guessing here
			FriarEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "FriarEpicGloves";
			item.Name = "Prayer-bound Gloves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 39;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.PIE;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI; ;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.Skill_Rejuvenation;
			FriarEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "FriarEpicVest";
			item.Name = "Prayer-bound Jerkin";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 797;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eProperty.MaxMana;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eResist.Crush;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			FriarEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "FriarEpicLegs";
			item.Name = "Prayer-bound Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 37;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 22;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Slash;
			FriarEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "FriarEpicArms";
			item.Name = "Prayer-bound Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 38;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.PIE;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Thrust;
			FriarEpicArms = item;

			#endregion Friar Epic
			#region Theurgist Epic

			item = new ItemTemplate();
			item.Id_nb = "TheurgistEpicBoots";
			item.Name = "Boots of Shielding Power";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 143;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 48;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eResist.Cold;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			TheurgistEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "TheurgistEpicHelm";
			item.Name = "Coif of Shielding Power";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1290; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 48;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 21;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Crush;
			TheurgistEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "TheurgistEpicGloves";
			item.Name = "Gloves of Shielding Power";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 142;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 48;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			TheurgistEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "TheurgistEpicVest";
			item.Name = "Jerkin of Shielding Power";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 733;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 48;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 24;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 14;
			item.Bonus2Type = (int)eProperty.MaxMana;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Cold;
			TheurgistEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "TheurgistEpicLegs";
			item.Name = "Legs of Shielding Power";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 140;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 48;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Wind;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Energy;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Cold;
			TheurgistEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "TheurgistEpicArms";
			item.Name = "Sleeves of Shielding Power";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 141;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 48;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Earth;
			item.Bonus3 = 16;
			item.Bonus3Type = (int)eStat.DEX;
			TheurgistEpicArms = item;

			#endregion Theurgist Epic
			#endregion Albion Epic
			#region Midgard Epic
			#region Spiritmaster Epic

			item = new ItemTemplate();
			item.Id_nb = "SpiritmasterEpicBoots";
			item.Name = "Spirit Touched Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 803;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Heat;
			SpiritmasterEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "SpiritmasterEpicHelm";
			item.Name = "Spirit Touched Cap";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 825; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Darkness;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Focus_Suppression;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.PIE;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			SpiritmasterEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "SpiritmasterEpicGloves";
			item.Name = "Spirit Touched Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 802;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Summoning;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.PIE;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			SpiritmasterEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "SpiritmasterEpicVest";
			item.Name = "Spirit Touched Vest";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 799;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			SpiritmasterEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "SpiritmasterEpicLegs";
			item.Name = "Spirit Touched Pants";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 800;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			SpiritmasterEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "SpiritmasterEpicArms";
			item.Name = "Spirit Touched Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 801;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 9;
			item.Bonus1Type = (int)eStat.PIE;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eResist.Thrust;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			SpiritmasterEpicArms = item;

			#endregion Spiritmaster Epic
			#region Runemaster Epic

			item = new ItemTemplate();
			item.Id_nb = "RunemasterEpicBoots";
			item.Name = "Raven-Rune Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 707;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Heat;
			RunemasterEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "RunemasterEpicHelm";
			item.Name = "Raven-Rune Cap";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 825; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Darkness;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Focus_Suppression;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.PIE;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			RunemasterEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "RunemasterEpicGloves";
			item.Name = "Raven-Rune Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 706;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Summoning;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.PIE;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			RunemasterEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "RunemasterEpicVest";
			item.Name = "Raven-Rune Vest";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 703;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			RunemasterEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "RunemasterEpicLegs";
			item.Name = "Raven-Rune Pants";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 704;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			RunemasterEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "RunemasterEpicArms";
			item.Name = "Raven-Rune Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 705;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 9;
			item.Bonus1Type = (int)eStat.PIE;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eResist.Thrust;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			RunemasterEpicArms = item;

			#endregion Runemaster Epic
			#region Bonedancer Epic

			item = new ItemTemplate();
			item.Id_nb = "BonedancerEpicBoots";
			item.Name = "Raven-Boned Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 1190;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Heat;
			BonedancerEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "BonedancerEpicHelm";
			item.Name = "Raven-Boned Cap";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 825; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Suppression;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.Focus_BoneArmy;
			BonedancerEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "BonedancerEpicGloves";
			item.Name = "Raven-Boned Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 1191;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Darkness;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.PIE;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			BonedancerEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "BonedancerEpicVest";
			item.Name = "Raven-Boned Vest";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 1187;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			BonedancerEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "BonedancerEpicLegs";
			item.Name = "Raven-Boned Pants";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 1188;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			BonedancerEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "BonedancerEpicArms";
			item.Name = "Raven-Boned Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 1189;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 9;
			item.Bonus1Type = (int)eStat.PIE;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eResist.Thrust;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			BonedancerEpicArms = item;

			#endregion Bonedancer Epic
			#region Healer Epic
			item = new ItemTemplate();
			item.Id_nb = "HealerEpicBoots";
			item.Name = "Valhalla Touched Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 702;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 21;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			HealerEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "HealerEpicHelm";
			item.Name = "Valhalla Touched Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 63; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Augmentation;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			HealerEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "HealerEpicGloves";
			item.Name = "Valhalla Touched Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 701;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Mending;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			HealerEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "HealerEpicVest";
			item.Name = "Valhalla Touched Haukberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 698;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Heat;
			HealerEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "HealerEpicLegs";
			item.Name = "Valhalla Touched Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 699;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			HealerEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "HealerEpicArms";
			item.Name = "Valhalla Touched Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 700;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Mending;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.PIE;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Matter;
			HealerEpicArms = item;
			#endregion Healer Epic
			#region Shaman Epic
			item = new ItemTemplate();
			item.Id_nb = "ShamanEpicBoots";
			item.Name = "Subterranean Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 770;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 39;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			ShamanEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ShamanEpicHelm";
			item.Name = "Subterranean Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 63; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Mending;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			ShamanEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ShamanEpicGloves";
			item.Name = "Subterranean Gloves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 769;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Subterranean;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			ShamanEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ShamanEpicVest";
			item.Name = "Subterranean Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 766;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			ShamanEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ShamanEpicLegs";
			item.Name = "Subterranean Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 767;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Spirit;
			ShamanEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "ShamanEpicArms";
			item.Name = "Subterranean Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 768;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Augmentation;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 18;
			item.Bonus3Type = (int)eStat.PIE;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Energy;
			ShamanEpicArms = item;
			#endregion Shaman Epic
			#region Hunter Epic
			item = new ItemTemplate();
			item.Id_nb = "HunterEpicBoots";
			item.Name = "Call of the Hunt Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 760;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 19;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Thrust;
			HunterEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "HunterEpicHelm";
			item.Name = "Call of the Hunt Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 829; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Spear;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Stealth;
			item.Bonus3 = 3;
			item.Bonus3Type = (int)eProperty.Skill_Composite;
			item.Bonus4 = 19;
			item.Bonus4Type = (int)eStat.DEX;
			HunterEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "HunterEpicGloves";
			item.Name = "Call of the Hunt Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 759;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 5;
			item.Bonus1Type = (int)eProperty.Skill_Composite;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 33;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			HunterEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "HunterEpicVest";
			item.Name = "Call of the Hunt Jerkin";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 756;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Cold;
			HunterEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "HunterEpicLegs";
			item.Name = "Call of the Hunt Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 757;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 7;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 12;
			item.Bonus4Type = (int)eResist.Matter;
			HunterEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "HunterEpicArms";
			item.Name = "Call of the Hunt Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 758;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Slash;
			HunterEpicArms = item;
			#endregion Hunter Epic
			#region Shadowblade Epic
			item = new ItemTemplate();
			item.Id_nb = "ShadowbladeEpicBoots";
			item.Name = "Shadow Shrouded Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 765;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 5;
			item.Bonus1Type = (int)eProperty.Skill_Stealth;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Heat;
			ShadowbladeEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ShadowbladeEpicHelm";
			item.Name = "Shadow Shrouded Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 335; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eStat.QUI;
			ShadowbladeEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ShadowbladeEpicGloves";
			item.Name = "Shadow Shrouded Gloves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 764;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 2;
			item.Bonus1Type = (int)eProperty.Skill_Critical_Strike;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 33;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.Skill_Envenom;
			ShadowbladeEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ShadowbladeEpicVest";
			item.Name = "Shadow Shrouded Jerkin";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 761;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 30;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Heat;
			ShadowbladeEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ShadowbladeEpicLegs";
			item.Name = "Shadow Shrouded Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 762;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Slash;
			ShadowbladeEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "ShadowbladeEpicArms";
			item.Name = "Shadow Shrouded Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 763;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Thrust;
			ShadowbladeEpicArms = item;
			#endregion Shadowblade Epic
			#region Warrior Epic
			item = new ItemTemplate();
			item.Id_nb = "WarriorEpicBoots";
			item.Name = "Tyr's Might Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 780;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Durability = 50000;
			item.Condition = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			WarriorEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "WarriorEpicHelm";
			item.Name = "Tyr's Might Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 832; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 11;
			item.Bonus4Type = (int)eResist.Crush;
			WarriorEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "WarriorEpicGloves";
			item.Name = "Tyr's Might Gloves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 779;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Shields;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Parry;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.STR;
			item.Bonus4 = 13;
			item.Bonus4Type = (int)eStat.DEX;
			WarriorEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "WarriorEpicVest";
			item.Name = "Tyr's Might Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 776;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			WarriorEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "WarriorEpicLegs";
			item.Name = "Tyr's Might Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 777;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Body;
			WarriorEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "WarriorEpicArms";
			item.Name = "Tyr's Might Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 778;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 22;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Slash;
			WarriorEpicArms = item;
			#endregion Warrior Epic
			#region Berserker Epic
			item = new ItemTemplate();
			item.Id_nb = "BerserkerEpicBoots";
			item.Name = "Courage Bound Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 755;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			BerserkerEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "BerserkerEpicHelm";
			item.Name = "Courage Bound Helm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 829; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eStat.QUI;
			BerserkerEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "BerserkerEpicGloves";
			item.Name = "Courage Bound Gloves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 754;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Left_Axe;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Parry;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.STR;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			BerserkerEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "BerserkerEpicVest";
			item.Name = "Courage Bound Jerkin";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 751;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			BerserkerEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "BerserkerEpicLegs";
			item.Name = "Courage Bound Leggings";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 752;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 7;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 12;
			item.Bonus4Type = (int)eResist.Slash;
			BerserkerEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "BerserkerEpicArms";
			item.Name = "Courage Bound Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 753;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			BerserkerEpicArms = item;
			#endregion Berserker Epic
			#region Thane Epic
			item = new ItemTemplate();
			item.Id_nb = "ThaneEpicBoots";
			item.Name = "Storm Touched Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 791;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			ThaneEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ThaneEpicHelm";
			item.Name = "Storm Touched Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 832; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Stormcalling;
			item.Bonus2 = 18;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			ThaneEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ThaneEpicGloves";
			item.Name = "Storm Touched Gloves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 790;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Sword;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Hammer;
			item.Bonus3 = 3;
			item.Bonus3Type = (int)eProperty.Skill_Axe;
			item.Bonus4 = 19;
			item.Bonus4Type = (int)eStat.STR;
			ThaneEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ThaneEpicVest";
			item.Name = "Storm Touched Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 787;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ThaneEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ThaneEpicLegs";
			item.Name = "Storm Touched Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 788;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.PIE;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			ThaneEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "ThaneEpicArms";
			item.Name = "Storm Touched Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 789;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Body;
			ThaneEpicArms = item;

			#endregion Thane Epic
			#region Skald Epic
			item = new ItemTemplate();
			item.Id_nb = "SkaldEpicBoots";
			item.Name = "Battlesung Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 775;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			SkaldEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "SkaldEpicHelm";
			item.Name = "Battlesung Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 832; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 5;
			item.Bonus1Type = (int)eProperty.Skill_Battlesongs;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CHR;
			item.Bonus3 = 33;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			SkaldEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "SkaldEpicGloves";
			item.Name = "Battlesung Gloves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 774;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			SkaldEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "SkaldEpicVest";
			item.Name = "Battlesung Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 771;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.CHR;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			SkaldEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "SkaldEpicLegs";
			item.Name = "Battlesung Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 772;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 27;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			SkaldEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "SkaldEpicArms";
			item.Name = "Battlesung Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 773;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 35;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Cold;
			SkaldEpicArms = item;
			#endregion Skald Epic
			#region Savage Epic
			item = new ItemTemplate();
			item.Id_nb = "SavageEpicBoots";
			item.Name = "Kelgor's Battle Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 1196;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 19;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Energy;
			SavageEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "SavageEpicHelm";
			item.Name = "Kelgor's Battle Helm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 824; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eStat.QUI;
			SavageEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "SavageEpicGloves";
			item.Name = "Kelgor's Battle Gauntlets";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 1195;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Parry;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 33;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 3;
			item.Bonus4Type = (int)eProperty.Skill_HandToHand;
			SavageEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "SavageEpicVest";
			item.Name = "Kelgor's Battle Vest";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 1192;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			SavageEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "SavageEpicLegs";
			item.Name = "Kelgor's Battle Leggings";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 1193;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eResist.Heat;
			item.Bonus2 = 7;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 15;
			item.Bonus4Type = (int)eStat.QUI;
			SavageEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "SavageEpicArms";
			item.Name = "Kelgor's Battle Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 1194;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 8;
			item.Bonus3 = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			SavageEpicArms = item;
			#endregion Savage Epic
			#endregion Midgard Epic
			#region Hibernia Epic
			#region Bard Epic
			item = new ItemTemplate();
			item.Id_nb = "BardEpicBoots";
			item.Name = "Moonsung Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 738;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.QUI;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eResist.Matter;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			BardEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "BardEpicHelm";
			item.Name = "Moonsung Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.CHR;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus3 = 3;
			item.Bonus3Type = (int)eProperty.Skill_Regrowth;
			item.Bonus4 = 21;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			BardEpicHelm = item;


			item = new ItemTemplate();
			item.Id_nb = "BardEpicGloves";
			item.Name = "Moonsung Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 737;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Nurture;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Music;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			BardEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "BardEpicVest";
			item.Name = "Moonsung Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 734;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Regrowth;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Nurture;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.CON;
			item.Bonus4 = 15;
			item.Bonus4Type = (int)eStat.CHR;
			BardEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "BardEpicLegs";
			item.Name = "Moonsung Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 735;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Matter;
			BardEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "BardEpicArms";
			item.Name = "Moonsung Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 736;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.CHR;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.CON;
			item.Bonus4 = 12;
			item.Bonus4Type = (int)eResist.Energy;
			BardEpicArms = item;
			#endregion Bard Epic
			#region Champion Epic
			item = new ItemTemplate();
			item.Id_nb = "ChampionEpicBoots";
			item.Name = "Moonglow Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 814;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 33;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eResist.Heat;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 15;
			item.Bonus4Type = (int)eStat.DEX;
			ChampionEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ChampionEpicHelm";
			item.Name = "Moonglow Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Valor;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.PowerRegenerationRate;
			ChampionEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ChampionEpicGloves";
			item.Name = "Moonglow Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 813;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Parry;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Crush;
			ChampionEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ChampionEpicVest";
			item.Name = "Moonglow Brestplate";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 810;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Valor;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			ChampionEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ChampionEpicLegs";
			item.Name = "Moonglow Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 811;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 18;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ChampionEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "ChampionEpicArms";
			item.Name = "Moonglow Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 812;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Large_Weapon;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ChampionEpicArms = item;
			#endregion Champion Epic
			#region Nightshade Epic
			item = new ItemTemplate();
			item.Id_nb = "NightshadeEpicBoots";
			item.Name = "Moonlit Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 750;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			NightshadeEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "NightshadeEpicHelm";
			item.Name = "Moonlit Helm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 9;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 9;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 9;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 39;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			NightshadeEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "NightshadeEpicGloves";
			item.Name = "Moonlit Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 749;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 2;
			item.Bonus1Type = (int)eProperty.Skill_Critical_Strike;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 5;
			item.Bonus4Type = (int)eProperty.Skill_Envenom;
			NightshadeEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "NightshadeEpicVest";
			item.Name = "Moonlit Leather Jerking";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 746;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 30;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			NightshadeEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "NightshadeEpicLegs";
			item.Name = "Moonlit Leggings";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 747;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Slash;
			NightshadeEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "NightshadeEpicArms";
			item.Name = "Moonlit Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 748;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 10;
			item.Object_Type = 33;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Celtic_Dual;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eResist.Cold;
			NightshadeEpicArms = item;
			#endregion Nightshade Epic
			#region Enchanter Epic

			item = new ItemTemplate();
			item.Id_nb = "EnchanterEpicBoots";
			item.Name = "Moonspun Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 382;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 39;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			EnchanterEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "EnchanterEpicHelm";
			item.Name = "Moonspun Cap";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1298; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 21;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 8;
			item.Bonus2Type = (int)eResist.Energy;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eProperty.Skill_Enchantments;
			item.Bonus4 = 18;
			item.Bonus4Type = (int)eStat.INT;
			EnchanterEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "EnchanterEpicGloves";
			item.Name = "Moonspun Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 381;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 30;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Mana;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eStat.INT;
			item.Bonus4 = 13;
			item.Bonus4Type = (int)eStat.DEX;
			EnchanterEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "EnchanterEpicVest";
			item.Name = "Moonspun Vest";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 781;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 30;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.DEX;
			EnchanterEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "EnchanterEpicLegs";
			item.Name = "Moonspun Pants";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 379;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Cold;
			EnchanterEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "EnchanterEpicArms";
			item.Name = "Moonspun Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 380;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 27;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 5;
			item.Bonus3Type = (int)eProperty.Skill_Light;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eStat.DEX;
			EnchanterEpicArms = item;

			#endregion Enchanter Epic
			#region Ranger Epic
			item = new ItemTemplate();
			item.Id_nb = "RangerEpicBoots";
			item.Name = "Mist Shrouded Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 819;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			RangerEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "RangerEpicHelm";
			item.Name = "Mist Shrouded Helm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 19;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eResist.Spirit;
			item.Bonus3 = 27;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			RangerEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "RangerEpicGloves";
			item.Name = "Mist Shrouded Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 818;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_RecurvedBow;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Crush;
			RangerEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "RangerEpicVest";
			item.Name = "Mist Shrouded Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 815;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 7;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 7;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 7;
			item.Bonus3Type = (int)eStat.QUI;
			item.Bonus4 = 48;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			RangerEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "RangerEpicLegs";
			item.Name = "Mist Shrouded Leggings";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 816;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 39;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			RangerEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "RangerEpicArms";
			item.Name = "Mist Shrouded Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 817;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			RangerEpicArms = item;
			#endregion Ranger Epic
			#region Hero Epic
			item = new ItemTemplate();
			item.Id_nb = "HeroEpicBoots";
			item.Name = "Misted Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 712;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			HeroEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "HeroEpicHelm";
			item.Name = "Misted Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 8;
			item.Bonus2Type = (int)eResist.Spirit;
			item.Bonus3 = 48;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			HeroEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "HeroEpicGloves";
			item.Name = "Misted Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 711;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 2;
			item.Bonus1Type = (int)eProperty.Skill_Shields;
			item.Bonus2 = 2;
			item.Bonus2Type = (int)eProperty.Skill_Parry;
			item.Bonus3 = 16;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 18;
			item.Bonus4Type = (int)eStat.QUI;
			HeroEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "HeroEpicVest";
			item.Name = "Misted Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 708;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.DEX;
			HeroEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "HeroEpicLegs";
			item.Name = "Misted Leggings";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 709;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 21;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Heat;
			HeroEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "HeroEpicArms";
			item.Name = "Misted Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 710;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 24;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Spirit;
			HeroEpicArms = item;
			#endregion Hero Epic
			#region Warden Epic
			item = new ItemTemplate();
			item.Id_nb = "WardenEpicBoots";
			item.Name = "Mystical Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 809;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Matter;
			WardenEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "WardenEpicHelm";
			item.Name = "Mystical Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.EMP;
			item.Bonus2 = 2;
			item.Bonus2Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus3 = 30;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.Skill_Regrowth;
			WardenEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "WardenEpicGloves";
			item.Name = "Mystical Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 808;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Skill_Nurture;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eResist.Slash;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			WardenEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "WardenEpicVest";
			item.Name = "Mystical Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 805;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 9;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 9;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 9;
			item.Bonus3Type = (int)eStat.EMP;
			item.Bonus2 = 39;
			item.Bonus2Type = (int)eProperty.MaxHealth;
			WardenEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "WardenEpicLegs";
			item.Name = "Mystical Legs";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 806;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			WardenEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "WardenEpicArms";
			item.Name = "Mystical Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 807;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 34;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.STR;
			item.Bonus2 = 8;
			item.Bonus2Type = (int)eResist.Matter;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 45;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			WardenEpicArms = item;
			#endregion Hibernia Epic
			#region Eldritch Epic
			item = new ItemTemplate();
			item.Id_nb = "EldritchEpicBoots";
			item.Name = "Mistwoven Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 382;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 9;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 9;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus4 = 21;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			EldritchEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "EldritchEpicHelm";
			item.Name = "Mistwoven Cap";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1298; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eResist.Heat;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eResist.Spirit;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eProperty.Focus_Void;
			item.Bonus4 = 19;
			item.Bonus4Type = (int)eStat.INT;
			EldritchEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "EldritchEpicGloves";
			item.Name = "Mistwoven Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 381;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Light;
			item.Bonus2 = 9;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eProperty.PowerRegenerationRate;
			item.Bonus4 = 24;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			EldritchEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "EldritchEpicVest";
			item.Name = "Mistwoven Vest";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 378;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 33;
			item.Bonus3 = (int)eProperty.MaxHealth;
			EldritchEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "EldritchEpicLegs";
			item.Name = "Mistwoven Pants";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 379;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eResist.Cold;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eResist.Body;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 16;
			item.Bonus4Type = (int)eStat.CON;
			EldritchEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "EldritchEpicArms";
			item.Name = "Mistwoven Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 380;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 4;
			item.Bonus1Type = (int)eProperty.Focus_Mana;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.INT;
			item.Bonus4 = 27;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			EldritchEpicArms = item;
			#endregion Eldritch Epic
			#region Druid Epic
			item = new ItemTemplate();
			item.Id_nb = "DruidEpicBoots";
			item.Name = "Sidhe Scale Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 743;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 9;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 9;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 14;
			item.Bonus3Type = (int)eResist.Body;
			item.Bonus4 = 36;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			DruidEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "DruidEpicHelm";
			item.Name = "Sidhe Scale Coif";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.EMP;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Nurture;
			item.Bonus3 = 3;
			item.Bonus3Type = (int)eProperty.Skill_Nature;
			item.Bonus4 = 27;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			DruidEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "DruidEpicGloves";
			item.Name = "Sidhe Scale Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 742;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Regrowth;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eProperty.MaxMana;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 12;
			item.Bonus4Type = (int)eStat.EMP;
			DruidEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "DruidEpicVest";
			item.Name = "Sidhe Scale Breastplate";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 739;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.EMP;
			item.Bonus2 = 3;
			item.Bonus2Type = (int)eProperty.Skill_Nature;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Slash;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			DruidEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "DruidEpicLegs";
			item.Name = "Sidhe Scale Leggings";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 740;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 57;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 8;
			item.Bonus2Type = (int)eResist.Crush;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Spirit;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Cold;
			DruidEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "DruidEpicArms";
			item.Name = "Sidhe Scale Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 741;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 27;
			item.Object_Type = 38;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 13;
			item.Bonus3Type = (int)eStat.EMP;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Matter;
			DruidEpicArms = item;
			#endregion Druid Epic
			#region Blademaster Epic
			item = new ItemTemplate();
			item.Id_nb = "BlademasterEpicBoots";
			item.Name = "Sidhe Studded Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 786;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.QUI;
			item.Bonus3 = 24;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Cold;
			BlademasterEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "BlademasterEpicHelm";
			item.Name = "Sidhe Studded Helm";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 30;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eResist.Spirit;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 16;
			item.Bonus4Type = (int)eStat.QUI;
			BlademasterEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "BlademasterEpicGloves";
			item.Name = "Sidhe Studded Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 785;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 3;
			item.Bonus3Type = (int)eProperty.Skill_Celtic_Dual;
			item.Bonus4 = 3;
			item.Bonus4Type = (int)eProperty.Skill_Parry;
			BlademasterEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "BlademasterEpicVest";
			item.Name = "Sidhe Studded Hauberk";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 782;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 33;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Slash;
			BlademasterEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "BlademasterEpicLegs";
			item.Name = "Sidhe Studded Leggings";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 783;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.QUI;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 27;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 12;
			item.Bonus4Type = (int)eResist.Cold;
			BlademasterEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "BlademasterEpicArms";
			item.Name = "Sidhe Studded Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 784;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 100;
			item.SPD_ABS = 19;
			item.Object_Type = 37;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 16;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Heat;
			BlademasterEpicArms = item;
			#endregion Blademaster Epic
			#region Animist Epic
			item = new ItemTemplate();
			item.Id_nb = "AnimistEpicBoots";
			item.Name = "Brightly Woven Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 382;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 27;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 12;
			item.Bonus4Type = (int)eResist.Matter;
			AnimistEpicBoots = item;

			item.Id_nb = "AnimistEpicHelm";
			item.Name = "Brightly Woven Cap";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Arboreal;
			item.Bonus3 = 21;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Thrust;
			AnimistEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "AnimistEpicGloves";
			item.Name = "Brightly Woven Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 381;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 9;
			item.Bonus2Type = (int)eStat.INT;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eProperty.Skill_Creeping;
			item.Bonus4 = 30;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			AnimistEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "AnimistEpicVest";
			item.Name = "Brightly Woven Robe";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 1186;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 30;
			item.Bonus2Type = (int)eProperty.MaxHealth;
			item.Bonus3 = 6;
			item.Bonus3Type = (int)eProperty.MaxMana;
			item.Bonus4 = 8;
			item.Bonus4Type = (int)eResist.Body;
			AnimistEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "AnimistEpicLegs";
			item.Name = "Brightly Woven Pants";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 379;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Body;
			AnimistEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "AnimistEpicArms";
			item.Name = "Brightly Woven Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 380;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 27;
			item.Bonus2Type = (int)eProperty.MaxHealth;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.INT;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.Skill_Mana;
			AnimistEpicArms = item;
			#endregion Animist Epic
			#region Mentalist Epic

			item = new ItemTemplate();
			item.Id_nb = "MentalistEpicBoots";
			item.Name = "Sidhe Woven Boots";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 382;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 12;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eResist.Matter;
			item.Bonus4 = 27;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			MentalistEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "MentalistEpicHelm";
			item.Name = "Sidhe Woven Cap";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1298; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 18;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Mentalism;
			item.Bonus3 = 8;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 21;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			MentalistEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "MentalistEpicGloves";
			item.Name = "Sidhe Woven Gloves ";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 381;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 30;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 4;
			item.Bonus2Type = (int)eProperty.Skill_Light;
			item.Bonus3 = 9;
			item.Bonus3Type = (int)eStat.INT;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eStat.DEX;
			MentalistEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "MentalistEpicVest";
			item.Name = "Sidhe Woven Vest";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 745;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 8;
			item.Bonus2Type = (int)eResist.Body;
			item.Bonus3 = 30;
			item.Bonus3Type = (int)eProperty.MaxHealth;
			item.Bonus4 = 6;
			item.Bonus4Type = (int)eProperty.MaxMana;
			MentalistEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "MentalistEpicLegs";
			item.Name = "Sidhe Woven Pants";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 379;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 16;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Cold;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Body;
			MentalistEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "MentalistEpicArms";
			item.Name = "Sidhe Woven Sleeves";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 380;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 10;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 27;
			item.Bonus2Type = (int)eProperty.MaxHealth;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.INT;
			item.Bonus4 = 4;
			item.Bonus4Type = (int)eProperty.Skill_Mana;
			MentalistEpicArms = item;
			#endregion Mentalist Epic
			#region Valewalker Epic
			item = new ItemTemplate();
			item.Id_nb = "ValewalkerEpicBoots";
			item.Name = "Boots of the Misty Glade";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.FeetArmor;
			item.Model = 382;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 12;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eResist.Matter;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ValewalkerEpicBoots = item;

			item = new ItemTemplate();
			item.Id_nb = "ValewalkerEpicHelm";
			item.Name = "Cap of the Misty Glade";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HeadArmor;
			item.Model = 1292; //NEED TO WORK ON..
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Arboreal;
			item.Bonus2 = 6;
			item.Bonus2Type = (int)eProperty.MaxMana;
			item.Bonus3 = 12;
			item.Bonus3Type = (int)eStat.CON;
			item.Bonus4 = 12;
			item.Bonus4Type = (int)eStat.INT;
			ValewalkerEpicHelm = item;

			item = new ItemTemplate();
			item.Id_nb = "ValewalkerEpicGloves";
			item.Name = "Gloves of the Misty Glades";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.HandsArmor;
			item.Model = 381;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Parry;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 15;
			item.Bonus3Type = (int)eStat.DEX;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Crush;
			ValewalkerEpicGloves = item;

			item = new ItemTemplate();
			item.Id_nb = "ValewalkerEpicVest";
			item.Name = "Robe of the Misty Glade";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.TorsoArmor;
			item.Model = 1003;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 13;
			item.Bonus1Type = (int)eStat.INT;
			item.Bonus2 = 13;
			item.Bonus2Type = (int)eStat.STR;
			item.Bonus3 = 4;
			item.Bonus3Type = (int)eProperty.Skill_Arboreal;
			item.Bonus4 = 10;
			item.Bonus4Type = (int)eResist.Energy;
			ValewalkerEpicVest = item;

			item = new ItemTemplate();
			item.Id_nb = "ValewalkerEpicLegs";
			item.Name = "Pants of the Misty Glade";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.LegsArmor;
			item.Model = 379;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 15;
			item.Bonus1Type = (int)eStat.DEX;
			item.Bonus2 = 15;
			item.Bonus2Type = (int)eStat.CON;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eResist.Crush;
			item.Bonus4 = 18;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ValewalkerEpicLegs = item;

			item = new ItemTemplate();
			item.Id_nb = "ValewalkerEpicArms";
			item.Name = "Sleeves of the Misty Glade";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.ArmsArmor;
			item.Model = 380;
			item.IsDropable = true;
			item.IsPickable = true;
			item.DPS_AF = 50;
			item.SPD_ABS = 0;
			item.Object_Type = 32;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 3;
			item.Bonus1Type = (int)eProperty.Skill_Scythe;
			item.Bonus2 = 10;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 10;
			item.Bonus3Type = (int)eStat.INT;
			item.Bonus4 = 33;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			ValewalkerEpicArms = item;

			#endregion Valewalker Epic

			#endregion Armour

			#endregion Armour/Equip
			#region Accessories
			#region bracer 1
			item = new ItemTemplate();
			item.Id_nb = "EpicBracer1";
			item.Name = "Epic Bracer";
			item.Level = 50;
			item.Item_Type = 33;
			item.Model = 598;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 0;
			item.Object_Type = 41;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 75;
			item.Bonus1Type = (int)eStat.CON;
			item.Bonus2 = 75;
			item.Bonus2Type = (int)eStat.DEX;
			item.Bonus3 = 75;
			item.Bonus3Type = (int)eStat.INT;
			item.Bonus4 = 75;
			item.Bonus4Type = (int)eProperty.MaxHealth;
			EpicBracer1 = item;
			#endregion bracer 1
			#region bracer 2
			item = new ItemTemplate();
			item.Id_nb = "EpicBracer2";
			item.Name = "Epic Bracer";
			item.Level = 50;
			item.Item_Type = 33;
			item.Model = 598;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 0;
			item.Object_Type = 41;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 20;
			item.Bonus1Type = (int)eResist.Crush;
			item.Bonus2 = 20;
			item.Bonus2Type = (int)eResist.Slash;
			item.Bonus3 = 20;
			item.Bonus3Type = (int)eResist.Thrust;
			item.Bonus4 = 20;
			item.Bonus4Type = (int)eResist.Body;
			EpicBracer2 = item;
			#endregion bracer 2
			#region Ring1
			item = new ItemTemplate();
			item.Id_nb = "EpicRing1";
			item.Name = "Epic Ring";
			item.Level = 50;
			item.Item_Type = 35;
			item.Model = 103;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 0;
			item.Object_Type = 41;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 20;
			item.Bonus1Type = (int)eResist.Cold;
			item.Bonus2 = 20;
			item.Bonus2Type = (int)eResist.Energy;
			item.Bonus3 = 20;
			item.Bonus3Type = (int)eResist.Heat;
			item.Bonus4 = 20;
			item.Bonus4Type = (int)eResist.Matter;
			EpicRing1 = item;
			#endregion Ring 1
			#region Ring2
			item = new ItemTemplate();
			item.Id_nb = "EpicRing2";
			item.Name = "Epic Ring";
			item.Level = 50;
			item.Item_Type = 35;
			item.Model = 103;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 0;
			item.Object_Type = 41;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 50;
			item.Bonus1Type = (int)eProperty.Quickness;
			item.Bonus2 = 50;
			item.Bonus2Type = (int)eProperty.Strength;
			item.Bonus3 = 50;
			item.Bonus3Type = (int)eProperty.Piety;
			item.Bonus4 = 20;
			item.Bonus4Type = (int)eResist.Spirit;
			EpicRing2 = item;
			#endregion Ring 2
			#region Jewell
			item = new ItemTemplate();
			item.Id_nb = "EpicJewell";
			item.Name = "Epic Jewell";
			item.Level = 50;
			item.Item_Type = 24;
			item.Model = 115;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 0;
			item.Object_Type = 41;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 150;
			item.Bonus1Type = (int)eProperty.MaxHealth;
			item.Bonus2 = 150;
			item.Bonus2Type = (int)eProperty.MaxMana;
			item.Bonus3 = 11;
			item.Bonus3Type = (int)eProperty.AllMagicSkills;
			item.Bonus4 = 50;
			item.Bonus4Type = (int)eProperty.AllFocusLevels;
			EpicJewell = item;
			#endregion Jewell
			#region Necklace
			item = new ItemTemplate();
			item.Id_nb = "EpicNecklace";
			item.Name = "Epic Necklace";
			item.Level = 50;
			item.Item_Type = 29;
			item.Model = 101;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 0;
			item.Object_Type = 41;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			item.Bonus1 = 11;
			item.Bonus1Type = (int)eProperty.AllMeleeWeaponSkills;
			item.Bonus2 = 11;
			item.Bonus2Type = (int)eProperty.AllDualWieldingSkills;
			item.Bonus3 = 11;
			item.Bonus3Type = (int)eProperty.AllArcherySkills;
			item.Bonus4 = 11;
			item.Bonus4Type = (int)eProperty.AllSkills;
			EpicNecklace = item;
			#endregion Necklace
			#region EpicCasterCloak
			item = new ItemTemplate();
			item.Id_nb = "EpicCasterCloak";
			item.Name = "Epic Caster Cloak";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.Cloak;
			item.Model = 57;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 0;
			item.Object_Type = 41;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.IsTradable = false;
			item.Durability = 50000;
			item.Bonus1 = 2;
			item.Bonus1Type = (int)eProperty.PowerRegenerationRate;
			EpicCasterCloak = item;
			#endregion EpicCasterCloak
			#region EpicMeleeCloak
			item = new ItemTemplate();
			item.Id_nb = "EpicMeleeCloak";
			item.Name = "Epic Melee Cloak";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.Cloak;
			item.Model = 57;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 0;
			item.Object_Type = 41;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.IsTradable = false;
			item.Durability = 50000;
			item.Bonus1 = 2;
			item.Bonus1Type = (int)eProperty.EnduranceRegenerationRate;
			EpicMeleeCloak = item;
			#endregion EpicMeleeCloak
			#endregion Accessories
			#region Other Free
			#region Epic Mount
			item = new ItemTemplate();
			item.Id_nb = "EpicMount";
			item.Name = "Epic Mount";
			item.Level = 50;
			item.Item_Type = (int)eInventorySlot.Horse;
			item.Model = 2912;
			item.IsDropable = true;
			item.IsPickable = true;
			item.SPD_ABS = 1;
			item.Object_Type = (int)eObjectType.Magical;
			item.Quality = 100;
			
			item.Weight = 22;
			item.Bonus = 35;
			item.MaxCondition = 50000;
			item.MaxDurability = 50000;
			item.Condition = 50000;
			item.Durability = 50000;
			EpicMount = item;
			#endregion
			#endregion
		#endregion Armour/Equip
			if (log.IsInfoEnabled)
				log.Info("Equipment NPC initialized");
		}
		#region Add To World

		public override bool AddToWorld()
		{
			Name = "Picculus Imp Refugee";
			Model = 638;
			GuildName = "Equipment Provider";
			Level = 50;
			Realm = (byte)CurrentZone.GetRealm();
			base.AddToWorld();
			return true;
		}

		#endregion Add To World
		#region Base Interact

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;
			if (player.Level < 50)
				SendReply(player, "Please come back when you are level 50");
			else SendReply(player, "Hello! " + player.Name + ", Are you interested in your [Armour] your [Weapons]\n " +
				"Your [Horse] or your [Cloak]?");
			return true;
		}
		#endregion Base Interact
		#region WhisperReceive Function

		public override bool WhisperReceive(GameLiving source, string str)
		{

			if (!base.WhisperReceive(source, str)) return false;
			if (!(source is GamePlayer)) return false;
			GamePlayer player = (GamePlayer)source;
			switch (str)
			{
				#region Horse
				case "Horse":
					{
						GiveItem(player, EpicMount);
					}
					break;
				#endregion Horse
				#region Armour
				case "Armour":
					{
						#region Albion
						#region Wizard
						if (player.CharacterClass.ID == (byte)eCharacterClass.Wizard)
						{
							GiveItem(player, WizardEpicBoots);
							GiveItem(player, WizardEpicHelm);
							GiveItem(player, WizardEpicGloves);
							GiveItem(player, WizardEpicVest);
							GiveItem(player, WizardEpicLegs);
							GiveItem(player, WizardEpicArms);
						}
						#endregion Wizard
						#region Minstrel
						if (player.CharacterClass.ID == (byte)eCharacterClass.Minstrel)
						{
							GiveItem(player, MinstrelEpicBoots);
							GiveItem(player, MinstrelEpicHelm);
							GiveItem(player, MinstrelEpicGloves);
							GiveItem(player, MinstrelEpicVest);
							GiveItem(player, MinstrelEpicLegs);
							GiveItem(player, MinstrelEpicArms);
						}
						#endregion Minstrel
						#region Sorcerer
						if (player.CharacterClass.ID == (byte)eCharacterClass.Sorcerer)
						{
							GiveItem(player, SorcererEpicBoots);
							GiveItem(player, SorcererEpicHelm);
							GiveItem(player, SorcererEpicGloves);
							GiveItem(player, SorcererEpicVest);
							GiveItem(player, SorcererEpicLegs);
							GiveItem(player, SorcererEpicArms);
						}
						#endregion Sorceror
						#region Cleric
						if (player.CharacterClass.ID == (byte)eCharacterClass.Cleric)
						{
							GiveItem(player, ClericEpicBoots);
							GiveItem(player, ClericEpicHelm);
							GiveItem(player, ClericEpicGloves);
							GiveItem(player, ClericEpicVest);
							GiveItem(player, ClericEpicLegs);
							GiveItem(player, ClericEpicArms);
						}
						#endregion Cleric
						#region Paladin
						if (player.CharacterClass.ID == (byte)eCharacterClass.Paladin)
						{
							GiveItem(player, PaladinEpicBoots);
							GiveItem(player, PaladinEpicGloves);
							GiveItem(player, PaladinEpicVest);
							GiveItem(player, PaladinEpicLegs);
							GiveItem(player, PaladinEpicArms);
							GiveItem(player, PaladinEpicHelm);
						}
						#endregion Paladin
						#region Mercenary
						if (player.CharacterClass.ID == (byte)eCharacterClass.Mercenary)
						{
							GiveItem(player, MercenaryEpicBoots);
							GiveItem(player, MercenaryEpicHelm);
							GiveItem(player, MercenaryEpicGloves);
							GiveItem(player, MercenaryEpicVest);
							GiveItem(player, MercenaryEpicLegs);
							GiveItem(player, MercenaryEpicArms);
						}
						#endregion Mercenary
						#region Reaver
						if (player.CharacterClass.ID == (byte)eCharacterClass.Reaver)
						{
							GiveItem(player, ReaverEpicBoots);
							GiveItem(player, ReaverEpicHelm);
							GiveItem(player, ReaverEpicGloves);
							GiveItem(player, ReaverEpicVest);
							GiveItem(player, ReaverEpicLegs);
							GiveItem(player, ReaverEpicArms);
						}
						#endregion Reaver
						#region Cabalist
						if (player.CharacterClass.ID == (byte)eCharacterClass.Cabalist)
						{
							GiveItem(player, CabalistEpicBoots);
							GiveItem(player, CabalistEpicHelm);
							GiveItem(player, CabalistEpicGloves);
							GiveItem(player, CabalistEpicVest);
							GiveItem(player, CabalistEpicLegs);
							GiveItem(player, CabalistEpicArms);
						}
						#endregion Cabalist
						#region Infiltrator
						if (player.CharacterClass.ID == (byte)eCharacterClass.Infiltrator)
						{
							GiveItem(player, InfiltratorEpicBoots);
							GiveItem(player, InfiltratorEpicHelm);
							GiveItem(player, InfiltratorEpicGloves);
							GiveItem(player, InfiltratorEpicVest);
							GiveItem(player, InfiltratorEpicLegs);
							GiveItem(player, InfiltratorEpicArms);
						}
						#endregion Infiltrator
						#region Necromancer
						if (player.CharacterClass.ID == (byte)eCharacterClass.Necromancer)
						{
							GiveItem(player, NecromancerEpicBoots);
							GiveItem(player, NecromancerEpicHelm);
							GiveItem(player, NecromancerEpicGloves);
							GiveItem(player, NecromancerEpicVest);
							GiveItem(player, NecromancerEpicLegs);
							GiveItem(player, NecromancerEpicArms);
						}
						#endregion Necromancer
						#region Scout
						if (player.CharacterClass.ID == (byte)eCharacterClass.Scout)
						{
							GiveItem(player, ScoutEpicBoots);
							GiveItem(player, ScoutEpicHelm);
							GiveItem(player, ScoutEpicGloves);
							GiveItem(player, ScoutEpicVest);
							GiveItem(player, ScoutEpicLegs);
							GiveItem(player, ScoutEpicArms);
						}
						#endregion Scout
						#region Armsman
						if (player.CharacterClass.ID == (byte)eCharacterClass.Armsman)
						{
							GiveItem(player, ArmsmanEpicBoots);
							GiveItem(player, ArmsmanEpicHelm);
							GiveItem(player, ArmsmanEpicGloves);
							GiveItem(player, ArmsmanEpicVest);
							GiveItem(player, ArmsmanEpicLegs);
							GiveItem(player, ArmsmanEpicArms);
						}
						#endregion Armsman
						#region Theurgist
						if (player.CharacterClass.ID == (byte)eCharacterClass.Theurgist)
						{
							GiveItem(player, TheurgistEpicBoots);
							GiveItem(player, TheurgistEpicHelm);
							GiveItem(player, TheurgistEpicGloves);
							GiveItem(player, TheurgistEpicVest);
							GiveItem(player, TheurgistEpicLegs);
							GiveItem(player, TheurgistEpicArms);
						}
						#endregion Theurgist
						#region Friar
						if (player.CharacterClass.ID == (byte)eCharacterClass.Friar)
						{
							GiveItem(player, FriarEpicBoots);
							GiveItem(player, FriarEpicHelm);
							GiveItem(player, FriarEpicGloves);
							GiveItem(player, FriarEpicVest);
							GiveItem(player, FriarEpicLegs);
							GiveItem(player, FriarEpicArms);
						}
						#endregion Friar
						#endregion Albion
						#region Midgard
						#region Hunter
						if (player.CharacterClass.ID == (byte)eCharacterClass.Hunter)
						{
							GiveItem(player, HunterEpicBoots);
							GiveItem(player, HunterEpicHelm);
							GiveItem(player, HunterEpicGloves);
							GiveItem(player, HunterEpicVest);
							GiveItem(player, HunterEpicLegs);
							GiveItem(player, HunterEpicArms);
						}
						#endregion Hunter
						#region Shadowblade
						if (player.CharacterClass.ID == (byte)eCharacterClass.Shadowblade)
						{
							GiveItem(player, ShadowbladeEpicBoots);
							GiveItem(player, ShadowbladeEpicHelm);
							GiveItem(player, ShadowbladeEpicGloves);
							GiveItem(player, ShadowbladeEpicVest);
							GiveItem(player, ShadowbladeEpicLegs);
							GiveItem(player, ShadowbladeEpicArms);
						}
						#endregion Shadowblade
						#region Spiritmaster
						if (player.CharacterClass.ID == (byte)eCharacterClass.Spiritmaster)
						{
							GiveItem(player, SpiritmasterEpicBoots);
							GiveItem(player, SpiritmasterEpicHelm);
							GiveItem(player, SpiritmasterEpicGloves);
							GiveItem(player, SpiritmasterEpicVest);
							GiveItem(player, SpiritmasterEpicLegs);
							GiveItem(player, SpiritmasterEpicArms);
						}
						#endregion Spiritmaster
						#region Runemaster
						if (player.CharacterClass.ID == (byte)eCharacterClass.Runemaster)
						{
							GiveItem(player, RunemasterEpicBoots);
							GiveItem(player, RunemasterEpicHelm);
							GiveItem(player, RunemasterEpicGloves);
							GiveItem(player, RunemasterEpicVest);
							GiveItem(player, RunemasterEpicLegs);
							GiveItem(player, RunemasterEpicArms);
						}
						#endregion Runemaster
						#region Bonedancer
						if (player.CharacterClass.ID == (byte)eCharacterClass.Bonedancer)
						{
							GiveItem(player, BonedancerEpicBoots);
							GiveItem(player, BonedancerEpicHelm);
							GiveItem(player, BonedancerEpicGloves);
							GiveItem(player, BonedancerEpicVest);
							GiveItem(player, BonedancerEpicLegs);
							GiveItem(player, BonedancerEpicArms);
						}
						#endregion Bonedancer
						#region Healer
						if (player.CharacterClass.ID == (byte)eCharacterClass.Healer)
						{
							GiveItem(player, HealerEpicBoots);
							GiveItem(player, HealerEpicHelm);
							GiveItem(player, HealerEpicGloves);
							GiveItem(player, HealerEpicVest);
							GiveItem(player, HealerEpicLegs);
							GiveItem(player, HealerEpicArms);
						}
						#endregion Healer
						#region Shaman
						if (player.CharacterClass.ID == (byte)eCharacterClass.Shaman)
						{
							GiveItem(player, ShamanEpicBoots);
							GiveItem(player, ShamanEpicHelm);
							GiveItem(player, ShamanEpicGloves);
							GiveItem(player, ShamanEpicVest);
							GiveItem(player, ShamanEpicLegs);
							GiveItem(player, ShamanEpicArms);
						}
						#endregion Shaman
						#region Warrior
						if (player.CharacterClass.ID == (byte)eCharacterClass.Warrior)
						{
							GiveItem(player, WarriorEpicBoots);
							GiveItem(player, WarriorEpicHelm);
							GiveItem(player, WarriorEpicGloves);
							GiveItem(player, WarriorEpicVest);
							GiveItem(player, WarriorEpicLegs);
							GiveItem(player, WarriorEpicArms);
						}
						#endregion Warrior
						#region Berserker
						if (player.CharacterClass.ID == (byte)eCharacterClass.Berserker)
						{
							GiveItem(player, BerserkerEpicBoots);
							GiveItem(player, BerserkerEpicHelm);
							GiveItem(player, BerserkerEpicGloves);
							GiveItem(player, BerserkerEpicVest);
							GiveItem(player, BerserkerEpicLegs);
							GiveItem(player, BerserkerEpicArms);
						}
						#endregion Berserker
						#region Thane
						if (player.CharacterClass.ID == (byte)eCharacterClass.Thane)
						{
							GiveItem(player, ThaneEpicBoots);
							GiveItem(player, ThaneEpicHelm);
							GiveItem(player, ThaneEpicGloves);
							GiveItem(player, ThaneEpicVest);
							GiveItem(player, ThaneEpicLegs);
							GiveItem(player, ThaneEpicArms);
						}
						#endregion Thane
						#region Skald
						if (player.CharacterClass.ID == (byte)eCharacterClass.Skald)
						{
							GiveItem(player, SkaldEpicBoots);
							GiveItem(player, SkaldEpicHelm);
							GiveItem(player, SkaldEpicGloves);
							GiveItem(player, SkaldEpicVest);
							GiveItem(player, SkaldEpicLegs);
							GiveItem(player, SkaldEpicArms);
						}
						#endregion Skald
						#region Savage
						if (player.CharacterClass.ID == (byte)eCharacterClass.Savage)
						{
							GiveItem(player, SavageEpicBoots);
							GiveItem(player, SavageEpicHelm);
							GiveItem(player, SavageEpicGloves);
							GiveItem(player, SavageEpicVest);
							GiveItem(player, SavageEpicLegs);
							GiveItem(player, SavageEpicArms);
						}
						#endregion Savage
						#endregion Midgard
						#region Hibernia
						#region Bard
						if (player.CharacterClass.ID == (byte)eCharacterClass.Bard)
						{
							GiveItem(player, BardEpicBoots);
							GiveItem(player, BardEpicHelm);
							GiveItem(player, BardEpicGloves);
							GiveItem(player, BardEpicVest);
							GiveItem(player, BardEpicLegs);
							GiveItem(player, BardEpicArms);
						}
						#endregion Bard
						#region Druid
						if (player.CharacterClass.ID == (byte)eCharacterClass.Druid)
						{
							GiveItem(player, DruidEpicBoots);
							GiveItem(player, DruidEpicHelm);
							GiveItem(player, DruidEpicGloves);
							GiveItem(player, DruidEpicVest);
							GiveItem(player, DruidEpicLegs);
							GiveItem(player, DruidEpicArms);
						}
						#endregion Druid
						#region Warden
						if (player.CharacterClass.ID == (byte)eCharacterClass.Warden)
						{
							GiveItem(player, WardenEpicBoots);
							GiveItem(player, WardenEpicHelm);
							GiveItem(player, WardenEpicGloves);
							GiveItem(player, WardenEpicVest);
							GiveItem(player, WardenEpicLegs);
							GiveItem(player, WardenEpicArms);
						}
						#endregion Warden
						#region Blademaster
						if (player.CharacterClass.ID == (byte)eCharacterClass.Blademaster)
						{
							GiveItem(player, BlademasterEpicBoots);
							GiveItem(player, BlademasterEpicGloves);
							GiveItem(player, BlademasterEpicHelm);
							GiveItem(player, BlademasterEpicVest);
							GiveItem(player, BlademasterEpicLegs);
							GiveItem(player, BlademasterEpicArms);
						}
						#endregion Blademaster
						#region Hero
						if (player.CharacterClass.ID == (byte)eCharacterClass.Hero)
						{
							GiveItem(player, HeroEpicBoots);
							GiveItem(player, HeroEpicHelm);
							GiveItem(player, HeroEpicGloves);
							GiveItem(player, HeroEpicVest);
							GiveItem(player, HeroEpicLegs);
							GiveItem(player, HeroEpicArms);
						}
						#endregion Hero
						#region Champion
						if (player.CharacterClass.ID == (byte)eCharacterClass.Champion)
						{
							GiveItem(player, ChampionEpicBoots);
							GiveItem(player, ChampionEpicHelm);
							GiveItem(player, ChampionEpicGloves);
							GiveItem(player, ChampionEpicVest);
							GiveItem(player, ChampionEpicLegs);
							GiveItem(player, ChampionEpicArms);
						}
						#endregion Champion
						#region Eldritch
						if (player.CharacterClass.ID == (byte)eCharacterClass.Eldritch)
						{
							GiveItem(player, EldritchEpicBoots);
							GiveItem(player, EldritchEpicHelm);
							GiveItem(player, EldritchEpicGloves);
							GiveItem(player, EldritchEpicVest);
							GiveItem(player, EldritchEpicLegs);
							GiveItem(player, EldritchEpicArms);
						}
						#endregion Eldritch
						#region Enchanter
						if (player.CharacterClass.ID == (byte)eCharacterClass.Enchanter)
						{
							GiveItem(player, EnchanterEpicBoots);
							GiveItem(player, EnchanterEpicHelm);
							GiveItem(player, EnchanterEpicGloves);
							GiveItem(player, EnchanterEpicVest);
							GiveItem(player, EnchanterEpicLegs);
							GiveItem(player, EnchanterEpicArms);
						}
						#endregion Enchanter
						#region Mentalist
						if (player.CharacterClass.ID == (byte)eCharacterClass.Mentalist)
						{
							GiveItem(player, MentalistEpicBoots);
							GiveItem(player, MentalistEpicHelm);
							GiveItem(player, MentalistEpicGloves);
							GiveItem(player, MentalistEpicVest);
							GiveItem(player, MentalistEpicLegs);
							GiveItem(player, MentalistEpicArms);
						}
						#endregion Mentalist
						#region Nightshade
						if (player.CharacterClass.ID == (byte)eCharacterClass.Nightshade)
						{
							GiveItem(player, NightshadeEpicBoots);
							GiveItem(player, NightshadeEpicHelm);
							GiveItem(player, NightshadeEpicGloves);
							GiveItem(player, NightshadeEpicVest);
							GiveItem(player, NightshadeEpicLegs);
							GiveItem(player, NightshadeEpicArms);
						}
						#endregion Nightshade
						#region Ranger
						if (player.CharacterClass.ID == (byte)eCharacterClass.Ranger)
						{
							GiveItem(player, RangerEpicBoots);
							GiveItem(player, RangerEpicHelm);
							GiveItem(player, RangerEpicGloves);
							GiveItem(player, RangerEpicVest);
							GiveItem(player, RangerEpicLegs);
							GiveItem(player, RangerEpicArms);
						}
						#endregion Ranger
						#region Animist
						if (player.CharacterClass.ID == (byte)eCharacterClass.Animist)
						{
							GiveItem(player, AnimistEpicBoots);
							GiveItem(player, AnimistEpicHelm);
							GiveItem(player, AnimistEpicGloves);
							GiveItem(player, AnimistEpicVest);
							GiveItem(player, AnimistEpicLegs);
							GiveItem(player, AnimistEpicArms);
						}
						#endregion Animist
						#region Valewalker
						if (player.CharacterClass.ID == (byte)eCharacterClass.Valewalker)
						{
							GiveItem(player, ValewalkerEpicBoots);
							GiveItem(player, ValewalkerEpicHelm);
							GiveItem(player, ValewalkerEpicGloves);
							GiveItem(player, ValewalkerEpicVest);
							GiveItem(player, ValewalkerEpicLegs);
							GiveItem(player, ValewalkerEpicArms);
						}
						#endregion Valewalker
						#endregion Hibernia
						break;
					}
				#endregion Armour
				#region Cloak
				case "Cloak":
					{
						if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Healer")
						|| (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Warlock")
						|| (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Bainshee") || (player.CharacterClass.Name == "Valewalker")
						|| (player.CharacterClass.Name == "Animist") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Heretic") || (player.CharacterClass.Name == "Necromancer")
						|| (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mentalist"))
						{
							GiveItem(player, EpicCasterCloak);
						}
						else
						{
							GiveItem(player, EpicMeleeCloak);
						}
						break;
					}
					#endregion Cloak
				#region Weapons
				case "Weapons":
					{
						#region Albion
						#region Wizard
						if (player.CharacterClass.ID == (byte)eCharacterClass.Wizard)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Wizard
						#region Minstrel
						if (player.CharacterClass.ID == (byte)eCharacterClass.Minstrel)
						{
							SendReply(player, "Would you like a [Slash Weapon], [Thrust Weapon], [Harp]\n" +
								"or a [Small Shield]");
						}
						#endregion Minstrel
						#region Sorcerer
						if (player.CharacterClass.ID == (byte)eCharacterClass.Sorcerer)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Sorceror
						#region Cleric
						if (player.CharacterClass.ID == (byte)eCharacterClass.Cleric)
						{
							SendReply(player, "Would you like A [Crush Weapon] or a [Medium Shield]?");
						}
						#endregion Cleric
						#region Paladin
						if (player.CharacterClass.ID == (byte)eCharacterClass.Paladin)
						{
							SendReply(player, "Would you like a [Two Handed Crush] a [Two Handed Slash] a [Two Handed Thrust]\n" +
								"or a [Crush Weapon] a [Slash Weapon] a [Thrust Weapon] or a [Large Shield]?");
						}
						#endregion Paladin
						#region Mercenary
						if (player.CharacterClass.ID == (byte)eCharacterClass.Mercenary)
						{
							SendReply(player, "Would you like a [Crush Weapon], [Slash Weapon], [Thrust Weapon]\n" +
								"a [Offhand Thruster], [Offhand Slasher], [Offhand Crusher], a [Medium Shield] or a [Short Bow]?");
						}
						#endregion Mercenary
						#region Reaver
						if (player.CharacterClass.ID == (byte)eCharacterClass.Reaver)
						{
							SendReply(player, "Would you like a [Slash Weapon], [Crush Weapon], [Thrust Weapon]\n" +
								"or a [Thrust Flex], [Slash Flex], [Crush Flex] or finally a [Large Shield]?");
						}
						#endregion Reaver
						#region Cabalist
						if (player.CharacterClass.ID == (byte)eCharacterClass.Cabalist)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Cabalist
						#region Infiltrator
						if (player.CharacterClass.ID == (byte)eCharacterClass.Infiltrator)
						{
							SendReply(player, "Would you like a [Slash Weapon], [Thrust Weapon]\n" +
							"a [Offhand Thruster], [Offhand Slasher], or a [Small Shield]?");
						}
						#endregion Infiltrator
						#region Necromancer
						if (player.CharacterClass.ID == (byte)eCharacterClass.Necromancer)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Necromancer
						#region Scout
						if (player.CharacterClass.ID == (byte)eCharacterClass.Scout)
						{
							SendReply(player, "Would you like a [Slash Weapon], [Thrust Weapon] a [Albion Bow] or a [Small Shield]");
						}
						#endregion Scout
						#region Armsman
						if (player.CharacterClass.ID == (byte)eCharacterClass.Armsman)
						{
							SendReply(player, "Would you like a [Crush Weapon], [Slash Weapon], [Thrust Weapon]\n" +
							"a [Two Handed Crush] a [Two Handed Slash] a [Two Handed Thrust] a [Large Shield]] a [Crush Pole] a [Slash Pole] or a [Thrust Pole]");
						}
						#endregion Armsman
						#region Theurgist
						if (player.CharacterClass.ID == (byte)eCharacterClass.Theurgist)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Theurgist
						#region Friar
						if (player.CharacterClass.ID == (byte)eCharacterClass.Friar)
						{
							SendReply(player, "Would you like a [Staff] a [Crush Weapon] or a [Small Shield]?");
						}
						#endregion Friar
						#endregion Albion
						#region Midgard
						#region Hunter
						if (player.CharacterClass.ID == (byte)eCharacterClass.Hunter)
						{
							SendReply(player, "Would you like a [Midgard Bow] a [One Handed Sword] a [Two Handed Sword]] or a [Midgard Slash Spear]?");
						}
						#endregion Hunter
						#region Shadowblade
						if (player.CharacterClass.ID == (byte)eCharacterClass.Shadowblade)
						{
							SendReply(player, "Would you like a [Sword], [Axe], [Left Axe], [Two Handed Sword], [Two Handed Axe] or a [Small Shield]?");
						}
						#endregion Shadowblade
						#region Spiritmaster
						if (player.CharacterClass.ID == (byte)eCharacterClass.Spiritmaster)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Spiritmaster
						#region Runemaster
						if (player.CharacterClass.ID == (byte)eCharacterClass.Runemaster)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Runemaster
						#region Bonedancer
						if (player.CharacterClass.ID == (byte)eCharacterClass.Bonedancer)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Bonedancer
						#region Healer
						if (player.CharacterClass.ID == (byte)eCharacterClass.Healer)
						{
							SendReply(player, "Would you like a [Hammer], a [Two Handed Hammer] or a [Small Shield]?");
						}
						#endregion Healer
						#region Shaman
						if (player.CharacterClass.ID == (byte)eCharacterClass.Shaman)
						{
							SendReply(player, "Would you like a [Hammer], a [Two Handed Hammer] or a [Small Shield]?");
						}
						#endregion Shaman
						#region Warrior
						if (player.CharacterClass.ID == (byte)eCharacterClass.Warrior)
						{
							SendReply(player, "Would you like a [Hammer], [Sword], [Axe] , a [Two Handed Hammer], [Two Handed Sword], [Two Handed Axe] or a [Large Shield]?");
						}
						#endregion Warrior
						#region Berserker
						if (player.CharacterClass.ID == (byte)eCharacterClass.Berserker)
						{
							SendReply(player, "Would you like a [Hammer], [Sword], [Axe], [Left Axe] , a [Two Handed Hammer], [Two Handed Sword], [Two Handed Axe] or a [Small Shield]?");
						}
						#endregion Berserker
						#region Thane
						if (player.CharacterClass.ID == (byte)eCharacterClass.Thane)
						{
							SendReply(player, "Would you like a [Hammer], [Sword], [Axe], a [Two Handed Hammer], [Two Handed Sword], [Two Handed Axe] or a [Medium Shield]?");
						}
						#endregion Thane
						#region Skald
						if (player.CharacterClass.ID == (byte)eCharacterClass.Skald)
						{
							SendReply(player, "Would you like a [Hammer], [Sword], [Axe], a [Two Handed Hammer], [Two Handed Sword], [Two Handed Axe] or a [Small Shield]?");
						}
						#endregion Skald
						#region Savage
						if (player.CharacterClass.ID == (byte)eCharacterClass.Savage)
						{
							SendReply(player, "Would you like a [Hammer], [Sword], [Axe], a [Two Handed Hammer], [Two Handed Sword], [Two Handed Axe] a [Small Shield]\n" +
								"Or a [Slash Claw], [Offhand Slash Claw], [Thrust Claw], [Offhand Thrust Claw] or a [Small Shield]?");
						}
						#endregion Savage
						#endregion Midgard
						#region Hibernia
						#region Bard
						if (player.CharacterClass.ID == (byte)eCharacterClass.Bard)
						{
							SendReply(player, "Would you like a [Blunt Weapon] a [Blade Weapon] a [Harp] or a [Small Shield]?");
						}
						#endregion Bard
						#region Druid
						if (player.CharacterClass.ID == (byte)eCharacterClass.Druid)
						{
							SendReply(player, "Would you like a [Blunt Weapon] a [Blade Weapon] or a [Small Shield]?");
						}
						#endregion Druid
						#region Warden
						if (player.CharacterClass.ID == (byte)eCharacterClass.Warden)
						{
							SendReply(player, "Would you like a [Blade Weapon] a [Blunt Weapon] or a [Medium Shield]?");
						}
						#endregion Warden
						#region Blademaster
						if (player.CharacterClass.ID == (byte)eCharacterClass.Blademaster)
						{
							SendReply(player, "Would you like a [Blade Weapon] a [Blunt Weapon] a [Pierce Weapon] a [Offhand Blade] a [Offhand Blunt] a [Offhand Pierce] OR a [Medium Shield]?");
						}
						#endregion Blademaster
						#region Hero
						if (player.CharacterClass.ID == (byte)eCharacterClass.Hero)
						{
							SendReply(player, "Would you like a [Blade Weapon] a [Blunt Weapon] a [Pierce Weapon] a [Large Blunt] a [Large Blade] a [Hibernian Blade Spear] a [Hibernian Pierce Spear] OR a [Large Shield]?");
						}
						#endregion Hero
						#region Champion
						if (player.CharacterClass.ID == (byte)eCharacterClass.Champion)
						{
							SendReply(player, "Would you like a [Blade Weapon] a [Blunt Weapon] a [Large Blunt] a [Large Blade] a [Hibernian Blade Spear] a [Hibernian Pierce Spear] OR a [Medium Shield]?");
						}
						#endregion Champion
						#region Eldritch
						if (player.CharacterClass.ID == (byte)eCharacterClass.Eldritch)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Eldritch
						#region Enchanter
						if (player.CharacterClass.ID == (byte)eCharacterClass.Enchanter)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Enchanter
						#region Mentalist
						if (player.CharacterClass.ID == (byte)eCharacterClass.Mentalist)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Mentalist
						#region Nightshade
						if (player.CharacterClass.ID == (byte)eCharacterClass.Nightshade)
						{
							SendReply(player, "Would you like a [Blade Weapon], [Offhand Blade], [Pierce Weapon] [Offhand Pierce] or a [Small Shield]?");
						}
						#endregion Nightshade
						#region Ranger
						if (player.CharacterClass.ID == (byte)eCharacterClass.Ranger)
						{
							SendReply(player, "Would you like a [Blade Weapon], [Offhand Blade], [Pierce Weapon] [Offhand Pierce] a [Small Shield] or a [Hibernian Bow]?");
						}
						#endregion Ranger
						#region Animist
						if (player.CharacterClass.ID == (byte)eCharacterClass.Animist)
						{
							SendReply(player, "Would you like the [Caster Staff]?");
						}
						#endregion Animist
						#region Valewalker
						if (player.CharacterClass.ID == (byte)eCharacterClass.Valewalker)
						{
							SendReply(player, "Would you like the [Hibernia Scythe]?");
						}
						#endregion Valewalker
						#endregion Hibernia
						break;
					}
					#endregion Weapons
				#region CapItems
				case "Cap Items":
					{
						GiveItem(player, EpicBracer1);
						GiveItem(player, EpicBracer2);
						GiveItem(player, EpicRing1);
						GiveItem(player, EpicRing2);
						GiveItem(player, EpicJewell);
						GiveItem(player, EpicNecklace);
						break;
					}
				#endregion CapItems
				#region Giving Weapons
				#region Hib
				#region Hibernia Scythe
				case "Hibernia Scythe":
					{
						GiveItem(player, HiberniaScythe);
						break;
					}
				#endregion Hibernia Scythe
				#region Caster Staff
				case "Caster Staff":
					{
						GiveItem(player, EpicCasterStaff);
						break;
					}
				#endregion Caster Staff
				#region Blade Weapon
				case "Blade Weapon":
					{
						GiveItem(player, HiberniaBlade);
						break;
					}
				#endregion Blade Weapon
				#region Offhand Blade
				case "Offhand Blade":
					{
						GiveItem(player, HiberniaLeftBlade);
						break;
					}
				#endregion Offhand Blade
				#region Pierce Weapon
				case "Pierce Weapon":
					{
						GiveItem(player, HiberniaPierce);
						break;
					}
				#endregion Pierce Weapon
				#region Offhand Pierce
				case "Offhand Pierce":
					{
						GiveItem(player, HiberniaLeftPierce);
						break;
					}
				#endregion Offhand Pierce
				#region Hibernian Bow
				case "Hibernian Bow":
					{
						GiveItem(player, RangerEpicBow);
						break;
					}
				#endregion Hibernian Bow
				#region Blunt Weapon
				case "Blunt Weapon":
					{
						GiveItem(player, HiberniaBlunt);
						break;
					}
				#endregion Blunt Weapon
				#region Offhand Blunt
				case "Offhand Blunt":
					{
						GiveItem(player, HiberniaLeftBlunt);
						break;
					}
				#endregion Offhand Blunt
				#region Large Blunt
				case "Large Blunt":
					{
						GiveItem(player, HiberniaCrushLargeWeapon);
						break;
					}
				#endregion Large Blunt
				#region Large Blade
				case "Large Blade":
					{
						GiveItem(player, HiberniaSlashLargeWeapon);
						break;
					}
				#endregion Large Blade
				#region Large Pierce
				case "Large Pierce":
					{
						GiveItem(player, HiberniaThrustLargeWeapon);
						break;
					}
				#endregion Large Pierce
				#region Hibernian Pierce Spear
				case "Hibernian Pierce Spear":
					{
						GiveItem(player, HiberniaTrustSpear);
						break;
					}
				#endregion Hibernian Pierce Spear
				#region Hibernian Blade Spear
				case "Hibernian Blade Spear":
					{
						GiveItem(player, HiberniaSlashSpear);
						break;
					}
				#endregion Hibernian Blade Spear
				#endregion Hib
				#region Mid
				#region Hammer
				case "Hammer":
					{
						GiveItem(player, Midgard1HHammer);
						break;
					}
				#endregion Hammer
				#region Axe
				case "Axe":
					{
						GiveItem(player, Midgard1HAxe);
						break;
					}
				#endregion Axe
				#region Left Axe
				case "Left Axe":
					{
						GiveItem(player, Midgard1HLeftAxe);
						break;
					}
				#endregion Left Axe
				#region Sword
				case "Sword":
					{
						GiveItem(player, Midgard1HSword);
						break;
					}
				#endregion Sword
				#region Two Handed Hammer
				case "Two Handed Hammer":
					{
						GiveItem(player, Midgard2HHammer);
						break;
					}
				#endregion Two Handed Hammer
				#region Two Handed Sword
				case "Two Handed Sword":
					{
						GiveItem(player, Midgard2HSword);
						break;
					}
				#endregion Two Handed Sword
				#region Two Handed Axe
				case "Two Handed Axe":
					{
						GiveItem(player, Midgard2HAxe);
						break;
					}
				#endregion Two Handed Axe
				#region Slash Claw
				case "Slash Claw":
					{
						GiveItem(player, MidgardSlashClaw);
						break;
					}
				#endregion Slash Claw
				#region Offhand Slash Claw
				case "Offhand Slash Claw":
					{
						GiveItem(player, MidgardSlashLeftClaw);
						break;
					}
				#endregion Offhand Slash Claw
				#region Thrust Claw
				case "Thrust Claw":
					{
						GiveItem(player, MidgardThrustClaw);
						break;
					}
				#endregion Thrust Claw
				#region Offhand Thrust Claw
				case "Offhand Thrust Claw":
					{
						GiveItem(player, MidgardThrustLeftClaw);
						break;
					}
				#endregion Offhand Thrust Claw
				#region Midgard Bow
				case "Midgard Bow":
					{
						GiveItem(player, HunterEpicBow);
						break;
					}
				#endregion Midgard Bow
				#region Midgard Slash Spear
				case "Midgard Slash Spear":
					{
						GiveItem(player, MidgardSlashSpear);
						break;
					}
				#endregion Midgard Slash Spear
				#endregion Mid
				#region Alb
				#region Staff
				case "Staff":
					{
						GiveItem(player, EpicFriarStaff);
						break;
					}
				#endregion Staff
				#region Crush Weapon
				case "Crush Weapon":
					{
						GiveItem(player, AlbionCrush);
						break;
					}
				#endregion Crush Weapon
				#region Thrust Weapon
				case "Thrust Weapon":
					{
						GiveItem(player, AlbionThrust);
						break;
					}
				#endregion Thrust Weapon
				#region Slash Weapon
				case "Slash Weapon":
					{
						GiveItem(player, AlbionSlash);
						break;
					}
				#endregion Slash Weapon
				#region Offhand Thruster
				case "Offhand Thruster":
					{
						GiveItem(player, AlbionLeftThrust);
						break;
					}
				#endregion Offhand Thruster
				#region Offhand Slasher
				case "Offhand Slasher":
					{
						GiveItem(player, AlbionLeftSlash);
						break;
					}
				#endregion Offhand Slasher
				#region Offhand Crusher
				case "Offhand Crusher":
					{
						GiveItem(player, AlbionLeftCrush);
						break;
					}
				#endregion Offhand Crusher
				#region Thrust Flex
				case "Thrust Flex":
					{
						GiveItem(player, AlbionThrustFlex);
						break;
					}
				#endregion Thrust Flex
				#region Slash Flex
				case "Slash Flex":
					{
						GiveItem(player, AlbionSlashFlex);
						break;
					}
				#endregion Slash Flex
				#region Crush Flex
				case "Crush Flex":
					{
						GiveItem(player, AlbionCrushFlex);
						break;
					}
				#endregion Crush Flex
				#region Two Handed Crush
				case "Two Handed Crush":
					{
						GiveItem(player, AlbionTwoHandedCrush);
						break;
					}
				#endregion Two Handed Crush
				#region Two Handed Slash
				case "Two Handed Slash":
					{
						GiveItem(player, AlbionTwoHandedSlash);
						break;
					}
				#endregion Two Handed Slash
				#region Two Handed Thrust
				case "Two Handed Thrust":
					{
						GiveItem(player, AlbionTwoHandedThrust);
						break;
					}
				#endregion Two Handed Thrust
				#region Crush Pole
				case "Crush Pole":
					{
						GiveItem(player, AlbionCrushPole);
						break;
					}
				#endregion Crush Pole
				#region Slash Pole
				case "Slash Pole":
					{
						GiveItem(player, AlbionSlashPole);
						break;
					}
				#endregion Slash Pole
				#region Thrust Pole
				case "Thrust Pole":
					{
						GiveItem(player, AlbionThrustPole);
						break;
					}
				#endregion Thrust Pole
				#region Albion Bow
				case "Albion Bow":
					{
						GiveItem(player, ScoutEpicBow);
						break;
					}
				#endregion Albion Bow
				#endregion Alb
				#region Misc-Weapons
				#region Small Shield
				case "Small Shield":
					{
						GiveItem(player, EpicSmallShield);
						break;
					}
				#endregion Small Shield
				#region Medium Shield
				case "Medium Shield":
					{
						GiveItem(player, EpicMediumShield);
						break;
					}
				#endregion Medium Shield
				#region Large Shield
				case "Large Shield":
					{
						GiveItem(player, EpicLargeShield);
						break;
					}
				#endregion Large Shield
				#region Harp
				case "Harp":
					{
						GiveItem(player, EpicHarp);
						break;
					}
				#endregion Harp
				#region Short Bow
				case "Short Bow":
					{
						GiveItem(player, EpicShortBow);
						break;
					}
				#endregion Short Bow
				#region Cross Bow
				case "Cross Bow":
					{
						GiveItem(player, EpicCrossBow);
						break;
					}
				#endregion Cross Bow
				#endregion Misc-Weapons
				#endregion Giving Weapons
			}
			return true;
		}

		#endregion
		private void SendReply(GamePlayer target, string msg)
		{
			target.Out.SendMessage(msg,
				eChatType.CT_Say, eChatLoc.CL_PopupWindow);
		}
	}
}
