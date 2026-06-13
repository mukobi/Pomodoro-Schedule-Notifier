using System;
using System.Collections.Generic;

namespace PomodoroScheduleNotifier
{
    public readonly record struct BreakMessage(string Text, string IconGlyph, string IconBackground);

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
            M("do or do not. there is no try.", "yo", "#463A75"),
            M("i have a bad feeling about this", "!", "#463A75"),
            M("use the force", "sw", "#463A75"),
            M("hello there", "ob", "#463A75"),
            M("this is the way", "mw", "#463A75"),

            // invincible
            M("think, mark!", "mk", "#475E9E"),
            M("are you sure?", "?", "#475E9E"),
            M("stand ready for my arrival, worm", "cq", "#475E9E"),
            M("you don't seem to understand", "om", "#475E9E"),
            M("i can always start again", "8", "#475E9E"),

            // cowboy bebop
            M("see you, space cowboy", "cb", "#7A3E3E"),
            M("whatever happens, happens", "wh", "#7A3E3E"),
            M("you're gonna carry that weight", "wt", "#7A3E3E"),
            M("bang", ".", "#7A3E3E"),

            // breaking bad
            M("say my name", "br", "#2F5A3A"),
            M("i am the one who knocks", "ww", "#2F5A3A"),
            M("tread lightly", "tl", "#2F5A3A"),
            M("yeah, science!", "he", "#2F5A3A"),
            M("better call saul", "sa", "#2F5A3A"),

            // arcane
            M("what could have been", "vi", "#6D4A8E"),
            M("in pursuit of great", "hx", "#6D4A8E"),
            M("we'll show them all", "jx", "#6D4A8E"),
            M("you're perfect", "jk", "#6D4A8E"),

            // andor
            M("one way out", "1", "#4E5967"),
            M("never more than twelve", "12", "#4E5967"),
            M("i can't swim", "~", "#4E5967"),
            M("nobody's listening", "nl", "#4E5967"),
            M("power doesn't panic", "pw", "#4E5967"),

            // evangelion
            M("i mustn't run away", "01", "#6A4B7A"),
            M("get in the robot", "eva", "#6A4B7A"),
            M("congratulations!", "cl", "#6A4B7A"),
            M("baka shinji", "as", "#6A4B7A"),

            // death note
            M("just as planned", "dn", "#3D3D46"),
            M("i'll take a potato chip... and eat it!", "pc", "#3D3D46"),
            M("i am justice", "l", "#3D3D46"),
            M("delete", "x", "#3D3D46"),

            // gaming culture
            M("ready? go!", "go", "#405166"),
            M("game!", "gg", "#405166"),
            M("new challenger approaching", "vs", "#405166"),
            M("final smash", "fs", "#405166"),
            M("low health", "hp", "#405166"),
            M("stamina low", "st", "#405166"),
            M("cooldown", "cd", "#405166"),
            M("campfire", "cf", "#405166"),

            // celeste
            M("just breathe", "ce", "#7A4B5F"),
            M("you can do this", "mt", "#7A4B5F"),
            M("this is it, madeline", "ma", "#7A4B5F"),
            M("strawberry", "sb", "#7A4B5F"),
            M("dash refill", "dr", "#7A4B5F"),
            M("golden strawberry", "gs", "#7A4B5F"),

            // half-life
            M("rise and shine, mr. freeman", "hl", "#8A562E"),
            M("the right man in the wrong place", "gm", "#8A562E"),
            M("wake up and smell the ashes", "hl", "#8A562E"),
            M("time, dr. freeman?", "t", "#8A562E"),
            M("crowbar", "cb", "#8A562E"),

            // portal
            M("the cake is a lie", "ap", "#D07235"),
            M("this was a triumph", "ap", "#D07235"),
            M("huge success", "ok", "#D07235"),
            M("are you still there?", "tr", "#D07235"),
            M("speedy thing goes in, speedy thing comes out", "<>", "#D07235"),
            M("companion cube", "[]", "#D07235"),

            // terraria
            M("recall potion", "rp", "#476A52"),
            M("magic mirror", "mm", "#476A52"),
            M("life crystal", "lc", "#476A52"),
            M("mana crystal", "mc", "#476A52"),
            M("potion of return", "pr", "#476A52"),
            M("boss later", "bl", "#476A52"),

            // metroid
            M("save station", "ss", "#6A5742"),
            M("energy tank", "et", "#6A5742"),
            M("morph ball", "mb", "#6A5742"),
            M("power bomb", "pb", "#6A5742"),
            M("the baby", "tb", "#6A5742"),

            // avatar: the last airbender
            M("my cabbages!", "cb", "#4F6D50"),
            M("that's rough, buddy", "zk", "#4F6D50"),
            M("there is no war in ba sing se", "bs", "#4F6D50"),
            M("leaves from the vine", "iv", "#4F6D50"),
            M("yip yip", "ap", "#4F6D50"),
            M("honor!", "hn", "#4F6D50"),

            // kendrick
            M("be humble", "kl", "#6A4A3C"),
            M("we gon' be alright", "al", "#6A4A3C"),
            M("sit down", "sd", "#6A4A3C"),
            M("protecting my soul", "ps", "#6A4A3C"),

            // k-pop demon hunters
            M("golden", "gd", "#8A6B35"),
            M("honmoon", "hm", "#8A6B35"),
            M("soda pop", "sp", "#8A6B35"),
            M("takedown", "td", "#8A6B35"),
            M("your idol", "id", "#8A6B35"),

            // dune
            M("fear is the mind-killer", "du", "#7A6440"),
            M("the spice must flow", "sp", "#7A6440"),
            M("the sleeper must awaken", "aw", "#7A6440"),
            M("walk without rhythm", "wr", "#7A6440"),
            M("desert power", "dp", "#7A6440"),

            // lord of the rings
            M("you shall not pass!", "gf", "#5A563E"),
            M("keep it secret. keep it safe.", "gs", "#5A563E"),
            M("what about second breakfast?", "2b", "#5A563E"),
            M("one does not simply walk into mordor", "md", "#5A563E"),
            M("my precious", "rg", "#5A563E"),

            // jurassic park
            M("life finds a way", "jp", "#5E6443"),
            M("clever girl", "cg", "#5E6443"),
            M("must go faster", ">>", "#5E6443"),
            M("hold on to your butts", "hb", "#5E6443"),
            M("spared no expense", "$", "#5E6443"),

            // cosmere
            M("life before death", "lb", "#4A5E72"),
            M("strength before weakness", "sw", "#4A5E72"),
            M("journey before destination", "jd", "#4A5E72"),
            M("there's always another secret", "as", "#4A5E72"),
            M("bridge four", "4", "#4A5E72"),

            // other
            M("there is no spoon", "sp", "#444444"),
            M("don't panic", "42", "#444444"),
            M("the work is mysterious and important", "sv", "#444444"),
            M("outie", "ot", "#444444"),
            M("volition check", "vc", "#444444")
        };

        private readonly Dictionary<string, BreakMessage> messagesByText;
        private readonly PromptRotator promptRotator;

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

        private static BreakMessage M(string text, string iconGlyph, string iconBackground)
        {
            return new BreakMessage(text, iconGlyph, iconBackground);
        }
    }
}
