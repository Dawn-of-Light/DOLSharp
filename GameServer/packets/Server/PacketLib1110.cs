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
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using log4net;
using DOL.GS.Spells;
using DOL.GS.Delve;
using DOL.GS.Geometry;

namespace DOL.GS.PacketHandler
{
    [PacketLib(1110, GameClient.eClientVersion.Version1110)]
    public class PacketLib1110 : PacketLib1109
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PacketLib1110(GameClient client)
            : base(client)
        {
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
		}

		/// <summary>
		/// new siege weapon animation packet 1.110
		/// </summary>
		public override void SendSiegeWeaponAnimation(GameSiegeWeapon siegeWeapon)
        {
            if (siegeWeapon == null)
                return;
            using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SiegeWeaponAnimation)))
            {
                pak.WriteInt((uint)siegeWeapon.ObjectID);
                var aimCoordinate = siegeWeapon.AimCoordinate;
                pak.WriteInt((uint)aimCoordinate.X);
                pak.WriteInt((uint)aimCoordinate.Y);
                pak.WriteInt((uint)aimCoordinate.Z);
                pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.ObjectID));
                pak.WriteShort(siegeWeapon.Effect);
                pak.WriteShort((ushort)(siegeWeapon.SiegeWeaponTimer.TimeUntilElapsed)); // timer is no longer ( value / 100 )
                pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
                pak.Fill(0, 3); // TODO : these bytes change depending on siege weapon action, to implement when different ammo types available.
                SendTCP(pak);
            }
        }
		
		/// <summary>
		/// new siege weapon fireanimation 1.110 // patch 0021
		/// </summary>
		/// <param name="siegeWeapon">The siege weapon</param>
		/// <param name="timer">How long the animation lasts for</param>
		public override void SendSiegeWeaponFireAnimation(GameSiegeWeapon siegeWeapon, int timer)
		{
			if (siegeWeapon == null)
				return;
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SiegeWeaponAnimation)))
			{
                var targetPosition = siegeWeapon.TargetObject.Position;
                if(targetPosition == Position.Nowhere) targetPosition = siegeWeapon.GroundTargetPosition;
				pak.WriteInt((uint) siegeWeapon.ObjectID);
				pak.WriteInt((uint) (targetPosition.X));
				pak.WriteInt((uint) (targetPosition.Y));
				pak.WriteInt((uint) (targetPosition.Z + 50));
				pak.WriteInt((uint) (siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.ObjectID));
				pak.WriteShort(siegeWeapon.Effect);
				pak.WriteShort((ushort) (timer)); // timer is no longer ( value / 100 )
				pak.WriteByte((byte) SiegeTimer.eAction.Fire);
				pak.WriteShort(0xE134); // default ammo type, the only type currently supported on DOL
				pak.WriteByte(0x08); // always this flag when firing
				SendTCP(pak);				
			}
		}		
    }
}
