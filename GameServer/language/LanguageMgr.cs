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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using DOL.Database;
using DOL.GS;
using log4net;

namespace DOL.Language
{
    public enum eTranslationKey
    {
        Area_Description,
        Area_ScreenDescription,
        MasterLevelStep,
        System_Text,
        Zone_Description,
        Zone_ScreenDescription,
    }

	public class LanguageMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region New language system experimental implementation

        #region Master level steps storage

        #region Complete

        public static string[,] MasterLevelStepsComplete = new string[,]
        {
            {
                "[Done] You have snuffed Ianetor's elemental flames!",
                "[Done] Justice has served Lornas!",
                "[Done] The enemy has been slain! The dying tribe can now flourish!",
                "[Done] Fadrin and his barrier have fallen! Those cursed are now free!",
                "[Done] You have successfully proven your might against Lord Krojer's champions!",
                "[Done] Kirkleis' curse has ended! The statue no longer rains fire!",
                "[Done] The Rassa has fallen!",
                "[Done] No shark roams the sea with a gilded ruby for a tummy!",
                "[Done] The curtain of power has been removed. Desmona no longer wears her harpy crown!",
                "[Done] Brave are those that brought Cetus down!"
            },
            {
                "[Done] The dark stalker is slain!",
                "[Done] You have eliminated the assassins!",
                "[Done] You have raided the Triton hoards!",
                "[Done] Rrionne's reflection faded along with her powers!",
                "[Done] You have outwitted Chaths and defeated his Test of Strategy!",
                "[Done] You have finally mastered Kanahkt's test!",
                "[Done] You have faced a darkness in the Eternal!",
                "[Done] You successfully stood your ground against Amenemhat!",
                "[Done] The Battle Masters have fallen before you and acknowledged your extraordinary skills in battle!",
                "[Done] The Ankh is destroyed and the imposter is slain!"
            },
            {
                "[Done] Moirai has been vanquished, and the runic power is yours!",
                "[Done] Kepa has fallen, her power is yours!",
                "[Done] Casta has been defeated, another runic power is yours!",
                "[Done] Another runic power has been gathered, Laodamedia is defeated!",
                "[Done] Antioos has been defeated, another runic power is yours!",
                "[Done] You have defeated the glowing barracuda, and gathered another runic power!",
                "[Done] Shraogh's reign has ended, and another runic power is in your grasp!",
                "[Done] The runic sting ray has fallen, and another runic power has been absorbed!",
                "[Done] Sinovia the octopus has been defeated, the runic power conveyed!",
                "[Done] Medusa has been vanquished, you are victorious in another trial!"
            },
            {
                "[Done] Colossal has been destroyed!",
                "[Done] You have conquered Seti the Pharaoh and his Fortress of Storms!",
                "[Done] The desert is silent! Sekhmet's roar heard no more!",
                "[Done] You've acquired the Radiant Sunstone!",
                "[Done] The Jann no longer play in the desert sands!",
                "[Done] Oukesson the Ghillan and his clones have been destroyed!",
                "[Done] The guardian of the river Iter awakens!",
                "[Done] The blood of Daehien flows no more!",
                "[Done] The marid, Gaurmaes will plague the Land of Atum no more!",
                "[Done] Martikhoras has met death!"
            },
            {
                "[Done] The jar has been found!",
                "[Done] The jar has been found!",
                "[Done] The jar has been found!",
                "[Done] The jar has been found!",
                "[Done] The mirror of Ra has been returned, the entrance to the Halls of Ma'ati is illuminated!",
                "[Done] You've received the resin needed to embalm!",
                "[Done] The mask has been retrieved!",
                "[Done] Your wrappings have been secured, soon you can face Ammut!",
                "[Done] You have drunk from the Fountain of Life and removed the curse of Ma'ati!",
                "[Done] You have successfully faced Ammut, devourer of souls!"
            },
            {
                "[Done] A haje-uraei has been slain!",
                "[Done] The sacrifice has been performed, the task completed!",
                "[Done] The way is clear for the doubters' return to the desert!",
                "[Done] Pallida-uraei has been hunted!",
                "[Done] The vents are cooled, and you have received the ash!",
                "[Done] The sacrificer Siraadi and his chanters have died, the power of the pillars will diminish.",
                "[Done] The guardians have failed to protect Ankhkare.",
                "[Done] The braziers no longer summon the efreet!",
                "[Done] The minotaur fortress has been breached and the mighty Vazul defeated!",
                "[Done] The Chimera has fallen!"
            },
            {
                "[Done] You have passed the gauntlet of fire!",
                "[Done] You have engaged the four feuding factions extensively, and have learned all you can about their combat strategies. You have proven yourself patient and observant.",
                "[Done] You have endured the test of the Apophians!",
                "[Done] You have endured the test of the Volurgons!",
                "[Done] You have endured the test of the Shaitan!",
                "[Done] You have endured the test of the Hephaestians!",
                "[Done] You have faced the challenge and defeated the Flame of Volcanus!",
                "[Done] Katorii's gaze is as dead as the halls in the heart of Volcanus!",
                "[Done] The way is clear!",
                "[Done] Typhon has been defeated!"
            },
            {
                "[Done] Bisul no longer controls the statues!",
                "[Done] Tholos no longer holds his shield!",
                "[Done] The sword has been liberated from the Jinn!",
                "[Done] Agne's statue will stand again!",
                "[Done] Kratos has honored his deal, and the leg has been found!",
                "[Done] The head of Agne has fallen!",
                "[Done] The torso has been found!",
                "[Done] Many gorgons have died and Agne's left arm has been found.",
                "[Done] The arms has been recovered from the great hall!",
                "[Done] Agne's army is defeated and Talos has fallen!"
            },
            {
                "[Done] You have defeated mighty Kyros!",
                "[Done] Lachlen was prepared to face you in battle but fell nonetheless!",
                "[Done] Evzen is destroyed and the warlords incapacitated!",
                "[Done] You awoke the four kings and defeated them in battle!",
                "[Done] Nelos was destroyed in teaching his lesson to you!",
                "[Done] The goal is within sight!",
                "[Done] The strongest of all Iaculi is dead and you still survive!",
                "[Done] You have proven yourself worthy of the Phoenix's Guardians.",
                "[Done] Neola has failed to protect her sisters.",
                "[Done] The phoenix has fallen!"
            },
        };

