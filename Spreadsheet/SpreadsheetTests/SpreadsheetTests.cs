using SpreadsheetUtilities;
using SS;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestConstructor()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("2x");
        }

        [TestMethod]
        public void TestBasicCells()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "3.5");
            s.SetContentsOfCell("b", "hey");
            s.SetContentsOfCell("c", "77");
            s.SetContentsOfCell("d", "hello there");

            Assert.AreEqual(3.5, s.GetCellContents("a"));
            Assert.AreEqual("hey", s.GetCellContents("b"));
            Assert.AreEqual(77.0, s.GetCellContents("c"));
            Assert.AreEqual("hello there", s.GetCellContents("d"));
        }

        [TestMethod]
        public void TestBasicCellsReplaceContent()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "3.5");
            s.SetContentsOfCell("b", "hey");
            s.SetContentsOfCell("a", "hello");
            s.SetContentsOfCell("b", "5.3");

            Assert.AreEqual("hello", s.GetCellContents("a"));
            Assert.AreEqual(5.3, s.GetCellContents("b"));
        }

        [TestMethod]
        public void TestGetContentOfEmptyCell()
        {
            Spreadsheet s = new Spreadsheet();

            Assert.AreEqual("", s.GetCellContents("a"));
            Assert.AreEqual("", s.GetCellContents("fjlksdf7"));
            Assert.AreEqual("", s.GetCellContents("H63jS"));
            Assert.AreEqual("", s.GetCellContents("D"));
        }

        [TestMethod]
        public void TestCellNameCaseDifference()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A", "2.2");

            Assert.AreEqual("", s.GetCellContents("a"));
            Assert.AreEqual(2.2, s.GetCellContents("A"));
        }

        [TestMethod]
        public void TestGetNonEmptyCells()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "3.5");
            s.SetContentsOfCell("b", "hey");
            s.SetContentsOfCell("c", "77");
            s.SetContentsOfCell("d", "hello there");

            List<string> list = (List<string>)s.GetNamesOfAllNonemptyCells();

            Assert.IsTrue(list.Contains("a"));
            Assert.IsTrue(list.Contains("b"));
            Assert.IsTrue(list.Contains("c"));
            Assert.IsTrue(list.Contains("d"));

            Assert.AreEqual(4, list.Count);

            Assert.IsFalse(list.Contains("A"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestStringCellInvalidName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("2x", "hey");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestDoubleCellInvalidName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("2x", "2.2");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestFormulaCellInvalidName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("2x", "=3*2");
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestBasicFormulaCellsWithCircularException()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "=c+9-b");
            s.SetContentsOfCell("b", "2");
            s.SetContentsOfCell("c", "=a+9");
        }

        [TestMethod]
        public void TestBasicFormulaCells()
        {
            Spreadsheet s = new Spreadsheet();

            List<string> list = (List<string>)s.SetContentsOfCell("a", "=12");
            s.SetContentsOfCell("b", "=a*c");
            s.SetContentsOfCell("c", "=a-4");
            s.SetContentsOfCell("d", "=b/2");
            list = (List<string>)s.SetContentsOfCell("a", "=10");

            Assert.AreEqual(4, list.Count);
            Assert.IsTrue(list.Contains("a"));
            Assert.IsTrue(list.Contains("b"));
            Assert.IsTrue(list.Contains("c"));
            Assert.IsTrue(list.Contains("d"));

            Assert.AreEqual("10", s.GetCellContents("a").ToString());
            Assert.AreEqual("a*c", s.GetCellContents("b").ToString());
            Assert.AreEqual("a-4", s.GetCellContents("c").ToString());
            Assert.AreEqual("b/2", s.GetCellContents("d").ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestBasicFormulaCells2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "=12");
            s.SetContentsOfCell("b", "=a*c");
            s.SetContentsOfCell("c", "=a-4");
            s.SetContentsOfCell("d", "=b/2");
            s.SetContentsOfCell("a", "=c-4");
        }

        //Write some more complicated tests that really stress the system and ensure the correct output.

        [TestMethod]
        public void TestGetDirectDependentsDuplicates()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "12");
            s.SetContentsOfCell("b", "42");
            s.SetContentsOfCell("c", "=a-b");
            s.SetContentsOfCell("d", "=b/2");
            s.SetContentsOfCell("e", "=(a+a)/2");
            s.SetContentsOfCell("f", "=a*15-6");
            s.SetContentsOfCell("g", "=a*c+(2*a)");
            List<string> list = (List<string>)s.SetContentsOfCell("a", "=b+2");

            Assert.AreEqual(5, list.Count);
            Assert.IsTrue(list.Contains("a"));
            Assert.IsTrue(list.Contains("c"));
            Assert.IsTrue(list.Contains("e"));
            Assert.IsTrue(list.Contains("f"));
            Assert.IsTrue(list.Contains("g"));


        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestComplexSetup()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            s.SetContentsOfCell("b", "=a-12*(14/7)");
            s.SetContentsOfCell("c", "=a+2");
            s.SetContentsOfCell("d", "=a*10+(5-3)");
            s.SetContentsOfCell("e", "=b-4");
            s.SetContentsOfCell("f", "=e*c");
            s.SetContentsOfCell("g", "=d+12*2");
            s.SetContentsOfCell("h", "=d-5");
            s.SetContentsOfCell("i", "=f+(j*3-2)");
            s.SetContentsOfCell("j", "=g-6");
            s.SetContentsOfCell("k", "=g/h");
            s.SetContentsOfCell("l", "=i*k+14");

            //This is to test removing old dependencies when converted to a string cell
            s.SetContentsOfCell("m", "=x+2");
            s.SetContentsOfCell("m", "test");

            List<string> list = (List<string>)s.SetContentsOfCell("a", "10");

            //Make sure all Values are accurate
            Assert.AreEqual(10.0, s.GetCellContents("a"));
            Assert.AreEqual("a-12*(14/7)", s.GetCellContents("b").ToString());
            Assert.AreEqual("a+2", s.GetCellContents("c").ToString());
            Assert.AreEqual("a*10+(5-3)", s.GetCellContents("d").ToString());
            Assert.AreEqual("b-4", s.GetCellContents("e").ToString());
            Assert.AreEqual("e*c", s.GetCellContents("f").ToString());
            Assert.AreEqual("d+12*2", s.GetCellContents("g").ToString());
            Assert.AreEqual("d-5", s.GetCellContents("h").ToString());
            Assert.AreEqual("f+(j*3-2)", s.GetCellContents("i").ToString());
            Assert.AreEqual("g-6", s.GetCellContents("j").ToString());
            Assert.AreEqual("g/h", s.GetCellContents("k").ToString());
            Assert.AreEqual("i*k+14", s.GetCellContents("l").ToString());

            //Check Dependencies of "a"
            Assert.AreEqual(12, list.Count);
            Assert.IsTrue(list.Contains("a"));
            Assert.IsTrue(list.Contains("b"));
            Assert.IsTrue(list.Contains("c"));
            Assert.IsTrue(list.Contains("d"));
            Assert.IsTrue(list.Contains("e"));
            Assert.IsTrue(list.Contains("f"));
            Assert.IsTrue(list.Contains("g"));
            Assert.IsTrue(list.Contains("h"));
            Assert.IsTrue(list.Contains("i"));
            Assert.IsTrue(list.Contains("j"));
            Assert.IsTrue(list.Contains("k"));
            Assert.IsTrue(list.Contains("l"));

            //Now, lets make some replacements and then check values again.
            s.SetContentsOfCell("b", "=12*(14/7)");
            s.SetContentsOfCell("f", "=e+20");
            s.SetContentsOfCell("k", "13.7");
            list = (List<string>)s.SetContentsOfCell("a", "10");

            //Make sure all Values are accurate
            Assert.AreEqual(10.0, s.GetCellContents("a"));
            Assert.AreEqual("12*(14/7)", s.GetCellContents("b").ToString());
            Assert.AreEqual("a+2", s.GetCellContents("c").ToString());
            Assert.AreEqual("a*10+(5-3)", s.GetCellContents("d").ToString());
            Assert.AreEqual("b-4", s.GetCellContents("e").ToString());
            Assert.AreEqual("e+20", s.GetCellContents("f").ToString());
            Assert.AreEqual("d+12*2", s.GetCellContents("g").ToString());
            Assert.AreEqual("d-5", s.GetCellContents("h").ToString());
            Assert.AreEqual("f+(j*3-2)", s.GetCellContents("i").ToString());
            Assert.AreEqual("g-6", s.GetCellContents("j").ToString());
            Assert.AreEqual(13.7, s.GetCellContents("k"));
            Assert.AreEqual("i*k+14", s.GetCellContents("l").ToString());

            //Check Dependencies of "a"
            Assert.AreEqual(8, list.Count);
            Assert.IsTrue(list.Contains("a"));
            Assert.IsTrue(list.Contains("c"));
            Assert.IsTrue(list.Contains("d"));
            Assert.IsTrue(list.Contains("g"));
            Assert.IsTrue(list.Contains("h"));
            Assert.IsTrue(list.Contains("i"));
            Assert.IsTrue(list.Contains("j"));
            Assert.IsTrue(list.Contains("l"));

            //Finally, let's make a ciruclar dependency and make sure that we find it
            s.SetContentsOfCell("a", "=l+2");
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestRestoreDependencies()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "=x+2");
            s.SetContentsOfCell("b", "=a-12*(14/7)");
            s.SetContentsOfCell("c", "=a+2");
            s.SetContentsOfCell("d", "=a*10+(5-3)");

            //Finally, let's make a ciruclar dependency and make sure that we find it
            s.SetContentsOfCell("a", "=d+2");

            //NOTE:
            //This test doesn't really have any surefire way to prove the old dependencies were restored, so I went into
            //The debugger to confirm that the dependencies were restored before our exception was thrown out. The reason
            //I wanted to test this is that the class says that if there is a circular exception, the spreadsheet should
            //remain as it was before the exception with no changes. This test was to verify that.
        }


        //PS5 ADDITIONAL TESTS*****************************************************************************************************************************************************************

        [TestMethod]
        public void TestComplexSetupAndGetValues()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            s.SetContentsOfCell("b", "=a-12*(14/7)");
            s.SetContentsOfCell("c", "=a+2");
            s.SetContentsOfCell("d", "=a*10+(5-3)");
            s.SetContentsOfCell("e", "=b-4");
            s.SetContentsOfCell("f", "=e*c");
            s.SetContentsOfCell("g", "=d+12*2");
            s.SetContentsOfCell("h", "=d-5");
            s.SetContentsOfCell("i", "=f+(j*3-2)");
            s.SetContentsOfCell("j", "=g-6");
            s.SetContentsOfCell("k", "=g/h");
            s.SetContentsOfCell("l", "=i*k+14");

            List<string> list = (List<string>)s.SetContentsOfCell("a", "10");

            //Make sure all contents are accurate
            Assert.AreEqual(10.0, s.GetCellContents("a"));
            Assert.AreEqual("a-12*(14/7)", s.GetCellContents("b").ToString());
            Assert.AreEqual("a+2", s.GetCellContents("c").ToString());
            Assert.AreEqual("a*10+(5-3)", s.GetCellContents("d").ToString());
            Assert.AreEqual("b-4", s.GetCellContents("e").ToString());
            Assert.AreEqual("e*c", s.GetCellContents("f").ToString());
            Assert.AreEqual("d+12*2", s.GetCellContents("g").ToString());
            Assert.AreEqual("d-5", s.GetCellContents("h").ToString());
            Assert.AreEqual("f+(j*3-2)", s.GetCellContents("i").ToString());
            Assert.AreEqual("g-6", s.GetCellContents("j").ToString());
            Assert.AreEqual("g/h", s.GetCellContents("k").ToString());
            Assert.AreEqual("i*k+14", s.GetCellContents("l").ToString());

            //Make sure all values are accurate
            Assert.AreEqual(10.0, s.GetCellValue("a"));
            Assert.AreEqual(-14.0, s.GetCellValue("b"));
            Assert.AreEqual(12.0, s.GetCellValue("c"));
            Assert.AreEqual(102.0, s.GetCellValue("d"));
            Assert.AreEqual(-18.0, s.GetCellValue("e"));
            Assert.AreEqual(-216.0, s.GetCellValue("f"));
            Assert.AreEqual(126.0, s.GetCellValue("g"));
            Assert.AreEqual(97.0, s.GetCellValue("h"));
            Assert.AreEqual(142.0, s.GetCellValue("i"));
            Assert.AreEqual(120.0, s.GetCellValue("j"));
            Assert.AreEqual(1.298969, (double)s.GetCellValue("k"), 1e-7);
            Assert.AreEqual(198.453608, (double)s.GetCellValue("l"), 1e-6);

            //Check Dependencies of "a"
            Assert.AreEqual(12, list.Count);
            Assert.IsTrue(list.Contains("a"));
            Assert.IsTrue(list.Contains("b"));
            Assert.IsTrue(list.Contains("c"));
            Assert.IsTrue(list.Contains("d"));
            Assert.IsTrue(list.Contains("e"));
            Assert.IsTrue(list.Contains("f"));
            Assert.IsTrue(list.Contains("g"));
            Assert.IsTrue(list.Contains("h"));
            Assert.IsTrue(list.Contains("i"));
            Assert.IsTrue(list.Contains("j"));
            Assert.IsTrue(list.Contains("k"));
            Assert.IsTrue(list.Contains("l"));

            //Now, lets make some replacements and then check values again.
            s.SetContentsOfCell("b", "=12*(14/7)");
            s.SetContentsOfCell("f", "=e+20");
            s.SetContentsOfCell("k", "13.7");
            list = (List<string>)s.SetContentsOfCell("a", "10");

            //Make sure all Values are accurate
            Assert.AreEqual(10.0, s.GetCellContents("a"));
            Assert.AreEqual("12*(14/7)", s.GetCellContents("b").ToString());
            Assert.AreEqual("a+2", s.GetCellContents("c").ToString());
            Assert.AreEqual("a*10+(5-3)", s.GetCellContents("d").ToString());
            Assert.AreEqual("b-4", s.GetCellContents("e").ToString());
            Assert.AreEqual("e+20", s.GetCellContents("f").ToString());
            Assert.AreEqual("d+12*2", s.GetCellContents("g").ToString());
            Assert.AreEqual("d-5", s.GetCellContents("h").ToString());
            Assert.AreEqual("f+(j*3-2)", s.GetCellContents("i").ToString());
            Assert.AreEqual("g-6", s.GetCellContents("j").ToString());
            Assert.AreEqual(13.7, s.GetCellContents("k"));
            Assert.AreEqual("i*k+14", s.GetCellContents("l").ToString());

            //Make sure all values are accurate
            Assert.AreEqual(10.0, s.GetCellValue("a"));
            Assert.AreEqual(24.0, s.GetCellValue("b"));
            Assert.AreEqual(12.0, s.GetCellValue("c"));
            Assert.AreEqual(102.0, s.GetCellValue("d"));
            Assert.AreEqual(20.0, s.GetCellValue("e"));
            Assert.AreEqual(40.0, s.GetCellValue("f"));
            Assert.AreEqual(126.0, s.GetCellValue("g"));
            Assert.AreEqual(97.0, s.GetCellValue("h"));
            Assert.AreEqual(398.0, s.GetCellValue("i"));
            Assert.AreEqual(120.0, s.GetCellValue("j"));
            Assert.AreEqual(13.7, (double)s.GetCellValue("k"), 1e-9);
            Assert.AreEqual(5466.6, (double)s.GetCellValue("l"), 1e-9);

            //Check Dependencies of "a"
            Assert.AreEqual(8, list.Count);
            Assert.IsTrue(list.Contains("a"));
            Assert.IsTrue(list.Contains("c"));
            Assert.IsTrue(list.Contains("d"));
            Assert.IsTrue(list.Contains("g"));
            Assert.IsTrue(list.Contains("h"));
            Assert.IsTrue(list.Contains("i"));
            Assert.IsTrue(list.Contains("j"));
            Assert.IsTrue(list.Contains("l"));
        }

        [TestMethod]
        public void TestSimpleSetupAndGetValues()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            Assert.AreEqual(10.0, s.GetCellValue("a"));

            s.SetContentsOfCell("b", "=a*2+5");
            Assert.AreEqual(25.0, s.GetCellValue("b"));

            s.SetContentsOfCell("c", "=b-a");
            Assert.AreEqual(15.0, s.GetCellValue("c"));
        }

        [TestMethod]
        public void TestSimpleSetupValueIsFormulaErrorDivByZero()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            Assert.AreEqual(10.0, s.GetCellValue("a"));

            s.SetContentsOfCell("b", "0");
            Assert.AreEqual(0.0, s.GetCellValue("b"));

            s.SetContentsOfCell("c", "=a/b");
            Assert.IsTrue(s.GetCellValue("c").GetType().Equals(typeof(FormulaError)));
        }

        [TestMethod]
        public void TestSimpleSetupValueIsFormulaErrorNonDeclaredVar()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            Assert.AreEqual(10.0, s.GetCellValue("a"));

            s.SetContentsOfCell("b", "5.0");
            Assert.AreEqual(5.0, s.GetCellValue("b"));

            s.SetContentsOfCell("c", "=a-x");
            Assert.IsTrue(s.GetCellValue("c").GetType().Equals(typeof(FormulaError)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetValueException()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellValue("21a");
        }

        [TestMethod]
        public void TestChangedBoolValue()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.Changed);

            s.SetContentsOfCell("a", "10");
            Assert.IsTrue(s.Changed);
        }

        [TestMethod]
        public void TestInvalidFormulaByAddingStringToDouble()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "hello");
            s.SetContentsOfCell("b", "=a+14.5");

            Assert.IsTrue(s.GetCellValue("b").GetType().Equals(typeof(FormulaError)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSimple2ndConstructor()
        {
            Spreadsheet s = new Spreadsheet(s => s==s.ToUpper(), s => s, "1.0");

            s.SetContentsOfCell("A", "10");
            Assert.AreEqual(10.0, s.GetCellValue("A"));

            s.SetContentsOfCell("B", "=A*2+5");
            Assert.AreEqual(25.0, s.GetCellValue("B"));

            s.SetContentsOfCell("c", "=B-A");
        }

        [TestMethod]
        public void TestValidateAndNormalize()
        {
            Spreadsheet s = new Spreadsheet(s => s == s.ToUpper(), s => s.ToUpper(), "1.0");

            s.SetContentsOfCell("a", "10");
            Assert.AreEqual(10.0, s.GetCellValue("a"));

            s.SetContentsOfCell("b", "=a*2+5");
            Assert.AreEqual(25.0, s.GetCellValue("b"));

            s.SetContentsOfCell("c", "=b-A");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableValidity()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("_A", "10");
        }

        [TestMethod]
        public void TestComplexSetupAndGetValuesWithValidatorAndNormalizer()
        {
            Spreadsheet s = new Spreadsheet(s => s == s.ToUpper(), s => s.ToUpper(), "1.0");

            s.SetContentsOfCell("a", "10");
            s.SetContentsOfCell("b", "=a-12*(14/7)");
            s.SetContentsOfCell("c", "=a+2");
            s.SetContentsOfCell("d", "=a*10+(5-3)");
            s.SetContentsOfCell("e", "=b-4");
            s.SetContentsOfCell("f", "=e*c");
            s.SetContentsOfCell("g", "=d+12*2");
            s.SetContentsOfCell("h", "=d-5");
            s.SetContentsOfCell("i", "=f+(j*3-2)");
            s.SetContentsOfCell("j", "=g-6");
            s.SetContentsOfCell("k", "=g/h");
            s.SetContentsOfCell("l", "=i*k+14");

            List<string> list = (List<string>)s.SetContentsOfCell("a", "10");

            //Make sure all contents are accurate
            Assert.AreEqual(10.0, s.GetCellContents("a"));
            Assert.AreEqual("A-12*(14/7)", s.GetCellContents("b").ToString());
            Assert.AreEqual("A+2", s.GetCellContents("c").ToString());
            Assert.AreEqual("A*10+(5-3)", s.GetCellContents("d").ToString());
            Assert.AreEqual("B-4", s.GetCellContents("e").ToString());
            Assert.AreEqual("E*C", s.GetCellContents("f").ToString());
            Assert.AreEqual("D+12*2", s.GetCellContents("g").ToString());
            Assert.AreEqual("D-5", s.GetCellContents("h").ToString());
            Assert.AreEqual("F+(J*3-2)", s.GetCellContents("i").ToString());
            Assert.AreEqual("G-6", s.GetCellContents("j").ToString());
            Assert.AreEqual("G/H", s.GetCellContents("k").ToString());
            Assert.AreEqual("I*K+14", s.GetCellContents("l").ToString());

            //Make sure all values are accurate
            Assert.AreEqual(10.0, s.GetCellValue("a"));
            Assert.AreEqual(-14.0, s.GetCellValue("b"));
            Assert.AreEqual(12.0, s.GetCellValue("c"));
            Assert.AreEqual(102.0, s.GetCellValue("d"));
            Assert.AreEqual(-18.0, s.GetCellValue("e"));
            Assert.AreEqual(-216.0, s.GetCellValue("f"));
            Assert.AreEqual(126.0, s.GetCellValue("g"));
            Assert.AreEqual(97.0, s.GetCellValue("h"));
            Assert.AreEqual(142.0, s.GetCellValue("i"));
            Assert.AreEqual(120.0, s.GetCellValue("j"));
            Assert.AreEqual(1.298969, (double)s.GetCellValue("k"), 1e-7);
            Assert.AreEqual(198.453608, (double)s.GetCellValue("l"), 1e-6);

            //Check Dependencies of "a"
            Assert.AreEqual(12, list.Count);
            Assert.IsTrue(list.Contains("A"));
            Assert.IsTrue(list.Contains("B"));
            Assert.IsTrue(list.Contains("C"));
            Assert.IsTrue(list.Contains("D"));
            Assert.IsTrue(list.Contains("E"));
            Assert.IsTrue(list.Contains("F"));
            Assert.IsTrue(list.Contains("G"));
            Assert.IsTrue(list.Contains("H"));
            Assert.IsTrue(list.Contains("I"));
            Assert.IsTrue(list.Contains("J"));
            Assert.IsTrue(list.Contains("K"));
            Assert.IsTrue(list.Contains("L"));

            //Now, lets make some replacements and then check values again.
            s.SetContentsOfCell("b", "=12*(14/7)");
            s.SetContentsOfCell("f", "=e+20");
            s.SetContentsOfCell("k", "13.7");
            list = (List<string>)s.SetContentsOfCell("a", "10");

            //Make sure all Values are accurate
            Assert.AreEqual(10.0, s.GetCellContents("a"));
            Assert.AreEqual("12*(14/7)", s.GetCellContents("b").ToString());
            Assert.AreEqual("A+2", s.GetCellContents("c").ToString());
            Assert.AreEqual("A*10+(5-3)", s.GetCellContents("d").ToString());
            Assert.AreEqual("B-4", s.GetCellContents("e").ToString());
            Assert.AreEqual("E+20", s.GetCellContents("f").ToString());
            Assert.AreEqual("D+12*2", s.GetCellContents("g").ToString());
            Assert.AreEqual("D-5", s.GetCellContents("h").ToString());
            Assert.AreEqual("F+(J*3-2)", s.GetCellContents("i").ToString());
            Assert.AreEqual("G-6", s.GetCellContents("j").ToString());
            Assert.AreEqual(13.7, s.GetCellContents("k"));
            Assert.AreEqual("I*K+14", s.GetCellContents("l").ToString());

            //Make sure all values are accurate
            Assert.AreEqual(10.0, s.GetCellValue("a"));
            Assert.AreEqual(24.0, s.GetCellValue("b"));
            Assert.AreEqual(12.0, s.GetCellValue("c"));
            Assert.AreEqual(102.0, s.GetCellValue("d"));
            Assert.AreEqual(20.0, s.GetCellValue("e"));
            Assert.AreEqual(40.0, s.GetCellValue("f"));
            Assert.AreEqual(126.0, s.GetCellValue("g"));
            Assert.AreEqual(97.0, s.GetCellValue("h"));
            Assert.AreEqual(398.0, s.GetCellValue("i"));
            Assert.AreEqual(120.0, s.GetCellValue("j"));
            Assert.AreEqual(13.7, (double)s.GetCellValue("k"), 1e-9);
            Assert.AreEqual(5466.6, (double)s.GetCellValue("l"), 1e-9);

            //Check Dependencies of "a"
            Assert.AreEqual(8, list.Count);
            Assert.IsTrue(list.Contains("A"));
            Assert.IsTrue(list.Contains("C"));
            Assert.IsTrue(list.Contains("D"));
            Assert.IsTrue(list.Contains("G"));
            Assert.IsTrue(list.Contains("H"));
            Assert.IsTrue(list.Contains("I"));
            Assert.IsTrue(list.Contains("J"));
            Assert.IsTrue(list.Contains("L"));
        }

        [TestMethod]
        public void TestSimpleSave()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            Assert.AreEqual(10.0, s.GetCellValue("a"));

            s.SetContentsOfCell("b", "=a*2+5");
            Assert.AreEqual(25.0, s.GetCellValue("b"));

            s.SetContentsOfCell("c", "=b-a");
            Assert.AreEqual(15.0, s.GetCellValue("c"));

            //Manually checked JSON file, it saved correctly and formatting is correct.
            s.Save("test.json");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestSimpleSaveException()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            Assert.AreEqual(10.0, s.GetCellValue("a"));

            s.SetContentsOfCell("b", "=a*2+5");
            Assert.AreEqual(25.0, s.GetCellValue("b"));

            s.SetContentsOfCell("c", "=b-a");
            Assert.AreEqual(15.0, s.GetCellValue("c"));

            //Manually checked JSON file, it saved correctly and formatting is correct.
            s.Save("/fakepath/nonsense/test.txt");
        }

        [TestMethod]
        public void TestSaveThenReload()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            Assert.AreEqual(10.0, s.GetCellValue("a"));

            s.SetContentsOfCell("b", "=a*2+5");
            Assert.AreEqual(25.0, s.GetCellValue("b"));

            s.SetContentsOfCell("c", "=b-a");
            Assert.AreEqual(15.0, s.GetCellValue("c"));

            //Manually checked JSON file, it saved correctly and formatting is correct.
            s.Save("testload.json");

            Spreadsheet s2 = new Spreadsheet("testload.json", s => true, s => s, "default");
            Assert.AreEqual(10.0, s2.GetCellValue("a"));
            Assert.AreEqual(25.0, s2.GetCellValue("b"));
            Assert.AreEqual(15.0, s2.GetCellValue("c"));

            Assert.AreEqual(10.0, s2.GetCellContents("a"));
            Assert.AreEqual("a*2+5", s2.GetCellContents("b").ToString());
            Assert.AreEqual("b-a", s2.GetCellContents("c").ToString());

        }

        [TestMethod]
        public void TestComplexSaveAndLoadWithValidatorAndNormalizer()
        {
            Spreadsheet s = new Spreadsheet(s => s == s.ToUpper(), s => s.ToUpper(), "1.0");

            s.SetContentsOfCell("a", "10");
            s.SetContentsOfCell("b", "=a-12*(14/7)");
            s.SetContentsOfCell("c", "=a+2");
            s.SetContentsOfCell("d", "=a*10+(5-3)");
            s.SetContentsOfCell("e", "=b-4");
            s.SetContentsOfCell("f", "=e*c");
            s.SetContentsOfCell("g", "=d+12*2");
            s.SetContentsOfCell("h", "=d-5");
            s.SetContentsOfCell("i", "=f+(j*3-2)");
            s.SetContentsOfCell("j", "=g-6");
            s.SetContentsOfCell("k", "=g/h");
            s.SetContentsOfCell("l", "=i*k+14");

            List<string> list = (List<string>)s.SetContentsOfCell("a", "10");

            //Make sure all contents are accurate
            Assert.AreEqual(10.0, s.GetCellContents("a"));
            Assert.AreEqual("A-12*(14/7)", s.GetCellContents("b").ToString());
            Assert.AreEqual("A+2", s.GetCellContents("c").ToString());
            Assert.AreEqual("A*10+(5-3)", s.GetCellContents("d").ToString());
            Assert.AreEqual("B-4", s.GetCellContents("e").ToString());
            Assert.AreEqual("E*C", s.GetCellContents("f").ToString());
            Assert.AreEqual("D+12*2", s.GetCellContents("g").ToString());
            Assert.AreEqual("D-5", s.GetCellContents("h").ToString());
            Assert.AreEqual("F+(J*3-2)", s.GetCellContents("i").ToString());
            Assert.AreEqual("G-6", s.GetCellContents("j").ToString());
            Assert.AreEqual("G/H", s.GetCellContents("k").ToString());
            Assert.AreEqual("I*K+14", s.GetCellContents("l").ToString());

            //Make sure all values are accurate
            Assert.AreEqual(10.0, s.GetCellValue("a"));
            Assert.AreEqual(-14.0, s.GetCellValue("b"));
            Assert.AreEqual(12.0, s.GetCellValue("c"));
            Assert.AreEqual(102.0, s.GetCellValue("d"));
            Assert.AreEqual(-18.0, s.GetCellValue("e"));
            Assert.AreEqual(-216.0, s.GetCellValue("f"));
            Assert.AreEqual(126.0, s.GetCellValue("g"));
            Assert.AreEqual(97.0, s.GetCellValue("h"));
            Assert.AreEqual(142.0, s.GetCellValue("i"));
            Assert.AreEqual(120.0, s.GetCellValue("j"));
            Assert.AreEqual(1.298969, (double)s.GetCellValue("k"), 1e-7);
            Assert.AreEqual(198.453608, (double)s.GetCellValue("l"), 1e-6);

            //Check Dependencies of "a"
            Assert.AreEqual(12, list.Count);
            Assert.IsTrue(list.Contains("A"));
            Assert.IsTrue(list.Contains("B"));
            Assert.IsTrue(list.Contains("C"));
            Assert.IsTrue(list.Contains("D"));
            Assert.IsTrue(list.Contains("E"));
            Assert.IsTrue(list.Contains("F"));
            Assert.IsTrue(list.Contains("G"));
            Assert.IsTrue(list.Contains("H"));
            Assert.IsTrue(list.Contains("I"));
            Assert.IsTrue(list.Contains("J"));
            Assert.IsTrue(list.Contains("K"));
            Assert.IsTrue(list.Contains("L"));

            Assert.AreEqual("1.0", s.Version);

            s.Save("ComplexTest.json");
            Assert.IsFalse(s.Changed);

            Spreadsheet s2 = new Spreadsheet("ComplexTest.json", s => s == s.ToLower(), s => s.ToLower(), "1.0");

            //Make sure all contents are accurate
            Assert.AreEqual(10.0, s2.GetCellContents("a"));
            Assert.AreEqual("a-12*(14/7)", s2.GetCellContents("b").ToString());
            Assert.AreEqual("a+2", s2.GetCellContents("c").ToString());
            Assert.AreEqual("a*10+(5-3)", s2.GetCellContents("d").ToString());
            Assert.AreEqual("b-4", s2.GetCellContents("e").ToString());
            Assert.AreEqual("e*c", s2.GetCellContents("f").ToString());
            Assert.AreEqual("d+12*2", s2.GetCellContents("g").ToString());
            Assert.AreEqual("d-5", s2.GetCellContents("h").ToString());
            Assert.AreEqual("f+(j*3-2)", s2.GetCellContents("i").ToString());
            Assert.AreEqual("g-6", s2.GetCellContents("j").ToString());
            Assert.AreEqual("g/h", s2.GetCellContents("k").ToString());
            Assert.AreEqual("i*k+14", s2.GetCellContents("l").ToString());

            //Make sure all values are accurate
            Assert.AreEqual(10.0, s2.GetCellValue("a"));
            Assert.AreEqual(-14.0, s2.GetCellValue("b"));
            Assert.AreEqual(12.0, s2.GetCellValue("c"));
            Assert.AreEqual(102.0, s2.GetCellValue("d"));
            Assert.AreEqual(-18.0, s2.GetCellValue("e"));
            Assert.AreEqual(-216.0, s2.GetCellValue("f"));
            Assert.AreEqual(126.0, s2.GetCellValue("g"));
            Assert.AreEqual(97.0, s2.GetCellValue("h"));
            Assert.AreEqual(142.0, s2.GetCellValue("i"));
            Assert.AreEqual(120.0, s2.GetCellValue("j"));
            Assert.AreEqual(1.298969, (double)s2.GetCellValue("k"), 1e-7);
            Assert.AreEqual(198.453608, (double)s2.GetCellValue("l"), 1e-6);
            
            list = (List<string>)s2.SetContentsOfCell("a", "10");

            //Check Dependencies of "a"
            Assert.AreEqual(12, list.Count);
            Assert.IsTrue(list.Contains("a"));
            Assert.IsTrue(list.Contains("b"));
            Assert.IsTrue(list.Contains("c"));
            Assert.IsTrue(list.Contains("d"));
            Assert.IsTrue(list.Contains("e"));
            Assert.IsTrue(list.Contains("f"));
            Assert.IsTrue(list.Contains("g"));
            Assert.IsTrue(list.Contains("h"));
            Assert.IsTrue(list.Contains("i"));
            Assert.IsTrue(list.Contains("j"));
            Assert.IsTrue(list.Contains("k"));
            Assert.IsTrue(list.Contains("l"));
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestSaveThenReloadExceptionVersionDifference()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("a", "10");
            Assert.AreEqual(10.0, s.GetCellValue("a"));

            s.SetContentsOfCell("b", "=a*2+5");
            Assert.AreEqual(25.0, s.GetCellValue("b"));

            s.SetContentsOfCell("c", "=b-a");
            Assert.AreEqual(15.0, s.GetCellValue("c"));

            //Manually checked JSON file, it saved correctly and formatting is correct.
            s.Save("testfail.json");

            Spreadsheet s2 = new Spreadsheet("testfail.json", s => true, s => s, "1.2");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestNonExistentFile()
        {
            Spreadsheet s = new Spreadsheet("nonexistent.json", s => true, s => s, "1.2");
        }
    }
}