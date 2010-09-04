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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.AI.Brain;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Zo' Arkat summoning
    /// </summary>
    [SpellHandlerAttribute("ZoSummon")]
    public class BracerOfZo : SpellHandler
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override bool IsUnPurgeAble { get { return true; } }
		
		protected ZoarkatPet[] deamons = new ZoarkatPet[3];
        
		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			if(Caster.TargetObject as GameLiving==null) return;
			GamePlayer player = Caster as GamePlayer;
			if (player == null)	return;
 
            INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
            if (template == null)
			{
				String errorMessage = String.Format("NPC template {0} is missing, spell ID = {1}", Spell.LifeDrainReturn, Spell.ID);
				if (log.IsWarnEnabled) log.Warn(errorMessage);
				if (player.Client.Account.PrivLevel > 1) MessageToCaster(errorMessage, eChatType.CT_Skill);
				return;
			}

            Point2D spawnPoint = Caster.GetPointFromHeading( Caster.Heading, 64 );
            int i = 0;
            for(i=0;i<3;i++)
            {               
                deamons[i] = new ZoarkatPet(template);
                deamons[i].SetOwnBrain(new ProcPetBrain(player));
                deamons[i].X = spawnPoint.X + Util.Random(20,40) - Util.Random(20,40);
                deamons[i].Y = spawnPoint.Y + Util.Random(20,40) - Util.Random(20,40);
                deamons[i].Z = Caster.Z;
                deamons[i].CurrentRegion = Caster.CurrentRegion;
                deamons[i].Heading = (ushort)((Caster.Heading + 2048) % 4096);
                deamons[i].Realm = Caster.Realm;
                deamons[i].CurrentSpeed = 0;
                deamons[i].Level = 36;
                deamons[i].Flags |= (uint)GameNPC.eFlags.FLYING;
                deamons[i].AddToWorld();
				(deamons[i].Brain as IOldAggressiveBrain).AddToAggroList(Caster.TargetObject as GameLiving, 1);
				(deamons[i].Brain as ProcPetBrain).Think();
            }			
		}

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
        	int i = 0;
            for(i=0;i<3;i++)
            {
            	if(deamons[i]!=null)
            	{
            		deamons[i].Health = 0;
            		deamons[i].Delete();
            	}
            }
            return base.OnEffectExpires(effect,noMessages);
        }
        public override int CalculateSpellResistChance(GameLiving target) { return 0; }
        public BracerOfZo(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    
    [SpellHandlerAttribute("Bedazzlement")]
    public class ZoDebuffSpellHandler : DualStatDebuff
    {
		public override eProperty Property1 { get { return eProperty.FumbleChance; } }
		public override eProperty Property2 { get { return eProperty.SpellFumbleChance; } }

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);
			target.StartInterruptTimer(target.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
		}
		
        public ZoDebuffSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}

namespace DOL.GS
{
	public class ZoarkatPet : GamePet
	{
		public override int MaxHealth
        {
            get { return Level*10; }
        }
		public override void OnAttackedByEnemy(AttackData ad) { }
		public ZoarkatPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
	}
}

