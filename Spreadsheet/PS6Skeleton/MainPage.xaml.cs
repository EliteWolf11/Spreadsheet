using Newtonsoft.Json.Linq;
using SpreadsheetUtilities;
using SS;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    private String[] colName;
    Spreadsheet s;
    string lastSaved;

    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();

        //Set the dark theme to active
        Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Dark;

        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(2, 3);

        //Keep track of the last saved location.
        lastSaved = saveLabel.Text;

        //Create the backing Spreadsheet.
        s = new Spreadsheet(s => true, s => s.ToUpper(), "ps6") ;

        //Array to quickly get Column names in letter form.
        colName = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        //Initialize cell name and value labels.
        cellNameLabel.Text = "C4";
        cellValueLabel.Text = "Cell Value:";
    }

    /// <summary>
    /// Sets the display up for the currently selected cell.
    /// </summary>
    /// <param name="grid"></param>
    private void displaySelection(SpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.GetContent(col, row, out string content);

        //Update the labels that track cell name and value and content.
        cellContentEntry.Text = content;
        cellValueLabel.Text = "Cell Value: " + s.GetCellValue(colName[col] + (++row).ToString()).ToString();
        cellNameLabel.Text = colName[col] + (row);
    }

    /// <summary>
    /// Functionality for when the "new" menu button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NewClicked(Object sender, EventArgs e)
    {
        //Check to see if there are unsaved changes.
        if (s.Changed)
        {
            DisplayAlert("Unsaved Changes", "There are unsaved changes. Please save this spreadsheet before performing this action.", "OK");
            return;
        }

        //Wipe the display and create a new spreadsheet.
        spreadsheetGrid.Clear();
        s = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");
    }

    /// <summary>
    /// For when "Save Location" menu button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void LocationClicked(Object sender, EventArgs e)
    {
        //Allow the user to type in a new save location.
        string location = await DisplayPromptAsync("Save Location:", "Enter your desired save location.");

        //So long as there is content, set it as the new save location.
        if(location!=null)
        {
            saveLabel.Text = location;
        }
    }

    /// <summary>
    /// For when the "Save" menu button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveClicked(Object sender, EventArgs e)
    {
        //Check for file overwriting
        if(saveLabel.Text != lastSaved)
        {
            bool answer = await DisplayAlert("Warning", "You are about to overwrite a different exisiting file. Continue?", "Continue", "Cancel");

            //If user cancels, stop attempting to save.
            if (!answer)
                return;
        }

        //Save the current spreadsheet to the default location.
        try
        {
            s.Save(saveLabel.Text);
        }
        catch
        {
            //Catch any save errors.
            _=DisplayAlert("Save Error", "There was an issue saving the file, attempt to save has been aborted.", "OK");
            return;
        }

        //Popup to alert of successful save.
        _=DisplayAlert("Save", "File has successfully saved.", "OK");
    }

    /// <summary>
    /// For when the "Help" menu button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HelpClicked(Object sender, EventArgs e)
    {
        //Display the help menu.
        DisplayAlert(
            "Help Menu:", 
            "Welcome to my Spreadsheet! Here is some information about how to use it: \n\n"
            
            + "Basic Functionality:\n"
            + "To select a cell, simply click on it with your mouse. You'll notice "
            + "three main boxes at the top of the screen. The far left box displays "
            + "the name of the current cell you have selected. The far right box "
            + "displays the value of the cell you have selected. The second box is "
            + "multipurpose: it displays the contents of the cell you have selected, "
            + "and is also where you can click to make edits to the contents of that cell. \n\n"
            + "To edit the selected cell, simply enter whatever your desired content is, "
            + "then hit the 'enter' or 'return' key. This will set the change in the spreadsheet. \n\n"
            + "This spreadsheet follows some basic rules about content. If you enter any number, "
            + "will register as that number in the spreadsheet. If you enter any text, that cell "
            + "value will simply be that text. To create a function, you must start the content with "
            + "the '=' sign, denoting that it is a function. At this point, you can enter any content "
            + "that follows the general rules of Formulas. This includes putting other cell dependencies "
            + "into the Formula. These contents are case insensitive. a4 and A4 will be treated the same. \n\n"
            + "There are a number of menu items in the spreadsheet. 'File' contains the 'New' button, which "
            + "creates a new spreadsheet. The 'Open' button will allow you to select any file to open "
            + "in the spreadsheet. Note that it must be a JSON file in the form that our spreadsheet can "
            + "read. These should be marked with the .sprd extension. The 'Help' Button brings you to this "
            + "menu. There is also a second menu bar item called 'Saving'. It contains the 'Save' button, "
            + "which will save the current spreadsheet to the save location listed at the top of your "
            + "screen in the app. The 'Save Location' button will allow you to set the save location of "
            + "your spreadsheet, acting as a 'Save As' feature of sorts. WARNING: you MUST include a full "
            + "file path for your save location for it to function as expected. There may be issues "
            + "if you don't do this. Also, you MUST include .sprd at the end of your save location. \n\n"
            + "There are two main types of Formula Errors you may see displayed in your spreadsheet while "
            + "using it. The first will be #DIV/0!, which is saying that you have a Divide by Zero error. "
            + "The second is #BADVAR. You will see this error if you input a variable that doesn't "
            + "have a value usable by the Formula the variable is in (such as text, or an empty cell). \n\n"
            + "Extra Feature: \n"
            + "The extra feature I've included is one that I'm quite proud of: a fully functional find/replace feature!" +
            " This feature will allow a user to type in the 'Find' entry box any CONTENT that they want to find in the spreadsheet" +
            " and a popup will inform them of ALL cells containing that content, if any. After this, the user may enter any content" +
            " that they desire into the entry box in the popup and click 'OK'. If they do, EVERY cell that was found will have it's contents" +
            " replaced with the new content, and this will also update ALL dependencies. Please be aware of some things: I have NOT implemented" +
            " multi-threading, so if you replace a LOT of cells that have a LOT of dependencies, the program may choke up for a bit due to running" +
            " on only one thread. I may choose to update this in the future to be efficient and more functional. This will function as a 'Replace All'," +
            " as of right now you cannot pick and choose which cells to replace with new content."
            
            ,"Done"
            );
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
       //Check to see if there are unsaved changes.
       if(s.Changed)
        {
            _=DisplayAlert("Unsaved Changes", "There are unsaved changes. Please save this spreadsheet before performing this action.", "OK");
            return;
        }

        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                //Update the save location in the GUI
                saveLabel.Text = fileResult.FullPath;
                //Update the last saved location (this is to watch for and prevent overwriting files)
                lastSaved = saveLabel.Text;

                //Clear the visual aspect of the grid.
                spreadsheetGrid.Clear();

                //Load a spreadsheet from the chosen filepath.
                s = new Spreadsheet(fileResult.FullPath, s => true, s => s.ToUpper(), "ps6");
                //Get all nonempty cells in the loaded sheet
                List<string> cellList = (List<string>)s.GetNamesOfAllNonemptyCells();

                //Loop through all nonempty cells.
                foreach(string cell in cellList)
                {
                    //Convert the cellName into col and row
                    string[] splitString = Regex.Split(cell, @"([0-9]+)");
                    int s2 = int.Parse(splitString[1]);

                    int cellCol = Array.IndexOf(colName, splitString[0]);
                    int cellRow = s2 - 1;

                    //Hold the cell value in an object
                    object value = s.GetCellValue(colName[cellCol] + (cellRow + 1).ToString());

                    //Check for a FormulaError so we can properly display it
                    if (value.GetType().Equals(typeof(FormulaError)))
                    {
                        FormulaError err = (FormulaError)value;
                        value = err.Reason;
                    }

                    //Set the value of the cell so that the value draws in the grid.
                    spreadsheetGrid.SetValue(cellCol, cellRow, value.ToString());
                    //Set the content of the cell
                    spreadsheetGrid.SetContent(cellCol, cellRow, s.GetCellContentString(cell).ToString());

                    //Get the current mouse position and content of that position.
                    spreadsheetGrid.GetSelection(out int col, out int row);
                    spreadsheetGrid.GetContent(col, row, out string content);

                    //Update the labels that track cell name and value and content.
                    cellContentEntry.Text = content;
                    cellValueLabel.Text = "Cell Value: " + s.GetCellValue(colName[col] + (++row).ToString()).ToString();
                    cellNameLabel.Text = colName[col] + (row);

                }
            }
        }
        //Popup error message if loading failed.
        catch
        {
            _ = DisplayAlert("Open File Error", "There was an issue attempting to open the selected file", "OK");
        }
    }

    /// <summary>
    /// What to do when an entry box is "completed" (either tab or return is hit).
    /// 
    /// This event will first set the content of the cell in the SPREADSHEET. It will catch any exceptions and display a popup error if the entry was invalid in some way.
    /// Next, it will set the content of the cell within the grid. After that, we loop through all cell dependencies to update the values and contents and make sure that those changes are displayed in our GUI.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EntryCompleted(Object sender, EventArgs e)
    {
        //Text currently entered in the entry.
        string text = ((Entry)sender).Text;

        //Get the current mouse selection
        spreadsheetGrid.GetSelection(out int col, out int row);

        //Clear grid display for this cell if the entry was empty.
        if (text.Equals(""))
        {
            spreadsheetGrid.SetValue(col, row, "");
        }

        //The spreadsheet needs to add one to the row to display the correct value, so we do this with a separate var.
        int sprRow = row + 1;

        //Update the spreadsheet
        List<string> updateList = new List<string>();
        //Use a try/catch block to catch exceptions and display an error message without breaking the program.
        try
        {
            updateList = (List<String>)s.SetContentsOfCell(colName[col] + (sprRow).ToString(), text);
        }
        catch
        {
            //An exception was hit, so popup an error message and break out of this Event (effectively canceling the attempt to enter in an invalid entry)
            DisplayAlert("Error", "Something went wrong when trying to set the contents of this cell. Perhaps you entered an invalid variable name?", "OK");
            return;
        }
        

        //Set the content of the cell
        spreadsheetGrid.SetContent(col, row, text);

        //At this point, we need to both set up this cell, and update potential values for all cells with dependencies to this cell.
        foreach (string cellName in updateList)
        {
            //Convert the cellName into col and row
            string[] splitString = Regex.Split(cellName, @"([0-9]+)");
            int s2 = int.Parse(splitString[1]);

            int cellCol = Array.IndexOf(colName, splitString[0]);
            int cellRow = s2 - 1;

            //Hold the cell value in an object
            object value = s.GetCellValue(colName[cellCol] + (cellRow + 1).ToString());

            //Check for a FormulaError so we can properly display it
            if (value.GetType().Equals(typeof(FormulaError)))
            {
                FormulaError err = (FormulaError)value;
                value = err.Reason;
            }

            //Set the value of the cell so that the value draws in the grid.
            spreadsheetGrid.SetValue(cellCol, cellRow, value.ToString());
            //Update the value label
            cellValueLabel.Text = "Cell Value: " + value.ToString();
        }
    }

    /// <summary>
    /// This method will search the spreadsheet for any cell with CONTENTS
    /// equal to the given entry, and use a DisplayAlert to notify the users
    /// of all cells containing that content.
    /// 
    /// After showing the cells containing the content, users have the option to use a Replace function
    /// to replace every found cell with some new content. This will update all cells as expected, including potential dependencies.
    /// 
    /// WARNING: as of right now, this could be a very heavy operation depending on the amount of replacements and their dependencies. 
    /// I may update this code one day to use multi-threading, but as of right now, all this will run on the same thread.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void findCompleted(Object sender, EventArgs e)
    {
        //Text currently entered in the entry.
        string text = ((Entry)sender).Text;

        //Get all our nonempty cells
        List<string> cellList = (List<string>)s.GetNamesOfAllNonemptyCells();

        //Loop through the list, searching for content strings equal to the entry. Save these in a new list.
        List<string> matches = new List<string>();

        //Loop through each cell, if the content string matches, add it to our list
        foreach(string cell in cellList)
        {
            if(text.Equals(s.GetCellContentString(cell)))
                matches.Add(cell);
        }

        //If we have something in the match list, display the cells we found
        if (matches.Count > 0)
        {
            //THIS IS THE REPLACE FUNCTION FROM HERE ON OUT
            string replaceWith = await DisplayPromptAsync("Find Feature:", "We found the following cells contain the content you're searching for: " + String.Join(", ", matches) + ".\n\n" + "Replace With:");

            //If user entered a replacement
            if (replaceWith != null)
            {
                //Loop through each matched cell and update it and it's dependencies.
                foreach (string cellName in matches)
                {
                    //Convert the cellName into col and row
                    string[] splitString = Regex.Split(cellName, @"([0-9]+)");
                    int s2 = int.Parse(splitString[1]);

                    int cellCol = Array.IndexOf(colName, splitString[0]);
                    int cellRow = s2 - 1;

                    //Clear grid display for this cell if the entry was empty.
                    if (replaceWith.Equals(""))
                    {
                        spreadsheetGrid.SetValue(cellCol, cellRow, "");
                    }

                    //Update the spreadsheet
                    List<string> updateList = new List<string>();
                    //Use a try/catch block to catch exceptions and display an error message without breaking the program.
                    try
                    {
                        updateList = (List<String>)s.SetContentsOfCell(cellName, replaceWith);
                    }
                    catch
                    {
                        //An exception was hit, so popup an error message and break out of this Event (effectively canceling the attempt to enter in an invalid entry)
                        _ = DisplayAlert("Error", "Something went wrong when trying to set the contents of this cell. Perhaps you entered an invalid variable name?", "OK");
                        return;
                    }

                    //Set the content of the cell
                    spreadsheetGrid.SetContent(cellCol, cellRow, replaceWith);

                    //At this point, we need to both set up this cell, and update potential values for all cells with dependencies to this cell.
                    foreach (string cell in updateList)
                    {
                        //Convert the cellName into col and row
                        string[] split = Regex.Split(cell, @"([0-9]+)");
                        int s3 = int.Parse(split[1]);

                        int Col = Array.IndexOf(colName, split[0]);
                        int Row = s3 - 1;

                        //Hold the cell value in an object
                        object value = s.GetCellValue(colName[Col] + (Row + 1).ToString());

                        //Check for a FormulaError so we can properly display it
                        if (value.GetType().Equals(typeof(FormulaError)))
                        {
                            FormulaError err = (FormulaError)value;
                            value = err.Reason;
                        }

                        //Set the value of the cell so that the value draws in the grid.
                        spreadsheetGrid.SetValue(Col, Row, value.ToString());
                        //Update the value label
                        cellValueLabel.Text = "Cell Value: " + value.ToString();
                    }
                }
            }
        }
        //Inform the user there were no matches.
        else
            _=DisplayAlert("Find Feature:", "There were no cells with the content you're looking for.", "OK");
    }
}