        #endregion

        #region Uncomplete

        public static string[,] MasterLevelStepsUncomplete = new string[,]
        {
            {
                "Ianetor likes to play with fire, and is seeking adventurers to participate in an explosive game in Oceanus Anatole! This is a group encounter.",
                "The thief that robbed Lornas still lives! This is a group encounter.",
                "The Zhton and Kynhroe clans of tritons in Messothalassa and Oceanus Boreal are dying, yet they still war with each other! Each clan seeks adventurers to fight for their cause! This is a battlegroup encounter.",
                "The barrier still holds those taken in by the cursed sea of Oceanus Hesperos! This is a group encounter.",
                "You have not yet proven yourself agaisnt the might of Lord Krojer's champions in single combat! This is a solo encounter.",
                "Kirkleis still looks upon himself with hatred in the mirror that mocks his curse! This is a battlegroup encounter.",
                "The portals lie dark and silent still! The Rassa remains locked away from those who would seek its power! This is a battlegroup encounter.",
                "The ruby lies in the sand beneath the head that stares one eye into the eternal depth! This is a battlegroup encounter.",
                "Desmona still wears her crown, the token needed remains unfound! This is a battlegroup encounter.",
                "Face Cetus with a ring, a ruby, a mirror and a crown! This is a battlegroup encounter."
            },
            {
                "Death stalks you from the shadows in the Temple of the Sobekite Eternal! This is a battlegroup encounter.",
                "The imposter's assasins are slaying the keepers of the Temple of the Sobekite Eternal! This is a battlegroup encounter.",
                "The triton chiefs and their followers in the flooded depths of the Temple of the Sobekite Eternal hoard stolen goods! This is a battlegroup encounter.",
                "Rrionnes' reflection holds the key to her invulerability! This is a battlegroup encounter.",
                "You have not beaten Chaths' Test of Strategy! This is a group encounter.",
                "Kanahkt's test seems to be too difficult for you! This is a battlegroup encounter.",
                "You mush face fear, terror, or horror! This is a battlegroup encounter.",
                "To prove victorious, you must face yourself! This is a group encounter.",
                "Defeat the Battle Masters who once defeated the trial of the Sobekite Eternal! This is a battle group encounter.",
                "An imposter has stolen the Ankh of Life, granting him immortality as long as he resides in the Temple of the Sobekite Eternal! This is a battlegroup encounter."
            },
            {
                "Moirai still holds the runic power behind her magical shielding!",
                "Kepa has agreed to give you her runic power, will you help?",
                "Casta still uses her artifacts to slay anyone who enters her lair!",
                "Laodamedia still rigorously guards her lair!",
                "Antioos asks for your help in exchange for another runic power!",
                "A strange barracuda resides in the darkness, glowing with power!",
                "Shraogh still strikes fear into the heart of those who oppose him!",
                "The sting ray swarm with an unnatural glowing power!",
                "Sinovia the octopus devours those who come near!",
                "Medusa still turns anyone who opposes her into stone! This is a battlegroup encounter."
            },
            {
                "Colossal has yet to be summoned from the scorpions of the Stygian Desert! This is a group encounter.",
                "The Fortress of Storms has not been taken! This is a battlegroup encounter.",
                "The Mighty One, the Eye of Ra, is rumored to roam the desert Land of Atum still! This is a group encounter.",
                "A valuable stone to some is but a bauble to a child! This is a battlegroup encounter.",
                "The Jann of the Land of Atum whirl among the dunes! This is a battlegroup encounter.",
                "Oukesson the Ghillian is still multiplying when fought! This is a group encounter.",
                "The guardians of the river Iter have yet to waken! This is a group encounter.",
                "he blood of Daehien still flows in the Land of Atum! This is a battlegroup encounter. ",
                "The marid, Gaurmaes, is still safe behind tornados and dust! This is a battlegroup encounter.",
                "Martikhoras reigns in his pyramid still! This is a battlegroup encounter."
            },
            {
                "The Canopic Jar - Intestines must be stolen from the Echo of Qebehsenuef! This is a group encounter.",
                "Another Canopic Jar - Stomach must be stolen from the Echo of Duamutef! This is a group encounter.",
                "Another Canopic Jar - Lungs must be stolen from the Echo of Hapy! This is a group encounter.",
                "Another Canopic Jar - Liver must be stolen from the Echo of Imsety! This is a group encounter.",
                "The gorgons of Aeurs still hold Cau Suraes mirror of Ra! This is a group encounter.",
                "The bound ones hold the resin that preserves! This is a group encounter.",
                "The bound ones hold a mask still needed to face Ammut! This is a group encounter.",
                "Wrappings can be found amongst the bound ones. This is a group encounter.",
                "You must drink the poisoned waters for the aid of Mahaf! This is a group encounter.",
                "You must face Ammut in the depths of the Halls of Ma'ati and be judged! This is a battlegroup encounter."
            },
            {
                "Look for a rare uraeus from where they appear. This is a group encounter.",
                "The sacrifice has yet to take place, the salamander has yet to be born! This is a battlegroup encounter.",
                "Lateef laments his fate as follower of Am-he and wishes to return to the desert! This is a battlegroup encounter.",
                "Pallida-uraei has yet to appear! Seek out the homes of the Uraeus! This is a battlegroup encounter.",
                "The flame spouts in easternmost Volcanus guard the objects, the flames must be cooled! This is a solo encounter.",
                "Seek out the sacrificer Siraadi amongst the pillars of vengenance in Typhon's Reach! This is a battlegroup encounter.",
                "Three of Ankhkare guardians guard him well. This is a battlegroup encounter.",
                "In the center of the island braziers still call efreet! This is a battlegroup encounter.",
                "Vazul's Fortress in the Azhen Isles has yet to be breached! This is a battlegroup encounter.",
                "The Chimera still holds strong in her fortress in East Ashen Isles! This is a battlegroup encounter."
            },
            {
                "You must acquire the gauntlet of fire! This is a group encounter.",
                "You can learn all there is to know of your opponent through combat, but time multiplies when there are four. It is not for you to end the feud, but you can't move on until they have all seen you as a threat. This is a group encounter.",
                "The Apophians wish to test your endurance. This is a group encounter.",
                "The Volurgons wish to test your endurance. This is a group encounter.",
                "The Shaitan wish to test your endurance. This is a group encounter.",
                "The Hephaestians wish to test your endurance. This is a group encounter.",
                "You must face a challeng in the Chamber of flame! This is a battlegroup encounter.",
                "Katorii's gaze destroys all life it falls upon, yes she must be destroyed! This is a battlegroup encounter.",
                "A mysterious force blocks your path! This is a battlegroup encounter.",
                "Typhon lies in the lava pool in the center of Volcanus deep! This is a battlegroup encounter."
            },
            {
                "Bisul the jinni still has the control crystal! This is a battlegroup encounter. ",
                "Agnes shield was destroyed long ago, but it has a twin in his brother Tholoss prossession! This is a battlegroup encounter.",
                "The sword the great Agne is now in the hands of a mischievous Jinn! This is a battlegroup encounter.",
                "Once noble and proud Agnes, now bits strewn about a Centaur's home. This is a group encounter.",
                "Kratos has not handed over the leg of Agne! This is a group encounter.",
                "Dawars servant bears the head of Agne! This is a battlegroup encounter.",
                "Find the torso of Agne amongst the statues of Aerus! This is a group encounter.",
                "A group of gorgons have collaborated to thwart your effort to rebuild Agne by hiding his left arm because the trials resulted in many gorgon deaths in the past! This is a battlegroup encounter.",
                "The right arm lies in the once great hall of a wreaked Atlantean temple. This is a group encounter.",
                "First you must defeat Agnes army on his battlefield then you must topple Talos in his temple! This is a battlegroup encounter."
            },
            {
                "Long ago Kyros demonstrated himself to be the ultimate warrior serving many different roles in combat, would you dare challenge him? This is a battlegroup encounter.",
                "Lachlen was a great explorer who was always prepared to face the unique dangers lurking in each new land! This is a group encounter.",
                "Long ago Evzen unified three warring territories after each territorys warlord was slain then ruled the combined landsd with an iron fist! This is a battlegroup encounter.",
                "Four kings likenesses are dormant and must be awakened! This is a battlegroup encounter.",
                "Nelos would like to teach you a lesson that you might not survive! This a battlegroup encounter.",
                "Centaur patrols led by Katri hinder you from reaching your goal! This is a battlegroup encounter.",
                "Only the strongest Iaculi survive as they cannibalize each other. This is a battlegroup encounter.",
                "You have not yet proven yourself worthy to the Phoenix's Guardians! This is a battlegroup encounter.",
                "Neola still guards her sisters well. This is a battlegroup encounter.",
                "The phoenix is not yet weakend, and still reigns above! This is a battlegroup encounter."
            }
        };

