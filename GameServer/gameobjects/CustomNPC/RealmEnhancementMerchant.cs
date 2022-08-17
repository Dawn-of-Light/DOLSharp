/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

/* 
 * NPC inspired from Shawn
 * All you need to do when you add this to your core is
 * /mob create DOL.GS.RealmEnhancementMerchant or /mob class DOL.GS.RealmEnhancementMerchant
 * 
 * Enjoy!
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections;

using DOL.Events;
using DOL.Database;
using DOL.Language;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;

using log4net;
using DOL.GS.Profession;
using DOL.GS.Finance;

namespace DOL.GS
{
	public class RealmEnhancementMerchant : GameMerchant
	{
		#region RealmEnhancementMerchant attrib/spells/casting
		public RealmEnhancementMerchant()
			: base()
		{
			Flags |= GameNPC.eFlags.PEACE;
		}

		public override int Concentration
		{
			get
			{
				return 10000;
			}
		}

		public override int Mana
		{
			get
			{
				return 10000;
			}
		}

		private Queue m_buffs = new Queue();
		private const int BUFFS_SPELL_DURATION = 1800;
		private const bool BUFFS_PLAYER_PET = true;

		/**
		 * Create DBSpell() 
		 *		allowAdd : 
		 *		castTime : casting time in seconds 
		 *		concentration : concentration used for spell 
		 *		clientEffect : visual client effect for spell 
		 *		icon : icon corresponding to spell 
		 *		duration : duration of the spell in seconds 
		 *		value : value of the spell (negative value in case of %)
		 *		description : description of the spell
		 *		name : name of the spell 
		 *		range : range of the spell 
		 *		spellId : id of the spell 
		 *		target : target of the spell (Self, Realm, Group, ...)
		 *		type : type of spell (AcuityBuff, ArmorFactorBuff, ConstitutionBuff, DexterityBuff, DexterityQuicknessBuff, HasteBuff, StrengthBuff, StrengthConstitutionBuff, ...)
		 *		effectGroup :  
		 */

		private static DBSpell createDBSpell(bool allowAdd, double castTime, int concentration, int clientEffect, int icon, int duration, double value, string description, string name, int range, int spellId, string target, string type, int effectGroup)
        {
			DBSpell spell = new DBSpell();
			spell.AllowAdd = allowAdd;
			spell.CastTime = castTime;
			spell.Concentration = concentration;
			spell.ClientEffect = clientEffect;
			spell.Icon = icon;
			spell.Duration = duration;
			spell.Value = value;
			spell.Name = name;
			spell.Description = description;
			spell.Range = range;
			spell.SpellID = spellId;
			spell.Target = target;
			spell.Type = type;
			spell.EffectGroup = effectGroup;
			return spell;
		}


		public override bool AddToWorld()
		{
			Level = 50;
			return base.AddToWorld();
		}
		public void BuffPlayer(GamePlayer player, Spell spell, SpellLine spellLine)
		{
			if (m_buffs == null) m_buffs = new Queue();
			
			m_buffs.Enqueue(new Container(spell, spellLine, player));

			//don't forget his pet !
			if(BUFFS_PLAYER_PET && player.ControlledBrain != null) 
			{
				if(player.ControlledBrain.Body != null) 
				{
					m_buffs.Enqueue(new Container(spell, spellLine, player.ControlledBrain.Body));
				}
			}

			CastBuffs();

		}
		public void CastBuffs()
		{
			Container con = null;
			while (m_buffs.Count > 0)
			{
				con = (Container)m_buffs.Dequeue();

				ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, con.Spell, con.SpellLine);

				if (spellHandler != null)
				{
					spellHandler.StartSpell(con.Target);
				}
			}
		}

		#region SpellCasting

		private static SpellLine m_MerchBaseSpellLine;
		private static SpellLine m_MerchSpecSpellLine;

		/// <summary>
		/// Spell line used by Merchs
		/// </summary>
		public static SpellLine MerchBaseSpellLine
		{
			get
			{
				if (m_MerchBaseSpellLine == null)
					m_MerchBaseSpellLine = new SpellLine("MerchBaseSpellLine", "BuffMerch Spells", "unknown", true);

				return m_MerchBaseSpellLine;
			}
		}
		public static SpellLine MerchSpecSpellLine
		{
			get
			{
				if (m_MerchSpecSpellLine == null)
					m_MerchSpecSpellLine = new SpellLine("MerchSpecSpellLine", "BuffMerch Spells", "unknown", false);

				return m_MerchSpecSpellLine;
			}
		}

		private static Spell m_baseaf;
		private static Spell m_basestr;
		private static Spell m_basecon;
		private static Spell m_basedex;

		private static Spell m_strcon;
		private static Spell m_dexqui;
		private static Spell m_acuity;
		private static Spell m_specaf;

		private static Spell m_haste;


		#region Spells

		/// <summary>
		/// Merch Base AF buff
		/// </summary>
		public static Spell MerchBaseAFBuff
		{
			get
			{
				if (m_baseaf == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823759"));
					// In case spell unknown, create it  
					if (spell == null)
					{ 
						spell = createDBSpell(false, 0, 1, 1461, 1461, BUFFS_SPELL_DURATION, 22, "The armor factor of the target is increased.", "Armor of the Realm",  1500, 225823759, "Realm", "ArmorFactorBuff", 1);
					}
					m_baseaf = new Spell(spell, 10);
				}
				return m_baseaf;
			}
		}

		/// <summary>
		/// Merch Base Str buff
		/// </summary>
		public static Spell MerchStrBuff
		{
			get
			{
				if (m_basestr == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823756"));
					// In case spell unknown, create it  
					if (spell == null)
					{
						spell = createDBSpell(false, 0, 1, 1451, 1451, BUFFS_SPELL_DURATION, 15, "The strength of the target is increased.", "Strength of the Realm", 1500, 225823756, "Realm", "StrengthBuff", 4);
					}
					m_basestr = new Spell(spell, 10);
				}
				return m_basestr;
			}
		}

		/// <summary>
		/// Merch Base Con buff
		/// </summary>
		public static Spell MerchConBuff
		{
			get
			{
				if (m_basecon == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823757"));
					// In case spell unknown, create it  
					if (spell == null)
					{
						spell = createDBSpell(false, 0, 1, 1481, 1481, BUFFS_SPELL_DURATION, 15, "The constitution of the target is increased.", "Fortitude of the Realm", 1500, 225823757, "Realm", "ConstitutionBuff", 201);
					}
					m_basecon = new Spell(spell, 10);
				}
				return m_basecon;
			}
		}

		/// <summary>
		/// Merch Base Dex buff 
		/// </summary>
		public static Spell MerchDexBuff
		{
			get
			{
				if (m_basedex == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823758"));
					// In case spell unknown, create it  
					if (spell == null)
					{
						spell = createDBSpell(false, 0, 1, 1461, 1461, BUFFS_SPELL_DURATION, 15, "The dexterity of the target is increased.", "Dexterity of the Realm", 1500, 225823758, "Realm", "DexterityBuff", 202);
					}
					m_basedex = new Spell(spell, 10);
				}
				return m_basedex;
			}
		}

		/// <summary>
		/// Merch Spec Str/Con buff
		/// </summary>
		public static Spell MerchStrConBuff
		{
			get
			{
				if (m_strcon == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823760"));
					// In case spell unknown, create it  
					if (spell == null)
					{
						spell = createDBSpell(false, 0, 1, 1511, 1511, BUFFS_SPELL_DURATION, 23, "The strength and constitution of the target are increased.", "Might of the Realm", 1500, 225823760, "Realm", "StrengthConstitutionBuff", 204);
					}
					m_strcon = new Spell(spell, 10);
				}
				return m_strcon;
			}
		}

		/// <summary>
		/// Merch Spec Dex/Qui buff 
		/// </summary>
		public static Spell MerchDexQuiBuff
		{
			get
			{
				if (m_dexqui == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823761"));
					// In case spell unknown, create it  
					if (spell == null)
					{
						spell = createDBSpell(false, 0, 1, 1521, 1521, BUFFS_SPELL_DURATION, 23, "The dexterity and quickness of the target are increased.", "Deftness of the Realm", 1500, 225823761, "Realm", "DexterityQuicknessBuff", 203);
					}
					m_dexqui = new Spell(spell, 10);
				}
				return m_dexqui;
			}
		}

		/// <summary>
		/// Merch Spec Acuity buff
		/// </summary>
		public static Spell MerchAcuityBuff
		{
			get
			{
				if (m_acuity == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823762"));
					// In case spell unknown, create it  
					if (spell == null)
					{
						spell = createDBSpell(false, 0, 1, 1535, 1535, BUFFS_SPELL_DURATION, 22, "The acuity of the target is increased.", "Acuity of the Realm", 1500, 225823762, "Realm", "AcuityBuff", 200);
					}
					m_acuity = new Spell(spell, 10);
				}
				return m_acuity;
			}
		}

		/// <summary>
		/// Merch Spec Af buff
		/// </summary>
		public static Spell MerchSpecAFBuff
		{
			get
			{
				if (m_specaf == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823763"));
					// In case spell unknown, create it  
					if (spell == null)
					{
						spell = createDBSpell(false, 0, 1, 1501, 1501, BUFFS_SPELL_DURATION, 29, "The armor factor (AF) of the target is increased.", "Barrier of the Realm", 1500, 225823763, "Realm", "ArmorFactorBuff", 2);
					}
					m_specaf = new Spell(spell, 10);
				}
				return m_specaf;
			}
		}

		/// <summary>
		/// Merch Haste buff
		/// </summary>
		public static Spell MerchHasteBuff
		{
			get
			{
				if (m_haste == null)
				{
					DBSpell spell = DOLDB<DBSpell>.SelectObject(DB.Column("SpellID").IsEqualTo("225823764"));
					// In case spell unknown, create it  
					if (spell == null)
					{
						spell = createDBSpell(false, 0, 1, 121, 121, BUFFS_SPELL_DURATION, -10, "Increases the target's combat speed.", "Haste of the Realm", 1500, 225823764, "Realm", "HasteBuff", 100);
					}
					m_haste = new Spell(spell, 10);
				}
				return m_haste;
			}
		}

		#endregion Spells

		#endregion SpellCasting

		private void SendReply(GamePlayer target, string msg)
		{
			target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
		}

		public class Container
		{
			private Spell m_spell;
			public Spell Spell
			{
				get { return m_spell; }
			}

			private SpellLine m_spellLine;
			public SpellLine SpellLine
			{
				get { return m_spellLine; }
			}

			private GameLiving m_target;
			public GameLiving Target
			{
				get { return m_target; }
				set { m_target = value; }
			}
			public Container(Spell spell, SpellLine spellLine, GameLiving target)
			{
				m_spell = spell;
				m_spellLine = spellLine;
				m_target = target;
			}
		}

		public override eQuestIndicator GetQuestIndicator(GamePlayer player)
		{
			return eQuestIndicator.Lore ;
		}
		#endregion

		private bool isBounty;
		
		public override bool Interact(GamePlayer player)
		{
			Catalog = MerchantCatalog.LoadFromDatabase("BGRealmEnhancementTokens");
			if (!base.Interact(player)) return false;
			TurnTo(player, 10000);
			player.Out.SendMessage("Greetings, " + player.Name + ". The King has instructed me to strengthen you so that you may defend the lands with valor. Simply hand me the token for the enhancement you desire, and I will empower you accordingly. Do you wish to purchase tokens with [Gold] or [Bounty Points]?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			isBounty = false;
			SendMerchantWindow(player);
			return true;
		}

		public override bool WhisperReceive(GameLiving source, string str)
				{
					if (!base.WhisperReceive(source, str))
						return false;
					GamePlayer player = source as GamePlayer;
					if (player == null) return false;

					switch (str)
					{
						case "Gold":
							{
								TurnTo(player, 10000);
								isBounty = false;
								Catalog = MerchantCatalog.LoadFromDatabase("BGRealmEnhancementTokens");
								SendMerchantWindow(player);
							}
							break;
						case "Bounty Points":
							{
								TurnTo(player, 10000);
								isBounty = true;
								Catalog = MerchantCatalog.LoadFromDatabase("BGBPRealmEnhancementTokens");
								player.Out.SendMerchantWindow(Catalog, eMerchantWindowType.Bp);
							}
							break;
					}
					return true;
				} 

		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			if (isBounty == true) //...pay with Bounty Points.
			{
				int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
				int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

				var template = Catalog.GetEntry(pagenumber, slotnumber).Item;
				if (template == null) return;

				int amountToBuy = number;
				if (template.PackSize > 0)
					amountToBuy *= template.PackSize;

				if (amountToBuy <= 0) return;

				long totalValue = number * (template.Price);

				lock (player.Inventory)
				{
					if (player.BountyPointBalance < totalValue)
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeedBP", totalValue), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (!player.Inventory.AddTemplate(GameInventoryItem.Create(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);

					string message;
					if (number > 1)
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPiecesBP", totalValue, template.GetName(1, false));
					else
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtBP", template.GetName(1, false), totalValue);
					player.RemoveMoney(Currency.BountyPoints.Mint(totalValue));
					player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				}
			}
			if (isBounty == false) //...pay with Money.
			{
				int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
				int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

				var template = Catalog.GetEntry(pagenumber, slotnumber).Item;
				if (template == null) return;

				int amountToBuy = number;
				if (template.PackSize > 0)
					amountToBuy *= template.PackSize;

				if (amountToBuy <= 0) return;

				var totalCost = Currency.Copper.Mint(number * template.Price);

				lock (player.Inventory)
				{

					if (player.CopperBalance < totalCost.Amount)
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeed", totalCost.ToText()), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					if (!player.Inventory.AddTemplate(GameInventoryItem.Create(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);

					string message;
					if (amountToBuy > 1)
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPieces", amountToBuy, template.GetName(1, false), totalCost.ToText());
					else
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.Bought", template.GetName(1, false), totalCost.ToText());

					if (!player.RemoveMoney(totalCost))
					{
						throw new Exception("Money amount changed while adding items.");
					}
					player.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
					InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, totalCost.Amount);
				}
			}
			return;
		}

		#region GiveTokens
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			GamePlayer t = source as GamePlayer;
			
			if (GetDistanceTo(t) > WorldMgr.INTERACT_DISTANCE)
			{
				((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (t != null && item != null)
			{
				if (item.Id_nb == "BG_Realm_Full_Buffs_Token_10" || item.Id_nb == "BG_BP_Realm_Full_Buffs_Token_10")
				{
					BuffPlayer(t, MerchBaseAFBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchStrBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchDexBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchConBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchStrConBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchDexQuiBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchAcuityBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchSpecAFBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchHasteBuff, MerchSpecSpellLine);

					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}

			}
			return base.ReceiveItem(source, item);
		}
	}
}
#endregion

