// Andraste v2.0 - Vico

using System.Collections.Generic;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS.Spells
{
    [SpellHandler("RealmLore")]
    public class RealmLore : SpellHandler
    {
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (!base.CheckBeginCast(selectedTarget))
            {
                return false;
            }

            if (selectedTarget == null)
            {
                return false;
            }

            if (selectedTarget is GameNPC)
            {
                MessageToCaster("This spell works only on players.", eChatType.CT_SpellResisted); return false;
            }

            if (selectedTarget as GamePlayer == null)
            {
                return false;
            }

            if (!Caster.IsWithinRadius(selectedTarget, Spell.Range))
            {
                MessageToCaster("Your target is too far away.", eChatType.CT_SpellResisted); return false;
            }

            return true;
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (!(target is GamePlayer player))
            {
                return;
            }

            var text = new List<string>
            {
                $"Class: {player.CharacterClass.Name}",
                $"Realmpoints: {player.RealmPoints} = {player.RealmLevel + 10:#L#} {player.RealmRankTitle(player.Client.Account.Language)}",
                "----------------------------------------------------",
                $"Str: {player.Strength} Dex: {player.Dexterity} Con: {player.Constitution}",
                $"Qui: {player.Quickness} Emp: {player.Empathy} Cha: {player.Charisma}",
                $"Pie: {player.Piety} Int: {player.Intelligence} HP: {player.MaxHealth}",
                "----------------------------------------------------"
            };

            IList<Specialization> specs = player.GetSpecList();
            foreach (object obj in specs)
            {
                if (obj is Specialization)
                {
                    text.Add($"{((Specialization) obj).Name}: {((Specialization) obj).Level}");
                }
            }

            text.Add("----------------------------------------------------");
            IList abilities = player.GetAllAbilities();
            foreach (Ability ab in abilities)
            {
                if (ab is RealmAbility ability && ability is RR5RealmAbility == false)
                {
                    text.Add(ability.Name);
                }
            } 

            (Caster as GamePlayer)?.Out.SendCustomTextWindow($"Realm Lore [ {player.Name} ]",text);
            (Caster as GamePlayer)?.Out.SendMessage($"Realm Lore [ {player.Name} ]\n{text}",eChatType.CT_System,eChatLoc.CL_SystemWindow);
        }

        public RealmLore(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}