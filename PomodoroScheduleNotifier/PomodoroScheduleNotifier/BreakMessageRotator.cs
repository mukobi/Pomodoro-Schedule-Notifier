using System;
using System.Collections.Generic;

namespace PomodoroScheduleNotifier
{
    public sealed class BreakMessageRotator
    {
        private static readonly string[] DefaultMessages =
        {
            // general
            "step away",
            "go offline",
            "touch grass",
            "soft reboot",
            "human mode",
            "eyes elsewhere",
            "breathe outside",
            "the desk can wait",
            "save point",
            "afk",
            "pause menu",
            "checkpoint reached",

            // star wars
            "do or do not. there is no try.",
            "i have a bad feeling about this",
            "use the force",
            "hello there",
            "this is the way",

            // invincible
            "think, mark!",
            "are you sure?",
            "stand ready for my arrival, worm",
            "you don't seem to understand",
            "i can always start again",

            // cowboy bebop
            "see you, space cowboy",
            "whatever happens, happens",
            "you're gonna carry that weight",
            "bang",

            // breaking bad
            "say my name",
            "i am the one who knocks",
            "tread lightly",
            "yeah, science!",
            "better call saul",

            // arcane
            "what could have been",
            "in pursuit of great",
            "we'll show them all",
            "you're perfect",

            // andor
            "one way out",
            "never more than twelve",
            "i can't swim",
            "nobody's listening",
            "power doesn't panic",

            // evangelion
            "i mustn't run away",
            "get in the robot",
            "congratulations!",
            "baka shinji",

            // death note
            "just as planned",
            "i'll take a potato chip... and eat it!",
            "i am justice",
            "delete",

            // gaming culture
            "ready? go!",
            "game!",
            "new challenger approaching",
            "final smash",
            "low health",
            "stamina low",
            "cooldown",
            "campfire",

            // celeste
            "just breathe",
            "you can do this",
            "this is it, madeline",
            "strawberry",
            "dash refill",
            "golden strawberry",

            // half-life
            "rise and shine, mr. freeman",
            "the right man in the wrong place",
            "wake up and smell the ashes",
            "time, dr. freeman?",
            "crowbar",

            // portal
            "the cake is a lie",
            "this was a triumph",
            "huge success",
            "are you still there?",
            "speedy thing goes in, speedy thing comes out",
            "companion cube",

            // terraria
            "recall potion",
            "magic mirror",
            "life crystal",
            "mana crystal",
            "potion of return",
            "boss later",

            // metroid
            "save station",
            "energy tank",
            "morph ball",
            "power bomb",
            "the baby",

            // avatar: the last airbender
            "my cabbages!",
            "that's rough, buddy",
            "there is no war in ba sing se",
            "leaves from the vine",
            "yip yip",
            "honor!",

            // kendrick
            "be humble",
            "we gon' be alright",
            "sit down",
            "protecting my soul",

            // k-pop demon hunters
            "golden",
            "honmoon",
            "soda pop",
            "takedown",
            "your idol",

            // dune
            "fear is the mind-killer",
            "the spice must flow",
            "the sleeper must awaken",
            "walk without rhythm",
            "desert power",

            // lord of the rings
            "you shall not pass!",
            "keep it secret. keep it safe.",
            "what about second breakfast?",
            "one does not simply walk into mordor",
            "my precious",

            // jurassic park
            "life finds a way",
            "clever girl",
            "must go faster",
            "hold on to your butts",
            "spared no expense",

            // cosmere
            "life before death",
            "strength before weakness",
            "journey before destination",
            "there's always another secret",
            "bridge four",

            // other
            "there is no spoon",
            "don't panic",
            "the work is mysterious and important",
            "outie",
            "volition check"
        };

        private readonly PromptRotator promptRotator;

        public BreakMessageRotator()
            : this(DefaultMessages, Random.Shared)
        {
        }

        public BreakMessageRotator(IReadOnlyList<string> messages, Random random)
        {
            promptRotator = new PromptRotator(messages, random);
        }

        public string Next()
        {
            return promptRotator.Next();
        }
    }
}