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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandler("IllusionBladeSummon")]
	public class IllusionBladeSummon : SummonSpellHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			//Template of the Illusionblade NPC
			INpcTemplate template = NpcTemplateMgr.GetTemplate(71980);
			
			
			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
				MessageToCaster("NPC template 71980 not found!", eChatType.CT_System);
				return;
			}

			GameSpellEffect effect = CreateSpellEffect(target, effectiveness);

			IControlledBrain brain = GetPetBrain(Caster);
			pet = GetGamePet(template);
			//brain.WalkState = eWalkState.Stay;
			pet.SetOwnBrain(brain as AI.ABrain);

			int x, y, z;
			ushort heading;
			Region region;

			GetPetLocation(out x, out y, out z, out heading, out region);

			pet.X = x;
			pet.Y = y;
			pet.Z = z;
			pet.Heading = heading;
			pet.CurrentRegion = region;

			pet.CurrentSpeed = 0;
			pet.Realm = Caster.Realm;
			pet.Level = Caster.Level;
			pet.AddToWorld();
			//Check for buffs
			if (brain is ControlledNpcBrain)
				(brain as ControlledNpcBrain).CheckSpells(StandardMobBrain.eCheckSpellType.Defensive);

			AddHandlers();

			SetBrainToOwner(brain);
			pet.AutoSetStats();

			effect.Start(pet);

			
			
			//Set pet infos & Brain
			
		}

		protected override GamePet GetGamePet(INpcTemplate template) { return new IllusionBladePet(template); }
		protected override IControlledBrain GetPetBrain(GameLiving owner) { return new ProcPetBrain(owner); }
		protected override void SetBrainToOwner(IControlledBrain brain) { }
		protected override void AddHandlers() { GameEventMgr.AddHandler(pet, GameLivingEvent.AttackFinished, EventHandler); }

		protected void EventHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
			if(args == null || args.AttackData == null)
				return;
		}
		public IllusionBladeSummon(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line) { }
	}
}

namespace DOL.GS
{
	public class IllusionBladePet : GamePet
	{
		public override int MaxHealth
		{
			get { return Level * 10; }
		}
		public override void OnAttackedByEnemy(AttackData ad) { }
		public IllusionBladePet (INpcTemplate npcTemplate) : base(npcTemplate) { }
	}
}

