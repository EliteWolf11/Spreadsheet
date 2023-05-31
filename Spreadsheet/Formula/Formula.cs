// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        //private fields
        private List<string> tokenlist;
        private Regex varCheck;
        private Regex doubCheck;


        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            //We need to parse the string first
            //We can trust that GetTokens returns a correctly parsed string containing no whitespace or blank strings.
            tokenlist =  new List<string>(GetTokens(formula));
            varCheck = new Regex(@"^[a-zA-Z_](?:[a-zA-Z_]|\d)*$");
            doubCheck = new Regex(@"^(?:\d+\.\d*|\d*\.\d+|\d+)(?:[eE][\+-]?\d+)?$");

            //ONE TOKEN RULE
            if (tokenlist.Count < 1)
                throw new FormulaFormatException("There is less than one token(s) in the string.");

            //Next, pass tokens into the normalizer
            for (int i = 0; i < tokenlist.Count; i++)
            {
                if (varCheck.IsMatch(tokenlist[i]))
                    tokenlist[i] = normalize(tokenlist[i]);
            }

            //Check for validity of variables overall (do they match the general requirements of what a variable is?)
            bool validVar = true;
            //TOKEN VALIDITY
            foreach (string token in tokenlist)
            {
                //If the token is either a variable, double, or operator, then it is a valid token.
                if (!varCheck.IsMatch(token) && !doubCheck.IsMatch(token) && !isOperator(token))
                    validVar = false;
            }

            //At least one of the tokens  wasn't valid.
            if (!validVar)
                throw new FormulaFormatException("A normalized variable has invalid syntax and is therefore not a valid variable.");

            //Pass tokens into validator
            foreach(string token in tokenlist)
                if(varCheck.IsMatch(token))
                    if (!isValid(token))
                        throw new FormulaFormatException("A normalized variable failed to pass the validator, and is therefore invalid.");

            //CHECK FOR OTHER SYNTAX ERRORS
            SyntaxChecker(tokenlist, varCheck, doubCheck);

            //If we pass this point, we have a valid function. It is immutable, so now we can trust that it will ALWAYS be valid.
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            Stack<double> values = new Stack<double>();
            Stack<string> operators = new Stack<string>();
            double number;
            bool isNumber;
            bool formulaErr = false;

            //Read through the tokens we have, performing operations till we read through every token
            foreach (string token in tokenlist)
            {
                //Immediately store a bool for if the substring is an int or not, if it is, we'll catch it as an int in "number"
                isNumber = Double.TryParse(token, out number);

                //Check for a plain value (no variable)
                if (isNumber)
                {
                    //Check if the top operator is * or /
                    if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                    {
                        //We want to operate on the current number, and the top value of the values stack
                        //We know that there is guarunteed to be one operator on the stack because to get here, we had to use peek and have it return true.
                        formulaErr = PushResult(values, number, operators.Pop());

                        if (formulaErr)
                            return new FormulaError("#DIV/0!");
                    }
                    //Push the value to the values stack
                    else
                        values.Push(number);
                }
                //Check for an operator (any of the math operators, or an opening/closing parenthesis)
                else if (isOperator(token))
                {
                    //First, check for a + or -
                    if (token.Equals("+") || token.Equals("-"))
                    {
                        //Check the operators stack for a + or -
                        if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                        {
                            //We want to operate on the top two values of the values stack, so we pop one in the method call (the other is popped in the method)
                            PushResult(values, values.Pop(), operators.Pop());
                            operators.Push(token);
                        }
                        else
                            operators.Push(token);
                    }
                    //Check for a )
                    else if (token.Equals(")"))
                    {
                        //First, check for + or - 
                        if (IsOnTop(operators, "+") || IsOnTop(operators, "-"))
                        {
                            //We'll operate on the top two values of the values stack
                            PushResult(values, values.Pop(), operators.Pop());
                        }

                        //At this point, we SHOULD find a '(', so check for it
                        if (IsOnTop(operators, "("))
                            operators.Pop();

                        //Check for * or /
                        if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                        {
                            //Operate on top two values of values stack
                            formulaErr = PushResult(values, values.Pop(), operators.Pop());

                            if (formulaErr)
                                return new FormulaError("#DIV/0!");
                        }
                    }
                    //If we didn't hit any other statement, our operate is guarunteed to be either a *, /, or (, in which case we just push it
                    else
                        operators.Push(token);
                }
                //At this point, we know it isn't an operator or a flat integer, so it must be a variable
                else
                {
                    double value;
                    //Get the value for the variable
                    try
                    {
                        value = lookup(token);
                    }
                    catch
                    {
                        return new FormulaError("#BADVAR");
                    }
                    //Check if the top operator is * or /
                    if (IsOnTop(operators, "*") || IsOnTop(operators, "/"))
                    {
                        //We want to operate on the current number, and the top value of the values stack
                        //We know that there is guarunteed to be one operator on the stack because to get here, we had to use peek and have it return true.
                        formulaErr = PushResult(values, value, operators.Pop());

                        if (formulaErr)
                            return new FormulaError("#DIV/0!");
                    }
                    //Push the value to the values stack
                    else
                        values.Push(value);
                }
            }

            //We finished going through all the substrings, time to get the final result

            //If there are no operators, then we should have only one value on the stack
            if (operators.Count == 0)
            {
                //If we only have one value, pop it, that's the answer.
                return values.Pop();
            }
            //If the operators stack isn't empty, there should be only one operator left.
            else
            {
                //We should have exactly two values left, operate on them and return final answer
                PushResult(values, values.Pop(), operators.Pop());
                return values.Pop();
            }
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            //Create our list that we'll return.
            List<string> varList = new();

            //Loop through our normalized token list
            foreach(string token in tokenlist)
            {
                //First, see if the token is a double or an operator, if so, continue to next token.
                if (isOperator(token) || Double.TryParse(token, out double d))
                    continue;

                //Check if the current variable already exists in our new list, if not, add it.
                if (!varList.Contains(token))
                    varList.Add(token);
            }

            return varList;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            //We'll use a String Builder to craft our ToString()
            StringBuilder sb = new();

            //Loop through our normalized token list (which will be void of whitespace), add each token to our StringBuilder
            foreach(string token in tokenlist)
            {
                if(Double.TryParse(token, out double result))
                {
                    string dstring = result.ToString();
                    sb.Append(dstring);
                }
                else
                    sb.Append(token);
            }

            //Return our string.
            return sb.ToString();
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            //Check if obj is null
            if (obj != null)
            {
                //Set up some variables that we will use.
                List<string> objList;
                string? objStr = obj.ToString();
                Type t = obj.GetType();

                //Check if obj is of type Formula
                if (!t.Equals(typeof(Formula)))
                    return false;

                //Get obj's tokens
                if (objStr != null)
                {
                    //Create a new Formula from Object so that we can normalize it's variables to "this" Formula's normalazation requirements.
                    Formula objForm = new Formula(objStr);
                    //Get a list of the tokens.
                    objList = new List<string>(GetTokens(objForm.ToString()));
                }
                //Probably can't hit this statement, but without it the upcoming code can't work because of how null types function (the if/else statement makes sure that objList WILL exist)
                else
                    return false;

                for (int i = 0; i < tokenlist.Count; i++)
                {
                    //If this formula's current token is an operator...
                    if (isOperator(tokenlist[i]))
                    {
                        if (!tokenlist[i].Equals(objList[i]))
                            return false;
                    }
                    //If this formula's current token is a double...
                    else if (Double.TryParse(tokenlist[i], out double result))
                    {
                        //Check if objList token is a double
                        if (Double.TryParse(objList[i], out double result2))
                        {
                            //Check if the doubles are equal
                            if (!result.ToString().Equals(result2.ToString()))
                                return false;
                        }
                        //objList token isn't a double.
                        else
                            return false;
                    }
                    //If this formula's current token is a variable...
                    else
                    {
                        //We know it is a variable, and we already normalized all of obj's variables, so just check if they're equal.
                        if (!tokenlist[i].Equals(objList[i]))
                            return false;
                    }
                }
                //If we make it out of the For loop without returning false, we know they are equal.
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (f1.Equals(f2))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (f1.Equals(f2))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            //Since any Formula that is equal to another will have the same string from our overloaded ToString(), getting the hash code of that string will make sure they are always equal.
            string str = this.ToString();

            return str.GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }

        /// <summary>
        /// This private helper method is called from the Formula Constructor to check for syntax errors. If it detects any, it will throw a FormulaFormatException.
        /// </summary>
        /// <param name="tokenlist">Our list of tokens for the Formula.</param>
        /// <exception cref="FormulaFormatException">A variety of possibilies, all having to do with faulty syntax.</exception>
        private static void SyntaxChecker(IEnumerable<string> tokenlist, Regex varCheck, Regex doubCheck)
        {
            //Note, at this point, we know that all tokens are valid. We are just checking syntax.

            string token;
            int openingP = 0;
            int closingP = 0;
            bool prevWasOp = false;
            bool prevWasVal;
            bool prevWasOParen = false;
            bool prevWasCParen = false;

            IEnumerator<string> e = tokenlist.GetEnumerator();

            //SYTANX CHECK #5
            e.MoveNext();
            token = e.Current;
            if (!varCheck.IsMatch(token) && !doubCheck.IsMatch(token) && !token.Equals("("))
                throw new FormulaFormatException("First token is not a variable, double, or opening parentheses.");

            prevWasVal = true;

            //Special case of a formula starting with an opening parentheses.
            if(token.Equals("("))
            {
                openingP++;
                prevWasOp = true;
                prevWasOParen = true;
                prevWasVal = false;
            }

            //We will loop through the whole token set, and check for syntax errors.
            while(e.MoveNext())
            {
                token = e.Current;

                //Current token is a double
                if(doubCheck.IsMatch(token))
                {
                    if (prevWasCParen)
                        throw new FormulaFormatException("A value or variable was detected immediately after a closing parentheses.");
                    if (prevWasVal)
                        throw new FormulaFormatException("Next item in Formula was a value when the item before it was also a value.");

                    prevWasOp = false;
                    prevWasVal = true;
                    prevWasOParen = false;
                    prevWasCParen = false;
                }
                //Current token is a variable
                else if(varCheck.IsMatch(token))
                {
                    if (prevWasCParen)
                        throw new FormulaFormatException("A value or variable was detected immediately after a closing parentheses.");
                    if (prevWasVal)
                        throw new FormulaFormatException("Next item in Formula was a value when the item before it was also a value.");

                    prevWasOp = false;
                    prevWasVal = true;
                    prevWasOParen = false;
                    prevWasCParen = false;
                }
                //Current token is an operator
                else
                {
                    //Check for specific syntax errors.
                    if (prevWasOp && !prevWasOParen && !prevWasCParen && !token.Equals("("))
                        throw new FormulaFormatException("Next item in Formula was an operator when the item before it was also an operator.");
                    else if (prevWasOParen && !token.Equals("("))
                        throw new FormulaFormatException("Previous token was an opening parentheses, and following token was a non-opening parentheses operator.");
                    else if (prevWasCParen && token.Equals("("))
                        throw new FormulaFormatException("Closing parentheses was followed by an opening parenthesis");


                    if (token.Equals("("))
                    {
                        openingP++;
                        prevWasOParen = true;
                        prevWasCParen = false;
                    }
                    else if (token.Equals(")"))
                    {
                        closingP++;
                        prevWasCParen = true;
                        prevWasOParen = false;
                    }
                    else
                    {
                        prevWasCParen = false;
                        prevWasOParen = false;
                    }

                    //SYNTAX CHECK #3
                    if (closingP > openingP)
                        throw new FormulaFormatException("There are more closing parentheses then there are opening parentheses.");

                    prevWasOp = true;
                    prevWasVal = false;
                }
            }

            //We're out of the while loop, meaning we accessed our last token. We need to make sure it is correct.
            //SYNTAX CHECK #6
            if (!varCheck.IsMatch(token) && !doubCheck.IsMatch(token) && !token.Equals(")"))
                throw new FormulaFormatException("Last token is not a variable, double, or closing parentheses.");

            //SYNTAX CHECK #4
            if (openingP != closingP)
                throw new FormulaFormatException("The amount of opening and closing parenthesis are not the same.");

        }

        /// <summary>
        /// This helper method checks if a string given to it is a valid operator for our infix expressions.
        /// Valid operators are (,),+,-,*,/
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool isOperator(string s)
        {
            //Check to see if our string is a valid operator
            if (s.Equals("+") || s.Equals("-") || s.Equals("*") || s.Equals("/") || s.Equals("(") || s.Equals(")"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// This helper method determines if a given string matches the string on top of a stack (Stack must be of type string).
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        private static bool IsOnTop(Stack<string> stack, string oper)
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
        private static bool PushResult(Stack<double> stack, double value2, string oper)
        {
                //Pop a value from the stack, catch it.
            double value1 = stack.Pop();
            bool errFound = false;

                //Use a switch statement to figure out what kind of math to perform
            switch (oper)
            {
                //Multiply value1 and value2, then push it to the values stack.
                case "*":
                    stack.Push(value1 * value2);
                    break;
                //Make sure value2 (the divisor) is not zero, then divide and push.
                case "/":
                    if (value2 != 0)
                    {
                        stack.Push(value1 / value2);
                        break;
                    }
                    else
                    {
                        errFound = true;
                        break;
                    }
                    
                //Add the values and push them.
                case "+":
                    stack.Push(value1 + value2);
                    break;
                //Subtract the values and push them.
                case "-":
                    stack.Push(value1 - value2);
                    break;
                //Default case, throw an exception because we didn't find a valid operator. Probably won't ever be hit, but is necessary.
                default: return errFound;
            }

            return errFound;
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}