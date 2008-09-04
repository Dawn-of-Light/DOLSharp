using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Scripts;
using DOL.Events;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Arms Length Realm Ability
	/// </summary>
	public class SonicBarrierAbility : RR5RealmAbility
	{
		public SonicBarrierAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GamePlayer player = living as GamePlayer;
            if (player.Group != null)
            {
                foreach (GamePlayer member in player.Group.GetPlayersInTheGroup())
                {
                    if (member.CharacterClass.ID == 1 || member.CharacterClass.ID == 2) // Plate
                    {
                        Spell Spell1 = SkillBase.GetSpellByID(36005); // 34 % Absorb-Spell
                        ISpellHandler spellhandler1 = ScriptMgr.CreateSpellHandler(player, Spell1, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                        spellhandler1.StartSpell(member);
                    }
                    else if (member.CharacterClass.ID == 9 || member.CharacterClass.ID == 10 || member.CharacterClass.ID == 23 || member.CharacterClass.ID == 49 || member.CharacterClass.ID == 58)    // Leather
                    {
                        Spell Spell2 = SkillBase.GetSpellByID(36002); // 10 % Absorb-Spell
                        ISpellHandler spellhandler2 = ScriptMgr.CreateSpellHandler(player, Spell2, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                        spellhandler2.StartSpell(member);
                    }
                    else if (member.CharacterClass.ID == 3 || member.CharacterClass.ID == 25 || member.CharacterClass.ID == 31 || member.CharacterClass.ID == 32 || member.CharacterClass.ID == 43 || member.CharacterClass.ID == 48 || member.CharacterClass.ID == 50 || member.CharacterClass.ID == 25) // Studderd
                    {
                        Spell Spell3 = SkillBase.GetSpellByID(36003); // 19 % Absorb-Spell
                        ISpellHandler spellhandler3 = ScriptMgr.CreateSpellHandler(player, Spell3, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                        spellhandler3.StartSpell(member);
                    }
                    else if (member.CharacterClass.ID == 4 || member.CharacterClass.ID == 6 || member.CharacterClass.ID == 11 || member.CharacterClass.ID == 19 || member.CharacterClass.ID == 21 || member.CharacterClass.ID == 22 || member.CharacterClass.ID == 24 || member.CharacterClass.ID == 26 || member.CharacterClass.ID == 28 || member.CharacterClass.ID == 34 || member.CharacterClass.ID == 44 || member.CharacterClass.ID == 45 || member.CharacterClass.ID == 46 || member.CharacterClass.ID == 47) // Chain 
                    {
                        Spell Spell4 = SkillBase.GetSpellByID(36004);// 24 % Absorb-Spell			
                        ISpellHandler spellhandler4 = ScriptMgr.CreateSpellHandler(player, Spell4, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                        spellhandler4.StartSpell(member);
                    }
                    else
                    {
                        Spell Spell5 = SkillBase.GetSpellByID(36001); // 0 % Absorb-Spell
                        ISpellHandler spellhandler5 = ScriptMgr.CreateSpellHandler(player, Spell5, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
                        spellhandler5.StartSpell(member);
                    }
                }
            }
            else
            {
                player.Out.SendMessage("You need a group for this Ability!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return;
            }
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 420;
		}

		public override void AddEffectsInfo(System.Collections.IList list)
		{
			list.Add("Sonic Barrier.");
			list.Add("");
			list.Add("Target: Group");
			list.Add("Duration: 45 sec");
			list.Add("Casting time: instant");
		}

	}
}
