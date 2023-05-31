Author: Connor Blood
Version: 1.0 (10/21/22)

10/21/22
As of right now, this is the finished project for PS6. I will be including as much info as I can here.

USE:

First, note that the following info is found in my Help Menu within the Spreadsheet, accessed by clicking on "File", then "Help".

Basic Functionality:
To select a cell, simply click on it with your mouse. You'll notice four main boxes at the top of the screen. The far left box displays the name of the current cell you have selected. 
The far right box displays the value of the cell you have selected. The second box is multipurpose: it displays the contents of the cell you have selected, and is also where you can 
click to make edits to the contents of that cell.

To edit the selected cell, simply enter whatever your desired content is, then hit the 'enter' or 'return' key. This will set the change in the spreadsheet.
            
This spreadsheet follows some basic rules about content. If you enter any number, will register as that number in the spreadsheet. If you enter any text, that cell 
value will simply be that text. To create a function, you must start the content with the '=' sign, denoting that it is a function. At this point, you can enter any 
content that follows the general rules of Formulas. This includes putting other cell dependencies into the Formula. These contents are case insensitive. a4 and A4 
will be treated the same.

There are a number of menu items in the spreadsheet. 'File' contains the 'New' button, which creates a new spreadsheet. The 'Open' button will allow you to select any file to open 
in the spreadsheet. Note that it must be a JSON file in the form that our spreadsheet can read. These should be marked with the .sprd extension. The 'Help' Button brings you to this 
menu. There is also a second menu bar item called 'Saving'. It contains the 'Save' button, which will save the current spreadsheet to the save location listed at the top of your 
screen in the app. The 'Save Location' button will allow you to set the save location of your spreadsheet, acting as a 'Save As' feature of sorts. WARNING: you MUST include a full 
file path for your save location for it to function as expected. There may be issues if you don't do this. Also, you MUST include .sprd at the end of your save location. 

There are two main types of Formula Errors you may see displayed in your spreadsheet while using it. The first will be #DIV/0!, which is saying that you have a Divide by Zero error. 
The second is #BADVAR. You will see this error if you input a variable that doesn't have a value usable by the Formula the variable is in (such as text, or an empty cell).
            
Extra Feature:
The extra feature I've included is one that I'm quite proud of: a fully functional find/replace feature! This feature will allow a user to type in the 'Find' entry box any CONTENT
that they want to find in the spreadsheet and a popup will inform them of ALL cells containing that content, if any. After this, the user may enter any content that they desire into 
the entry box in the popup and click 'OK'. If they do, EVERY cell that was found will have it's contents replaced with the new content, and this will also update ALL dependencies. 
Please be aware of some things: I have NOT implemented multi-threading, so if you replace a LOT of cells that have a LOT of dependencies, the program may choke up for a bit due to running
on only one thread. I may choose to update this in the future to be efficient and more functional. This will function as a 'Replace All', as of right now you cannot pick and choose which 
cells to replace with new content.



DESGIN CHOICES:

Exceptions thrown by the code are all handled by DisplayAlerts notifying the user of any issue with whatever they tried to do. The nature of the DisplayAlert reflects the general (not specific)
issue with the action. For example, if a user enters a bad var, it will mention an issue with the entry. The same message is displayed for CircularExceptions, simply that there is an issue with
the entry. 

I made one modification to my Spreadsheet class. I added a public method called GetCellContentString that is simply an accessor for the "content string" of any cell. This content string is the version
of content of a cell that includes the '=' for a formula. Adding this method made it MUCH easier to work with certain aspects of my code. The content string already existed, I just previously had
no way of accessing it easily from the MainPage.

I changed the Formula class by changing the creation Reasons for the FormulaError objects to reflect in the GUI of the spreadsheet error, such as changing "Divide by 0 Error" to the smaller "#DIV/0!"

No external or additional addons were used in the creation of this spreadsheet. All I used was my code and the given skeletons/demos. I partially chose to do this to learn MAUI's base functionality better
as well as to not have to worry about other users having to get addons or additional things to run my spreadsheet.

As of right now, for the functionality of my Find/Replace method I simply reused a lot of code from the EntryCompleted for the replace feature. Right now it's a double nested for loop, which definitly 
isn't ideal, but it's all I had time for in the scope of this project. On top of this potentially heavy runtime for large amounts of replacements, the project is not using concurrency right now. I 
plan to eventually add this feature and clean up the code of Find/Replace as a personal project, but for the sake of PS6 what I did fit my time and it functions well enough. As part of this optimizing for the
future, I would also probably create a helper method of some sort for the EntryCompleted/ Find/Replace features. This would prevent the duplicate code and make everything look nicer. This would've required more 
time than I had for the scope of the project.



FUTURE IMPLEMENATION:

This section is mainly for my personal use as a notebook of things I hope to implement in the future as a personal project.

1. Clean up code (EntryCompleted/FindCompleted duplicate code)
2. Multithreading
3. Typing into cells without having to click the entry box
4. Make a nicer GUI
5. Have the Row and Column label highlight as a different color to make it easier to see which cell is selected.
6. Clean up the Help popup to be easier to read and navigate.
7. Figure out a way to make saving an easier and more seamless experience for the user (don't have to worry about manually typing .sprd, typing out a whole filepath, etc.)