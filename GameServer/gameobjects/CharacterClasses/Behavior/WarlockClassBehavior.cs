namespace DOL.GS
{
    public class WarlockClassBehavior : DefaultClassBehavior
    {
        /// <summary>
        /// FIXME this has nothing to do here !
        /// </summary>
        public override bool CanChangeCastingSpeed(SpellLine line, Spell spell)
        {
            if (spell.SpellType == "Chamber")
                return false;

            if ((line.KeyName == "Cursing"
                 || line.KeyName == "Cursing Spec"
                 || line.KeyName == "Hexing"
                 || line.KeyName == "Witchcraft")
                && (spell.SpellType != "ArmorFactorBuff"
                    && spell.SpellType != "Bladeturn"
                    && spell.SpellType != "ArmorAbsorptionBuff"
                    && spell.SpellType != "MatterResistDebuff"
                    && spell.SpellType != "Uninterruptable"
                    && spell.SpellType != "Powerless"
                    && spell.SpellType != "Range"
                    && spell.Name != "Lesser Twisting Curse"
                    && spell.Name != "Twisting Curse"
                    && spell.Name != "Lesser Winding Curse"
                    && spell.Name != "Winding Curse"
                    && spell.Name != "Lesser Wrenching Curse"
                    && spell.Name != "Wrenching Curse"
                    && spell.Name != "Lesser Warping Curse"
                    && spell.Name != "Warping Curse"))
            {
                return false;
            }

            return true;
        }
    }
}