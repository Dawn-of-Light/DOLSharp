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
using DOL.AI;
using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.ServerProperties;
using DOL.Language;
using log4net;
using System.Reflection;

namespace DOL.GS
{
    public class Doppelganger : GameSummoner
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        override public int PetSummonThreshold { get { return 50; } }

        override public NpcTemplate PetTemplate { get { return m_petTemplate; } }
        static private NpcTemplate m_petTemplate = null;

        override public byte PetLevel { get { return 50; } }
        override public byte PetSize { get { return 50; } }

        public Doppelganger() : base() { }
        public Doppelganger(ABrain defaultBrain) : base(defaultBrain) { }
        public Doppelganger(INpcTemplate template) : base(template) { }

        static Doppelganger()
        {
           DBNpcTemplate chthonian = GameServer.Database.SelectObject<DBNpcTemplate>("Name=@Name", new QueryParameter("@Name", "chthonian crawler"));
            if (chthonian != null)
                m_petTemplate = new NpcTemplate(chthonian);
        }

        /// <summary>
        /// Realm point value of this living
        /// </summary>
        public override int RealmPointsValue
        {
            get { return Properties.DOPPELGANGER_REALM_POINTS; }
        }

        /// <summary>
        /// Bounty point value of this living
        /// </summary>
        public override int BountyPointsValue
        {
            get { return Properties.DOPPELGANGER_BOUNTY_POINTS; }
        }

        protected const ushort doppelModel = 2248;

        /// <summary>
        /// Gets/sets the object health
        /// </summary>
        public override int Health
        {
            get { return base.Health; }
            set
            {
                base.Health = value;

                if (value >= MaxHealth)
                {
                    if (Model == doppelModel)
                        Disguise();
                }
                else if (value <= MaxHealth >> 1 && Model != doppelModel)
                {
                    Model = doppelModel;
                    Name = "doppelganger";
                    Inventory = new GameNPCInventory(GameNpcInventoryTemplate.EmptyTemplate);
                    BroadcastLivingEquipmentUpdate();
                }
            }
        }

        /// <summary>
        /// Load a npc from the npc template
        /// </summary>
        /// <param name="obj">template to load from</param>
        public override void LoadFromDatabase(DataObject obj)
        {
            base.LoadFromDatabase(obj);

            Disguise();
        }

        /// <summary>
        /// Starts a melee or ranged attack on a given target.
        /// </summary>
        /// <param name="attackTarget">The object to attack.</param>
        public override void StartAttack(GameObject attackTarget)
        {
            // Don't allow ranged attacks
            if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
            {
                bool standard = Inventory.GetItem(eInventorySlot.RightHandWeapon) != null;
                bool twoHanded = Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null;

                if (standard && twoHanded)
                {
                    if (Util.Random(1) < 1)
                        SwitchWeapon(eActiveWeaponSlot.Standard);
                    else
                        SwitchWeapon(eActiveWeaponSlot.TwoHanded);
                }
                else if (twoHanded)
                    SwitchWeapon(eActiveWeaponSlot.TwoHanded);
                else
                    SwitchWeapon(eActiveWeaponSlot.Standard);
            }
            base.StartAttack(attackTarget);
        }