        #endregion

        #endregion

        /// <summary>
        /// Returns a list with all language keys that are allowed to use on the server.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllowedLangKeys()
        {
            log.Debug("Entering GetAllowedLangKeys");

            List<string> allowedKeys = new List<string>();
            string[] tmp = DOL.GS.ServerProperties.Properties.ALLOWED_CUSTOM_LANGUAGE_KEYS.Replace(" ", "").Split(';');

            foreach (string allowedKey in tmp)
                allowedKeys.Add(allowedKey);

            if (!allowedKeys.Contains("EN"))
                allowedKeys.Add("EN");

            foreach(string str in allowedKeys)
                log.Debug("GetAllowedLangKeys:" + str);

            return allowedKeys;
        }

        /// <summary>
        /// Returns true if the given language key is allowed to use on the server, or false if not.
        /// </summary>
        /// <param name="langKey">The language key to check</param>
        /// <returns></returns>
        public static bool IsLangKeyAllowedToUse(string langKey)
        {
            log.Debug("Entering IsLangKeyAllowedToUse");
            return GetAllowedLangKeys().Contains(langKey) ? true : false;
        }

        /// <summary>
        /// Returns an translated string
        /// </summary>
        /// <param name="language">The language</param>
        /// <param name="eKey">The eTranslationKey to use</param>
        /// <param name="translationID">The translation id</param>
        /// <param name="unique"></param>
        /// <returns></returns>
        public static string GetTranslation(string language, eTranslationKey eKey, string translationID, string unique)
        {
            log.Debug("Entering GetTranslation");

            if (Util.IsEmpty(translationID, true))
                return "translate error";

            //First, check if the clients language key is an allowed language key.
            if (language == "EN" || !IsLangKeyAllowedToUse(language))
                return translationID;

            string translation = "";

            //Now let us look what translation type is requested
            switch (eKey)
            {
                #region Area_Description
                case eTranslationKey.Area_Description:
                    {
                        var dbo = GameServer.Database.SelectObject<DBLanguageArea>("TranslationId='" + GameServer.Database.Escape(translationID) + "' AND Language='" + GameServer.Database.Escape(language) + "'");
                        if (dbo != null)
                        {
                            //Lets check if we have text
                            if (!Util.IsEmpty(dbo.Description, true))
                                translation = dbo.Description;
                        }

                    } break;
                #endregion
                #region Area_ScreenDescription
                case eTranslationKey.Area_ScreenDescription:
                    {
                        var dbo = GameServer.Database.SelectObject<DBLanguageArea>("TranslationId='" + GameServer.Database.Escape(translationID) + "' AND Language='" + GameServer.Database.Escape(language) + "'");
                        if (dbo != null)
                        {
                            //Lets check if we have text
                            if (!Util.IsEmpty(dbo.ScreenDescription, true))
                                translation = dbo.ScreenDescription;
                        }
                    } break;
                #endregion
                #region MasterLevelStep
                case eTranslationKey.MasterLevelStep:
                    {
                        var dbo = GameServer.Database.SelectObject<DBLanguageMasterLevelStep>("TranslationId='" + GameServer.Database.Escape(translationID) + "' AND Language='" + GameServer.Database.Escape(language) + "'");
                        if (dbo != null)
                        {
                            //Lets check if we have text
                            if (!Util.IsEmpty(dbo.Text, true))
                                translation = dbo.Text;
                        }
                    } break;
                #endregion
                #region System_Text
                case eTranslationKey.System_Text:
                    {
                        log.Debug("Entering case");
                        if (!string.IsNullOrWhiteSpace(unique))
                        {
                            log.Debug("Entering case if block");
                            var dbo = GameServer.Database.SelectObject<DBLanguageSystemText>("TranslationId='" + GameServer.Database.Escape(translationID) + "' AND TranslationUnique='" + GameServer.Database.Escape(unique) + "' AND Language='" + GameServer.Database.Escape(language) + "'");
                            if (dbo != null)
                            {
                                //Lets check if we have text
                                if (!Util.IsEmpty(dbo.Text, true))
                                    translation = dbo.Text;
                            }
                        }
                        else
                        {
                            var dbo = GameServer.Database.SelectObject<DBLanguageSystemText>("TranslationId='" + GameServer.Database.Escape(translationID) + "' AND Language='" + GameServer.Database.Escape(language) + "'");
                            if (dbo != null)
                            {
                                log.Debug("Entering case else block");
                                //Lets check if we have text
                                if (!Util.IsEmpty(dbo.Text, true))
                                    translation = dbo.Text;
                            }
                        }
                    } break;
                #endregion
                #region Zone_Description
                case eTranslationKey.Zone_Description:
                    {
                        var dbo = GameServer.Database.SelectObject<DBLanguageZone>("TranslationId='" + GameServer.Database.Escape(translationID) + "' AND Language='" + GameServer.Database.Escape(language) + "'");
                        if (dbo != null)
                        {
                            //Lets check if we have text
                            if (!Util.IsEmpty(dbo.Description, true))
                                translation = dbo.Description;
                        }
                    } break;
                #endregion
                #region Zone_ScreenDescription
                case eTranslationKey.Zone_ScreenDescription:
                    {
                        var dbo = GameServer.Database.SelectObject<DBLanguageZone>("TranslationId='" + GameServer.Database.Escape(translationID) + "' AND Language='" + GameServer.Database.Escape(language) + "'");
                        if (dbo != null)
                        {
                            //Lets check if we have text
                            if (!Util.IsEmpty(dbo.ScreenDescription, true))
                                translation = dbo.ScreenDescription;
                        }
                    } break;
                #endregion
            }

            if (Util.IsEmpty(translation, true))
                translation = translationID; //Will be changed after a little talk with Graveen/Tolakram -> http://www.dolserver.net/viewtopic.php?p=124343#p124343

            log.Debug("Returning translation:" + translation);

            return translation;
        }

