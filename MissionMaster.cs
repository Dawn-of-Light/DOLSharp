using System.Text;
using DOL.GS.Quests;
using DOL.GS.PacketHandler;
using DOL.GS.Keeps;

namespace DOL.GS.Scripts
{
    public class MissionMaster : GameNPC
    {
		public override bool AddToWorld()
		{
			Name = "Frontier Watchman";
			GuildName = "Mission Master";
			Level = 60;
			switch (Realm)
			{
				case 0:
				case 1: Inventory = ClothingMgr.Albion_Lord.CloneTemplate(); break;
				case 2: Inventory = ClothingMgr.Midgard_Lord.CloneTemplate(); break;
				case 3: Inventory = ClothingMgr.Hibernia_Lord.CloneTemplate(); break;
			}
			switch (Realm)
			{
				case 0:
				case 1:
					{
						switch (Util.Random(0, 5))
						{
							case 0: Model = TemplateMgr.HighlanderMale; break;//Highlander Male
							case 1: Model = TemplateMgr.BritonMale; break;//Briton Male
							case 2: Model = TemplateMgr.AvalonianMale; break;//Avalonian Male
							case 3: Model = TemplateMgr.HighlanderFemale; break;//Highlander Female
							case 4: Model = TemplateMgr.BritonFemale; break;//Briton Female
							case 5: Model = TemplateMgr.AvalonianFemale; break;//Avalonian Female
						}
						break;
					}
				case 2:
					{
						switch (Util.Random(0, 7))
						{
							case 0: Model = TemplateMgr.DwarfMale; break;//Dwarf Male
							case 1: Model = TemplateMgr.NorseMale; break;//Norse Male
							case 2: Model = TemplateMgr.TrollMale; break;//Troll Male
							case 3: Model = TemplateMgr.KoboldMale; break;//Kobold Male
							case 4: Model = TemplateMgr.DwarfFemale; break;//Dwarf Female
							case 5: Model = TemplateMgr.NorseFemale; break;//Norse Female
							case 6: Model = TemplateMgr.TrollFemale; break;//Troll Female
							case 7: Model = TemplateMgr.KoboldFemale; break;//Kobold Female
						}
						break;
					}
				case 3:
					{
						switch (Util.Random(0, 7))
						{
							case 0: Model = TemplateMgr.CeltMale; break;//Celt Male
							case 1: Model = TemplateMgr.FirbolgMale; break;//Firbolg Male
							case 2: Model = TemplateMgr.LurikeenMale; break;//Lurikeen Male
							case 3: Model = TemplateMgr.ElfMale; break;//Elf Male
							case 4: Model = TemplateMgr.CeltFemale; break;//Celt Female
							case 5: Model = TemplateMgr.FirbolgFemale; break;//Firbolg Female
							case 6: Model = TemplateMgr.LurikeenFemale; break;//Lurikeen Female
							case 7: Model = TemplateMgr.ElfFemale; break;//Elf Female
						}
						break;
					}
			}
			return base.AddToWorld();
		}

        public override bool Interact(GamePlayer player)
        {
            //build string to to send to chat window
            StringBuilder chat = new StringBuilder(this.Name + " says, \"Would you like to aquire a [Mission]?");
            //send chat text
            player.Out.SendMessage(chat.ToString(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
			return base.Interact(player);
        }
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str))
                return false;
            GamePlayer player = (GamePlayer)source;
            switch (str)
            {
				case "Mission":
					{
						SayTo(player, "Would you like a [Personal], [Group] or [Guild] mission?");
						break;
					}
				#region Mission
				case "Personal":
					{
						if (player.Mission != null)
							player.Mission.ExpireMission();

						switch (Util.Random(0, 1))
						{
							//kill a certain number of enemy guards (15?)
							case 0:
								{
									player.Mission = new KillMission(typeof(GameKeepGuard), 15, "enemy realm guards", player);
									break;
								}
							//kill a certain number of enemy players. (5?)
							case 1:
								{
									player.Mission = new KillMission(typeof(GamePlayer), 5, "enemy players", player);
									break;
								}
							//scout an area in the enemy lands,
							case 2: break;
							//find an enemy assassin and slay him,
							case 3: break;
						}
						SayTo(player, player.Mission.Description);
						SayTo(player, "If you are unable to complete your mission, talk to me again to get a new [Personal] mission");
						break;
					}
				case "Group":
					{
						if (player.PlayerGroup == null)
						{
							SayTo(player, "You have no group!");
							return false;
						}

						if (player.PlayerGroup.Leader != player)
						{
							SayTo(player, "You are not the leader of your group!");
							return false;
						}

						if (player.PlayerGroup.Mission != null)
							player.PlayerGroup.Mission.ExpireMission();

						switch (Util.Random(0, 4))
						{
							//kill a certain number of enemy guards, (25?)
							case 0:
								{
									player.PlayerGroup.Mission = new KillMission(typeof(GameKeepGuard), 25, "enemy realm guards", player.PlayerGroup);
									break;
								}
							//kill a certain number of enemy players.
							case 1:
								{
									player.PlayerGroup.Mission = new KillMission(typeof(GamePlayer), 15, "enemy players", player.PlayerGroup);
									break;
								}
							//capture an enemy tower,
							case 2:
								{
									player.PlayerGroup.Mission = new CaptureMission(CaptureMission.eCaptureType.Tower, player.PlayerGroup);
									break;
								}
							//capture an enemy keep,
							case 3:
								{
									player.PlayerGroup.Mission = new CaptureMission(CaptureMission.eCaptureType.Keep, player.PlayerGroup);
									break;
								}
							//raze an enemy tower
							case 4:
								{
									player.PlayerGroup.Mission = new RaizeMission(player.PlayerGroup);
									break;
								}
							//destroy an enemy caravan,
						}
						SayTo(player, player.PlayerGroup.Mission.Description);
						SayTo(player, "If you are unable to complete your mission, talk to me again to get a new [Group] mission");
						break;
					}
				case "Guild":
					{
						if (player.Guild == null)
						{
							SayTo(player, "You have no guild!");
							return false;
						}

						if (player.Guild.GotAccess(player, eGuildRank.OcSpeak))
						{
							SayTo(player, "You are not high enough rank in your guild!");
							return false;
						}

						SayTo(player, "I have no guild missions at this time.");

						//Complete 20 kill enemy missions.
						//Complete five guard kill missions.
						//Take 5 towers belonging to a particular realm.
						//Take 2 keeps belonging to a particular realm.
						break;
					}
				#endregion
			}
            return true;
        }
    }
}
