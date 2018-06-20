using System.Collections;
using System.Collections.Generic;
using Infrastructure.NoSQL;
using Xunit;

namespace MicroTests
{
    public class TagsCountUpdaterTest
    {
        [Theory]
        [ClassData(typeof(TagGenerator))]
        public async void Compare(string desc, List<string> start, List<string> end, int added, int removed)
        {
            int addCount = 0;
            int removeCount = 0;

            await TagCountUpdater.Compare(start, end, str => addCount++, str => removeCount++);

            Assert.Equal(added, addCount);
            Assert.Equal(removed, removeCount);
        }

        private class TagGenerator : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] {"both empty", new List<string>(), new List<string>(), 0, 0},
                new object[] {"one new", new List<string>(), new List<string> {"work"}, 1, 0},
                new object[] {"one removed", new List<string> {"work"}, new List<string>(), 0, 1},
                new object[] {"none added/removed", new List<string> {"work"}, new List<string> {"work"}, 0, 0},
                new object[] {"one added and removed", new List<string> {"work"}, new List<string> {"play"}, 1, 1},
                new object[]
                {
                    "one added and removed",
                    new List<string> {"work", "always"},
                    new List<string> {"play", "always"},
                    1,
                    1
                },
                new object[]
                {
                    "two added and removed",
                    new List<string> {"work1", "work2", "always"},
                    new List<string> {"play1", "play2", "always"},
                    2,
                    2
                },
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}