        /// <summary>
        /// Translate the sentence
        /// </summary>
        /// <param name="client"></param>
        /// <param name="eKey"></param>
        /// <param name="translationID"></param>
        /// <param name="unique"></param>
        /// <returns></returns>
        public static string GetTranslation(GameClient client, eTranslationKey eKey, string translationID, string unique)
        {
            if (client == null || client.Account == null)
                //return GetTranslation(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE, eKey, translationID, unique);
                return translationID;

            //if (client.Player != null && client.Account.PrivLevel > 1)
            //{
            //    bool debug = client.Player.TempProperties.getProperty("LANGUAGEMGR-DEBUG", false);
            //    if (debug && IDSentences.ContainsKey(TranslationID))
            //        return "[" + TranslationID + "]=<" + IDSentences[TranslationID][client.Account.Language] + ">";
            //}

            return GetTranslation(client.Account.Language, eKey, translationID, unique);
        }

        #endregion

        /// <summary>
		/// All the sentences [TranslationID] [Language] = [Sentence]
		/// </summary>
		public static Dictionary<string, Dictionary<string, string>> IDSentences;
		
		/// <summary>
		/// Give a way to change or relocate the lang files
		/// </summary>
		private static string LangPath = Path.Combine(GameServer.Instance.Configuration.RootDirectory ,"languages");
		public static void SetLangPath(string path)
		{
			LangPath = path;
		}
		
