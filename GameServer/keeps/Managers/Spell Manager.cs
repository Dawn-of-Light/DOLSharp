using System;
using System.Collections;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Class to deal with spell casting for the guards
	/// </summary>
	public class SpellMgr
	{
		public static void CheckForNuke(GameKeepGuard guard)
		{
			if(guard==null) return;
			GameLiving target = guard.TargetObject as GameLiving;
			if(target==null) return;
			if(!target.IsAlive) return;
			if(target is GamePlayer && !KeepMgr.IsEnemy(guard, target as GamePlayer, true)) return;
            if ( !guard.IsWithinRadius( target, WorldMgr.VISIBILITY_DISTANCE ) ) { guard.TargetObject = null; return; }
			GamePlayer LOSChecker = null;
			if (target is GamePlayer) LOSChecker = target as GamePlayer;
			else
			{
				foreach (GamePlayer player in guard.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					LOSChecker = player;
					break;
				}
			}
			if (LOSChecker == null)	return;
			LOSChecker.Out.SendCheckLOS(guard, target, new CheckLOSResponse(guard.GuardStartSpellNukeCheckLOS));
		}
		/// <summary>
		/// Method to check the area for heals
		/// </summary>
		/// <param name="guard">The guard object</param>
		public static void CheckAreaForHeals(GameKeepGuard guard)
		{
			GameLiving target = null;
			foreach (GamePlayer player in guard.GetPlayersInRadius(2000))
			{
				if(!player.IsAlive) continue;
				if (GameServer.ServerRules.IsSameRealm(player, guard, true))
				{
					if (player.HealthPercent < 60)
					{
						target = player;
						break;
					}
				}
			}
			if (target == null)
			{
				foreach (GameNPC npc in guard.GetNPCsInRadius(2000))
				{
					if (npc is GameSiegeWeapon) continue;
					if (GameServer.ServerRules.IsSameRealm(npc, guard, true))
					{
						if (npc.HealthPercent < 60)
						{
							target = npc;
							break;
						}
					}
				}
			}

			if (target != null)
			{
				GamePlayer LOSChecker = null;
				if (target is GamePlayer)
				{
					LOSChecker = target as GamePlayer;
				}
				else
				{
					foreach (GamePlayer player in guard.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						LOSChecker = player;
						break;
					}
				}
				if (LOSChecker == null)
					return;
				if(!target.IsAlive) return;
				guard.TargetObject = target;
				LOSChecker.Out.SendCheckLOS(guard, target, new CheckLOSResponse(guard.GuardStartSpellHealCheckLOS));
			}
		}

		/// <summary>
		/// Method for a lord to cast a heal spell
		/// </summary>
		/// <param name="lord">The lord object</param>
		public static void LordCastHealSpell(GameKeepGuard lord)
		{
			//decide which healing spell
			Spell spell = GetLordHealSpell((eRealm)lord.Realm);
			//cast the healing spell
			if (spell != null && !lord.IsStunned && !lord.IsMezzed)
			{
				lord.StopAttack();
				lord.TargetObject = lord;
				lord.CastSpell(spell, SpellMgr.GuardSpellLine);
			}
		}

		/// <summary>
		/// Method to cast a heal spell
		/// </summary>
		/// <param name="guard">The guard object</param>
		/// <param name="target">The spell target</param>
		public static void CastHealSpell(GameNPC guard, GameLiving target)
		{
			//decide which healing spell
			Spell spell = GetGuardHealSmallSpell((eRealm)guard.Realm);
			//cast the healing spell
			if (spell != null && !guard.IsStunned && !guard.IsMezzed  )
			{
				guard.StopAttack();
				guard.TargetObject = target;
				guard.CastSpell(spell, SpellMgr.GuardSpellLine);
			}
		}

		/// <summary>
		/// Method to cast a nuke spell
		/// </summary>
		/// <param name="guard">The guard object</param>
		/// <param name="target">The spell target</param>
		public static void CastNukeSpell(GameNPC guard, GameLiving target)
		{
			guard.TargetObject = target;
			switch (guard.Realm)
			{
				case eRealm.None:
				case eRealm.Albion: LaunchSpell(47, "Pyromancy", guard); break;
				case eRealm.Midgard: LaunchSpell(48, "Runecarving", guard); break;
				case eRealm.Hibernia: LaunchSpell(47, "Way of the Eclipse", guard); break;
			}
		}

		/// <summary>
		/// Method to launch a spell
		/// </summary>
		/// <param name="spellLevel">The spell level</param>
		/// <param name="spellLineName">The spell line</param>
		/// <param name="guard">The guard caster</param>
		public static void LaunchSpell(int spellLevel, string spellLineName, GameNPC guard)
		{
			if (guard.TargetObject == null)
				return;

			Spell castSpell = null;
			SpellLine castLine = null;

			castLine = SkillBase.GetSpellLine(spellLineName);
			List<Spell> spells = SkillBase.GetSpellList(castLine.KeyName);

			foreach (Spell spell in spells)
			{
				if (spell.Level == spellLevel)
				{
					castSpell = spell;
					break;
				}
			}
			if (guard.AttackState)
				guard.StopAttack();
			if (guard.IsMoving)
				guard.StopFollow();
			guard.TurnTo(guard.TargetObject);
			guard.CastSpell(castSpell, castLine);
		}

		public static Spell GetLordHealSpell(eRealm realm)
		{
			switch (realm)
			{
				case eRealm.None:
				case eRealm.Albion:
						return AlbLordHealSpell;
				case eRealm.Midgard:
						return MidLordHealSpell;
				case eRealm.Hibernia:
						return HibLordHealSpell;
			}
			return null;
		}

		public static Spell GetGuardHealSmallSpell(eRealm realm)
		{
			switch (realm)
			{ 
				case eRealm.None:
				case eRealm.Albion:
					return AlbGuardHealSmallSpell;
				case eRealm.Midgard:
					return MidGuardHealSmallSpell;
				case eRealm.Hibernia:
					return HibGuardHealSmallSpell;
			}
			return null;
		}

		#region Spells and Spell Line
		private static SpellLine m_GuardSpellLine;
		/// <summary>
		/// Spell line used by guards
		/// </summary>
		public static SpellLine GuardSpellLine
		{
			get
			{
				if (m_GuardSpellLine == null)
					m_GuardSpellLine = new SpellLine("GuardSpellLine", "Guard Spells", "unknown", false);

				return m_GuardSpellLine;
			}
		}


		private static Spell m_albLordHealSpell;
		private static Spell m_midLordHealSpell;
		private static Spell m_hibLordHealSpell;

		/// <summary>
		/// The spell the Albion Lord uses to heal itself
		/// </summary>
		public static Spell AlbLordHealSpell
		{
			get
			{
				if (m_albLordHealSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 2;
					spell.ClientEffect = 1340;
					spell.Value = 225; //350;
					spell.Name = "Guard Heal";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 90001;
					spell.Target = "Realm";
					spell.Type = "Heal";
					spell.Uninterruptible = true;
					m_albLordHealSpell = new Spell(spell, 50);
				}
				return m_albLordHealSpell;
			}
		}

		/// <summary>
		/// The spell the Midgard Lord uses to heal itself
		/// </summary>
		public static Spell MidLordHealSpell
		{
			get
			{
				if (m_midLordHealSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 2;
					spell.ClientEffect = 3011;
					spell.Value = 225;//350;
					spell.Name = "Guard Heal";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 90002;
					spell.Target = "Realm";
					spell.Type = "Heal";
					spell.Uninterruptible = true;
					m_midLordHealSpell = new Spell(spell, 50);
				}
				return m_midLordHealSpell;
			}
		}

		/// <summary>
		/// The spell the Hibernia Lord uses to heal itself
		/// </summary>
		public static Spell HibLordHealSpell
		{
			get
			{
				if (m_hibLordHealSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 2;
					spell.ClientEffect = 3030;
					spell.Value = 225;//350;
					spell.Name = "Guard Heal";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 90003;
					spell.Target = "Realm";
					spell.Type = "Heal";
					spell.Uninterruptible = true;
					m_hibLordHealSpell = new Spell(spell, 50);
				}
				return m_hibLordHealSpell;
			}
		}

		private static Spell m_albGuardHealSmallSpell;
		private static Spell m_midGuardHealSmallSpell;
		private static Spell m_hibGuardHealSmallSpell;

		/// <summary>
		/// The spell that Albion Guards use to heal small amounts
		/// </summary>
		public static Spell AlbGuardHealSmallSpell
		{
			get
			{
				if (m_albGuardHealSmallSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 2;
					spell.ClientEffect = 1340;
					spell.Value = 200;
					spell.Name = "Guard Heal";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 90004;
					spell.Target = "Realm";
					spell.Type = "Heal";
					//spell.Interruptable = 1;
					m_albGuardHealSmallSpell = new Spell(spell, 50);
				}
				return m_albGuardHealSmallSpell;
			}
		}

		/// <summary>
		/// The spell that Midgard Guards use to heal small amounts
		/// </summary>
		public static Spell MidGuardHealSmallSpell
		{
			get
			{
				if (m_midGuardHealSmallSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 2;
					spell.ClientEffect = 3011;
					spell.Value = 200;
					spell.Name = "Guard Heal";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 90005;
					spell.Target = "Realm";
					spell.Type = "Heal";
					//spell.Interruptable = 1;
					m_midGuardHealSmallSpell = new Spell(spell, 50);
				}
				return m_midGuardHealSmallSpell;
			}
		}

		/// <summary>
		/// The spell that Hibernian Guards use to heal small amounts
		/// </summary>
		public static Spell HibGuardHealSmallSpell
		{
			get
			{
				if (m_hibGuardHealSmallSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 2;
					spell.ClientEffect = 3030;
					spell.Value = 200;
					spell.Name = "Guard Heal";
					spell.Range = WorldMgr.VISIBILITY_DISTANCE;
					spell.SpellID = 90006;
					spell.Target = "Realm";
					spell.Type = "Heal";
					//spell.Interruptable = 1;
					m_hibGuardHealSmallSpell = new Spell(spell, 50);
				}
				return m_hibGuardHealSmallSpell;
			}
		}

		//private static Spell m_albLargeGuardHealSpell;
		//private static Spell m_midLargeGuardHealSpell;
		//private static Spell m_hibLargeGuardHealSpell;

		//private static Spell m_albGuardNukeSpell;
		//private static Spell m_midGuardNukeSpell;
		//private static Spell m_hibGuardNukeSpell;

		//private static Spell m_albGuardDOTSpell;
		//private static Spell m_midGuardDOTSpell;
		//private static Spell m_hibGuardDOTSpell;

		//private static Spell m_albGuardMezSpell;
		//private static Spell m_midGuardMezSpell;
		//private static Spell m_hibGuardMezSpell;

		#endregion
	}
}
