using System;

using DOL.Database;
using DOL.Events;
using DOL.GS;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// This is a convieniance enum for for inventory item hand flag
	/// </summary>
	public enum eHandFlag
	{
		/// <summary>
		/// Right Handed Weapon
		/// </summary>
		Right = 0,
		/// <summary>
		/// Two Handed Weapon
		/// </summary>
		Two = 1,
		/// <summary>
		/// Left Handed Weapon
		/// </summary>
		Left = 2,
	}

	/// <summary>
	/// This enum is used to tell us what extension level we want the armor to be
	/// </summary>
	public enum eExtension
	{ 
		/// <summary>
		/// Armor Extension 2
		/// </summary>
		Two = 2,
		/// <summary>
		/// Armor Extension 3
		/// </summary>
		Three = 3,
		/// <summary>
		/// Armor Extension 4
		/// </summary>
		Four = 4,
		/// <summary>
		/// Armor Extension 5
		/// </summary>
		Five = 5,
	}

	/// <summary>
	/// Class to manage the clothing of the guards
	/// </summary>
	public class ClothingMgr
	{
		//Declare the inventory template
		#region Albion
		public static GameNpcInventoryTemplate Albion_Archer = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Albion_Caster = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Albion_Fighter = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Albion_Healer = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Albion_Stealther = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Albion_Lord = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Albion_FighterPK = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Albion_ArcherPK = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Albion_CasterPK = new GameNpcInventoryTemplate();
		#endregion
		#region Midgard
		public static GameNpcInventoryTemplate Midgard_Archer = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Midgard_Caster = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Midgard_Fighter = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Midgard_Healer = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Midgard_Stealther = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Midgard_Lord = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Midgard_FighterPK = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Midgard_ArcherPK = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Midgard_CasterPK = new GameNpcInventoryTemplate();
		#endregion
		#region Hibernia
		public static GameNpcInventoryTemplate Hibernia_Archer = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Hibernia_Caster = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Hibernia_Fighter = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Hibernia_Healer = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Hibernia_Stealther = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Hibernia_Lord = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Hibernia_FighterPK = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Hibernia_ArcherPK = new GameNpcInventoryTemplate();
		public static GameNpcInventoryTemplate Hibernia_CasterPK = new GameNpcInventoryTemplate();
		#endregion

		/// <summary>
		/// Method to load all the templates into memory
		/// </summary>
		public static void LoadTemplates()
		{
			#region Albion
			#region Archer
            if (!Albion_Archer.LoadFromDatabase("albion_archer"))
            {
                Albion_Archer.AddNPCEquipment(eInventorySlot.Cloak, 676);
                Albion_Archer.AddNPCEquipment(eInventorySlot.TorsoArmor, 81);
                Albion_Archer.AddNPCEquipment(eInventorySlot.LegsArmor, 82);
                Albion_Archer.AddNPCEquipment(eInventorySlot.ArmsArmor, 83);
                Albion_Archer.AddNPCEquipment(eInventorySlot.HandsArmor, 85);
                Albion_Archer.AddNPCEquipment(eInventorySlot.FeetArmor, 84);
                Albion_Archer.AddNPCEquipment(eInventorySlot.HeadArmor, 824);
                Albion_Archer.AddNPCEquipment(eInventorySlot.DistanceWeapon, 132);
                Albion_Archer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 1);
                Albion_Archer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 59);
                Albion_Archer = Albion_Archer.CloseTemplate();
                Albion_Archer.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                //Albion_Archer.GetItem(eInventorySlot.DistanceWeapon).Object_Type = (int)eObjectType.Longbow;
                //Albion_Archer.GetItem(eInventorySlot.DistanceWeapon).SlotPosition = Slot.RANGED;
                Albion_Archer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
            #endregion
			#region Caster
            if (!Albion_Caster.LoadFromDatabase("albion_caster"))
            {
                Albion_Caster.AddNPCEquipment(eInventorySlot.Cloak, 676);
                Albion_Caster.AddNPCEquipment(eInventorySlot.TorsoArmor, 58);
                Albion_Caster.AddNPCEquipment(eInventorySlot.LegsArmor, 140);
                Albion_Caster.AddNPCEquipment(eInventorySlot.ArmsArmor, 141);
                Albion_Caster.AddNPCEquipment(eInventorySlot.HandsArmor, 142);
                Albion_Caster.AddNPCEquipment(eInventorySlot.FeetArmor, 143);
                Albion_Caster.AddNPCEquipment(eInventorySlot.HeadArmor, 822);
                Albion_Caster.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 19);
                Albion_Caster = Albion_Caster.CloseTemplate();
                Albion_Caster.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Fighter
            if (!Albion_Fighter.LoadFromDatabase("albion_fighter"))
            {
                Albion_Fighter.AddNPCEquipment(eInventorySlot.Cloak, 676);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.TorsoArmor, 46);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.LegsArmor, 47);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.ArmsArmor, 48);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.HandsArmor, 49);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.FeetArmor, 50);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.HeadArmor, 64);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 67);
                Albion_Fighter = Albion_Fighter.CloseTemplate();
                //Albion_Fighter.GetItem(eInventorySlot.DistanceWeapon).Object_Type = (int)eObjectType.Longbow;
                //Albion_Fighter.GetItem(eInventorySlot.DistanceWeapon).SlotPosition = Slot.RANGED;
                Albion_Fighter.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Albion_Fighter.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Lord
            if (!Albion_Lord.LoadFromDatabase("albion_lord"))
            {
                Albion_Lord.AddNPCEquipment(eInventorySlot.Cloak, 676);
                Albion_Lord.AddNPCEquipment(eInventorySlot.TorsoArmor, 46);
                Albion_Lord.AddNPCEquipment(eInventorySlot.LegsArmor, 47);
                Albion_Lord.AddNPCEquipment(eInventorySlot.ArmsArmor, 48);
                Albion_Lord.AddNPCEquipment(eInventorySlot.HandsArmor, 49);
                Albion_Lord.AddNPCEquipment(eInventorySlot.FeetArmor, 50);
                Albion_Lord.AddNPCEquipment(eInventorySlot.HeadArmor, 64);
                Albion_Lord.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Albion_Lord.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
                Albion_Lord.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 67);
                Albion_Lord.AddNPCEquipment(eInventorySlot.DistanceWeapon, 132);
                Albion_Lord = Albion_Lord.CloseTemplate();
                Albion_Lord.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                //Albion_Fighter.GetItem(eInventorySlot.DistanceWeapon).Object_Type = (int)eObjectType.Longbow;
                //Albion_Fighter.GetItem(eInventorySlot.DistanceWeapon).SlotPosition = Slot.RANGED;
                Albion_Lord.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Albion_Lord.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Healer
            if (!Albion_Healer.LoadFromDatabase("albion_healer"))
            {
                Albion_Healer.AddNPCEquipment(eInventorySlot.Cloak, 676);
                Albion_Healer.AddNPCEquipment(eInventorySlot.TorsoArmor, 41);
                Albion_Healer.AddNPCEquipment(eInventorySlot.LegsArmor, 42);
                Albion_Healer.AddNPCEquipment(eInventorySlot.ArmsArmor, 43);
                Albion_Healer.AddNPCEquipment(eInventorySlot.HandsArmor, 44);
                Albion_Healer.AddNPCEquipment(eInventorySlot.FeetArmor, 45);
                Albion_Healer.AddNPCEquipment(eInventorySlot.HeadArmor, 63);
                Albion_Healer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 61);
                Albion_Healer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 13);
                Albion_Healer = Albion_Healer.CloseTemplate();
                Albion_Healer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
			#endregion
			#region Stealther
            if (!Albion_Stealther.LoadFromDatabase("albion_stealther"))
            {
                Albion_Stealther.AddNPCEquipment(eInventorySlot.Cloak, 676);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.TorsoArmor, 31);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.LegsArmor, 32);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.ArmsArmor, 33);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.HandsArmor, 34);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.FeetArmor, 40);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.HeadArmor, 62);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 1);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.RightHandWeapon, 1);
                Albion_Stealther = Albion_Stealther.CloseTemplate();
                Albion_Stealther.GetItem(eInventorySlot.LeftHandWeapon).Hand = (int)eHandFlag.Left;
                Albion_Stealther.GetItem(eInventorySlot.LeftHandWeapon).SlotPosition = Slot.LEFTHAND;
            }
			#endregion
			#region PK
            //portal keep
			Albion_FighterPK.LoadFromDatabase("alb_fighter_pk");
			Albion_ArcherPK.LoadFromDatabase("alb_archer_pk");
			Albion_CasterPK.LoadFromDatabase("alb_caster_pk");
			#endregion
			#endregion
			#region Midgard
			#region Archer
            if (!Midgard_Archer.LoadFromDatabase("midgard_archer"))
            {
                Midgard_Archer.AddNPCEquipment(eInventorySlot.Cloak, 677);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.TorsoArmor, 230);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.LegsArmor, 231);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.ArmsArmor, 232);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.HandsArmor, 233);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.FeetArmor, 234);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.HeadArmor, 829);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.DistanceWeapon, 564);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 328);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 59);
                Midgard_Archer = Midgard_Archer.CloseTemplate();
                Midgard_Archer.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                //Midgard_Archer.GetItem(eInventorySlot.DistanceWeapon).Object_Type = (int)eObjectType.CompositeBow;
                //Midgard_Archer.GetItem(eInventorySlot.DistanceWeapon).SlotPosition = Slot.RANGED;
                Midgard_Archer.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
                Midgard_Archer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
			#endregion
			#region Caster
            if (!Midgard_Caster.LoadFromDatabase("midgard_caster"))
            {
                Midgard_Caster.AddNPCEquipment(eInventorySlot.Cloak, 677);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.TorsoArmor, 58);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.LegsArmor, 246);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.ArmsArmor, 247);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.HandsArmor, 248);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.FeetArmor, 249);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.HeadArmor, 825);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 327);
                Midgard_Caster = Midgard_Caster.CloseTemplate();
                Midgard_Caster.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Fighter
            if (!Midgard_Fighter.LoadFromDatabase("midgard_fighter"))
            {
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.Cloak, 677);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.TorsoArmor, 235);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.LegsArmor, 236);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.ArmsArmor, 237);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.HandsArmor, 238);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.FeetArmor, 239);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.HeadArmor, 832);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 6);
                Midgard_Fighter = Midgard_Fighter.CloseTemplate();
                Midgard_Fighter.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Midgard_Fighter.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Lord
            if (!Midgard_Lord.LoadFromDatabase("midgard_lord"))
            {
                Midgard_Lord.AddNPCEquipment(eInventorySlot.Cloak, 677);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.TorsoArmor, 235);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.LegsArmor, 236);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.ArmsArmor, 237);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.HandsArmor, 238);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.FeetArmor, 239);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.HeadArmor, 832);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 6);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.DistanceWeapon, 564);
                Midgard_Lord = Midgard_Lord.CloseTemplate();
                Midgard_Lord.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                Midgard_Lord.GetItem(eInventorySlot.DistanceWeapon).Object_Type = (int)eObjectType.Longbow;
                Midgard_Lord.GetItem(eInventorySlot.DistanceWeapon).SlotPosition = Slot.RANGED;
                Midgard_Lord.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Midgard_Lord.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Healer
            if (!Midgard_Healer.LoadFromDatabase("midgard_healer"))
            {
                Midgard_Healer.AddNPCEquipment(eInventorySlot.Cloak, 677);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.TorsoArmor, 235);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.LegsArmor, 236);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.ArmsArmor, 237);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.HandsArmor, 238);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.FeetArmor, 239);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.HeadArmor, 832);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 59);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 320);
                Midgard_Healer = Midgard_Healer.CloseTemplate();
                Midgard_Healer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
			#endregion
			#region Stealther
            if (!Midgard_Stealther.LoadFromDatabase("midgard_stealther"))
            {
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.Cloak, 677);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.TorsoArmor, 240);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.LegsArmor, 241);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.ArmsArmor, 242);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.HandsArmor, 243);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.FeetArmor, 244);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.HeadArmor, 335);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 315);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.RightHandWeapon, 315);
                Midgard_Stealther = Midgard_Stealther.CloseTemplate();
                Midgard_Stealther.GetItem(eInventorySlot.LeftHandWeapon).Hand = (int)eHandFlag.Left;
                Midgard_Stealther.GetItem(eInventorySlot.LeftHandWeapon).SlotPosition = Slot.LEFTHAND;
            }
			#endregion
			#region PK
			Midgard_FighterPK.LoadFromDatabase("mid_fighter_pk");
			Midgard_ArcherPK.LoadFromDatabase("mid_archer_pk");
			Midgard_CasterPK.LoadFromDatabase("mid_caster_pk");
			#endregion
			#endregion
			#region Hibernia
			#region Archer
            if (!Hibernia_Archer.LoadFromDatabase("hibernia_archer"))
            {
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.Cloak, 678);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.TorsoArmor, 383);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.LegsArmor, 384);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.ArmsArmor, 385);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.HandsArmor, 386);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.FeetArmor, 387);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.HeadArmor, 835);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.DistanceWeapon, 471);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 940);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 940);
                Hibernia_Archer = Hibernia_Archer.CloseTemplate();
                Hibernia_Archer.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                Hibernia_Archer.GetItem(eInventorySlot.DistanceWeapon).Object_Type = (int)eObjectType.RecurvedBow;
                Hibernia_Archer.GetItem(eInventorySlot.DistanceWeapon).SlotPosition = Slot.RANGED;
                Hibernia_Archer.GetItem(eInventorySlot.LeftHandWeapon).Hand = (int)eHandFlag.Left;
            }
			#endregion
			#region Caster
            if (!Hibernia_Caster.LoadFromDatabase("hibernia_caster"))
            {
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.Cloak, 678);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.TorsoArmor, 338);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.LegsArmor, 339);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.ArmsArmor, 340);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.HandsArmor, 341);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.FeetArmor, 342);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.HeadArmor, 826);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 468);
                Hibernia_Caster = Hibernia_Caster.CloseTemplate();
                Hibernia_Caster.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Fighter
            if (!Hibernia_Fighter.LoadFromDatabase("hibernia_fighter"))
            {
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.Cloak, 678);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.TorsoArmor, 348);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.LegsArmor, 349);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.ArmsArmor, 350);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.HandsArmor, 351);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.FeetArmor, 352);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.HeadArmor, 838);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.RightHandWeapon, 446);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 640);
                Hibernia_Fighter = Hibernia_Fighter.CloseTemplate();
                Hibernia_Fighter.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Hibernia_Fighter.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Lord
            if (!Hibernia_Lord.LoadFromDatabase("hibernia_lord"))
            {
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.Cloak, 678);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.TorsoArmor, 348);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.LegsArmor, 349);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.ArmsArmor, 350);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.HandsArmor, 351);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.FeetArmor, 352);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.HeadArmor, 838);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.RightHandWeapon, 446);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 640);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.DistanceWeapon, 471);
                //Hibernia_Lord = Hibernia_Lord.CloseTemplate();
                Hibernia_Lord.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                Hibernia_Lord.GetItem(eInventorySlot.DistanceWeapon).Object_Type = (int)eObjectType.CompositeBow;
                Hibernia_Lord.GetItem(eInventorySlot.DistanceWeapon).SlotPosition = Slot.RANGED;
                Hibernia_Lord.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Hibernia_Lord.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
			#endregion
			#region Healer
            if (!Hibernia_Healer.LoadFromDatabase("hibernia_healer"))
            {
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.Cloak, 678);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.TorsoArmor, 348);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.LegsArmor, 349);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.ArmsArmor, 350);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.HandsArmor, 351);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.FeetArmor, 352);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.HeadArmor, 838);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 59);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 450);
                Hibernia_Healer = Hibernia_Healer.CloseTemplate();
                Hibernia_Healer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
			#endregion
			#region Stealther
            if (!Hibernia_Stealther.LoadFromDatabase("hibernia_stealther"))
            {
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.Cloak, 678);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.TorsoArmor, 393);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.LegsArmor, 394);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.ArmsArmor, 395);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.HandsArmor, 396);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.FeetArmor, 397);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.HeadArmor, 438);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 940);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.RightHandWeapon, 940);
                Hibernia_Stealther = Hibernia_Stealther.CloseTemplate();
                Hibernia_Stealther.GetItem(eInventorySlot.LeftHandWeapon).Hand = (int)eHandFlag.Left;
                Hibernia_Stealther.GetItem(eInventorySlot.LeftHandWeapon).SlotPosition = Slot.LEFTHAND;
            }
			#endregion
			#region PK
			Hibernia_FighterPK.LoadFromDatabase("hib_fighter_pk");
			Hibernia_ArcherPK.LoadFromDatabase("hib_archer_pk");
			Hibernia_CasterPK.LoadFromDatabase("hib_caster_pk");
			#endregion
			#endregion
		}

		/// <summary>
		/// Method to equip a guard
		/// </summary>
		/// <param name="guard">The guard object</param>
		public static void EquipGuard(GameKeepGuard guard)
		{
			if (guard is FrontierHastener)
				return;

			switch (guard.Realm)
			{
				case eRealm.None:
				case eRealm.Albion:
					{
						if (guard is GuardFighter)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Albion_FighterPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Albion_Fighter.CloneTemplate();
						}
						else if (guard is GuardLord || guard is MissionMaster)
							guard.Inventory = ClothingMgr.Albion_Lord.CloneTemplate();
						else if (guard is GuardHealer)
							guard.Inventory = ClothingMgr.Albion_Healer.CloneTemplate();
						else if (guard is GuardArcher)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Albion_ArcherPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Albion_Archer.CloneTemplate();
						}
						else if (guard is GuardCaster)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Albion_CasterPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Albion_Caster.CloneTemplate();
						}
						else if (guard is GuardStealther)
							guard.Inventory = ClothingMgr.Albion_Stealther.CloneTemplate();
						break;
					}
				case eRealm.Midgard:
					{
						if (guard is GuardFighter)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Midgard_FighterPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Midgard_Fighter.CloneTemplate();
						}
						else if (guard is GuardLord || guard is MissionMaster)
							guard.Inventory = ClothingMgr.Midgard_Lord.CloneTemplate();
						else if (guard is GuardHealer)
							guard.Inventory = ClothingMgr.Midgard_Healer.CloneTemplate();
						else if (guard is GuardArcher)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Midgard_ArcherPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Midgard_Archer.CloneTemplate();
						}
						else if (guard is GuardCaster)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Midgard_CasterPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Midgard_Caster.CloneTemplate();
						}
						else if (guard is GuardStealther)
							guard.Inventory = ClothingMgr.Midgard_Stealther.CloneTemplate();
						break;
					}
				case eRealm.Hibernia:
					{
						if (guard is GuardFighter)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Hibernia_FighterPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Hibernia_Fighter.CloneTemplate();
						}
						else if (guard is GuardLord || guard is MissionMaster)
							guard.Inventory = ClothingMgr.Hibernia_Lord.CloneTemplate();
						else if (guard is GuardHealer)
							guard.Inventory = ClothingMgr.Hibernia_Healer.CloneTemplate();
						else if (guard is GuardArcher)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Hibernia_ArcherPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Hibernia_Archer.CloneTemplate();
						}
						else if (guard is GuardCaster)
						{
							if (guard.IsPortalKeepGuard)
								guard.Inventory = ClothingMgr.Hibernia_CasterPK.CloneTemplate();
							else guard.Inventory = ClothingMgr.Hibernia_Caster.CloneTemplate();
						}
						else if (guard is GuardStealther)
							guard.Inventory = ClothingMgr.Hibernia_Stealther.CloneTemplate();
						break;
					}
			}
			if (guard.Inventory == null)
			{
				KeepMgr.Logger.Error("Inventory is null for " + guard.GetType().ToString());
				return;
			}
			GameNpcInventoryTemplate template = guard.Inventory as GameNpcInventoryTemplate;
			guard.Inventory = new GameNPCInventory(template);

			//special extensions
			InventoryItem item = null;
			item = guard.Inventory.GetItem(eInventorySlot.TorsoArmor);
			if (item != null)
				item.Extension = (int)eExtension.Five;
			item = guard.Inventory.GetItem(eInventorySlot.HandsArmor);
			if (item != null)
				item.Extension = (int)eExtension.Five;
			item = guard.Inventory.GetItem(eInventorySlot.FeetArmor);
			if (item != null)
				item.Extension = (int)eExtension.Five;

			// set the active slot
			// casters and midgard archers use two handed weapons as default
			if (guard is GuardCaster)
				guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			else if (guard is GuardArcher && guard.Realm == eRealm.Midgard)
				guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			else if ((guard is GuardFighter || guard is GuardLord) && Util.Chance(50))
				guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			else guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);
		}

		/// <summary>
		/// Method to Set an Emblem to a Guards equipment
		/// </summary>
		/// <param name="guard">The guard object</param>
		public static void SetEmblem(GameKeepGuard guard)
		{
			if (guard.Inventory == null)
				return;
			if (guard.Component == null)
				return;
			int emblem = 0;
			if (guard.Component.Keep.Guild != null) emblem = guard.Component.Keep.Guild.theGuildDB.Emblem;
			InventoryItem cloak = guard.Inventory.GetItem(eInventorySlot.Cloak);
			if (cloak != null) cloak.Emblem = emblem;
			InventoryItem shield = guard.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			if (shield != null) shield.Emblem = emblem;
			guard.UpdateNPCEquipmentAppearance();
		}
	}
}