		/// <summary>
		/// Count files in a language directory
		/// </summary>
		/// <param name="abrev"></param>
		/// <returns></returns>
		private static int CountLanguageFiles(string abrev)
		{
			int count = 0;
			string langPath = Path.Combine(LangPath , abrev );
			
			if (!Directory.Exists(langPath))
				return count;

			foreach (string file in Directory.GetFiles(langPath, "*", SearchOption.AllDirectories))
			{
				if (!file.EndsWith(".txt"))
					continue;
				count++;
			}

			return count;
		}

		public static bool Refresh(string TranslationID)
		{
			if (!LanguageMgr.IDSentences.ContainsKey(TranslationID)) return false;
			DBLanguage obj = GameServer.Database.SelectObject<DBLanguage>("`TranslationID` = '" + GameServer.Database.Escape(TranslationID) + "'");
			if (obj == null) return false;
			IDSentences[obj.TranslationID]["EN"] = obj.EN;
			IDSentences[obj.TranslationID]["DE"] = obj.DE != null && obj.DE != "" ? obj.DE : obj.EN;
			IDSentences[obj.TranslationID]["FR"] = obj.FR != null && obj.FR != "" ? obj.FR : obj.EN;
			IDSentences[obj.TranslationID]["IT"] = obj.IT != null && obj.IT != "" ? obj.IT : obj.EN;
			IDSentences[obj.TranslationID]["CU"] = obj.CU != null && obj.CU != "" ? obj.CU : obj.EN;
			return true;
		}

