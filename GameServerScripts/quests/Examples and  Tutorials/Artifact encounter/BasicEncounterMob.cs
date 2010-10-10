using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using log4net;
using System.Reflection;
using DOL.GS.Atlantis;
using DOL.Database;
using DOL.Language;
using DOL.GS.Spells;

namespace DOL.GS.Atlantis
{
	/// <summary>
	/// The base class that most or all ArtifactEncounter mob's should inherit from.
	/// </summary>
	public class BasicEncounterMob : GameNPC
	{
		#region Immunity

		private bool m_immuneToMagic = false;

		/// <summary>
		/// Whether or not this object is immune to magic.
		/// </summary>
		public bool IsImmuneToMagic
		{
			get { return m_immuneToMagic; }
			set { m_immuneToMagic = value; }
		}

		private bool m_immuneToMelee = false;

		/// <summary>
		/// Whether or not this object is immune to melee.
		/// </summary>
		public bool IsImmuneToMelee
		{
			get { return m_immuneToMelee; }
			set { m_immuneToMelee = value; }
		}

		private bool m_immuneToCrush = false;

		/// <summary>
		/// Whether or not this object is immune to crush damage.
		/// </summary>
		public bool IsImmuneToCrush
		{
			get { return m_immuneToCrush; }
			set { m_immuneToCrush = value; }
		}

		private bool m_immuneToSlash = false;

		/// <summary>
		/// Whether or not this object is immune to slash damage.
		/// </summary>
		public bool IsImmuneToSlash
		{
			get { return m_immuneToSlash; }
			set { m_immuneToSlash = value; }
		}

		private bool m_immuneToThrust = false;

		/// <summary>
		/// Whether or not this object is immune to thrust damage.
		/// </summary>
		public bool IsImmuneToThrust
		{
			get { return m_immuneToThrust; }
			set { m_immuneToThrust = value; }
		}

		private bool m_immuneToBody = false;

		/// <summary>
		/// Whether or not this object is immune to body damage.
		/// </summary>
		public bool IsImmuneToBody
		{
			get { return m_immuneToBody; }
			set { m_immuneToBody = value; }
		}

		private bool m_immuneToCold = false;

		/// <summary>
		/// Whether or not this object is immune to cold damage.
		/// </summary>
		public bool IsImmuneToCold
		{
			get { return m_immuneToCold; }
			set { m_immuneToCold = value; }
		}

		private bool m_immuneToEnergy = false;

		/// <summary>
		/// Whether or not this object is immune to energy damage.
		/// </summary>
		public bool IsImmuneToEnergy
		{
			get { return m_immuneToEnergy; }
			set { m_immuneToEnergy = value; }
		}

		private bool m_immuneToHeat = false;

		/// <summary>
		/// Whether or not this object is immune to heat damage.
		/// </summary>
		public bool IsImmuneToHeat
		{
			get { return m_immuneToHeat; }
			set { m_immuneToHeat = value; }
		}

		private bool m_immuneToMatter = false;

		/// <summary>
		/// Whether or not this object is immune to matter damage.
		/// </summary>
		public bool IsImmuneToMatter
		{
			get { return m_immuneToMatter; }
			set { m_immuneToMatter = value; }
		}

		private bool m_immuneToSpirit = false;

		/// <summary>
		/// Whether or not this object is immune to spirit damage.
		/// </summary>
		public bool IsImmuneToSpirit
		{
			get { return m_immuneToSpirit; }
			set { m_immuneToSpirit = value; }
		}
		#endregion Immunity

		public override void SaveIntoDatabase() {}

		public virtual void CastSpellnoLOSchecks(Spell spell, SpellLine line)
		{
			if ((m_runningSpellHandler != null && spell.CastTime > 0))
			{
				Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.AlreadyCasting));
				return;
			}
			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
			if (spellhandler != null)
			{
				m_runningSpellHandler = spellhandler;
				spellhandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
				spellhandler.CastSpell();
			}
			else
			{
				return;
			}
		}

		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			//check for Immunity to Magic or Melee damage.
			#region Immunity
			if ((this.IsImmuneToMagic) && (damageType == eDamageType.Body || damageType == eDamageType.Cold || damageType == eDamageType.Crush || damageType == eDamageType.Energy || damageType == eDamageType.Heat || damageType == eDamageType.Matter || damageType == eDamageType.Spirit))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to magic and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToMelee) && (damageType == eDamageType.Crush || damageType == eDamageType.Slash || damageType == eDamageType.Thrust || damageType == eDamageType.Natural))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to melee and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToCrush) && (damageType == eDamageType.Crush))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to crush and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToSlash) && (damageType == eDamageType.Slash))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to slash and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}
			
			if ((this.IsImmuneToThrust) && (damageType == eDamageType.Thrust))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to thrust and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToBody) && (damageType == eDamageType.Body))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to body and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToCold) && (damageType == eDamageType.Cold))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to cold and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToEnergy) && (damageType == eDamageType.Energy))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to energy and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToHeat) && (damageType == eDamageType.Heat))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to heat and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToMatter) && (damageType == eDamageType.Matter))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to matter and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}

			if ((this.IsImmuneToSpirit) && (damageType == eDamageType.Spirit))
			{
				if (source is GamePlayer)
				{
					GamePlayer player = source as GamePlayer;
					player.Out.SendMessage("The " + this.Name + " is immune to spirit and your damage fails to effect it!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
					return;
				}
				return;
			}
			#endregion Immunity

			base.TakeDamage(source, damageType, damageAmount, criticalAmount);
		}
	}
}

