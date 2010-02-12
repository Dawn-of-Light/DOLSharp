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
        public static GameNpcInventoryTemplate Midgard_Hastener = new GameNpcInventoryTemplate();
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
                Albion_Archer.AddNPCEquipment(eInventorySlot.Cloak, 3800);
                Albion_Archer.AddNPCEquipment(eInventorySlot.TorsoArmor, 728);
                Albion_Archer.AddNPCEquipment(eInventorySlot.LegsArmor, 663);
                Albion_Archer.AddNPCEquipment(eInventorySlot.ArmsArmor, 664);
                Albion_Archer.AddNPCEquipment(eInventorySlot.HandsArmor, 665);
                Albion_Archer.AddNPCEquipment(eInventorySlot.FeetArmor, 666);
                Albion_Archer.AddNPCEquipment(eInventorySlot.DistanceWeapon, 849);
                Albion_Archer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 653);
                Albion_Archer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 59);
                Albion_Archer = Albion_Archer.CloseTemplate();
                Albion_Archer.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                Albion_Archer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
            #endregion
            #region Caster
            if (!Albion_Caster.LoadFromDatabase("albion_caster"))
            {
                Albion_Caster.AddNPCEquipment(eInventorySlot.Cloak, 3800);
                Albion_Caster.AddNPCEquipment(eInventorySlot.TorsoArmor, 58);
                Albion_Caster.AddNPCEquipment(eInventorySlot.HandsArmor, 142);
                Albion_Caster.AddNPCEquipment(eInventorySlot.FeetArmor, 143);
                Albion_Caster.AddNPCEquipment(eInventorySlot.RightHandWeapon, 13);
                Albion_Caster.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 1170);
                Albion_Caster = Albion_Caster.CloseTemplate();
                Albion_Caster.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
            #endregion
            #region Fighter
            if (!Albion_Fighter.LoadFromDatabase("albion_fighter"))
            {
                Albion_Fighter.AddNPCEquipment(eInventorySlot.Cloak, 3800); 
                Albion_Fighter.AddNPCEquipment(eInventorySlot.TorsoArmor, 662);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.LegsArmor, 663);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.ArmsArmor, 664);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.HandsArmor, 665);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.FeetArmor, 666);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.HeadArmor, 95);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.RightHandWeapon, 10);
                Albion_Fighter.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 649);
                Albion_Fighter = Albion_Fighter.CloseTemplate();
                Albion_Fighter.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Albion_Fighter.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
            #endregion
            #region Lord
            if (!Albion_Lord.LoadFromDatabase("albion_lord"))
            {
                Albion_Lord.AddNPCEquipment(eInventorySlot.Cloak, 3800); //676
                Albion_Lord.AddNPCEquipment(eInventorySlot.TorsoArmor, 662);
                Albion_Lord.AddNPCEquipment(eInventorySlot.LegsArmor, 663);
                Albion_Lord.AddNPCEquipment(eInventorySlot.ArmsArmor, 664);
                Albion_Lord.AddNPCEquipment(eInventorySlot.HandsArmor, 665);
                Albion_Lord.AddNPCEquipment(eInventorySlot.FeetArmor, 666);
                Albion_Lord.AddNPCEquipment(eInventorySlot.HeadArmor, 95);
                Albion_Lord.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Albion_Lord.AddNPCEquipment(eInventorySlot.RightHandWeapon, 10);
                Albion_Lord.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 649);
                Albion_Lord.AddNPCEquipment(eInventorySlot.DistanceWeapon, 132);
                Albion_Lord = Albion_Lord.CloseTemplate();
                Albion_Lord.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                Albion_Lord.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Albion_Lord.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
            #endregion
            #region Healer
            if (!Albion_Healer.LoadFromDatabase("albion_healer"))
            {
                Albion_Healer.AddNPCEquipment(eInventorySlot.Cloak, 3800);
                Albion_Healer.AddNPCEquipment(eInventorySlot.TorsoArmor, 713);
                Albion_Healer.AddNPCEquipment(eInventorySlot.LegsArmor, 663);
                Albion_Healer.AddNPCEquipment(eInventorySlot.ArmsArmor, 664);
                Albion_Healer.AddNPCEquipment(eInventorySlot.HandsArmor, 665);
                Albion_Healer.AddNPCEquipment(eInventorySlot.FeetArmor, 666);
                Albion_Healer.AddNPCEquipment(eInventorySlot.HeadArmor, 94);
                Albion_Healer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 61);
                Albion_Healer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 3282);
                Albion_Healer = Albion_Healer.CloseTemplate();
                Albion_Fighter.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
            #endregion
            #region Stealther
            if (!Albion_Stealther.LoadFromDatabase("albion_stealther"))
            {
                Albion_Stealther.AddNPCEquipment(eInventorySlot.Cloak, 3800);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.TorsoArmor, 792);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.LegsArmor, 663);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.ArmsArmor, 664);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.HandsArmor, 665);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.FeetArmor, 666);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 653);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.RightHandWeapon, 653);
                Albion_Stealther.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 653);
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
                Midgard_Archer.AddNPCEquipment(eInventorySlot.Cloak, 3801);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.TorsoArmor, 668);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.LegsArmor, 2943);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.ArmsArmor, 2944);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.HandsArmor, 2945);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.FeetArmor, 2946);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.HeadArmor, 2874);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.DistanceWeapon, 1037);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 328);
                Midgard_Archer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 59);
                Midgard_Archer = Midgard_Archer.CloseTemplate();
                Midgard_Archer.GetItem(eInventorySlot.DistanceWeapon).Hand = (int)eHandFlag.Two;
                Midgard_Archer.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
                Midgard_Archer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
            #endregion
            #region Caster
            if (!Midgard_Caster.LoadFromDatabase("midgard_caster"))
            {
                Midgard_Caster.AddNPCEquipment(eInventorySlot.Cloak, 3801); 
                Midgard_Caster.AddNPCEquipment(eInventorySlot.TorsoArmor, 98);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.HandsArmor, 142);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.FeetArmor, 143);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.RightHandWeapon, 13);
                Midgard_Caster.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 566);
                Midgard_Caster = Midgard_Caster.CloseTemplate();
                Midgard_Caster.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
            #endregion
            #region Fighter
            if (!Midgard_Fighter.LoadFromDatabase("midgard_fighter"))
            {
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.Cloak, 3801); 
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.TorsoArmor, 668);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.LegsArmor, 2943);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.ArmsArmor, 2944);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.HandsArmor, 2945);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.FeetArmor, 2946);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.HeadArmor, 2874);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.RightHandWeapon, 313);
                Midgard_Fighter.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 572);
                Midgard_Fighter = Midgard_Fighter.CloseTemplate();
                Midgard_Fighter.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Midgard_Fighter.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
            #endregion
            #region Lord
            if (!Midgard_Lord.LoadFromDatabase("midgard_lord"))
            {
                Midgard_Lord.AddNPCEquipment(eInventorySlot.Cloak, 3801);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.TorsoArmor, 668);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.LegsArmor, 2943);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.ArmsArmor, 2944);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.HandsArmor, 2945);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.FeetArmor, 2946);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.HeadArmor, 2874);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 60);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.RightHandWeapon, 313);
                Midgard_Lord.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 572);
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
                Midgard_Healer.AddNPCEquipment(eInventorySlot.Cloak, 3801); 
                Midgard_Healer.AddNPCEquipment(eInventorySlot.TorsoArmor, 668);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.LegsArmor, 2943);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.ArmsArmor, 2944);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.HandsArmor, 2945);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.FeetArmor, 2946);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.HeadArmor, 2874);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 59);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 3335);
                Midgard_Healer.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 3336);
                Midgard_Healer = Midgard_Healer.CloseTemplate();
                Midgard_Healer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
            #endregion
            #region Hastener
            if (!Midgard_Hastener.LoadFromDatabase("midgard_hastener"))
            {
                Midgard_Hastener.AddNPCEquipment(eInventorySlot.Cloak, 443, 43);
                Midgard_Hastener.AddNPCEquipment(eInventorySlot.TorsoArmor, 230);
                Midgard_Hastener.AddNPCEquipment(eInventorySlot.HandsArmor, 233);
                Midgard_Hastener.AddNPCEquipment(eInventorySlot.FeetArmor, 234);
                Midgard_Hastener.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 228);
                Midgard_Hastener = Midgard_Hastener.CloseTemplate();
                Midgard_Hastener.GetItem(eInventorySlot.LeftHandWeapon).Hand = (int)eHandFlag.Left;
                Midgard_Hastener.GetItem(eInventorySlot.LeftHandWeapon).SlotPosition = Slot.LEFTHAND;
            }
            #endregion
            #region Stealther
            if (!Midgard_Stealther.LoadFromDatabase("midgard_stealther"))
            {
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.Cloak, 3801); 
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.TorsoArmor, 668);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.LegsArmor, 2943);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.ArmsArmor, 2944);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.HandsArmor, 2945);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.FeetArmor, 2946);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.HeadArmor, 335);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 573);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.RightHandWeapon, 573);
                Midgard_Stealther.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 577);
                Midgard_Stealther = Midgard_Stealther.CloseTemplate();
                Midgard_Stealther.GetItem(eInventorySlot.LeftHandWeapon).Hand = (int)eHandFlag.Left;
                Albion_Stealther.GetItem(eInventorySlot.LeftHandWeapon).SlotPosition = Slot.LEFTHAND;
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
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.Cloak, 3802);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.TorsoArmor, 667);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.LegsArmor, 989);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.ArmsArmor, 990);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.HandsArmor, 991);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.FeetArmor, 992);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.HeadArmor, 1207);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.DistanceWeapon, 919);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 643);
                Hibernia_Archer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 643);
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
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.Cloak, 3802);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.TorsoArmor, 97);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.HandsArmor, 142);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.FeetArmor, 143);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.RightHandWeapon, 13);
                Hibernia_Caster.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 1176);
                Hibernia_Caster = Hibernia_Caster.CloseTemplate();
                Hibernia_Caster.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
            #endregion
            #region Fighter
            if (!Hibernia_Fighter.LoadFromDatabase("hibernia_fighter"))
            {
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.Cloak, 3802);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.TorsoArmor, 667);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.LegsArmor, 989);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.ArmsArmor, 990);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.HandsArmor, 991);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.FeetArmor, 992);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.HeadArmor, 1207);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 79);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.RightHandWeapon, 897);
                Hibernia_Fighter.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 476);
                Hibernia_Fighter = Hibernia_Fighter.CloseTemplate();
                Hibernia_Fighter.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
                Hibernia_Fighter.GetItem(eInventorySlot.TwoHandWeapon).Hand = (int)eHandFlag.Two;
            }
            #endregion
            #region Lord
            if (!Hibernia_Lord.LoadFromDatabase("hibernia_lord"))
            {
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.Cloak, 3802); 
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.TorsoArmor, 667);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.LegsArmor, 989);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.ArmsArmor, 990);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.HandsArmor, 991);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.FeetArmor, 992);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.HeadArmor, 1207);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 79);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.RightHandWeapon, 897);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 476);
                Hibernia_Lord.AddNPCEquipment(eInventorySlot.DistanceWeapon, 471);
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
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.Cloak, 3802);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.TorsoArmor, 667);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.LegsArmor, 989);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.ArmsArmor, 990);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.HandsArmor, 991);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.FeetArmor, 992);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.HeadArmor, 1207);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 59);
                Hibernia_Healer.AddNPCEquipment(eInventorySlot.RightHandWeapon, 3247);
                Hibernia_Healer = Hibernia_Healer.CloseTemplate();
                Hibernia_Healer.GetItem(eInventorySlot.LeftHandWeapon).Object_Type = (int)eObjectType.Shield;
            }
            #endregion
            #region Stealther
            if (!Hibernia_Stealther.LoadFromDatabase("hibernia_stealther"))
            {
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.Cloak, 3802);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.TorsoArmor, 667);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.LegsArmor, 989);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.ArmsArmor, 990);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.HandsArmor, 991);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.FeetArmor, 992);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 2685);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.RightHandWeapon, 2685);
                Hibernia_Stealther.AddNPCEquipment(eInventorySlot.TwoHandWeapon, 2687);
                Hibernia_Stealther = Hibernia_Stealther.CloseTemplate();
                Hibernia_Stealther.GetItem(eInventorySlot.LeftHandWeapon).Hand = (int)eHandFlag.Left;
                Albion_Stealther.GetItem(eInventorySlot.LeftHandWeapon).SlotPosition = Slot.LEFTHAND;
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
			if(!ServerProperties.Properties.AUTOEQUIP_GUARDS_LOADED_FROM_DB && !guard.LoadedFromScript)
			{
				return;
			}
            if (guard is FrontierHastener)
            {
                switch (guard.Realm)
                {
                    case eRealm.None:
                    case eRealm.Albion:
                    case eRealm.Hibernia:
                    case eRealm.Midgard:
                        {
                            guard.Inventory = ClothingMgr.Midgard_Hastener.CloneTemplate();
                            break;
                        }
                }
            }

			switch (guard.ModelRealm)
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

			const int renegadeArmorColor = 19;

			InventoryItem item = null;
			item = guard.Inventory.GetItem(eInventorySlot.TorsoArmor);
			if (item != null)
			{
				if (guard.Realm != eRealm.None)
				{
					item.Extension = (int)eExtension.Five;
				}
				else
				{
					item.Extension = (int)eExtension.Four;
					item.Color = renegadeArmorColor;
				}
			}
			item = guard.Inventory.GetItem(eInventorySlot.HandsArmor);
			if (item != null)
			{
				if (guard.Realm != eRealm.None)
				{
					item.Extension = (int)eExtension.Five;
				}
				else
				{
					item.Extension = (int)eExtension.Four;
					item.Color = renegadeArmorColor;
				}
			}
			item = guard.Inventory.GetItem(eInventorySlot.FeetArmor);
			if (item != null)
			{
				if (guard.Realm != eRealm.None)
				{
					item.Extension = (int)eExtension.Five;
				}
				else
				{
					item.Extension = (int)eExtension.Four;
					item.Color = renegadeArmorColor;
				}
			}


			if (guard.Realm == eRealm.None)
			{
				item = guard.Inventory.GetItem(eInventorySlot.Cloak);
				if (item != null)
				{
					item.Model = 3632;
					item.Color = renegadeArmorColor;
				}
				item = guard.Inventory.GetItem(eInventorySlot.TorsoArmor);
				if (item != null)
				{
					item.Color = renegadeArmorColor;
				}
				item = guard.Inventory.GetItem(eInventorySlot.ArmsArmor);
				if (item != null)
				{
					item.Color = renegadeArmorColor;
				}
				item = guard.Inventory.GetItem(eInventorySlot.LegsArmor);
				if (item != null)
				{
					item.Color = renegadeArmorColor;
				}
			}

			// set the active slot
			// casters use two handed weapons as default
            // archers use distance weapons as default
			if (guard is GuardCaster)
				guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.TwoHanded);
			else if (guard is GuardArcher)
				guard.SwitchWeapon(GameLiving.eActiveWeaponSlot.Distance);
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
			if (guard.Component.Keep.Guild != null)
			{
				emblem = guard.Component.Keep.Guild.Emblem;
			}
			InventoryItem cloak = guard.Inventory.GetItem(eInventorySlot.Cloak);
			if (cloak != null)
			{
				cloak.Emblem = emblem;

				if (cloak.Emblem != 0)
					cloak.Model = 558; // change to a model that looks ok with an emblem
			}
			InventoryItem shield = guard.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			if (shield != null)
			{
				shield.Emblem = emblem;
			}
			guard.UpdateNPCEquipmentAppearance();
		}
	}
}