		/// <summary>
		/// Load language database and files
		/// </summary>
		/// <param name="lng"></param>
		/// <param name="abrev"></param>
		/// <param name="longname"></param>
		/// <returns></returns>
		private static bool LoadLanguages()
		{
			if (log.IsInfoEnabled)
				log.Info("[LanguageMgr] Loading translations ID...");
			
			if (DOL.GS.ServerProperties.Properties.USE_DBLANGUAGE)
			{
				IList<DBLanguage> objs = GameServer.Database.SelectAllObjects<DBLanguage>();
				foreach (DBLanguage obj in objs)
				{
					if (!IDSentences.ContainsKey(obj.TranslationID))
					{
						IDSentences.Add(obj.TranslationID, new Dictionary<string, string>());
						IDSentences[obj.TranslationID].Add("EN", obj.EN);
						IDSentences[obj.TranslationID].Add("DE", obj.DE != null && obj.DE != "" ? obj.DE : obj.EN);
						IDSentences[obj.TranslationID].Add("FR", obj.FR != null && obj.FR != "" ? obj.FR : obj.EN);
						IDSentences[obj.TranslationID].Add("IT", obj.IT != null && obj.IT != "" ? obj.IT : obj.EN);
						IDSentences[obj.TranslationID].Add("CU", obj.CU != null && obj.CU != "" ? obj.CU : obj.EN);
					}
				}
			}

			if (!IDSentences.ContainsKey("SHORT_NAME"))
			{
				IDSentences.Add("SHORT_NAME", new Dictionary<string, string>());
				IDSentences["SHORT_NAME"].Add("EN", "EN");
				IDSentences["SHORT_NAME"].Add("DE", "DE");
				IDSentences["SHORT_NAME"].Add("FR", "FR");
				IDSentences["SHORT_NAME"].Add("IT", "IT");
				IDSentences["SHORT_NAME"].Add("CU", "CU");
			}

			if (!IDSentences.ContainsKey("LONG_NAME"))
			{
				IDSentences.Add("LONG_NAME", new Dictionary<string, string>());
				IDSentences["LONG_NAME"].Add("EN", "English");
				IDSentences["LONG_NAME"].Add("DE", "Deutsch");
				IDSentences["LONG_NAME"].Add("FR", "Français");
				IDSentences["LONG_NAME"].Add("IT", "Italian");
				IDSentences["LONG_NAME"].Add("CU", "Custom");
			}

			m_refreshFromFiles = new List<string>();
			foreach (string abrev in IDSentences["SHORT_NAME"].Keys)
				CheckFromFiles(abrev);

			if (DOL.GS.ServerProperties.Properties.USE_DBLANGUAGE)
			{
				if (m_refreshFromFiles.Count > 0)
				{
					foreach (string id in m_refreshFromFiles)
					{
						bool create = false;
						if (IDSentences.ContainsKey(id))
						{
							DBLanguage obj = (DBLanguage)GameServer.Database.SelectObject<DBLanguage>("`TranslationID` = '" + GameServer.Database.Escape(id) + "'");
							if (obj == null)
							{
								obj = new DBLanguage();
								obj.TranslationID = id;
								create = true;
							}
							obj.EN = IDSentences[id]["EN"];
							obj.DE = IDSentences[id]["DE"];
							obj.FR = IDSentences[id]["FR"];
							obj.IT = IDSentences[id]["IT"];
							obj.CU = IDSentences[id]["CU"];
							if (create) GameServer.Database.AddObject(obj);
							else GameServer.Database.SaveObject(obj);
						}

						if (log.IsWarnEnabled)
							log.Warn("[LanguageMgr] TranslationID <" + id + "> " + (create ? "created" : "updated") + " in database!");
					}

					if (log.IsWarnEnabled)
						log.Warn("[LanguageMgr] Loaded " + m_refreshFromFiles.Count + " translations ID from files to database!");
				}
			}
			m_refreshFromFiles.Clear();

			if (log.IsInfoEnabled)
				log.Info("[LanguageMgr] Translations ID loaded.");
			
			return true;
		}

