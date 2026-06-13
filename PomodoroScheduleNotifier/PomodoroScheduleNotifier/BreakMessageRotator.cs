using System;
using System.Collections.Generic;

namespace PomodoroScheduleNotifier
{
    public readonly record struct BreakMessage(
        string Text,
        string IconGlyph,
        string IconBackground,
        string? IconImageUrl = null);

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
            M("do or do not. there is no try.", "yo", "#463A75", "https://starwars.fandom.com/wiki/Special:Redirect/file/Yoda_SWSB.png"),
            M("i have a bad feeling about this", "!", "#463A75", "https://starwars.fandom.com/wiki/Special:Redirect/file/Millennium_Falcon_Fathead_TROS.png"),
            M("use the force", "sw", "#463A75", "https://starwars.fandom.com/wiki/Special:Redirect/file/YodaForceLift.jpg"),
            M("hello there", "ob", "#463A75", "https://starwars.fandom.com/wiki/Special:Redirect/file/ObiWanHS-SWE.jpg"),
            M("this is the way", "mw", "#463A75", "https://starwars.fandom.com/wiki/Special:Redirect/file/DinDjarinArmor-CGSWG.png"),

            // invincible
            M("think, mark!", "mk", "#475E9E", "https://amazon-invincible.fandom.com/wiki/Special:Redirect/file/Nolan_coalition_fullbod.png"),
            M("are you sure?", "?", "#475E9E", "https://amazon-invincible.fandom.com/wiki/Special:Redirect/file/Invincible_%28Mark_Grayson%29.png"),
            M("stand ready for my arrival, worm", "cq", "#475E9E", "https://amazon-invincible.fandom.com/wiki/Special:Redirect/file/Viltrumite_Conquest.png"),
            M("you don't seem to understand", "om", "#475E9E", "https://amazon-invincible.fandom.com/wiki/Special:Redirect/file/Invincible_%28Mark_Grayson%29.png"),
            M("i can always start again", "8", "#475E9E", "https://amazon-invincible.fandom.com/wiki/Special:Redirect/file/Nolan_coalition_fullbod.png"),

            // cowboy bebop
            M("see you, space cowboy", "cb", "#7A3E3E", "https://cowboybebop.fandom.com/wiki/Special:Redirect/file/Spike_Spiegel_Main.png"),
            M("whatever happens, happens", "wh", "#7A3E3E", "https://cowboybebop.fandom.com/wiki/Special:Redirect/file/Spike_Spiegel_Main.png"),
            M("you're gonna carry that weight", "wt", "#7A3E3E", "https://cowboybebop.fandom.com/wiki/Special:Redirect/file/Bebop_Exterior_Mars.png"),
            M("bang", ".", "#7A3E3E", "https://cowboybebop.fandom.com/wiki/Special:Redirect/file/Spike_Spiegel_Main.png"),

            // breaking bad
            M("say my name", "br", "#2F5A3A", "https://breakingbad.fandom.com/wiki/Special:Redirect/file/BB-S5B-Walt-590.jpg"),
            M("i am the one who knocks", "ww", "#2F5A3A", "https://breakingbad.fandom.com/wiki/Special:Redirect/file/BB-S5B-Walt-590.jpg"),
            M("tread lightly", "tl", "#2F5A3A", "https://breakingbad.fandom.com/wiki/Special:Redirect/file/BB-S5B-Walt-590.jpg"),
            M("yeah, science!", "he", "#2F5A3A", "https://breakingbad.fandom.com/wiki/Special:Redirect/file/Jesse_Season_5B.jpg"),
            M("better call saul", "sa", "#2F5A3A", "https://breakingbad.fandom.com/wiki/Special:Redirect/file/BCS_S6_Portrait_Jimmy.jpg"),

            // arcane
            M("what could have been", "vi", "#6D4A8E", "https://arcane.fandom.com/wiki/Special:Redirect/file/JinxS2End.png"),
            M("in pursuit of great", "hx", "#6D4A8E", "https://arcane.fandom.com/wiki/Special:Redirect/file/AstralViktor.png"),
            M("we'll show them all", "jx", "#6D4A8E", "https://arcane.fandom.com/wiki/Special:Redirect/file/JinxS2End.png"),
            M("you're perfect", "jk", "#6D4A8E", "https://arcane.fandom.com/wiki/Special:Redirect/file/Silco2_Arcane_Render.png"),

