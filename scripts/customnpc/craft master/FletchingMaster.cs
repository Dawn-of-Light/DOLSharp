 //using DOL.Database;
//using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// the master for armorcrafting
	/// </summary>
	[NPCGuildScript("Fletchers Master")]
	public class FletchingMaster : CraftNPC
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
			get { return "Fletching"; }
		}

		public override string GUILD_CRAFTERS
		{
			get { return "Fletchers"; }
		}

		public override eCraftingSkill TheCraftingSkill
		{
			get { return eCraftingSkill.Fletching; }
		}

		public override string InitialEntersentence
		{
			get { return "Would you like to join the Order of [" + GUILD_ORDER + "]? As a Fletcher you can expect to create bows arrows and staves. While you will excel in Fletching and have good skills in Weaponscrafting, you can expect great Difficulty in Armorcrafting and Tayloring. A well trained Armor crafter also has a great bit of skill to perform Siege Crafting should it be needed"; }
		}
	}
}