		private static IList<string> m_refreshFromFiles;

		private static void CheckFromFiles(string abrev)
		{
			if (CountLanguageFiles(abrev) == 0)
			{
				log.Error(abrev + " language not found!");
				
				if (DOL.GS.ServerProperties.Properties.SERV_LANGUAGE == abrev)
				{
					log.Error("Default " + abrev + " language files missing!! Server can't start without!");
					if (GameServer.Instance != null)
						GameServer.Instance.Stop();
					return;
				}
			}

			string langPath = Path.Combine(LangPath , abrev );
			foreach (string FilePath in Directory.GetFiles(langPath, "*", SearchOption.AllDirectories))
			{
				if (!FilePath.EndsWith(".txt"))
					continue;

				string[] lines = File.ReadAllLines(FilePath, Encoding.GetEncoding("utf-8"));
				IList textList = new ArrayList(lines);
				
				foreach (string line in textList)
				{
					if (line.StartsWith("#"))
						continue;

					if (line.IndexOf(':') == -1)
						continue;

					string[] splitted = new string[2];

					splitted[0] = line.Substring(0, line.IndexOf(':'));
					splitted[1] = line.Substring(line.IndexOf(':') + 1);
					
					splitted[1] = splitted[1].Replace("\t", " ");
					splitted[1] = splitted[1].Trim();

					if (!IDSentences.ContainsKey(splitted[0]))
					{
						IDSentences.Add(splitted[0], new Dictionary<string,string>());
						IDSentences[splitted[0]].Add("EN", splitted[1]);
						IDSentences[splitted[0]].Add("DE", "");
						IDSentences[splitted[0]].Add("FR", "");
						IDSentences[splitted[0]].Add("IT", "");
						IDSentences[splitted[0]].Add("CU", "");
						if (!m_refreshFromFiles.Contains(splitted[0]))
							m_refreshFromFiles.Add(splitted[0]);
					}
					else if(m_refreshFromFiles.Contains(splitted[0]))
					{
						if (!IDSentences[splitted[0]].ContainsKey(abrev))
							IDSentences[splitted[0]].Add(abrev, "");
						IDSentences[splitted[0]][abrev] = splitted[1];
					}
				}
			}
		}


		/// <summary>
		/// Initial function
		/// </summary>
		/// <returns></returns>
		public static bool Init()
		{
			IDSentences = new Dictionary<string, Dictionary<string, string>>();
			
			LoadLanguages();

			return true;
		}


