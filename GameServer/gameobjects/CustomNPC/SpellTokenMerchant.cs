using DOL.Database;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using System.Linq;

namespace DOL.GS
{
	public class SpellTokenMerchant : GameMerchant
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static SpellLine MerchSpellLine {get;} = new SpellLine("MerchBaseSpellLine", "BuffMerch Spells", "unknown", true);

		public SpellTokenMerchant()
			: base()
		{
			Flags |= GameNPC.eFlags.PEACE;
		}

		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			var giver = source as GamePlayer;
			var spellId = item.Effect;
			
			if (GetDistanceTo(giver) > WorldMgr.INTERACT_DISTANCE)
			{
				giver.SendSystemMessage($"You are too far away to give anything to {GetName(0, false)}.");
				return false;
			}
			if (giver != null && item != null && spellId > 0)
			{
				var receivedItemIsInMerchantCatalog = Catalog.GetAllEntries().Where(e => e.Item.Id_nb == item.Id_nb).Any();
				if (receivedItemIsInMerchantCatalog)
				{
					var spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo(spellId));
					if (spell == null)
					{
						log.Error($"Token {item.Id_nb} uses spell with ID {spellId}, that could not be found.");
						return false;
					}
					CastSpell(giver, new Spell(spell, item.Level), MerchSpellLine);

					giver.SendMessage($"Fight well, {giver.RaceName}.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					giver.Inventory.RemoveItem(item);
					return true;
				}
			}
			return base.ReceiveItem(source, item);
		}

		private void CastSpell(GameLiving target, Spell spell, SpellLine spellLine)
		{
				ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, spellLine);

				if (spellHandler != null)
				{
					spellHandler.StartSpell(target);
				}
		}
	}
}
