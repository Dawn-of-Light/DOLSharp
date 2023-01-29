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
 */
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.GS.PlayerClass;
using DOL.GS.ServerProperties;
using DOL.Language;


namespace DOL.GS.Keeps
{
	public class GuardCaster : GameKeepGuard
	{
		public const int INTERVAL = 360 * 1000;

		protected virtual int Timer(RegionTimer callingTimer)
		{
			if (base.IsAlive)
			{
				foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendSpellCastAnimation(this, 4321, 30);
					RegionTimer timer = new RegionTimer(player, new RegionTimerCallback(ShowEffect), 3000);
				}
			}
			return INTERVAL;
		}

		public int ShowEffect(RegionTimer timer)
		{
			if (base.IsAlive)
			{
				foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendSpellEffectAnimation(this, this, 4321, 0, false, 1);
				}
				foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				{

					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GuardCaster.SkinsHardens", this.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

				}
			}
			timer.Stop();
			timer = null;
			return 0;
		}

		public override bool AddToWorld()
		{
			bool success = base.AddToWorld();
			if (success) new RegionTimer(this, new RegionTimerCallback(Timer), INTERVAL);
			return success;
		}

		public override double GetArmorAbsorb(eArmorSlot slot)
		{
			return base.GetArmorAbsorb(slot) - 0.05;
		}

		protected override ICharacterClass GetClass()
		{
			if (ModelRealm == eRealm.Albion) return new ClassWizard();
			else if (ModelRealm == eRealm.Midgard) return new ClassRunemaster();
			else if (ModelRealm == eRealm.Hibernia) return new ClassEldritch();
			return new CharacterClassBase();
		}

		protected override KeepGuardBrain GetBrain() => new CasterBrain();

		protected override void SetName()
		{
			switch (ModelRealm)
			{
				case eRealm.None:
				case eRealm.Albion:
					if (IsPortalKeepGuard)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.MasterWizard");
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Wizard");
					break;
				case eRealm.Midgard:
					if (IsPortalKeepGuard)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.MasterRunes");
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Runemaster");
					break;
				case eRealm.Hibernia:
					if (IsPortalKeepGuard)
						Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.MasterEldritch");
					else Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Eldritch");
					break;
			}

			if (Realm == eRealm.None)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Renegade", Name);
			}
		}
	}
}
