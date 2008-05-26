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
            switch (node.LocalName.ToLower()) // faster than LINQ
                {
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
                        ability.Description = node["Description"].InnerText;
                        ability.IconID = int.Parse(node["IconID"].InnerText);
                        ability.Name = node["Name"].InnerText;
                        ability.KeyName = node["KeyName"].InnerText;
                        KeyMapping.Add(node["Ability_ID"].InnerText, ability.ID);
                        ability.Save();
                        return true;
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
                #region Battleground
		case "battleground":
                    if(KeyMapping.ContainsKey(node["Battleground_ID"].InnerText))
                        return true; //TODO: Log duplicate
                    Battleground battleground = new Battleground();
                    battleground.MaxLevel = byte.Parse(node["MaxLevel"].InnerText);
                    battleground.MaxRealmLevel = byte.Parse(node["MaxRealmLevel"].InnerText);
                    battleground.MinLevel = byte.Parse(node["MinLevel"].InnerText);
                    battleground.RegionID = ushort.Parse(node["RegionID"].InnerText);
                    battleground.FillObjectRelations();
                    battleground.Save();
                    KeyMapping.Add(node["Battleground_ID"].InnerText,battleground.ID);
                    break; 
	#endregion
                    #region BindPoint
                    case "bindpoint":
                        if (KeyMapping.ContainsKey(node["Bindpoint_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        BindPoint bp = new BindPoint();
                        bp.X = int.Parse(node["X"].InnerText);
                        bp.Y = int.Parse(node["Y"].InnerText);
                        bp.Z = int.Parse(node["Z"].InnerText);
                        bp.Radius = ushort.Parse(node["Radius"].InnerText);
                        bp.Realm = int.Parse(node["Realm"].InnerText);
                        bp.Region = int.Parse(node["Region"].InnerText);
                        KeyMapping.Add(node["Bindpoint_ID"].InnerText, bp.ID);
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
                        crafteditem.CraftingLevel = int.Parse(node["CraftingLevel"].InnerText);
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
                    #region DBHouse
                    case "dbhouse":
                        if (KeyMapping.ContainsKey(node["DBHouse_ID"].InnerText))
                            return true;//TODO: Log duplicate
                        DBCraftedXItem house = DatabaseLayer.Instance.SelectObject(typeof(DBHouse), "HouseNumber", node["HouseNumber"].InnerText) as DBCraftedXItem;
                        if (house != null)
                        {
                            KeyMapping.Add(node["DBHouse_ID"].InnerText, house.ID);
                        }
                        house = new DBCraftedXItem();
                        house.CraftedItemId_nb = node["CraftedItemId_nb"].InnerText;
                        house.IngredientId_nb = node["IngredientId_nb"].InnerText;
                        house.Count = int.Parse(node["Count"].InnerText);
                        KeyMapping.Add(node["DBHouse_ID"].InnerText, house.ID);
                        house.FillObjectRelations();
                        house.Save();
                        return true;
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
                        rank.RankLevel = byte.Parse(node["RankLevel"].InnerText) ;
                        rank.Release = ushort.Parse(node["Release"].InnerText) != 0;
                        rank.Remove = ushort.Parse(node["Remove"].InnerText) != 0;
                        rank.Title = node["Title"].InnerText;
                        rank.Upgrade = ushort.Parse(node["Upgrade"].InnerText) != 0;
                        rank.View = ushort.Parse(node["View"].InnerText) != 0;
                        rank.Withdraw = ushort.Parse(node["Withdraw"].InnerText) != 0;
                        rank.FillObjectRelations();
                        rank.Save();
                        break; 
                       #endregion
                    #region InventoryItem
		case "inventoryitem":
                    InventoryItem invitem= (InventoryItem) DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(InventoryItem),node["Id_nb"].InnerText);
                    if(KeyMapping.ContainsKey(node["InventoryItem_ID"].InnerText))
                        return true; //TODO: Log duplicate
                     if (!KeyMapping.ContainsKey(node["OwerID"].InnerText))
                            return false; //Wait for the owner to appear later on
                    if(invitem != null)
                    {
                        KeyMapping.Add(node["InventoryItem_ID"].InnerText,invitem.ID);
                        return true; //TODO: Log IDNB duplicate
                    }
                    invitem.AllowedClasses  = node["AllowedClasses"].InnerText;
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
                    invitem.Bonus7 =int.Parse(node["Bonus7"].InnerText);
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
                    invitem.DPS_AF =int.Parse(node["DPS_AF"].InnerText);
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
                    invitem.PackSize =  int.Parse(node["PackSize"].InnerText);
                    invitem.Platinum =  short.Parse(node["Platinum"].InnerText);
                    invitem.PoisonCharges =  int.Parse(node["PoisonCharges"].InnerText);
                    invitem.PoisonMaxCharges =  int.Parse(node["PoisonMaxCharges"].InnerText);
                    invitem.PoisonSpellID =  int.Parse(node["PoisonSpellID"].InnerText);
                    invitem.ProcSpellID =  int.Parse(node["ProcSpellID"].InnerText);
                    invitem.ProcSpellID1  = int.Parse(node["ProcSpellID1"].InnerText);
                    invitem.Quality =  int.Parse(node["Quality"].InnerText);
                    invitem.Realm =  int.Parse(node["Realm"].InnerText);
                    invitem.SellPrice =  int.Parse(node["SellPrice"].InnerText);
                    invitem.Silver = byte.Parse(node["Silver"].InnerText);
                    invitem.SlotPosition =  int.Parse(node["SlotPosition"].InnerText);
                    invitem.SPD_ABS =  int.Parse(node["SPD_ABS"].InnerText);
                    invitem.SpellID =  int.Parse(node["SpellID"].InnerText);
                    invitem.SpellID1 = int.Parse(node["SpellID1"].InnerText);
                    invitem.Type_Damage =  int.Parse(node["Type_Damage"].InnerText);
                    //invitem.Value =  long.Parse(node["Value"].InnerText);
                    invitem.Weight =  int.Parse(node["Weight"].InnerText);
                    invitem.FillObjectRelations();
                    invitem.Save();
                    break; 
	#endregion
                    #region Artifact
		                case "artifact": //TODO: Move back -it depends on ItemTempar
                    if (KeyMapping.ContainsKey(node["Artifact_ID"].InnerText))
                            return true;//TODO: Log duplicate
                    if (!KeyMapping.ContainsKey(node["BookID"].InnerText))
                        return false;
                    if(!KeyMapping.ContainsKey(node["EncounterID"].InnerText))
                        return false;
                    if(!KeyMapping.ContainsKey(node["QuestID"].InnerText))
                        return false;
                    if(!KeyMapping.ContainsKey(node["ScholarID"].InnerText))
                        return false;
                    Artifact artifact = (Artifact)DatabaseLayer.Instance.SelectObject(typeof(Artifact),"ArtifactID",node["ArtifactID"].InnerText);
                    if(artifact != null)
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
                    artifact.QuestID =node["QuestID"].InnerText;
                    artifact.ScholarID = node["ScholarID"].InnerText;
                    artifact.Scroll1 = node["Scroll1"].InnerText;
                    artifact.Scroll12 =  node["Scroll12"].InnerText;
                    artifact.Scroll13 = node["Scroll13"].InnerText; 
                    artifact.Scroll2 = node["Scroll2"].InnerText;
                    artifact.Scroll23= node["Scroll23"].InnerText;
                    artifact.Scroll3 = node["Scroll3"].InnerText;
                    artifact.ScrollLevel = int.Parse(node["ScrollLevel"].InnerText);
                    artifact.ScrollModel1 = int.Parse(node["ScrollModel1"].InnerText);
                    artifact.ScrollModel2 =  int.Parse(node["ScrollModel2"].InnerText);
                    artifact.XPRate =  int.Parse(node["XPRate"].InnerText);
                    artifact.Zone =  node["Zone"].InnerText; //TODO: Verify key 
                    KeyMapping.Add(node["Artifact_ID"].InnerText,artifact.ID);
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
                    break; 
	#endregion
                #region ArtifactXItem
		case "artifactxitem":
                    if(KeyMapping.ContainsKey(node["ArtifactXItem_ID"].InnerText))
                        return true;//TODO: Log duplicate
                    if(!KeyMapping.ContainsKey(node["ArtifactID"].InnerText))
                        return false;
                    if(DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(ItemTemplate),node["ItemIDNb"].InnerText) == null)
                        return false;
                    ArtifactXItem artifactxitem = new ArtifactXItem();
                    artifactxitem.ArtifactID = node["ArtifactID"].InnerText;
                    artifactxitem.ItemIDNb = node["ItemIDNb"].InnerText;
                    artifactxitem.Realm = int.Parse(node["Realm"].InnerText);
                    artifactxitem.Version = node["Version"].InnerText;
                    artifactxitem.FillObjectRelations();
                    artifactxitem.Save();
                    KeyMapping.Add(node["ArtifactXItem_ID"].InnerText,artifactxitem.ID);
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
                    break; 
                #endregion
                #region  DBKeepHookpoint
                case "dbkeephookpoint":
                    if (KeyMapping.ContainsKey(node["DBKeepHookPoint_ID"].InnerText))
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
                    break;
                #endregion
                default:
                        return true;
                }
            return true;
        }
        /// <summary>
        /// Imports a database and merges it with the current one ( ID_Nbs are verified, nothing else). Merging might not really work 
        /// Requires a PHPMyAdmin XML dump...
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <returns></returns>
        public static bool PerformConversion(XmlNode ParentNode)
        {            
            // Old (string) Keys -> New UINT64 keys
            Dictionary<string, UInt64> KeyMapping =new Dictionary<string,ulong>();
            //this is designed for PHPMYADMIN XML dumps
            List<XmlNode> Orphans = new List<XmlNode>(); 
            foreach (XmlNode node in ParentNode.ChildNodes.OfType<XmlElement>())
            {
                if (HandleSingleNode(node, KeyMapping) == false)
                    Orphans.Add(node); // some dependency was not yet imported
            }
            while(Orphans.Count > 0)
            {
                bool bRemoved = false; 
                foreach(XmlNode node in Orphans)
                {
                    if(HandleSingleNode(node,KeyMapping) == true)
                    {
                        Orphans.Remove(node);
                        bRemoved = true;
                        break;
                    }
                }
                if (!bRemoved)
                {
                    return false;
                }
            }
            return true;
           
        }
    }
}
