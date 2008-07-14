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
 * [Ganrod] Nidel 2008-07-08
 * - Turret find target if needed and Cast spell
 */
using DOL.GS;

namespace DOL.AI.Brain
{
  public class TurretFNFBrain : TurretBrain
  {
    public TurretFNFBrain(GameLiving owner) : base(owner)
    {
    }

    /// <summary>
    /// [Ganrod] Nidel:
    /// Cast only Offensive or Defensive spell.
    /// <para>If Offensive spell is true, Defensive spell isn't casted.</para>
    /// </summary>
    public override void Think()
    {
      if(CheckSpells(eCheckSpellType.Offensive))
      {
        AttackMostWanted();
        return;
      }
      if(CheckSpells(eCheckSpellType.Defensive))
      {
        // don't need target for radius spell
        if(((TurretPet) Body).TurretSpell.Radius == 0)
        {
          Body.TargetObject = GetDefensiveTarget(((TurretPet) Body).TurretSpell);
        }
        TrustCast(((TurretPet) Body).TurretSpell);
        return;
      }
    }

    /// <summary>
    /// [Ganrod] Nidel: Find and get random target in radius for Defensive spell, like 1.90 EU off servers.
    /// </summary>
    /// <param name="spell"></param>
    /// <returns></returns>
    public GameLiving GetDefensiveTarget(Spell spell)
    {
      if(Body.TargetObject == null || !((GameLiving) Body.TargetObject).IsAlive || LivingHasEffect(Body.TargetObject as GameLiving, spell))
      {
        Body.TargetObject = null;

        foreach(GamePlayer player in Body.GetPlayersInRadius((ushort) spell.Range))
        {
          //Buff owner at first time if in radius
          if(player == GetPlayerOwner() && !LivingHasEffect(player, spell))
          {
            return player;
          }
          if(ListDefensiveTarget.Contains(player) || LivingHasEffect(player, spell))
          {
            if(LivingHasEffect(player, spell))
            {
              ListDefensiveTarget.Remove(player);
            }
            continue;
          }
          if(GameServer.ServerRules.IsSameRealm(Body, player, false))
          {
            ListDefensiveTarget.Add(player);
          }
        }
        foreach(GameNPC npc in Body.GetNPCsInRadius((ushort) spell.Range))
        {
          if(npc == Body && !LivingHasEffect(Body, spell))
          {
            return Body;
          }
          if(ListDefensiveTarget.Contains(npc) || LivingHasEffect(npc, spell))
          {
            if(LivingHasEffect(npc, spell))
            {
              ListDefensiveTarget.Remove(npc);
            }
            continue;
          }
          if(GameServer.ServerRules.IsSameRealm(Body, npc, false))
          {
            ListDefensiveTarget.Add(npc);
          }
        }
      }
      // Get one random target.
      return ListDefensiveTarget.Count > 0 ? ListDefensiveTarget[Util.Random(ListDefensiveTarget.Count - 1)] : null;
    }

    protected override void AttackMostWanted()
    {
      if(!IsActive)
      {
        return;
      }

      if(((TurretPet) Body).TurretSpell.Radius == 0)
      {
        CheckPlayerAggro();
        CheckNPCAggro();
        GameLiving target = CalculateNextAttackTarget();
        if(target != null)
        {
          Body.TargetObject = target;
          TrustCast(((TurretPet) Body).TurretSpell);
        }
      }
      TrustCast(((TurretPet) Body).TurretSpell);
    }

    /// <summary>
    /// Updates the pet window
    /// </summary>
    public override void UpdatePetWindow()
    {
    }
  }
}