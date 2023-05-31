using SpreadsheetUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;

namespace SS
{
    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a cell name if and only if it consists of one or more letters,
    /// followed by one or more digits AND it satisfies the predicate IsValid.
    /// For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
    /// satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
    /// regardless of IsValid.
    /// 
    /// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
    /// must be normalized with the Normalize method before it is used by or saved in 
    /// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
    /// the Formula "x3+a5" should be converted to "X3+A5" before use.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  The distinction is
    /// important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Spreadsheet : AbstractSpreadsheet
    {
        //Data Structure to keep track of all named Cells (cells that currently are non-empty).
        [JsonProperty(PropertyName ="cells")]
        private Dictionary<string, Cell> namedCells;
        //DependencyGraph for tracking cells
        private DependencyGraph graph;

        //Default Constructor
        /// <summary>
        /// Default Constructor for Spreadsheet.
        /// </summary>
        public Spreadsheet() : base(s => true, s => s, "default")
        {
            namedCells = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
            this.Changed = false;
        }

        //3-Param Constructor
        /// <summary>
        /// Takes in 3 parameters to create a spreadsheet.
        /// </summary>
        /// <param name="isValid">validator</param>
        /// <param name="normalize">normalizer</param>
        /// <param name="version">version number</param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            namedCells = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
            this.Changed = false;
        }

        //4-Param Constructor
        /// <summary>
        /// Takes in the 4 parameters, creates a new spreadsheet from a saved spreadsheet.
        /// If any issues occur with initializing a spreadsheet from the saved spreadsheet, this will throw a SpreadsheetReadWriteException.
        /// </summary>
        /// <param name="filepath">file path for a json file containing a saved spreadsheet.</param>
        /// <param name="isValid">validator</param>
        /// <param name="normalize">normalizer</param>
        /// <param name="version">version number.</param>
        /// <exception cref="SpreadsheetReadWriteException"></exception>
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            //Read in a JSON file, create a spreadsheet with it.
            Spreadsheet? spreadsheet;
            try
            {
                string jsonString = File.ReadAllText(filepath);
                spreadsheet = JsonConvert.DeserializeObject<Spreadsheet>(jsonString);
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Couldn't find or read saved file.");
            }
            

