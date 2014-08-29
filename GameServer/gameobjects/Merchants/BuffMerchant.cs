/* Created by Shawn
 * Date : March 29 2011
 * Version : 1.0
 * 
 * All you need to do when you add this to your core is
 * /mob create DOL.GS.BuffMerchant
 * and then
 * /merchant sell add BuffTokens
 * 
 * Updated by Mattress to apply buffs equally to all class types, set values to match live.
 * Added option to purchase with bounty points.  Feb 6, 2012.
 * 
 * Enjoy!
 */

using System;
using System.Reflection;
using System.Collections;

using DOL.Events;
using DOL.Database;
using DOL.AI.Brain;
using DOL.Language;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;

using log4net;

namespace DOL.GS
{
	public class BuffMerchant : GameMerchant
	{
		#region BuffMerchant attrib/spells/casting
		public BuffMerchant()
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
		private const int BUFFS_SPELL_DURATION = int.MaxValue;
		private const bool BUFFS_PLAYER_PET = true;

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
		private static SpellLine m_MerchOtherSpellLine;

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
		public static SpellLine MerchOtherSpellLine
		{
			get
			{
				if (m_MerchOtherSpellLine == null)
					m_MerchOtherSpellLine = new SpellLine("MerchOtherSpellLine", "BuffMerch Spells", "unknown", true);
				
				return m_MerchOtherSpellLine;
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
		#region Non-live (commented out)
		private static Spell m_powereg;
		private static Spell m_dmgadd;
		private static Spell m_hpRegen;
		#endregion None-live (commented out)
		private static Spell m_speed;
		
		#region Spells

		/// <summary>
		/// Merch Base AF buff (VERIFIED)
		/// </summary>
		public static Spell MerchBaseAFBuff
		{
			get
			{
				if (m_baseaf == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1467;
					spell.Icon = 1467;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 78; //Effective buff 58
					spell.Name = "Armor of the Realm";
					spell.Description = "Adds to the recipient's Armor Factor (AF) resulting in better protection againts some forms of attack. It acts in addition to any armor the target is wearing.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88001;
					spell.Target = "Realm";
					spell.Type = "ArmorFactorBuff";
					spell.EffectGroup = 1;

					m_baseaf = new Spell(spell, 50);
				}
				return m_baseaf;
			}
		}
		/// <summary>
		/// Merch Base Str buff (VERIFIED)
		/// </summary>
		public static Spell MerchStrBuff
		{
			get
			{
				if (m_basestr == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1457;
					spell.Icon = 1457;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 74; //effective buff 55
					spell.Name = "Strength of the Realm";
					spell.Description = "Increases target's Strength.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88002;
					spell.Target = "Realm";
					spell.Type = "StrengthBuff";
					spell.EffectGroup = 4;

					m_basestr = new Spell(spell, 50);
				}
				return m_basestr;
			}
		}

		/// <summary>
		/// Merch Base Con buff (VERIFIED)
		/// </summary>
		public static Spell MerchConBuff
		{
			get
			{
				if (m_basecon == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1486;
					spell.Icon = 1486;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 74; //effective buff 55
					spell.Name = "Fortitude of the Realm";
					spell.Description = "Increases target's Constitution.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88003;
					spell.Target = "Realm";
					spell.Type = "ConstitutionBuff";
					spell.EffectGroup = 201;

					m_basecon = new Spell(spell, 50);
				}
				return m_basecon;
			}
		}

		/// <summary>
		/// Merch Base Dex buff (VERIFIED)
		/// </summary>
		public static Spell MerchDexBuff
		{
			get
			{
				if (m_basedex == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1476;
					spell.Icon = 1476;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 74; //effective buff 55
					spell.Name = "Dexterity of the Realm";
					spell.Description = "Increases Dexterity for a character.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88004;
					spell.Target = "Realm";
					spell.Type = "DexterityBuff";
					spell.EffectGroup = 202;

					m_basedex = new Spell(spell, 50);
				}
				return m_basedex;
			}
		}

		/// <summary>
		/// Merch Spec Str/Con buff (VERIFIED)
		/// </summary>
		public static Spell MerchStrConBuff
		{
			get
			{
				if (m_strcon == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1517;
					spell.Icon = 1517;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 114; //effective buff 85
					spell.Name = "Might of the Realm";
					spell.Description = "Increases Str/Con for a character";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88005;
					spell.Target = "Realm";
					spell.Type = "StrengthConstitutionBuff";
					spell.EffectGroup = 204;

					m_strcon = new Spell(spell, 50);
				}
				return m_strcon;
			}
		}

		/// <summary>
		/// Merch Spec Dex/Qui buff (VERIFIED)
		/// </summary>
		public static Spell MerchDexQuiBuff
		{
			get
			{
				if (m_dexqui == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1526;
					spell.Icon = 1526;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 114; //effective buff 85
					spell.Name = "Deftness of the Realm";
					spell.Description = "Increases Dexterity and Quickness for a character.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88006;
					spell.Target = "Realm";
					spell.Type = "DexterityQuicknessBuff";
					spell.EffectGroup = 203;

					m_dexqui = new Spell(spell, 50);
				}
				return m_dexqui;
			}
		}

		/// <summary>
		/// Merch Spec Acuity buff (VERIFIED)
		/// </summary>
		public static Spell MerchAcuityBuff
		{
			get
			{
				if (m_acuity == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1538;
					spell.Icon = 1538;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 96; //effective buff 72;
					spell.Name = "Acuity of the Realm";
					spell.Description = "Increases Acuity (casting attribute) for a character.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88007;
					spell.Target = "Realm";
					spell.Type = "AcuityBuff";
					spell.EffectGroup = 200;

					m_acuity = new Spell(spell, 50);
				}
				return m_acuity;
			}
		}

		/// <summary>
		/// Merch Spec Af buff (VERIFIED)
		/// </summary>
		public static Spell MerchSpecAFBuff
		{
			get
			{
				if (m_specaf == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1506;
					spell.Icon = 1506;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 90; //effective buff 67
					spell.Name = "Armor of the Realm";
					spell.Description = "Adds to the recipient's Armor Factor (AF), resulting in better protection against some forms of attack. It acts in addition to any armor the target is wearing.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88014;
					spell.Target = "Realm";
					spell.Type = "ArmorFactorBuff";
					spell.EffectGroup = 2;

					m_specaf = new Spell(spell, 50);
				}
				return m_specaf;
			}
		}

		/// <summary>
		/// Merch Haste buff (VERIFIED)
		/// </summary>
		public static Spell MerchHasteBuff
		{
			get
			{
				if (m_haste == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 407;
					spell.Icon = 407;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 15;
					spell.Name = "Haste of the Realm";
					spell.Description = "Increases the target's combat speed.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88010;
					spell.Target = "Realm";
					spell.Type = "CombatSpeedBuff";
					spell.EffectGroup = 100;
					
					m_haste = new Spell(spell, 50);
				}
				return m_haste;
			}
		}
		#region Non-live (commented out)
		
		/// <summary>
		/// Merch Power Reg buff
		/// </summary>
		public static Spell MerchPoweregBuff
		{
			get
			{
				if (m_powereg == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 980;
					spell.Icon = 980;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 30;
					spell.Name = "Power of the Realm";
					spell.Description = "Target regenerates power regeneration during the duration of the spell";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88008;
					spell.Target = "Realm";
					spell.Type = "PowerRegenBuff";
					m_powereg = new Spell(spell, 50);
				}
				return m_powereg;
			}
		}
	   
		/// <summary>
		/// Merch Damage Add buff
		/// </summary>
		public static Spell MerchDmgaddBuff
		{
			get
			{
				if (m_dmgadd == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 18;
					spell.Icon = 18;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Damage = 5.0;
					spell.DamageType = 15;
					spell.Name = "Damage of the Realm";
					spell.Description = "Target's melee attacks do additional damage.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88009;
					spell.Target = "Realm";
					spell.Type = "DamageAdd";
					m_dmgadd = new Spell(spell, 50);
				}
				return m_dmgadd;
			}
		}
		
		/// <summary>
		/// Merch HP Regen buff
		/// </summary>
		public static Spell MerchHPRegenBuff
		{
			get
			{
				if (m_hpRegen == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1534;
					spell.Icon = 1534;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 7;
					spell.Name = "Health of the Realm";
					spell.Description = "Target regenerates the given amount of health every tick";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88011;
					spell.Target = "Realm";
					spell.Type = "HealthRegenBuff";
					m_hpRegen = new Spell(spell, 50);
				}
				return m_hpRegen;
			}
		}
		
		#endregion Non-live (commented out)

		/// <summary>
		/// Merch Speed of the Realm
		/// </summary>
		public static Spell MerchSpeedBuff
		{
			get
			{
				if (m_speed == null)
				{
					m_speed = SkillBase.GetSpellByID(GameHastener.SPEEDOFTHEREALMID);
				}
				return m_speed;
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
			TradeItems = new MerchantTradeItems("BuffTokens");
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
						TradeItems = new MerchantTradeItems("BuffTokens");
						SendMerchantWindow(player);
					}
					break;
				case "Bounty Points":
					{
						TurnTo(player, 10000);
						isBounty = true;
						TradeItems = new MerchantTradeItems("BPBuffTokens");
						player.Out.SendMerchantWindow(TradeItems, eMerchantWindowType.Bp);
					}
					break;
			}
			return true;
		}

		public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			if (isBounty == true) //...pay with Bounty Points.
			{
				byte pagenumber = (byte)(item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS);
				int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

				ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
				if (template == null) return;

				// TODO : Translation
				if (template.PackageID.Contains("BuffTokens") && number > 1)
				{
					player.Out.SendMessage("You can buy only one buff at a time !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				
				int amountToBuy = number;
				if (template.PackSize > 0)
					amountToBuy *= template.PackSize;

				if (amountToBuy <= 0) return;

				long totalValue = number * (template.Price);

				lock (player.Inventory)
				{
					if (player.BountyPoints < totalValue)
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeedBP", totalValue), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					
					
					if (!template.PackageID.Contains("BuffTokens") && !player.Inventory.AddTemplate(GameInventoryItem.Create<ItemTemplate>(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					else
					{
						InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);
					}

					string message;
					if (number > 1)
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPiecesBP", totalValue, template.GetName(1, false));
					else
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtBP", template.GetName(1, false), totalValue);

					// Start buffs right away !
					if(template.PackageID.Contains("BuffTokens")) 
					{
						CastDirectBuffs(player, template);
					}

					player.BountyPoints -= totalValue;
					player.Out.SendUpdatePoints();
					player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				}
			}
			if (isBounty == false) //...pay with Money.
			{
				byte pagenumber = (byte)(item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS);
				int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

				ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
				if (template == null) return;
				
				// TODO : Translation
				if (template.PackageID.Contains("BuffTokens") && number > 1)
				{
					player.Out.SendMessage("You can buy only one buff at a time !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				
				int amountToBuy = number;
				if (template.PackSize > 0)
					amountToBuy *= template.PackSize;

				if (amountToBuy <= 0) return;

				long totalValue = number * template.Price;

				lock (player.Inventory)
				{

					if (player.GetCurrentMoney() < totalValue)
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeed", Money.GetString(totalValue)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					if (!template.PackageID.Contains("BuffTokens") && !player.Inventory.AddTemplate(GameInventoryItem.Create<ItemTemplate>(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					else
					{
						InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);
					}

					string message;
					if (amountToBuy > 1)
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPieces", amountToBuy, template.GetName(1, false), Money.GetString(totalValue));
					else
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.Bought", template.GetName(1, false), Money.GetString(totalValue));

					if(template.PackageID.Contains("BuffTokens")) 
					{
						CastDirectBuffs(player, template);
					}
					
					if (!player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
					{
						throw new Exception("Money amount changed while adding items.");
					}
					
					InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, totalValue);
				}
			}
			return;
		}
			
		#region Throw Buffs
		private bool CastDirectBuffs(GamePlayer t, ItemTemplate item)
		{
			BuffPlayer(t, MerchSpeedBuff, MerchSpecSpellLine);
			switch(item.Id_nb)
			{
				case "Full_Buffs_Token" :
				case "BPFull_Buffs_Token" :
				{
					// Disable base AF for Self-AF classes. (Caster Like)
					switch((eCharacterClass)t.CharacterClass.ID)
					{
						case eCharacterClass.Sorcerer :
						case eCharacterClass.Wizard :
						case eCharacterClass.Eldritch :
						case eCharacterClass.Mentalist :
						case eCharacterClass.Valewalker :
						case eCharacterClass.Bonedancer :
						case eCharacterClass.Cabalist :
						case eCharacterClass.Theurgist :
						case eCharacterClass.Enchanter :
						case eCharacterClass.Animist :
						case eCharacterClass.Runemaster :
						case eCharacterClass.Spiritmaster :
							break;
						
						default:
							BuffPlayer(t, MerchBaseAFBuff, MerchBaseSpellLine);
							break;
					}
					
					BuffPlayer(t, MerchStrBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchDexBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchConBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchSpecAFBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchStrConBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchDexQuiBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchAcuityBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchHasteBuff, MerchSpecSpellLine);
					#region Non-live (commented out)
					//BuffPlayer(t, MerchPoweregBuff, MerchSpecSpellLine);
					//BuffPlayer(t, MerchDmgaddBuff, MerchSpecSpellLine);
					//BuffPlayer(t, MerchHPRegenBuff, MerchSpecSpellLine);
					//BuffPlayer(t, MerchEndRegenBuff, MerchSpecSpellLine);
					//BuffPlayer(t, MerchHealBuff, MerchSpecSpellLine);
					#endregion Non-live (commented out)
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "Specialization_Buffs_Token" :
				case "BPSpecialization_Buffs_Token":
				{
					BuffPlayer(t, MerchSpecAFBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchStrConBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchDexQuiBuff, MerchSpecSpellLine);
					BuffPlayer(t, MerchAcuityBuff, MerchSpecSpellLine);				
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "Baseline_Buffs_Token":
				case "BPBaseline_Buffs_Token":
				{
					// Disable base AF for Self-AF classes. (Caster Like)
					switch((eCharacterClass)t.CharacterClass.ID)
					{
						case eCharacterClass.Sorcerer :
						case eCharacterClass.Wizard :
						case eCharacterClass.Eldritch :
						case eCharacterClass.Mentalist :
						case eCharacterClass.Valewalker :
						case eCharacterClass.Bonedancer :
						case eCharacterClass.Cabalist :
						case eCharacterClass.Theurgist :
						case eCharacterClass.Enchanter :
						case eCharacterClass.Animist :
						case eCharacterClass.Runemaster :
						case eCharacterClass.Spiritmaster :
							break;
						
						default:
							BuffPlayer(t, MerchBaseAFBuff, MerchBaseSpellLine);
							break;
					}
					
					BuffPlayer(t, MerchStrBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchDexBuff, MerchBaseSpellLine);
					BuffPlayer(t, MerchConBuff, MerchBaseSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
					
				}
				case "Strength_Buff_Token": 
				case"BPStrength_Buff_Token":
				{
					BuffPlayer(t, MerchStrBuff, MerchBaseSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "Fortification_Buff_Token":
				case "BPFortification_Buff_Token":
				{
					BuffPlayer(t, MerchConBuff, MerchBaseSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "Dexterity_Buff_Token": 
				case "BPDexterity_Buff_Token":
				{
					BuffPlayer(t, MerchDexBuff, MerchBaseSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "Armor_Buff_Token":
				case "BPArmor_Buff_Token":
				{
					BuffPlayer(t, MerchBaseAFBuff, MerchBaseSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "StrCon_Buff_Token":
				case "BPStrCon_Buff_Token":
				{
					BuffPlayer(t, MerchStrConBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "DexQui_Buff_Token":
				case "BPDexQui_Buff_Token":
				{
					BuffPlayer(t, MerchDexQuiBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "Acu_Buff_Token":
				case "BPAcu_Buff_Token":
				{
					BuffPlayer(t, MerchAcuityBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "SpecAF_Buff_Token":
				case "BPSpecAF_Buff_Token":
				{
					BuffPlayer(t, MerchSpecAFBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "Haste_Buff_Token":
				case "BPHaste_Buff_Token":
				{
					BuffPlayer(t, MerchHasteBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				#region Non-live (commented out)
				case "PowerReg_Buff_Token":
				{
					BuffPlayer(t, MerchPoweregBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "DmgAdd_Buff_Token":
				{
					BuffPlayer(t, MerchDmgaddBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				case "HPReg_Buff_Token":
				{
					BuffPlayer(t, MerchHPRegenBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				#endregion Non-live (commented out)
			}
			return false;
		}
		#endregion

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
				if(CastDirectBuffs(t, item.Template)) 
				{
					t.Inventory.RemoveItem(item);
					return true;
				}
			}
			
			return base.ReceiveItem(source, item);
		}
	}
}
#endregion