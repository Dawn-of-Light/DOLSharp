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

using System;
using System.Collections.Generic;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.AI.Brain
{
	public class TurretBrain : ControlledNpc
	{
		private GameLiving m_target;

		public TurretBrain(GameLiving owner) : base(owner) { }

		public override int ThinkInterval
		{
			get { return 1000; }
		}

		protected override bool CheckDefensiveSpells(Spell spell)
		{
			switch (spell.SpellType)
			{
				case "HeatColdMatterBuff":
				case "BodySpiritEnergyBuff":
				case "ArmorAbsorbtionBuff":
					if (Body.TargetObject == null || !(Body.TargetObject as GameLiving).IsAlive || LivingHasEffect(Body.TargetObject as GameLiving, spell))
					{
						Body.TargetObject = null;

						List<GameLiving> list = new List<GameLiving>();

						foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)spell.Range))
						{
							if (GameServer.ServerRules.IsSameRealm(Body, player, true) && !LivingHasEffect(player, spell))
								list.Add(player);
						}

						foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)spell.Range))
						{
							if (GameServer.ServerRules.IsSameRealm(Body, npc, true) && !LivingHasEffect(npc, spell))
								list.Add(npc);
						}

						if (list.Count > 0)
							Body.TargetObject = list[Util.Random(list.Count - 1)];
					}
					break;
				default:
					return false;
			}

			if (Body.TargetObject != null)
			{
				if (!Body.CastSpell(spell, m_mobSpellLine))
					return false;
				if (Body.TargetObject != Body && spell.CastTime > 0)
					Body.TurnTo(Body.TargetObject);
				return true;
			}

			return false;
		}

		protected override bool CheckOffensiveSpells(Spell spell)
		{
			switch (spell.SpellType)
			{
				case "DirectDamage":
				case "DamageSpeedDecrease":
				case "SpeedDecrease":
				case "MeleeDamageDebuff":
				case "Taunt":
					if (Body.TargetObject == null || !(Body.TargetObject as GameLiving).IsAlive)
					{
						Body.TargetObject = null;

						List<GameLiving> list = new List<GameLiving>();

						foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)spell.Range))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Body, player, true) && !player.IsStealthed && !LivingHasEffect(player, spell))
								list.Add(player);
						}

						foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)spell.Range))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, true) && !LivingHasEffect(npc, spell))
								list.Add(npc);
						}

						if (list.Count > 0)
							Body.TargetObject = list[Util.Random(list.Count - 1)];
					}
					break;
				default:
					return false;
			}

			if (Body.TargetObject != null)
			{
				if (!Body.CastSpell(spell, m_mobSpellLine))
					return false;
				if (Body.TargetObject != Body && spell.CastTime > 0)
					Body.TurnTo(Body.TargetObject);
				return true;
			}

			return false;
		}

		#region Think
		public override void Think()
		{
			CheckSpells(eCheckSpellType.Offensive);
			CheckSpells(eCheckSpellType.Defensive);		

			//if (Spell.Target.ToLower() == "realm")
			//{
			//    if (Spell.Range == 0 && Spell.Radius > 0)
			//    {
			//        Body.CastSpell(Spell, SkillBase.GetSpellLine("Mob Spells"));
			//        return;
			//    }
			//    else
			//    {
			//        IList friendly = new ArrayList();
			//        if (Target == null)
			//            friendly = FindTarget(true);
			//        else if (Target == Body)
			//            friendly = FindTarget(true);
			//        else if (!WorldMgr.CheckDistance(Body, Target, Spell.Range))
			//            friendly = FindTarget(true);
			//        else if (!Target.IsAlive)
			//            friendly = FindTarget(true);
			//        else if (Target is GameNPC && (Target as GameNPC).IsRespawning == true)
			//            friendly = FindTarget(true);
			//        else if (Target is GamePlayer && Target.IsStealthed)
			//            friendly = FindTarget(true);

			//        if (friendly.Count > 0 && Target == null)
			//        {
			//            //pick a random target...
			//            int targetnum = Util.Random(0, friendly.Count - 1);

			//            //Choose a random target.
			//            Target = friendly[targetnum] as GameLiving;
			//        }
			//        if (Target != null)
			//        {
			//            if (!Target.IsAlive)
			//                Target = null;
			//            if (Target is GamePlayer && Target.IsStealthed)
			//                Target = null;
			//            //Console.WriteLine(Target.Name);
			//            if (Target != null && WorldMgr.CheckDistance(Body, Target, Spell.Range) && !LivingHasEffect(Target, Spell))
			//            {
			//                if (!Target.IsAlive)
			//                    Target = null;

			//                Body.TargetObject = Target;
			//                Body.TurnTo(Target);
			//                GamePlayer LOSChecker = null;
			//                if (Target is GamePlayer)
			//                    LOSChecker = Target as GamePlayer;
			//                else if (Target is GameNPC)
			//                {
			//                    foreach (GamePlayer ply in this.Body.GetPlayersInRadius(300))
			//                    {
			//                        if (ply != null)
			//                        {
			//                            LOSChecker = ply;
			//                            break;
			//                        }
			//                    }
			//                }
			//                if (LOSChecker == null)
			//                    return;
			//                LOSChecker.Out.SendCheckLOS(LOSChecker, Body, new CheckLOSResponse(this.PetStartSpellAttackCheckLOS));
			//            }
			//            else
			//                Target = null;
			//        }
			//        return;
			//    }
			//}
			//else if (Spell.Target.ToLower() == "enemy")
			//{
			//    if (Spell.Range == 0 && Spell.Radius > 0)
			//    {
			//        //Andraste
			//        Spell.Level = Body.Level;
			//        Body.CastSpell(Spell, SkillBase.GetSpellLine("Reserved Spells"));
			//        return;
			//    }
			//    else
			//    {
			//        IList enemies = new ArrayList();
			//        if (Target == null)
			//            enemies = FindTarget(false);
			//        else if (Target.Health == 0)
			//            FindTarget(false);
			//        else if (!WorldMgr.CheckDistance(Body, Target, Spell.Range))
			//            enemies = FindTarget(false);
			//        else if (!Target.IsAlive)
			//            enemies = FindTarget(false);
			//        else if (Target is GamePlayer && Target.IsStealthed)
			//            enemies = FindTarget(false);
			//        if (enemies.Count > 0 && Target == null)
			//        {
			//            //pick a random target...
			//            int targetnum = Util.Random(0, enemies.Count - 1);

			//            //Choose a random target.
			//            Target = enemies[targetnum] as GameLiving;
			//        }
			//        if (Target != null)
			//        {
			//            if (!Target.IsAlive)
			//                Target = null;
			//            if (Target is GamePlayer && Target.IsStealthed)
			//                Target = null;
			//            else if (Target != null && WorldMgr.CheckDistance(Body, Target, Spell.Range) && !LivingHasEffect(Target, Spell))
			//            {
			//                //Cast Spell
			//                Body.TargetObject = Target;
			//                Body.TurnTo(Target);

			//                GamePlayer LOSChecker = null;
			//                if (Target is GamePlayer)
			//                    LOSChecker = Target as GamePlayer;
			//                else if (Target is GameNPC)
			//                {
			//                    foreach (GamePlayer ply in this.Body.GetPlayersInRadius(300))
			//                    {
			//                        if (ply != null)
			//                        {
			//                            LOSChecker = ply;
			//                            break;
			//                        }
			//                    }
			//                }

			//                if (LOSChecker == null)
			//                    return;

			//                LOSChecker.Out.SendCheckLOS(LOSChecker, Body, new CheckLOSResponse(PetStartSpellAttackCheckLOS));
			//            }
			//            else
			//                Target = null;
			//        }
			//        return;
			//    }
			//}
		}

		public void PetStartSpellAttackCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			//if ((response & 0x100) == 0x100)
				//Body.CastSpell(Spell, SkillBase.GetSpellLine("Reserved Spells"));
		}
		#endregion
	}
}