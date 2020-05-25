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
using System.Collections;
using System.Collections.Generic;

using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.Database;
using DOL.AI.Brain;
using DOL.GS.Keeps;
using DOL.GS.Housing;

namespace DOL.Tests
{
	/// <summary>
	/// PacketLib Implementation for Unit Tests
	/// </summary>
	public class TestPacketLib : IPacketLib
	{
		public Func<TestPacketLib, int> BowPrepareGetFunc { get; set; }
		public Action<TestPacketLib, int> BowPrepareSetFunc { get; set; }
		public int BowPrepare {
			get { return BowPrepareGetFunc != null ? BowPrepareGetFunc(this) : default(int); }
			set { if (BowPrepareSetFunc != null) BowPrepareSetFunc(this, value); }
		}
		public Func<TestPacketLib, int> BowShootGetFunc { get; set; }
		public Action<TestPacketLib, int> BowShootSetFunc { get; set; }
		public int BowShoot {
			get { return BowShootGetFunc != null ? BowShootGetFunc(this) : default(int); }
			set { if (BowShootSetFunc != null) BowShootSetFunc(this, value); }
		}
		public Func<TestPacketLib, int> OneDualWeaponHitGetFunc { get; set; }
		public Action<TestPacketLib, int> OneDualWeaponHitSetFunc { get; set; }
		public int OneDualWeaponHit {
			get { return OneDualWeaponHitGetFunc != null ? OneDualWeaponHitGetFunc(this) : default(int); }
			set { if (OneDualWeaponHitSetFunc != null) OneDualWeaponHitSetFunc(this, value); }
		}
		public Func<TestPacketLib, int> BothDualWeaponHitGetFunc { get; set; }
		public Action<TestPacketLib, int> BothDualWeaponHitSetFunc { get; set; }
		public int BothDualWeaponHit {
			get { return BothDualWeaponHitGetFunc != null ? BothDualWeaponHitGetFunc(this) : default(int); }
			set { if (BothDualWeaponHitSetFunc != null) BothDualWeaponHitSetFunc(this, value); }
		}
		
