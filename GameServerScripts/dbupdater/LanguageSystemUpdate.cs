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

using System.Collections.Generic;

using DOL.Database;
using DOL.Database.Attributes;
using log4net;

namespace DOL.GS.DatabaseUpdate
{
    [DatabaseUpdate]
    public class LanguageSystemUpdate : IDatabaseUpdater
    {
        #region DBLanguage table structure
        private class Language : DataObject
        {
            protected string m_translationid;
            protected string m_EN = "";
            protected string m_DE = "";
            protected string m_FR = "";
            protected string m_IT = "";
            protected string m_CU = "";
            protected string m_packageID;

            public Language() { }

            [DataElement(AllowDbNull = false, Unique = true)]
            public string TranslationID
            {
                get { return m_translationid; }
                set { Dirty = true; m_translationid = value; }
            }

            [DataElement(AllowDbNull = false)]
            public string EN
            {
                get { return m_EN; }
                set { Dirty = true; m_EN = value; }
            }

            [DataElement(AllowDbNull = true)]
            public string DE
            {
                get { return m_DE; }
                set { Dirty = true; m_DE = value; }
            }

            [DataElement(AllowDbNull = true)]
            public string FR
            {
                get { return m_FR; }
                set { Dirty = true; m_FR = value; }
            }

            [DataElement(AllowDbNull = true)]
            public string IT
            {
                get { return m_IT; }
                set { Dirty = true; m_IT = value; }
            }

            [DataElement(AllowDbNull = true)]
            public string CU
            {
                get { return m_CU; }
                set { Dirty = true; m_CU = value; }
            }

            [DataElement(AllowDbNull = true)]
            public string PackageID
            {
                get { return m_packageID; }
                set { Dirty = true; m_packageID = value; }
            }
        }
        #endregion DBLanguage table structure

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Update()
        {
            log.Info("Updating the LanguageSystem table (this can take a few minutes)...");

            if (GameServer.Database.GetObjectCount<DBLanguageSystem>() < 1 && ServerProperties.Properties.USE_DBLANGUAGE)
            {
                var objs = GameServer.Database.SelectAllObjects<Language>();
                if (objs.Count > 0)
                {
                    List<DBLanguageSystem> lngObjs = new List<DBLanguageSystem>();

                    foreach (Language obj in objs)
                    {
                        if (Util.IsEmpty(obj.TranslationID))
                            continue;

                        // This kind of row will later be readded by the LanguageMgr
                        // with it's updated values.
                        if (obj.TranslationID.Contains("System.LanguagesName."))
                            continue;

                        DBLanguageSystem lngObj = null;

                        if (!Util.IsEmpty(obj.EN))
                        {
                            if (!ListContainsObjectData(lngObjs, "EN", obj.TranslationID)) // Ignore duplicates
                            {
                                lngObj = new DBLanguageSystem();
                                lngObj.TranslationId = obj.TranslationID;
                                lngObj.Language = "EN";
                                lngObj.Text = obj.EN;
                                lngObj.Tag = obj.PackageID;
                                lngObjs.Add(lngObj);
                            }
                        }

                        if (!Util.IsEmpty(obj.DE))
                        {
                            if (!ListContainsObjectData(lngObjs, "DE", obj.TranslationID)) // Ignore duplicates
                            {
                                lngObj = new DBLanguageSystem();
                                lngObj.TranslationId = obj.TranslationID;
                                lngObj.Language = "DE";
                                lngObj.Text = obj.DE;
                                lngObj.Tag = obj.PackageID;
                                lngObjs.Add(lngObj);
                            }
                        }

                        if (!Util.IsEmpty(obj.FR))
                        {
                            if (!ListContainsObjectData(lngObjs, "FR", obj.TranslationID)) // Ignore duplicates
                            {
                                lngObj = new DBLanguageSystem();
                                lngObj.TranslationId = obj.TranslationID;
                                lngObj.Language = "FR";
                                lngObj.Text = obj.FR;
                                lngObj.Tag = obj.PackageID;
                                lngObjs.Add(lngObj);
                            }
                        }

                        if (!Util.IsEmpty(obj.IT))
                        {
                            if (!ListContainsObjectData(lngObjs, "IT", obj.TranslationID)) // Ignore duplicates
                            {
                                lngObj = new DBLanguageSystem();
                                lngObj.TranslationId = obj.TranslationID;
                                lngObj.Language = "IT";
                                lngObj.Text = obj.IT;
                                lngObj.Tag = obj.PackageID;
                                lngObjs.Add(lngObj);
                            }
                        }

                        // CU will be ignored!
                    }

                    foreach (DBLanguageSystem lngObj in lngObjs)
                    {
                        GameServer.Database.AddObject(lngObj);

                        if (log.IsWarnEnabled)
                            log.Warn("Moving sentence from 'language' to 'languagesystem'. ( Language <" + lngObj.Language +
                                     "> - TranslationId <" + lngObj.TranslationId + "> )");
                    }
                }
            }
        }

        private bool ListContainsObjectData(List<DBLanguageSystem> list, string language, string translationId)
        {
            bool contains = false;

            foreach (DBLanguageSystem lngObj in list)
            {
                if (lngObj.TranslationId != translationId)
                    continue;

                if (lngObj.Language != language)
                    continue;

                contains = true;
                break;
            }

            return contains;
        }
    }
}