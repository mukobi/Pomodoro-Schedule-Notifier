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
        double IconFocusY = 0.5);

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
            M("do or do not. there is no try.", "yo", "#463A75", "https://static.wikia.nocookie.net/starwars/images/d/d6/Yoda_SWSB.png/revision/latest?format=original&cb=20150206140125", iconFocusY: 0.35),
            M("i have a bad feeling about this", "!", "#463A75", "https://static.wikia.nocookie.net/starwars/images/e/e2/TFAHanSolo.png/revision/latest?format=original&cb=20160208055002"),
            M("hello there", "ob", "#463A75", "https://static.wikia.nocookie.net/starwars/images/4/4e/ObiWanHS-SWE.jpg/revision/latest?format=original&cb=20111115052816"),
            M("this is the way", "mw", "#463A75", "https://static.wikia.nocookie.net/starwars/images/4/46/DinDjarinArmor-CGSWG.png/revision/latest?format=original&cb=20241206044557", iconFocusY: 0.25),

            // invincible
            M("think, mark!", "mk", "#475E9E", "https://static.wikia.nocookie.net/amazon-invincible/images/8/8d/Nolan_coalition_fullbod.png/revision/latest?format=original&cb=20260510211004", iconFocusY: 0.24),
            M("are you sure?", "?", "#475E9E", "https://static.wikia.nocookie.net/amazon-invincible/images/a/a3/Invincible_%28Mark_Grayson%29.png/revision/latest?format=original&cb=20250717141424", iconFocusY: 0.24),
            M("stand ready for my arrival, worm", "cq", "#475E9E", "https://static.wikia.nocookie.net/amazon-invincible/images/a/af/Viltrumite_Conquest.png/revision/latest?format=original&cb=20260630053731", iconFocusY: 0.22),

            // cowboy bebop
            M("bang", ".", "#7A3E3E", "https://static.wikia.nocookie.net/cowboybebop/images/b/b2/Spike_Spiegel_Main.png/revision/latest?format=original&cb=20250315014957"),

            // breaking bad
            M("i am the one who knocks", "ww", "#2F5A3A", "https://static.wikia.nocookie.net/breakingbad/images/e/e7/BB-S5B-Walt-590.jpg/revision/latest?format=original&cb=20250728222301"),
            M("yeah, science!", "he", "#2F5A3A", "https://static.wikia.nocookie.net/breakingbad/images/c/ca/Jesse_Season_5B.jpg/revision/latest?format=original&cb=20220611094739"),
            M("better call saul", "sa", "#2F5A3A", "https://static.wikia.nocookie.net/breakingbad/images/8/8e/BCS_S6_Portrait_Jimmy.jpg/revision/latest?format=original&cb=20220802210840"),

            // arcane
            M("in pursuit of great", "hx", "#6D4A8E", "https://static.wikia.nocookie.net/arcane/images/e/e2/AstralViktor.png/revision/latest?format=original&cb=20250323005235"),
            M("we'll show them all", "jx", "#6D4A8E", "https://static.wikia.nocookie.net/arcane/images/7/71/JinxS2End.png/revision/latest?format=original&cb=20250420160319"),
            M("you're perfect", "jk", "#6D4A8E", "https://static.wikia.nocookie.net/arcane/images/4/46/Silco2_Arcane_Render.png/revision/latest?format=original&cb=20240524011310"),

            // andor
            M("one way out", "1", "#4E5967", "https://static.wikia.nocookie.net/starwars/images/5/5d/KinoLoy-NL.png/revision/latest?format=original&cb=20221119055504"),
            M("power doesn't panic", "pw", "#4E5967", "https://static.wikia.nocookie.net/starwars/images/4/46/LuthenRael-Chrome2023.png/revision/latest?format=original&cb=20251104054755"),

            // evangelion
            M("i mustn't run away", "01", "#6A4B7A", "https://static.wikia.nocookie.net/evangelion/images/9/92/Shinji_Ikari.png/revision/latest?format=original&cb=20210731041210"),
            M("get in the robot", "eva", "#6A4B7A", "https://static.wikia.nocookie.net/evangelion/images/2/2c/Evangelion_Unit-01_front1.png/revision/latest?format=original&cb=20190520193846"),
            M("congratulations!", "cl", "#6A4B7A", "https://static.wikia.nocookie.net/evangelion/images/c/cf/Congratulations_%28EP_26%29.png/revision/latest?format=original&cb=20121216062238"),
            M("baka shinji", "as", "#6A4B7A", "https://static.wikia.nocookie.net/evangelion/images/1/1f/Asuka%27s_Rage.png/revision/latest?format=original&cb=20190721114525"),

            // death note
            M("just as planned", "dn", "#3D3D46", "https://static.wikia.nocookie.net/deathnote/images/0/05/299276.jpg/revision/latest?format=original&cb=20160609084120"),
            M("delete", "x", "#3D3D46", "https://static.wikia.nocookie.net/deathnote/images/6/6e/Mikami_%28blanc_et_noir%29.JPG/revision/latest?format=original&cb=20160608092329"),

            // gaming culture
            M("ready? go!", "go", "#405166", "https://ssb.wiki.gallery/images/b/be/UltimateAnnouncerGO%21.jpg"),
            M("game!", "gg", "#405166", "https://ssb.wiki.gallery/images/f/fe/ScreenKOSmashUltimate.png"),
            M("new challenger approaching", "vs", "#405166", "https://ssb.wiki.gallery/images/8/8f/Challenger%27s_Approach_notification_screen.jpg"),
            M("final smash", "fs", "#405166", "https://ssb.wiki.gallery/images/d/d1/Smash_Ball_%28Super_Smash_Bros._for_Wii_U%29.jpg"),
            M("low health", "hp", "#405166", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Life_Crystal.png"),
            M("stamina low", "st", "#405166", "https://static.wikia.nocookie.net/celestegame/images/c/c3/Madeline_Idle_Animation_%28No_Backpack%29.gif/revision/latest?format=original&cb=20210819050728"),
            M("cooldown", "cd", "#405166", "https://ssb.wiki.gallery/images/b/b4/Fs_meter.png"),
            M("campfire", "cf", "#405166", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Campfire.png"),

            // celeste
            M("just breathe", "ce", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/b/ba/Feather.gif/revision/latest?format=original&cb=20190112111945"),
            M("you can do this", "mt", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/c/c3/Madeline_Idle_Animation_%28No_Backpack%29.gif/revision/latest?format=original&cb=20210819050728"),
            M("strawberry", "sb", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/6/6d/Strawberry.png/revision/latest?format=original&cb=20200216224654"),
            M("dash refill", "dr", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/e/ef/DiamondGem.png/revision/latest?format=original&cb=20190129213535"),
            M("golden strawberry", "gs", "#7A4B5F", "https://static.wikia.nocookie.net/celestegame/images/b/b9/Golden_Strawberry-1.png/revision/latest?format=original&cb=20200623021535"),

            // half-life
            M("rise and shine, mr. freeman", "hl", "#8A562E", "https://combineoverwiki.net/wiki/Special:Redirect/file/G-man_hl2.jpg"),
            M("crowbar", "cb", "#8A562E", "https://combineoverwiki.net/wiki/Special:Redirect/file/Crowbar_first.jpg"),

            // portal
            M("the cake is a lie", "ap", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_Cake.png"),
            M("this was a triumph", "ap", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Glados.png"),
            M("are you still there?", "tr", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_Turret.png"),
            M("speedy thing goes in, speedy thing comes out", "<>", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_gun.png"),
            M("companion cube", "[]", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_Companion_Cube.png"),

            // terraria
            M("recall potion", "rp", "#476A52", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Recall_Potion.png"),
            M("magic mirror", "mm", "#476A52", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Magic_Mirror.png"),
            M("life crystal", "lc", "#476A52", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Life_Crystal.png"),
            M("mana crystal", "mc", "#476A52", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Mana_Crystal.png"),
            M("potion of return", "pr", "#476A52", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Potion_of_Return.png"),
            M("boss later", "bl", "#476A52", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Eye_of_Cthulhu.png"),

            // metroid
            M("save station", "ss", "#6A5742", "https://cdn.wikimg.net/en/metroidwiki/images/a/a1/Save_Station_mp1_Artwork.jpg"),
            M("energy tank", "et", "#6A5742", "https://cdn.wikimg.net/en/metroidwiki/images/4/40/Energy_Tank_%28Echoes%29.png"),
            M("morph ball", "mb", "#6A5742", "https://cdn.wikimg.net/en/metroidwiki/images/7/76/Morph_Ball_%28Echoes%29.png"),
            M("power bomb", "pb", "#6A5742", "https://cdn.wikimg.net/en/metroidwiki/images/5/56/Power_Bomb_%28Echoes%29.png"),
            M("the baby", "tb", "#6A5742", "https://cdn.wikimg.net/en/metroidwiki/images/f/fe/Baby_attacking_Samus.gif"),

            // avatar: the last airbender
            M("my cabbages!", "cb", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/2/2f/Cabbage_merchant.png/revision/latest?format=original&cb=20140112200908"),
            M("that's rough, buddy", "zk", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/4/4b/Zuko.png/revision/latest?format=original&cb=20180630112142"),
            M("there is no war in ba sing se", "bs", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/1/1f/Joo_Dee.png/revision/latest?format=original&cb=20140422090643"),
            M("leaves from the vine", "iv", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/c/c1/Iroh_smiling.png/revision/latest?format=original&cb=20130626131914"),
            M("yip yip", "ap", "#4F6D50", "https://static.wikia.nocookie.net/avatar/images/6/65/Appa_flying.png/revision/latest?format=original&cb=20140517110636"),

            // kendrick
            M("we gon' be alright", "al", "#6A4A3C", "https://static.wikia.nocookie.net/kendricklamar/images/9/9a/Kendrick_Lamar_2025.jpg/revision/latest?format=original&cb=20250210105907"),

            // k-pop demon hunters
            M("golden", "gd", "#8A6B35", "https://static.wikia.nocookie.net/kpop-demon-hunters/images/3/31/Rumi_Portrait.png/revision/latest?format=original&cb=20250725045033"),
            M("soda pop", "sp", "#8A6B35", "https://static.wikia.nocookie.net/kpop-demon-hunters/images/9/96/Saja_boys_demon.jpg/revision/latest?format=original&cb=20251121193200"),
            M("your idol", "id", "#8A6B35", "https://static.wikia.nocookie.net/kpop-demon-hunters/images/f/f6/Jinu_Headshot_Wendell.jpg/revision/latest?format=original&cb=20260409003106"),

            // dune
            M("fear is the mind-killer", "du", "#7A6440", "https://static.wikia.nocookie.net/dune/images/d/dc/Frank-herberts-22dune22-22paul-muaddib-calling-his-first-sandworm22-by-john-schoenherr-1.jpg/revision/latest?format=original&cb=20190804071135"),
            M("the spice must flow", "sp", "#7A6440", "https://static.wikia.nocookie.net/dune/images/2/2d/Baron_Harkonnen-John_Schoenherr-Illustrated_Dune_%281978%29.jpg/revision/latest?format=original&cb=20250421052559"),
            M("walk without rhythm", "wr", "#7A6440", "https://static.wikia.nocookie.net/dune/images/1/16/Stilgar_and_His_Men_%28by_John_Schoenherr%29.jpg/revision/latest?format=original&cb=20250604023522"),

            // lord of the rings
            M("you shall not pass!", "gf", "#5A563E", "https://static.wikia.nocookie.net/lotr/images/4/47/Gandalf_by_Damiani.png/revision/latest?format=original&cb=20230617183640"),
            M("what about second breakfast?", "2b", "#5A563E", "https://static.wikia.nocookie.net/lotr/images/b/b3/PippinByMagali.JPG/revision/latest?format=original&cb=20190729053305"),
            M("one does not simply walk into mordor", "md", "#5A563E", "https://static.wikia.nocookie.net/lotr/images/9/95/Boromir%2C_Venlian.png/revision/latest?format=original&cb=20230619183907"),
            M("my precious", "rg", "#5A563E", "https://static.wikia.nocookie.net/lotr/images/7/7d/Gollum%2C_R_V.jpg/revision/latest?format=original&cb=20230621011547"),

            // jurassic park
            M("life finds a way", "jp", "#5E6443", "https://static.wikia.nocookie.net/jurassicpark/images/0/0a/Ian_Malcolm_in_2022_1p.png/revision/latest?format=original&cb=20250706044511"),
            M("clever girl", "cg", "#5E6443", "https://static.wikia.nocookie.net/jurassicpark/images/b/b0/Robert_Muldoon_.jpg/revision/latest?format=original&cb=20250529065308"),
            M("hold on to your butts", "hb", "#5E6443", "https://static.wikia.nocookie.net/jurassicpark/images/0/0c/Ray_Arnold_%28JP%29_Profile.png/revision/latest?format=original&cb=20210622173309"),
            M("spared no expense", "$", "#5E6443", "https://static.wikia.nocookie.net/jurassicpark/images/5/50/John_Hammond_1997_JP.png/revision/latest?format=original&cb=20250706034223"),

            // cosmere
            M("life before death", "lb", "#4A5E72", "https://static.wikia.nocookie.net/stormlightarchive/images/8/81/Kaladin_emmgoyer.jpg/revision/latest?format=original&cb=20160901065014", iconFocusY: 0.27),
            M("there's always another secret", "as", "#4A5E72", "https://static.wikia.nocookie.net/mistborn/images/9/92/Kelsier.png/revision/latest?format=original&cb=20161003074932"),
            M("bridge four", "4", "#4A5E72", "https://static.wikia.nocookie.net/stormlightarchive/images/c/c3/B4_decal_navy_73162.1392137430.900.900.jpg/revision/latest?format=original&cb=20140506011016"),

            // other
            M("there is no spoon", "sp", "#444444", "https://static.wikia.nocookie.net/matrix/images/6/63/Spoon_boy.png/revision/latest?format=original&cb=20110124083000"),
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
            double iconFocusY = 0.5)
        {
            return new BreakMessage(text, iconGlyph, iconBackground, iconImageUrl, iconFocusX, iconFocusY);
        }
    }
}
