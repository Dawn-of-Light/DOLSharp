using DOL.GS;
//using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// This class holds all information that
	/// EVERY armor crafting master npc in the game world needs!
	/// </summary>
	[NHibernate.Mapping.Attributes.Subclass(NameType=typeof(ArmorCraftingMaster), ExtendsType=typeof(GameCraftMaster))] 
	public class ArmorCraftingMaster : GameCraftMaster
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
		};

		public override eCraftingSkill[] TrainedSkills
		{
			get { return m_trainedSkills; }
		}

		public override string GUILD_ORDER
		{
			get { return "ArmorCrafting"; }
		}

		public override string GUILD_CRAFTERS
		{
			get { return "Armorsmiths"; }
		}

		public override eCharacterClass[] AllowedClass
		{
			get
			{
				return new eCharacterClass[]
					{
						eCharacterClass.Armsman,
						eCharacterClass.Cleric,
						eCharacterClass.Mercenary,
						eCharacterClass.Paladin,
						eCharacterClass.Reaver,
						eCharacterClass.Fighter,
						eCharacterClass.Guardian,
						eCharacterClass.Hero,
						eCharacterClass.Blademaster,
						eCharacterClass.Champion,
						eCharacterClass.Naturalist,
						eCharacterClass.Bard,
						eCharacterClass.Druid,
						eCharacterClass.Warden,
						eCharacterClass.Viking,
						eCharacterClass.Warrior,
						eCharacterClass.Berserker,
						eCharacterClass.Thane,
						eCharacterClass.Skald,
						eCharacterClass.Seer,
						eCharacterClass.Shaman,
						eCharacterClass.Healer,
						eCharacterClass.Vampiir,
						eCharacterClass.Savage,
						eCharacterClass.Valkyrie,
					};
			}
		}

		public override eCraftingSkill TheCraftingSkill
		{
			get { return eCraftingSkill.ArmorCrafting; }
		}

		public override string InitialEntersentence
		{
			get { return "Would you like to join the Order of [" + GUILD_ORDER + "]? As a crafter of armor, you can expect to create armor of various types, from studded leather to chain mail armor. While you will excel in armor crafting and have good skills in tailoring and weapon craft, you can expect great difficulty in fletching. A well-trained armor crafter also has small bit of skill to perform siege crafting should it be of need."; }
		}
	}
}