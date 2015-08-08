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
		private const int BUFFS_SPELL_DURATION = 7200;
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
		private static Spell m_casterbaseaf;
		private static Spell m_casterbasestr;
		private static Spell m_casterbasecon;
		private static Spell m_casterbasedex;
		private static Spell m_casterstrcon;
		private static Spell m_casterdexqui;
		private static Spell m_casteracuity;
		private static Spell m_casterspecaf;
		private static Spell m_haste;
		#region Non-live (commented out)
		//private static Spell m_powereg;
		//private static Spell m_dmgadd;
		//private static Spell m_hpRegen;
		//private static Spell m_heal;
		#endregion None-live (commented out)

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
		/// Merch Caster Base AF buff (VERIFIED)
		/// </summary>
		public static Spell casterMerchBaseAFBuff
		{
			get
			{
				if (m_casterbaseaf == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1467;
					spell.Icon = 1467;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 58; //Effective buff 58
					spell.Name = "Armor of the Realm";
					spell.Description = "Adds to the recipient's Armor Factor (AF) resulting in better protection againts some forms of attack. It acts in addition to any armor the target is wearing.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 89001;
					spell.Target = "Realm";
					spell.Type = "ArmorFactorBuff";
					spell.EffectGroup = 1;

					m_casterbaseaf = new Spell(spell, 50);
				}
				return m_casterbaseaf;
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
		/// Merch Caster Base Str buff (VERIFIED)
		/// </summary>
		public static Spell casterMerchStrBuff
		{
			get
			{
				if (m_casterbasestr == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1457;
					spell.Icon = 1457;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 55; //effective buff 55
					spell.Name = "Strength of the Realm";
					spell.Description = "Increases target's Strength.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 89002;
					spell.Target = "Realm";
					spell.Type = "StrengthBuff";
					spell.EffectGroup = 4;

					m_casterbasestr = new Spell(spell, 50);
				}
				return m_casterbasestr;
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
		/// Merch Caster Base Con buff (VERIFIED)
		/// </summary>
		public static Spell casterMerchConBuff
		{
			get
			{
				if (m_casterbasecon == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1486;
					spell.Icon = 1486;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 55; //effective buff 55
					spell.Name = "Fortitude of the Realm";
					spell.Description = "Increases target's Constitution.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 89003;
					spell.Target = "Realm";
					spell.Type = "ConstitutionBuff";
					spell.EffectGroup = 201;

					m_casterbasecon = new Spell(spell, 50);
				}
				return m_casterbasecon;
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
		/// Merch Caster Base Dex buff (VERIFIED)
		/// </summary>
		public static Spell casterMerchDexBuff
		{
			get
			{
				if (m_casterbasedex == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1476;
					spell.Icon = 1476;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 55; //effective buff 55
					spell.Name = "Dexterity of the Realm";
					spell.Description = "Increases Dexterity for a character.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 89004;
					spell.Target = "Realm";
					spell.Type = "DexterityBuff";
					spell.EffectGroup = 202;

					m_casterbasedex = new Spell(spell, 50);
				}
				return m_casterbasedex;
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
		/// Merch Caster Spec Str/Con buff (VERIFIED)
		/// </summary>
		public static Spell casterMerchStrConBuff
		{
			get
			{
				if (m_casterstrcon == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1517;
					spell.Icon = 1517;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 85; //effective buff 85
					spell.Name = "Might of the Realm";
					spell.Description = "Increases Str/Con for a character";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 89005;
					spell.Target = "Realm";
					spell.Type = "StrengthConstitutionBuff";
					spell.EffectGroup = 204;

					m_casterstrcon = new Spell(spell, 50);
				}
				return m_casterstrcon;
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
		/// Merch Caster Spec Dex/Qui buff (VERIFIED)
		/// </summary>
		public static Spell casterMerchDexQuiBuff
		{
			get
			{
				if (m_casterdexqui == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1526;
					spell.Icon = 1526;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 85; //effective buff 85
					spell.Name = "Deftness of the Realm";
					spell.Description = "Increases Dexterity and Quickness for a character.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 89006;
					spell.Target = "Realm";
					spell.Type = "DexterityQuicknessBuff";
					spell.EffectGroup = 203;

					m_casterdexqui = new Spell(spell, 50);
				}
				return m_casterdexqui;
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
		/// Merch Caster Spec Acuity buff (VERIFIED)
		/// </summary>
		public static Spell casterMerchAcuityBuff
		{
			get
			{
				if (m_casteracuity == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1538;
					spell.Icon = 1538;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 72; //effective buff 72;
					spell.Name = "Acuity of the Realm";
					spell.Description = "Increases Acuity (casting attribute) for a character.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 89007;
					spell.Target = "Realm";
					spell.Type = "AcuityBuff";
					spell.EffectGroup = 200;

					m_casteracuity = new Spell(spell, 50);
				}
				return m_casteracuity;
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
		/// Merch Caster Spec Af buff (VERIFIED)
		/// </summary>
		public static Spell casterMerchSpecAFBuff
		{
			get
			{
				if (m_casterspecaf == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1506;
					spell.Icon = 1506;
					spell.Duration = BUFFS_SPELL_DURATION;
					spell.Value = 67; //effective buff 67
					spell.Name = "Armor of the Realm";
					spell.Description = "Adds to the recipient's Armor Factor (AF), resulting in better protection against some forms of attack. It acts in addition to any armor the target is wearing.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 89014;
					spell.Target = "Realm";
					spell.Type = "ArmorFactorBuff";
					spell.EffectGroup = 2;

					m_casterspecaf = new Spell(spell, 50);
				}
				return m_casterspecaf;
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
		/*
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
		
		/// <summary>
		/// Merch Heal buff
		/// </summary>
		public static Spell MerchHealBuff
		{
			get
			{
				if (m_heal == null)
				{
					DBSpell spell = new DBSpell();
					spell.AllowAdd = false;
					spell.CastTime = 0;
					spell.Concentration = 1;
					spell.ClientEffect = 1424;
					spell.Value = 3000;
					spell.Name = "Blessed Health of the Realm";
					spell.Description = "Heals the target.";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 88013;
					spell.Target = "Realm";
					spell.Type = "Heal";
					m_heal = new Spell(spell, 50);
				}
				return m_heal;
			}
		}
		 */
		#endregion Non-live (commented out)

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
				int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
				int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

				ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
				if (template == null) return;

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
					player.BountyPoints -= totalValue;
					player.Out.SendUpdatePoints();
					player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				}
			}
			if (isBounty == false) //...pay with Money.
			{
				int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
				int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

				ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
				if (template == null) return;

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

					if (!player.Inventory.AddTemplate(GameInventoryItem.Create(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);

					string message;
					if (amountToBuy > 1)
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPieces", amountToBuy, template.GetName(1, false), Money.GetString(totalValue));
					else
						message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.Bought", template.GetName(1, false), Money.GetString(totalValue));

					if (!player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
					{
						throw new Exception("Money amount changed while adding items.");
					}
					InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, totalValue);
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
				if (item.Id_nb == "Full_Buffs_Token" || item.Id_nb == "BPFull_Buffs_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchBaseAFBuff, MerchBaseSpellLine);
						BuffPlayer(t, casterMerchStrBuff, MerchBaseSpellLine);
						BuffPlayer(t, casterMerchDexBuff, MerchBaseSpellLine);
						BuffPlayer(t, casterMerchConBuff, MerchBaseSpellLine);
						BuffPlayer(t, casterMerchSpecAFBuff, MerchSpecSpellLine);
						BuffPlayer(t, casterMerchStrConBuff, MerchSpecSpellLine);
						BuffPlayer(t, casterMerchDexQuiBuff, MerchSpecSpellLine);
						BuffPlayer(t, casterMerchAcuityBuff, MerchSpecSpellLine);
						BuffPlayer(t, MerchHasteBuff, MerchSpecSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchBaseAFBuff, MerchBaseSpellLine);
						BuffPlayer(t, MerchStrBuff, MerchBaseSpellLine);
						BuffPlayer(t, MerchDexBuff, MerchBaseSpellLine);
						BuffPlayer(t, MerchConBuff, MerchBaseSpellLine);
						BuffPlayer(t, MerchSpecAFBuff, MerchSpecSpellLine);
						BuffPlayer(t, MerchStrConBuff, MerchSpecSpellLine);
						BuffPlayer(t, MerchDexQuiBuff, MerchSpecSpellLine);
						BuffPlayer(t, MerchAcuityBuff, MerchSpecSpellLine);
						BuffPlayer(t, MerchHasteBuff, MerchSpecSpellLine);
					}
					#region Non-live (commented out)
					//BuffPlayer(t, MerchPoweregBuff, MerchSpecSpellLine);
					//BuffPlayer(t, MerchDmgaddBuff, MerchSpecSpellLine);
					//BuffPlayer(t, MerchHPRegenBuff, MerchSpecSpellLine);
					//BuffPlayer(t, MerchEndRegenBuff, MerchSpecSpellLine);
					//BuffPlayer(t, MerchHealBuff, MerchSpecSpellLine);
					#endregion Non-live (commented out)
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "Specialization_Buffs_Token" || item.Id_nb == "BPSpecialization_Buffs_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchSpecAFBuff, MerchSpecSpellLine);
						BuffPlayer(t, casterMerchStrConBuff, MerchSpecSpellLine);
						BuffPlayer(t, casterMerchDexQuiBuff, MerchSpecSpellLine);
						BuffPlayer(t, casterMerchAcuityBuff, MerchSpecSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchSpecAFBuff, MerchSpecSpellLine);
						BuffPlayer(t, MerchStrConBuff, MerchSpecSpellLine);
						BuffPlayer(t, MerchDexQuiBuff, MerchSpecSpellLine);
						BuffPlayer(t, MerchAcuityBuff, MerchSpecSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;

				}
				if (item.Id_nb == "Baseline_Buffs_Token" || item.Id_nb == "BPBaseline_Buffs_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchBaseAFBuff, MerchBaseSpellLine);
						BuffPlayer(t, casterMerchStrBuff, MerchBaseSpellLine);
						BuffPlayer(t, casterMerchDexBuff, MerchBaseSpellLine);
						BuffPlayer(t, casterMerchConBuff, MerchBaseSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchBaseAFBuff, MerchBaseSpellLine);
						BuffPlayer(t, MerchStrBuff, MerchBaseSpellLine);
						BuffPlayer(t, MerchDexBuff, MerchBaseSpellLine);
						BuffPlayer(t, MerchConBuff, MerchBaseSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "Strength_Buff_Token" || item.Id_nb == "BPStrength_Buff_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchStrBuff, MerchBaseSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchStrBuff, MerchBaseSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "Fortification_Buff_Token" || item.Id_nb == "BPFortification_Buff_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchConBuff, MerchBaseSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchConBuff, MerchBaseSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "Dexterity_Buff_Token" || item.Id_nb == "BPDexterity_Buff_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchDexBuff, MerchBaseSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchDexBuff, MerchBaseSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "Armor_Buff_Token" || item.Id_nb == "BPArmor_Buff_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchBaseAFBuff, MerchBaseSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchBaseAFBuff, MerchBaseSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "StrCon_Buff_Token" || item.Id_nb == "BPStrCon_Buff_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchStrConBuff, MerchSpecSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchStrConBuff, MerchSpecSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "DexQui_Buff_Token" || item.Id_nb == "BPDexQui_Buff_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchDexQuiBuff, MerchSpecSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchDexQuiBuff, MerchSpecSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "Acu_Buff_Token" || item.Id_nb == "BPAcu_Buff_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchAcuityBuff, MerchSpecSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchAcuityBuff, MerchSpecSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "SpecAF_Buff_Token" || item.Id_nb == "BPSpecAF_Buff_Token")
				{
					if (t.CharacterClass.ClassType == eClassType.ListCaster)
					{
						BuffPlayer(t, casterMerchSpecAFBuff, MerchSpecSpellLine);
					}
					else
					{
						BuffPlayer(t, MerchSpecAFBuff, MerchSpecSpellLine);
					}
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "Haste_Buff_Token" || item.Id_nb == "BPHaste_Buff_Token")
				{
					BuffPlayer(t, MerchHasteBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				#region Non-live (commented out)
				/*
				if (item.Id_nb == "PowerReg_Buff_Token")
				{
					BuffPlayer(t, MerchPoweregBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "DmgAdd_Buff_Token")
				{
					BuffPlayer(t, MerchDmgaddBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				if (item.Id_nb == "HPReg_Buff_Token")
				{
					BuffPlayer(t, MerchHPRegenBuff, MerchSpecSpellLine);
					t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					t.Inventory.RemoveItem(item);
					return true;
				}
				 */
				#endregion Non-live (commented out)
			}
			#region Non-live (commented out)
			/*if (item.Id_nb == "EnduReg_Buff_Token")
			{
				BuffPlayer(t, MerchEndRegenBuff, MerchSpecSpellLine);
				t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				t.Inventory.RemoveItem(item);
				return true;
			}
			if (item.Id_nb == "Heal_Buff_Token")
			{
				BuffPlayer(t, MerchHealBuff, MerchSpecSpellLine);
				t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				t.Inventory.RemoveItem(item);
				return true;
			}
			if (item.Id_nb == "Otherline_Buffs_Token")
			{
				BuffPlayer(t, MerchPoweregBuff, MerchSpecSpellLine);
				BuffPlayer(t, MerchDmgaddBuff, MerchSpecSpellLine);
				BuffPlayer(t, MerchHasteBuff, MerchSpecSpellLine);
				BuffPlayer(t, MerchHPRegenBuff, MerchSpecSpellLine);
				//BuffPlayer(t, MerchEndRegenBuff, MerchSpecSpellLine);
				BuffPlayer(t, MerchHealBuff, MerchSpecSpellLine);
				t.Out.SendMessage("Fight well, " + t.RaceName + ".", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				t.Inventory.RemoveItem(item);
				return true;
			}
			 */
			#endregion Non-live (commented out)
			return base.ReceiveItem(source, item);
		}
	}
}
#endregion

#region Tokens
namespace DOL.GS.Items
{
	public class BuffTokens
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_BUFF_TOKENS)
				return;
			
			ItemTemplate item;
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Full_Buffs_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Full_Buffs_Token";
				item.Name = "Full Buffs Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 320000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Specialization_Buffs_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Specialization_Buffs_Token";
				item.Name = "Specialization Buffs Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 240000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Baseline_Buffs_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Baseline_Buffs_Token";
				item.Name = "Baseline Buffs Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 80000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Strength_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Strength_Buff_Token";
				item.Name = "Strength Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 20000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Fortification_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Fortification_Buff_Token";
				item.Name = "Fortification Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 20000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Dexterity_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Dexterity_Buff_Token";
				item.Name = "Dexertity Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 20000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Armor_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Armor_Buff_Token";
				item.Name = "Armor Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 20000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("StrCon_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "StrCon_Buff_Token";
				item.Name = "Might Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 40000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("DexQui_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "DexQui_Buff_Token";
				item.Name = "Deftness Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 40000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Acu_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Acu_Buff_Token";
				item.Name = "Enlightenment Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 40000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("SpecAF_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "SpecAF_Buff_Token";
				item.Name = "Barrier Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 60000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Haste_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Haste_Buff_Token";
				item.Name = "Haste Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 60000;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			#region Non-live (commented out)
			/*
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Otherline_Buffs_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Otherline_Buffs_Token";
				item.Name = "Extended Buffs Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("DmgAdd_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "DmgAdd_Buff_Token";
				item.Name = "Damage Add Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("PowerReg_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "PowerReg_Buff_Token";
				item.Name = "Power Regen Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("HPReg_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "HPReg_Buff_Token";
				item.Name = "HP Regen Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
 
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("EnduReg_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "EnduReg_Buff_Token";
				item.Name = "Endurance Regen Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Heal_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Heal_Buff_Token";
				item.Name = "Heal Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			 */
			#endregion Non-live (commented out)
		}
	}
	public class BPBuffTokens
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			if (!ServerProperties.Properties.LOAD_BUFF_TOKENS)
				return;
			
			ItemTemplate item;

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPFull_Buffs_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPFull_Buffs_Token";
				item.Name = "Full Buffs Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 32;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPSpecialization_Buffs_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPSpecialization_Buffs_Token";
				item.Name = "Specialization Buffs Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 24;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPBaseline_Buffs_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPBaseline_Buffs_Token";
				item.Name = "Baseline Buffs Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 8;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPStrength_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPStrength_Buff_Token";
				item.Name = "Strength Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 2;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPFortification_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPFortification_Buff_Token";
				item.Name = "Fortification Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 2;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPDexterity_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPDexterity_Buff_Token";
				item.Name = "Dexertity Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 2;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPArmor_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPArmor_Buff_Token";
				item.Name = "Armor Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 2;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPStrCon_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPStrCon_Buff_Token";
				item.Name = "Might Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 4;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPDexQui_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPDexQui_Buff_Token";
				item.Name = "Deftness Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 4;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPAcu_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPAcu_Buff_Token";
				item.Name = "Enlightenment Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 4;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPSpecAF_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPSpecAF_Buff_Token";
				item.Name = "Barrier Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 6;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("BPHaste_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "BPHaste_Buff_Token";
				item.Name = "Haste Buff Token";
				item.Level = 1;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 6;
				item.Weight = 1;
				item.PackageID = "BPBuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			#region Non-live (commented out)
			/*
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Otherline_Buffs_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Otherline_Buffs_Token";
				item.Name = "Extended Buffs Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("DmgAdd_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "DmgAdd_Buff_Token";
				item.Name = "Damage Add Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("PowerReg_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "PowerReg_Buff_Token";
				item.Name = "Power Regen Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("HPReg_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "HPReg_Buff_Token";
				item.Name = "HP Regen Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
 
			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("EnduReg_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "EnduReg_Buff_Token";
				item.Name = "Endurance Regen Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0; // Change to your pricing
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}

			item = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Heal_Buff_Token");
			if (item == null)
			{
				item = new ItemTemplate();
				item.Id_nb = "Heal_Buff_Token";
				item.Name = "Heal Buff Token";
				item.Level = 50;
				item.Item_Type = 40;
				item.Model = 485;
				item.IsDropable = false;
				item.IsPickable = true;
				item.Price = 0;
				item.Weight = 1;
				item.PackageID = "BuffTokens";
				GameServer.Database.AddObject(item);
				if (log.IsDebugEnabled)
					log.Debug("Added " + item.Id_nb);
			}
			 */
			#endregion Non-live (commented out)
		}
	}
}
#endregion

#region MerchantList
public class BuffTokensList
{

	[GameServerStartedEvent]
	public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
	{
		ItemTemplate[] buffMerch = (ItemTemplate[])GameServer.Database.SelectObjects<ItemTemplate> ("PackageID like '" + GameServer.Database.Escape("BuffTokens") + "' ORDER by Item_Type");
		MerchantItem m_item = null;
		int pagenumber = 0;
		int slotposition = 0;
		m_item = (MerchantItem)GameServer.Database.SelectObject<MerchantItem>("ItemListID='BuffTokens'");
		if (m_item == null)
		{
			foreach (ItemTemplate item in buffMerch)
			{
				m_item = new MerchantItem();
				m_item.ItemListID = "BuffTokens";
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
public class BPBuffTokensList
{

	[GameServerStartedEvent]
	public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
	{
		ItemTemplate[] buffMerch = (ItemTemplate[])GameServer.Database.SelectObjects<ItemTemplate>("PackageID like '" + GameServer.Database.Escape("BPBuffTokens") + "' ORDER by Item_Type");
		MerchantItem m_item = null;
		int pagenumber = 0;
		int slotposition = 0;
		m_item = (MerchantItem)GameServer.Database.SelectObject<MerchantItem>("ItemListID='BPBuffTokens'");
		if (m_item == null)
		{
			foreach (ItemTemplate item in buffMerch)
			{
				m_item = new MerchantItem();
				m_item.ItemListID = "BPBuffTokens";
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