            // andor
            M("one way out", "1", "#4E5967", "https://starwars.fandom.com/wiki/Special:Redirect/file/KinoLoy-NL.png"),
            M("never more than twelve", "12", "#4E5967", "https://starwars.fandom.com/wiki/Special:Redirect/file/KinoLoy-NL.png"),
            M("i can't swim", "~", "#4E5967", "https://starwars.fandom.com/wiki/Special:Redirect/file/KinoLoy-NL.png"),
            M("nobody's listening", "nl", "#4E5967", "https://starwars.fandom.com/wiki/Special:Redirect/file/Narkina5PrisonComplexZoomedin.png"),
            M("power doesn't panic", "pw", "#4E5967", "https://starwars.fandom.com/wiki/Special:Redirect/file/LuthenRael-Chrome2023.png"),

            // evangelion
            M("i mustn't run away", "01", "#6A4B7A", "https://evangelion.fandom.com/wiki/Special:Redirect/file/Shinji_Ikari.png"),
            M("get in the robot", "eva", "#6A4B7A", "https://evangelion.fandom.com/wiki/Special:Redirect/file/Evangelion_Unit-01_front1.png"),
            M("congratulations!", "cl", "#6A4B7A", "https://evangelion.fandom.com/wiki/Special:Redirect/file/Congratulations_(EP_26).png"),
            M("baka shinji", "as", "#6A4B7A", "https://evangelion.fandom.com/wiki/Special:Redirect/file/Asuka%27s_Rage.png"),

            // death note
            M("just as planned", "dn", "#3D3D46", "https://deathnote.fandom.com/wiki/Special:Redirect/file/299276.jpg"),
            M("i'll take a potato chip... and eat it!", "pc", "#3D3D46", "https://deathnote.fandom.com/wiki/Special:Redirect/file/Potato_Chip.gif"),
            M("i am justice", "l", "#3D3D46", "https://deathnote.fandom.com/wiki/Special:Redirect/file/299276.jpg"),
            M("delete", "x", "#3D3D46", "https://deathnote.fandom.com/wiki/Special:Redirect/file/Mikami_(blanc_et_noir).JPG"),

