///////////////////////////////////////////////////
////////     Item Model NPC v1.3           ////////
////////     by Benumbed                   ////////
///////////////////////////////////////////////////

/*   Version Notes
 * 
 *  1.0 - Original (Weapon Speed Chanter)
 *  1.1 - Added Procs
 *  1.2 - Added Effects (Glows)
 *  1.3 - Edited to custom fit I50
 * 
*/

using System;
using DOL;
using DOL.GS;
using DOL.Events;
using DOL.Database;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
    [NPCGuildScript("Item Model")]
    public class ItemModelNPC : GameNPC
    {
        private string ItemModelNPC_ITEM_WEAK = "DOL.GS.Scripts.ItemModelNPC_Item_Manipulation";

        #region Constants
        /* Define Constants */
        private ArrayList m_thrustpolearm;
        private ArrayList m_slashpolearm;
        private ArrayList m_crushpolearm;
        private ArrayList m_slashlw;
        private ArrayList m_crushlw;
        private ArrayList m_scythe;
        private ArrayList m_cs;
        private ArrayList m_slashmidgardspear;
        private ArrayList m_thrustmidgardspear;
        private ArrayList m_albcrushtwohand;
        private ArrayList m_albslashtwohand;
        private ArrayList m_albthrusttwohand;
        private ArrayList m_midhammertwohand;
        private ArrayList m_midswordtwohand;
        private ArrayList m_midaxetwohand;
        private ArrayList m_slashflex;
        private ArrayList m_thrustflex;
        private ArrayList m_crushflex;
        private ArrayList m_slashclaw;
        private ArrayList m_thrustclaw;
        private ArrayList m_1hcrush;
        private ArrayList m_1hthrust;
        private ArrayList m_1hslash;
        private ArrayList m_1hhammer;
        private ArrayList m_1haxe;
        private ArrayList m_1hsword;
        private ArrayList m_1hblunt;
        private ArrayList m_1hpierce;
        private ArrayList m_1hblade;
        private ArrayList m_shield;


        int counterattack = 0;
        private eEmote m_emote = eEmote.Raise; // The Emote the NPC does when Interacted
        const int AllowGM = 1; // Allow GM's to bypass checks

        private string Prefix1 = "Unveiling "; // Prefix for Model change
        private string Prefix2 = "Unveiling "; // Prefix for Extension change

        const int AllowModelChange = 1; // 1= yes, 0 = no
        const int ModelChargeType = 0; // 0 = Free, 1 = Money, 2 = BountyPoints
        private long ModelPrice = Money.GetMoney(0, 0, 20, 0, 0);//M/P/G/S/C Amount of gold used for RemoveMoney IF MONEY IS USED ONLY
        const int ModelBountyPrice = 20; // Amount of BountyPoints used for RemoveBountyPoints IF BOUNTY POINTS IS USED ONLY

        const int AllowExtensionChange = 1; // 1= yes, 0 = no
        const int ExtensionChargeType = 0; // 0 = Free, 1 = Money, 2 = BountyPoints
        private long ExtensionPrice = Money.GetMoney(0, 0, 20, 0, 0);//M/P/G/S/C Amount of gold used for RemoveMoney IF MONEY IS USED ONLY
        const int ExtensionBountyPrice = 20; // Amount of BountyPoints used for RemoveBountyPoints IF BOUNTY POINTS IS USED ONLY
        /* END Define Constants */
        #endregion Constants

        #region Fill Weapon Models Allowed

        #region Shield

        private void fillShields()
        {
            m_shield = new ArrayList();

            m_shield.Add(59);
            m_shield.Add(60);
            m_shield.Add(61);
            m_shield.Add(1123);
            m_shield.Add(1141);
            m_shield.Add(1147);
            m_shield.Add(1129);
            m_shield.Add(1138);
            m_shield.Add(1114);
            m_shield.Add(1126);
            m_shield.Add(1665);
            m_shield.Add(1663);
            m_shield.Add(1664);
            m_shield.Add(2218);
            m_shield.Add(2219);
            m_shield.Add(2220);
            m_shield.Add(2210);
            m_shield.Add(2211);
            m_shield.Add(2212);
            m_shield.Add(2200);
            m_shield.Add(2201);
            m_shield.Add(2202);
            m_shield.Add(1162);
            m_shield.Add(1159);
        }

        #endregion

        #region Two Hand

        private void fillThrustPolearm()
        {
            m_thrustpolearm = new ArrayList();

            m_thrustpolearm.Add(3299);
            m_thrustpolearm.Add(2191);
            m_thrustpolearm.Add(872);
            m_thrustpolearm.Add(1929);
            m_thrustpolearm.Add(1931);
            m_thrustpolearm.Add(1932);
            m_thrustpolearm.Add(1661);
        }

        private void fillSlashPolearm()
        {
            m_slashpolearm = new ArrayList();

            m_slashpolearm.Add(3297);
            m_slashpolearm.Add(2191);
            m_slashpolearm.Add(872);
            m_slashpolearm.Add(1929);
            m_slashpolearm.Add(1931);
            m_slashpolearm.Add(1932);
        }

        private void fillCrushPolearm()
        {
            m_crushpolearm = new ArrayList();

            m_crushpolearm.Add(3298);
            m_crushpolearm.Add(2191);
            m_crushpolearm.Add(872);
            m_crushpolearm.Add(1929);
            m_crushpolearm.Add(1931);
            m_crushpolearm.Add(1932);
        }

        private void fillSlashLW()
        {
            m_slashlw = new ArrayList();

            m_slashlw.Add(3259);
            m_slashlw.Add(3254);
            m_slashlw.Add(2208);
            m_slashlw.Add(2196);
            m_slashlw.Add(2204);
            m_slashlw.Add(2110);
            m_slashlw.Add(1670);
            m_slashlw.Add(907);
            m_slashlw.Add(1981);
            m_slashlw.Add(1982);
            m_slashlw.Add(1983);
            m_slashlw.Add(1984);
        }

        private void fillCrushLW()
        {
            m_crushlw = new ArrayList();

            m_crushlw.Add(3260);
            m_crushlw.Add(3255);
            m_crushlw.Add(2110);
            m_crushlw.Add(1670);
            m_crushlw.Add(2206);
            m_crushlw.Add(2215);
            m_crushlw.Add(2213);
            m_crushlw.Add(908);
            m_crushlw.Add(912);
            m_crushlw.Add(640);
            m_crushlw.Add(1977);
            m_crushlw.Add(1978);
            m_crushlw.Add(1979);
            m_crushlw.Add(1980);
        }

        private void fillScythe()
        {
            m_scythe = new ArrayList();

            m_scythe.Add(3231);
            m_scythe.Add(927);
            m_scythe.Add(926);
            m_scythe.Add(928);
            m_scythe.Add(929);
            m_scythe.Add(930);
            m_scythe.Add(932);
            m_scythe.Add(2001);
            m_scythe.Add(2002);
            m_scythe.Add(2003);
            m_scythe.Add(2004);
            m_scythe.Add(2111);
            m_scythe.Add(2213);
        }

        private void fillCS()
        {
            m_cs = new ArrayList();

            m_cs.Add(3263);
            m_cs.Add(2191);
            m_cs.Add(1661);
            m_cs.Add(556);
            m_cs.Add(642);
            m_cs.Add(475);
            m_cs.Add(476);
            m_cs.Add(477);
            m_cs.Add(470);
            m_cs.Add(2005);
            m_cs.Add(2006);
            m_cs.Add(2007);
            m_cs.Add(2008);
        }

        private void fillSlashMidgardSpear()
        {
            m_slashmidgardspear = new ArrayList();

            m_slashmidgardspear.Add(3320);
            m_slashmidgardspear.Add(328);
            m_slashmidgardspear.Add(329);
            m_slashmidgardspear.Add(331);
            m_slashmidgardspear.Add(332);
            m_slashmidgardspear.Add(2045);
            m_slashmidgardspear.Add(2046);
            m_slashmidgardspear.Add(2047);
            m_slashmidgardspear.Add(2048);
            m_slashmidgardspear.Add(2191);
            m_slashmidgardspear.Add(1661);
        }

        private void fillThrustMidgardSpear()
        {
            m_thrustmidgardspear = new ArrayList();

            m_thrustmidgardspear.Add(3319);
            m_thrustmidgardspear.Add(328);
            m_thrustmidgardspear.Add(329);
            m_thrustmidgardspear.Add(331);
            m_thrustmidgardspear.Add(332);
            m_thrustmidgardspear.Add(2045);
            m_thrustmidgardspear.Add(2046);
            m_thrustmidgardspear.Add(2047);
            m_thrustmidgardspear.Add(2048);
            m_thrustmidgardspear.Add(2191);
            m_thrustmidgardspear.Add(1661);
        }

        private void fillAlbCrushTwoHand()
        {
            m_albcrushtwohand = new ArrayList();

            m_albcrushtwohand.Add(3302);
            m_albcrushtwohand.Add(3308);
            m_albcrushtwohand.Add(2110);
            m_albcrushtwohand.Add(1670);
            m_albcrushtwohand.Add(2113);
            m_albcrushtwohand.Add(2206);
            m_albcrushtwohand.Add(2215);
            m_albcrushtwohand.Add(842);
            m_albcrushtwohand.Add(1893);
            m_albcrushtwohand.Add(1894);
            m_albcrushtwohand.Add(1895);
            m_albcrushtwohand.Add(1896);
        }

        private void fillAlbSlashTwoHand()
        {
            m_albslashtwohand = new ArrayList();

            m_albslashtwohand.Add(3300);
            m_albslashtwohand.Add(3306);
            m_albslashtwohand.Add(2110);
            m_albslashtwohand.Add(1670);
            m_albslashtwohand.Add(2208);
            m_albslashtwohand.Add(2193);
            m_albslashtwohand.Add(2204);
            m_albslashtwohand.Add(845);
            m_albslashtwohand.Add(1901);
            m_albslashtwohand.Add(1902);
            m_albslashtwohand.Add(1903);
            m_albslashtwohand.Add(1904);
        }

        private void fillAlbThrustTwoHand()
        {
            m_albthrusttwohand = new ArrayList();

            m_albthrusttwohand.Add(3301);
            m_albthrusttwohand.Add(3307);
            m_albthrusttwohand.Add(1661);
            m_albthrusttwohand.Add(846);
            m_albthrusttwohand.Add(1897);
            m_albthrusttwohand.Add(1898);
            m_albthrusttwohand.Add(1899);
            m_albthrusttwohand.Add(1900);
        }

        private void fillMidHammerTwoHand()
        {
            m_midhammertwohand = new ArrayList();

            m_midhammertwohand.Add(3354);
            m_midhammertwohand.Add(3324);
            m_midhammertwohand.Add(3336);
            m_midhammertwohand.Add(3338);
            m_midhammertwohand.Add(3330);
            m_midhammertwohand.Add(3342);
            m_midhammertwohand.Add(3348);
            m_midhammertwohand.Add(2110);
            m_midhammertwohand.Add(1670);
            m_midhammertwohand.Add(2113);
            m_midhammertwohand.Add(2206);
            m_midhammertwohand.Add(2215);
            m_midhammertwohand.Add(842);
            m_midhammertwohand.Add(1893);
            m_midhammertwohand.Add(1894);
            m_midhammertwohand.Add(1895);
            m_midhammertwohand.Add(1896);
        }

        private void fillMidSwordTwoHand()
        {
            m_midswordtwohand = new ArrayList();

            m_midswordtwohand.Add(3356);
            m_midswordtwohand.Add(3326);
            m_midswordtwohand.Add(3318);
            m_midswordtwohand.Add(3332);
            m_midswordtwohand.Add(3314);
            m_midswordtwohand.Add(3344);
            m_midswordtwohand.Add(3350);
            m_midswordtwohand.Add(2110);
            m_midswordtwohand.Add(1670);
            m_midswordtwohand.Add(2208);
            m_midswordtwohand.Add(2196);
            m_midswordtwohand.Add(2204);
            m_midswordtwohand.Add(845);
            m_midswordtwohand.Add(1901);
            m_midswordtwohand.Add(1902);
            m_midswordtwohand.Add(1903);
            m_midswordtwohand.Add(1904);
        }

        private void fillMidAxeTwoHand()
        {
            m_midaxetwohand = new ArrayList();

            m_midaxetwohand.Add(3352);
            m_midaxetwohand.Add(3322);
            m_midaxetwohand.Add(3328);
            m_midaxetwohand.Add(3316);
            m_midaxetwohand.Add(3340);
            m_midaxetwohand.Add(3346);
            m_midaxetwohand.Add(2110);
            m_midaxetwohand.Add(2217);
            m_midaxetwohand.Add(2985);
            m_midaxetwohand.Add(2983);
            m_midaxetwohand.Add(2049);
            m_midaxetwohand.Add(2051);
            m_midaxetwohand.Add(1033);
            m_midaxetwohand.Add(1030);
            m_midaxetwohand.Add(1027);
        }

        #endregion Two Hand

        #region One Hand

        private void fillSlashFlex()
        {
            m_slashflex = new ArrayList();

            m_slashflex.Add(3292);
            m_slashflex.Add(2119);
            m_slashflex.Add(859);
            m_slashflex.Add(865);
            m_slashflex.Add(1925);
            m_slashflex.Add(1926);
            m_slashflex.Add(1927);
            m_slashflex.Add(1928);
        }

        private void fillThrustFlex()
        {
            m_thrustflex = new ArrayList();

            m_thrustflex.Add(3292);
            m_thrustflex.Add(2119);
            m_thrustflex.Add(859);
            m_thrustflex.Add(865);
            m_thrustflex.Add(1925);
            m_thrustflex.Add(1926);
            m_thrustflex.Add(1927);
            m_thrustflex.Add(1928);
        }

        private void fillCrushFlex()
        {
            m_crushflex = new ArrayList();

            m_crushflex.Add(3292);
            m_crushflex.Add(2119);
            m_crushflex.Add(858);
            m_crushflex.Add(864);
            m_crushflex.Add(1921);
            m_crushflex.Add(1922);
            m_crushflex.Add(1923);
            m_crushflex.Add(1924);
        }

        private void fillSlashClaw()
        {
            m_slashclaw = new ArrayList();

            m_slashclaw.Add(3333);
            m_slashclaw.Add(2469);
            m_slashclaw.Add(2197);
            m_slashclaw.Add(982);
            m_slashclaw.Add(980);
            m_slashclaw.Add(970);
            m_slashclaw.Add(2025);
            m_slashclaw.Add(2026);
            m_slashclaw.Add(2027);
            m_slashclaw.Add(2028);
        }

        private void fillThrustClaw()
        {
            m_thrustclaw = new ArrayList();

            m_thrustclaw.Add(3333);
            m_thrustclaw.Add(2469);
            m_thrustclaw.Add(2197);
            m_thrustclaw.Add(982);
            m_thrustclaw.Add(980);
            m_thrustclaw.Add(970);
            m_thrustclaw.Add(2025);
            m_thrustclaw.Add(2026);
            m_thrustclaw.Add(2027);
            m_thrustclaw.Add(2028);
        }

        private void fill1hCrush()
        {
            m_1hcrush = new ArrayList();

            m_1hcrush.Add(3294);
            m_1hcrush.Add(3303);
            m_1hcrush.Add(3282);
            m_1hcrush.Add(3283);
            m_1hcrush.Add(3289);
            m_1hcrush.Add(13);
            m_1hcrush.Add(647);
            m_1hcrush.Add(853);
            m_1hcrush.Add(854);
            m_1hcrush.Add(14);
            m_1hcrush.Add(1671);
            m_1hcrush.Add(1917);
            m_1hcrush.Add(1918);
            m_1hcrush.Add(1919);
            m_1hcrush.Add(1920);
            m_1hcrush.Add(1674);
            m_1hcrush.Add(2109);
            m_1hcrush.Add(1672);
            m_1hcrush.Add(2112);
            m_1hcrush.Add(2198);
            m_1hcrush.Add(2205);
            m_1hcrush.Add(2214);
        }

        private void fill1hSlash()
        {
            m_1hslash = new ArrayList();

            m_1hslash.Add(3295);
            m_1hslash.Add(3304);
            m_1hslash.Add(3284);
            m_1hslash.Add(3290);
            m_1hslash.Add(3269);
            m_1hslash.Add(3273);
            m_1hslash.Add(3276);
            m_1hslash.Add(3);
            m_1hslash.Add(4);
            m_1hslash.Add(5);
            m_1hslash.Add(10);
            m_1hslash.Add(1668);
            m_1hslash.Add(2203);
            m_1hslash.Add(2109);
            m_1hslash.Add(2112);
        }

        private void fill1hThrust()
        {
            m_1hthrust = new ArrayList();

            m_1hthrust.Add(3296);
            m_1hthrust.Add(3305);
            m_1hthrust.Add(3285);
            m_1hthrust.Add(3291);
            m_1hthrust.Add(3270);
            m_1hthrust.Add(3274);
            m_1hthrust.Add(3277);
            m_1hthrust.Add(944);
            m_1hthrust.Add(457);
            m_1hthrust.Add(885);
            m_1hthrust.Add(1953);
            m_1hthrust.Add(1954);
            m_1hthrust.Add(1955);
            m_1hthrust.Add(1956);
            m_1hthrust.Add(877);
            m_1hthrust.Add(1699);
            m_1hthrust.Add(1807);
            m_1hthrust.Add(2468);
        }

        private void fill1hSword()
        {
            m_1hsword = new ArrayList();

            m_1hsword.Add(3325);
            m_1hsword.Add(3317);
            m_1hsword.Add(3331);
            m_1hsword.Add(3313);
            m_1hsword.Add(3343);
            m_1hsword.Add(3349);
            m_1hsword.Add(3355);
            m_1hsword.Add(311);
            m_1hsword.Add(310);
            m_1hsword.Add(312);
            m_1hsword.Add(313);
            m_1hsword.Add(2109);
            m_1hsword.Add(2112);
            m_1hsword.Add(1668);
            m_1hsword.Add(2203);
        }

        private void fill1hHammer()
        {
            m_1hhammer = new ArrayList();

            m_1hhammer.Add(3323);
            m_1hhammer.Add(3329);
            m_1hhammer.Add(3341);
            m_1hhammer.Add(3347);
            m_1hhammer.Add(3353);
            m_1hhammer.Add(322);
            m_1hhammer.Add(323);
            m_1hhammer.Add(324);
            m_1hhammer.Add(2041);
            m_1hhammer.Add(2042);
            m_1hhammer.Add(2043);
            m_1hhammer.Add(2044);
            m_1hhammer.Add(1671);
            m_1hhammer.Add(2109);
            m_1hhammer.Add(1672);
            m_1hhammer.Add(2112);
            m_1hhammer.Add(2198);
            m_1hhammer.Add(2205);
            m_1hhammer.Add(2214);
        }

        private void fill1hAxe()
        {
            m_1haxe = new ArrayList();

            m_1haxe.Add(3321);
            m_1haxe.Add(3327);
            m_1haxe.Add(3315);
            m_1haxe.Add(3339);
            m_1haxe.Add(3345);
            m_1haxe.Add(3351);
            m_1haxe.Add(319);
            m_1haxe.Add(315);
            m_1haxe.Add(573);
            m_1haxe.Add(2037);
            m_1haxe.Add(2038);
            m_1haxe.Add(2039);
            m_1haxe.Add(2040);
            m_1haxe.Add(2109);
            m_1haxe.Add(2216);
        }

        private void fill1hBlade()
        {
            m_1hblade = new ArrayList();

            m_1hblade.Add(3235);
            m_1hblade.Add(3244);
            m_1hblade.Add(3251);
            m_1hblade.Add(3256);
            m_1hblade.Add(3233);
            m_1hblade.Add(3241);
            m_1hblade.Add(3249);
            m_1hblade.Add(3247);
            m_1hblade.Add(3);
            m_1hblade.Add(4);
            m_1hblade.Add(5);
            m_1hblade.Add(10);
            m_1hblade.Add(1668);
            m_1hblade.Add(2203);
            m_1hblade.Add(2109);
            m_1hblade.Add(2112);
        }

        private void fill1hPierce()
        {
            m_1hpierce = new ArrayList();

            m_1hpierce.Add(3245);
            m_1hpierce.Add(3252);
            m_1hpierce.Add(3257);
            m_1hpierce.Add(3234);
            m_1hpierce.Add(3242);
            m_1hpierce.Add(944);
            m_1hpierce.Add(457);
            m_1hpierce.Add(885);
            m_1hpierce.Add(1953);
            m_1hpierce.Add(1954);
            m_1hpierce.Add(1955);
            m_1hpierce.Add(1956);
            m_1hpierce.Add(887);
            m_1hpierce.Add(1669);
            m_1hpierce.Add(1807);
            m_1hpierce.Add(2468);
        }

        private void fill1hBlunt()
        {
            m_1hblunt = new ArrayList();

            m_1hblunt.Add(3236);
            m_1hblunt.Add(3246);
            m_1hblunt.Add(3253);
            m_1hblunt.Add(3258);
            m_1hblunt.Add(3250);
            m_1hblunt.Add(3248);
            m_1hblunt.Add(322);
            m_1hblunt.Add(323);
            m_1hblunt.Add(324);
            m_1hblunt.Add(2041);
            m_1hblunt.Add(2042);
            m_1hblunt.Add(2043);
            m_1hblunt.Add(2044);
            m_1hblunt.Add(1671);
            m_1hblunt.Add(2109);
            m_1hblunt.Add(1672);
            m_1hblunt.Add(2112);
            m_1hblunt.Add(2198);
            m_1hblunt.Add(2205);
            m_1hblunt.Add(2214);
        }

        #endregion One Hand

        #endregion Fill Weapon Models

        #region Kill Attacking Player
        // Kill Player if attacking NPC
        public override void StartAttack(GameObject attackTarget)
        {
            if ((attackTarget is GamePlayer))
            {
                GamePlayer t = (GamePlayer)attackTarget;
                TurnTo(t, 50);
                if (counterattack < 20)
                {
                    Say(t.Name + ", stop attacking me or I will kill you.");
                    counterattack++;
                }
                else
                {
                    counterattack = 0;
                    t.Die(this);
                }
            }
        }
        #endregion Kill Attacking Player

        #region Interact Function
        //This function interacts with the player when right clicked
        public override bool Interact(GamePlayer player)
        {
            if (base.Interact(player))
            {
                // Turn the NPC in the direction of the interact
                TurnTo(player.X, player.Y);

				Emote(m_emote);

                player.Out.SendMessage("Greetings, " + player.Name + "!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                #region Model Change
                if (AllowModelChange > 0)
                {
                    if (ModelChargeType > 0)
                    {
                        if (ModelChargeType > 1)
                        {
                            player.Out.SendMessage("I can change the model of your Weapons/Armor for " + ModelBountyPrice + " Bounty Points!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        else
                        {
                            player.Out.SendMessage("I can change the model of your Weapons/Armor for " + Money.GetString(ModelPrice) + "!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                    }
                    else
                    {
                        player.Out.SendMessage("I can change the model of your Weapons/Armor for FREE!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }

                }
                #endregion Model Change

                #region Extension Change
                if (AllowExtensionChange > 0)
                {
                    if (ExtensionChargeType > 0)
                    {
                        if (ExtensionChargeType > 1)
                        {
                            player.Out.SendMessage("I can change the pads of your Weapons/Armor for " + ExtensionBountyPrice + " Bounty Points!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        else
                        {
                            player.Out.SendMessage("I can change the pads of your Weapons/Armor for " + Money.GetString(ExtensionPrice) + "!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                    }
                    else
                    {
                        player.Out.SendMessage("I can change the pads of your Weapons/Armor for FREE!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    }

                }
                #endregion Extension Change

                player.Out.SendMessage("Simply give me your Item and we can begin!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                return true;
            }
            return false;
        }
        #endregion Interact Function

        #region ReceiveItem Function

        // Receive Items Function
        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer t = source as GamePlayer;
            if (t == null || item == null) return false;

            if (WorldMgr.GetDistance(this, t) > WorldMgr.INTERACT_DISTANCE)
            {
                t.Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (m_thrustpolearm == null)
                fillThrustPolearm();

            if (m_slashpolearm == null)
                fillSlashPolearm();

            if (m_crushpolearm == null)
                fillCrushPolearm();

            if (m_slashlw == null)
                fillSlashLW();

            if (m_crushlw == null)
                fillCrushLW();

            if (m_scythe == null)
                fillScythe();

            if (m_cs == null)
                fillCS();

            if (m_slashmidgardspear == null)
                fillSlashMidgardSpear();

            if (m_thrustmidgardspear == null)
                fillThrustMidgardSpear();

            if (m_albcrushtwohand == null)
                fillAlbCrushTwoHand();

            if (m_albslashtwohand == null)
                fillAlbSlashTwoHand();

            if (m_albthrusttwohand == null)
                fillAlbThrustTwoHand();

            if (m_midhammertwohand == null)
                fillMidHammerTwoHand();

            if (m_midswordtwohand == null)
                fillMidSwordTwoHand();

            if (m_midaxetwohand == null)
                fillMidAxeTwoHand();

            if (m_slashflex == null)
                fillSlashFlex();

            if (m_thrustflex == null)
                fillThrustFlex();

            if (m_crushflex == null)
                fillCrushFlex();

            if (m_slashclaw == null)
                fillSlashClaw();

            if (m_thrustclaw == null)
                fillThrustClaw();

            if (m_1hcrush == null)
                fill1hCrush();

            if (m_1hthrust == null)
                fill1hThrust();

            if (m_1hslash == null)
                fill1hSlash();

            if (m_1hhammer == null)
                fill1hHammer();

            if (m_1haxe == null)
                fill1hAxe();

            if (m_1hsword == null)
                fill1hSword();

            if (m_1hblunt == null)
                fill1hBlunt();

            if (m_1hpierce == null)
                fill1hPierce();

            if (m_1hblade == null)
                fill1hBlade();

            if (m_shield == null)
                fillShields();

            #region Model Change
            if (AllowModelChange > 0)
            {
                #region Armor
                // Vest
                if (item.Item_Type == 25)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Vest] or [Epic Vest] or [ToA Vest]\n");
                }

                // Arms
                if (item.Item_Type == 28)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Arms] or [Epic Arms] or [ToA Arms]\n");
                }

                // Legs
                if (item.Item_Type == 27)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Legs] or [Epic Legs] or [ToA Legs]\n");
                }

                // Helm
                if (item.Item_Type == 21)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Helm] or [Epic Helm] or [ToA Helm]\n");
                }

                // Gloves
                if (item.Item_Type == 22)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Gloves] or [Epic Gloves] or [ToA Gloves]\n");
                }

                // Boots
                if (item.Item_Type == 23)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Boots] or [Epic Boots] or [ToA Boots]\n");
                }

                // Cloak
                if (item.Item_Type == 26)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Cloak] or [ToA Cloak]\n");
                }
                #endregion Armor

                #region Shield
                // Shield
                if (item.Object_Type == 42)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Shield] or [ToA Shield]\n");
                }
                #endregion Shield

                #region Bow
                // Bow
                if (item.Item_Type == 13)
                {
                    if ((t.CharacterClass.Name == "Ranger")
                        || (t.CharacterClass.Name == "Scout")
                        || (t.CharacterClass.Name == "Hunter"))
                    {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Bow] or [Epic Bow] or [ToA Bow]\n");

                    }
                    else 
                    {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Classic Bow]\n");
                    }
                }
                #endregion Bow

                #region Instruments
                // Instruments
                if (item.Object_Type == 45)
                {
                    SendReply(t, "Select what type of Model you would like:\n" +
                                    "[Epic Instrument] or [ToA Instrument]\n");
                }
                #endregion Instruments

                #region One Hand Weapons
                // One Hand Weapon
                if ((item.Item_Type == 10) || ((item.Item_Type == 11) && (item.Object_Type != 42)))
                {
                    // Alb 1h Crush Weapon
                    if (item.Object_Type == 2)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Crush] or [Epic 1h Crush] or [ToA 1h Crush]\n");
                    }

                    // Alb 1h Slash Weapon
                    if (item.Object_Type == 3)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Slash] or [Epic 1h Slash] or [ToA 1h Slash]\n");
                    }

                    // Alb 1h Thrust Weapon
                    if (item.Object_Type == 4)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Thrust] or [Epic 1h Thrust] or [ToA 1h Thrust]\n");
                    }

                    // Flexible
                    if (item.Object_Type == 24)
                    {
                        // Crush
                        if (item.Type_Damage == 1)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Crush Flex]\n" +
                                            "[Epic Crush Flex]\n" +
                                            "[ToA Crush Flex]\n");
                        }
                        // Slash
                        if (item.Type_Damage == 2)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Slash Flex]\n" +
                                            "[Epic Slash Flex]\n" +
                                            "[ToA Slash Flex]\n");
                        }
                        // Thrust
                        if (item.Type_Damage == 3)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Thrust Flex]\n" +
                                            "[Epic Thrust Flex]\n" +
                                            "[ToA Thrust Flex]\n");
                        }
                    }

                    // Mid 1h Sword Weapon
                    if (item.Object_Type == 11)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Sword] or [Epic 1h Sword] or [ToA 1h Sword]\n");
                    }

                    // Mid 1h Hammer Weapon
                    if (item.Object_Type == 12)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Hammer] or [Epic 1h Hammer] or [ToA 1h Hammer]\n");
                    }

                    // Mid 1h Axe Weapon
                    if (item.Object_Type == 13)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Axe] or [Epic 1h Axe] or [ToA 1h Axe]\n");
                    }

                    // Hand to Hand
                    if (item.Object_Type == 25)
                    {
                        // Slash
                        if (item.Type_Damage == 2)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Slash Claw]\n" +
                                            "[Epic Slash Claw]\n" +
                                            "[ToA Slash Claw]\n");
                        }
                        // Thrust
                        if (item.Type_Damage == 3)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Thrust Claw]\n" +
                                            "[Epic Thrust Claw]\n" +
                                            "[ToA Thrust Claw]\n");
                        }
                    }

                    // Hib 1h Blade Weapon
                    if (item.Object_Type == 19)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Blade] or [Epic 1h Blade] or [ToA 1h Blade]\n");
                    }

                    // Hib 1h Pierce Weapon
                    if (item.Object_Type == 21)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Pierce] or [Epic 1h Pierce] or [ToA 1h Pierce]\n");
                    }

                    // Hib 1h Blunt Weapon
                    if (item.Object_Type == 20)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic 1h Blunt] or [Epic 1h Blunt] or [ToA 1h Blunt]\n");
                    }

                }
                #endregion One Hand Weapons

                #region Two Hand Weapons
                // Two Hand Weapon
                if ((item.Item_Type == 12) || (item.Item_Type == 30))
                {
                    // Staff
                    if (item.Object_Type == 8)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic Staff] or [Epic Staff] or [ToA Staff]\n");
                    }

                    // Polearm
                    if (item.Object_Type == 7)
                    {
                        // Crush
                        if (item.Type_Damage == 1)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Crush Polearm]\n" +
                                            "[Epic Crush Polearm]\n" +
                                            "[ToA Crush Polearm]\n");
                        }
                        // Slash
                        if (item.Type_Damage == 2)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Slash Polearm]\n" +
                                            "[Epic Slash Polearm]\n" +
                                            "[ToA Slash Polearm]\n");
                        }
                        // Thrust
                        if (item.Type_Damage == 3)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Thrust Polearm]\n" +
                                            "[Epic Thrust Polearm]\n" +
                                            "[ToA Thrust Polearm]\n");
                        }
                    }

                    // Albion Two Hand
                    if (item.Object_Type == 6)
                    {
                        // Crush
                        if (item.Type_Damage == 1)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Crush TwoHand]\n" +
                                            "[Epic Crush TwoHand]\n" +
                                            "[ToA Crush TwoHand]\n");
                        }
                        // Slash
                        if (item.Type_Damage == 2)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Slash TwoHand]\n" +
                                            "[Epic Slash TwoHand]\n" +
                                            "[ToA Slash TwoHand]\n");
                        }
                        // Thrust
                        if (item.Type_Damage == 3)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Thrust TwoHand]\n" +
                                            "[Epic Thrust TwoHand]\n" +
                                            "[ToA Thrust TwoHand]\n");
                        }
                    }

                    // Large Weapon
                    if (item.Object_Type == 22)
                    {
                        // Crush
                        if (item.Type_Damage == 1)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Epic Crush LargeWeapon]\n" +
                                            "[Classic Crush LargeWeapon]\n" +
                                            "[ToA Crush LargeWeapon]\n");
                        }
                        // Slash
                        if (item.Type_Damage == 2)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Epic Slash LargeWeapon]\n" +
                                            "[Classic Slash LargeWeapon]\n" +
                                            "[ToA Slash LargeWeapon]\n");
                        }
                        // Thrust -- N/A ATM
                        /*if (item.Type_Damage == 3)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Epic Thrust LargeWeapon]\n" +
                                            "[ToA Thrust LargeWeapon]\n");
                        }*/
                    }

                    // Scythe
                    if (item.Object_Type == 26)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic Scythe]\n" +
                                        "[Epic Scythe]\n" +
                                        "[ToA Scythe]\n");
                    }

                    // Celtic Spear
                    if (item.Object_Type == 23)
                    {
                        SendReply(t, "Select what type of Model you would like:\n" +
                                        "[Classic Celtic Spear]\n" +
                                        "[Epic Celtic Spear]\n" +
                                        "[ToA Celtic Spear]\n");
                    }

                    // Midgard Spear
                    if (item.Object_Type == 14)
                    {
                        // Slash
                        if (item.Type_Damage == 2)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Slash Midgard Spear]\n" +
                                            "[Epic Slash Midgard Spear]\n" +
                                            "[ToA Slash Midgard Spear]\n");
                        }
                        // Thrust
                        if (item.Type_Damage == 3)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic Thrust Midgard Spear]\n" +
                                            "[Epic Thrust Midgard Spear]\n" +
                                            "[ToA Thrust Midgard Spear]\n");
                        }
                    }

                    // Midgard Two Handed
                    if (item.Item_Type == 12)
                    {
                        // Sword
                        if (item.Object_Type == 11)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic 2h Sword]\n" +
                                            "[Epic 2h Sword]\n" +
                                            "[ToA 2h Sword]\n");
                        }
                        // Axe
                        if (item.Object_Type == 13)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic 2h Axe]\n" +
                                            "[Epic 2h Axe]\n" +
                                            "[ToA 2h Axe]\n");
                        }
                        // Hammer
                        if (item.Object_Type == 12)
                        {
                            SendReply(t, "Select what type of Model you would like:\n" +
                                            "[Classic 2h Hammer]\n" +
                                            "[Epic 2h Hammer]\n" +
                                            "[ToA 2h Hammer]\n");
                        }
                    }
                }
                #endregion Two Hand Weapons
            }
            #endregion Model Change

            #region Extension Change
            if (AllowExtensionChange > 0)
            {
                #region Armor
                // Vest
                if (item.Item_Type == 25)
                {
                    SendReply(t, "Would you like to change your [Pads]?\n");
                }

                // Arms
                if (item.Item_Type == 28)
                {
                    SendReply(t, "Would you like to change your [Pads]?\n");
                }

                // Legs
                if (item.Item_Type == 27)
                {
                    SendReply(t, "Would you like to change your [Pads]?\n");
                }

                // Helm
                if (item.Item_Type == 21)
                {
                    SendReply(t, "Would you like to change your [Pads]?\n");
                }

                // Gloves
                if (item.Item_Type == 22)
                {
                    SendReply(t, "Would you like to change your [Pads]?\n");
                }

                // Boots
                if (item.Item_Type == 23)
                {
                    SendReply(t, "Would you like to change your [Pads]?\n");
                }

                #endregion Armor

            }
            #endregion Extension Change

            t.TempProperties.setProperty(ItemModelNPC_ITEM_WEAK, new WeakReference(item));
            return false;
        }

        #endregion ReceiveItem Function

        #region WhisperReceive Function
        // This function is the callback function that is called when someone whispers something to this NPC!
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;

            if (!(source is GamePlayer)) return false;

            GamePlayer player = (GamePlayer)source;

            GameObject obj = player.TargetObject;

            // Turn the NPC in the direction of the interact
            TurnTo(player.X, player.Y);

            switch (str)
            {
                #region Extension Change

                case "Pads":
                    SendReply(player, "Now, Select what type:\n");

                    SendReply(player, "[None]\n" +
                                    "[Very Light]\n" +
                                    "[Light]\n" +
                                    "[Medium]\n" +
                                    "[Heavy]\n" +
                                    "[Very Heavy]\n");
                    break;

                case "None":
                    changeExtension(player, 0);
                    break;
                case "Very Light":
                    changeExtension(player, 1);
                    break;
                case "Light":
                    changeExtension(player, 2);
                    break;
                case "Medium":
                    changeExtension(player, 3);
                    break;
                case "Heavy":
                    changeExtension(player, 4);
                    break;
                case "Very Heavy":
                    changeExtension(player, 5);
                    break;


                #endregion Extension Change

                #region Model Change

                #region Vest
                // Vest
                case "Classic Vest":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 388);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 378);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 378);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 378);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 378);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 393);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 383);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 383);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 383);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 388);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 388);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 388);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 139);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 139);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 139);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 139);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 36);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 441);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 81);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 46);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 46);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 230);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 245);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 245);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 245);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 240);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 230);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 235);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 230);

                    break;

                case "Epic Vest":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 708);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 1003);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 378);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 781);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 745);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 746);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 734);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 815);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 782);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 739);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 810);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 805);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 682);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 804);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 733);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 798);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 792);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 797);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 728);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 723);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 713);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 718);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 1267);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 688);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 693);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 1192);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 703);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 799);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 1187);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 761);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 751);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 698);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 766);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 771);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 787);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 776);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 756);

                    break;

                case "ToA Vest":
                    SendReply(player, "Now, Select what model:\n");

                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        SendReply(player, "[Nailah's Robe]\n" +
                                        "[Nailah's Tunic]\n" +
                                        "[Guard of Valor]\n" +
                                        "[Volcanus Vest]\n" +
                                        "[Aerus Vest]\n" +
                                        "[Oceanus Vest]\n" +
                                        "[Stygia Vest]\n");
                        break;
                    }

                    else if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                            || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                            || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        SendReply(player, "[Guard of Valor]\n" +
                                    "[Eirene's Hauberk]\n" +
                                    "[Volcanus Vest]\n" +
                                    "[Aerus Vest]\n" +
                                    "[Oceanus Vest]\n" +
                                    "[Stygia Vest]\n");
                        break;
                    }
                    else
                    {
                        SendReply(player, "[Guard of Valor]\n" +
                                        "[Golden Scarab Vest]\n" +
                                        "[Volcanus Vest]\n" +
                                        "[Aerus Vest]\n" +
                                        "[Oceanus Vest]\n" +
                                        "[Stygia Vest]\n");
                        break;
                    }

                case "Nailah's Robe":
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2516);
                        break;
                    }
                    else if ((player.CharacterClass.Name == "Friar"))
                    { 
                        changeModel(player, 2518);
                        break;
                    }
                    break;

                case "Nailah's Tunic":
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2185);
                        break;
                    }
                    else if ((player.CharacterClass.Name == "Friar"))
                    {
                        changeModel(player, 2185);
                        break;
                    }
                    break;

                case "Guard of Valor":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2479);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2470);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2121);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 2475);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 2476);
                        break;
                    }
                    break;

                case "Eirene's Hauberk":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2226);
                        break;
                    }

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 2511);
                        break;
                    }
                    break;

                case "Golden Scarab Vest":
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2187);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 2497);
                        break;
                    }
                    break;

                case "Volcanus Vest":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1703);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2176);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2162);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1780);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1694);
                        break;
                    }
                    break;

                case "Aerus Vest":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1685);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2144);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2238);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1798);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1736);
                        break;
                    }
                    break;

                case "Oceanus Vest":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2092);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 1640);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1626);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1848);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1771);
                        break;
                    }
                    break;

                case "Stygia Vest":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2124);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2135);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2153);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1757);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1789);
                        break;
                    }
                    break;

                #endregion Vest

                #region Arms
                // Arms
                case "Classic Arms":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 390);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 395);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 385);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 385);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 385);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 390);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 390);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 390);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 141);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 141);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 141);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 141);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 33);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 33);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 83);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 43);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 43);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 43);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 43);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 48);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 48);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 232);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 247);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 247);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 247);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 242);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 232);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 237);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 237);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 237);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 237);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 237);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 232);

                    break;

                case "Epic Arms":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 710);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 748);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 736);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 817);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 784);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 741);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 812);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 807);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 794);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 38);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 730);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 725);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 715);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 720);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 1269);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 690);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 695);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 1194);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 380);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 763);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 753);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 700);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 768);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 773);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 789);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 778);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 758);

                    break;

                case "ToA Arms":
                    SendReply(player, "Now, Select what model:\n");

                    if ((player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Shadowblade") || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        SendReply(player, "[Arms of the Winds]\n" +
                                        "[Foppish Sleeves]\n" +
                                        "[Volcanus Arms]\n" +
                                        "[Aerus Arms]\n" +
                                        "[Oceanus Arms]\n" +
                                        "[Stygia Arms]\n");

                        break;
                    }
                    else
                    {
                        SendReply(player, "[Arms of the Winds]\n" +
                                        "[Volcanus Arms]\n" +
                                        "[Aerus Arms]\n" +
                                        "[Oceanus Arms]\n" +
                                        "[Stygia Arms]\n");

                        break;
                    }

                case "Arms of the Winds":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2503);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2501);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2500);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 2502);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1733);
                        break;
                    }
                    break;

                case "Foppish Sleeves":
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2490);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1732);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 2491);
                        break;
                    }
                    break;

                case "Volcanus Arms":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1702);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2175);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2161);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1779);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1693);
                        break;
                    }
                    break;

                case "Aerus Arms":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1684);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2143);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2237);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1797);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1735);
                        break;
                    }
                    break;

                case "Oceanus Arms":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2091);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 1639);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1625);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1847);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1770);
                        break;
                    }
                    break;

                case "Stygia Arms":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2133);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2134);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2152);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1756);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1788);
                        break;
                    }
                    break;

                #endregion Arms

                #region Legs
                // Legs
                case "Classic Legs":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 389);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 394);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 384);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 384);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 384);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 389);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 389);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 389);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 140);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 140);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 140);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 140);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 37);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 37);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 82);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 256);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 256);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 256);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 256);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 47);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 47);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 231);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 246);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 246);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 246);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 241);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 231);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 236);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 236);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 236);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 236);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 236);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 231);

                    break;

                case "Epic Legs":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 709);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 747);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 736);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 816);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 783);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 740);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 811);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 806);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 793);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 37);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 730);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 724);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 714);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 719);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 1268);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 689);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 694);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 1193);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 379);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 762);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 752);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 699);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 767);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 772);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 788);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 777);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 757);

                    break;

                case "ToA Legs":
                    SendReply(player, "Now, Select what model:\n");

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Bard")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        SendReply(player, "[Alvarus' Leggings]\n" +
                                        "[Wings Dive]\n" +
                                        "[Volcanus Legs]\n" +
                                        "[Aerus Legs]\n" +
                                        "[Oceanus Legs]\n" +
                                        "[Stygia Legs]\n");

                        break;
                    }
                    else
                    {

                        SendReply(player, "[Alvarus' Leggings]\n" +
                                        "[Volcanus Legs]\n" +
                                        "[Aerus Legs]\n" +
                                        "[Oceanus Legs]\n" +
                                        "[Stygia Legs]\n");

                        break;
                    }

                case "Alvarus' Leggings":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2510);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2507);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2505);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 2509);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1745);
                        break;
                    }
                    break;

                case "Wings Dive":
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 2482);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Bard")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Hunter") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Savage"))
                    {
                        changeModel(player, 1767);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2483);
                        break;
                    }
                    break;    
                    
                case "Volcanus Legs":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1700);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2182);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2167);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1786);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1700);
                        break;
                    }
                    break;

                case "Aerus Legs":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1691);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2150);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2243);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1804);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1742);
                        break;
                    }
                    break;

                case "Oceanus Legs":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2098);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 1646);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1631);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1854);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1777);
                        break;
                    }
                    break;

                case "Stygia Legs":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2130);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2141);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2158);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1763);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1815);
                        break;
                    }
                    break;

                #endregion Legs

                #region Helm
                // Helm
                case "Classic Helm":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 838);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 1197);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 1197);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 1197);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 1197);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 335);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 835);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 835);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 835);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 838);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 838);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 838);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 823);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 823);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 823);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 823);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 62);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 62);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 824);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 63);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 63);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 63);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 63);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 93);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 93);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 829);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 1213);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 1213);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 1213);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 335);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 829);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 1216);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 1216);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 1216);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 1216);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 1216);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 829);

                    break;
                case "Epic Helm":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 1292);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 1292);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 1292);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 1292);

                    break;

                case "ToA Helm":
                    SendReply(player, "Now, Select what model:\n");

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden"))
                    {
                        if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion"))
                        {
                            SendReply(player, "[Winged Helm]\n" +
                                            "[Crown of Zahur]\n" +
                                            "[Volcanus Helm]\n" +
                                            "[Aerus Helm]\n" +
                                            "[Oceanus Helm]\n" +
                                            "[Stygia Helm]\n");
                            break;
                        }
                        else
                        {
                            SendReply(player, "[Crown of Zahur]\n" +
                                            "[Volcanus Helm]\n" +
                                            "[Aerus Helm]\n" +
                                            "[Oceanus Helm]\n" +
                                            "[Stygia Helm]\n");
                            break;
                        }
                    }

                    else if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Infiltrator")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Scout"))
                        {
                            SendReply(player, "[Winged Helm]\n" +
                                            "[Crown of Zahur]\n" +
                                            "[Volcanus Helm]\n" +
                                            "[Aerus Helm]\n" +
                                            "[Oceanus Helm]\n" +
                                            "[Stygia Helm]\n");
                            break;
                        }
                        else
                        {
                            SendReply(player, "[Crown of Zahur]\n" +
                                            "[Volcanus Helm]\n" +
                                            "[Aerus Helm]\n" +
                                            "[Oceanus Helm]\n" +
                                            "[Stygia Helm]\n");
                            break;
                        }
                    }

                    else if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Shadowblade")
                   || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane")
                   || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Hunter"))
                    {

                        if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter") || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                        {
                            SendReply(player, "[Winged Helm]\n" +
                                            "[Crown of Zahur]\n" +
                                            "[Winged Valkrie Helm]\n" +
                                            "[Volcanus Helm]\n" +
                                            "[Aerus Helm]\n" +
                                            "[Oceanus Helm]\n" +
                                            "[Stygia Helm]\n");
                            break;
                        }
                        else
                        {
                            SendReply(player, "[Crown of Zahur]\n" +
                                            "[Winged Valkrie Helm]\n" +
                                            "[Volcanus Helm]\n" +
                                            "[Aerus Helm]\n" +
                                            "[Oceanus Helm]\n" +
                                            "[Stygia Helm]\n");
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                case "Winged Helm":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")  || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Scout"))
                    {
                        changeModel(player, 2223);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster") ||(player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion"))
                    {
                        changeModel(player, 2225);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter") || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 2224);
                        break;
                    }
                    break;

                case "Winged Valkrie Helm":
                    changeModel(player, 2951);
                    break;

                case "Crown of Zahur":
                    if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Infiltrator")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1839);
                        break;
                    }

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden"))
                    {
                        changeModel(player, 1840);
                        break;
                    }

                    if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Shadowblade")
                        || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane")
                        || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1841);
                        break;
                    }
                    break;

                case "Volcanus Helm":
                    if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Infiltrator")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                        {
                            changeModel(player, 2376);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar"))
                        {
                            changeModel(player, 2376);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard"))
                        {
                            changeModel(player, 2361);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Scout"))
                        {
                            changeModel(player, 2370);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver"))
                        {
                            changeModel(player, 2373);
                            break;
                        }
                    }

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden"))
                    {
                        if ((player.CharacterClass.Name == "Nightshade"))
                        {
                            changeModel(player, 2400);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist"))
                        {
                            changeModel(player, 2397);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster"))
                        {
                            changeModel(player, 2406);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion"))
                        {
                            changeModel(player, 2409);
                            break;
                        }
                    }

                    if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Shadowblade")
                        || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane")
                        || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Hunter"))
                    {
                        if ((player.CharacterClass.Name == "Shadowblade"))
                        {
                            changeModel(player, 2382);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                        {
                            changeModel(player, 2379);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                        {
                            changeModel(player, 2388);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                        {
                            changeModel(player, 2391);
                            break;
                        }
                    }
                    break;

                case "Aerus Helm":
                    if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Infiltrator")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                        {
                            changeModel(player, 2430);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar"))
                        {
                            changeModel(player, 2418);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard"))
                        {
                            changeModel(player, 2415);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Scout"))
                        {
                            changeModel(player, 2424);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver"))
                        {
                            changeModel(player, 2421);
                            break;
                        }
                    }

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden"))
                    {
                        if ((player.CharacterClass.Name == "Nightshade"))
                        {
                            changeModel(player, 2454);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist"))
                        {
                            changeModel(player, 2451);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster"))
                        {
                            changeModel(player, 2460);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion"))
                        {
                            changeModel(player, 2457);
                            break;
                        }
                    }

                    if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Shadowblade")
                        || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane")
                        || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Hunter"))
                    {
                        if ((player.CharacterClass.Name == "Shadowblade"))
                        {
                            changeModel(player, 2436);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                        {
                            changeModel(player, 2433);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                        {
                            changeModel(player, 2442);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                        {
                            changeModel(player, 2439);
                            break;
                        }
                    }
                    break;

                case "Oceanus Helm":
                    if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Infiltrator")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                        {
                            changeModel(player, 2268);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar"))
                        {
                            changeModel(player, 2256);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard"))
                        {
                            changeModel(player, 2253);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Scout"))
                        {
                            changeModel(player, 2262);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver"))
                        {
                            changeModel(player, 2259);
                            break;
                        }
                    }

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden"))
                    {
                        if ((player.CharacterClass.Name == "Nightshade"))
                        {
                            changeModel(player, 2292);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist"))
                        {
                            changeModel(player, 2289);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster"))
                        {
                            changeModel(player, 2298);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion"))
                        {
                            changeModel(player, 2295);
                            break;
                        }
                    }

                    if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Shadowblade")
                        || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane")
                        || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Hunter"))
                    {
                        if ((player.CharacterClass.Name == "Shadowblade"))
                        {
                            changeModel(player, 2274);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                        {
                            changeModel(player, 2271);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                        {
                            changeModel(player, 2280);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                        {
                            changeModel(player, 2277);
                            break;
                        }
                    }
                    break;

                case "Stygia Helm":
                    if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Infiltrator")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                        {
                            changeModel(player, 2322);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar"))
                        {
                            changeModel(player, 2310);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard"))
                        {
                            changeModel(player, 2307);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Scout"))
                        {
                            changeModel(player, 2316);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver"))
                        {
                            changeModel(player, 2313);
                            break;
                        }
                    }

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden"))
                    {
                        if ((player.CharacterClass.Name == "Nightshade"))
                        {
                            changeModel(player, 2346);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist"))
                        {
                            changeModel(player, 2343);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster"))
                        {
                            changeModel(player, 2352);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion"))
                        {
                            changeModel(player, 2349);
                            break;
                        }
                    }

                    if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Shadowblade")
                        || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane")
                        || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Hunter"))
                    {
                        if ((player.CharacterClass.Name == "Shadowblade"))
                        {
                            changeModel(player, 2328);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                        {
                            changeModel(player, 2325);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                        {
                            changeModel(player, 2334);
                            break;
                        }
                        if ((player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                        {
                            changeModel(player, 2331);
                            break;
                        }
                    }
                    break;

                #endregion Helm

                #region Gloves
                // Gloves
                case "Classic Gloves":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 391);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 376);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 371);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 371);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 371);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 391);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 391);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 391);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 142);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 142);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 142);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 142);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 34);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 34);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 85);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 44);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 44);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 44);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 44);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 49);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 49);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 233);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 248);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 248);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 248);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 243);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 233);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 238);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 238);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 238);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 238);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 238);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 233);

                    break;
                case "Epic Gloves":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 711);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 749);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 737);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 818);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 785);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 742);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 813);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 808);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 795);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 39);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 731);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 726);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 716);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 721);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 1270);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 691);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 696);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 1195);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 381);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 764);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 754);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 701);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 769);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 774);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 790);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 779);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 759);

                    break;

                case "ToA Gloves":
                    SendReply(player, "Now, Select what model:\n");

                    SendReply(player, "[Maddening Scalars]\n" +
                                    "[Mariasha's Sharkskin Gloves]\n" +
                                    "[Volcanus Gloves]\n" +
                                    "[Aerus Gloves]\n" +
                                    "[Oceanus Gloves]\n" +
                                    "[Stygia Gloves]\n");

                    break;

                case "Maddening Scalars":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1746);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2493);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2492);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 2494);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 2495);
                        break;
                    }
                    break;

                case "Mariasha's Sharkskin Gloves":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1746);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2493);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2492);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 2494);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 2495);
                        break;
                    }
                    break;

                case "Volcanus Gloves":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1708);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2493);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2249);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1785);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1699);
                        break;
                    }
                    break;

                case "Aerus Gloves":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1690);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2149);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2250);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1803);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1741);
                        break;
                    }
                    break;

                case "Oceanus Gloves":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2097);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 1645);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1620);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1853);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 2106);
                        break;
                    }
                    break;

                case "Stygia Gloves":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2129);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2140);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2248);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1762);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1814);
                        break;
                    }
                    break;

                #endregion Gloves

                #region Boots
                // Boots
                case "Classic Boots":

                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 392);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 397);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 387);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 387);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 387);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 392);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 392);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 392);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 143);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 143);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 143);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 143);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 133);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 133);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 160);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 45);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 45);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 45);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 45);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 50);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 50);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 234);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 249);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 249);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 249);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 244);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 234);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 239);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 239);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 239);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 239);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 239);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 234);

                    break;

                case "Epic Boots":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 712);
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 750);
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 738);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 819);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 786);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 743);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 814);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 809);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 796);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 40);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 732);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 727);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 717);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 722);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 1271);
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 692);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 697);

                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 1196);
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 382);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 765);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 755);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 702);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 770);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 775);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 791);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 780);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 760);

                    break;

                case "ToA Boots":
                    SendReply(player, "Now, Select what model:\n");

                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist") || (player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer") || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        SendReply(player, "[Flamedancer's Boots]\n" +
                                        "[Volcanus Boots]\n" +
                                        "[Aerus Boots]\n" +
                                        "[Oceanus Boots]\n" +
                                        "[Stygia Boots]\n");
                        break;
                    }
                    else
                    {
                        SendReply(player, "[Enyalio's Boots]\n" +
                                        "[Volcanus Boots]\n" +
                                        "[Aerus Boots]\n" +
                                        "[Oceanus Boots]\n" +
                                        "[Stygia Boots]\n");
                        break;
                    }

                case "Flamedancer's Boots":
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2486);
                        break;
                    }
                    else
                    {
                        changeModel(player, 1730);
                        break;
                    }

                case "Enyalio's Boots":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2487);
                        break;
                    }

                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1729);
                        break;
                    }

                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 2488);
                        break;
                    }
                    break;

                case "Volcanus Boots":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1706);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2179);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2165);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1783);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1715);
                        break;
                    }
                    break;

                case "Aerus Boots":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 1688);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2147);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2241);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1801);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1751);
                        break;
                    }
                    break;

                case "Oceanus Boots":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2095);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 1643);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1629);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1851);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1774);
                        break;
                    }
                    break;

                case "Stygia Boots":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        changeModel(player, 2127);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Shadowblade"))
                    {
                        changeModel(player, 2138);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 2156);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Berserker") || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1760);
                        break;
                    }
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Champion")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1812);
                        break;
                    }
                    break;

                #endregion Boots

                #region Cloak

                case "Classic Cloak":
                    changeModel(player, 57);
                    break;

                case "ToA Cloak":
                    SendReply(player, "Now, Select what model:\n");

                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        SendReply(player, "[Cloudsong]\n" +
                                        "[Airy Cloak]\n" +
                                        "[Coral Cloak]\n" +
                                        "[Desert Cloak]\n");
                        break;
                    }

                    else if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Nightshade")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Shadowblade") || (player.CharacterClass.Name == "Hunter"))
                    {
                        SendReply(player, "[Harpy Feather]\n" +
                                        "[Shades of Mist]\n" +
                                        "[Airy Cloak]\n" +
                                        "[Coral Cloak]\n" +
                                        "[Desert Cloak]\n");
                        break;
                    }

                    else if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        SendReply(player, "[Healers Embrace]\n" +
                                        "[Airy Cloak]\n" +
                                        "[Coral Cloak]\n" +
                                        "[Desert Cloak]\n");
                        break;
                    }

                    else if ((player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden")
                   || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                   || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Berserker"))
                    {
                        SendReply(player, "[Harpy Feather]\n" +
                                        "[Shades of Mist]\n" +
                                        "[Magma Cloak]\n" +
                                        "[Coral Cloak]\n" +
                                        "[Desert Cloak]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Harpy Feather":
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden")
                   || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                   || (player.CharacterClass.Name == "Shadowblade") || (player.CharacterClass.Name == "Hunter") || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Berserker"))
                    {
                        changeModel(player, 1721);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Healers Embrace":
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 1723);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Shades of Mist":
                    if ((player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden")
                        || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Shadowblade") || (player.CharacterClass.Name == "Hunter") || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Berserker"))
                    {
                        changeModel(player, 1726);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Cloudsong":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1727);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Magma Cloak":
                    if ((player.CharacterClass.Name == "Ranger") || (player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Savage") || (player.CharacterClass.Name == "Thane") || (player.CharacterClass.Name == "Warrior") || (player.CharacterClass.Name == "Skald") || (player.CharacterClass.Name == "Berserker"))
                    {
                        changeModel(player, 1725);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Airy Cloak":
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid") || (player.CharacterClass.Name == "Valewalker") || (player.CharacterClass.Name == "Nightshade") || (player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cleric") || (player.CharacterClass.Name == "Infiltrator") || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Scout") || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman") || (player.CharacterClass.Name == "Shadowblade") || (player.CharacterClass.Name == "Hunter") || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1720);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Coral Cloak":
                    changeModel(player, 1722);
                    break;

                case "Desert Cloak":
                    changeModel(player, 1724);
                    break;

                #endregion Cloak

                #region Shield

                case "Classic Shield":
                    // Small
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        SendReply(player, "[Round Shield]\n" +
                                        "[Horned Shield]\n" +
                                        "[Grave Shield]\n" +
                                        "[Peanut Shield]\n");
                        break;
                    }

                    // Medium
                    else if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                         || (player.CharacterClass.Name == "Cleric")
                         || (player.CharacterClass.Name == "Thane"))
                    {
                        SendReply(player, "[Heater Shield]\n" +
                                        "[Horned Shield]\n" +
                                        "[Grave Shield]\n" +
                                        "[Celtic Shield]\n");
                        break;
                    }

                    // Large
                    else if ((player.CharacterClass.Name == "Hero")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Warrior"))
                    {
                        SendReply(player, "[Tower Shield]\n" +
                                        "[Horned Shield]\n" +
                                        "[Riot Shield]\n" +
                                        "[Grave Shield]\n" +
                                        "[Celtic Shield]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Round Shield":
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 59);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Heater Shield":
                    if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                         || (player.CharacterClass.Name == "Cleric")
                         || (player.CharacterClass.Name == "Thane"))
                    {
                        changeModel(player, 60);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Tower Shield":
                    if ((player.CharacterClass.Name == "Hero")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 61);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Riot Shield":
                    if ((player.CharacterClass.Name == "Hero")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1123);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Peanut Shield":
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 1141);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Celtic Shield":
                    if ((player.CharacterClass.Name == "Hero")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1147);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Grave Shield":
                    // Small
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 1129);
                        break;
                    }

                    // Medium
                    else if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Thane"))
                    {
                        changeModel(player, 1129);
                        break;
                    }

                    // Large
                    else
                    {
                        changeModel(player, 1138);
                        break;
                    }

                case "Horned Shield":
                    // Small
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 1114);
                        break;
                    }

                    // Medium
                    else if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Thane"))
                    {
                        changeModel(player, 1114);
                        break;
                    }

                    // Large
                    else
                    {
                        changeModel(player, 1126);
                        break;
                    }

                case "ToA Shield":
                    SendReply(player, "Now, Select what model:\n");

                    // Small
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        SendReply(player, "[Aten's Shield]\n" +
                                        "[Cyclops Eye Shield]\n" +
                                        "[Magma Shield]\n" +
                                        "[Nautilus Shield]\n" +
                                        "[Leaf Shield]\n" +
                                        "[Aerus Shield]\n");
                        break;
                    }

                    // Medium
                    else if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Thane"))
                    {
                        SendReply(player, "[Aten's Shield]\n" +
                                        "[Cyclops Eye Shield]\n" +
                                        "[Magma Shield]\n" +
                                        "[Nautilus Shield]\n" +
                                        "[Leaf Shield]\n" +
                                        "[Aerus Shield]\n");
                        break;
                    }

                    // Large
                    else if ((player.CharacterClass.Name == "Hero")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Warrior"))
                    {
                        SendReply(player, "[Shield of Khaos]\n" +
                                        "[Aten's Shield]\n" +
                                        "[Cyclops Eye Shield]\n" +
                                        "[Magma Shield]\n" +
                                        "[Nautilus Shield]\n" +
                                        "[Leaf Shield]\n" +
                                        "[Aerus Shield]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Shield of Khaos":
                    if ((player.CharacterClass.Name == "Hero")
                        || (player.CharacterClass.Name == "Reaver") || (player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin")
                        || (player.CharacterClass.Name == "Warrior"))
                    {
                        changeModel(player, 1665);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Aten's Shield":
                    changeModel(player, 1663);
                    break;

                case "Cyclops Eye Shield":
                    changeModel(player, 1664);
                    break;

                case "Magma Shield":
                    // Small
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 2218);
                        break;
                    }

                    // Medium
                    else if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Thane"))
                    {
                        changeModel(player, 2219);
                        break;
                    }

                    // Large
                    else 
                    {
                        changeModel(player, 2220);
                        break;
                    }

                case "Aerus Shield":
                    // Small
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 2210);
                        break;
                    }

                    // Medium
                    else if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Thane"))
                    {
                        changeModel(player, 2211);
                        break;
                    }

                    // Large
                    else 
                    {
                        changeModel(player, 2212);
                        break;
                    }

                case "Nautilus Shield":
                    // Small
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 2200);
                        break;
                    }

                    // Medium
                    else if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Thane"))
                    {
                        changeModel(player, 2201);
                        break;
                    }

                    // Large
                    else
                    {
                        changeModel(player, 2202);
                        break;
                    }

                case "Leaf Shield":
                    // Small
                    if ((player.CharacterClass.Name == "Bard") || (player.CharacterClass.Name == "Druid")
                        || (player.CharacterClass.Name == "Minstrel") || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Healer") || (player.CharacterClass.Name == "Shaman"))
                    {
                        changeModel(player, 1162);
                        break;
                    }

                    // Medium
                    else if ((player.CharacterClass.Name == "Champion") || (player.CharacterClass.Name == "Warden") || (player.CharacterClass.Name == "Blademaster")
                        || (player.CharacterClass.Name == "Cleric")
                        || (player.CharacterClass.Name == "Thane"))
                    {
                        changeModel(player, 1162);
                        break;
                    }

                    // Large
                    else
                    {
                        changeModel(player, 1159);
                        break;
                    }


                #endregion Shield

                #region Bow
                case "Classic Bow":
                    if (player.CharacterClass.Name == "Ranger")
                    {
                        {
                            SendReply(player, "[Recurve Bow]\n" +
                                            "[Skinners Bow]\n" +
                                            "[Thorn Bow]\n" +
                                            "[Air Recurve Bow]\n" +
                                            "[Earth Recurve Bow]\n" +
                                            "[Fire Recurve Bow]\n" +
                                            "[Water Recurve Bow]\n");
                            break;
                        }
                    }

                    else if (player.CharacterClass.Name == "Scout")
                    {
                        {
                            SendReply(player, "[LongBow]\n" +
                                            "[Skinners Bow]\n" +
                                            "[Thorn Bow]\n" +
                                            "[Air LongBow]\n" +
                                            "[Earth LongBow]\n" +
                                            "[Fire LongBow]\n" +
                                            "[Water LongBow]\n");
                            break;
                        }
                    }

                    else if (player.CharacterClass.Name == "Hunter")
                    {
                        {
                            SendReply(player, "[Composite Bow]\n" +
                                            "[Skinners Bow]\n" +
                                            "[Thorn Bow]\n" +
                                            "[Air Composite Bow]\n" +
                                            "[Earth Composite Bow]\n" +
                                            "[Fire Composite Bow]\n" +
                                            "[Water Composite Bow]\n");
                            break;
                        }
                    }

                    else if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        {
                            SendReply(player, "[Crossbow]\n" +
                                            "[Assault Crossbow]\n" +
                                            "[Brawlers Crossbow]\n" +
                                            "[Skinners Crossbow]\n" +
                                            "[Air Crossbow]\n" +
                                            "[Earth Crossbow]\n" +
                                            "[Fire Crossbow]\n" +
                                            "[Water Crossbow]\n");
                            break;
                        }
                    }

                    else if ((player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Warden"))
                    {
                        {
                            SendReply(player, "[Shortbow]\n" +
                                            "[Skinners Bow]\n" +
                                            "[Thorn Bow]\n" +
                                            "[Air Shortbow]\n" +
                                            "[Earth Shortbow]\n" +
                                            "[Fire Shortbow]\n" +
                                            "[Water Shortbow]\n");
                            break;
                        }
                    }

                    else
                    {
                        break;
                    }

                case "Air Composite Bow":
                    if (player.CharacterClass.Name == "Hunter")
                    {
                        changeModel(player, 2061);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Earth Composite Bow":
                    if (player.CharacterClass.Name == "Hunter")
                    {
                        changeModel(player, 2062);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Fire Composite Bow":
                    if (player.CharacterClass.Name == "Hunter")
                    {
                        changeModel(player, 2063);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Water Composite Bow":
                    if (player.CharacterClass.Name == "Hunter")
                    {
                        changeModel(player, 2064);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Air Recurve Bow":
                    if (player.CharacterClass.Name == "Ranger")
                    {
                        changeModel(player, 1993);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Earth Recurve Bow":
                    if (player.CharacterClass.Name == "Ranger")
                    {
                        changeModel(player, 1994);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Fire Recurve Bow":
                    if (player.CharacterClass.Name == "Ranger")
                    {
                        changeModel(player, 1995);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Water Recurve Bow":
                    if (player.CharacterClass.Name == "Ranger")
                    {
                        changeModel(player, 1996);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Air Longbow":
                    if (player.CharacterClass.Name == "Scout")
                    {
                        changeModel(player, 1909);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Earth Longbow":
                    if (player.CharacterClass.Name == "Scout")
                    {
                        changeModel(player, 1910);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Fire Longbow":
                    if (player.CharacterClass.Name == "Scout")
                    {
                        changeModel(player, 1911);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Water Longbow":
                    if (player.CharacterClass.Name == "Scout")
                    {
                        changeModel(player, 1912);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Air Crossbow":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        changeModel(player, 1961);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Earth Crossbow":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        changeModel(player, 1962);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Fire Crossbow":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        changeModel(player, 1963);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Water Crossbow":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        changeModel(player, 1964);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Air Shortbow":
                    if ((player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Warden"))
                    {
                        changeModel(player, 1997);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Earth Shortbow":
                    if ((player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Warden"))
                    {
                        changeModel(player, 1998);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Fire Shortbow":
                    if ((player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Warden"))
                    {
                        changeModel(player, 1999);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Water Shortbow":
                    if ((player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Warden"))
                    {
                        changeModel(player, 2000);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Skinners Bow":
                    changeModel(player, 851);
                    break;

                case "Thorn Bow":
                    changeModel(player, 852);
                    break;

                case "Longbow":
                    if (player.CharacterClass.Name == "Scout")
                    {
                        changeModel(player, 132);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Shortbow":
                    if ((player.CharacterClass.Name == "Mercenary") || (player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Blademaster") || (player.CharacterClass.Name == "Warden"))
                    {
                        changeModel(player, 569);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Recurve Bow":
                    if (player.CharacterClass.Name == "Ranger")
                    {
                        changeModel(player, 471);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Composite Bow":
                    if (player.CharacterClass.Name == "Hunter")
                    {
                        changeModel(player, 564);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Crossbow":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        changeModel(player, 226);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Assault Crossbow":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        changeModel(player, 890);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Brawlers Crossbow":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        changeModel(player, 891);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Skinners Crossbow":
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin") || (player.CharacterClass.Name == "Infiltrator"))
                    {
                        changeModel(player, 893);
                        break;
                    }
                    else
                    {
                        break;
                    }


                case "ToA Bow":
                    if ((player.CharacterClass.Name == "Ranger")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Hunter"))
                    {
                        {
                            SendReply(player, "[Braggart's Bow]\n" +
                                            "[Fool's Bow]\n");
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                case "Braggart's Bow":
                    if ((player.CharacterClass.Name == "Ranger")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1667);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Fool's Bow":
                    if ((player.CharacterClass.Name == "Ranger")
                        || (player.CharacterClass.Name == "Scout")
                        || (player.CharacterClass.Name == "Hunter"))
                    {
                        changeModel(player, 1666);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Epic Bow":
                    // Hibernia
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 3243);

                    // Albion
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 3275);

                    // Midgard
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 3365);

                    break;

                #endregion Bow

                #region Instruments

                case "ToA Instrument":
                    // Hibernia
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 2116);

                    // Albion
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 2116);

                    break;

                case "Epic Instrument":
                    // Hibernia
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 3239);

                    // Albion
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 3280);

                    break;

                #endregion Instruments

                #region Weapons

                #region Two Hand

                #region Staff
                case "Epic Staff":
                    // Hibernia
                    if (player.CharacterClass.Name == "Eldritch") changeModel(player, 3226);
                    if (player.CharacterClass.Name == "Enchanter") changeModel(player, 3227);
                    if (player.CharacterClass.Name == "Mentalist") changeModel(player, 3228);

                    // Albion
                    if (player.CharacterClass.Name == "Cabalist") changeModel(player, 3264);
                    if (player.CharacterClass.Name == "Sorcerer") changeModel(player, 3265);
                    if (player.CharacterClass.Name == "Theurgist") changeModel(player, 3266);
                    if (player.CharacterClass.Name == "Wizard") changeModel(player, 3267);
                    if (player.CharacterClass.Name == "Friar") changeModel(player, 3271);

                    // Midgard
                    if (player.CharacterClass.Name == "Runemaster") changeModel(player, 3309);
                    if (player.CharacterClass.Name == "Spiritmaster") changeModel(player, 3310);
                    if (player.CharacterClass.Name == "Bonedancer") changeModel(player, 3311);

                    break;

                case "Classic Staff":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        SendReply(player, "[Mage Staff]\n" +
                                            "[Mage Staff 2]\n" +
                                            "[Mage Staff 3]\n" +
                                            "[Shod Staff]\n" +
                                            "[Shod Staff 2]\n" +
                                            "[Shod Staff 3]\n" +
                                            "[Shod Staff 4]\n" +
                                            "[Claw Staff]\n" +
                                            "[Globe Staff]\n" +
                                            "[Hook Staff]\n");
                        break;
                    }

                    else if ((player.CharacterClass.Name == "Friar"))
                    {
                        SendReply(player, "[Quarterstaff]\n" +
                                            "[QuarterShod Staff]\n" +
                                            "[Bishop Staff]\n" +
                                            "[Spiked Staff]\n" +
                                            "[Shod Staff 2]\n" +
                                            "[Shod Staff 3]\n" +
                                            "[Shod Staff 4]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Globe Staff":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1166);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Hook Staff":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                    changeModel(player, 1170);
                    break;
                                                                                                                                                                                        }
                    else
                    {
                        break;
                    }

                case "Shod Staff":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                    changeModel(player, 565);
                    break;
                                                                                                                                                                                                                }
                    else
                    {
                        break;
                    }

                case "Shod Staff 2":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1167);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Shod Staff 3":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1168);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Shod Staff 4":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Friar") || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 1169);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Claw Staff":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 828);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Bishop Staff":
                    if ((player.CharacterClass.Name == "Friar"))
                    {
                        changeModel(player, 881);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Spiked Staff":
                    if ((player.CharacterClass.Name == "Friar"))
                    {
                        changeModel(player, 882);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Mage Staff":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 19);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Mage Staff 2":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        changeModel(player, 327);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Mage Staff 3":
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                    changeModel(player, 468);
                    break;
                                                                }
                    else
                    {
                        break;
                    }

                case "Quarterstaff":
                    if ((player.CharacterClass.Name == "Friar"))
                    {
                        changeModel(player, 442);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "QuarterShod Staff":
                    if ((player.CharacterClass.Name == "Friar"))
                    {
                        changeModel(player, 567);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "ToA Staff":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Eldritch") || (player.CharacterClass.Name == "Enchanter") || (player.CharacterClass.Name == "Mentalist")
                        || (player.CharacterClass.Name == "Cabalist") || (player.CharacterClass.Name == "Sorcerer") || (player.CharacterClass.Name == "Theurgist") || (player.CharacterClass.Name == "Wizard") || (player.CharacterClass.Name == "Friar")
                        || (player.CharacterClass.Name == "Runemaster") || (player.CharacterClass.Name == "Spiritmaster") || (player.CharacterClass.Name == "Bonedancer"))
                    {
                        SendReply(player, "[Traldor's Oracle]\n" +
                                            "[Tartaros Gift]\n" +
                                            "[Staff of the God]\n");
                        break;
                    }

                    else if ((player.CharacterClass.Name == "Friar"))
                    {
                        SendReply(player, "[Traldor's Oracle]\n" +
                                            "[Tartaros Gift]\n" +
                                            "[Staff of the God]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Traldor's Oracle":
                    changeModel(player, 1658);
                    break;

                case "Tartaros Gift":
                    changeModel(player, 1659);
                    break;

                case "Staff of the God":
                    changeModel(player, 1660);
                    break;

                #endregion Staff

                #region Polearm
                case "Epic Slash Polearm":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3297);
                    break;

                case "Epic Thrust Polearm":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3299);
                    break;

                case "Epic Crush Polearm":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3298);
                    break;

                case "ToA Slash Polearm":
                    SendReply(player, "[Trident]\n");
                    break;

                case "Trident":
                    changeModel(player, 2191);
                    break;

                case "ToA Thrust Polearm":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman"))
                    {
                        SendReply(player, "[Spear of Kings]\n" +
                                        "[Trident]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Spear of Kings":
                    changeModel(player, 1661);
                    break;

                case "ToA Crush Polearm":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman"))
                    {
                        SendReply(player, "[Trident]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Classic Crush Polearm":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman"))
                    {
                        SendReply(player, "[Footman's Pick]\n" +
                                        "[Military Fork]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Classic Thrust Polearm":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman"))
                    {
                        SendReply(player, "[Footman's Pick]\n" +
                                        "[Military Fork]\n" +
                                        "[Air Military Fork]\n" +
                                        "[Earth Military Fork]\n" +
                                        "[Fire Military Fork]\n" +
                                        "[Water Military Fork]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Classic Slash Polearm":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman"))
                    {
                        SendReply(player, "[Footman's Pick]\n" +
                                        "[Military Fork]\n" +
                                        "[Air Military Fork]\n" +
                                        "[Earth Military Fork]\n" +
                                        "[Fire Military Fork]\n" +
                                        "[Water Military Fork]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Footman's Pick":
                    if ((player.CharacterClass.Name == "Armsman"))
                    {
                        changeModel(player, 871);
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Military Fork":
                    if ((player.CharacterClass.Name == "Armsman"))
                    {
                        changeModel(player, 872);
                        break;
                    }
                    else
                    {
                        break;
                    }

                #endregion Polearm

                #region Large Weapon
                case "Epic Slash LargeWeapon":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 3259);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 3254);

                    break;

                /*case "Epic Thrust LargeWeapon":
                    // Hibernia -- N/A ATM
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 3261 );
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 3261);

                    break;*/

                case "Epic Crush LargeWeapon":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 3260);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 3255);

                    break;

                case "ToA Slash LargeWeapon":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Champion"))
                    {
                        SendReply(player, "[Malice's Axe]\n" +
                                        "[Bane of Battler]\n" +
                                        "[2h Katana]\n" +
                                        "[2h Kopesh]\n" +
                                        "[2h Aerus Sword]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "2h Katana":
                    changeModel(player, 2208);
                    break;

                case "2h Kopesh":
                    changeModel(player, 2196);
                    break;

                case "2h Aerus Sword":
                    changeModel(player, 2204);
                    break;

                case "Malice's Axe":
                    changeModel(player, 2110);
                    break;

                case "Bane of Battler":
                    changeModel(player, 1670);
                    break;

                /*case "ToA Thrust LargeWeapon":
                    // Hibernia -- N/A ATM
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Champion"))
                    {
                        SendReply(player, "[Spear of Kings]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                    break;*/

                case "ToA Crush LargeWeapon":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Champion"))
                    {
                        SendReply(player, "[Malice's Axe]\n" +
                                        "[Bane of Battler]\n" +
                                        "[Bruiser]\n" +
                                        "[2h Aerus Mace]\n" +
                                        "[2h Magma Hammer]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "2h Aerus Mace":
                    changeModel(player, 2206);
                    break;

                case "2h Magma Hammer":
                    changeModel(player, 2215);
                    break;

                case "Bruiser":
                    changeModel(player, 2113);
                    break;

                case "Classic Slash LargeWeapon":
                    // Albion
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Champion"))
                    {
                        SendReply(player, "[Great Sword LW]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Great Sword LW":
                    changeModel(player, 907);
                    break;

                case "Classic Crush LargeWeapon":
                    // Albion
                    if ((player.CharacterClass.Name == "Hero") || (player.CharacterClass.Name == "Champion"))
                    {
                        SendReply(player, "[Great Hammer LW]\n" +
                                        "[Shod Shilelagh LW]\n" +
                                        "[Sledge Hammer LW]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Great Hammer LW":
                    changeModel(player, 908);
                    break;

                case "Shod Shilelagh LW":
                    changeModel(player, 912);
                    break;

                case "Sledge Hammer LW":
                    changeModel(player, 640);
                    break;

                #endregion Large Weapon

                #region Scythe
                case "Epic Scythe":
                    // Hibernia
                    if (player.CharacterClass.Name == "Valewalker") changeModel(player, 3231);

                    break;

                case "Classic Scythe":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Valewalker"))
                    {
                        SendReply(player, "[Scythe]\n" +
                                        "[Great Scythe]\n" +
                                        "[Great War Scythe]\n" +
                                        "[Harvest Scythe]\n" +
                                        "[Martial Scythe]\n" +
                                        "[War Scythe]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }
                case "Scythe":
                    changeModel(player, 927);
                    break;

                case "Great Scythe":
                    changeModel(player, 926);
                    break;

                case "Great War Scythe":
                    changeModel(player, 928);
                    break;

                case "Harvest Scythe":
                    changeModel(player, 929);
                    break;
                    
                case "Martial Scythe":
                    changeModel(player, 930);
                    break;

                case "War Scythe":
                    changeModel(player, 932);
                    break;

                case "ToA Scythe":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Valewalker"))
                    {
                        SendReply(player, "[Spear of Kings]\n" +
                                        "[Snakecharmer's Scythe]\n" +
                                        "[Magma Scythe]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Snakecharmer's Scythe":
                    changeModel(player, 2111);
                    break;

                case "Magma Scythe":
                    changeModel(player, 2213);
                    break;

                #endregion Scythe

                #region Celtic Spear
                case "Epic Celtic Spear":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 3263);

                    break;

                case "Classic Celtic Spear":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Hero"))
                    {
                        SendReply(player, "[Celtic Spear]\n" +
                                        "[Hooked Celtic Spear]\n" +
                                        "[Barbed Celtic Spear]\n" +
                                        "[Long Celtic Spear]\n" +
                                        "[War Celtic Spear]\n" +
                                        "[Short Celtic Spear]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }
                case "Celtic Spear":
                    changeModel(player, 556);
                    break;

                case "Hooked Celtic Spear":
                    changeModel(player, 642);
                    break;

                case "Barbed Celtic Spear":
                    changeModel(player, 475);
                    break;

                case "Long Celtic Spear":
                    changeModel(player, 476);
                    break;

                case "War Celtic Spear":
                    changeModel(player, 477);
                    break;

                case "Short Celtic Spear":
                    changeModel(player, 470);
                    break;

                case "ToA Celtic Spear":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Hero"))
                    {
                        SendReply(player, "[Spear of Kings]\n" +
                                        "[Trident]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                #endregion Celtic Spear

                #region Midgard Spear
                case "Epic Slash Midgard Spear":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 3320);

                    break;

                case "Epic Thrust Midgard Spear":
                    // Hibernia
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 3319);

                    break;

                case "Classic Slash Midgard Spear":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Hero"))
                    {
                        SendReply(player, "[Midgard Spear]\n" +
                                        "[Long Midgard Spear]\n" +
                                        "[Bill Midgard Spear]\n" +
                                        "[Big Midgard Spear]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Classic Thrust Midgard Spear":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Hero"))
                    {
                        SendReply(player, "[Midgard Spear]\n" +
                                        "[Long Midgard Spear]\n" +
                                        "[Bill Midgard Spear]\n" +
                                        "[Big Midgard Spear]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Midgard Spear":
                    changeModel(player, 328);
                    break;

                case "Long Midgard Spear":
                    changeModel(player, 329);
                    break;

                case "Bill Midgard Spear":
                    changeModel(player, 331);
                    break;

                case "Big Midgard Spear":
                    changeModel(player, 332);
                    break;

                case "ToA Slash Midgard Spear":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Hunter"))
                    {
                        SendReply(player, "[Spear of Kings]\n" +
                                        "[Trident]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "ToA Thrust Midgard Spear":
                    // Hibernia
                    if ((player.CharacterClass.Name == "Hunter"))
                    {
                        SendReply(player, "[Spear of Kings]\n" +
                                        "[Trident]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                #endregion Midgard Spear

                #region Albion TwoHand
                case "Epic Slash TwoHand":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3300);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 3306);

                    break;

                case "Epic Thrust TwoHand":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3301);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 3307);

                    break;

                case "Epic Crush TwoHand":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3302);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 3308);

                    break;

                case "ToA Slash TwoHand":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        SendReply(player, "[Malice's Axe]\n" +
                                        "[Bane of Battler]\n" +
                                        "[2h Katana]\n" +
                                        "[2h Kopesh]\n" +
                                        "[2h Aerus Sword]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "ToA Thrust TwoHand":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        SendReply(player, "[Spear of Kings]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "ToA Crush TwoHand":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        SendReply(player, "[Malice's Axe]\n" +
                                        "[Bane of Battler]\n" +
                                        "[Bruiser]\n" +
                                        "[2h Aerus Mace]\n" +
                                        "[2h Magma Hammer]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "Classic Slash TwoHand":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        SendReply(player, "[2h Slash]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "2h Slash":
                    changeModel(player, 845);
                    break;

                case "Classic Thrust TwoHand":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        SendReply(player, "[2h Thrust]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "2h Thrust":
                    changeModel(player, 846);
                    break;

                case "Classic Crush TwoHand":
                    // Albion
                    if ((player.CharacterClass.Name == "Armsman") || (player.CharacterClass.Name == "Paladin"))
                    {
                        SendReply(player, "[2h Blunt]\n");
                        break;
                    }
                    else
                    {
                        break;
                    }

                case "2h Blunt":
                    changeModel(player, 842);
                    break;

                #endregion Albion TwoHand

                #region Midgard TwoHand
                case "Epic 2h Sword":
                    // Midgard
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 3356);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 3326);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 3318);
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 3332);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 3314);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 3344);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 3350);

                    break;

                case "Epic 2h Axe":
                    // Midgard
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 3352);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 3322);
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 3328);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 3316);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 3340);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 3346);

                    break;

                case "Epic 2h Hammer":
                    // Midgard
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 3354);
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 3324);
                    if (player.CharacterClass.Name == "Healer") changeModel(player, 3336);
                    if (player.CharacterClass.Name == "Shaman") changeModel(player, 3338);
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 3330);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 3342);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 3348);

                    break;

                case "ToA 2h Sword":

                    SendReply(player, "[Malice's Axe]\n" +
                                        "[Bane of Battler]\n" +
                                        "[2h Katana]\n" +
                                        "[2h Kopesh]\n" +
                                        "[2h Aerus Sword]\n");
                    break;

                case "ToA 2h Axe":

                    SendReply(player, "[Malice's Axe]\n" +
                                        "[Magma Axe]\n");
                    break;

                case "Magma Axe":
                    changeModel(player, 2217);
                    break;

                case "ToA 2h Hammer":

                    SendReply(player, "[Malice's Axe]\n" +
                                        "[Bane of Battler]\n" +
                                        "[Bruiser]\n" +
                                        "[2h Aerus Mace]\n" +
                                        "[2h Magma Hammer]\n");
                    break;

                case "Classic 2h Sword":
   
                    SendReply(player, "[2h Slash]\n" +
                                        "[Dwarven Great Sword]\n" +
                                        "[Kobold Great Sword]\n" +
                                        "[Troll Great Sword]\n");
                    break;

                case "Dwarven Great Sword":
                    changeModel(player, 658);
                    break;

                case "Kobold Great Sword":
                    changeModel(player, 1032);
                    break;

                case "Troll Great Sword ":
                    changeModel(player, 1035);
                    break;

                case "Classic 2h Axe":

                    SendReply(player, "[2h Battle Axe]\n" +
                                        "[2h Pick Axe]\n" +
                                        "[2h Great Axe]\n" +
                                        "[2h Great Axe2]\n" +
                                        "[2h Great Axe3]\n");
                    break;

                case "2h Battle Axe":
                    changeModel(player, 2985);
                    break;

                case "2h Pick Axe":
                    changeModel(player, 2983);
                    break;

                case "2h Great Axe":
                    changeModel(player, 1033);
                    break;

                case "2h Great Axe2":
                    changeModel(player, 1030);
                    break;
                    
                case "2h Great Axe3":
                    changeModel(player, 1027);
                    break;

                case "Classic 2h Hammer":

                    SendReply(player, "[2h Blunt]\n");
                    break;

                #endregion Midgard TwoHand

                #endregion Two Hand

                #region One Hand

                #region Alb 1h Crush Weapon
                case "Epic 1h Crush":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3294);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 3303);
                    if (player.CharacterClass.Name == "Cleric") changeModel(player, 3282);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 3283);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 3289);

                    break;

                case "Classic 1h Crush":

                    SendReply(player, "[Mace]\n" +
                                    "[Mace 2]\n" +
                                    "[Mace 3]\n" +
                                    "[Bishops Mace]\n" +
                                    "[Flanged Mace]\n");
                    break;

                case "Mace":
                    changeModel(player, 13);
                    break;

                case "Mace 2":
                    changeModel(player, 647);
                    break;

                case "Mace 3":
                    changeModel(player, 853);
                    break;

                case "Bishops Mace":
                    changeModel(player, 854);
                    break;

                case "Flanged Mace":
                    changeModel(player, 14);
                    break;

                case "ToA 1h Crush":
                    SendReply(player, "[1h Bruiser]\n" +
                                    "[1h Malice's Axe]\n" +
                                    "[1h Bane of Battler]\n" +
                                    "[Scepter of the Meritorious]\n" +
                                    "[Scepter Mace]\n" +
                                    "[1h Magma Hammer]\n" +
                                    "[1h Aerus Mace]\n");
                    break;

                case "1h Bruiser":
                    changeModel(player, 1671);
                    break;

                case "1h Malice's Axe":
                    changeModel(player, 2109);
                    break;

                case "Scepter of the Meritorious":
                    changeModel(player, 1672);
                    break;

                case "1h Bane of Battler":
                    changeModel(player, 2112);
                    break;

                case "Scepter Mace":
                    changeModel(player, 2198);
                    break;

                case "1h Aerus Mace":
                    changeModel(player, 2205);
                    break;

                case "1h Magma Hammer":
                    changeModel(player, 2214);
                    break;

                #endregion Alb 1h Crush Weapon

                #region Alb 1h Slash Weapon
                case "Epic 1h Slash":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3295);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 3304);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 3284);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 3290);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 3269);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 3273);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 3276);

                    break;

                case "Classic 1h Slash":

                    SendReply(player, "[Short Sword]\n" +
                                    "[Long Sword]\n" +
                                    "[Broad Sword]\n" +
                                    "[Bastard Sword]\n");
                    break;

                case "Short Sword":
                    changeModel(player, 3);
                    break;

                case "Long Sword":
                    changeModel(player, 4);
                    break;

                case "Broad Sword":
                    changeModel(player, 5);
                    break;

                case "Bastard Sword":
                    changeModel(player, 10);
                    break;

                case "ToA 1h Slash":
                    SendReply(player, "[1h Malice's Axe]\n" +
                                    "[1h Bane of Battler]\n" +
                                    "[Traitor's Dagger]\n" +
                                    "[1h Aerus Sword]\n");
                    break;

                case "Traitor's Dagger":
                    changeModel(player, 1668);
                    break;

                case "1h Aerus Sword":
                    changeModel(player, 2203);
                    break;

                #endregion Alb 1h Slash Weapon

                #region Alb 1h Thrust Weapon
                case "Epic 1h Thrust":
                    // Albion
                    if (player.CharacterClass.Name == "Armsman") changeModel(player, 3296);
                    if (player.CharacterClass.Name == "Paladin") changeModel(player, 3305);
                    if (player.CharacterClass.Name == "Mercenary") changeModel(player, 3285);
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 3291);
                    if (player.CharacterClass.Name == "Infiltrator") changeModel(player, 3270);
                    if (player.CharacterClass.Name == "Scout") changeModel(player, 3274);
                    if (player.CharacterClass.Name == "Minstrel") changeModel(player, 3277);

                    break;

                case "Classic 1h Thrust":

                    SendReply(player, "[Dagger]\n" +
                                    "[Curved Dagger]\n" +
                                    "[Parrying Dagger]\n" +
                                    "[Duelists Dagger]\n" +
                                    "[Parrying Rapier]\n" +
                                    "[Duelists Rapier]\n");
                    break;

                case "Dagger":
                    changeModel(player, 944);
                    break;

                case "Curved Dagger":
                    changeModel(player, 457);
                    break;

                case "Duelists Dagger":
                    changeModel(player, 885);
                    break;

                case "Duelists Rapier":
                    changeModel(player, 886);
                    break;

                case "Parrying Dagger":
                    changeModel(player, 887);
                    break;

                case "Parrying Rapier":
                    changeModel(player, 888);
                    break;

                case "ToA 1h Thrust":
                    SendReply(player, "[Crocodile's Tooth Dagger]\n" +
                                    "[1h Golden Spear]\n" +
                                    "[1h Spear of Kings]\n");
                    break;

                case "Crocodile's Tooth Dagger ":
                    changeModel(player, 1669);
                    break;

                case "1h Golden Spear":
                    changeModel(player, 1807);
                    break;

                case "1h Spear of Kings":
                    changeModel(player, 2468);
                    break;

                #endregion Alb 1h Thrust Weapon

                #region Alb 1h Flex Weapon
                case "Epic Thrust Flex":
                    // Albion
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 3292);

                    break;

                case "Epic Crush Flex":
                    // Albion
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 3292);

                    break;

                case "Epic Slash Flex":
                    // Albion
                    if (player.CharacterClass.Name == "Reaver") changeModel(player, 3292);

                    break;

                case "Classic Thrust Flex":

                    SendReply(player, "[Chain Whip]\n" +
                                    "[Spiked Whip]\n");
                    break;

                case "Classic Slash Flex":

                    SendReply(player, "[Chain Whip]\n" +
                                    "[Spiked Whip]\n");
                    break;

                case "Chain Whip":
                    changeModel(player, 859);
                    break;

                case "Spiked Whip":
                    changeModel(player, 865);
                    break;

                case "Classic Crush Flex":

                    SendReply(player, "[Chain Flail]\n" +
                                    "[Spiked Flail]\n");
                    break;

                case "Chain Flail":
                    changeModel(player, 858);
                    break;

                case "Spiked Flail":
                    changeModel(player, 864);
                    break;

                case "ToA Thrust Flex":
                    SendReply(player, "[Snakecharmer's Flex]\n");
                    break;

                case "ToA Crush Flex":
                    SendReply(player, "[Snakecharmer's Flex]\n");
                    break;

                case "ToA Slash Flex":
                    SendReply(player, "[Snakecharmer's Flex]\n");
                    break;

                case "Snakecharmer's Flex":
                    changeModel(player, 2119);
                    break;

                #endregion Alb 1h Flex Weapon

                #region Mid 1h Sword Weapon
                case "Epic 1h Sword":
                    // Midgard
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 3325);
                    if (player.CharacterClass.Name == "Hunter") changeModel(player, 3317);
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 3331);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 3313);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 3343);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 3349);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 3355);

                    break;

                case "Classic 1h Sword":

                    SendReply(player, "[Mid Short Sword]\n" +
                                    "[Mid Long Sword]\n" +
                                    "[Mid Broad Sword]\n" +
                                    "[Mid Bastard Sword]\n" +
                                    "[Dwarven Short Sword]\n" +
                                    "[Kobold Long Sword]\n" +
                                    "[Kobold Short Sword]\n" +
                                    "[Troll Broad Sword]\n" +
                                    "[Troll Short Sword]\n");
                    break;

                case "Mid Short Sword":
                    changeModel(player, 311);
                    break;

                case "Mid Long Sword":
                    changeModel(player, 310);
                    break;

                case "Mid Broad Sword":
                    changeModel(player, 312);
                    break;

                case "Mid Bastard Sword":
                    changeModel(player, 313);
                    break;

                case "Dwarven Short Sword":
                    changeModel(player, 655);
                    break;

                case "Kobold Long Sword":
                    changeModel(player, 1015);
                    break;

                case "Kobold Short Sword":
                    changeModel(player, 1017);
                    break;

                case "Troll Broad Sword":
                    changeModel(player, 1020);
                    break;

                case "Troll Short Sword":
                    changeModel(player, 1024);
                    break;

                case "ToA 1h Sword":
                    SendReply(player, "[1h Malice's Axe]\n" +
                                    "[1h Bane of Battler]\n" +
                                    "[Traitor's Dagger]\n" +
                                    "[1h Aerus Sword]\n");
                    break;

                #endregion Mid 1h Sword Weapon

                #region Mid 1h Hammer Weapon
                case "Epic 1h Hammer":
                    // Midgard
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 3323);
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 3329);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 3341);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 3347);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 3353);

                    break;

                case "Classic 1h Hammer":

                    SendReply(player, "[Great Hammer]\n" +
                                    "[Pick Hammer]\n" +
                                    "[War Hammer]\n");
                    break;


                case "War Hammer":
                    changeModel(player, 322);
                    break;

                case "Pick Hammer":
                    changeModel(player, 323);
                    break;

                case "Great Hammer":
                    changeModel(player, 324);
                    break;

                case "ToA 1h Hammer":
                    SendReply(player, "[1h Bruiser]\n" +
                                    "[1h Malice's Axe]\n" +
                                    "[1h Bane of Battler]\n" +
                                    "[Scepter of the Meritorious]\n" +
                                    "[Scepter Mace]\n" +
                                    "[1h Magma Hammer]\n" +
                                    "[1h Aerus Mace]\n");
                    break;

                #endregion Mid 1h Hammer Weapon

                #region Mid 1h Axe Weapon
                case "Epic 1h Axe":
                    // Midgard
                    if (player.CharacterClass.Name == "Berserker") changeModel(player, 3321);
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 3327);
                    if (player.CharacterClass.Name == "Shadowblade") changeModel(player, 3315);
                    if (player.CharacterClass.Name == "Skald") changeModel(player, 3339);
                    if (player.CharacterClass.Name == "Thane") changeModel(player, 3345);
                    if (player.CharacterClass.Name == "Warrior") changeModel(player, 3351);

                    break;

                case "Classic 1h Axe":

                    SendReply(player, "[Spiked Axe]\n" +
                                    "[Double Axe]\n" +
                                    "[War Axe]\n");
                    break;

                case "War Axe":
                    changeModel(player, 319);
                    break;

                case "Spiked Axe":
                    changeModel(player, 315);
                    break;

                case "Double Axe":
                    changeModel(player, 573);
                    break;

                case "ToA 1h Axe":
                    SendReply(player, "[1h Malice's Axe]\n" +
                                    "[1h Magma Axe]\n");
                    break;

                case "1h Magma Axe":
                    changeModel(player, 2216);
                    break;

                #endregion Mid 1h Axe Weapon

                #region Mid Hand to Hand Weapon
                case "Epic Slash Claw":
                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 3333);

                    break;

                case "Epic Thrust Claw":
                    // Midgard
                    if (player.CharacterClass.Name == "Savage") changeModel(player, 3334);

                    break;

                case "Classic Slash Claw":

                    SendReply(player, "[Moon Claw]\n" +
                                    "[Large Moon Claw]\n" +
                                    "[Great Fang]\n");
                    break;

                case "Classic Thrust Claw":

                    SendReply(player, "[Moon Claw]\n" +
                                    "[Large Moon Claw]\n" +
                                    "[Great Fang]\n");
                    break;

                case "Moon Claw":
                    changeModel(player, 982);
                    break;

                case "Large Moon Claw":
                    changeModel(player, 980);
                    break;

                case "Great Fang":
                    changeModel(player, 970);
                    break;

                case "ToA Slash Claw":
                    SendReply(player, "[Snakecharmer's Claw]\n" +
                                    "[Scorpion Greave]\n");
                    break;

                case "ToA Thrust Claw":
                    SendReply(player, "[Snakecharmer's Claw]\n" +
                                    "[Scorpion Greave]\n");
                    break;

                case "Snakecharmer's Claw":
                    changeModel(player, 2469);
                    break;

                case "Scorpion Greave":
                    changeModel(player, 2197);
                    break;

                #endregion Mid Hand to Hand Weapon

                #region Hib 1h Blade Weapon
                case "Epic 1h Blade":
                    // Hibernia
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 3235);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 3244);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 3251);
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 3256);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 3233);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 3241);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 3249);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 3247);

                    break;

                case "Classic 1h Blade":

                    SendReply(player, "[Short Sword]\n" +
                                    "[Long Sword]\n" +
                                    "[Broad Sword]\n" +
                                    "[Bastard Sword]\n");
                    break;

                case "ToA 1h Blade":
                    SendReply(player, "[1h Malice's Axe]\n" +
                                    "[1h Bane of Battler]\n" +
                                    "[Traitor's Dagger]\n" +
                                    "[1h Aerus Sword]\n");
                    break;

                #endregion Hib 1h Blade Weapon

                #region Hib 1h Pierce Weapon
                case "Epic 1h Pierce":
                    // Hibernia
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 3245);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 3252);
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 3257);
                    if (player.CharacterClass.Name == "Nightshade") changeModel(player, 3234);
                    if (player.CharacterClass.Name == "Ranger") changeModel(player, 3242);

                    break;

                case "Classic 1h Pierce":

                    SendReply(player, "[Dagger]\n" +
                                    "[Curved Dagger]\n" +
                                    "[Curved Dagger]\n" +
                                    "[Parrying Dagger]\n" +
                                    "[Duelists Dagger]\n");
                    break;

                case "ToA 1h Pierce":
                    SendReply(player, "[Crocodile's Tooth Dagger]\n" +
                                    "[1h Golden Spear]\n" +
                                    "[1h Spear of Kings]\n");
                    break;

                #endregion Alb 1h Thrust Weapon

                #region Hib 1h Blunt Weapon
                case "Epic 1h Blunt":
                    // Albion
                    if (player.CharacterClass.Name == "Bard") changeModel(player, 3236);
                    if (player.CharacterClass.Name == "Blademaster") changeModel(player, 3246);
                    if (player.CharacterClass.Name == "Champion") changeModel(player, 3253);
                    if (player.CharacterClass.Name == "Hero") changeModel(player, 3258);
                    if (player.CharacterClass.Name == "Warden") changeModel(player, 3250);
                    if (player.CharacterClass.Name == "Druid") changeModel(player, 3248);

                    break;

                case "Classic 1h Blunt":

                    SendReply(player, "[Great Hammer]\n" +
                                    "[Pick Hammer]\n" +
                                    "[War Hammer]\n");
                    break;

                case "ToA 1h Blunt":
                    SendReply(player, "[1h Bruiser]\n" +
                                    "[1h Malice's Axe]\n" +
                                    "[1h Bane of Battler]\n" +
                                    "[Scepter of the Meritorious]\n" +
                                    "[Scepter Mace]\n" +
                                    "[1h Magma Hammer]\n" +
                                    "[1h Aerus Mace]\n");
                    break;

                #endregion Hib 1h Blunt Weapon

                #endregion One Hand

                #endregion Weapons

                #endregion Model Change
            }

            return true;
        }
        #endregion WhisperReceive Function

        #region SendReply Function
        // This funtion sends some more text to the popup window as a shortcut
        public void SendReply(GamePlayer player, string msg)
        {
            player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
        }
        #endregion SendReply Function

        #region changeModel Function
        // This function changes the Model ID of the Item
        public void changeModel(GamePlayer player, int modelid)
        {
            WeakReference itemWeak = (WeakReference)player.TempProperties.getObjectProperty(ItemModelNPC_ITEM_WEAK, new WeakRef(null));

            player.TempProperties.removeProperty(ItemModelNPC_ITEM_WEAK);

            InventoryItem item = (InventoryItem)itemWeak.Target;

            GameObject tempobj = player.TargetObject;

            #region Check to Make Sure Valid

            #region Shields
            // Shields
            if (item.Object_Type == 42)
            {

                if (!m_shield.Contains(modelid))
                {
                    // Clear Popup Window
                    player.Out.SendChangeTarget(player);
                    player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    player.Out.SendChangeTarget(tempobj);

                    SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                    return;
                }
            }
            #endregion Shields

            #region Two Hand Weapons
            // Two Hand Weapon
            if ((item.Item_Type == 12) || (item.Item_Type == 30))
            {
                // Polearm
                if (item.Object_Type == 7)
                {
                    // Crush
                    if (item.Type_Damage == 1)
                    {
                        if (!m_crushpolearm.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Slash
                    if (item.Type_Damage == 2)
                    {
                        if (!m_slashpolearm.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Thrust
                    if (item.Type_Damage == 3)
                    {
                        if (!m_thrustpolearm.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                }

                // Albion Two Hand
                if (item.Object_Type == 6)
                {        
                    // Crush
                    if (item.Type_Damage == 1)
                    {
                        if (!m_albcrushtwohand.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Slash
                    if (item.Type_Damage == 2)
                    {
                        if (!m_albslashtwohand.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Thrust
                    if (item.Type_Damage == 3)
                    {
                        if (!m_albthrusttwohand.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                }

                // Large Weapon
                if (item.Object_Type == 22)
                {
                    // Crush
                    if (item.Type_Damage == 1)
                    {
                        if (!m_crushlw.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Slash
                    if (item.Type_Damage == 2)
                    {
                        if (!m_slashlw.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }

                }
                // Scythe
                if (item.Object_Type == 26)
                {
                    if (!m_scythe.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Celtic Spear
                if (item.Object_Type == 23)
                {
                    if (!m_cs.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Midgard Spear
                if (item.Object_Type == 14)
                {
                    // Slash
                    if (item.Type_Damage == 2)
                    {
                        if (!m_slashmidgardspear.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Thrust
                    if (item.Type_Damage == 3)
                    {
                        if (!m_thrustmidgardspear.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                }

                // Midgard Two Handed
                if (item.Item_Type == 12)
                {
                    // Sword
                    if (item.Object_Type == 11)
                    {
                        if (!m_midswordtwohand.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Axe
                    if (item.Object_Type == 13)
                    {
                        if (!m_midaxetwohand.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Hammer
                    if (item.Object_Type == 12)
                    {
                        if (!m_midhammertwohand.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                }
            }
            #endregion Two Hand Weapons

            #region One Hand Weapons
            // One Hand Weapon
            if ((item.Item_Type == 10) || ((item.Item_Type == 11) && (item.Object_Type != 42)))
            {
                // Alb 1h Crush Weapon
                if (item.Object_Type == 2)
                {
                    if (!m_1hcrush.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Alb 1h Slash Weapon
                if (item.Object_Type == 3)
                {
                    if (!m_1hslash.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Alb 1h Thrust Weapon
                if (item.Object_Type == 4)
                {
                    if (!m_1hthrust.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Flexible
                if (item.Object_Type == 24)
                {
                    // Crush
                    if (item.Type_Damage == 1)
                    {
                        if (!m_crushflex.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Slash
                    if (item.Type_Damage == 2)
                    {
                        if (!m_slashflex.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Thrust
                    if (item.Type_Damage == 3)
                    {
                        if (!m_thrustflex.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                }

                // Mid 1h Sword Weapon
                if (item.Object_Type == 11)
                {
                    if (!m_1hsword.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Mid 1h Hammer Weapon
                if (item.Object_Type == 12)
                {
                    if (!m_1hhammer.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Mid 1h Axe Weapon
                if (item.Object_Type == 13)
                {
                    if (!m_1haxe.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Hand to Hand
                if (item.Object_Type == 25)
                {
                    // Slash
                    if (item.Type_Damage == 2)
                    {
                        if (!m_slashclaw.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                    // Thrust
                    if (item.Type_Damage == 3)
                    {
                        if (!m_thrustclaw.Contains(modelid))
                        {
                            // Clear Popup Window
                            player.Out.SendChangeTarget(player);
                            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                            player.Out.SendChangeTarget(tempobj);

                            SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                            return;
                        }
                    }
                }

                // Hib 1h Blade Weapon
                if (item.Object_Type == 19)
                {
                    if (!m_1hblade.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Hib 1h Pierce Weapon
                if (item.Object_Type == 21)
                {
                    if (!m_1hpierce.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

                // Hib 1h Blunt Weapon
                if (item.Object_Type == 20)
                {
                    if (!m_1hblunt.Contains(modelid))
                    {
                        // Clear Popup Window
                        player.Out.SendChangeTarget(player);
                        player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                        player.Out.SendChangeTarget(tempobj);

                        SendReply(player, "You have tried to fool me! Try again with a valid model for that type of weapon, " + player.Name + "!");
                        return;
                    }
                }

            }
            #endregion One Hand Weapons

            #endregion Check to Make Sure Valid

            if (ModelChargeType > 1)
            {
                if (player.BountyPoints <= ModelBountyPrice)
                {
                    player.Out.SendMessage("I need " + ModelBountyPrice + " Bounty Points to enchant that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
            }
           else
            {
                if (player.GetCurrentMoney() <= ModelPrice)
                {
                    player.Out.SendMessage("I need " + Money.GetString(ModelPrice) + " to enchant that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
            }

            if (ModelChargeType > 1)
            {
                player.RemoveBountyPoints(ModelBountyPrice);
            }
            else
            {
                player.RemoveMoney(ModelPrice, null);
            }

            if (item.Name.StartsWith(Prefix1) || item.Name.StartsWith(Prefix2))
            {
                ItemTemplate item2 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), item.Id_nb);
                if (item2 != null)
                {
                    item.Name = item2.Name;
                }
            }

            item.Name = Prefix1 + item.Name;
            item.Model = modelid;
            item.IsPickable = false;
            item.IsDropable = false;

            player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });

            // Clear Popup Window
            player.Out.SendChangeTarget(player);
            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            player.Out.SendChangeTarget(tempobj);

            SendReply(player, "My work upon " + item.Name + " is complete. Farewell " + player.Name + "!");
        }
        #endregion changeModel Function

        #region changeExtension Function
        // This function changes the Extension ID of the Item
        public void changeExtension(GamePlayer player, byte extensionid)
        {
            WeakReference itemWeak = (WeakReference)player.TempProperties.getObjectProperty(ItemModelNPC_ITEM_WEAK, new WeakRef(null));

            player.TempProperties.removeProperty(ItemModelNPC_ITEM_WEAK);

            InventoryItem item = (InventoryItem)itemWeak.Target;

            GameObject tempobj = player.TargetObject;

            if (ExtensionChargeType > 1)
            {
                if (player.BountyPoints <= ExtensionBountyPrice)
                {
                    player.Out.SendMessage("I need " + ExtensionBountyPrice + " Bounty Points to enchant that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
            }
            else
            {
                if (player.GetCurrentMoney() <= ExtensionPrice)
                {
                    player.Out.SendMessage("I need " + Money.GetString(ExtensionPrice) + " to enchant that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
            }

            if (ExtensionChargeType > 1)
            {
                player.RemoveBountyPoints(ExtensionBountyPrice);
            }
            else
            {
                player.RemoveMoney(ExtensionPrice, null);
            }

            if (item.Name.StartsWith(Prefix1) || item.Name.StartsWith(Prefix2))
            {
                ItemTemplate item2 = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), item.Id_nb);
                if (item2 != null)
                {
                    item.Name = item2.Name;
                }
            }

            item.Name = Prefix2 + item.Name;
            item.Extension = extensionid;
            item.IsPickable = false;
            item.IsDropable = false;

            player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });

            // Clear Popup Window
            player.Out.SendChangeTarget(player);
            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            player.Out.SendChangeTarget(tempobj);

            SendReply(player, "My work upon " + item.Name + " is complete. Farewell " + player.Name + "!");
        }
        #endregion changeExtension Function


    }
}