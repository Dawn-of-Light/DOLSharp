 //using DOL.Database;
//using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// the master for armorcrafting
	/// </summary>
	[NPCGuildScript("Weaponsmiths Master")]
	public class WeaponCraftingMaster : CraftNPC
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