		/// <summary>
		/// Convert the shortname into enumerator
		/// </summary>
		/// <param name="abrev"></param>
		/// <returns></returns>
		public static string NameToLangs(string abrev)
		{
			if (!IDSentences.ContainsKey("SHORT_NAME")
			    || !IDSentences["SHORT_NAME"].ContainsKey(abrev)) return "EN";

			return IDSentences["SHORT_NAME"][abrev];
		}


		/// <summary>
		/// Convert the enumerator into shortname
		/// </summary>
		/// <param name="l"></param>
		/// <returns></returns>
		public static string LangsToName(string lng)
		{
			if (!IDSentences.ContainsKey("SHORT_NAME")
			    || !IDSentences["SHORT_NAME"].ContainsKey(lng)) return "EN";

			return IDSentences["SHORT_NAME"][lng];
		}


		/// <summary>
		/// Convert the enumerator into longname
		/// </summary>
		/// <param name="c"></param>
		/// <param name="l"></param>
		/// <returns></returns>
		public static string LangsToCompleteName(GameClient client, string lng)
		{
			if (!IDSentences.ContainsKey("LONG_NAME")
			    || !IDSentences["LONG_NAME"].ContainsKey(lng)) return "English";

			return IDSentences["LONG_NAME"][lng];
		}


		/// <summary>
		/// This returns the last part of the translation text id if actual translation fails
		/// This helps to avoid returning strings that are too long and overflow the client
		/// In addition, later version clients seem to reject names with special characters in them
		/// </summary>
		/// <param name="TranslationID"></param>
		/// <returns></returns>
		public static string GetTranslationErrorText(string lang, string TranslationID)
		{
			try
			{
				string str = TranslationID;

				// trying to find entries like "GamePlayer.Title" and not "This is an untranslated sentence"
				if (str.Contains(".") && str.Contains(" ") == false && str.EndsWith(".") == false)
					str = TranslationID.Substring(TranslationID.LastIndexOf(".") + 1);

				if (str.Length > 0)
					return str;
				else
					return lang + " no text found";
			}
			catch
			{
			}

			return lang + " Error";
		}
		

		/// <summary>
		/// Translate the sentence
		/// </summary>
		/// <param name="lang"></param>
		/// <param name="TranslationID"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string GetTranslation(string lang, string TranslationID, params object [] args)
		{
			// in case args are null, set them to an empty array so we don't catch any null refs
			if(args == null)
			{
				args = new object[0];
			}

			string translated = TranslationID;

			if (IDSentences.ContainsKey(TranslationID) == false)
			{
				return GetTranslationErrorText(lang, translated);
			}

			if (IDSentences[TranslationID].ContainsKey(lang) == false || IDSentences[TranslationID][lang].Length == 0)
			{
				// if language is invalid, not found, or the returned string is empty then give english a try
				lang = "EN";

				if (IDSentences[TranslationID].ContainsKey(lang) == false)
				{
					log.ErrorFormat("LanguageMGR: No default EN entry found for {0}!", TranslationID);
					return GetTranslationErrorText(lang, translated);
				}
			}

			translated = IDSentences[TranslationID][lang];

			if (translated.Length == 0)
			{
				// never allow the return of an empty string
				log.ErrorFormat("LanguageMGR: String empty on translation of {0} to language {1}!", TranslationID, lang);
				return lang + " no text found";
			}

			try
			{
				if (args.Length > 0)
					translated = string.Format(translated, args);
			}
			catch
			{
				log.ErrorFormat("LanguageMGR: Parameter number incorrect: {0} for language {1}, Arg count = {2}", TranslationID, lang, args.Length);
			}

			return translated;
		}


		/// <summary>
		/// Translate the sentence
		/// </summary>
		/// <param name="client"></param>
		/// <param name="TranslationID"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string GetTranslation(GameClient client, string TranslationID, params object[] args)
		{
			if (client == null || client.Account == null)
				return GetTranslation(DOL.GS.ServerProperties.Properties.SERV_LANGUAGE, TranslationID, args);

			if(client.Player != null && client.Account.PrivLevel > 1)
			{
				bool debug = client.Player.TempProperties.getProperty("LANGUAGEMGR-DEBUG", false);
				if (debug && IDSentences.ContainsKey(TranslationID))
					return "[" + TranslationID + "]=<" + IDSentences[TranslationID][client.Account.Language] + ">";
			}

			return GetTranslation(client.Account.Language, TranslationID, args);
		}
	}
}