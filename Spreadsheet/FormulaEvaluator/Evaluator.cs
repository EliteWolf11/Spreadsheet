using System;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    //Declaring the Lookup delegate
    public delegate int Lookup(String v);

    /// <summary>
    /// This class Evaluates infix expressions that are given to it. It uses a delegate called Lookup to search for the value of a variable given to it in the form
    /// of any number of letters followed by any number of integers. The string is the given infix expression, which MUST only use variables of the given format, integers, or the
    /// operators (,),+,-,*,/    -- If the class and it's methods find any invalid character, an ArgumentException will be thrown.
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Like mentioned in the class XML comment, this method takes a string form of an Infix Expression that must meet the requirements stated in the class XML. It also takes
        /// a Lookup delegate as described in the class XML comments. Throws ArgumentException for a number of reasons: invalid character in Infix Expression, inproper formatting
        /// of Infix Expression (too many parenthesis, too many operators as compared to variables, doesn't follow the rules of Infix Expressions, etc.)
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="variableEvaluator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            //Declare a regular expression for checking variable validity
            Regex regex = new Regex(@"^[A-Z]+[0-9]+$", RegexOptions.IgnoreCase);

            //Splits the given string into tokens
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            //Clears all whitespace out of each string
            for(int i = 0; i< substrings.Length; i++)
                substrings[i] = substrings[i].Trim();

            
            //Create two stacks, one for vars and one for operators. Create an int to catch string parses for integers, and a bool to make sure an int is an int.
            Stack<int> values = new Stack<int>();
            Stack<string> operators = new Stack<string>();
            int number;
            bool isNumber;

            //Read through the tokens we have, performing operations till we read through every token
            foreach (string substring in substrings)
            {
                //Immediately store a bool for if the substring is an int or not, if it is, we'll catch it as an int in "number"
                isNumber = int.TryParse(substring, out number);

                //Check for an empty string, if found, continue to next iteration of loop
                if (substring.Length < 1 || substring.Equals(" "))
                    continue;
                //Check for a plain value (no variable)
                else if (isNumber)
                {
                    //Check if the top operator is * or /
                    if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                        //We want to operate on the current number, and the top value of the values stack
                        //We know that there is guarunteed to be one operator on the stack because to get here, we had to use peek and have it return true.
                        values.PushResult(number, operators.Pop());
                    //Push the value to the values stack
                    else
                        values.Push(number);
                }
                //Check for an operator (any of the math operators, or an opening/closing parenthesis)
                else if (substring.isOperator())
                {
                    //First, check for a + or -
                    if (substring.Equals("+") || substring.Equals("-"))
                    {
                        //Check the operators stack for a + or -
                        if (operators.IsOnTop("+") || operators.IsOnTop("-"))
                        {
                            //Check for at least one item in the values stack before proceeding
                            if (values.Count > 0)
                            {
                                //We want to operate on the top two values of the values stack, so we pop one in the method call (the other is popped in the method)
                                values.PushResult(values.Pop(), operators.Pop());
                                operators.Push(substring);
                            }
                            //In case the values stack is empty
                            else
                                throw new ArgumentException("The values stack is empty.");
                        }
                        else
                            operators.Push(substring);
                    }
                    //Check for a )
                    else if (substring.Equals(")"))
                    {
                        //First, check for + or - 
                        if (operators.IsOnTop("+") || operators.IsOnTop("-"))
                        {
                            //Check for at least one value in the values stack
                            if (values.Count > 0)
                            {
                                //We'll operate on the top two values of the values stack
                                values.PushResult(values.Pop(), operators.Pop());
                            }
                            //Throw an exception because the values stack is empty
                            else
                                throw new ArgumentException("The values stack is empty.");
                        }

                        //At this point, we SHOULD find a '(', so check for it
                        if (operators.IsOnTop("("))
                            //If we found it, pop it
                            operators.Pop();
                        //If we didn't find it, throw an exception
                        else
                            throw new ArgumentException("A '(' operator was not found where it should be.");

                        //Check for * or /
                        if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                        {
                            //Make sure we're not empty in the values stack
                            if (values.Count > 0)
                            {
                                //Operate on top two values of values stack
                                values.PushResult(values.Pop(), operators.Pop());
                            }
                            //Empty values stack
                            else
                                throw new ArgumentException("The values stack is empty.");
                        }
                    }
                    //If we didn't hit any other statement, our operate is guarunteed to be either a *, /, or (, in which case we just push it
                    else
                        operators.Push(substring);  
                }
                //At this point, we know it isn't an operator or a flat integer, so it must be a variable
                else if (substring.isValidVar(regex))
                {
                    //Get the value for the variable
                    int value = variableEvaluator(substring);
                    //Check if the top operator is * or /
                    if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                        //We want to operate on the current number, and the top value of the values stack
                        //We know that there is guarunteed to be one operator on the stack because to get here, we had to use peek and have it return true.
                        values.PushResult(value, operators.Pop());
                    //Push the value to the values stack
                    else
                        values.Push(value);
                }
                else
                    throw new ArgumentException("The substring passed is not a valid integer, operator, or variable.");
            }

            //We finished going through all the substrings, time to get the final result

            //If there are no operators, then we should have only one value on the stack
            if (operators.Count == 0)
            {
                //If we only have one value, pop it, that's the answer.
                if (values.Count == 1)
                    return values.Pop();
                else
                    throw new ArgumentException("There was not exactly one value left in the values stack.");
            }
            //If the operators stack isn't empty, there should be only one operator left.
            else if (operators.Count == 1)
            {
                //Final operator SHOULD be + or -
                if (operators.IsOnTop("+") || operators.IsOnTop("-"))
                {
                    //We should have exactly two values left, operate on them and return final answer
                    if (values.Count == 2)
                    {
                        values.PushResult(values.Pop(), operators.Pop());
                        return values.Pop();
                    }
                    else
                        throw new ArgumentException("There was not exactly two values in the values stack.");
                }
                else
                    throw new ArgumentException("The final operator was not + or -.");
            }
            else
                throw new ArgumentException("There was more than one operator left on the stack.");
        }

        /// <summary>
        /// This helper method checks if a string given to it is a valid operator for our infix expressions.
        /// Valid operators are (,),+,-,*,/
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool isOperator(this string s)
        {
            //Check to see if our string is a valid operator
            if (s.Equals("+") || s.Equals("-") || s.Equals("*") || s.Equals("/") || s.Equals("(") || s.Equals(")"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// This helper method checks to see if a Variable that was given is valid, i.e. it matches the rules of our class -- 1 or more letters followed by 1 or more integers.
        /// To check this, we pass in a regex (regular expression) created in the Evaluate class.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private static bool isValidVar(this string s, Regex r)
        {
            //Use our regex to confirm if it is a valid variable
            return r.IsMatch(s);
        }

        /// <summary>
        /// This helper method determines if a given string matches the string on top of a stack (Stack must be of type string).
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        private static bool IsOnTop(this Stack<string> stack, string oper)
        {
            //returns true if what's on top of the stack equals the operator we're looking for
            if (stack.Count > 0)
                return stack.Peek().Equals(oper);
            else
                return false;
        }

        /// <summary>
        /// This method is used to perform operations on two values, then push the result to the values stack. It functions by using the values stack, a given int, and a string
        /// that is our operator. It will pop 1 value from the values stack, and uses a switch statement to determine which operator to perform the math with.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="value2"></param>
        /// <param name="oper"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void PushResult(this Stack<int> stack, int value2, string oper)
        {
            //Make sure we have at least one value in the values stack since we will be popping values.
            if (stack.Count > 0)
            {
                //Pop a value from the stack, catch it.
                int value1 = stack.Pop();

                //Use a switch statement to figure out what kind of math to perform
                switch (oper)
                {
                    //Multiply value1 and value2, then push it to the values stack.
                    case "*":
                        stack.Push(value1 * value2);
                        return;
                    //Make sure value2 (the divisor) is not zero, then divide and push.
                    case "/":
                        if (value2 != 0)
                        {
                            stack.Push(value1 / value2);
                            return;
                        }
                        else
                            throw new ArgumentException("Divide by 0 error.");
                    //Add the values and push them.
                    case "+":
                        stack.Push(value1 + value2);
                        return;
                    //Subtract the values and push them.
                    case "-":
                        stack.Push(value1 - value2);
                        return;
                    //Default case, throw an exception because we didn't find a valid operator.
                    default: throw new ArgumentException("Did not find valid operator.");
                }

            }
            else
                throw new ArgumentException("The value stack is empty.");
        }
    }
}