#region Tokens
namespace DOL.GS.Items
{
	public class RealmEnhancementTokens
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_BUFF_TOKENS)
				return;
			
			ItemTemplate item;
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BG_Realm_Full_Buffs_Token_10");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BG_Realm_Full_Buffs_Token_10";
				item.Name = "Full Buffs Token (Level 10+)";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 320000;
				item.Weight = 1;
				item.PackageID = "BG_Realm_Enhancement";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
		}
	}
	public class BPRealmEnhancementTokens
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_BUFF_TOKENS)
				return;
			
			ItemTemplate item;

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BG_BP_Realm_Full_Buffs_Token_10");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BG_BP_Realm_Full_Buffs_Token_10";
				item.Name = "Full Buffs Token (Level 10+)";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 32;
				item.Weight = 1;
				item.PackageID = "BG_Realm_Enhancement";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}


		}
	}
}
#endregion

#region MerchantList
public class RealmEnhancementTokensList
{

	[GameServerStartedEvent]
	public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
	{
		ItemTemplate[] buffMerch = DOLDB<ItemTemplate>.SelectObjects(DB.Column(nameof(ItemTemplate.PackageID)).IsLike("BG_Realm_Enhancement")).OrderBy(it => it.Item_Type).ToArray();
		MerchantItem m_item = null;
		int pagenumber = 0;
		int slotposition = 0;
		m_item = DOLDB<MerchantItem>.SelectObject(DB.Column(nameof(MerchantItem.ItemListID)).IsEqualTo("BGRealmEnhancementTokens"));
		if (m_item == null)
		{
			foreach (ItemTemplate item in buffMerch)
			{
				m_item = new MerchantItem();
				m_item.ItemListID = "BGRealmEnhancementTokens";
				m_item.ItemTemplateID = item.Id_nb;
				m_item.PageNumber = pagenumber;
				m_item.SlotPosition = slotposition;
				m_item.AllowAdd = true;
				GameServer.Database.AddObject(m_item);
				if (slotposition == 29)
				{
					slotposition = 0;
					pagenumber += 1;
				}
				else
				{
					slotposition += 1;
				}
			}
		}
	}
}
public class BPRealmEnhancementTokensList
{

	[GameServerStartedEvent]
	public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
	{
		ItemTemplate[] buffMerch = DOLDB<ItemTemplate>.SelectObjects(DB.Column(nameof(ItemTemplate.PackageID)).IsLike("BG_BP_Realm_Enhancement")).OrderBy(it => it.Item_Type).ToArray();
		MerchantItem m_item = null;
		int pagenumber = 0;
		int slotposition = 0;
		m_item = DOLDB<MerchantItem>.SelectObject(DB.Column(nameof(MerchantItem.ItemListID)).IsEqualTo("BGBPRealmEnhancementTokens"));
		if (m_item == null)
		{
			foreach (ItemTemplate item in buffMerch)
			{
				m_item = new MerchantItem();
				m_item.ItemListID = "BGBPRealmEnhancementTokens";
				m_item.ItemTemplateID = item.Id_nb;
				m_item.PageNumber = pagenumber;
				m_item.SlotPosition = slotposition;
				m_item.AllowAdd = true;
				GameServer.Database.AddObject(m_item);
				if (slotposition == 29)
				{
					slotposition = 0;
					pagenumber += 1;
				}
				else
				{
					slotposition += 1;
				}
			}
		}
	}
}
#endregion

