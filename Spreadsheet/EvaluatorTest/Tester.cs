using FormulaEvaluator;
using System.Data;

namespace FormulaEvaluator
{
    internal class Tester
    {
        //private static Lookup Del = variableEval;

        static void Main(string[] args)
        {
            String test;
            int answer;

            //Test 1  --  Basic Infix, no Vars
            test = "2+3*(5/2-4)";
            answer = Evaluator.Evaluate(test, variableEval);
            Console.WriteLine(answer);

            //Test 2 -- Basic Infix, no Vars, whitespace of up to two spaces
            test = "(2 * 2 ) / 2 + 9 - (  3 * 2)";
            answer = Evaluator.Evaluate(test, variableEval);
            Console.WriteLine(answer);

            //Test 3 -- Basic Infix, one Var
            test = "(2*2) + A7";
            answer = Evaluator.Evaluate(test, variableEval);
            Console.WriteLine(answer);

            //Test 4 -- Basic Infix, one Var with lowercase
            test = "(2*2) + a7";
            answer = Evaluator.Evaluate(test, variableEval);
            Console.WriteLine(answer);

            //Test 5 -- Basic Infix, one Var with multiple letters and numbers
            test = "(2*2) + aBc123";
            answer = Evaluator.Evaluate(test, variableEval);
            Console.WriteLine(answer);

            //Test 6 -- Test bad variable (ran multiple configs of this -- letters after numbers, combinations, etc. Everything threw exception as expected
            //String test = "(2*2) + 3A";
            //answer = Evaluator.Evaluate(test, variableEval);
            //Console.WriteLine(answer);

            //Test 7 -- Basic Infix, divide by zero
            //String test = "7 / (2-2)";
            //answer = Evaluator.Evaluate(test, variableEval);
            //Console.WriteLine(answer);

            //Test 8 -- Variable not found by lookup
            //test = "7 + b4";
            //answer = Evaluator.Evaluate(test, variableEval);
            //Console.WriteLine(answer);

            //Test 8 -- Bad amount of values
            //test = "7 + 7 9 + 2";
            //answer = Evaluator.Evaluate(test, variableEval);
            //Console.WriteLine(answer);

            //Test 9 -- Bad amount of operators
            //test = "7 + 3 - (2 + * 7)";
            //answer = Evaluator.Evaluate(test, variableEval);
            //Console.WriteLine(answer);

            //Test 10 -- Extra parentheses
            //test = "7 + (5*3))";
            //answer = Evaluator.Evaluate(test, variableEval);
            //Console.WriteLine(answer);
        }

        public static int variableEval(string s)
        {
            if (s.Equals("A7", StringComparison.OrdinalIgnoreCase))
                return 5;
            else if (s.Equals("abc123", StringComparison.OrdinalIgnoreCase))
                return 13;
            else
                throw new ArgumentException("Variable was not found.");
        }
    }
}