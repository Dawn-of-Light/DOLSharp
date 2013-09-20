using System;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	public class WrathofChampionsAbility : TimedRealmAbility
	{
        public WrathofChampionsAbility(DBAbility dba, int level) : base(dba, level) { }

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer caster = living as GamePlayer;
			if (caster == null)
				return;

			Int32 dmgValue = 0;
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (Level)
				{
					case 1: dmgValue = 200; break;
					case 2: dmgValue = 350; break;
					case 3: dmgValue = 500; break;
					case 4: dmgValue = 625; break;
					case 5: dmgValue = 750; break;
				}
			}
			else
			{
				switch (Level)
				{
					case 1: dmgValue = 200; break;
					case 2: dmgValue = 500; break;
					case 3: dmgValue = 750; break;
				}
			}

			//send cast messages
			foreach (GamePlayer i_player in caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (i_player == caster)
				{
					i_player.MessageToSelf("You cast " + this.Name + "!", eChatType.CT_Spell);
				}
				else
				{
					i_player.MessageFromArea(caster, caster.Name + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}

			//deal damage to npcs
			foreach (GameNPC mob in caster.GetNPCsInRadius(200))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(caster, mob, true) == false) continue;

				mob.TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);
				caster.Out.SendMessage("You hit the " + mob.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				foreach (GamePlayer player2 in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player2.Out.SendSpellCastAnimation(caster, 4468, 0);
					player2.Out.SendSpellEffectAnimation(caster, mob, 4468, 0, false, 1);
				}
			}

			//deal damage to players
			foreach (GamePlayer t_player in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(caster, t_player, true) == false)
					continue;
					
                //Check to see if the player is phaseshifted
                GameSpellEffect phaseshift;
                phaseshift = SpellHandler.FindEffectOnTarget(t_player, "Phaseshift");
                if (phaseshift != null)
                {
                    caster.Out.SendMessage(t_player.Name + " is Phaseshifted and can't be effected by this Spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    continue;
                }

				if (!caster.IsWithinRadius( t_player, 200 ))
					continue;
				t_player.TakeDamage(caster, eDamageType.Spirit, dmgValue, 0);

				// send a message
				caster.Out.SendMessage("You hit " + t_player.Name + " for " + dmgValue + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				t_player.Out.SendMessage(caster.Name + " hits you for " + dmgValue + " damage.", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);

				foreach (GamePlayer n_player in t_player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					n_player.Out.SendSpellCastAnimation(caster, 4468, 0);
					n_player.Out.SendSpellEffectAnimation(caster, t_player, 4468, 0, false, 1);
				}
			}
			DisableSkill(living);
			caster.LastAttackTickPvP = caster.CurrentRegion.Time;
		}

        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
