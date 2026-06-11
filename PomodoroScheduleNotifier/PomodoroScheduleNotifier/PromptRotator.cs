using System;
using System.Collections.Generic;

namespace PomodoroScheduleNotifier
{
    public sealed class PromptRotator
    {
        private readonly Random random;
        private readonly List<string> prompts;
        private readonly List<string> remainingPrompts = new();
        private string? lastPrompt;
        private bool justRefilled;

        public PromptRotator(IReadOnlyList<string> prompts, Random random)
        {
            if (prompts.Count == 0)
            {
                throw new ArgumentException("At least one prompt is required.", nameof(prompts));
            }

            this.random = random;
            this.prompts = new List<string>(prompts);
        }

        public string Next()
        {
            if (remainingPrompts.Count == 0)
            {
                Refill();
            }

            int index = GetNextIndex();
            string prompt = remainingPrompts[index];
            remainingPrompts.RemoveAt(index);
            lastPrompt = prompt;
            justRefilled = false;
            return prompt;
        }

        private void Refill()
        {
            remainingPrompts.AddRange(prompts);
            justRefilled = true;
        }

        private int GetNextIndex()
        {
            if (!justRefilled || remainingPrompts.Count <= 1 || lastPrompt == null)
            {
                return random.Next(remainingPrompts.Count);
            }

            int lastPromptIndex = remainingPrompts.IndexOf(lastPrompt);
            if (lastPromptIndex < 0)
            {
                return random.Next(remainingPrompts.Count);
            }

            int index = random.Next(remainingPrompts.Count - 1);
            return index >= lastPromptIndex ? index + 1 : index;
        }
    }
}
