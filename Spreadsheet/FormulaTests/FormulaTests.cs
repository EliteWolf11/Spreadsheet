using SpreadsheetUtilities;

namespace FormulaTests
{
    [TestClass]
    public class FormulaTests
    {
        [TestMethod]
        public void TestDefaultConstructorWithValidFormula()
        {
            string formula = "A1+3";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(formula, f1.ToString());
        }

        [TestMethod]
        public void TestDefaultConstructorWithValidFormulaWithParentheses()
        {
            string formula = "(A1+3)*4";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(formula, f1.ToString());
        }

        [TestMethod]
        public void TestDefaultConstructorWithComplexValidFormula()
        {
            string formula = "_C4  * (A1*2.3/7)  + (3.4-8.0)";
            Formula f1 = new Formula(formula);
            Assert.AreEqual("_C4*(A1*2.3/7)+(3.4-8)", f1.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestDefaultConstructorWithInvalidFormula_BadVariable()
        {
            string formula = "8+8C_";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSyntaxChecker_InvalidStartingToken()
        {
            string formula = "*8.5+2";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSyntaxChecker_TwoValuesBackToBack()
        {
            string formula = "8.27.9+2.0";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSyntaxChecker_TwoOperatorsBackToBack()
        {
            string formula = "A3+*4";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSyntaxChecker_OpeningParenFollowedByOperator()
        {
            string formula = "A3*(*4+2)";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSyntaxChecker_ClosingParenFollowedByOpeningParen()
        {
            string formula = "A3*(4+2)(7+2)";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSyntaxChecker_MoreClosingThanOpening()
        {
            string formula = "A3*((B4+7))) * (x3 +9)))";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSyntaxChecker_LastTokenIsOperator()
        {
            string formula = "A3*9+";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSyntaxChecker_UnequalOpeningAndClosing()
        {
            string formula = "A3*((B4+7) * ((x3 +9))";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestDefaultConstructorWithInvalidFormula_BadVariable2()
        {
            string formula = "8+_c77>";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestDefaultConstructorWithEmptyFormula()
        {
            string formula = " ";
            Formula f1 = new Formula(formula);
        }

        [TestMethod]
        public void TestGetVariablesWithNoDuplicates()
        {
            string formula = "8.7 + A3 - b7";
            Formula f1 = new Formula(formula);
            List<string> list = new List<string>(f1.GetVariables());

            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.Contains("A3"));
            Assert.IsTrue(list.Contains("b7"));
        }

        [TestMethod]
        public void TestGetVariablesWithDuplicates()
        {
            string formula = "8.7 + A3 - b7 + (A3)";
            Formula f1 = new Formula(formula);
            List<string> list = new List<string>(f1.GetVariables());

            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.Contains("A3"));
            Assert.IsTrue(list.Contains("b7"));
        }

        [TestMethod]
        public void TestGetVariablesWithDuplicatesAfterNormalizationToLowercase()
        {
            string formula = "8.7 + A3 - b7 + (a3)";
            Formula f1 = new Formula(formula, s => s.ToLower(), s => true);
            List<string> list = new List<string>(f1.GetVariables());

            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.Contains("a3"));
            Assert.IsTrue(list.Contains("b7"));
        }

        [TestMethod]
        public void TestGetVariablesWithoutNormalizationToLowercase()
        {
            string formula = "8.7 + A3 - b7 + (a3)";
            Formula f1 = new Formula(formula);
            List<string> list = new List<string>(f1.GetVariables());

            Assert.IsTrue(list.Count == 3);
            Assert.IsTrue(list.Contains("A3"));
            Assert.IsTrue(list.Contains("a3"));
            Assert.IsTrue(list.Contains("b7"));
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestValidatorExceptionCatch()
        {
            string formula = "A3+4.3*B9";
            Formula f1 = new Formula(formula, s => s, s => !s.Equals("A3"));
        }

        [TestMethod]
        public void TestEvaluateWithBasicFormula()
        {
            string formula = "8*2+4";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(20.0, f1.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestEvaluateWithBasicFormulaWithVar()
        {
            string formula = "8*2+a3";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(26.0, f1.Evaluate(s => 10));
        }

        [TestMethod]
        public void TestEvaluateWithBadLookup()
        {
            string formula = "8*2+a3";
            Formula f1 = new Formula(formula);
            Assert.IsTrue(f1.Evaluate(s => throw new ArgumentException("No variable found.")).GetType() == typeof(FormulaError));
        }

        [TestMethod]
        public void TestEvaluateWithBasicFormula2()
        {
            string formula = "8*2";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(16.0, f1.Evaluate(s => 0));
        }

        [TestMethod]
        public void TestDivByZero()
        {
            string formula = "8.5/0";
            Formula f1 = new Formula(formula);
            Assert.IsTrue(f1.Evaluate(s => 0).GetType() == typeof(FormulaError));
        }

        [TestMethod]
        public void TestDivByZeroThroughVar()
        {
            string formula = "8.5/a77h5k";
            Formula f1 = new Formula(formula);
            Assert.IsTrue(f1.Evaluate(s => 0).GetType() == typeof(FormulaError));
        }

        [TestMethod]
        public void TestEvaluateWithBasicFormulaWithVars()
        {
            string formula = "5+a4+_u7+H88+10+b3";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(25.0, f1.Evaluate(s => 2.5));
        }

        [TestMethod]
        public void TestEvaluateWithBasicFormulaWithDivision()
        {
            string formula = "6/2.0-2.5";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(0.5, f1.Evaluate(s => 2.5));
        }

        [TestMethod]
        public void TestEvaluateWithBasicParenFormula()
        {
            string formula = "7*(5.0+2.0)";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(49.0, f1.Evaluate(s => 4.0));
        }

        [TestMethod]
        public void TestEvaluateWithBasicParenFormulaThatHasDivByZero()
        {
            string formula = "7/(5.0-5.0)";
            Formula f1 = new Formula(formula);
            Assert.IsTrue(f1.Evaluate(s => 0).GetType() == typeof(FormulaError));
        }

        [TestMethod]
        public void TestEvaluateWithParenFormula()
        {
            string formula = "5.0+((x7-3.0*2.0)-(6.0/3.0))";
            Formula f1 = new Formula(formula);
            Assert.AreEqual(1.0, f1.Evaluate(s => 4.0));
        }

        [TestMethod]
        public void TestEquals()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "5.0  +  6.5  *  (  a4  +  b1  )";
            Formula f2 = new Formula(formula2);

            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod]
        public void TestEqualsWithDiffVars()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "5.0  +  6.5  *  (  a4  +  b2  )";
            Formula f2 = new Formula(formula2);

            Assert.IsFalse(f1.Equals(f2));
        }

        [TestMethod]
        public void TestEqualsWithDiffOperators()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "5.0  +  6.5  *  (  a4  -  b2  )";
            Formula f2 = new Formula(formula2);

            Assert.IsFalse(f1.Equals(f2));
        }

        [TestMethod]
        public void TestEqualsWithDiffDoubles()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "5.3  +  6.5  *  (  a4  -  b2  )";
            Formula f2 = new Formula(formula2);

            Assert.IsFalse(f1.Equals(f2));
        }

        [TestMethod]
        public void TestEqualsWithDoubleVSVariable()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "A10  +  6.5  *  (  a4  -  b2  )";
            Formula f2 = new Formula(formula2);

            Assert.IsFalse(f1.Equals(f2));
        }

        [TestMethod]
        public void TestEqualsWithNull()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string? formula2 = null;

            Assert.IsFalse(f1.Equals(formula2));
        }

        [TestMethod]
        public void TestEqualsWithDiffTypes()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "5.0+6.5*(a4+b1)";

            Assert.IsFalse(f1.Equals(formula2));
        }

        [TestMethod]
        public void TestOverloadedEquals()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "5.0  +  6.5  *  (  a4  +  b1  )";
            Formula f2 = new Formula(formula2);

            Assert.IsTrue(f1==f2);
        }

        [TestMethod]
        public void TestOverloadedEqualsFalse()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "A4  +  6.5  *  (  a4  +  b1  )";
            Formula f2 = new Formula(formula2);

            Assert.IsFalse(f1 == f2);
        }

        [TestMethod]
        public void TestOverloadedNotEquals()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "5.0  +  6.5  *  (  a4  +  b1  )";
            Formula f2 = new Formula(formula2);

            Assert.IsFalse(f1 != f2);
        }

        [TestMethod]
        public void TestOverloadedNotEqualsFalse()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "A4  +  6.5  *  (  a4  +  b1  )";
            Formula f2 = new Formula(formula2);

            Assert.IsTrue(f1 != f2);
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "5.0  +  6.5  *  (  a4  +  b1  )";
            Formula f2 = new Formula(formula2);

            Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
        }

        [TestMethod]
        public void TestGetHashCodeFalse()
        {
            string formula1 = "5.0+6.5*(a4+b1)";
            Formula f1 = new Formula(formula1);
            string formula2 = "A5  +  6.5  *  (  a4  +  b1  )";
            Formula f2 = new Formula(formula2);

            Assert.IsFalse(f1.GetHashCode() == f2.GetHashCode());
        }
    }
}