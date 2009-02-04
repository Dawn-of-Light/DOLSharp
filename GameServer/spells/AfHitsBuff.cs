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
using DOL.Database;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells {

 [SpellHandlerAttribute("AfHitsBuff")]
 public class AfHitsBuffSpellHandler:PropertyChangingSpell {
  public override int BonusCategory1 { get { return 1; } }
  public override eProperty Property1 { get { return eProperty.MaxHealth; } }
  public override void OnEffectStart(GameSpellEffect effect) {
   base.OnEffectStart(effect);
   double bonus;
   if(m_spell.Value<0) { bonus=1+m_spell.Value/-100; }
   else { bonus=1+m_spell.Value/100; }
   effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.ArmorFactor,this,bonus);
    //effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxHealth, this, bonus);
   SendUpdates(effect.Owner); }

 public override int OnEffectExpires(GameSpellEffect effect,bool noMessages) {
  base.OnEffectExpires(effect,noMessages);
  effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.ArmorFactor,this);
  //effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxHealth, this);
  SendUpdates(effect.Owner);
  return 0; }

 protected override void SendUpdates(GameLiving target) {
  GamePlayer player=target as GamePlayer;
  if(player!=null) {
   player.Out.SendCharStatsUpdate();
   player.Out.SendUpdateWeaponAndArmorStats();
   player.UpdatePlayerStatus(); } }
	
 public AfHitsBuffSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) {}

}}