            //Null catcher for the spreadsheet (necessary)
            if (spreadsheet != null)
            {
                //Check version numbers to make sure they are the same.
                if (spreadsheet.Version.Equals(this.Version))
                {
                    //Set up this object to use all of our deserialized spreadsheet.
                    namedCells = spreadsheet.namedCells;
                    graph = spreadsheet.graph;
                    this.Changed = false;


                    List<string> list = (List<string>)GetNamesOfAllNonemptyCells();
                    //Now we need to re-set up the DependencyGraph and Values -- so take the content string of every cell and initialize everything with it.
                    foreach (string cellname in list)
                    {
                        //Make sure there is a Cell at this name
                        if (namedCells.TryGetValue(cellname, out Cell? cell))
                        {
                            //Set contents of cell from its content string -- catch any issues and turn them into a SpreadsheetReadWriteException.
                            try
                            {
                                SetContentsOfCell(cellname, cell.GetContentString());
                            }
                            catch
                            {
                                throw new SpreadsheetReadWriteException("There was an issue with the cells of the spreadsheet (either invalid names, circular exception, etc..");
                            }
                        }
                    }
                }
                //Versions aren't the same...
                else
                    throw new SpreadsheetReadWriteException("Version of saved spreadsheet is different than this spreadsheet.");
            }
            //Emergency catch incase there is an issue reading the json file.
            else
                throw new SpreadsheetReadWriteException("Default error catch for reading saved spreadsheet (blanket catch in case of non-specific issue).");
        }

        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            //A string is passed in, so we throw it into our Dictionary to get the corresponding cell.
            if (isValidName(name))
            {
                if (namedCells.TryGetValue(name, out Cell? cell))
                    //Return cell contents.
                    return cell.GetContent();
                else
                    return "";
            }
            else
                throw new InvalidNameException();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            name = Normalize(name);
            //A string is passed in, so we throw it into our Dictionary to get the corresponding cell.
            if (isValidName(name))
            {
                if (namedCells.TryGetValue(name, out Cell? cell))
                    //Return cell value.
                    return cell.GetValue();
                else
                    return "";
            }
            else
                throw new InvalidNameException();
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            List<string> names = new List<string>();
            foreach (string cellName in namedCells.Keys)
            {
                names.Add(cellName);
            }
            return names;
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using a JSON format.
        /// The JSON object should have the following fields:
        /// "Version" - the version of the spreadsheet software (a string)
        /// "cells" - an object containing 0 or more cell objects
        ///           Each cell object has a field named after the cell itself 
        ///           The value of that field is another object representing the cell's contents
        ///               The contents object has a single field called "stringForm",
        ///               representing the string form of the cell's contents
        ///               - If the contents is a string, the value of stringForm is that string
        ///               - If the contents is a double d, the value of stringForm is d.ToString()
        ///               - If the contents is a Formula f, the value of stringForm is "=" + f.ToString()
        /// 
        /// For example, if this spreadsheet has a version of "default" 
        /// and contains a cell "A1" with contents being the double 5.0 
        /// and a cell "B3" with contents being the Formula("A1+2"), 
        /// a JSON string produced by this method would be:
        /// 
        /// {
        ///   "cells": {
        ///     "A1": {
        ///       "stringForm": "5"
        ///     },
        ///     "B3": {
        ///       "stringForm": "=A1+2"
        ///     }
        ///   },
        ///   "Version": "default"
        /// }
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            //Set up the Json Properties so that when we serialize this Spreadsheet, it should turn out as expected.
            string jsonForm = JsonConvert.SerializeObject(this);
            //Console.WriteLine(jsonForm);

            //Write the JSON to the designated file, catch any issues and throw a SpreadsheetReadWriteException.
            try
            {
                File.WriteAllText(filename, jsonForm);
            }
            catch
            {
                throw new SpreadsheetReadWriteException("There was an issue saving the spreadsheet.");
            }
            
            //After a successful save, change our "changed" bool to false.
            this.Changed = false;
        }

        /// <summary>
        /// The contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, double number)
        {
            List<string> vars = new List<string>();

            //Check if a namedCell already exists
            if (namedCells.TryGetValue(name, out Cell? cell))
            {
                //If the old content was a Formula, we need to get rid of those old dependencies.
                if (cell.GetContent().GetType().Equals(typeof(Formula)))
                {
                    Formula oldFormulaContent = (Formula)cell.GetContent();
                    //Update the DependencyGraph
                    vars = (List<string>)oldFormulaContent.GetVariables();
                    foreach (string var in vars)
                    {
                        graph.RemoveDependency(var, name);
                    }
                }

                //update the content of the named cell
                cell.SetContent(number);
            }
            //We haven't named a cell by that name yet, so "create" a new Cell and set the value, add it to the collection of namedCells
            else
            {
                Cell newCell = new Cell(name, number);
                namedCells.Add(name, newCell);
            }

            //Get all direct and indirect dependents of this cell
            LinkedList<string> list = (LinkedList<string>)GetCellsToRecalculate(name);

            return list.ToList<string>();
        }

        /// <summary>
        /// The contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, string text)
        {
            List<string> vars = new List<string>();

            //Check if a namedCell already exists
            if (namedCells.TryGetValue(name, out Cell? cell))
            {
                //If the old content was a Formula, we need to get rid of those old dependencies.
                if (cell.GetContent().GetType().Equals(typeof(Formula)))
                {
                    Formula oldFormulaContent = (Formula)cell.GetContent();
                    //Update the DependencyGraph
                    vars = (List<string>)oldFormulaContent.GetVariables();
                    foreach (string var in vars)
                    {
                        graph.RemoveDependency(var, name);
                    }
                }

                //update the content of the named cell
                if (text != "")
                {
                    cell.SetContent(text);
                }
                else
                    namedCells.Remove(name);
            }
            //We haven't named a cell by that name yet, so "create" a new Cell and set the value, add it to the collection of namedCells
            else 
            {
                if (text != "")
                {
                    Cell newCell = new Cell(name, text);
                    namedCells.Add(name, newCell);
                }
            }
                    


            //Get all direct and indirect dependents of this cell
            LinkedList<string> list = (LinkedList<string>)GetCellsToRecalculate(name);

            //takes care of edge case where someone input a cell as "", we don't want to recalulate that value, it will cause issues.
            if (text == "")
                list.RemoveFirst();

            return list.ToList<string>();
        }

        /// <summary>
        /// If changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula. The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            LinkedList<string> list = new LinkedList<string>();
            List<string> vars = new List<string>();

            //Check if a namedCell already exists
            if (namedCells.TryGetValue(name, out Cell? cell))
            {
                //If the old content was a Formula, we need to get rid of those old dependencies.
                if(cell.GetContent().GetType().Equals(typeof(Formula)))
                {
                    Formula oldFormulaContent = (Formula)cell.GetContent();
                    //Update the DependencyGraph
                    vars = (List<string>)oldFormulaContent.GetVariables();
                    foreach (string var in vars)
                    {
                        graph.RemoveDependency(var, name);
                    }
                }

                //Save the old content just in case
                object oldContent = cell.GetContent();
                object oldValue = cell.GetValue();

                //update the content of the named cell
                cell.SetContent(formula);

                //Update the DependencyGraph
                vars = (List<string>)formula.GetVariables();
                foreach (string var in vars)
                {
                    graph.AddDependency(var, name);
                }

                //Check for Circular Exception
                try
                {
                    list = (LinkedList<string>)GetCellsToRecalculate(name);
                }
                catch (CircularException e)
                {
                    //We found a Circular Exception: reset the cell to it's old content, then throw the exception.
                    cell.SetContent(oldContent);
                    cell.SetValue(oldValue);

                    foreach (string var in vars)
                    {
                        graph.RemoveDependency(var, name);
                    }
                    //If the old content was a Formula, we have to restore the old dependencies.
                    if(oldContent.GetType().Equals(typeof(Formula)))
                    {
                        Formula oldFormulaContent = (Formula)oldContent;
                        //Update the DependencyGraph
                        vars = (List<string>)oldFormulaContent.GetVariables();
                        foreach (string var in vars)
                        {
                            graph.AddDependency(var, name);
                        }
                    }

                    throw e;
                }
            }
            //We haven't named a cell by that name yet, so "create" a new Cell and set the value, add it to the collection of namedCells
            else
            {
                Cell newCell = new Cell(name, formula);
                namedCells.Add(name, newCell);

                //Update DependencyGraph
                vars = (List<string>)formula.GetVariables();
                foreach (string var in vars)
                {
                    graph.AddDependency(var, name);
                }

                //Check for Circular Exception
                try
                {
                    list = (LinkedList<string>)GetCellsToRecalculate(name);
                }
                catch (CircularException e)
                {
                    //We found a Circular Exception: reset the cell to it's old content, then throw the exception.
                    SetCellContents(name, "");

                    foreach (string var in vars)
                    {
                        graph.RemoveDependency(var, name);
                    }

                    throw e;
                }
            }

            return list.ToList<string>();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            List<string> vars = new List<string>();

            name = Normalize(name);

            //Check for valid cell name
            if (isValidName(name))
            {
                //See if our content is a double, if so, set the cell contents to that double
                if (Double.TryParse(content, out double result))
                {
                    vars = (List<string>)SetCellContents(name, result);
                }
                //Deal with Formulas (a string that starts with "=")
                else if (content.StartsWith("="))
                {
                    //Remove the "=" and try to create a formula
                    content = content.Remove(0, 1);
                    Formula formula = new Formula(content, Normalize, IsValid);

                    vars = (List<string>)SetCellContents(name, formula);
                }
                //Set the cell to a string of value: content.
                else
                    vars = (List<string>)SetCellContents(name, content);

                //The spreadsheet has been changed, update the bool value
                this.Changed = true;

                ReEvaluateValues(vars);

                return vars;
            }
            else
                throw new InvalidNameException();
        }

        /// <summary>
        /// Special helper needed for the GUI functionality of the Spreadsheet. Returns the Content STRING of a cell.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidNameException"></exception>
        public string GetCellContentString(string name)
        {
            name = Normalize(name);
            //A string is passed in, so we throw it into our Dictionary to get the corresponding cell.
            if (isValidName(name))
            {
                if (namedCells.TryGetValue(name, out Cell? cell))
                    //Return cell contents.
                    return cell.GetContentString();
                else
                    return "";
            }
            else
                throw new InvalidNameException();
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return graph.GetDependents(name);
        }

        /// <summary>
        /// This is a private helper method that confirms whether or not a variable name is valid in the context of a Spreadsheet.
        /// 
        /// There are two steps to making sure the name of a cell is valid. First, we check the general requirement for Spreadsheet (any number of letters followed by any number of integers).
        /// 
        /// Secondly, we compare the variable name to the given validator that was initialized at construction.
        /// 
        /// If we pass both validations, return true, return false in all other cases.
        /// </summary>
        /// <param name="name">Name of cell in question</param>
        /// <returns>True if name is valid, false otherwise.</returns>
        private bool isValidName(string name)
        {
            Regex varCheck = new Regex(@"^[a-zA-Z](?:[a-zA-Z]|\d)*$");
            if (varCheck.IsMatch(name))
            {
                if (this.IsValid(name))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// This method takes any given string that represents a variable, and attempts to return the value of that variable.
        /// This value is defined in the cell by the name of param var. If there is no value of type double within the named cell, then this
        /// method will throw an ArgumentException.
        /// </summary>
        /// <param name="var">A string representing the name of a cell in a spreadsheet.</param>
        /// <returns>A double, the value of the cell, or throws an ArgumentException if no value of type double is found in the Cell.</returns>
        private double Lookup(string var)
        {
            object value = GetCellValue(var);

            if(value.GetType().Equals(typeof(double)))
            {
                return (double)value;
            }
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// This method takes in a list of variables that need to have their values re-evaluated, and does that, setting their new values as it goes.
        /// </summary>
        /// <param name="vars">An IList<string> of variable names that need to be re-evaluated. The order of this list DOES matter, and the first item will be evaluated first, so on and so forth.</string></param>
        private void ReEvaluateValues(IList<string> vars)
        {
            foreach(string var in vars)
            {
                Cell cell = namedCells[var];

                //If this cell has a content of Double, then set it's value to that double.
                if (cell.GetContent().GetType().Equals(typeof(double)))
                {
                    cell.SetValue(cell.GetContent());
                }
                //If this cell has a content type of string, then set the value to that string.
                else if(cell.GetContent().GetType().Equals(typeof(string)))
                {
                    cell.SetValue(cell.GetContent());
                }
                //The content is a formula, evaluate it.
                else
                {
                    Formula formula = (Formula)cell.GetContent();
                    cell.SetValue(formula.Evaluate(Lookup));
                }
            }
        }


        /// <summary>
        /// Class that represents a cell in a spreadsheet.
        /// 
        /// A Cell is defined by having a name (string), contents (string, double, or Formula), and a value (string, double, or FormulaError)
        /// 
        /// 
        /// CELL NAMES
        /// 
        /// A string is a valid cell name if and only if:
        ///   (1) its first character is an underscore or a letter
        ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
        /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
        /// 
        /// 
        /// CELL CONTENTS
        /// 
        /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
        /// contents is an empty string, we say that the cell is empty.
        /// 
        /// CELL VALUES
        /// 
        /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.
        /// 
        /// If a cell's contents is a string, its value is that string.
        /// 
        /// If a cell's contents is a double, its value is that double.
        /// 
        /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
        /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
        /// of course, can depend on the values of variables.  The value of a variable is the 
        /// value of the spreadsheet cell it names (if that cell's value is a double) or 
        /// is undefined (otherwise).
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        private class Cell
        {
            private string name;
            [JsonProperty(PropertyName ="stringForm")]
            private string? contentString;

            private object content;
            private object value;

            public Cell()
            {
                content = "";
                value = "";
                contentString = "";
                name = "";
            }

            public Cell(string s, object content)
            {
                this.name = s;
                this.content = content;
                this.value = "";

                //Set up a string version of the Cell contents for serialzation.
                //The code in this if statement is to take care of adding a "=" in front of our content string if the content is a Formula.
                if(content.GetType().Equals(typeof(Formula)))
                {
                    this.contentString = content.ToString();
                    if(contentString != null)
                        contentString = contentString.Insert(0, "=");
                }
                else
                    this.contentString = content.ToString();
            }

            public object GetContent()
            {
                return content;
            }

            public void SetContent(object newValue)
            {
                content = newValue;

                //Set the new content string, accounting for potential formulas as in the constructor.
                if (content.GetType().Equals(typeof(Formula)))
                {
                    this.contentString = content.ToString();
                    if (contentString != null)
                        contentString = contentString.Insert(0, "=");
                }
                else
                    this.contentString = content.ToString();
            }

            public object GetValue()
            {
                return value;
            }

            public void SetValue(object newValue)
            {
                value = newValue;
            }

            public string GetContentString()
            {
                if (contentString != null)
                    return contentString;
                //Must be here to take care of null issues, won't ever be used though.
                else
                    return "";
            }
        }
    }
}
