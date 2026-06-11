using System;
using System.Collections.Generic;
using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class PromptRotatorTests
    {
        [Fact]
        public void Next_UsesAllPromptsBeforeRepeating()
        {
            string[] prompts = { "alpha", "beta", "gamma", "delta" };
            PromptRotator rotator = new(prompts, new Random(17));
            HashSet<string> seen = new();

            for (int i = 0; i < prompts.Length; i++)
            {
                Assert.True(seen.Add(rotator.Next()));
            }

            Assert.Equal(prompts.Length, seen.Count);
        }

        [Fact]
        public void Next_AvoidsImmediateRepeatAfterRefill()
        {
            string[] prompts = { "alpha", "beta" };
            PromptRotator rotator = new(prompts, new AlwaysZeroRandom());

            string first = rotator.Next();
            string second = rotator.Next();
            string third = rotator.Next();

            Assert.NotEqual(first, second);
            Assert.NotEqual(second, third);
        }

        [Fact]
        public void Next_KeepsFullDeckAfterRefill()
        {
            string[] prompts = { "alpha", "beta", "gamma" };
            PromptRotator rotator = new(prompts, new Random(23));
            HashSet<string> secondDeck = new();

            for (int i = 0; i < prompts.Length; i++)
            {
                rotator.Next();
            }

            for (int i = 0; i < prompts.Length; i++)
            {
                Assert.True(secondDeck.Add(rotator.Next()));
            }

            Assert.Equal(prompts.Length, secondDeck.Count);
        }

        private sealed class AlwaysZeroRandom : Random
        {
            public override int Next(int maxValue)
            {
                return 0;
            }
        }
    }
}
