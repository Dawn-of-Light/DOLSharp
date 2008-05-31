using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DOL.Database2;
/*
 
 * status:
 * Converted Tables:
 * Ability -> DBAbility
 * Area    -> DBArea
 * BindPoint-> BindPoint
 * ClassXRealmAbility -> ClassXRealmAbility
 * CraftedItem -> DBCraftedItem
 * CraftedXItem ->DBCraftedXItem
 */
namespace DOLStudio.Conversion
{
    class LegacyImporter
    {
        /// <summary>
        /// Handles a single XML Node
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="KeyMapping">The dictionary that keeps the IDs ( a.k.a. the context)</param>
        /// <returns>
        /// true: Object was imported
        /// false: Object was "orphaned"
        /// </returns>
        protected static bool HandleSingleNode(XmlNode node, Dictionary<string, UInt64> KeyMapping)
        {
            string name = node.LocalName.ToLower();
            if (name.EndsWith("_archive")) //these have to be redone ...
            {
                name = name.Replace("_archive", "");
            }
            switch (name) // faster than LINQ
                {
                    //These are hand-written so they may contain errors, but a lot of it is intellisense
                    // There is no need for comments here , they just imnport from XML
                    //Server Stats are not converted- to save memory and because old <-> new is not comparable
                    #region Database release tables (64) 
                    #region A-C ( 13)
                    #region Ability
                    case "ability":
                        if (KeyMapping.ContainsKey(node["Ability_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        DBAbility ability = DatabaseLayer.Instance.SelectObject(typeof(DBAbility), "KeyName", node["KeyName"]) as DBAbility;
                        if (ability != null)
                        {
                            KeyMapping.Add(node["Ability_ID"].InnerText, ability.ID);
                            return true;//TODO: Log duplicate
                        }
                        ability = new DBAbility();
                        if (node["Description"] != null)
                            ability.Description = node["Description"].InnerText;
                        ability.IconID = int.Parse(node["IconID"].InnerText);
                        ability.Name = node["Name"].InnerText;
                        ability.KeyName = node["KeyName"].InnerText;
                        KeyMapping.Add(node["Ability_ID"].InnerText, ability.ID);
                        ability.Save();
                        return true;
                    #endregion
                    #region Artifact
                    case "artifact": //TODO: Move back -it depends on ItemTempar
                        if (KeyMapping.ContainsKey(node["Artifact_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        if (!KeyMapping.ContainsKey(node["BookID"].InnerText))
                            return false;
                        if (!KeyMapping.ContainsKey(node["EncounterID"].InnerText))
                            return false;
                        if (!KeyMapping.ContainsKey(node["QuestID"].InnerText))
                            return false;
                        if (!KeyMapping.ContainsKey(node["ScholarID"].InnerText))
                            return false;
                        Artifact artifact = (Artifact)DatabaseLayer.Instance.SelectObject(typeof(Artifact), "ArtifactID", node["ArtifactID"].InnerText);
                        if (artifact != null)
                            return true;  //TODO: Log duplicate
                        artifact = new Artifact();
                        artifact.ArtifactID = node["ArtifactID"].InnerText;
                        artifact.BookID = node["BookID"].InnerText;
                        artifact.BookModel = int.Parse(node["BookModel"].InnerText);
                        artifact.EncounterID = node["EncounterID"].InnerText;
                        artifact.MessageCombineBook = node["MessageCombineBooks"].InnerText;
                        artifact.MessageCombineScrolls = node["MessageCombineScrolls"].InnerText;
                        artifact.MessageReceiveBook = node["MessageReceiveBook"].InnerText;
                        artifact.MessageReceiveScrolls = node["MessageReceiveScrolls"].InnerText;
                        artifact.MessageUse = node["MessageUse"].InnerText;
                        artifact.QuestID = node["QuestID"].InnerText;
                        artifact.ScholarID = node["ScholarID"].InnerText;
                        artifact.Scroll1 = node["Scroll1"].InnerText;
                        artifact.Scroll12 = node["Scroll12"].InnerText;
                        artifact.Scroll13 = node["Scroll13"].InnerText;
                        artifact.Scroll2 = node["Scroll2"].InnerText;
                        artifact.Scroll23 = node["Scroll23"].InnerText;
                        artifact.Scroll3 = node["Scroll3"].InnerText;
                        artifact.ScrollLevel = int.Parse(node["ScrollLevel"].InnerText);
                        artifact.ScrollModel1 = int.Parse(node["ScrollModel1"].InnerText);
                        artifact.ScrollModel2 = int.Parse(node["ScrollModel2"].InnerText);
                        artifact.XPRate = int.Parse(node["XPRate"].InnerText);
                        artifact.Zone = node["Zone"].InnerText; //TODO: Verify key 
                        KeyMapping.Add(node["Artifact_ID"].InnerText, artifact.ID);
                        artifact.FillObjectRelations();
                        artifact.Save();
                        break;
                    #endregion // depends on InventoryItem / maybe quest
                    #region ArtifactBonus
                    case "artifactbonus":
                        if (KeyMapping.ContainsKey(node["ArtifactBonus_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        ArtifactBonus artbonus = new ArtifactBonus();
                        artbonus.ArtifactID = node["ArtifactID"].InnerText; //TODO: Change that
                        artbonus.BonusID = int.Parse(node["BonusID"].InnerText);
                        artbonus.Level = int.Parse(node["Level"].InnerText);
                        artbonus.FillObjectRelations();
                        artbonus.Save();
                        KeyMapping.Add(node["ArtifactBonus_ID"].InnerText, artbonus.ID);
                        break;
                    #endregion
                    #region ArtifactXItem
                    case "artifactxitem":
                        if (KeyMapping.ContainsKey(node["ArtifactXItem_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        if (!KeyMapping.ContainsKey(node["ArtifactID"].InnerText))
                            return false;
                        if (DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), node["ItemIDNb"].InnerText) == null)
                            return false;
                        ArtifactXItem artifactxitem = new ArtifactXItem();
                        artifactxitem.ArtifactID = node["ArtifactID"].InnerText;
                        artifactxitem.ItemIDNb = node["ItemIDNb"].InnerText;
                        artifactxitem.Realm = int.Parse(node["Realm"].InnerText);
                        artifactxitem.Version = node["Version"].InnerText;
                        artifactxitem.FillObjectRelations();
                        artifactxitem.Save();
                        KeyMapping.Add(node["ArtifactXItem_ID"].InnerText, artifactxitem.ID);
                        break;
                    #endregion
                    #region Area
                    case "area":
                        if (KeyMapping.ContainsKey(node["Area_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        DBArea area = new DBArea();
                        area.Description = node["Description"].InnerText;
                        area.X = int.Parse(node["X"].InnerText);
                        area.Y = int.Parse(node["Y"].InnerText);
                        area.Z = int.Parse(node["Z"].InnerText);
                        area.Radius = int.Parse(node["Radius"].InnerText);
                        area.Region = ushort.Parse(node["Region"].InnerText);
                        area.ClassType = node["ClassType"].InnerText;
                        area.CanBroadcast = ushort.Parse(node["CanBroadcast"].InnerText) != 0;
                        area.Sound = byte.Parse(node["Sound"].InnerText);
                        area.CheckLOS = ushort.Parse(node["CheckLOS"].InnerText) != 0;
                        KeyMapping.Add(node["Area_ID"].InnerText, area.ID);
                        area.FillObjectRelations();
                        area.Save();
                        return true;
                    #endregion
                    #region Ban
                    case "ban":
                        if (KeyMapping.ContainsKey(node["Ban_ID"].InnerText))
                            return true;
                        DBBannedAccount ban = new DBBannedAccount();
                        ban.Account = node["Account"].InnerText;
                        ban.Author = node["Author"].InnerText;
                        ban.DateBan = DateTime.Parse(node["Author"].InnerText);
                        ban.Ip = node["Ip"].InnerText;
                        ban.Reason = node["Reason"].InnerText;
                        ban.Type = node["Type"].InnerText;
                        ban.FillObjectRelations();
                        ban.Save();
                        KeyMapping.Add(node["Ban_ID"].InnerText, ban.ID);
                        break;
                    #endregion
                    #region Battleground
                    case "battleground":
                        if (KeyMapping.ContainsKey(node["Battleground_ID"].InnerText))
                            return true; //TODO: Log duplicate
                        Battleground battleground = new Battleground();
                        battleground.MaxLevel = byte.Parse(node["MaxLevel"].InnerText);
                        battleground.MaxRealmLevel = byte.Parse(node["MaxRealmLevel"].InnerText);
                        battleground.MinLevel = byte.Parse(node["MinLevel"].InnerText);
                        battleground.RegionID = ushort.Parse(node["RegionID"].InnerText);
                        battleground.FillObjectRelations();
                        battleground.Save();
                        KeyMapping.Add(node["Battleground_ID"].InnerText, battleground.ID);
                        break;
                    #endregion
                    #region BindPoint
                    case "bindpoint":
                        if (KeyMapping.ContainsKey(node["BindPoint_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        BindPoint bp = new BindPoint();
                        bp.X = int.Parse(node["X"].InnerText);
                        bp.Y = int.Parse(node["Y"].InnerText);
                        bp.Z = int.Parse(node["Z"].InnerText);
                        bp.Radius = ushort.Parse(node["Radius"].InnerText);
                        if (node["Realm"] != null)
                            bp.Realm = int.Parse(node["Realm"].InnerText);
                        bp.Region = int.Parse(node["Region"].InnerText);
                        KeyMapping.Add(node["BindPoint_ID"].InnerText, bp.ID);
                        bp.Save();
                        return true;
                    #endregion
                    #region ClassXRealmAbility
                    case "classxrealmability":
                        if (KeyMapping.ContainsKey(node["ClassXRealmAbility_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        ClassXRealmAbility obj = new ClassXRealmAbility();
                        obj.AbilityKey = node["AbilityKey"].InnerText;
                        obj.CharClass = int.Parse(node["CharClass"].InnerText);
                        KeyMapping.Add(node["ClassXRealmAbility_ID"].InnerText, obj.ID);
                        obj.Save();
                        return true;
                    #endregion
                    #region ChampSpecs
                    case "champspecs":
                        if (KeyMapping.ContainsKey(node["ChampSpecs_ID"].InnerText))
                            return true;
                        DBChampSpecs champspecs = new DBChampSpecs();
                        champspecs.Cost = int.Parse(node["Cost"].InnerText);
                        champspecs.IdLine = int.Parse(node["IdLine"].InnerText);
                        champspecs.Index = int.Parse(node["Index"].InnerText);
                        champspecs.SkillIndex = int.Parse(node["SkillIndex"].InnerText);
                        champspecs.SpellID = int.Parse(node["SpellID"].InnerText);
                        champspecs.FillObjectRelations();
                        champspecs.Save();
                        KeyMapping.Add(node["ChampSpecs_ID"].InnerText, champspecs.ID);
                        break;
                    #endregion
                    #region CharacterXMasterLevel
                    case "characterxmasterlevel":
                        if (KeyMapping.ContainsKey(node["CharacterXMasterLevel_ID"].InnerText))
                            return true;
                        DBCharacterXMasterLevel charxml = new DBCharacterXMasterLevel();
                        charxml.CharName = node["CharName"].InnerText;
                        charxml.MLLevel = int.Parse(node["MLLevel"].InnerText);
                        charxml.MLStep = int.Parse(node["MLStep"].InnerText);
                        charxml.StepCompleted = ushort.Parse(node["StepCompleted"].InnerText) != 0;
                        charxml.ValidationDate = DateTime.Parse(node["ValidationDate"].InnerText);
                        charxml.FillObjectRelations();
                        charxml.Save();
                        KeyMapping.Add(node["CharacterXMasterLevel_ID"].InnerText, charxml.ID);
                        break;
                    #endregion

                    #region CraftedItem
                    case "crafteditem":
                        if (KeyMapping.ContainsKey(node["CraftedItem_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        DBCraftedItem crafteditem = DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(DBCraftedItem), node["Id_nb"].InnerText) as DBCraftedItem;// DatabaseLayer.Instance.SelectObject(typeof(DBHouse), "HouseNumber", node["HouseNumber"]);

                        if (crafteditem != null)
                        {
                            KeyMapping.Add(node["CraftedItem_ID"].InnerText, crafteditem.ID);
                            return true;
                        }
                        crafteditem = new DBCraftedItem();
                        crafteditem.CraftedItemID = node["CraftedItemID"].InnerText;
                        crafteditem.Id_nb = node["Id_nb"].InnerText;
                        if (node["CraftingLevel"] != null)
                            crafteditem.CraftingLevel = int.Parse(node["CraftingLevel"].InnerText);
                        if (node["CraftingSkillType"] != null)
                            crafteditem.CraftingSkillType = int.Parse(node["CraftingSkillType"].InnerText);
                        KeyMapping.Add(node["CraftedItem_ID"].InnerText, crafteditem.ID);
                        crafteditem.Save();
                        return true;
                    #endregion
                    #region CraftedXItem
                    case "craftedxitem":
                        if (KeyMapping.ContainsKey(node["CraftedXItem_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        DBCraftedXItem craftedxitem = new DBCraftedXItem();
                        craftedxitem.CraftedItemId_nb = node["CraftedItemId_nb"].InnerText;
                        craftedxitem.IngredientId_nb = node["IngredientId_nb"].InnerText;
                        craftedxitem.Count = int.Parse(node["Count"].InnerText);
                        KeyMapping.Add(node["CraftedXItem_ID"].InnerText, craftedxitem.ID);
                        craftedxitem.Save();
                        return true;
                    #endregion
                    #endregion
                    #region D-G ( 10 )
                    #region DBHouse
                    case "dbhouse":
                        if (KeyMapping.ContainsKey(node["DBHouse_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        if (node["OwnerIDs"] != null)
                        {
                            foreach (string s in node["OwnerIDs"].InnerText.Split(';'))
                            {
                                if (!KeyMapping.ContainsKey(s))
                                    return false;
                            }
                        }
                        DBHouse house = DatabaseLayer.Instance.SelectObject(typeof(DBHouse), "HouseNumber", node["HouseNumber"].InnerText) as DBHouse;
                        if (house != null)
                        {
                            KeyMapping.Add(node["DBHouse_ID"].InnerText, house.ID);
                            return true;
                        }
                        house = new DBHouse();
                        house.CreationTime = DateTime.Parse(node["CreationTime"].InnerText);
                        if (node["DoorMaterial"] != null)
                            house.DoorMaterial = int.Parse(node["DoorMaterial"].InnerText);
                        if (node["Emblem"] != null)
                            house.Emblem = int.Parse(node["Emblem"].InnerText);
                        if (node["GuildHouse"] != null)
                            house.GuildHouse = ushort.Parse(node["GuildHouse"].InnerText) != 0;
                        if (node["GuildName"] != null)
                            house.GuildName = node["GuildName"].InnerText;
                        if (node["Heading"] != null)
                            house.Heading = int.Parse(node["Heading"].InnerText);
                        house.HouseNumber = int.Parse(node["HouseNumber"].InnerText);
                        if (node["IndoorGuildBanner"] != null)
                            house.IndoorGuildBanner = ushort.Parse(node["IndoorGuildBanner"].InnerText) != 0;
                        if (node["IndoorGuildShield"] != null)
                            house.IndoorGuildShield = ushort.Parse(node["IndoorGuildShield"].InnerText) != 0;
                        if (node["KeptMoney"] != null)
                            house.KeptMoney = long.Parse(node["KeptMoney"].InnerText);
                        if (node["LastPaid"] != null)
                            house.LastPaid = DateTime.Parse(node["LastPaid"].InnerText);
                        if (node["Model"] != null)
                            house.Model = int.Parse(node["Model"].InnerText);
                        if (node["Name"] != null)
                            house.Name = node["Name"].InnerText;
                        if (node["NoPurge"] != null)
                            house.NoPurge = ushort.Parse(node["NoPurge"].InnerText) != 0;
                        if (node["OutdoorGuildBanner"] != null)
                            house.OutdoorGuildBanner = ushort.Parse(node["OutdoorGuildBanner"].InnerText) != 0;
                        if (node["OutdoorGuildShield"] != null)
                            house.OutdoorGuildShield = ushort.Parse(node["OutdoorGuildShield"].InnerText) != 0;
                        if (node["OwnerIDs"] != null)
                            foreach (string s in node["OwnerIDs"].InnerText.Split(';'))
                            {
                                house.OwnerIDs.Add(KeyMapping[s]);
                            }
                        if (node["Porch"] != null)
                            house.Porch = ushort.Parse(node["Porch"].InnerText) != 0;
                        if (node["PorchMaterial"] != null)
                            house.PorchMaterial = int.Parse(node["PorchMaterial"].InnerText);
                        if (node["PorchRoofColor"] != null)
                            house.PorchRoofColor = int.Parse(node["PorchRoofColor"].InnerText);
                        house.RegionID = ushort.Parse(node["RegionID"].InnerText);
                        if (node["RoofMaterial"] != null)
                            house.RoofMaterial = int.Parse(node["RoofMaterial"].InnerText);
                        if (node["Rug1Color"] != null)
                            house.Rug1Color = int.Parse(node["Rug1Color"].InnerText);
                        if (node["Rug2Color"] != null)
                            house.Rug2Color = int.Parse(node["Rug2Color"].InnerText);
                        if (node["Rug3Color"] != null)
                            house.Rug3Color = int.Parse(node["Rug3Color"].InnerText);
                        if (node["Rug4Color"] != null)
                            house.Rug4Color = int.Parse(node["Rug4Color"].InnerText);
                        if (node["TrussMaterial"] != null)
                            house.TrussMaterial = int.Parse(node["TrussMaterial"].InnerText);
                        if (node["WallMaterial"] != null)
                            house.WallMaterial = int.Parse(node["WallMaterial"].InnerText);
                        if (node["WindowMaterial"] != null)
                            house.WindowMaterial = int.Parse(node["WindowMaterial"].InnerText);
                        house.X = int.Parse(node["X"].InnerText);
                        house.Y = int.Parse(node["Y"].InnerText);
                        house.Z = int.Parse(node["Z"].InnerText);
                        KeyMapping.Add(node["DBHouse_ID"].InnerText, house.ID);
                        house.FillObjectRelations();
                        house.Save();
                        return true;
                    #endregion
                    #region DHouseCharsXPerms
                    case "dbhousecharsxperms":
                        if (KeyMapping.ContainsKey(node["DBHouseCharsXPerms"].InnerText))
                            return true;
                        DBHouseCharsXPerms charsxperms = new DBHouseCharsXPerms();
                        charsxperms.HouseNumber = int.Parse(node["HouseNumber"].InnerText);
                        charsxperms.Name = node["Name"].InnerText;
                        charsxperms.PermLevel = int.Parse(node["PermLevel"].InnerText);
                        charsxperms.Slot = int.Parse(node["Slot"].InnerText);
                        charsxperms.Type = byte.Parse(node["Type"].InnerText);
                        charsxperms.FillObjectRelations();
                        charsxperms.Save();
                        KeyMapping.Add(node["DBHouseCharsXPerms_ID"].InnerText, charsxperms.ID);
                        break;
                    #endregion
                    #region DBHousePermissions
                    case "dbhousepermissions":
                        if (KeyMapping.ContainsKey(node["DBHousePermissions_ID"].InnerText))
                            return true;
                        DBHousePermissions permissions = new DBHousePermissions();
                        permissions.Appearance = byte.Parse(node["Appearance"].InnerText);
                        permissions.Banish = byte.Parse(node["Banish"].InnerText);
                        permissions.Bind = byte.Parse(node["Bind"].InnerText);
                        permissions.Enter = byte.Parse(node["Enter"].InnerText);
                        permissions.Garden = byte.Parse(node["Garden"].InnerText);
                        permissions.HouseNumber = int.Parse(node["HouseNumber"].InnerText);
                        permissions.Interior = byte.Parse(node["Interior"].InnerText);
                        permissions.Merchant = byte.Parse(node["Merchant"].InnerText);
                        permissions.PayRent = byte.Parse(node["PayRent"].InnerText);
                        permissions.PermLevel = int.Parse(node["PermLevel"].InnerText);
                        permissions.Tools = byte.Parse(node["Tools"].InnerText);
                        permissions.UseMerchant = byte.Parse(node["UseMerchant"].InnerText);
                        permissions.Vault1 = byte.Parse(node["Vault1"].InnerText);
                        permissions.Vault2 = byte.Parse(node["Vault2"].InnerText);
                        permissions.Vault3 = byte.Parse(node["Vault3"].InnerText);
                        permissions.Vault4 = byte.Parse(node["Vault4"].InnerText);
                        permissions.FillObjectRelations();
                        permissions.Save();
                        KeyMapping.Add(node["DBHousePermissions_ID"].InnerText, permissions.ID);
                        break;
                    #endregion
                    #region DBIndoorItem
                    case "dbindooritem":
                        if (KeyMapping.ContainsKey(node["DBIndoorItem_ID"].InnerText))
                            return true;
                        DBHouseIndoorItem indoor = new DBHouseIndoorItem();
                        indoor.BaseItemID = node["BaseItemID"].InnerText;
                        indoor.Color = int.Parse(node["Color"].InnerText);
                        indoor.HouseNumber = int.Parse(node["HouseNumber"].InnerText);
                        indoor.Model = int.Parse(node["Model"].InnerText);
                        indoor.Placemode = int.Parse(node["Placemode"].InnerText);
                        indoor.Position = int.Parse(node["Position"].InnerText);
                        indoor.Rotation = int.Parse(node["Rotation"].InnerText);
                        indoor.Size = int.Parse(node["Size"].InnerText);
                        indoor.X = int.Parse(node["X"].InnerText);
                        indoor.Y = int.Parse(node["Y"].InnerText);
                        indoor.FillObjectRelations();
                        indoor.Save();
                        KeyMapping.Add(node["DBIndoorItem_ID"].InnerText, indoor.ID);
                        break;
                    #endregion
                    #region DBOutdoorItem
                    case "dboutdooritem":
                        if (KeyMapping.ContainsKey(node["DBOutdoorItem_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["BaseItemID"].InnerText))
                            return false;
                        DBHouseOutdoorItem outdoor = new DBHouseOutdoorItem();
                        outdoor.BaseItemID = KeyMapping[node["BaseItemID"].InnerText];
                        outdoor.HouseNumber = int.Parse(node["HouseNumber"].InnerText);
                        outdoor.Model = int.Parse(node["Model"].InnerText);
                        outdoor.Position = int.Parse(node["Position"].InnerText);
                        outdoor.Rotation = int.Parse(node["Rotation"].InnerText);
                        outdoor.FillObjectRelations();
                        outdoor.Save();
                        KeyMapping.Add(node["DBOutdoorItem_ID"].InnerText, outdoor.ID);
                        break;
                    #endregion
                    #region Door
                    case "door":
                        if (KeyMapping.ContainsKey(node["Door_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        DBDoor door = new DBDoor();
                        door.Name = node["Name"].InnerText;
                        door.X = int.Parse(node["X"].InnerText);
                        door.Y = int.Parse(node["Y"].InnerText);
                        door.Z = int.Parse(node["Z"].InnerText);
                        door.Heading = int.Parse(node["Heading"].InnerText);
                        door.DoorID = int.Parse(node["InternalID"].InnerText);
                        KeyMapping.Add(node["Door_ID"].InnerText, door.ID);
                        door.Save();
                        return true;
                    #endregion
                    #region Faction
                    case "faction":
                        if (KeyMapping.ContainsKey(node["Faction_ID"].InnerText))
                            return true;
                        DBFaction faction = new DBFaction();
                        faction.BaseAggroLevel = int.Parse(node["BaseAggroLevel"].InnerText);
                        faction.Name = node["Name"].InnerText;
                        faction.Index = int.Parse(node["ID"].InnerText);
                        KeyMapping.Add(node["Faction_ID"].InnerText, faction.ID);
                        break;
                    #endregion
                    #region FactionAggroLevel
                    case "factionaggrolevel":
                        if (KeyMapping.ContainsKey(node["FactionAggroLevel_ID"].InnerText))
                            return true;
                        DBFactionAggroLevel aggro = new DBFactionAggroLevel();
                        aggro.AggroLevel = int.Parse(node["AggroLevel"].InnerText);
                        aggro.CharacterID = node["CharacterID"].InnerText;
                        aggro.FactionID = int.Parse(node["FactionID"].InnerText);
                        aggro.FillObjectRelations();
                        aggro.Save();
                        KeyMapping.Add(node["FactionAggroLevel_ID"].InnerText, aggro.ID);
                        break;
                    #endregion
                    #region Guild
                    case "guild":
                        if (KeyMapping.ContainsKey(node["Guild_ID"].InnerText))
                            return true;//TODO: Log duplicate                        
                        if (!KeyMapping.ContainsKey(node["AllianceID"].InnerText))
                            return false; //Wait for the alliance to appear later on
                        DBGuild guild = new DBGuild();
                        guild.AllianceID = KeyMapping[node["AllianceID"].InnerText];
                        guild.Bank = double.Parse(node["Bank"].InnerText);
                        guild.BountyPoints = long.Parse(node["BountyPoints"].InnerText);
                        guild.BuffTime = DateTime.Parse(node["BuffTime"].InnerText);
                        guild.BuffType = long.Parse(node["BuffType"].InnerText);
                        guild.Dues = ushort.Parse(node["Dues"].InnerText) != 0;
                        guild.DuesPercent = long.Parse(node["DuesPercent"].InnerText);
                        guild.Email = node["Email"].InnerText;
                        guild.Emblem = int.Parse(node["Emblem"].InnerText);
                        guild.GuildBanner = ushort.Parse(node["GuildBanner"].InnerText) != 0;
                        guild.GuildHouseNumber = int.Parse(node["GuildHouseNumber"].InnerText);
                        guild.GuildLevel = long.Parse(node["GuildLevel"].InnerText);
                        guild.GuildName = node["GuildName"].InnerText;
                        guild.HaveGuildHouse = ushort.Parse(node["HaveGuildHouse"].InnerText) != 0;
                        guild.MeritPoints = long.Parse(node["MeritPoints"].InnerText);
                        guild.Motd = node["Motd"].InnerText;
                        guild.oMotd = node["oMotd"].InnerText;
                        guild.RealmPoints = long.Parse(node["RealmPoints"].InnerText);
                        guild.Webpage = node["Webpage"].InnerText;
                        guild.FillObjectRelations();
                        guild.Save();
                        KeyMapping.Add(node["Guild_ID"].InnerText, guild.ID);
                        return true;
                    #endregion
                    #region GuildRank
                    case "guildrank":
                        if (KeyMapping.ContainsKey(node["GuildRank_ID"].InnerText))
                            return true;//TODO: Log duplicate                        
                        if (!KeyMapping.ContainsKey(node["GuildID"].InnerText))
                            return false; //Wait for the alliance to appear later on
                        DBRank rank = new DBRank();
                        rank.AcHear = ushort.Parse(node["AcHear"].InnerText) != 0;
                        rank.AcSpeak = ushort.Parse(node["AcSpeak"].InnerText) != 0;
                        rank.Alli = ushort.Parse(node["Alli"].InnerText) != 0;
                        rank.Buff = ushort.Parse(node["Buff"].InnerText) != 0;
                        rank.Claim = ushort.Parse(node["Claim"].InnerText) != 0;
                        rank.Dues = ushort.Parse(node["Dues"].InnerText) != 0;
                        rank.Emblem = ushort.Parse(node["Emblem"].InnerText) != 0;
                        rank.GcHear = ushort.Parse(node["GcHear"].InnerText) != 0;
                        rank.GcSpeak = ushort.Parse(node["GcSpeak"].InnerText) != 0;
                        rank.guildID = KeyMapping[node["GuildID"].InnerText];
                        rank.Invite = ushort.Parse(node["Invite"].InnerText) != 0;
                        rank.OcHear = ushort.Parse(node["OcHear"].InnerText) != 0;
                        rank.OcSpeak = ushort.Parse(node["OcSpeak"].InnerText) != 0;
                        rank.Promote = ushort.Parse(node["Promote"].InnerText) != 0;
                        rank.RankLevel = byte.Parse(node["RankLevel"].InnerText);
                        rank.Release = ushort.Parse(node["Release"].InnerText) != 0;
                        rank.Remove = ushort.Parse(node["Remove"].InnerText) != 0;
                        rank.Title = node["Title"].InnerText;
                        rank.Upgrade = ushort.Parse(node["Upgrade"].InnerText) != 0;
                        rank.View = ushort.Parse(node["View"].InnerText) != 0;
                        rank.Withdraw = ushort.Parse(node["Withdraw"].InnerText) != 0;
                        rank.FillObjectRelations();
                        rank.Save();
                        KeyMapping.Add(node["GuildRank_ID"].InnerText, rank.ID);
                        break;
                    #endregion
                    #endregion
                    #region H-L (13)
                    #region HousepointItem
                    case "housepointitem":
                        if (KeyMapping.ContainsKey(node["HousepointItem_ID"].InnerText))
                            return true;
                        DBHousepointItem hpitem = new DBHousepointItem();
                        hpitem.Heading = ushort.Parse(node["Heading"].InnerText);
                        hpitem.HouseID = int.Parse(node["HouseID"].InnerText);
                        hpitem.Index = byte.Parse(node["Index"].InnerText);
                        hpitem.ItemTemplateID = node["ItemTemplateID"].InnerText;
                        hpitem.Position = uint.Parse(node["Position"].InnerText);
                        hpitem.FillObjectRelations();
                        hpitem.Save();
                        KeyMapping.Add(node["HousepointItem_ID"].InnerText, hpitem.ID);
                        break;
                    #endregion
                    #region InventoryItem
                    case "inventoryitem":
                        InventoryItem invitem = (InventoryItem)DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(InventoryItem), node["Id_nb"].InnerText);
                        if (KeyMapping.ContainsKey(node["InventoryItem_ID"].InnerText))
                            return true; //TODO: Log duplicate
                        if (!KeyMapping.ContainsKey(node["OwnerID"].InnerText))
                            return false; //Wait for the owner to appear later on


                        if (invitem != null)
                        {
                            KeyMapping.Add(node["InventoryItem_ID"].InnerText, invitem.ID);
                            return true; //TODO: Log IDNB duplicate
                        }
                        invitem = new InventoryItem();
                        KeyMapping.Add(node["InventoryItem_ID"].InnerText, invitem.ID);
                        invitem.AllowedClasses = node["AllowedClasses"].InnerText;
                        invitem.Bonus = int.Parse(node["Bonus"].InnerText);
                        invitem.Bonus1 = int.Parse(node["Bonus1"].InnerText);
                        invitem.Bonus10 = int.Parse(node["Bonus10"].InnerText);
                        invitem.Bonus10Type = int.Parse(node["Bonus10Type"].InnerText);
                        invitem.Bonus1Type = int.Parse(node["Bonus1Type"].InnerText);
                        invitem.Bonus2 = int.Parse(node["Bonus2"].InnerText);
                        invitem.Bonus2Type = int.Parse(node["Bonus2Type"].InnerText);
                        invitem.Bonus3 = int.Parse(node["Bonus3Type"].InnerText);
                        invitem.Bonus3Type = int.Parse(node["Bonus3Type"].InnerText);
                        invitem.Bonus4 = int.Parse(node["Bonus4"].InnerText);
                        invitem.Bonus4Type = int.Parse(node["Bonus4Type"].InnerText);
                        invitem.Bonus5 = int.Parse(node["Bonus5"].InnerText);
                        invitem.Bonus5Type = int.Parse(node["Bonus5Type"].InnerText);
                        invitem.Bonus6 = int.Parse(node["Bonus6"].InnerText);
                        invitem.Bonus6Type = int.Parse(node["Bonus6Type"].InnerText);
                        invitem.Bonus7 = int.Parse(node["Bonus7"].InnerText);
                        invitem.Bonus7Type = int.Parse(node["Bonus7Type"].InnerText);
                        invitem.Bonus8 = int.Parse(node["Bonus8"].InnerText);
                        invitem.Bonus8Type = int.Parse(node["Bonus8Type"].InnerText);
                        invitem.Bonus9 = int.Parse(node["Bonus9"].InnerText);
                        invitem.Bonus9Type = int.Parse(node["Bonus9Type"].InnerText);
                        invitem.CanDropAsLoot = ushort.Parse(node["CanDropAsLoot"].InnerText) != 0;
                        invitem.CanUseAgainIn = int.Parse(node["CanUseAgainIn"].InnerText);
                        invitem.CanUseEvery = int.Parse(node["CanUseEvery"].InnerText);
                        invitem.Charges = int.Parse(node["Charges"].InnerText);
                        invitem.Charges1 = int.Parse(node["Charges1"].InnerText);
                        invitem.Color = int.Parse(node["Color"].InnerText);
                        invitem.Condition = int.Parse(node["Condition"].InnerText);
                        //invitem.ConditionPercent = byte.Parse(node["ConditionPercent"].InnerText);
                        invitem.Cooldown = int.Parse(node["Cooldown"].InnerText);
                        invitem.Copper = byte.Parse(node["Copper"].InnerText);
                        invitem.Count = int.Parse(node["Count"].InnerText);
                        invitem.CrafterName = node["CrafterName"].InnerText;
                        invitem.DPS_AF = int.Parse(node["DPS_AF"].InnerText);
                        invitem.Durability = int.Parse(node["Durability"].InnerText);
                        //invitem.DurabilityPercent = byte.Parse(node["DurabilityPercent"].InnerText);
                        invitem.Effect = int.Parse(node["Effect"].InnerText);
                        invitem.Emblem = int.Parse(node["Emblem"].InnerText);
                        invitem.Experience = long.Parse(node["Experience"].InnerText);
                        invitem.Extension = byte.Parse(node["Extension"].InnerText);
                        invitem.ExtraBonus = int.Parse(node["ExtraBonus"].InnerText);
                        invitem.ExtraBonusType = int.Parse(node["ExtraBonusType"].InnerText);
                        invitem.Gold = short.Parse(node["Gold"].InnerText);
                        invitem.Hand = int.Parse(node["Hand"].InnerText);
                        invitem.Id_nb = node["Id_nb"].InnerText;
                        invitem.IsDropable = ushort.Parse(node["IsDropable"].InnerText) != 0;
                        //invitem.IsMagical = ushort.Parse(node["IsMagical"].InnerText) != 0;
                        invitem.IsPickable = ushort.Parse(node["IsPickable"].InnerText) != 0;
                        //invitem.IsStackable = ushort.Parse(node["IsStackable"].InnerText) != 0;
                        invitem.IsTradable = ushort.Parse(node["IsTradable"].InnerText) != 0;
                        invitem.Item_Type = int.Parse(node["Item_Type"].InnerText);
                        invitem.Level = int.Parse(node["Level"].InnerText);
                        invitem.MaxCharges = int.Parse(node["MaxCharges"].InnerText);
                        invitem.MaxCharges1 = int.Parse(node["MaCharges1"].InnerText);
                        invitem.MaxCondition = int.Parse(node["MaxCondition"].InnerText);
                        invitem.MaxCount = int.Parse(node["MaxCount"].InnerText);
                        invitem.MaxDurability = int.Parse(node["MaxDurability"].InnerText);
                        invitem.Model = int.Parse(node["Model"].InnerText);
                        invitem.Name = node["Name"].InnerText;
                        invitem.Object_Type = int.Parse(node["Object_Type"].InnerText);
                        invitem.OwnerID = KeyMapping[node["OwnerID"].InnerText];
                        invitem.PackSize = int.Parse(node["PackSize"].InnerText);
                        invitem.Platinum = short.Parse(node["Platinum"].InnerText);
                        invitem.PoisonCharges = int.Parse(node["PoisonCharges"].InnerText);
                        invitem.PoisonMaxCharges = int.Parse(node["PoisonMaxCharges"].InnerText);
                        invitem.PoisonSpellID = int.Parse(node["PoisonSpellID"].InnerText);
                        invitem.ProcSpellID = int.Parse(node["ProcSpellID"].InnerText);
                        invitem.ProcSpellID1 = int.Parse(node["ProcSpellID1"].InnerText);
                        invitem.Quality = int.Parse(node["Quality"].InnerText);
                        invitem.Realm = int.Parse(node["Realm"].InnerText);
                        invitem.SellPrice = int.Parse(node["SellPrice"].InnerText);
                        invitem.Silver = byte.Parse(node["Silver"].InnerText);
                        invitem.SlotPosition = int.Parse(node["SlotPosition"].InnerText);
                        invitem.SPD_ABS = int.Parse(node["SPD_ABS"].InnerText);
                        invitem.SpellID = int.Parse(node["SpellID"].InnerText);
                        invitem.SpellID1 = int.Parse(node["SpellID1"].InnerText);
                        invitem.Type_Damage = int.Parse(node["Type_Damage"].InnerText);
                        //invitem.Value =  long.Parse(node["Value"].InnerText);
                        invitem.Weight = int.Parse(node["Weight"].InnerText);
                        invitem.FillObjectRelations();
                        invitem.Save();
                        break;
                    #endregion
                    #region ItemTemplate
                    case "itemtemplate":
                        ItemTemplate itemtemplate = (ItemTemplate)DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), node["Id_nb"].InnerText);
                        if (KeyMapping.ContainsKey(node["ItemTemplate_ID"].InnerText))
                            return true; //TODO: Log duplicate


                        if (itemtemplate != null)
                        {
                            KeyMapping.Add(node["ItemTemplate_ID"].InnerText, itemtemplate.ID);
                            return true; //TODO: Log IDNB duplicate
                        }
                        itemtemplate = new ItemTemplate();
                        KeyMapping.Add(node["ItemTemplate_ID"].InnerText, itemtemplate.ID);
                        itemtemplate.AllowedClasses = node["AllowedClasses"].InnerText;
                        itemtemplate.Bonus = int.Parse(node["Bonus"].InnerText);
                        itemtemplate.Bonus1 = int.Parse(node["Bonus1"].InnerText);
                        itemtemplate.Bonus10 = int.Parse(node["Bonus10"].InnerText);
                        itemtemplate.Bonus10Type = int.Parse(node["Bonus10Type"].InnerText);
                        itemtemplate.Bonus1Type = int.Parse(node["Bonus1Type"].InnerText);
                        itemtemplate.Bonus2 = int.Parse(node["Bonus2"].InnerText);
                        itemtemplate.Bonus2Type = int.Parse(node["Bonus2Type"].InnerText);
                        itemtemplate.Bonus3 = int.Parse(node["Bonus3Type"].InnerText);
                        itemtemplate.Bonus3Type = int.Parse(node["Bonus3Type"].InnerText);
                        itemtemplate.Bonus4 = int.Parse(node["Bonus4"].InnerText);
                        itemtemplate.Bonus4Type = int.Parse(node["Bonus4Type"].InnerText);
                        itemtemplate.Bonus5 = int.Parse(node["Bonus5"].InnerText);
                        itemtemplate.Bonus5Type = int.Parse(node["Bonus5Type"].InnerText);
                        itemtemplate.Bonus6 = int.Parse(node["Bonus6"].InnerText);
                        itemtemplate.Bonus6Type = int.Parse(node["Bonus6Type"].InnerText);
                        itemtemplate.Bonus7 = int.Parse(node["Bonus7"].InnerText);
                        itemtemplate.Bonus7Type = int.Parse(node["Bonus7Type"].InnerText);
                        itemtemplate.Bonus8 = int.Parse(node["Bonus8"].InnerText);
                        itemtemplate.Bonus8Type = int.Parse(node["Bonus8Type"].InnerText);
                        itemtemplate.Bonus9 = int.Parse(node["Bonus9"].InnerText);
                        itemtemplate.Bonus9Type = int.Parse(node["Bonus9Type"].InnerText);
                        itemtemplate.CanDropAsLoot = ushort.Parse(node["CanDropAsLoot"].InnerText) != 0;
                        itemtemplate.CanUseEvery = int.Parse(node["CanUseEvery"].InnerText);
                        itemtemplate.Charges = int.Parse(node["Charges"].InnerText);
                        itemtemplate.Charges1 = int.Parse(node["Charges1"].InnerText);
                        itemtemplate.Color = int.Parse(node["Color"].InnerText);
                        itemtemplate.Condition = int.Parse(node["Condition"].InnerText);
                        //itemtemplate.ConditionPercent = byte.Parse(node["ConditionPercent"].InnerText);
                        itemtemplate.Copper = byte.Parse(node["Copper"].InnerText);
                        itemtemplate.DPS_AF = int.Parse(node["DPS_AF"].InnerText);
                        itemtemplate.Durability = int.Parse(node["Durability"].InnerText);
                        //itemtemplate.DurabilityPercent = byte.Parse(node["DurabilityPercent"].InnerText);
                        itemtemplate.Effect = int.Parse(node["Effect"].InnerText);
                        itemtemplate.Emblem = int.Parse(node["Emblem"].InnerText);
                        itemtemplate.Extension = byte.Parse(node["Extension"].InnerText);
                        itemtemplate.ExtraBonus = int.Parse(node["ExtraBonus"].InnerText);
                        itemtemplate.ExtraBonusType = int.Parse(node["ExtraBonusType"].InnerText);
                        itemtemplate.Gold = short.Parse(node["Gold"].InnerText);
                        itemtemplate.Hand = int.Parse(node["Hand"].InnerText);
                        itemtemplate.Id_nb = node["Id_nb"].InnerText;
                        itemtemplate.IsDropable = ushort.Parse(node["IsDropable"].InnerText) != 0;
                        //itemtemplate.IsMagical = ushort.Parse(node["IsMagical"].InnerText) != 0;
                        itemtemplate.IsPickable = ushort.Parse(node["IsPickable"].InnerText) != 0;
                        //itemtemplate.IsStackable = ushort.Parse(node["IsStackable"].InnerText) != 0;
                        itemtemplate.IsTradable = ushort.Parse(node["IsTradable"].InnerText) != 0;
                        itemtemplate.Item_Type = int.Parse(node["Item_Type"].InnerText);
                        itemtemplate.Level = int.Parse(node["Level"].InnerText);
                        itemtemplate.MaxCharges = int.Parse(node["MaxCharges"].InnerText);
                        itemtemplate.MaxCharges1 = int.Parse(node["MaCharges1"].InnerText);
                        itemtemplate.MaxCondition = int.Parse(node["MaxCondition"].InnerText);
                        itemtemplate.MaxCount = int.Parse(node["MaxCount"].InnerText);
                        itemtemplate.MaxDurability = int.Parse(node["MaxDurability"].InnerText);
                        itemtemplate.Model = int.Parse(node["Model"].InnerText);
                        itemtemplate.Name = node["Name"].InnerText;
                        itemtemplate.Object_Type = int.Parse(node["Object_Type"].InnerText);
                        itemtemplate.PackSize = int.Parse(node["PackSize"].InnerText);
                        itemtemplate.Platinum = short.Parse(node["Platinum"].InnerText);
                        itemtemplate.PoisonCharges = int.Parse(node["PoisonCharges"].InnerText);
                        itemtemplate.PoisonMaxCharges = int.Parse(node["PoisonMaxCharges"].InnerText);
                        itemtemplate.PoisonSpellID = int.Parse(node["PoisonSpellID"].InnerText);
                        itemtemplate.ProcSpellID = int.Parse(node["ProcSpellID"].InnerText);
                        itemtemplate.ProcSpellID1 = int.Parse(node["ProcSpellID1"].InnerText);
                        itemtemplate.Quality = int.Parse(node["Quality"].InnerText);
                        itemtemplate.Realm = int.Parse(node["Realm"].InnerText);
                        itemtemplate.Silver = byte.Parse(node["Silver"].InnerText);
                        itemtemplate.SPD_ABS = int.Parse(node["SPD_ABS"].InnerText);
                        itemtemplate.SpellID = int.Parse(node["SpellID"].InnerText);
                        itemtemplate.SpellID1 = int.Parse(node["SpellID1"].InnerText);
                        itemtemplate.Type_Damage = int.Parse(node["Type_Damage"].InnerText);
                        //itemtemplate.Value =  long.Parse(node["Value"].InnerText);
                        itemtemplate.Weight = int.Parse(node["Weight"].InnerText);
                        itemtemplate.FillObjectRelations();
                        itemtemplate.Save();
                        break;
                    #endregion


                    #region LineXSpell
                    case "linexspell":
                        if (KeyMapping.ContainsKey(node["LineXSpell_ID"].InnerText))
                            return true;
                        DBLineXSpell linexspell = new DBLineXSpell();
                        linexspell.Level = int.Parse(node["Level"].InnerText);
                        linexspell.LineName = node["LineName"].InnerText;
                        linexspell.SpellID = int.Parse(node["SpellID"].InnerText);
                        linexspell.FillObjectRelations();
                        linexspell.Save();
                        KeyMapping.Add(node["LineXSpell_ID"].InnerText, linexspell.ID);
                        break;
                    #endregion
                    #region Keep
                    case "Keep":
                        if (KeyMapping.ContainsKey(node["Keep_ID"].InnerText))
                            return true;
                        DBKeep keep = new DBKeep();
                        keep.AlbionDifficultyLevel = int.Parse(node["AlbionDifficultyLevel"].InnerText);
                        keep.BaseLevel = byte.Parse(node["BaseLevel"].InnerText);
                        keep.ClaimedGuildName = node["ClaimedGuildName"].InnerText;
                        keep.Heading = ushort.Parse(node["Heading"]);
                        keep.HiberniaDifficultyLevel = int.Parse(node["HiberniaDifficultyLevel"]);
                        keep.KeepID = int.Parse(node["KeepID"].InnerText);
                        keep.KeepType = int.Parse(node["KeepType"].InnerText);
                        keep.Level = byte.Parse(node["Level"].InnerText);
                        keep.MidgardDifficultyLevel = int.Parse(node["MidgardDifficultyLevel"].InnerText);
                        keep.Name = node["Name"].InnerText;
                        keep.OriginalRealm = int.Parse(node["OriginalName"].InnerText);
                        keep.Realm = byte.Parse(node["Realm"].InnerText);
                        keep.Region = ushort.Parse(node["Region"].InnerText);

                        keep.X = int.Parse(node["X"].InnerText);
                        keep.Y = int.Parse(node["Y"].InnerText);
                        keep.Z = int.Parse(node["Z"].InnerText);
                        keep.FillObjectRelations();
                        keep.Save();
                    #endregion
                    #region KeepComponent
                    case "KeepComponent":
                        if (KeyMapping.ContainsKey(node["KeepComponent_ID"].InnerText))
                            return true;
                        DBKeepComponent keepcomp = new DBKeepComponent();
                        keepcomp.Heading = int.Parse(node["Heading"].InnerText);
                        keepcomp.Health = int.Parse(node["Health"].InnerText);
                        keepcomp.KeepID = int.Parse(node["KeepID"].InnerText);
                        keepcomp.Skin = int.Parse(node["Skin"].InnerText);
                        keepcomp.X = int.Parse(node["X"].InnerText);
                        keepcomp.Y = int.Parse(node["Y"].InnerText);
                        keepcomp.FillObjectRelations();
                        keepcomp.Save();
                        KeyMapping.Add(node["KeepComponent_ID"].InnerText, keepcomp.ID);
                        break;
                    #endregion
                    #region  KeepHookpoint
                    case "keephookpoint":
                        if (KeyMapping.ContainsKey(node["KeepHookPoint_ID"].InnerText))
                            return true; // duplicate
                        DBKeepHookPoint keephookpoint = new DBKeepHookPoint();
                        keephookpoint.Heading = int.Parse(node["Heading"].InnerText);
                        keephookpoint.Height = int.Parse(node["Height"].InnerText);
                        keephookpoint.HookPointID = int.Parse(node["HookPointID"].InnerText);
                        keephookpoint.KeepComponentSkinID = int.Parse(node["KeepComponentSkinID"].InnerText);
                        keephookpoint.X = int.Parse(node["X"].InnerText);
                        keephookpoint.Y = int.Parse(node["Y"].InnerText);
                        keephookpoint.Z = int.Parse(node["Z"].InnerText);
                        keephookpoint.FillObjectRelations();
                        keephookpoint.Save();
                        KeyMapping.Add(node["KeepHookPoint_ID"].InnerText, keephookpoint.ID);
                        break;
                    #endregion
                    #region KeepHookpointItem
                    case "keephookpointitem":
                        if (KeyMapping.ContainsKey(node["KeepHookPointItem_ID"].InnerText))
                            return true;
                        DBKeepHookPointItem hpi = new DBKeepHookPointItem();
                        hpi.ClassType = node["ClassType"].InnerText;
                        hpi.ComponentID = int.Parse(node["ComponentID"].InnerText);
                        hpi.HookPointID = int.Parse(node["HookPointID"].InnerText);
                        hpi.KeepID = int.Parse(node["KeepID"].InnerText);
                        hpi.FillObjectRelations();
                        hpi.Save();
                        KeyMapping.Add(node["KeepHookPointItem_ID"].InnerText, hpi.ID);
                        break;
                    #endregion
                    #region KeepPosition
                    case "keepposition":
                        if (KeyMapping.ContainsKey(node["KeepPosition_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["TemplateID"].InnerText))
                            return false;
                        DBKeepPosition keepposition = new DBKeepPosition();
                        keepposition.ClassType = node["ClassType"].InnerText;
                        keepposition.ComponentRotation = int.Parse(node["ComponentRotation"].InnerText);
                        keepposition.ComponentSkin = int.Parse(node["ComponentSkin"].InnerText);
                        keepposition.Height = int.Parse(node["Height"].InnerText);
                        keepposition.HOff = int.Parse(node["HOff"].InnerText);
                        keepposition.TemplateID = node["TemplateID"].InnerText;
                        keepposition.TemplateType = int.Parse(node["TemplateType"].InnerText);
                        keepposition.XOff = int.Parse(node["XOff"].InnerText);
                        keepposition.YOff = int.Parse(node["YOff"].InnerText);
                        keepposition.ZOff = int.Parse(node["ZOff"].InnerText);
                        keepposition.FillObjectRelations();
                        keepposition.Save();
                        KeyMapping.Add(node["KeepPosition_ID"].InnerText, keepposition.ID);
                        break;
                    #endregion
                    #region LinkedFaction
                    case "linkedfaction":
                        if (KeyMapping.ContainsKey(node["LinkedFaction_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["FactionID"].InnerText))
                            return false;
                        if (!KeyMapping.ContainsKey(node["LinkedFactionID"].InnerText))
                            return false;
                        DBLinkedFaction linkedfaction = new DBLinkedFaction();
                        linkedfaction.FactionID = int.Parse(node["FactionID"].InnerText);
                        linkedfaction.IsFriend = ushort.Parse(node["IsFriend"].InnerText) != 0;
                        linkedfaction.LinkedFactionID = int.Parse(node["LinkedFactionID"].InnerText);
                        linkedfaction.FillObjectRelations();
                        linkedfaction.Save();
                        KeyMapping.Add(node["LinkedFaction_ID"].InnerText, linkedfaction.ID);
                        break;

                    #endregion
                    #region LootGenerator
                    case "lootgenerator":
                        if (KeyMapping.ContainsKey(node["LootGenerator_ID"].InnerText))
                            return true;
                        DBLootGenerator lootgenerator = new DBLootGenerator();
                        lootgenerator.ExclusivePriority = int.Parse(node["ExclusivePriority"].InnerText);
                        lootgenerator.LootGeneratorClass = node["LootGeneratorClass"].InnerText;
                        if (node["MobFaction"] != null)
                            lootgenerator.MobFaction = node["MobFaction"].InnerText;
                        if (node["MobGuild"] != null)
                            lootgenerator.MobGuild = node["MobGuild"].InnerText;
                        if (node["MobName"] != null)
                            lootgenerator.MobName = node["MobName"].InnerText;
                        lootgenerator.RegionID = ushort.Parse(node["RegionID"].InnerText);
                        lootgenerator.FillObjectRelations();
                        lootgenerator.Save();
                        KeyMapping.Add(node["LootGenerator_ID"].InnerText, lootgenerator.ID);
                        break;
                    #endregion
                    #region "LootOtd":
                    case "loototd":
                        if (KeyMapping.ContainsKey(node["LootOTD_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["ItemTemplateID"].InnerText))
                            return false;

                        DBLootOTD loototd = new DBLootOTD();
                        loototd.ItemTemplateID = node["ItemTemplateID"].InnerText;
                        loototd.MinLevel = int.Parse(node["MinLevel"].InnerText);
                        loototd.MobName = node["MobName"].InnerText;
                        loototd.SerializedClassAllowed = node["SerializedClassAllowed"].InnerText; //TODO: Verify
                        loototd.FillObjectRelations();
                        loototd.Save();
                        KeyMapping.Add(node["LootOTD_ID"].InnerText, loototd.ID);
                        break;
                    #endregion
                    #region LootTemplate
                    case "loottemplate":
                        if (KeyMapping.ContainsKey(node["LootTemplate_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["ItemTemplateID"].InnerText))
                            return false;
                        DBLootTemplate loottemplate = new DBLootTemplate();
                        loottemplate.Chance = int.Parse(node["Chance"].InnerText);
                        loottemplate.ItemTemplateID = node["ItemTemplateID"].InnerText;
                        loottemplate.TemplateName = node["TemplateName"].InnerText;
                        loottemplate.FillObjectRelations();
                        loottemplate.Save();
                        KeyMapping.Add(node["LootTemplate_ID"].InnerText, loottemplate.ID);
                        break;
                    #endregion
                    #endregion
                    #region m-p (12)
                    #region MerchantItem
                    case "merchantitem":
                        if (KeyMapping.ContainsKey(node["MerchantItem_ID"].InnerText))
                            return true; //TODO: Duplicate
                        if (!KeyMapping.ContainsKey(node["ItemTemplateID"].InnerText))
                            return false;

                        MerchantItem merchantitem = new MerchantItem();
                        //TODO: is MerchantItem a foreign key (relation) or an associative key? 
                        merchantitem.ItemListID = node["ItemListID"].InnerText;
                        merchantitem.ItemTemplateID = node["ItemTemplateID"].InnerText;
                        merchantitem.PageNumber = int.Parse(node["PageNumber"].InnerText);
                        merchantitem.SlotPosition = int.Parse(node["SlotPosition"].InnerText);
                        merchantitem.FillObjectRelations();
                        merchantitem.Save();
                        KeyMapping.Add(node["MerchantItem_ID"].InnerText, merchantitem.ID);
                        break;
                    #endregion
                    #region Mob
                    case "mob":
                        if (KeyMapping.ContainsKey(node["Mob_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["BoatOwnerID"].InnerText))
                            return false;
                        if (!KeyMapping.ContainsKey(node["EquipmentTemplateID"].InnerText))
                            return false;
                        if (!KeyMapping.ContainsKey(node["ItemsListTemplateID"].InnerText))
                            return false;
                        //if (!KeyMapping.ContainsKey(node["PathID"].InnerText))
                        //  return false;
                        Mob mob = new Mob();
                        mob.AggroLevel = int.Parse(node["AggroLevel"].InnerText);
                        mob.AggroRange = int.Parse(node["AggroRange"].InnerText);
                        mob.BoatOwnerID = KeyMapping[node["BoatOwnerID"].InnerText];
                        if (node["BodyType"] != null)
                            mob.BodyType = int.Parse(node["BodyType"].InnerText);
                        mob.Brain = node["Brain"].InnerText;
                        mob.Charisma = int.Parse(node["Charisma"].InnerText);
                        mob.ClassType = node["ClassType"].InnerText;
                        mob.Constitution = int.Parse(node["Constitution"].InnerText);
                        mob.Dexterity = int.Parse(node["Dexterity"].InnerText);
                        mob.Empathy = int.Parse(node["Empathy"].InnerText);
                        mob.EquipmentTemplateID = KeyMapping[node["EquipmentTemplateID"].InnerText];
                        if (node["FactionID"] != null)
                            mob.FactionID = int.Parse(node["FactionID"].InnerText);
                        mob.Flags = uint.Parse(node["Flags"].InnerText);
                        mob.Guild = node["Guild"].InnerText;
                        mob.Heading = ushort.Parse(node["Heading"].InnerText);
                        mob.HouseNumber = int.Parse(node["HouseNumber"].InnerText);
                        mob.Intelligence = int.Parse(node["Intelligence"].InnerText);
                        mob.ItemsListTemplateID = node["ItemsListTemplateID"].InnerText; //TODO: Verify that
                        mob.Level = byte.Parse(node["Level"].InnerText);
                        if (node["MaxDistance"] != null)
                            mob.MaxDistance = int.Parse(node["MaxDistance"].InnerText);
                        mob.MeleeDamageType = int.Parse(node["MeleeDamageType"].InnerText);
                        mob.Model = ushort.Parse(node["Model"].InnerText);
                        mob.Name = node["Name"].InnerText;
                        if (node["NPCTemplateID"] != null)
                            mob.NPCTemplateID = int.Parse(node["NPCTemplateID"].InnerText);
                        mob.PathID = node["PathID"].InnerText;
                        mob.Piety = int.Parse(node["Piety"].InnerText);
                        mob.Quickness = int.Parse(node["Quickness"].InnerText);
                        mob.Realm = byte.Parse(node["Realm"].InnerText);
                        mob.Region = ushort.Parse(node["Region"].InnerText);
                        if (node["RespawnInterval"] != null)
                            mob.RespawnInterval = int.Parse(node["RespawnInterval"].InnerText);
                        if (node["RoamingRange"] != null)
                            mob.RoamingRange = int.Parse(node["RoamingRange"].InnerText);
                        mob.Size = byte.Parse(node["Size"].InnerText);
                        mob.Speed = int.Parse(node["Speed"].InnerText);
                        mob.Strength = int.Parse(node["Strength"].InnerText);
                        mob.X = int.Parse(node["X"].InnerText);
                        mob.Y = int.Parse(node["Y"].InnerText);
                        mob.Z = int.Parse(node["Z"].InnerText);
                        mob.FillObjectRelations();
                        mob.Save();
                        KeyMapping.Add(node["Mob_ID"].InnerText, mob.ID);
                        break;
                    #endregion     //25th table, and the most important one
                    #region MobXlootTemplate
                    case "mobxloottemplate":
                        if (KeyMapping.ContainsKey(node["MobXLootTemplate_ID"].InnerText))
                            return true;
                        DBMobXLootTemplate mobxloottemplate = new DBMobXLootTemplate();
                        mobxloottemplate.DropCount = int.Parse(node["DropCount"].InnerText);
                        mobxloottemplate.LootTemplateName = node["LootTemplateName"].InnerText;
                        mobxloottemplate.MobName = node["MobName"].InnerText;
                        mobxloottemplate.FillObjectRelations();
                        mobxloottemplate.Save();
                        KeyMapping.Add(node["MobXLootTemplate_ID"].InnerText, mobxloottemplate.ID);
                        break;
                    #endregion
                    #region News
                    case "news":
                        if (KeyMapping.ContainsKey(node["News_ID"].InnerText))
                            return true;
                        DBNews news = new DBNews();
                        news.CreationDate = DateTime.Parse(node["CreationDate"].InnerText);
                        news.Realm = byte.Parse(node["Realm"].InnerText);
                        news.Text = node["Text"].InnerText;
                        news.Type = byte.Parse(node["Type"].InnerText);
                        news.FillObjectRelations();
                        news.Save();
                        KeyMapping.Add(node["News_ID"].InnerText, news.ID);
                        break;
                    #endregion
                    #region NPCEquipment
                    case "npcequipment":
                        if (KeyMapping.ContainsKey(node["NPCEquipment_ID"].InnerText))
                            return true;
                        //if( !KeyMapping.ContainsKey(node["TemplateID"].InnerText))
                        //    return false;
                        NPCEquipment npcequipment = new NPCEquipment();
                        npcequipment.Color = int.Parse(node["Color"].InnerText);
                        npcequipment.Effect = int.Parse(node["Effect"].InnerText);
                        if (node["Extension"] != null)
                            npcequipment.Extension = int.Parse(node["Extension"].InnerText);
                        npcequipment.Model = int.Parse(node["Model"].InnerText);
                        npcequipment.Slot = int.Parse(node["Slot"].InnerText);
                        //DUe to recursion , we move that away
                        //npcequipment.TemplateID = KeyMapping["TemplateID"];
                        npcequipment.FillObjectRelations();
                        npcequipment.Save();
                        KeyMapping.Add(node["NPCEquipment_ID"].InnerText, npcequipment.ID);
                        break;
                    #endregion
                    #region NPCTemplate
                    case "npctemplate":
                        if (KeyMapping.ContainsKey(node["NpcTemplate_ID"].InnerText))
                            return true;
                        if (node["EquipTemplateID"] != null && KeyMapping.ContainsKey(node["EquipmentTemplateID"].InnerText))
                            return false;
                        DBNpcTemplate npctemplate = new DBNpcTemplate();
                        // we got to fix up the 1x1 recursion ...
                        if (node["EquipTemplateID"] != null)
                        {
                            try
                            {
                                npcequipment = (NPCEquipment)DatabaseLayer.Instance.GetDatabaseObjectFromID(KeyMapping[node["EquipmentTemplateID"].InnerText]);
                                npctemplate.EquipmentTemplateID = npcequipment.ID;
                                npcequipment.TemplateID = npctemplate.ID;
                            }
                            catch (KeyNotFoundException e)
                            {
                                npctemplate.EquipmentTemplateID = 0;
                            }
                        }
                        if (node["Abilities"] != null)
                            npctemplate.Abilities = node["Abilities"].InnerText;
                        if (node["AggroLevel"] != null)
                            npctemplate.AggroLevel = byte.Parse(node["AggroLevel"].InnerText);
                        if (node["AggroRange"] != null)
                            npctemplate.AggroRange = int.Parse(node["AggroRange"].InnerText);
                        npctemplate.BlockChance = byte.Parse(node["BlockChance"].InnerText);
                        if (node["BodyType"] != null)
                            npctemplate.BodyType = int.Parse(node["BodyType"].InnerText);
                        npctemplate.Charisma = int.Parse(node["Charisma"].InnerText);
                        npctemplate.ClassType = node["ClassType"].InnerText;
                        npctemplate.Constitution = int.Parse(node["Constitution"].InnerText);
                        npctemplate.Dexterity = int.Parse(node["Dexterity"].InnerText);
                        npctemplate.Empathy = int.Parse(node["Empathy"].InnerText);

                        npctemplate.EvadeChance = byte.Parse(node["EvadeChance"].InnerText);
                        npctemplate.Ghost = ushort.Parse(node["Ghost"].InnerText) != 0;
                        if (node["GuildName"] != null)
                            npctemplate.GuildName = node["GuildName"].InnerText;
                        npctemplate.Intelligence = int.Parse(node["Intelligence"].InnerText);
                        npctemplate.LeftHandSwingChance = byte.Parse(node["LeftHandSwingChance"].InnerText);
                        npctemplate.Level = node["Level"].InnerText;
                        if (node["MaxDistance"] != null)
                            npctemplate.MaxDistance = int.Parse(node["MaxDistance"].InnerText);
                        npctemplate.MaxSpeed = short.Parse(node["MaxSpeed"].InnerText);
                        npctemplate.MeleeDamageType = byte.Parse(node["MeleeDamageType"].InnerText);
                        npctemplate.Model = node["Model"].InnerText;
                        npctemplate.Name = node["Name"].InnerText;
                        npctemplate.ParryChance = byte.Parse(node["ParryChance"].InnerText);
                        npctemplate.Piety = int.Parse(node["Piety"].InnerText);
                        npctemplate.Quickness = int.Parse(node["Quickness"].InnerText);
                        if (node["Size"] != null)
                            npctemplate.Size = node["Size"].InnerText;
                        if (node["Spells"] != null)
                            npctemplate.Spells = node["Spells"].InnerText;
                        npctemplate.Strength = int.Parse(node["Strength"].InnerText);
                        if (node["Style"] != null)
                            npctemplate.Styles = node["Styles"].InnerText;
                        npctemplate.TemplateId = int.Parse(node["TemplateId"].InnerText);
                        if (node["TetherRange"] != null)
                            npctemplate.TetherRange = int.Parse(node["TetherRange"].InnerText);
                        npctemplate.FillObjectRelations();
                        npctemplate.Save();
                        KeyMapping.Add(node["NpcTemplate_ID"].InnerText, npctemplate.ID);

                        break;
                    #endregion
                    #region OTDXCharacter
                    case "otdxcharacter":
                        if (KeyMapping.ContainsKey(node["OTDXCharacter_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["LootOTD_ID"].InnerText))
                            return false;
                        DBOTDXCharacter otdxcharacter = new DBOTDXCharacter();
                        otdxcharacter.CharacterName = node["CharacterName"].InnerText;
                        otdxcharacter.LootOTD_ID = KeyMapping[node["LootOTD_ID"].InnerText];
                        otdxcharacter.FillObjectRelations();
                        otdxcharacter.Save();
                        KeyMapping.Add(node["OTDXCharacter_ID"].InnerText, otdxcharacter.ID);
                        break;
                    #endregion
                    #region Path
                    case "path":
                        if (KeyMapping.ContainsKey(node["Path_ID"].InnerText))
                            return true; //todo: log duplicate
                        DBPath path = new DBPath();
                        path.PathID = node["PathID"].InnerText;
                        path.PathType = int.Parse(node["PathType"].InnerText);
                        path.FillObjectRelations();
                        path.Save();
                        KeyMapping.Add(node["Path_ID"].InnerText, path.ID);
                        break;
                    #endregion
                    #region PathPoints
                    case "pathpoints":
                        if (KeyMapping.ContainsKey(node["PathPoints_ID"].InnerText))
                            return true;
                        DBPathPoint pathpoint = new DBPathPoint();
                        pathpoint.MaxSpeed = int.Parse(node["MaxSpeed"].InnerText);
                        pathpoint.PathID = node["PathID"].InnerText;
                        pathpoint.Step = int.Parse(node["Step"].InnerText);
                        pathpoint.WaitTime = int.Parse(node["WaitTime"].InnerText);
                        pathpoint.X = int.Parse(node["X"].InnerText);
                        pathpoint.Y = int.Parse(node["Y"].InnerText);
                        pathpoint.Z = int.Parse(node["Z"].InnerText);
                        pathpoint.FillObjectRelations();
                        pathpoint.Save();
                        KeyMapping.Add(node["PathPoints_ID"].InnerText, pathpoint.ID);
                        break;
                    #endregion
                    #region PlayerBoats
                    case "playerboats":
                        if (KeyMapping.ContainsKey(node["PlayerBoats_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["BoatID"].InnerText))
                            return false;
                        if (!KeyMapping.ContainsKey(node["OwnerID"].InnerText))
                            return false;
                        DBBoat boat = new DBBoat();
                        boat.BoatID = KeyMapping[node["BoatID"].InnerText];
                        boat.BoatOwner = KeyMapping[node["OwnerID"].InnerText];
                        boat.BoatMaxSpeedBase = int.Parse(node["BoatMaxSpeedBase"].InnerText);
                        boat.BoatModel = ushort.Parse(node["BoatModel"].InnerText);
                        boat.BoatName = node["BoatName"].InnerText;
                        boat.FillObjectRelations();
                        boat.Save();
                        KeyMapping.Add(node["PlayerBoats_ID"].InnerText, boat.ID);
                        break;
                    #endregion
                    #region PlayerXEffect
                    case "PlayerXEffect":
                        if (KeyMapping.ContainsKey(node["PlayerXEffect_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["ChardID"].InnerText))
                            return false;
                        PlayerXEffect playerxeffect = new PlayerXEffect();
                        playerxeffect.ChardID = KeyMapping[node["ChardID"].InnerText];
                        playerxeffect.Duration = int.Parse(node["Duration"].InnerText);
                        playerxeffect.EffectType = node["EffectType"].InnerText;
                        playerxeffect.IsHandler = ushort.Parse(node["IsHandler"].InnerText) != 0;
                        playerxeffect.SpellLine = node["SpellLine"].InnerText;
                        playerxeffect.Var1 = int.Parse(node["Var1"].InnerText);
                        playerxeffect.Var2 = int.Parse(node["Var2"].InnerText);
                        playerxeffect.Var3 = int.Parse(node["Var3"].InnerText);
                        playerxeffect.Var4 = int.Parse(node["Var4"].InnerText);
                        playerxeffect.Var5 = int.Parse(node["Var5"].InnerText);
                        playerxeffect.Var6 = int.Parse(node["Var6"].InnerText);
                        playerxeffect.FillObjectRelations();
                        playerxeffect.Save();
                        KeyMapping.Add(node["PlayerXEffect_ID"].InnerText, playerxeffect.ID);
                        break;
                    #endregion
                    #endregion
                    #region Q-S ( 12)
                    #region Quest
                    case "quest":
                        if (KeyMapping.ContainsKey(node["Quest_ID"].InnerText))
                            return true;
                        DBQuest quest = new DBQuest();
                        quest.CharName = node["CharName"].InnerText;
                        quest.CustomPropertiesString = node["CustomPropertiesString"].InnerText;
                        quest.Name = node["Name"].InnerText;
                        quest.Step = int.Parse(node["Step"].InnerText);
                        quest.FillObjectRelations();
                        quest.Save();
                        KeyMapping.Add(node["Quest_ID"].InnerText, quest.ID);
                        break;
                    #endregion
                    #region Relic
                    case "relic":
                        if (KeyMapping.ContainsKey(node["Relic_ID"].InnerText))
                            return true;
                        DBRelic relic = new DBRelic();
                        relic.Heading = int.Parse(node["Heading"].InnerText);
                        relic.OriginalRealm = int.Parse(node["OriginalRealm"].InnerText);
                        relic.Realm = int.Parse(node["Realm"].InnerText);
                        relic.Region = int.Parse(node["Region"].InnerText);
                        relic.relicType = int.Parse(node["RelicType"].InnerText);
                        relic.X = int.Parse(node["X"].InnerText);
                        relic.Y = int.Parse(node["Y"].InnerText);
                        relic.Z = int.Parse(node["Z"].InnerText);
                        relic.FillObjectRelations();
                        relic.Save();
                        KeyMapping.Add(node["Relic_ID"].InnerText, relic.ID);
                        break;
                    #endregion
                    #region Salvage
                    case "salvage":
                        if (KeyMapping.ContainsKey(node["Salvage_ID"].InnerText))
                            return true;
                        DBSalvage salvage = new DBSalvage();
                        salvage.Id_nb = node["Id_nb"].InnerText;
                        salvage.ObjectType = int.Parse(node["ObjectType"].InnerText);
                        salvage.SalvageLevel = int.Parse(node["SalvageLevel"].InnerText);
                        salvage.FillObjectRelations();
                        salvage.Save();
                        KeyMapping.Add(node["Salvage_ID"].InnerText, salvage.ID);
                        break;
                    #endregion
                    #region ServerProperty
                    case "serverproperty":
                        if (KeyMapping.ContainsKey(node["ServerProperty_ID"].InnerText))
                            return true;
                        ServerProperty serverproperty = new ServerProperty();
                        serverproperty.DefaultValue = node["DefaultValue"].InnerText;
                        serverproperty.Description = node["Description"].InnerText;
                        serverproperty.Key = node["Key"].InnerText;
                        serverproperty.Value = node["Value"].InnerText;
                        serverproperty.FillObjectRelations();
                        serverproperty.Save();
                        KeyMapping.Add(node["ServerProperty_ID"].InnerText, serverproperty.ID);
                        break;
                    #endregion
                    #region SinglePermission
                    case "singlepermission":
                        if (!KeyMapping.ContainsKey(node["SinglePermission_ID"].InnerText))
                            return true;
                        if (!KeyMapping.ContainsKey(node["PlayerID"].InnerText))
                            return false;
                        DBSinglePermission singleperm = new DBSinglePermission();
                        singleperm.Command = node["Command"].InnerText;
                        singleperm.PlayerID = KeyMapping[node["PlayerID"].InnerText];
                        singleperm.FillObjectRelations();
                        singleperm.Save();
                        KeyMapping.Add(node["SinglePermission_ID"].InnerText, singleperm.ID);
                        break;
                    #endregion
                    #region SpellLine
                    case "spellline":
                        if (KeyMapping.ContainsKey(node["SpellLine_ID"].InnerText))
                            return true;
                        DBSpellLine spellline = new DBSpellLine();
                        spellline.IsBaseLine = ushort.Parse(node["IsBaseLine"].InnerText) != 0;
                        spellline.KeyName = node["KeyName"].InnerText;
                        spellline.Name = node["Name"].InnerText;
                        spellline.Spec = node["Spec"].InnerText;
                        spellline.FillObjectRelations();
                        spellline.Save();
                        KeyMapping.Add(node["SpellLine_ID"].InnerText, spellline.ID);
                        break;
                    #endregion
                    #region StarterEquipment
                    case "starterequipment":
                        if (KeyMapping.ContainsKey(node["StarterEquipment_ID"].InnerText))
                            return true;
                        StarterEquipment start = new StarterEquipment();
                        start.Class = byte.Parse(node["Class"].InnerText);
                        start.TemplateID = node["TemplateID"].InnerText;
                        start.FillObjectRelations();
                        start.Save();
                        KeyMapping.Add(node["StarterEquipment_ID"].InnerText, start.ID);
                        break;
                    #endregion
                    #region Specialization
                    case "specialization":
                        if (KeyMapping.ContainsKey(node["Specialization_ID"].InnerText))
                            return true;
                        DBSpecialization spec = new DBSpecialization();
                        spec.Description = node["Description"].InnerText;
                        spec.Icon = ushort.Parse(node["Icon"].InnerText);
                        spec.KeyName = node["KeyName"].InnerText;
                        spec.Name = node["Name"].InnerText;
                        spec.Icon = ushort.Parse(node["Icon"].InnerText);
                        spec.FillObjectRelations();
                        spec.Save();
                        KeyMapping.Add(ode["Specialization_ID"].InnerText, spec.ID);
                        break;
                    #endregion
                    #region SpecXAbility
                    case "specxability":
                        if (KeyMapping.ContainsKey(node["SpecXAbility_ID"].InnerText))
                            return true;
                        DBSpecXAbility specxab = new DBSpecXAbility();
                        specxab.AbilityKey = node["AbilityKey"].InnerText;
                        specxab.AbilityLevel = int.Parse(node["AbilityLevel"].InnerText);
                        specxab.Spec = node["Spec"].InnerText;
                        specxab.SpecLevel = int.Parse(node["SpecLevel"].InnerText);
                        specxab.FillObjectRelations();
                        specxab.Save();
                        KeyMapping.Add(node["SpecXAbility_ID"].InnerText, specxab.ID);
                        break;
                    #endregion
                    #region Spell
                    case "spell":
                        if (KeyMapping.ContainsKey(node["Spell_ID"].InnerText))
                            return true;
                        DBSpell spell = new DBSpell();
                        spell.AllowBolt = ushort.Parse(node["AllowBolt"].InnerText) != 0;
                        spell.AmnesiaChance = int.Parse(node["AmnesiaChance"].InnerText) != 0;
                        spell.CastTime = double.Parse(node["CastTime"].InnerText);
                        spell.ClientEffect = int.Parse(node["ClientEffect"].InnerText);
                        spell.Concentration = int.Parse(node["Concentration"].InnerText);
                        spell.Damage = double.Parse(node["Damage"].InnerText);
                        spell.DamageType = int.Parse(node["DamageType"].InnerText);
                        spell.Description = node["Description"].InnerText;
                        spell.Duration = int.Parse(node["Duration"].InnerText);
                        spell.EffectGroup = int.Parse(node["EffectGroup"].InnerText);
                        spell.Frequency = int.Parse(node["Frequency"].InnerText);
                        spell.HealthPenalty = int.Parse(node["HealthPenalty"].InnerText);
                        spell.Icon = int.Parse(node["Icon"].InnerText);
                        spell.InstrumentRequirement = int.Parse(node["InstrumentRequirement"].InnerText);
                        spell.IsFocus = ushort.Parse(node["IsFocus"].InnerText) != 0;
                        spell.IsPrimary = ushort.Parse(node["IsPrimary"].InnerText) != 0;
                        spell.IsSecondary = ushort.Parse(node["IsSecondary"].InnerText) != 0;
                        spell.LifeDrainReturn = int.Parse(node["LifeDrainReturn"].InnerText) != 0;
                        spell.Message1 = node["Message1"].InnerText;
                        spell.Message2 = node["Message2"].InnerText;
                        spell.Message3 = node["Message3"].InnerText;
                        spell.Message4 = node["Message4"].InnerText;
                        spell.MoveCast = ushort.Parse(node["MoveCast"].InnerText) != 0;
                        spell.Name = node["Name"].InnerText;
                        spell.Power = int.Parse(node["Power"].InnerText);
                        spell.Pulse = int.Parse(node["Pulse"].InnerText);
                        spell.PulsePower = int.Parse(node["PulsePower"].InnerText);
                        spell.Radius = int.Parse(node["Radius"].InnerText);
                        spell.Range = int.Parse(node["Range"].InnerText);
                        spell.RecastDelay = int.Parse(node["RecastDelay"].InnerText);
                        spell.ResurrectHealth = int.Parse(node["ResurrectHealth"].InnerText);
                        spell.ResurrectMana = int.Parse(node["ResurrectMana"].InnerText);
                        spell.SharedTimerGroup = int.Parse(node["SharedTimerGroup"].InnerText);
                        spell.SpellGroup = int.Parse(node["SpellGroup"].InnerText);
                        spell.SpellID = int.Parse(node["SpellID"].InnerText);
                        spell.SubSpellID = int.Parse(node["SubSpellID"].InnerText);
                        spell.Target = node["Target"].InnerText;
                        spell.Type = node["Type"].InnerText;
                        spell.Uninterruptible = ushort.Parse(node["Uniterruptible"].InnerText) != 0;
                        spell.Value = double.Parse(node["Value"].InnerText);
                        spell.FillObjectRelations();
                        spell.Save();
                        KeyMapping.Add(node["Spell_ID"].InnerText, spell.ID);
                        break;
                    #endregion
                    #region Style
                    case "style":
                        if (KeyMapping.ContainsKey(node["Style_ID"].InnerText))
                            return true;
                        DBStyle style = new DBStyle();
                        style.ArmorHitLocation = int.Parse(node["ArmorHitLocation"].InnerText);
                        style.AttackResultRequirement = int.Parse(node["AttackResultRequirement"].InnerText);
                        style.BonusToDefense = int.Parse(node["BonusToDefense"].InnerText);
                        style.BonusToHit = int.Parse(node["BonusToHit"].InnerText);
                        style.ClassId = int.Parse(node["ClassId"].InnerText);
                        style.EnduranceCost = int.Parse(node["EnduranceCost"].InnerText);
                        style.GrowthRate = double.Parse(node["GrowthRate"].InnerText);
                        style.Icon = int.Parse(node["Icon"].InnerText);
                        style.Name = node["Name"].InnerText;
                        style.OpeningRequirementType = int.Parse(node["OpeningRequirementType"].InnerText);
                        style.OpeningRequirementValue = int.Parse(node["OperningRequirementValue"].InnerText);
                        style.RandomProc = ushort.Parse(node["RandomProc"].InnerText) != 0;
                        style.SpecKeyName = node["SpecKeyName"].InnerText;
                        style.SpecLevelRequirement = int.Parse(node["SpecLevelRequirement"].InnerText);
                        style.StealthRequirement = ushort.Parse(node["StealthRequirement"].InnerText) != 0;
                        style.TwoHandAnimation = int.Parse(node["TwoHandAnimation"].InnerText);
                        style.WeaponTypeRequirement = int.Parse(node["WeaponTypeRequirement"].InnerText);
                        break;
                    #endregion
                    #region StyleXSpell
                    case "StyleXSpell":
                        if (KeyMapping.ContainsKey(node["StyleXSpell_ID"].InnerText))
                            return true;
                        DBStyleXSpell stxsp = new DBStyleXSpell();
                        stxsp.Chance = int.Parse(node["Chance"].InnerText);
                        stxsp.ClassID = int.Parse(node["ClassID"].InnerText);
                        stxsp.SpellID = int.Parse(node["SpellID"].InnerText);
                        stxsp.StyleID = int.Parse(node["StyleID"].InnerText);
                        stxsp.FillObjectRelations();
                        stxsp.Save();
                        KeyMapping.Add(node["StyleXSpell_ID"].InnerText, stxsp.ID);
                        break;
                    #endregion
                    #endregion
                    #region T-Z ( 4)
                    #region Task
                    case "task":
                        if (KeyMapping.ContainsKey(node["Task_ID"].InnerText))
                            return true;
                        DBTask task = new DBTask();
                        task.CharName = node["CharName"].InnerText;
                        task.CustomPropertiesString = node["CustomPropertiesString"].InnerText;
                        task.TasksDone = int.Parse(node["TasksDOne"].InnerText);
                        task.TaskType = node["TaskType"].InnerText;
                        task.TimeOut = DateTime.Parse(node["TimeOut"].InnerText);
                        task.FillObjectRelations();
                        task.Save();
                        KeyMapping.Add(node["Task_ID"].InnerText, task.ID);
                        break;
                    #endregion
                    #region Teleport
                    case "teleport":
                        if (KeyMapping.ContainsKey(node["Teleport_ID"].InnerText))
                            return true;
                        Teleport teleport = new Teleport();
                        teleport.Heading = ushort.Parse(node["Heading"].InnerText);
                        teleport.Realm = int.Parse(node["Realm"].InnerText);
                        teleport.RegionID = int.Parse(node["RegionID"].InnerText);
                        teleport.TeleportID = node["TeleportID"].InnerText;
                        teleport.X = int.Parse(node["X"].InnerText);
                        teleport.Y = int.Parse(node["Y"].InnerText);
                        teleport.Z = int.Parse(node["Z"].InnerText);
                        teleport.FillObjectRelations();
                        teleport.Save();
                        KeyMapping.Add(node["Teleport_ID"].InnerText, teleport.ID);
                        break;
                    #endregion
                    #region Worldobject
                    case "worldobject":
                        if (KeyMapping.ContainsKey(node["WorldObject_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        WorldObject worldobject = new WorldObject();
                        worldobject.ClassType = node["ClassType"].InnerText;
                        worldobject.Emblem = int.Parse(node["Emblem"].InnerText);
                        worldobject.Heading = ushort.Parse(node["Heading"].InnerText);
                        worldobject.Model = ushort.Parse(node["Model"].InnerText);
                        worldobject.Name = node["Name"].InnerText;
                        worldobject.Region = ushort.Parse(node["Region"].InnerText);
                        worldobject.X = int.Parse(node["X"].InnerText);
                        worldobject.Y = int.Parse(node["Y"].InnerText);
                        worldobject.Z = int.Parse(node["Z"].InnerText);
                        worldobject.FillObjectRelations();
                        worldobject.Save();
                        KeyMapping.Add(node["WorldObject_ID"].InnerText, worldobject.ID);
                        break;
                    #endregion
                    #region ZonePoint
                    case "zonepoint":
                        if (KeyMapping.ContainsKey(node["ZonePoint_ID"].InnerText))
                            return true;
                        ZonePoint zp = new ZonePoint();
                        zp.ClassType = node["ClassType"].InnerText;
                        zp.Heading = ushort.Parse(node["Heading"].InnerText);
                        zp.Id = ushort.Parse(node["Id"].InnerText);
                        zp.Realm = ushort.Parse(node["Realm"].InnerText);
                        zp.Region = ushort.Parse(node["Region"].InnerText);
                        zp.X = int.Parse(node["X"].InnerText);
                        zp.Y = int.Parse(node["Y"].InnerText);
                        zp.Z = int.Parse(node["Z"].InnerText);
                        zp.FillObjectRelations();
                        zp.Save();
                        KeyMapping.Add(node["ZonePoint_ID"].InnerText, zp.ID);
                        break;
                    #endregion
                    #endregion 
                    #endregion
                default:
                        return false;
                }
            return true;
        }
        /// <summary>
        /// Imports a database and merges it with the current one ( ID_Nbs are verified, nothing else). Merging might not really work 
        /// Requires a PHPMyAdmin XML dump...
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <returns></returns>
        public static bool PerformConversion(XmlNode ParentNode,System.Windows.Forms.ProgressBar pbar)
        {            
            // Old (string) Keys -> New UINT64 keys
            Dictionary<string, UInt64> KeyMapping =new Dictionary<string,ulong>();
            //this is designed for PHPMYADMIN XML dumps
            List<XmlNode> Orphans = new List<XmlNode>(); 
            KeyMapping.Add("",0); // easy trick - NULL gets mapped to 0
            //fixes the "database name" node
            // TODO: Make this readable:
            // IT actually just selects all subelements of the first element
            int Converted = 0;
            pbar.Maximum = ParentNode.ChildNodes.OfType<XmlElement>().First().ChildNodes.Count;
            foreach (XmlNode node in 
                ParentNode.ChildNodes.OfType<XmlElement>().First().ChildNodes.OfType<XmlElement>())
            {
                if (HandleSingleNode(node, KeyMapping) == false)
                    Orphans.Add(node); // some dependency was not yet imported
                Converted++;
                if((Converted % 500) == 0)
                {
                    pbar.Value = Converted;
                }
            }
            pbar.Maximum = Orphans.Count;
            pbar.Value = 0;
            while(Orphans.Count > 0)
            {
                bool bRemoved = false; 
                foreach(XmlNode node in Orphans)
                {
                    if(HandleSingleNode(node,KeyMapping) == true)
                    {
                        Orphans.Remove(node);
                        bRemoved = true;     
                        pbar.Value++;
                        break;
                    }                    
                }
                if (!bRemoved)
                {
                    return false;
                }
            }
            System.Windows.Forms.MessageBox.Show(
                "Total Tables Converted " + Converted + "Orphans left: " + Orphans.Count);

            return true;
           
        }
    }
}
