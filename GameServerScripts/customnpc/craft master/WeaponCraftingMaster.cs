 //using DOL.GS.Database;
//using DOL.GS.PacketHandler;

using NHibernate.Mapping.Attributes;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// This class holds all information that
	/// EVERY Weapon Crafting master npc in the game world needs!
	/// </summary>
	[Subclass(NameType=typeof(WeaponCraftingMaster), ExtendsType=typeof(GameCraftMaster))] 
	public class WeaponCraftingMaster : GameCraftMaster
	{
		private static readonly eCraftingSkill[] m_trainedSkills = 
		{
			eCraftingSkill.ArmorCrafting,
			eCraftingSkill.ClothWorking,
			eCraftingSkill.Fletching,
			eCraftingSkill.LeatherCrafting,
			eCraftingSkill.SiegeCrafting,
			eCraftingSkill.Tailoring,
			eCraftingSkill.WeaponCrafting,
			eCraftingSkill.MetalWorking,
			eCraftingSkill.WoodWorking,
		};

		public override eCraftingSkill[] TrainedSkills
		{
			get { return m_trainedSkills; }
		}

		public override string GUILD_ORDER
		{
			get { return "WeaponCrafting"; }
		}

		public override string GUILD_CRAFTERS
		{
			get { return "Weaponsmiths"; }
		}

		public override eCharacterClass[] AllowedClass
		{
			get
			{
				return new eCharacterClass[]
					{
						eCharacterClass.Reaver,
						eCharacterClass.Fighter,
						eCharacterClass.Mercenary,
						eCharacterClass.Paladin,
						eCharacterClass.Armsman,
						eCharacterClass.Infiltrator,
						eCharacterClass.Guardian,
						eCharacterClass.Hero,
						eCharacterClass.Blademaster,
						eCharacterClass.Champion,
						eCharacterClass.Nightshade,
						eCharacterClass.Ranger,
						eCharacterClass.Bard,
						eCharacterClass.Viking,
						eCharacterClass.Warrior,
						eCharacterClass.Berserker,
						eCharacterClass.Thane,
						eCharacterClass.Skald,
						eCharacterClass.MidgardRogue,
						eCharacterClass.Shadowblade,
						eCharacterClass.Hunter,
						eCharacterClass.Heretic,
						eCharacterClass.Vampiir,
						eCharacterClass.Savage,
						eCharacterClass.Valkyrie,

					};
			}
		}

		public override eCraftingSkill TheCraftingSkill
		{
			get { return eCraftingSkill.WeaponCrafting; }
		}

		public override string InitialEntersentence
		{
			get { return "Would you like to join the Order of [" + GUILD_ORDER + "]? As a crafter of weapons you can expect to forge swords axes hammers and spears. While you will excel in weaponscrafting and have good skills in Armor crafting, you can expect great Difficulty in Tayloring and Fletching. A well trained Weapons crafter also has a Great bit of skill to perform Siege Crafting should it be needed"; }
		}
	}
}