using System;
using System.Collections.Generic;

namespace PomodoroScheduleNotifier
{
    public readonly record struct BreakMessage(
        string Text,
        string IconGlyph,
        string IconBackground,
        string? IconImageUrl = null,
        double IconFocusX = 0.5,
        double IconFocusY = 0.32);

    public sealed class BreakMessageRotator
    {
        private static readonly BreakMessage[] DefaultMessages =
        {
            // general
            M("step away", "->", "#3A3A35"),
            M("go offline", "off", "#3A3A35"),
            M("touch grass", "gr", "#31533B"),
            M("soft reboot", "re", "#3A3A35"),
            M("human mode", "hu", "#3A3A35"),
            M("eyes elsewhere", "eye", "#3A3A35"),
            M("breathe outside", "~", "#3A3A35"),
            M("the desk can wait", "dk", "#3A3A35"),
            M("save point", "sv", "#3A3A35"),
            M("afk", "afk", "#3A3A35"),
            M("pause menu", "ii", "#3A3A35"),
            M("checkpoint reached", "ck", "#3A3A35"),

            // star wars
            M("do or do not. there is no try.", "yo", "#463A75", "https://static.wikia.nocookie.net/starwars/images/f/f7/YodaForceLift.jpg/revision/latest?format=original&cb=20070711224710", iconFocusY: 0.35),
            M("this is where the fun begins", "ak", "#463A75", "https://static.wikia.nocookie.net/starwars/images/5/57/Anakin_Skywalker_SWDR.png/revision/latest?format=original&cb=20190719021014"),
            M("hello there", "ob", "#463A75", "https://static.wikia.nocookie.net/starwars/images/f/f1/Kenobi_faces_Grievous_ROTS.png/revision/latest?format=original&cb=20130215052829", iconFocusX: 0.42),
            M("this is the way", "mw", "#463A75", "https://static.wikia.nocookie.net/starwars/images/4/46/DinDjarinArmor-CGSWG.png/revision/latest?format=original&cb=20241206044557", iconFocusY: 0.25),

            // invincible
            M("think, mark!", "mk", "#475E9E", "https://static.wikia.nocookie.net/amazon-invincible/images/b/be/Omni_Man_and_Mark_at_duty_Invincible_%2848%29.png/revision/latest?format=original&cb=20210409150042"),
            M("are you sure?", "om", "#475E9E", "https://static.wikia.nocookie.net/amazon-invincible/images/d/df/Omni-man_practices_how_to_tells_Mark_the_truth.png/revision/latest?format=original&cb=20250222135547"),
            M("that's the neat part", "om", "#475E9E", "https://static.wikia.nocookie.net/amazon-invincible/images/d/d2/NolanGrayson-render.png/revision/latest?format=original&cb=20260109114450", iconFocusY: 0.16),
            M("look what they need to mimic", "om", "#475E9E", "https://static.wikia.nocookie.net/amazon-invincible/images/8/8c/NolanViltrumite-render.png/revision/latest?format=original&cb=20260102090714", iconFocusY: 0.16),
            M("stand ready for my arrival, worm", "cq", "#475E9E", "https://static.wikia.nocookie.net/amazon-invincible/images/a/af/Viltrumite_Conquest.png/revision/latest?format=original&cb=20260630053731", iconFocusY: 0.22),

            // cowboy bebop
            M("bang", ".", "#7A3E3E", "https://static.wikia.nocookie.net/cowboybebop/images/b/b2/Spike_Spiegel_Main.png/revision/latest?format=original&cb=20250315014957"),

            // breaking bad
            M("i am the one who knocks", "ww", "#2F5A3A", "https://static.wikia.nocookie.net/breakingbad/images/e/e7/BB-S5B-Walt-590.jpg/revision/latest?format=original&cb=20250728222301"),
            M("yeah, science!", "he", "#2F5A3A", "https://static.wikia.nocookie.net/breakingbad/images/c/ca/Jesse_Season_5B.jpg/revision/latest?format=original&cb=20220611094739"),
            M("better call saul", "sa", "#2F5A3A", "https://static.wikia.nocookie.net/breakingbad/images/8/8e/BCS_S6_Portrait_Jimmy.jpg/revision/latest?format=original&cb=20220802210840"),

            // arcane
            M("in pursuit of great", "hx", "#6D4A8E", "https://static.wikia.nocookie.net/arcane/images/e/ef/Viktor_Machine_Herald_Arcane_Render.png/revision/latest?format=original&cb=20241217153206", iconFocusY: 0.2),
            M("we'll show them all", "jx", "#6D4A8E", "https://static.wikia.nocookie.net/arcane/images/7/71/JinxS2End.png/revision/latest?format=original&cb=20250420160319"),
            M("you're perfect", "jk", "#6D4A8E", "https://static.wikia.nocookie.net/arcane/images/8/88/Silco_Alt.jpeg/revision/latest?format=original&cb=20241129022041"),

            // andor
            M("one way out", "1", "#4E5967", "https://static.wikia.nocookie.net/starwars/images/5/5d/KinoLoy-NL.png/revision/latest?format=original&cb=20221119055504"),
            M("power doesn't panic", "pw", "#4E5967", "https://static.wikia.nocookie.net/starwars/images/7/7a/LuthenRael-Andor112.png/revision/latest?format=original&cb=20221215014152"),

            // evangelion
            M("i mustn't run away", "01", "#6A4B7A", "https://static.wikia.nocookie.net/evangelion/images/9/92/Shinji_Ikari.png/revision/latest?format=original&cb=20210731041210", iconFocusY: 0.2),
            M("get in the robot", "eva", "#6A4B7A", "https://static.wikia.nocookie.net/evangelion/images/2/2c/Evangelion_Unit-01_front1.png/revision/latest?format=original&cb=20190520193846", iconFocusY: 0.22),
            M("congratulations!", "cl", "#6A4B7A", "https://static.wikia.nocookie.net/evangelion/images/c/cf/Congratulations_%28EP_26%29.png/revision/latest?format=original&cb=20121216062238"),
            M("baka shinji", "as", "#6A4B7A", "https://static.wikia.nocookie.net/evangelion/images/1/1f/Asuka%27s_Rage.png/revision/latest?format=original&cb=20190721114525"),

            // death note
            M("just as planned", "dn", "#3D3D46", "https://static.wikia.nocookie.net/deathnote/images/9/9c/Light_yagami.jpg/revision/latest?format=original&cb=20210215131239"),
            M("delete", "x", "#3D3D46", "https://static.wikia.nocookie.net/deathnote/images/d/d8/Mikami%27s_Shinigami_Eyes.png/revision/latest?format=original&cb=20170902111153"),

            // gaming culture
            M("ready? go!", "go", "#405166", "https://ssb.wiki.gallery/images/b/be/UltimateAnnouncerGO%21.jpg"),
            M("game!", "gg", "#405166", "https://ssb.wiki.gallery/images/f/fe/ScreenKOSmashUltimate.png"),
            M("new challenger approaching", "vs", "#405166", "https://ssb.wiki.gallery/images/8/8f/Challenger%27s_Approach_notification_screen.jpg"),
            M("final smash", "fs", "#405166", "https://ssb.wiki.gallery/images/d/d1/Smash_Ball_%28Super_Smash_Bros._for_Wii_U%29.jpg"),
            M("low health", "hp", "#405166", "https://terraria.wiki.gg/images/Four_heart_crystals_in_the_Jungle.png?fbe6c6"),
            M("stamina low", "st", "#405166", "https://static.wikia.nocookie.net/celestegame/images/6/6a/Chapter_7_Intro_Screen.png/revision/latest?format=original&cb=20210818170437"),

            // celeste
            M("just breathe", "ce", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/e/ec/Chapter_6_Alternate_Complete_Screen.png/revision/latest?format=original&cb=20210818165753"),
            M("you can do this", "mt", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/4/4a/Madeline.png/revision/latest?format=original&cb=20180516071349"),
            M("strawberry", "sb", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/1/18/Max_strawbs.png/revision/latest?format=original&cb=20190628211824"),
            M("dash refill", "dr", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/3/36/FCBRoom10.png/revision/latest?format=original&cb=20221119142949"),
            M("golden strawberry", "gs", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/0/09/Golden_Ridge.png/revision/latest?format=original&cb=20200202073511"),

            // half-life
            M("rise and shine, mr. freeman", "hl", "#8A562E", "https://static.wikia.nocookie.net/half-life/images/4/41/G-Man_Alyx_Trailer.jpg/revision/latest?format=original&cb=20191122020607&path-prefix=en"),
            M("crowbar", "cb", "#8A562E", "https://combineoverwiki.net/wiki/Special:Redirect/file/Crowbar_first.jpg"),

            // portal
            M("the cake is a lie", "ap", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_Cake.png"),
            M("this was a triumph", "ap", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Glados.png"),
            M("are you still there?", "tr", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_Turret.png"),
            M("speedy thing goes in, speedy thing comes out", "<>", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_gun.png"),
            M("companion cube", "[]", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_Companion_Cube.png"),

            // terraria
            M("the guide has arrived", "gd", "#476A52", "https://static.wikia.nocookie.net/terraria_gamepedia/images/6/65/Terraria_Forest_1.2.png/revision/latest?format=original&cb=20210924203659"),
            M("magic mirror", "mm", "#476A52", "https://terraria.wiki.gg/images/Player_in_Forest.png?cca459"),
            M("you feel an evil presence watching you", "eye", "#476A52", "https://terraria.wiki.gg/images/Two_heart_crystals.png?6bdb35"),
            M("a horrible chill goes down your spine", "ice", "#476A52", "https://terraria.wiki.gg/images/Underground_Cabin_in_Ice_biome.png?a610b5"),
            M("impending doom approaches", "do", "#476A52", "https://terraria.wiki.gg/images/Terraria_Biomes.jpg?11cdcb"),

            // metroid
            M("save station", "ss", "#6A5742", "https://static.wikia.nocookie.net/metroid/images/8/81/M5_art_Samus_01.png/revision/latest?format=original&cb=20210822055726"),
            M("energy tank", "et", "#6A5742", "https://static.wikia.nocookie.net/metroid/images/6/68/Lightsuit.jpg/revision/latest?format=original&cb=20130406125348"),
            M("morph ball", "mb", "#6A5742", "https://static.wikia.nocookie.net/metroid/images/e/ea/M5_art_Samus_06.png/revision/latest?format=original&cb=20211106040601", iconFocusY: 0.18),
            M("power bomb", "pb", "#6A5742", "https://static.wikia.nocookie.net/metroid/images/3/38/Metroid_Dread_Gravity_Suit_full_body.png/revision/latest?format=original&cb=20221031035744"),
            M("the baby", "tb", "#6A5742", "https://static.wikia.nocookie.net/metroid/images/c/cc/MD_Samus_Infected.jpg/revision/latest?format=original&cb=20211121001617"),

            // avatar: the last airbender
            M("my cabbages!", "cb", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/2/2f/Cabbage_merchant.png/revision/latest?format=original&cb=20140112200908"),
            M("that's rough, buddy", "zk", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/4/4b/Zuko.png/revision/latest?format=original&cb=20180630112142"),
            M("there is no war in ba sing se", "bs", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/1/1f/Joo_Dee.png/revision/latest?format=original&cb=20140422090643"),
            M("hope is something you give yourself", "iv", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/9/92/Iroh_makes_tea_for_spirits.png/revision/latest?format=original&cb=20210825235309"),
            M("life is like this dark tunnel", "ir", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/6/68/General_Iroh_planning.png/revision/latest?format=original&cb=20190927164124"),
            M("yip yip", "ap", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/6/65/Appa_flying.png/revision/latest?format=original&cb=20140517110636"),

            // kendrick
            M("we gon' be alright", "al", "#6A4A3C", "https://static.wikia.nocookie.net/kendricklamar/images/9/9a/Kendrick_Lamar_2025.jpg/revision/latest?format=original&cb=20250210105907"),

            // k-pop demon hunters
            M("golden", "gd", "#8A6B35", "https://static.wikia.nocookie.net/kpop-demon-hunters/images/3/31/Rumi_Portrait.png/revision/latest?format=original&cb=20250725045033"),
            M("soda pop", "sp", "#8A6B35", "https://static.wikia.nocookie.net/kpop-demon-hunters/images/9/96/Saja_boys_demon.jpg/revision/latest?format=original&cb=20251121193200"),
            M("your idol", "id", "#8A6B35", "https://static.wikia.nocookie.net/kpop-demon-hunters/images/f/f6/Jinu_Headshot_Wendell.jpg/revision/latest?format=original&cb=20260409003106"),

            // undertale
            M("determination", "dt", "#5A2E45", "https://static.wikia.nocookie.net/undertale/images/a/a9/Determination_screenshot.png/revision/latest?format=original&cb=20160211161851"),
            M("but it refused", "rf", "#5A2E45", "https://static.wikia.nocookie.net/undertale/images/5/55/Determination_soundtrack.png/revision/latest?format=original&cb=20151104164703"),
            M("despite everything", "fr", "#5A2E45", "https://static.wikia.nocookie.net/undertale/images/6/61/Frisk_artwork_Little_Buddy.png/revision/latest?format=original&cb=20160801125429"),

            // dune
            M("fear is the mind-killer", "du", "#7A6440", "https://static.wikia.nocookie.net/dune/images/d/dc/Frank-herberts-22dune22-22paul-muaddib-calling-his-first-sandworm22-by-john-schoenherr-1.jpg/revision/latest?format=original&cb=20190804071135"),
            M("the spice must flow", "sp", "#7A6440", "https://static.wikia.nocookie.net/dune/images/2/2d/Baron_Harkonnen-John_Schoenherr-Illustrated_Dune_%281978%29.jpg/revision/latest?format=original&cb=20250421052559"),
            M("walk without rhythm", "wr", "#7A6440", "https://static.wikia.nocookie.net/dune/images/1/16/Stilgar_and_His_Men_%28by_John_Schoenherr%29.jpg/revision/latest?format=original&cb=20250604023522"),

            // lord of the rings
            M("you shall not pass!", "gf", "#5A563E", "https://static.wikia.nocookie.net/lotr/images/4/47/Gandalf_by_Damiani.png/revision/latest?format=original&cb=20230617183640"),
            M("what about second breakfast?", "2b", "#5A563E", "https://static.wikia.nocookie.net/lotr/images/8/8f/Pippin_and_Merry_05.JPG/revision/latest?format=original&cb=20211013135815"),
            M("one does not simply walk into mordor", "md", "#5A563E", "https://static.wikia.nocookie.net/lotr/images/9/95/Boromir%2C_Venlian.png/revision/latest?format=original&cb=20230619183907"),
            M("my precious", "rg", "#5A563E", "https://static.wikia.nocookie.net/lotr/images/8/84/Gollum_realizes_-_AUJ.jpg/revision/latest?format=original&cb=20131013090039"),

            // jurassic park
            M("life finds a way", "jp", "#5E6443", "https://static.wikia.nocookie.net/jurassicpark/images/d/d1/Malcolm_book_How_The_World_Will_End.png/revision/latest?format=original&cb=20220807003255"),
            M("clever girl", "cg", "#5E6443", "https://static.wikia.nocookie.net/jurassicpark/images/b/b0/Robert_Muldoon_.jpg/revision/latest?format=original&cb=20250529065308"),
            M("hold on to your butts", "hb", "#5E6443", "https://static.wikia.nocookie.net/jurassicpark/images/0/0c/Ray_Arnold_%28JP%29_Profile.png/revision/latest?format=original&cb=20210622173309"),
            M("spared no expense", "$", "#5E6443", "https://static.wikia.nocookie.net/jurassicpark/images/5/50/John_Hammond_1997_JP.png/revision/latest?format=original&cb=20250706034223"),

            // cosmere
            M("life before death", "lb", "#4A5E72", "https://static.wikia.nocookie.net/stormlightarchive/images/f/f9/Kaladin_noText.png/revision/latest?format=original&cb=20230503202145", iconFocusY: 0.27),
            M("there's always another secret", "as", "#4A5E72", "https://static.wikia.nocookie.net/mistborn/images/9/92/Kelsier.png/revision/latest?format=original&cb=20161003074932"),
            M("bridge four", "4", "#4A5E72", "https://static.wikia.nocookie.net/stormlightarchive/images/c/c3/B4_decal_navy_73162.1392137430.900.900.jpg/revision/latest?format=original&cb=20140506011016"),

            // other
            M("there is no spoon", "sp", "#444444", "https://static.wikia.nocookie.net/matrix/images/a/a8/There_is_no_Spoon.jpg/revision/latest?format=original&cb=20130205035913"),
            M("don't panic", "42", "#444444", "https://static.wikia.nocookie.net/hitchhikers/images/a/ae/Arthur_Dent_TV.png/revision/latest?format=original&cb=20230627160818"),
            M("the work is mysterious and important", "sv", "#444444", "https://static.wikia.nocookie.net/severance-series/images/c/cc/2x10-21.jpg/revision/latest?format=original&cb=20250429233125"),
            M("outie", "ot", "#444444", "https://static.wikia.nocookie.net/severance-series/images/6/63/MarkID.jpg/revision/latest?format=original&cb=20230201152254"),
            M("volition check", "vc", "#444444", "https://static.wikia.nocookie.net/discoelysium_gamepedia_en/images/8/83/Portrait_volition.png/revision/latest?format=original&cb=20190719153920")
        };

        private readonly Dictionary<string, BreakMessage> messagesByText;
        private readonly PromptRotator promptRotator;

        public static IReadOnlyList<BreakMessage> StandardMessages => DefaultMessages;

        public BreakMessageRotator()
            : this(DefaultMessages, Random.Shared)
        {
        }

        public BreakMessageRotator(IReadOnlyList<string> messages, Random random)
        {
            List<BreakMessage> breakMessages = new();
            foreach (string message in messages)
            {
                breakMessages.Add(M(message, ".", "#3A3A35"));
            }

            (messagesByText, promptRotator) = CreateRotator(breakMessages, random);
        }

        private BreakMessageRotator(IReadOnlyList<BreakMessage> messages, Random random)
        {
            (messagesByText, promptRotator) = CreateRotator(messages, random);
        }

        public BreakMessage Next()
        {
            return messagesByText[promptRotator.Next()];
        }

        private static (Dictionary<string, BreakMessage> MessagesByText, PromptRotator Rotator) CreateRotator(
            IReadOnlyList<BreakMessage> messages,
            Random random)
        {
            Dictionary<string, BreakMessage> messagesByText = new();
            List<string> messageTexts = new();

            foreach (BreakMessage message in messages)
            {
                messagesByText.Add(message.Text, message);
                messageTexts.Add(message.Text);
            }

            return (messagesByText, new PromptRotator(messageTexts, random));
        }

        private static BreakMessage M(
            string text,
            string iconGlyph,
            string iconBackground,
            string? iconImageUrl = null,
            double iconFocusX = 0.5,
            double iconFocusY = 0.32)
        {
            return new BreakMessage(text, iconGlyph, iconBackground, iconImageUrl, iconFocusX, iconFocusY);
        }
    }
}
