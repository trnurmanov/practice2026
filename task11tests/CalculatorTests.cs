using System;
using Xunit;
using task11;

namespace task11tests
{
    public class CalculatorTests
    {

        [Fact]
        public void CreateCalculator_UnusedCodeInside_ShouldStillCompile()
        {
            string codeWithExtras = @"
        public class Calculator
        {
            private string name = ""test"";
            private static int counter = 0;
            
            public int Add(int a, int b) => a + b;
            public int Minus(int a, int b) => a - b;
            public int Mul(int a, int b) => a * b;
            public int Div(int a, int b) => a / b;
            
            private void Helper() { counter++; }
            public string GetName() => name;
        }";

            ICalculator calculator = ClassGenerator.CreateCalculator(codeWithExtras);

            Assert.Equal(-50, calculator.Minus(0, 50));
            Assert.Equal(500, calculator.Mul(100, 5));
        }

        [Fact]
        public void CreateCalculator_SingleLineCode_ShouldCompileCorrectly()
        {
            string oneLiner = "public class Calculator { public int Add(int a, int b) => a + b; public int Minus(int a, int b) => a - b; public int Mul(int a, int b) => a * b; public int Div(int a, int b) => a / b; }";

            ICalculator calculator = ClassGenerator.CreateCalculator(oneLiner);

            Assert.Equal(222, calculator.Add(200, 22));
            Assert.Equal(150, calculator.Mul(3, 50));
        }

        [Fact]
        public void CreateCalculator_CodeWithComments_ShouldIgnoreThemAndCompile()
        {
            string codeWithComments = @"
        // This is a calculator implementation
        public class Calculator
        {
            /// <summary>Adds two numbers</summary>
            public int Add(int a, int b) => a + b;
            
            /* Multi-line
               comment here */
            public int Minus(int a, int b) => a - b;
            
            public int Mul(int a, int b) => a * b; // multiply
            
            // Division method
            public int Div(int a, int b) => a / b;
        }";

            ICalculator calculator = ClassGenerator.CreateCalculator(codeWithComments);

            Assert.Equal(100, calculator.Add(40, 60));
            Assert.Equal(10, calculator.Div(100, 10));
        }

        
    }
}