		public Func<TestPacketLib, eServerPackets, byte> GetPacketCodeFunc { get; set; }
		public byte GetPacketCode(eServerPackets packetCode)
		{
			return GetPacketCodeFunc != null ? GetPacketCodeFunc(this, packetCode) : default(byte);
		}
		public Action<TestPacketLib, GSTCPPacketOut> SendTCPPacket { get; set; }
		public void SendTCP(GSTCPPacketOut packet)
		{
			if (SendTCPPacket != null) SendTCPPacket(this, packet);
		}
		public Action<TestPacketLib, byte[]> SendTCPBuf { get; set; }
		public void SendTCP(byte[] buf)
		{
			if (SendTCPBuf != null) SendTCPBuf(this, buf);
		}
		public Action<TestPacketLib, GSTCPPacketOut> SendTCPRawPacket { get; set; }
		public void SendTCPRaw(GSTCPPacketOut packet)
		{
			if (SendTCPRawPacket != null) SendTCPRawPacket(this, packet);
		}
		public Action<TestPacketLib, GSUDPPacketOut> SendUDPPacket { get; set; }
		public void SendUDP(GSUDPPacketOut packet)
		{
			if (SendUDPPacket != null) SendUDPPacket(this, packet);
		}
		public Action<TestPacketLib, byte[]> SendUDPBuf { get; set; }
		public void SendUDP(byte[] buf)
		{
			if (SendUDPBuf != null) SendUDPBuf(this, buf);
		}
		public Action<TestPacketLib, GSUDPPacketOut> SendUDPRawPacket { get; set; }
		public void SendUDPRaw(GSUDPPacketOut packet)
		{
			if (SendUDPRawPacket != null) SendUDPRawPacket(this, packet);
		}
		public Action<TestPacketLib, GamePlayer> SendWarlockChamberEffectMethod { get; set; }
		public void SendWarlockChamberEffect(GamePlayer player)
		{
			if (SendWarlockChamberEffectMethod != null) SendWarlockChamberEffectMethod(this, player);
		}
		public Action<TestPacketLib> SendVersionAndCryptKeyMethod { get; set; }
		public void SendVersionAndCryptKey()
		{
			if (SendVersionAndCryptKeyMethod != null) SendVersionAndCryptKeyMethod(this);
		}
		public Action<TestPacketLib, eLoginError> SendLoginDeniedMethod { get; set; }
		public void SendLoginDenied(eLoginError et)
		{
			if (SendLoginDeniedMethod != null) SendLoginDeniedMethod(this, et);
		}
		public Action<TestPacketLib> SendLoginGrantedMethod { get; set; }
		public void SendLoginGranted()
		{
			if (SendLoginGrantedMethod != null) SendLoginGrantedMethod(this);
		}
		public Action<TestPacketLib, byte> SendLoginGrantedByteMethod { get; set; }
		public void SendLoginGranted(byte color)
		{
			if (SendLoginGrantedByteMethod != null) SendLoginGrantedByteMethod(this, color);
		}
		public Action<TestPacketLib> SendSessionIDMethod { get; set; }
		public void SendSessionID()
		{
			if (SendSessionIDMethod != null) SendSessionIDMethod(this);
		}
		public Action<TestPacketLib, ulong, ushort> SendPingReplyMethod { get; set; }
		public void SendPingReply(ulong timestamp, ushort sequence)
		{
			if (SendPingReplyMethod != null) SendPingReplyMethod(this, timestamp, sequence);
		}
		public Action<TestPacketLib, eRealm> SendRealmMethod { get; set; }
		public void SendRealm(eRealm realm)
		{
			if (SendRealmMethod != null) SendRealmMethod(this, realm);
		}
		public Action<TestPacketLib, eRealm> SendCharacterOverviewMethod { get; set; }
		public void SendCharacterOverview(eRealm realm)
		{
			if (SendCharacterOverviewMethod != null) SendCharacterOverviewMethod(this, realm);
		}
		public Action<TestPacketLib, string, byte> SendDupNameCheckReplyMethod { get; set; }
		public void SendDupNameCheckReply(string name, byte nameExists)
		{
			if (SendDupNameCheckReplyMethod != null) SendDupNameCheckReplyMethod(this, name, nameExists);
		}
		public Action<TestPacketLib, string, bool> SendBadNameCheckReplyMethod { get; set; }
		public void SendBadNameCheckReply(string name, bool bad)
		{
			if (SendBadNameCheckReplyMethod != null) SendBadNameCheckReplyMethod(this, name, bad);
		}
		public Action<TestPacketLib, bool> SendAttackModeMethod { get; set; }
		public void SendAttackMode(bool attackState)
		{
			if (SendAttackModeMethod != null) SendAttackModeMethod(this, attackState);
		}
		public Action<TestPacketLib, string> SendCharCreateReplyMethod { get; set; }
		public void SendCharCreateReply(string name)
		{
			if (SendCharCreateReplyMethod != null) SendCharCreateReplyMethod(this, name);
		}
		public Action<TestPacketLib> SendCharStatsUpdateMethod { get; set; }
		public void SendCharStatsUpdate()
		{
			if (SendCharStatsUpdateMethod != null) SendCharStatsUpdateMethod(this);
		}
		public Action<TestPacketLib> SendCharResistsUpdateMethod { get; set; }
		public void SendCharResistsUpdate()
		{
			if (SendCharResistsUpdateMethod != null) SendCharResistsUpdateMethod(this);
		}
		public Action<TestPacketLib> SendRegionsMethod { get; set; }
		public void SendRegions()
		{
			if (SendRegionsMethod != null) SendRegionsMethod(this);
		}
		public Action<TestPacketLib> SendGameOpenReplyMethod { get; set; }
		public void SendGameOpenReply()
		{
			if (SendGameOpenReplyMethod != null) SendGameOpenReplyMethod(this);
		}
		public Action<TestPacketLib> SendPlayerPositionAndObjectIDMethod { get; set; }
		public void SendPlayerPositionAndObjectID()
		{
			if (SendPlayerPositionAndObjectIDMethod != null) SendPlayerPositionAndObjectIDMethod(this);
		}
		public Action<TestPacketLib, bool> SendPlayerJumpMethod { get; set; }
		public void SendPlayerJump(bool headingOnly)
		{
			if (SendPlayerJumpMethod != null) SendPlayerJumpMethod(this, headingOnly);
		}
		public Action<TestPacketLib, byte> SendPlayerInitFinishedMethod { get; set; }
		public void SendPlayerInitFinished(byte mobs)
		{
			if (SendPlayerInitFinishedMethod != null) SendPlayerInitFinishedMethod(this, mobs);
		}
		public Action<TestPacketLib> SendUDPInitReplyMethod { get; set; }
		public void SendUDPInitReply()
		{
			if (SendUDPInitReplyMethod != null) SendUDPInitReplyMethod(this);
		}
		public Action<TestPacketLib> SendTimeMethod { get; set; }
		public void SendTime()
		{
			if (SendTimeMethod != null) SendTimeMethod(this);
		}
		public Action<TestPacketLib, string, eChatType, eChatLoc> SendMessageMethod { get; set; }
		public void SendMessage(string msg, eChatType type, eChatLoc loc)
		{
			if (SendMessageMethod != null) SendMessageMethod(this, msg, type, loc);
		}
		public Action<TestPacketLib, GamePlayer> SendPlayerCreateMethod { get; set; }
		public void SendPlayerCreate(GamePlayer playerToCreate)
		{
			if (SendPlayerCreateMethod != null) SendPlayerCreateMethod(this, playerToCreate);
		}
		public Action<TestPacketLib, GameObject, Guild> SendObjectGuildIDMethod { get; set; }
		public void SendObjectGuildID(GameObject obj, Guild guild)
		{
			if (SendObjectGuildIDMethod != null) SendObjectGuildIDMethod(this, obj, guild);
		}
		public Action<TestPacketLib, bool> SendPlayerQuitMethod { get; set; }
		public void SendPlayerQuit(bool totalOut)
		{
			if (SendPlayerQuitMethod != null) SendPlayerQuitMethod(this, totalOut);
		}
		public Action<TestPacketLib, GameObject> SendObjectRemoveMethod { get; set; }
		public void SendObjectRemove(GameObject obj)
		{
			if (SendObjectRemoveMethod != null) SendObjectRemoveMethod(this, obj);
		}
		public Action<TestPacketLib, GameObject> SendObjectCreateMethod { get; set; }
		public void SendObjectCreate(GameObject obj)
		{
			if (SendObjectCreateMethod != null) SendObjectCreateMethod(this, obj);
		}
		public Action<TestPacketLib, bool> SendDebugModeMethod { get; set; }
		public void SendDebugMode(bool on)
		{
			if (SendDebugModeMethod != null) SendDebugModeMethod(this, on);
		}
		public Action<TestPacketLib, GameObject, ushort> SendModelChangeMethod { get; set; }
		public void SendModelChange(GameObject obj, ushort newModel)
		{
			if (SendModelChangeMethod != null) SendModelChangeMethod(this, obj, newModel);
		}
		public Action<TestPacketLib, GameObject, ushort, byte> SendModelAndSizeChangeMethod { get; set; }
		public void SendModelAndSizeChange(GameObject obj, ushort newModel, byte newSize)
		{
			if (SendModelAndSizeChangeMethod != null) SendModelAndSizeChangeMethod(this, obj, newModel, newSize);
		}
		public Action<TestPacketLib, ushort, ushort, byte> SendModelAndSizeChangeIDMethod { get; set; }
		public void SendModelAndSizeChange(ushort objectId, ushort newModel, byte newSize)
		{
			if (SendModelAndSizeChangeIDMethod != null) SendModelAndSizeChangeIDMethod(this, objectId, newModel, newSize);
		}
		public Action<TestPacketLib, GameObject, eEmote> SendEmoteAnimationMethod { get; set; }
		public void SendEmoteAnimation(GameObject obj, eEmote emote)
		{
			if (SendEmoteAnimationMethod != null) SendEmoteAnimationMethod(this, obj, emote);
		}
		public Action<TestPacketLib, GameNPC> SendNPCCreateMethod { get; set; }
		public void SendNPCCreate(GameNPC npc)
		{
			if (SendNPCCreateMethod != null) SendNPCCreateMethod(this, npc);
		}
		public Action<TestPacketLib, GameLiving> SendLivingEquipmentUpdateMethod { get; set; }
		public void SendLivingEquipmentUpdate(GameLiving living)
		{
			if (SendLivingEquipmentUpdateMethod != null) SendLivingEquipmentUpdateMethod(this, living);
		}
		public Action<TestPacketLib> SendRegionChangedMethod { get; set; }
		public void SendRegionChanged()
		{
			if (SendRegionChangedMethod != null) SendRegionChangedMethod(this);
		}
		public Action<TestPacketLib> SendUpdatePointsMethod { get; set; }
		public void SendUpdatePoints()
		{
			if (SendUpdatePointsMethod != null) SendUpdatePointsMethod(this);
		}
		public Action<TestPacketLib> SendUpdateMoneyMethod { get; set; }
		public void SendUpdateMoney()
		{
			if (SendUpdateMoneyMethod != null) SendUpdateMoneyMethod(this);
		}
		public Action<TestPacketLib> SendUpdateMaxSpeedMethod { get; set; }
		public void SendUpdateMaxSpeed()
		{
			if (SendUpdateMaxSpeedMethod != null) SendUpdateMaxSpeedMethod(this);
		}
		public Action<TestPacketLib, string> SendDelveInfoMethod { get; set; }
		public void SendDelveInfo(string info)
		{
			if (SendDelveInfoMethod != null) SendDelveInfoMethod(this, info);
		}
		public Action<TestPacketLib, GameObject, GameObject, ushort, ushort, int, byte, byte, byte> SendCombatAnimationMethod { get; set; }
		public void SendCombatAnimation(GameObject attacker, GameObject defender, ushort weaponID, ushort shieldID, int style,
		                         byte stance, byte result, byte targetHealthPercent)
		{
			if (SendCombatAnimationMethod != null) SendCombatAnimationMethod(this, attacker, defender, weaponID, shieldID, style, stance, result, targetHealthPercent);
		}
		public Action<TestPacketLib> SendStatusUpdateMethod { get; set; }
		public void SendStatusUpdate()
		{
			if (SendStatusUpdateMethod != null) SendStatusUpdateMethod(this);
		}
		public Action<TestPacketLib, byte> SendStatusUpdateByteMethod { get; set; }
		public void SendStatusUpdate(byte sittingFlag)
		{
			if (SendStatusUpdateByteMethod != null) SendStatusUpdateByteMethod(this, sittingFlag);
		}
		public Action<TestPacketLib, GameLiving, ushort, ushort> SendSpellCastAnimationMethod { get; set; }
		public void SendSpellCastAnimation(GameLiving spellCaster, ushort spellID, ushort castingTime)
		{
			if (SendSpellCastAnimationMethod != null) SendSpellCastAnimationMethod(this, spellCaster, spellID, castingTime);
		}
		public Action<TestPacketLib, GameObject, GameObject, ushort, ushort, bool, byte> SendSpellEffectAnimationMethod { get; set; }
		public void SendSpellEffectAnimation(GameObject spellCaster, GameObject spellTarget, ushort spellid, ushort boltTime,
		                              bool noSound, byte success)
		{
			if (SendSpellEffectAnimationMethod != null) SendSpellEffectAnimationMethod(this, spellCaster, spellTarget, spellid, boltTime, noSound, success);
		}
		public Action<TestPacketLib, GameObject, GameObject, bool> SendRidingMethod { get; set; }
		public void SendRiding(GameObject rider, GameObject steed, bool dismount)
		{
			if (SendRidingMethod != null) SendRidingMethod(this, rider, steed, dismount);
		}
		public Action<TestPacketLib, GamePlayer[]> SendFindGroupWindowUpdateMethod { get; set; }
		public void SendFindGroupWindowUpdate(GamePlayer[] list)
		{
			if (SendFindGroupWindowUpdateMethod != null) SendFindGroupWindowUpdateMethod(this, list);
		}
		public Action<TestPacketLib, GamePlayer, string> SendGroupInviteCommandMethod { get; set; }
		public void SendGroupInviteCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			if (SendGroupInviteCommandMethod != null) SendGroupInviteCommandMethod(this, invitingPlayer, inviteMessage);
		}
		public Action<TestPacketLib, eDialogCode, ushort, ushort, ushort, ushort, eDialogType, bool, string> SendDialogBoxMethod { get; set; }
		public void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type,
		                   bool autoWrapText, string message)
		{
			if (SendDialogBoxMethod != null) SendDialogBoxMethod(this, code, data1, data2, data3, data4, type, autoWrapText, message);
		}
		public Action<TestPacketLib, string, CustomDialogResponse> SendCustomDialogMethod { get; set; }
		public void SendCustomDialog(string msg, CustomDialogResponse callback)
		{
			if (SendCustomDialogMethod != null) SendCustomDialogMethod(this, msg, callback);
		}
		public Action<TestPacketLib, GameObject, GameObject, CheckLOSResponse> SendCheckLOSMethod { get; set; }
		public void SendCheckLOS(GameObject Checker, GameObject Target, CheckLOSResponse callback)
		{
			if (SendCheckLOSMethod != null) SendCheckLOSMethod(this, Checker, Target, callback);
		}
		public Action<TestPacketLib, GameObject, GameObject, CheckLOSMgrResponse> SendCheckLOSMgrMethod { get; set; }
		public void SendCheckLOS(GameObject source, GameObject target, CheckLOSMgrResponse callback)
		{
			if (SendCheckLOSMgrMethod != null) SendCheckLOSMgrMethod(this, source, target, callback);
		}
		public Action<TestPacketLib, GamePlayer, string> SendGuildLeaveCommandMethod { get; set; }
		public void SendGuildLeaveCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			if (SendGuildLeaveCommandMethod != null) SendGuildLeaveCommandMethod(this, invitingPlayer, inviteMessage);
		}
		public Action<TestPacketLib, GamePlayer, string> SendGuildInviteCommandMethod { get; set; }
		public void SendGuildInviteCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			if (SendGuildInviteCommandMethod != null) SendGuildInviteCommandMethod(this, invitingPlayer, inviteMessage);
		}
		public Action<TestPacketLib, GameNPC, GamePlayer, RewardQuest> SendQuestOfferWindowMethod { get; set; }
		public void SendQuestOfferWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest)
		{
			if (SendQuestOfferWindowMethod != null) SendQuestOfferWindowMethod(this, questNPC, player, quest);
		}
		public Action<TestPacketLib, GameNPC, GamePlayer, RewardQuest> SendQuestRewardWindowMethod { get; set; }
		public void SendQuestRewardWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest)
		{
			if (SendQuestRewardWindowMethod != null) SendQuestRewardWindowMethod(this, questNPC, player, quest);
		}
		public Action<TestPacketLib, GameNPC, GamePlayer, DataQuest> SendQuestOfferWindowDataMethod { get; set; }
		public void SendQuestOfferWindow(GameNPC questNPC, GamePlayer player, DataQuest quest)
		{
			if (SendQuestOfferWindowDataMethod != null) SendQuestOfferWindowDataMethod(this, questNPC, player, quest);
		}
		public Action<TestPacketLib, GameNPC, GamePlayer, DataQuest> SendQuestRewardWindowDataMethod { get; set; }
		public void SendQuestRewardWindow(GameNPC questNPC, GamePlayer player, DataQuest quest)
		{
			if (SendQuestRewardWindowDataMethod != null) SendQuestRewardWindowDataMethod(this, questNPC, player, quest);
		}
		public Action<TestPacketLib, GameNPC, ushort, string> SendQuestSubscribeCommandMethod { get; set; }
		public void SendQuestSubscribeCommand(GameNPC invitingNPC, ushort questid, string inviteMessage)
		{
			if (SendQuestSubscribeCommandMethod != null) SendQuestSubscribeCommandMethod(this, invitingNPC, questid, inviteMessage);
		}
		public Action<TestPacketLib, GameNPC, ushort, string> SendQuestAbortCommandMethod { get; set; }
		public void SendQuestAbortCommand(GameNPC abortingNPC, ushort questid, string abortMessage)
		{
			if (SendQuestAbortCommandMethod != null) SendQuestAbortCommandMethod(this, abortingNPC, questid, abortMessage);
		}
		public Action<TestPacketLib> SendGroupWindowUpdateMethod { get; set; }
		public void SendGroupWindowUpdate()
		{
			if (SendGroupWindowUpdateMethod != null) SendGroupWindowUpdateMethod(this);
		}
		public Action<TestPacketLib, bool, bool, GameLiving> SendGroupMemberUpdateMethod { get; set; }
		public void SendGroupMemberUpdate(bool updateIcons, bool updateMap, GameLiving living)
		{
			if (SendGroupMemberUpdateMethod != null) SendGroupMemberUpdateMethod(this, updateIcons, true, living);
		}
		public Action<TestPacketLib, bool, bool> SendGroupMembersUpdateMethod { get; set; }
		public void SendGroupMembersUpdate(bool updateIcons, bool updateMap)
		{
			if (SendGroupMembersUpdateMethod != null) SendGroupMembersUpdateMethod(this, updateIcons, true);
		}
		public Action<TestPacketLib, ICollection<InventoryItem>> SendInventoryItemsUpdateMethod { get; set; }
		public void SendInventoryItemsUpdate(ICollection<InventoryItem> itemsToUpdate)
		{
			if (SendInventoryItemsUpdateMethod != null) SendInventoryItemsUpdateMethod(this, itemsToUpdate);
		}
		public Action<TestPacketLib, ICollection<int>> SendInventorySlotsUpdateMethod { get; set; }
		public void SendInventorySlotsUpdate(ICollection<int> slots)
		{
			if (SendInventorySlotsUpdateMethod != null) SendInventorySlotsUpdateMethod(this, slots);
		}
		public Action<TestPacketLib, eInventoryWindowType, ICollection<InventoryItem>> SendInventoryItemsUpdateWindowMethod { get; set; }
		public void SendInventoryItemsUpdate(eInventoryWindowType windowType, ICollection<InventoryItem> itemsToUpdate)
		{
			if (SendInventoryItemsUpdateWindowMethod != null) SendInventoryItemsUpdateWindowMethod(this, windowType, itemsToUpdate);
		}
		public Action<TestPacketLib, IDictionary<int, InventoryItem>, eInventoryWindowType> SendInventoryItemsUpdateDictMethod { get; set; }
		public void SendInventoryItemsUpdate(IDictionary<int, InventoryItem> updateItems, eInventoryWindowType windowType)
		{
			if (SendInventoryItemsUpdateDictMethod != null) SendInventoryItemsUpdateDictMethod(this, updateItems, windowType);
		}
		public Action<TestPacketLib, Region, IDoor> SendDoorStateMethod { get; set; }
		public void SendDoorState(Region region, IDoor door)
		{
			if (SendDoorStateMethod != null) SendDoorStateMethod(this, region, door);
		}
		public Action<TestPacketLib, MerchantTradeItems, eMerchantWindowType> SendMerchantWindowMethod { get; set; }
		public void SendMerchantWindow(MerchantTradeItems itemlist, eMerchantWindowType windowType)
		{
			if (SendMerchantWindowMethod != null) SendMerchantWindowMethod(this, itemlist, windowType);
		}
		public Action<TestPacketLib> SendTradeWindowMethod { get; set; }
		public void SendTradeWindow()
		{
			if (SendTradeWindowMethod != null) SendTradeWindowMethod(this);
		}
		public Action<TestPacketLib> SendCloseTradeWindowMethod { get; set; }
		public void SendCloseTradeWindow()
		{
			if (SendCloseTradeWindowMethod != null) SendCloseTradeWindowMethod(this);
		}
		public Action<TestPacketLib, GamePlayer, GameObject> SendPlayerDiedMethod { get; set; }
		public void SendPlayerDied(GamePlayer killedPlayer, GameObject killer)
		{
			if (SendPlayerDiedMethod != null) SendPlayerDiedMethod(this, killedPlayer, killer);
		}
		public Action<TestPacketLib, GamePlayer> SendPlayerReviveMethod { get; set; }
		public void SendPlayerRevive(GamePlayer revivedPlayer)
		{
			if (SendPlayerReviveMethod != null) SendPlayerReviveMethod(this, revivedPlayer);
		}
		public Action<TestPacketLib, GamePlayer> SendPlayerForgedPositionMethod { get; set; }
		public void SendPlayerForgedPosition(GamePlayer player)
		{
			if (SendPlayerForgedPositionMethod != null) SendPlayerForgedPositionMethod(this, player);
		}
		public Action<TestPacketLib> SendUpdatePlayerMethod { get; set; }
		public void SendUpdatePlayer()
		{
			if (SendUpdatePlayerMethod != null) SendUpdatePlayerMethod(this);
		}
		public Action<TestPacketLib> SendUpdatePlayerSkillsMethod { get; set; }
		public void SendUpdatePlayerSkills()
		{
			if (SendUpdatePlayerSkillsMethod != null) SendUpdatePlayerSkillsMethod(this);
		}
		public Action<TestPacketLib> SendUpdateWeaponAndArmorStatsMethod { get; set; }
		public void SendUpdateWeaponAndArmorStats()
		{
			if (SendUpdateWeaponAndArmorStatsMethod != null) SendUpdateWeaponAndArmorStatsMethod(this);
		}
		public Action<TestPacketLib, string, IList<string>> SendCustomTextWindowMethod { get; set; }
		public void SendCustomTextWindow(string caption, IList<string> text)
		{
			if (SendCustomTextWindowMethod != null) SendCustomTextWindowMethod(this, caption, text);
		}
		public Action<TestPacketLib> SendPlayerTitlesMethod { get; set; }
		public void SendPlayerTitles()
		{
			if (SendPlayerTitlesMethod != null) SendPlayerTitlesMethod(this);
		}
		public Action<TestPacketLib, GamePlayer> SendPlayerTitleUpdateMethod { get; set; }
		public void SendPlayerTitleUpdate(GamePlayer player)
		{
			if (SendPlayerTitleUpdateMethod != null) SendPlayerTitleUpdateMethod(this, player);
		}
		public Action<TestPacketLib> SendEncumberanceMethod { get; set; }
		public void SendEncumberance()
		{
			if (SendEncumberanceMethod != null) SendEncumberanceMethod(this);
		}
		public Action<TestPacketLib, string[]> SendAddFriendsMethod { get; set; }
		public void SendAddFriends(string[] friendNames)
		{
			if (SendAddFriendsMethod != null) SendAddFriendsMethod(this, friendNames);
		}
		public Action<TestPacketLib, string[]> SendRemoveFriendsMethod { get; set; }
		public void SendRemoveFriends(string[] friendNames)
		{
			if (SendRemoveFriendsMethod != null) SendRemoveFriendsMethod(this, friendNames);
		}
		public Action<TestPacketLib, string, int> SendTimerWindowMethod { get; set; }
		public void SendTimerWindow(string title, int seconds)
		{
			if (SendTimerWindowMethod != null) SendTimerWindowMethod(this, title, seconds);
		}
		public Action<TestPacketLib> SendCloseTimerWindowMethod { get; set; }
		public void SendCloseTimerWindow()
		{
			if (SendCloseTimerWindowMethod != null) SendCloseTimerWindowMethod(this);
		}
		public Action<TestPacketLib, int, List<Tuple<Specialization, List<Tuple<Skill, byte>>>>> SendCustomTrainerWindowMethod { get; set; }
		public void SendCustomTrainerWindow(int type, List<Tuple<Specialization, List<Tuple<Skill, byte>>>> tree)
		{
			if (SendCustomTrainerWindowMethod != null) SendCustomTrainerWindowMethod(this, type, tree);
		}
		public Action<TestPacketLib, int> SendChampionTrainerWindowMethod { get; set; }
		public void SendChampionTrainerWindow(int type)
		{
			if (SendChampionTrainerWindowMethod != null) SendChampionTrainerWindowMethod(this, type);
		}
		public Action<TestPacketLib> SendTrainerWindowMethod { get; set; }
		public void SendTrainerWindow()
		{
			if (SendTrainerWindowMethod != null) SendTrainerWindowMethod(this);
		}
		public Action<TestPacketLib, GameLiving> SendInterruptAnimationMethod { get; set; }
		public void SendInterruptAnimation(GameLiving living)
		{
			if (SendInterruptAnimationMethod != null) SendInterruptAnimationMethod(this, living);
		}
		public Action<TestPacketLib, ICollection<Tuple<Skill, int>>> SendDisableSkillMethod { get; set; }
		public void SendDisableSkill(ICollection<Tuple<Skill, int>> skills)
		{
			if (SendDisableSkillMethod != null) SendDisableSkillMethod(this, skills);
		}
		public Action<TestPacketLib, IList, int> SendUpdateIconsMethod { get; set; }
		public void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			if (SendUpdateIconsMethod != null) SendUpdateIconsMethod(this, changedEffects, lastUpdateEffectsCount);
		}
		public Action<TestPacketLib> SendLevelUpSoundMethod { get; set; }
		public void SendLevelUpSound()
		{
			if (SendLevelUpSoundMethod != null) SendLevelUpSoundMethod(this);
		}
		public Action<TestPacketLib, byte> SendRegionEnterSoundMethod { get; set; }
		public void SendRegionEnterSound(byte soundId)
		{
			if (SendRegionEnterSoundMethod != null) SendRegionEnterSoundMethod(this, soundId);
		}
		public Action<TestPacketLib, string, object[]> SendDebugMessageMethod { get; set; }
		public void SendDebugMessage(string format, params object[] parameters)
		{
			if (SendDebugMessageMethod != null) SendDebugMessageMethod(this, format, parameters);
		}
		public Action<TestPacketLib, string, object[]> SendDebugPopupMessageMethod { get; set; }
		public void SendDebugPopupMessage(string format, params object[] parameters)
		{
			if (SendDebugPopupMessageMethod != null) SendDebugPopupMessageMethod(this, format, parameters);
		}
		public Action<TestPacketLib> SendEmblemDialogueMethod { get; set; }
		public void SendEmblemDialogue()
		{
			if (SendEmblemDialogueMethod != null) SendEmblemDialogueMethod(this);
		}
		public Action<TestPacketLib, uint, uint, ushort, ushort, ushort> SendWeatherMethod { get; set; }
		public void SendWeather(uint x, uint width, ushort speed, ushort fogdiffusion, ushort intensity)
		{
			if (SendWeatherMethod != null) SendWeatherMethod(this, x, width, speed, fogdiffusion, intensity);
		}
		public Action<TestPacketLib, GamePlayer, byte> SendPlayerModelTypeChangeMethod { get; set; }
		public void SendPlayerModelTypeChange(GamePlayer player, byte modelType)
		{
			if (SendPlayerModelTypeChangeMethod != null) SendPlayerModelTypeChangeMethod(this, player, modelType);
		}
		public Action<TestPacketLib, GameObject> SendObjectDeleteMethod { get; set; }
		public void SendObjectDelete(GameObject obj)
		{
			if (SendObjectDeleteMethod != null) SendObjectDeleteMethod(this, obj);
		}
		public Action<TestPacketLib, ushort> SendObjectIdDeleteMethod { get; set; }
		public void SendObjectDelete(ushort objId)
		{
			if (SendObjectIdDeleteMethod != null) SendObjectIdDeleteMethod(this, objId);
		}
		public Action<TestPacketLib, GameObject> SendObjectUpdateMethod { get; set; }
		public void SendObjectUpdate(GameObject obj)
		{
			if (SendObjectUpdateMethod != null) SendObjectUpdateMethod(this, obj);
		}
		public Action<TestPacketLib> SendQuestListUpdateMethod { get; set; }
		public void SendQuestListUpdate()
		{
			if (SendQuestListUpdateMethod != null) SendQuestListUpdateMethod(this);
		}
		public Action<TestPacketLib, AbstractQuest> SendQuestUpdateMethod { get; set; }
		public void SendQuestUpdate(AbstractQuest quest)
		{
			if (SendQuestUpdateMethod != null) SendQuestUpdateMethod(this, quest);
		}
		public Action<TestPacketLib> SendConcentrationListMethod { get; set; }
		public void SendConcentrationList()
		{
			if (SendConcentrationListMethod != null) SendConcentrationListMethod(this);
		}
		public Action<TestPacketLib> SendUpdateCraftingSkillsMethod { get; set; }
		public void SendUpdateCraftingSkills()
		{
			if (SendUpdateCraftingSkillsMethod != null) SendUpdateCraftingSkillsMethod(this);
		}
		public Action<TestPacketLib, GameObject> SendChangeTargetMethod { get; set; }
		public void SendChangeTarget(GameObject newTarget)
		{
			if (SendChangeTargetMethod != null) SendChangeTargetMethod(this, newTarget);
		}
		public Action<TestPacketLib, Point3D> SendChangeGroundTargetMethod { get; set; }
		public void SendChangeGroundTarget(Point3D newTarget)
		{
			if (SendChangeGroundTargetMethod != null) SendChangeGroundTargetMethod(this, newTarget);
		}
		public Action<TestPacketLib, GameLiving, ePetWindowAction, eAggressionState, eWalkState> SendPetWindowMethod { get; set; }
		public void SendPetWindow(GameLiving pet, ePetWindowAction windowAction, eAggressionState aggroState, eWalkState walkState)
		{
			if (SendPetWindowMethod != null) SendPetWindowMethod(this, pet, windowAction, aggroState, walkState);
		}
		public Action<TestPacketLib, eSoundType, ushort> SendPlaySoundMethod { get; set; }
		public void SendPlaySound(eSoundType soundType, ushort soundID)
		{
			if (SendPlaySoundMethod != null) SendPlaySoundMethod(this, soundType, soundID);
		}
		public Action<TestPacketLib, GameNPC, eQuestIndicator> SendNPCsQuestEffectMethod { get; set; }
		public void SendNPCsQuestEffect(GameNPC npc, eQuestIndicator indicator)
		{
			if (SendNPCsQuestEffectMethod != null) SendNPCsQuestEffectMethod(this, npc, indicator);
		}
		public Action<TestPacketLib, byte> SendMasterLevelWindowMethod { get; set; }
		public void SendMasterLevelWindow(byte ml)
		{
			if (SendMasterLevelWindowMethod != null) SendMasterLevelWindowMethod(this, ml);
		}
		public Action<TestPacketLib, GamePlayer, byte, byte, byte, byte, byte> SendHexEffectMethod { get; set; }
		public void SendHexEffect(GamePlayer player, byte effect1, byte effect2, byte effect3, byte effect4, byte effect5)
		{
			if (SendHexEffectMethod != null) SendHexEffectMethod(this, player , effect1, effect2, effect3, effect4, effect5);
		}
		public Action<TestPacketLib, GamePlayer, bool> SendRvRGuildBannerMethod { get; set; }
		public void SendRvRGuildBanner(GamePlayer player, bool show)
		{
			if (SendRvRGuildBannerMethod != null) SendRvRGuildBannerMethod(this, player, show);
		}
		public Action<TestPacketLib, GameSiegeWeapon> SendSiegeWeaponAnimationMethod { get; set; }
		public void SendSiegeWeaponAnimation(GameSiegeWeapon siegeWeapon)
		{
			if (SendSiegeWeaponAnimationMethod != null) SendSiegeWeaponAnimationMethod(this, siegeWeapon);
		}
		public Action<TestPacketLib, GameSiegeWeapon, int> SendSiegeWeaponFireAnimationMethod { get; set; }
		public void SendSiegeWeaponFireAnimation(GameSiegeWeapon siegeWeapon, int timer)
		{
			if (SendSiegeWeaponFireAnimationMethod != null) SendSiegeWeaponFireAnimationMethod(this, siegeWeapon, timer);
		}
		public Action<TestPacketLib> SendSiegeWeaponCloseInterfaceMethod { get; set; }
		public void SendSiegeWeaponCloseInterface()
		{
			if (SendSiegeWeaponCloseInterfaceMethod != null) SendSiegeWeaponCloseInterfaceMethod(this);
		}
		public Action<TestPacketLib, GameSiegeWeapon, int> SendSiegeWeaponInterfaceMethod { get; set; }
		public void SendSiegeWeaponInterface(GameSiegeWeapon siegeWeapon, int time)
		{
			if (SendSiegeWeaponInterfaceMethod != null) SendSiegeWeaponInterfaceMethod(this, siegeWeapon, time);
		}
		public Action<TestPacketLib, GameLiving, bool> SendLivingDataUpdateMethod { get; set; }
		public void SendLivingDataUpdate(GameLiving living, bool updateStrings)
		{
			if (SendLivingDataUpdateMethod != null) SendLivingDataUpdateMethod(this, living, updateStrings);
		}
		public Action<TestPacketLib, ushort, ushort, ushort, ushort, ushort, ushort> SendSoundEffectMethod { get; set; }
		public void SendSoundEffect(ushort soundId, ushort zoneId, ushort x, ushort y, ushort z, ushort radius)
		{
			if (SendSoundEffectMethod != null) SendSoundEffectMethod(this, soundId, zoneId, x, y, z, radius);
		}
		public Action<TestPacketLib, IGameKeep> SendKeepInfoMethod { get; set; }
		public void SendKeepInfo(IGameKeep keep)
		{
			if (SendKeepInfoMethod != null) SendKeepInfoMethod(this, keep);
		}
		public Action<TestPacketLib, IGameKeep> SendKeepRealmUpdateMethod { get; set; }
		public void SendKeepRealmUpdate(IGameKeep keep)
		{
			if (SendKeepRealmUpdateMethod != null) SendKeepRealmUpdateMethod(this, keep);
		}
		public Action<TestPacketLib, IGameKeep> SendKeepRemoveMethod { get; set; }
		public void SendKeepRemove(IGameKeep keep)
		{
			if (SendKeepRemoveMethod != null) SendKeepRemoveMethod(this, keep);
		}
		public Action<TestPacketLib, IGameKeepComponent> SendKeepComponentInfoMethod { get; set; }
		public void SendKeepComponentInfo(IGameKeepComponent keepComponent)
		{
			if (SendKeepComponentInfoMethod != null) SendKeepComponentInfoMethod(this, keepComponent);
		}
		public Action<TestPacketLib, IGameKeepComponent> SendKeepComponentDetailUpdateMethod { get; set; }
		public void SendKeepComponentDetailUpdate(IGameKeepComponent keepComponent)
		{
			if (SendKeepComponentDetailUpdateMethod != null) SendKeepComponentDetailUpdateMethod(this, keepComponent);
		}
		public Action<TestPacketLib, IGameKeepComponent> SendKeepComponentRemoveMethod { get; set; }
		public void SendKeepComponentRemove(IGameKeepComponent keepComponent)
		{
			if (SendKeepComponentRemoveMethod != null) SendKeepComponentRemoveMethod(this, keepComponent);
		}
		public Action<TestPacketLib, IGameKeep, byte> SendKeepClaimMethod { get; set; }
		public void SendKeepClaim(IGameKeep keep, byte flag)
		{
			if (SendKeepClaimMethod != null) SendKeepClaimMethod(this, keep, flag);
		}
		public Action<TestPacketLib, IGameKeep, bool> SendKeepComponentUpdateMethod { get; set; }
		public void SendKeepComponentUpdate(IGameKeep keep, bool LevelUp)
		{
			if (SendKeepComponentUpdateMethod != null) SendKeepComponentUpdateMethod(this, keep, LevelUp);
		}
		public Action<TestPacketLib, IGameKeepComponent> SendKeepComponentInteractMethod { get; set; }
		public void SendKeepComponentInteract(IGameKeepComponent component)
		{
			if (SendKeepComponentInteractMethod != null) SendKeepComponentInteractMethod(this, component);
		}
		public Action<TestPacketLib, IGameKeepComponent, int> SendKeepComponentHookPointMethod { get; set; }
		public void SendKeepComponentHookPoint(IGameKeepComponent component, int selectedHookPointIndex)
		{
			if (SendKeepComponentHookPointMethod != null) SendKeepComponentHookPointMethod(this, component, selectedHookPointIndex);
		}
		public Action<TestPacketLib, IGameKeepComponent, int> SendClearKeepComponentHookPointMethod { get; set; }
		public void SendClearKeepComponentHookPoint(IGameKeepComponent component, int selectedHookPointIndex)
		{
			if (SendClearKeepComponentHookPointMethod != null) SendClearKeepComponentHookPointMethod(this, component, selectedHookPointIndex);
		}
		public Action<TestPacketLib, GameKeepHookPoint> SendHookPointStoreMethod { get; set; }
		public void SendHookPointStore(GameKeepHookPoint hookPoint)
		{
			if (SendHookPointStoreMethod != null) SendHookPointStoreMethod(this, hookPoint);
		}
		public Action<TestPacketLib, ICollection<IGameKeep>> SendWarmapUpdateMethod { get; set; }
		public void SendWarmapUpdate(ICollection<IGameKeep> list)
		{
			if (SendWarmapUpdateMethod != null) SendWarmapUpdateMethod(this, list);
		}
		public Action<TestPacketLib, List<List<byte>>, List<List<byte>>> SendWarmapDetailUpdateMethod { get; set; }
		public void SendWarmapDetailUpdate(List<List<byte>> fights, List<List<byte>> groups)
		{
			if (SendWarmapDetailUpdateMethod != null) SendWarmapDetailUpdateMethod(this, fights, groups);
		}
		public Action<TestPacketLib> SendWarmapBonusesMethod { get; set; }
		public void SendWarmapBonuses()
		{
			if (SendWarmapBonusesMethod != null) SendWarmapBonusesMethod(this);
		}
		public Action<TestPacketLib, House> SendHouseMethod { get; set; }
		public void SendHouse(House house)
		{
			if (SendHouseMethod != null) SendHouseMethod(this, house);
		}
		public Action<TestPacketLib, House, bool> SendHouseOccupiedMethod { get; set; }
		public void SendHouseOccupied(House house, bool flagHouseOccuped)
		{
			if (SendHouseOccupiedMethod != null) SendHouseOccupiedMethod(this, house, flagHouseOccuped);
		}
		public Action<TestPacketLib, House> SendRemoveHouseMethod { get; set; }
		public void SendRemoveHouse(House house)
		{
			if (SendRemoveHouseMethod != null) SendRemoveHouseMethod(this, house);
		}
		public Action<TestPacketLib, House> SendGardenMethod { get; set; }
		public void SendGarden(House house)
		{
			if (SendGardenMethod != null) SendGardenMethod(this, house);
		}
		public Action<TestPacketLib, House, int> SendGardenIntMethod { get; set; }
		public void SendGarden(House house, int i)
		{
			if (SendGardenIntMethod != null) SendGardenIntMethod(this, house, i);
		}
		public Action<TestPacketLib, House> SendEnterHouseMethod { get; set; }
		public void SendEnterHouse(House house)
		{
			if (SendEnterHouseMethod != null) SendEnterHouseMethod(this, house);
		}
		public Action<TestPacketLib, House, ushort> SendExitHouseMethod { get; set; }
		public void SendExitHouse(House house, ushort unknown = 0)
		{
			if (SendExitHouseMethod != null) SendExitHouseMethod(this, house, unknown);
		}
		public Action<TestPacketLib, House> SendFurnitureMethod { get; set; }
		public void SendFurniture(House house)
		{
			if (SendFurnitureMethod != null) SendFurnitureMethod(this, house);
		}
		public Action<TestPacketLib, House, int> SendFurnitureIntMethod { get; set; }
		public void SendFurniture(House house, int i)
		{
			if (SendFurnitureIntMethod != null) SendFurnitureIntMethod(this, house, i);
		}
		public Action<TestPacketLib, string> SendHousePayRentDialogMethod { get; set; }
		public void SendHousePayRentDialog(string title)
		{
			if (SendHousePayRentDialogMethod != null) SendHousePayRentDialogMethod(this, title);
		}
		public Action<TestPacketLib, House> SendToggleHousePointsMethod { get; set; }
		public void SendToggleHousePoints(House house)
		{
			if (SendToggleHousePointsMethod != null) SendToggleHousePointsMethod(this, house);
		}
		public Action<TestPacketLib, House> SendRentReminderMethod { get; set; }
		public void SendRentReminder(House house)
		{
			if (SendRentReminderMethod != null) SendRentReminderMethod(this, house);
		}
		public Action<TestPacketLib, IList<InventoryItem>, byte, byte> SendMarketExplorerWindowMethod { get; set; }
		public void SendMarketExplorerWindow(IList<InventoryItem> items, byte page, byte maxpage)
		{
			if (SendMarketExplorerWindowMethod != null) SendMarketExplorerWindowMethod(this, items, page, maxpage);
		}
		public Action<TestPacketLib> SendMarketExplorerWindowMthod { get; set; }
		public void SendMarketExplorerWindow()
		{
			if (SendMarketExplorerWindowMthod != null) SendMarketExplorerWindowMthod(this);
		}
		public Action<TestPacketLib, long> SendConsignmentMerchantMoneyMethod { get; set; }
		public void SendConsignmentMerchantMoney(long money)
		{
			if (SendConsignmentMerchantMoneyMethod != null) SendConsignmentMerchantMoneyMethod(this, money);
		}
		public Action<TestPacketLib, House> SendHouseUsersPermissionsMethod { get; set; }
		public void SendHouseUsersPermissions(House house)
		{
			if (SendHouseUsersPermissionsMethod != null) SendHouseUsersPermissionsMethod(this, house);
		}
		public Action<TestPacketLib> SendStarterHelpMethod { get; set; }
		public void SendStarterHelp()
		{
			if (SendStarterHelpMethod != null) SendStarterHelpMethod(this);
		}
		public Action<TestPacketLib> SendPlayerFreeLevelUpdateMethod { get; set; }
		public void SendPlayerFreeLevelUpdate()
		{
			if (SendPlayerFreeLevelUpdateMethod != null) SendPlayerFreeLevelUpdateMethod(this);
		}
		public Action<TestPacketLib, GameMovingObject> SendMovingObjectCreateMethod { get; set; }
		public void SendMovingObjectCreate(GameMovingObject obj)
		{
			if (SendMovingObjectCreateMethod != null) SendMovingObjectCreateMethod(this, obj);
		}
		public Action<TestPacketLib, GamePlayer> SendSetControlledHorseMethod { get; set; }
		public void SendSetControlledHorse(GamePlayer player)
		{
			if (SendSetControlledHorseMethod != null) SendSetControlledHorseMethod(this, player);
		}
		public Action<TestPacketLib, GamePlayer, bool> SendControlledHorseMethod { get; set; }
		public void SendControlledHorse(GamePlayer player, bool flag)
		{
			if (SendControlledHorseMethod != null) SendControlledHorseMethod(this, player, flag);
		}
		public Action<TestPacketLib, GSTCPPacketOut, int, int> CheckLengthHybridSkillsPacketMethod { get; set; }
		public void CheckLengthHybridSkillsPacket(ref GSTCPPacketOut pak, ref int maxSkills, ref int first)
		{
			if (CheckLengthHybridSkillsPacketMethod != null) CheckLengthHybridSkillsPacketMethod(this, pak, maxSkills, first);
		}
		public Action<TestPacketLib> SendNonHybridSpellLinesMethod { get; set; }
		public void SendNonHybridSpellLines()
		{
			if (SendNonHybridSpellLinesMethod != null) SendNonHybridSpellLinesMethod(this);
		}
		public Action<TestPacketLib, string> SendCrashMethod { get; set; }
		public void SendCrash(string str)
		{
			if (SendCrashMethod != null) SendCrashMethod(this, str);
		}
		public Action<TestPacketLib> SendRegionColorSchemeMethod { get; set; }
		public void SendRegionColorScheme()
		{
			if (SendRegionColorSchemeMethod != null) SendRegionColorSchemeMethod(this);
		}
		public Action<TestPacketLib, byte> SendRegionColorSchemeByteMethod { get; set; }
		public void SendRegionColorScheme(byte color)
		{
			if (SendRegionColorSchemeByteMethod != null) SendRegionColorSchemeByteMethod(this, color);
		}
		public Action<TestPacketLib, GameLiving, bool> SendVampireEffectMethod { get; set; }
		public void SendVampireEffect(GameLiving living, bool show)
		{
			if (SendVampireEffectMethod != null) SendVampireEffectMethod(this, living, show);
		}
		public Action<TestPacketLib, byte> SendXFireInfoMethod { get; set; }
		public void SendXFireInfo(byte flag)
		{
			if (SendXFireInfoMethod != null) SendXFireInfoMethod(this, flag);
		}
		public Action<TestPacketLib, byte> SendMinotaurRelicMapRemoveMethod { get; set; }
		public void SendMinotaurRelicMapRemove(byte id)
		{
			if (SendMinotaurRelicMapRemoveMethod != null) SendMinotaurRelicMapRemoveMethod(this, id);
		}
		public Action<TestPacketLib, byte, ushort, int, int, int> SendMinotaurRelicMapUpdateMethod { get; set; }
		public void SendMinotaurRelicMapUpdate(byte id, ushort region, int x, int y, int z)
		{
			if (SendMinotaurRelicMapUpdateMethod != null) SendMinotaurRelicMapUpdateMethod(this, id, region, x, y, z);
		}
		public Action<TestPacketLib, GamePlayer, int, bool> SendMinotaurRelicWindowMethod { get; set; }
		public void SendMinotaurRelicWindow(GamePlayer player, int spell, bool flag)
		{
			if (SendMinotaurRelicWindowMethod != null) SendMinotaurRelicWindowMethod(this, player, spell, flag);
		}
		public Action<TestPacketLib, GamePlayer, int> SendMinotaurRelicBarUpdateMethod { get; set; }
		public void SendMinotaurRelicBarUpdate(GamePlayer player, int xp)
		{
			if (SendMinotaurRelicBarUpdateMethod != null) SendMinotaurRelicBarUpdateMethod(this, player, xp);
		}
		public Action<TestPacketLib, byte> SendBlinkPanelMethod { get; set; }
		public void SendBlinkPanel(byte flag)
		{
			if (SendBlinkPanelMethod != null) SendBlinkPanelMethod(this, flag);
		}

		public TestPacketLib()
		{
		}
	}
}
