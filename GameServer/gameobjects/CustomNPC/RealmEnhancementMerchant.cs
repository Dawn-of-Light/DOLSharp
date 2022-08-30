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
 * All you need to do when you add this to your core is
 * /mob create DOL.GS.RealmEnhancementMerchant or /mob class DOL.GS.RealmEnhancementMerchant
 * /merchant sell add BGRealmEnhancementTokens
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
					if (spell != null)
					{
						m_baseaf = new Spell(spell, 10);
					}
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
					if (spell != null)
					{
						m_basestr = new Spell(spell, 10);
					}
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
					if (spell != null)
					{
						m_basecon = new Spell(spell, 10);
					}
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
					if (spell != null)
					{
						m_basedex = new Spell(spell, 10);
					}
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
					if (spell != null)
					{
						m_strcon = new Spell(spell, 10);
					}
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
					if (spell != null)
					{
						m_dexqui = new Spell(spell, 10);
					}
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
					if (spell != null)
					{
						m_acuity = new Spell(spell, 10);
					}
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
					if (spell != null)
					{
						m_specaf = new Spell(spell, 10);
					}
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
					if (spell != null)
					{
						m_haste = new Spell(spell, 10);
					}
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

		#endregion

		private bool isBounty;

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