            // gaming culture
            M("ready? go!", "go", "#405166", "https://ssb.wiki.gallery/images/b/be/UltimateAnnouncerGO%21.jpg"),
            M("game!", "gg", "#405166", "https://ssb.wiki.gallery/images/f/fe/ScreenKOSmashUltimate.png"),
            M("new challenger approaching", "vs", "#405166", "https://ssb.wiki.gallery/images/8/8f/Challenger%27s_Approach_notification_screen.jpg"),
            M("final smash", "fs", "#405166", "https://ssb.wiki.gallery/images/d/d1/Smash_Ball_%28Super_Smash_Bros._for_Wii_U%29.jpg"),
            M("low health", "hp", "#405166", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Life_Crystal.png"),
            M("stamina low", "st", "#405166", "https://celestegame.fandom.com/wiki/Special:Redirect/file/Madeline_Idle_Animation_(No_Backpack).gif"),
            M("cooldown", "cd", "#405166", "https://ssb.wiki.gallery/images/b/b4/Fs_meter.png"),
            M("campfire", "cf", "#405166", "https://terraria.wiki.gg/wiki/Special:Redirect/file/Campfire.png"),

            // celeste
            M("just breathe", "ce", "#7A4B5F", "https://celestegame.fandom.com/wiki/Special:Redirect/file/Feather.gif"),
            M("you can do this", "mt", "#7A4B5F", "https://celestegame.fandom.com/wiki/Special:Redirect/file/Madeline_Idle_Animation_(No_Backpack).gif"),
            M("this is it, madeline", "ma", "#7A4B5F", "https://celestegame.fandom.com/wiki/Special:Redirect/file/Madeline_Idle_Animation_(No_Backpack).gif"),
            M("strawberry", "sb", "#7A4B5F", "https://celestegame.fandom.com/wiki/Special:Redirect/file/Strawberry.png"),
            M("dash refill", "dr", "#7A4B5F", "https://celestegame.fandom.com/wiki/Special:Redirect/file/DiamondGem.png"),
            M("golden strawberry", "gs", "#7A4B5F", "https://celestegame.fandom.com/wiki/Special:Redirect/file/Golden_Strawberry-1.png"),

            // half-life
            M("rise and shine, mr. freeman", "hl", "#8A562E", "https://combineoverwiki.net/wiki/Special:Redirect/file/G-man_hl2.jpg"),
            M("the right man in the wrong place", "gm", "#8A562E", "https://combineoverwiki.net/wiki/Special:Redirect/file/G-man_hl2.jpg"),
            M("wake up and smell the ashes", "hl", "#8A562E", "https://combineoverwiki.net/wiki/Special:Redirect/file/G-man_hl2.jpg"),
            M("time, dr. freeman?", "t", "#8A562E", "https://combineoverwiki.net/wiki/Special:Redirect/file/G-man_hl2.jpg"),
            M("crowbar", "cb", "#8A562E", "https://combineoverwiki.net/wiki/Special:Redirect/file/Crowbar_first.jpg"),

            // portal
            M("the cake is a lie", "ap", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Portal_Cake.png"),
            M("this was a triumph", "ap", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Glados.png"),
            M("huge success", "ok", "#D07235", "https://theportalwiki.com/wiki/Special:Redirect/file/Glados.png"),
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
            M("save station", "ss", "#6A5742", "https://www.metroidwiki.org/wiki/Special:Redirect/file/Save_Station_mp1_Artwork.jpg"),
            M("energy tank", "et", "#6A5742", "https://www.metroidwiki.org/wiki/Special:Redirect/file/Energy_Tank_(Echoes).png"),
            M("morph ball", "mb", "#6A5742", "https://www.metroidwiki.org/wiki/Special:Redirect/file/Morph_Ball_(Echoes).png"),
            M("power bomb", "pb", "#6A5742", "https://www.metroidwiki.org/wiki/Special:Redirect/file/Power_Bomb_(Echoes).png"),
            M("the baby", "tb", "#6A5742", "https://cdn.wikimg.net/en/metroidwiki/images/f/fe/Baby_attacking_Samus.gif"),

            // avatar: the last airbender
            M("my cabbages!", "cb", "#4F6D50", "https://avatar.fandom.com/wiki/Special:Redirect/file/Cabbage_merchant.png"),
            M("that's rough, buddy", "zk", "#4F6D50", "https://avatar.fandom.com/wiki/Special:Redirect/file/Zuko.png"),
            M("there is no war in ba sing se", "bs", "#4F6D50", "https://avatar.fandom.com/wiki/Special:Redirect/file/Joo_Dee.png"),
            M("leaves from the vine", "iv", "#4F6D50", "https://avatar.fandom.com/wiki/Special:Redirect/file/Iroh_smiling.png"),
            M("yip yip", "ap", "#4F6D50", "https://avatar.fandom.com/wiki/Special:Redirect/file/Appa_flying.png"),
            M("honor!", "hn", "#4F6D50", "https://avatar.fandom.com/wiki/Special:Redirect/file/Zuko.png"),

            // kendrick
            M("be humble", "kl", "#6A4A3C", "https://kendricklamar.fandom.com/wiki/Special:Redirect/file/HUMBLE..jpg"),
            M("we gon' be alright", "al", "#6A4A3C", "https://kendricklamar.fandom.com/wiki/Special:Redirect/file/To_Pimp_a_Butterfly.jpg"),
            M("sit down", "sd", "#6A4A3C", "https://kendricklamar.fandom.com/wiki/Special:Redirect/file/HUMBLE..jpg"),
            M("protecting my soul", "ps", "#6A4A3C", "https://kendricklamar.fandom.com/wiki/Special:Redirect/file/Kendrick_Lamar_2025.jpg"),

            // k-pop demon hunters
            M("golden", "gd", "#8A6B35", "https://kpop-demon-hunters.fandom.com/wiki/Special:Redirect/file/Golden_Cover.png"),
            M("honmoon", "hm", "#8A6B35", "https://kpop-demon-hunters.fandom.com/wiki/Special:Redirect/file/Honmoon.png"),
            M("soda pop", "sp", "#8A6B35", "https://kpop-demon-hunters.fandom.com/wiki/Special:Redirect/file/Soda_Pop_Cover.png"),
            M("takedown", "td", "#8A6B35", "https://kpop-demon-hunters.fandom.com/wiki/Special:Redirect/file/Takedown_Cover.png"),
            M("your idol", "id", "#8A6B35", "https://kpop-demon-hunters.fandom.com/wiki/Special:Redirect/file/Your_Idol_Cover.png"),

            // dune
            M("fear is the mind-killer", "du", "#7A6440", "https://dune.fandom.com/wiki/Special:Redirect/file/Frank-herberts-22dune22-22paul-muaddib-calling-his-first-sandworm22-by-john-schoenherr-1.jpg"),
            M("the spice must flow", "sp", "#7A6440", "https://dune.fandom.com/wiki/Special:Redirect/file/Melange_%28Dune_Enyclopedia%29.webp"),
            M("the sleeper must awaken", "aw", "#7A6440", "https://dune.fandom.com/wiki/Special:Redirect/file/Frank-herberts-22dune22-22paul-muaddib-calling-his-first-sandworm22-by-john-schoenherr-1.jpg"),
            M("walk without rhythm", "wr", "#7A6440", "https://dune.fandom.com/wiki/Special:Redirect/file/Analog-ProphetOfDune-SchoenherrSandworm.png"),
            M("desert power", "dp", "#7A6440", "https://dune.fandom.com/wiki/Special:Redirect/file/Stilgar_and_His_Men_%28by_John_Schoenherr%29.jpg"),

            // lord of the rings
            M("you shall not pass!", "gf", "#5A563E", "https://lotr.fandom.com/wiki/Special:Redirect/file/Gandalf_by_Damiani.png"),
            M("keep it secret. keep it safe.", "gs", "#5A563E", "https://lotr.fandom.com/wiki/Special:Redirect/file/The_One_Ring_on_a_map_of_Middle-earth.jpg"),
            M("what about second breakfast?", "2b", "#5A563E", "https://lotr.fandom.com/wiki/Special:Redirect/file/PippinByMagali.JPG"),
            M("one does not simply walk into mordor", "md", "#5A563E", "https://lotr.fandom.com/wiki/Special:Redirect/file/Boromir,_Venlian.png"),
            M("my precious", "rg", "#5A563E", "https://lotr.fandom.com/wiki/Special:Redirect/file/Gollum,_R_V.jpg"),

            // jurassic park
            M("life finds a way", "jp", "#5E6443", "https://jurassicpark.fandom.com/wiki/Special:Redirect/file/Ian_Malcolm_in_2022_1p.png"),
            M("clever girl", "cg", "#5E6443", "https://jurassicpark.fandom.com/wiki/Special:Redirect/file/Velociraptor_Rebirth.webp"),
            M("must go faster", ">>", "#5E6443", "https://jurassicpark.fandom.com/wiki/Special:Redirect/file/Ember_new_render.png"),
            M("hold on to your butts", "hb", "#5E6443", "https://jurassicpark.fandom.com/wiki/Special:Redirect/file/Ray_Arnold_%28JP%29_Profile.png"),
            M("spared no expense", "$", "#5E6443", "https://jurassicpark.fandom.com/wiki/Special:Redirect/file/John_Hammond_1997_JP.png"),

            // cosmere
            M("life before death", "lb", "#4A5E72", "https://stormlightarchive.fandom.com/wiki/Special:Redirect/file/SoTSAatKR_IS.jpg"),
            M("strength before weakness", "sw", "#4A5E72", "https://stormlightarchive.fandom.com/wiki/Special:Redirect/file/SoTSAatKR_IS.jpg"),
            M("journey before destination", "jd", "#4A5E72", "https://stormlightarchive.fandom.com/wiki/Special:Redirect/file/SoTSAatKR_IS.jpg"),
            M("there's always another secret", "as", "#4A5E72", "https://mistborn.fandom.com/wiki/Special:Redirect/file/Kelsier.png"),
            M("bridge four", "4", "#4A5E72", "https://stormlightarchive.fandom.com/wiki/Special:Redirect/file/B4_decal_navy_73162.1392137430.900.900.jpg"),

            // other
            M("there is no spoon", "sp", "#444444", "https://matrix.fandom.com/wiki/Special:Redirect/file/Spoon_boy.png"),
            M("don't panic", "42", "#444444", "https://hitchhikers.fandom.com/wiki/Special:Redirect/file/TVSeriesTitles.jpg"),
            M("the work is mysterious and important", "sv", "#444444", "https://severance-series.fandom.com/wiki/Special:Redirect/file/MDR-Desks.jpg"),
            M("outie", "ot", "#444444", "https://severance-series.fandom.com/wiki/Special:Redirect/file/MarkID.jpg"),
            M("volition check", "vc", "#444444", "https://discoelysium.fandom.com/wiki/Special:Redirect/file/Portrait_volition.png")
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
            string? iconImageUrl = null)
        {
            return new BreakMessage(text, iconGlyph, iconBackground, iconImageUrl);
        }
    }
}
