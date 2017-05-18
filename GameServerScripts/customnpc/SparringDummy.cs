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
using DOL.GS.PacketHandler;
using DOL.AI;
using DOL.GS;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Sparring dummy. Can be set to fight back or remain idle, and can have its defenses adjusted.
	/// </summary>
	public class SparringDummy : GameNPC
	{
		//How to use: In game type /mob create DOL.GS.Scripts.SparringDummy
		//Select the mob and type /mob peace
		//You are now ready! Right click mob to view combat options.
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public SparringDummy() : base()
		{
			m_maxSpeedBase = 0;
			SetOwnBrain(new DummyBrain());
		}
		
		/// <summary>
		/// If true, sparring dummy will fight back.
		/// </summary>
		private bool fightMode;
		
		/// <summary>
		/// Some NPC characteristics applied on creating.
		/// </summary>
		public override bool AddToWorld()
        {
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 4);
			template.AddNPCEquipment(eInventorySlot.LeftHandWeapon, 61);
			Inventory = template.CloseTemplate();			
            Name = "Sparring Dummy";
            Level = 50;
            Model = 10;
            MaxSpeedBase = 0;
            VisibleActiveWeaponSlots = 16;
			base.AddToWorld();
            return true;
        }	
		
		/// <summary>
		/// Training Dummies never loose health
		/// </summary>
		public override int Health
		{
			get{ return base.MaxHealth;}
			set {}
		}

		/// <summary>
		/// Training Dummies are always alive
		/// </summary>
		public override bool IsAlive
		{
			get{ return true;}
		}
		
		private long m_lastAttacked;
		
		public long LastAttacked
		{
			get { return m_lastAttacked; }
			set { m_lastAttacked = value; }
		}
		
		/// <summary>
		/// Sparring dummy will fight back in melee range.
		/// </summary>
		/// <param name="ad"></param>
		public override void OnAttackedByEnemy(AttackData ad)
		{
			if (ad.IsHit && ad.CausesCombat)
			{
				if (fightMode)
					StartAttack(ad.Attacker);
				if (ad.Attacker.Realm == 0 || this.Realm == 0)
				{
					m_lastAttacked = CurrentRegion.Time;
					ad.Attacker.LastAttackTickPvE = CurrentRegion.Time;
				}
				else
				{
					m_lastAttacked = CurrentRegion.Time;
					ad.Attacker.LastAttackTickPvP = CurrentRegion.Time;
				}
			}
		}
		
		private void SendReply(GamePlayer player, string msg)
        {
            player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
        }
		
		public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;
            if (AttackState || player.AttackState)
            {
            	player.Out.SendMessage("You cannot change my settings while either of us are in combat mode! Please wait.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            	return false;
            }
            player.Out.SendMessage("Hello "+player.Name+", I can be your sparring partner. You are able to adjust my [level], [block], [parry], and [evade] rates for testing purposes. You can also [reset] them back to default. I can also [fight] back if you would like to practice reactionary styles, but I promise I wont hit hard!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            return true;
        }
		
		public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str))
                return false;
            GamePlayer player = source as GamePlayer;
            if (player == null) return false;
			if (AttackState || player.AttackState)
			{
				player.Out.SendMessage("You cannot change my settings while either of us are in combat mode! Please wait.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				return false;
			}
			
			switch (str)
			{
				case "block":
					SendReply(player, "You can choose from some set block percentages. [block 10], [block 20], [block 30], [block 40], or [block 50]"); break;
					case "block 10": SendReply(player, "My block chance has been set to 10%"); m_blockChance = 10; break;
					case "block 20": SendReply(player, "My block chance has been set to 20%"); m_blockChance = 20; break;
					case "block 30": SendReply(player, "My block chance has been set to 30%"); m_blockChance = 30; break;
					case "block 40": SendReply(player, "My block chance has been set to 40%"); m_blockChance = 40; break;
					case "block 50": SendReply(player, "My block chance has been set to 50%"); m_blockChance = 50; break;
						
				case "parry":
					SendReply(player, "You can choose from some set parry percentages. [parry 10], [parry 20], [parry 30], [parry 40], or [parry 50]"); break;
						case "parry 10": SendReply(player, "My parry chance has been set to 10%"); m_parryChance = 10; break;
						case "parry 20": SendReply(player, "My parry chance has been set to 20%"); m_parryChance = 20; break;
						case "parry 30": SendReply(player, "My parry chance has been set to 30%"); m_parryChance = 30; break;
						case "parry 40": SendReply(player, "My parry chance has been set to 40%"); m_parryChance = 40; break;
						case "parry 50": SendReply(player, "My parry chance has been set to 50%"); m_parryChance = 50; break;
				case "evade":
					SendReply(player, "You can choose from some set evade percentages. [evade 10], [evade 20], [evade 30], [evade 40], or [evade 50]"); break;
						case "evade 10": SendReply(player, "My evade chance has been set to 10%"); m_evadeChance = 10; break;
						case "evade 20": SendReply(player, "My evade chance has been set to 20%"); m_evadeChance = 20; break;
						case "evade 30": SendReply(player, "My evade chance has been set to 30%"); m_evadeChance = 30; break;
						case "evade 40": SendReply(player, "My evade chance has been set to 40%"); m_evadeChance = 40; break;
						case "evade 50": SendReply(player, "My evade chance has been set to 50%"); m_evadeChance = 50; break;		
				case "level":
					SendReply(player, "You can choose from some set levels, or I can be [equal] to your level. [level 10], [level 20], [level 30], [level 40], or [level 50]"); break;
						case "level 10": SendReply(player, "My level is now 10"); Level = 10; BroadcastUpdate(); break;
						case "level 20": SendReply(player, "My level is now 20"); Level = 20; BroadcastUpdate(); break;
						case "level 30": SendReply(player, "My level is now 30"); Level = 30; BroadcastUpdate(); break;
						case "level 40": SendReply(player, "My level is now 40"); Level = 40; BroadcastUpdate(); break;
						case "level 50": SendReply(player, "My level is now 50"); Level = 50; BroadcastUpdate(); break;
						case "equal": SendReply(player, "My level is now equal to you."); Level = player.Level; BroadcastUpdate();break;
				case "reset":
					SendReply(player, "My defence values have been reset to zero and I will not fight back.");
					m_blockChance = 0;
					m_parryChance = 0;
					m_evadeChance = 0;
					fightMode = false;
					break;
				case "fight":
					SendReply(player, "I will now fight back when attacked.");
					fightMode = true;
					break;
			}
			return true;
		}				
	}
	
	/// <summary>
	/// Simple brain from sparring dummy.
	/// </summary>
	public class DummyBrain : ABrain
	{		
		/// <summary>
		/// Sparring dummy does not need to think often. He only needs to think to disengage from combat.
		/// </summary>
		public override int ThinkInterval
		{
			get { return 10000; }
		}
		public override void Think()
		{
			SparringDummy dummy = Body as SparringDummy;
			
			if (Body.AttackState)
			{
				const long delay = 5000;				

				if (dummy.LastAttacked + delay < Body.CurrentRegion.Time)
				{
					Body.StopAttack();
					Body.TargetObject = null;					
					return;
				}
			}			
		}
	}
}