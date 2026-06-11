using System;
using System.Collections.Generic;

namespace PomodoroScheduleNotifier
{
    public sealed class StretchPromptRotator
    {
        private static readonly string[] DefaultPrompts =
        {
            "neck + traps",
            "shoulders + chest",
            "wrists + forearms",
            "hips + glutes",
            "hamstrings + calves",
            "back + spine",
            "ankles + feet",
            "jaw + face"
        };

        private readonly PromptRotator promptRotator;

        public StretchPromptRotator()
            : this(DefaultPrompts, Random.Shared)
        {
        }

        public StretchPromptRotator(IReadOnlyList<string> prompts, Random random)
        {
            promptRotator = new PromptRotator(prompts, random);
        }

        public string Next()
        {
            return promptRotator.Next();
        }
    }
}