        /// <summary>
        /// Disguise the doppelganger as an invader
        /// </summary>
        protected void Disguise()
        {
            if (Util.Chance(50))
                Gender = eGender.Male;
            else
                Gender = eGender.Female;

            switch (Util.Random(2))
            {
                case 0: // Albion
                    Name = $"Albion {LanguageMgr.GetTranslation(LanguageMgr.DefaultLanguage, "GamePlayer.RealmTitle.Invader")}";

                    switch (Util.Random(4))
                    {
                        case 0: // Archer
                            Inventory = ClothingMgr.Albion_Archer.CloneTemplate();
                            SwitchWeapon(eActiveWeaponSlot.Distance);

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 3))
                                {
                                    case 0: Model = TemplateMgr.SaracenMale; break;//Saracen Male
                                    case 1: Model = TemplateMgr.HighlanderMale; break;//Highlander Male
                                    case 2: Model = TemplateMgr.BritonMale; break;//Briton Male
                                    case 3: Model = TemplateMgr.IcconuMale; break;//Icconu Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 3))
                                {
                                    case 0: Model = TemplateMgr.SaracenFemale; break;//Saracen Female
                                    case 1: Model = TemplateMgr.HighlanderFemale; break;//Highlander Female
                                    case 2: Model = TemplateMgr.BritonFemale; break;//Briton Female
                                    case 3: Model = TemplateMgr.IcconuFemale; break;//Icconu Female
                                }
                            }
                            break;
                        case 1: // Caster
                            Inventory = ClothingMgr.Albion_Caster.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.AvalonianMale; break;//Avalonian Male
                                    case 1: Model = TemplateMgr.BritonMale; break;//Briton Male
                                    case 2: Model = TemplateMgr.HalfOgreMale; break;//Half Ogre Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.AvalonianFemale; break;//Avalonian Female
                                    case 1: Model = TemplateMgr.BritonFemale; break;//Briton Female
                                    case 2: Model = TemplateMgr.HalfOgreFemale; break;//Half Ogre Female
                                }
                            }
                            break;
                        case 2: // Fighter
                            Inventory = ClothingMgr.Albion_Fighter.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 6))
                                {
                                    case 0: Model = TemplateMgr.HighlanderMale; break;//Highlander Male
                                    case 1: Model = TemplateMgr.BritonMale; break;//Briton Male
                                    case 2: Model = TemplateMgr.SaracenMale; break;//Saracen Male
                                    case 3: Model = TemplateMgr.AvalonianMale; break;//Avalonian Male
                                    case 4: Model = TemplateMgr.HalfOgreMale; break;//Half Ogre Male
                                    case 5: Model = TemplateMgr.IcconuMale; break;//Icconu Male
                                    case 6: Model = TemplateMgr.MinotaurMaleAlb; break;//Minotuar
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 5))
                                {
                                    case 0: Model = TemplateMgr.HighlanderFemale; break;//Highlander Female
                                    case 1: Model = TemplateMgr.BritonFemale; break;//Briton Female
                                    case 2: Model = TemplateMgr.SaracenFemale; break;//Saracen Female
                                    case 3: Model = TemplateMgr.AvalonianFemale; break;//Avalonian Female
                                    case 4: Model = TemplateMgr.HalfOgreFemale; break;//Half Ogre Female
                                    case 5: Model = TemplateMgr.IcconuFemale; break;//Icconu Female
                                }
                            }
                            break;
                        case 3: // GuardHealer
                            Inventory = ClothingMgr.Albion_Healer.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.HighlanderMale; break;//Highlander Male
                                    case 1: Model = TemplateMgr.BritonMale; break;//Briton Male
                                    case 2: Model = TemplateMgr.AvalonianMale; break;//Avalonian Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.HighlanderFemale; break;//Highlander Female
                                    case 1: Model = TemplateMgr.BritonFemale; break;//Briton Female
                                    case 2: Model = TemplateMgr.AvalonianFemale; break;//Avalonian Female
                                }
                            }
                            break;
                        case 4: // Stealther
                            Inventory = ClothingMgr.Albion_Stealther.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.SaracenMale; break;//Saracen Male
                                    case 1: Model = TemplateMgr.BritonMale; break;//Briton Male
                                    case 2: Model = TemplateMgr.IcconuMale; break;//Icconu Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.SaracenFemale; break;//Saracen Female
                                    case 1: Model = TemplateMgr.BritonFemale; break;//Briton Female
                                    case 2: Model = TemplateMgr.IcconuFemale; break;//Icconu Female
                                }
                            }
                            break;
                    }
                    break;
                case 1: // Hibernia
                    Name = $"Hibernia {LanguageMgr.GetTranslation(LanguageMgr.DefaultLanguage, "GamePlayer.RealmTitle.Invader")}";

                    switch (Util.Random(4))
                    {
                        case 0: // Archer
                            Inventory = ClothingMgr.Hibernia_Archer.CloneTemplate();
                            SwitchWeapon(eActiveWeaponSlot.Distance);

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 3))
                                {
                                    case 0: Model = TemplateMgr.LurikeenMale; break;//Lurikeen Male
                                    case 1: Model = TemplateMgr.ElfMale; break;//Elf Male
                                    case 2: Model = TemplateMgr.CeltMale; break;//Celt Male
                                    case 3: Model = TemplateMgr.SharMale; break;//Shar Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 3))
                                {
                                    case 0: Model = TemplateMgr.LurikeenFemale; break;//Lurikeen Female
                                    case 1: Model = TemplateMgr.ElfFemale; break;//Elf Female
                                    case 2: Model = TemplateMgr.CeltFemale; break;//Celt Female
                                    case 3: Model = TemplateMgr.SharFemale; break;//Shar Female
                                }
                            }
                            break;
                        case 1: // Caster
                            Inventory = ClothingMgr.Hibernia_Caster.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 1))
                                {
                                    case 0: Model = TemplateMgr.ElfMale; break;//Elf Male
                                    case 1: Model = TemplateMgr.LurikeenMale; break;//Lurikeen Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 1))
                                {
                                    case 0: Model = TemplateMgr.ElfFemale; break;//Elf Female
                                    case 1: Model = TemplateMgr.LurikeenFemale; break;//Lurikeen Female
                                }
                            }
                            break;
                        case 2: // Fighter
                            Inventory = ClothingMgr.Hibernia_Fighter.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 4))
                                {
                                    case 0: Model = TemplateMgr.FirbolgMale; break;//Firbolg Male
                                    case 1: Model = TemplateMgr.LurikeenMale; break;//Lurikeen Male
                                    case 2: Model = TemplateMgr.CeltMale; break;//Celt Male
                                    case 3: Model = TemplateMgr.SharMale; break;//Shar Male
                                    case 4: Model = TemplateMgr.MinotaurMaleHib; break;//Minotaur
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 3))
                                {
                                    case 0: Model = TemplateMgr.FirbolgFemale; break;//Firbolg Female
                                    case 1: Model = TemplateMgr.LurikeenFemale; break;//Lurikeen Female
                                    case 2: Model = TemplateMgr.CeltFemale; break;//Celt Female
                                    case 3: Model = TemplateMgr.SharFemale; break;//Shar Female
                                }
                            }
                            break;
                        case 3: // GuardHealer
                            Inventory = ClothingMgr.Hibernia_Healer.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.CeltMale; break;//Celt Male
                                    case 1: Model = TemplateMgr.FirbolgMale; break;//Firbolg Male
                                    case 2: Model = TemplateMgr.SylvianMale; break;//Sylvian Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.CeltFemale; break;//Celt Female
                                    case 1: Model = TemplateMgr.FirbolgFemale; break;//Firbolg Female
                                    case 2: Model = TemplateMgr.SylvianFemale; break;//Sylvian Female
                                }
                            }
                            break;
                        case 4: // Stealther
                            Inventory = ClothingMgr.Hibernia_Stealther.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 1))
                                {
                                    case 0: Model = TemplateMgr.ElfMale; break;//Elf Male
                                    case 1: Model = TemplateMgr.LurikeenMale; break;//Lurikeen Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 1))
                                {
                                    case 0: Model = TemplateMgr.ElfFemale; break;//Elf Female
                                    case 1: Model = TemplateMgr.LurikeenFemale; break;//Lurikeen Female
                                }
                            }
                            break;
                    }
                    break;
                case 2: // Midgard
                    Name = $"Midgard {LanguageMgr.GetTranslation(LanguageMgr.DefaultLanguage, "GamePlayer.RealmTitle.Invader")}";

                    switch (Util.Random(4))
                    {
                        case 0: // Archer
                            Inventory = ClothingMgr.Midgard_Archer.CloneTemplate();
                            SwitchWeapon(eActiveWeaponSlot.Distance);

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 4))
                                {
                                    case 0: Model = TemplateMgr.NorseMale; break;//Norse Male
                                    case 1: Model = TemplateMgr.KoboldMale; break;//Kobold Male
                                    case 2: Model = TemplateMgr.DwarfMale; break;//Dwarf Male
                                    case 3: Model = TemplateMgr.ValkynMale; break;//Valkyn Male
                                    case 4: Model = TemplateMgr.FrostalfMale; break;//Frostalf Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 4))
                                {
                                    case 0: Model = TemplateMgr.NorseFemale; break;//Norse Female
                                    case 1: Model = TemplateMgr.KoboldFemale; break;//Kobold Female
                                    case 2: Model = TemplateMgr.DwarfFemale; break;//Dwarf Female
                                    case 3: Model = TemplateMgr.ValkynFemale; break;//Valkyn Female
                                    case 4: Model = TemplateMgr.FrostalfFemale; break;//Frostalf Female
                                }
                            }
                            break;
                        case 1: // Caster
                            Inventory = ClothingMgr.Midgard_Caster.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 3))
                                {
                                    case 0: Model = TemplateMgr.KoboldMale; break;//Kobold Male
                                    case 1: Model = TemplateMgr.NorseMale; break;//Norse Male
                                    case 2: Model = TemplateMgr.DwarfMale; break;//Dwarf Male
                                    case 3: Model = TemplateMgr.FrostalfMale; break;//Frostalf Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 3))
                                {
                                    case 0: Model = TemplateMgr.KoboldFemale; break;//Kobold Female
                                    case 1: Model = TemplateMgr.NorseFemale; break;//Norse Female
                                    case 2: Model = TemplateMgr.DwarfFemale; break;//Dwarf Female
                                    case 3: Model = TemplateMgr.FrostalfFemale; break;//Frostalf Female
                                }
                            }
                            break;
                        case 2: // Fighter
                            Inventory = ClothingMgr.Midgard_Fighter.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 5))
                                {
                                    case 0: Model = TemplateMgr.TrollMale; break;//Troll Male
                                    case 1: Model = TemplateMgr.NorseMale; break;//Norse Male
                                    case 2: Model = TemplateMgr.DwarfMale; break;//Dwarf Male
                                    case 3: Model = TemplateMgr.KoboldMale; break;//Kobold Male
                                    case 4: Model = TemplateMgr.ValkynMale; break;//Valkyn Male
                                    case 5: Model = TemplateMgr.MinotaurMaleMid; break;//Minotaur
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 4))
                                {
                                    case 0: Model = TemplateMgr.TrollFemale; break;//Troll Female
                                    case 1: Model = TemplateMgr.NorseFemale; break;//Norse Female
                                    case 2: Model = TemplateMgr.DwarfFemale; break;//Dwarf Female
                                    case 3: Model = TemplateMgr.KoboldFemale; break;//Kobold Female
                                    case 4: Model = TemplateMgr.ValkynFemale; break;//Valkyn Female
                                }
                            }
                            break;
                        case 3: // GuardHealer
                            Inventory = ClothingMgr.Midgard_Healer.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.DwarfMale; break;//Dwarf Male
                                    case 1: Model = TemplateMgr.NorseMale; break;//Norse Male
                                    case 2: Model = TemplateMgr.FrostalfMale; break;//Frostalf Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.DwarfFemale; break;//Dwarf Female
                                    case 1: Model = TemplateMgr.NorseFemale; break;//Norse Female
                                    case 2: Model = TemplateMgr.FrostalfFemale; break;//Frostalf Female
                                }
                            }
                            break;
                        case 4: // Stealther
                            Inventory = ClothingMgr.Midgard_Stealther.CloneTemplate();

                            if (Gender == eGender.Male)
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.KoboldMale; break;//Kobold Male
                                    case 1: Model = TemplateMgr.NorseMale; break;//Norse Male
                                    case 2: Model = TemplateMgr.ValkynMale; break;//Valkyn Male
                                }
                            }
                            else
                            {
                                switch (Util.Random(0, 2))
                                {
                                    case 0: Model = TemplateMgr.KoboldFemale; break;//Kobold Female
                                    case 1: Model = TemplateMgr.NorseFemale; break;//Norse Female
                                    case 2: Model = TemplateMgr.ValkynFemale; break;//Valkyn Female
                                }
                            }
                            break;
                    }
                    break;
            }

            bool distance = Inventory.GetItem(eInventorySlot.DistanceWeapon) != null;
            bool standard = Inventory.GetItem(eInventorySlot.RightHandWeapon) != null;
            bool twoHanded = Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null;

            if (distance)
                SwitchWeapon(eActiveWeaponSlot.Distance);
            else if (standard && twoHanded)
            {
                if (Util.Random(1) < 1)
                    SwitchWeapon(eActiveWeaponSlot.Standard);
                else
                    SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            }
            else if (twoHanded)
                SwitchWeapon(eActiveWeaponSlot.TwoHanded);
            else
                SwitchWeapon(eActiveWeaponSlot.Standard);
            
        }
    }
}
