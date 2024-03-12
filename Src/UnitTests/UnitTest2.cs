using Bb.Process;
using Bb.OpenApiServices;
using Bb.Nugets;
using System.Text;

namespace UnitTests
{

    public class UnitTest2
    {

        public UnitTest2()
        {

        }

        [Fact]
        public void Test1()
        {
            var v = VersionMatcher.Parse("6.1");

            Assert.True(v.Version == new Version(6, 1));

            Assert.True(v.Evaluate(new Version(6, 1)));
            Assert.True(v.Evaluate(new Version(6, 1, 2)));

            Assert.False(v.Version == new Version(6, 0));

        }

        [Fact]
        public void Test()
        {
            var v = VersionMatcher.Parse("[6.1]");
            Assert.True(v.Version == new Version(6, 1));

            Assert.True(v.Evaluate(new Version(6, 1)));

            Assert.False(v.Evaluate(new Version(6, 0)));
            Assert.False(v.Evaluate(new Version(6, 1, 2)));
        }

        [Fact]
        public void Test3()
        {
            var v = VersionMatcher.Parse("6.*");
            Assert.True(v.Version == new Version(6, 0));
            Assert.True(v.Evaluate(new Version(6, 0)));
            Assert.True(v.Evaluate(new Version(6, 1)));
            Assert.True(v.Evaluate(new Version(6, 1, 2)));
            Assert.False(v.Evaluate(new Version(5, 0)));
        }

        [Fact]
        public void Test4()
        {
            // Accepts any version above, but not including 4.1.3. Could be used to guarantee a dependency with a specific bug fix.
            var v = VersionMatcher.Parse("(4.1.3,)");
            Assert.True(v.Version == new Version(4, 1, 3));
            Assert.True(v.Evaluate(new Version(6, 1)));
            Assert.False(v.Evaluate(new Version(4, 1, 3)));
        }

        [Fact]
        public void Test5()
        {
            // Accepts any version up below 5.x, which might be used to prevent pulling in a later version of a dependency that changed its interface. 
            // Accepte toute version inférieure à 5.x, ce qui peut être utilisé pour empêcher l'intégration d'une version ultérieure d'une dépendance dont l'interface a été modifiée.
            var v = VersionMatcher.Parse("(,5.0)");
            Assert.True(v.Version == new Version(5, 0));

            Assert.True(v.Evaluate(new Version(4, 1)));

            Assert.False(v.Evaluate(new Version(5, 2)));
            Assert.False(v.Evaluate(new Version(6, 2)));

        }

        [Fact]
        public void Test6()
        {
            // Accepts any 1.x or 2.x version, but not 0.x or 3.x and higher.
            // Accepte toute version 1.x ou 2.x, mais pas 0.x ou 3.x et plus.
            var v = VersionMatcher.Parse("[1,3)");
            Assert.True(v.Version == new Version(1, 0));
            Assert.True(v.Child.Version == new Version(3, 0));

            Assert.False(v.Evaluate(new Version(0, 2)));
            Assert.False(v.Evaluate(new Version(3, 0)));
            Assert.False(v.Evaluate(new Version(3, 1)));

            Assert.True(v.Evaluate(new Version(1, 0)));
            Assert.True(v.Evaluate(new Version(2, 1)));

        }

        [Fact]
        public void Test7()
        {
            var v = VersionMatcher.Parse("[1.3.2,1.5)");

            Assert.True(v.Version == new Version(1, 3, 2));
            Assert.True(v.Child.Version == new Version(1, 5));

            Assert.False(v.Evaluate(new Version(1, 2)));

            Assert.True(v.Evaluate(new Version(1, 3, 2)));
            Assert.True(v.Evaluate(new Version(1, 3, 3)));

            Assert.False(v.Evaluate(new Version(1, 5)));
            Assert.False(v.Evaluate(new Version(1, 6)));
        }

    }

}