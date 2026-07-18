using System;
using Xunit;
using task14;

namespace task14tests
{
    public class IntegralTests
    {
        [Fact]
        public void Test_ConstantFunction_ReturnsCorrectArea()
        {
            double expected = 10.0;
            double result = DefiniteIntegral.Solve(0, 5, x => 2.0, 1e-5, 4);
            Assert.Equal(expected, result, 1e-4);
        }

        [Fact]
        public void Test_ZeroBounds_ReturnsZero()
        {
            double expected = 0.0;
            double result = DefiniteIntegral.Solve(3, 3, x => x * x, 1e-5, 2);
            Assert.Equal(expected, result, 1e-4);
        }

        [Fact]
        public void Test_CubicFunction_MultipleThreads()
        {
            double expected = 0.0;
            double result = DefiniteIntegral.Solve(-2, 2, x => x * x * x, 1e-5, 8);
            Assert.Equal(expected, result, 1e-4);
        }
    }
}
