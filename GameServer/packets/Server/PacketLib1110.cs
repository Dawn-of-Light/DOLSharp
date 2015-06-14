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
#define NOENCRYPTION
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using DOL.Database;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using DOL.Language;
using log4net;


namespace DOL.GS.PacketHandler
{
    [PacketLib(1110, GameClient.eClientVersion.Version1110)]
    public class PacketLib1110 : PacketLib1109
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.110
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1110(GameClient client)
            : base(client)
        {
        }
                
        /// <summary>
        /// Property to enable "forced" Tooltip send when Update are made to player skills, or player effects.
        /// This can be controlled through server propertiers !
        /// </summary>
		public virtual bool ForceTooltipUpdate {
			get { return ServerProperties.Properties.USE_NEW_TOOLTIP_FORCEDUPDATE; }
		}

        /// <summary>
		/// New system in v1.110+ for delve info. delve is cached by client in extra file, stored locally.
		/// </summary>
		/// <param name="info"></param>
		public override void SendDelveInfo(string info)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DelveInfo)))
			{
				pak.WriteString(info, 2048);
				pak.WriteByte(0); // 0-terminated
				SendTCP(pak);
			}
		}

		public override void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			if (m_gameClient.Player == null)
			{
				return;
			}
			
			IList<int> tooltipids = new List<int>();
			
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.UpdateIcons)))
			{
				long initPos = pak.Position;
	
				int fxcount = 0;
				int entriesCount = 0;
	
				pak.WriteByte(0); // effects count set in the end
				pak.WriteByte(0); // unknown
				pak.WriteByte(Icons); // unknown
				pak.WriteByte(0); // unknown
	
				foreach (IGameEffect effect in m_gameClient.Player.EffectList)
				{
					if (effect.Icon != 0)
					{
						fxcount++;
						if (changedEffects != null && !changedEffects.Contains(effect))
						{
							continue;
						}
						
						// store tooltip update for gamespelleffect.
						if (ForceTooltipUpdate && (effect is GameSpellEffect))
						{
							Spell spell = ((GameSpellEffect)effect).Spell;
							tooltipids.Add(spell.InternalID);
						}
	
						//						log.DebugFormat("adding [{0}] '{1}'", fxcount-1, effect.Name);
						pak.WriteByte((byte)(fxcount - 1)); // icon index
						pak.WriteByte((effect is GameSpellEffect || effect.Icon > 5000) ? (byte)(fxcount - 1) : (byte)0xff);
						
						byte ImmunByte = 0;
						var gsp = effect as GameSpellEffect;
						if (gsp != null && gsp.IsDisabled)
							ImmunByte = 1;
						pak.WriteByte(ImmunByte); // new in 1.73; if non zero says "protected by" on right click
						
						// bit 0x08 adds "more..." to right click info
						pak.WriteShort(effect.Icon);
						//pak.WriteShort(effect.IsFading ? (ushort)1 : (ushort)(effect.RemainingTime / 1000));
						pak.WriteShort((ushort)(effect.RemainingTime / 1000));
						if (effect is GameSpellEffect)
							pak.WriteShort((ushort)((GameSpellEffect)effect).Spell.InternalID); //v1.110+ send the spell ID for delve info in active icon
						else
							pak.WriteShort(0);//don't override existing tooltip ids
	
						byte flagNegativeEffect = 0;
						if (effect is StaticEffect)
						{
							if (((StaticEffect)effect).HasNegativeEffect)
							{
								flagNegativeEffect = 1;
							}
						}
						else if (effect is GameSpellEffect)
						{
							if (!((GameSpellEffect)effect).SpellHandler.HasPositiveEffect)
							{
								flagNegativeEffect = 1;
							}
						}
						pak.WriteByte(flagNegativeEffect);
	
						pak.WritePascalString(effect.Name);
						entriesCount++;
					}
				}
	
				int oldCount = lastUpdateEffectsCount;
				lastUpdateEffectsCount = fxcount;
	
				while (oldCount > fxcount)
				{
					pak.WriteByte((byte)(fxcount++));
					pak.Fill(0, 10);
					entriesCount++;
					//					log.DebugFormat("adding [{0}] (empty)", fxcount-1);
				}
	
				if (changedEffects != null)
				{
					changedEffects.Clear();
				}
	
				if (entriesCount == 0)
				{
					return; // nothing changed - no update is needed
				}
	
				pak.Position = initPos;
				pak.WriteByte((byte)entriesCount);
				pak.Seek(0, SeekOrigin.End);
	
				SendTCP(pak);
			}
			
			// force tooltips update
			foreach (int entry in tooltipids)
			{
				if (m_gameClient.CanSendTooltip(24, entry))
					SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveSpell(m_gameClient, entry));
			}
		}
		
		/// <summary>
		/// Override for handling force tooltip update...
		/// </summary>
		public override void SendTrainerWindow()
		{
			base.SendTrainerWindow();
			
			// Send tooltips
			if (ForceTooltipUpdate && m_gameClient.TrainerSkillCache != null)
				SendForceTooltipUpdate(m_gameClient.TrainerSkillCache.SelectMany(e => e.Item2).Select(e => e.Item3));
		}
		
		/// <summary>
		/// Send Delve for Provided Collection of Skills that need forced Tooltip Update.
		/// </summary>
		/// <param name="skills"></param>
		protected virtual void SendForceTooltipUpdate(IEnumerable<Skill> skills)
		{
			foreach (Skill t in skills)
			{
				if (t is Specialization)
					continue;
				
				if (t is RealmAbility)
				{
					if (m_gameClient.CanSendTooltip(27, t.InternalID))
						SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveRealmAbility(m_gameClient, t.InternalID));
				}
				else if (t is Ability)
				{
					if (m_gameClient.CanSendTooltip(28, t.InternalID))
						SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveAbility(m_gameClient, t.InternalID));
				}
				else if (t is Style)
				{
					if (m_gameClient.CanSendTooltip(25, t.InternalID))
						SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveStyle(m_gameClient, t.InternalID));
				}
				else if (t is Spell)
				{
					if (t is Song || (t is Spell && ((Spell)t).NeedInstrument))
					{
						if (m_gameClient.CanSendTooltip(26, ((Spell)t).InternalID))
							SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveSong(m_gameClient, ((Spell)t).InternalID));
					}
					
					if (m_gameClient.CanSendTooltip(24, ((Spell)t).InternalID))
						SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveSpell(m_gameClient, ((Spell)t).InternalID));
				}
			}
		}
		
    }
}
