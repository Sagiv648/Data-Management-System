using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using SQL_CSharp_final_project.Models;


namespace SQL_CSharp_final_project.Shell
{

    public class Shell
    {
        
      
        public delegate void paramCommand(string[] cmd);
        public delegate void generalCommand();
        

        public static List<Database> totalDatabases = new List<Database>();
        public static Database contextDatabase = null;
        public static Table contextTable = null;
        public static ConsoleColor response = ConsoleColor.DarkCyan;
        public static Dictionary<string, paramCommand> parameterCommandsExecution = new Dictionary<string, paramCommand>();
        public static Dictionary<string, generalCommand> nonParameterCommandsExecution = new Dictionary<string, generalCommand>();
        public static string[] location = new string[4];
        public static string buffer;
        public static void shellInit()
        {
            
            location[3] = "$";

            Console.ForegroundColor = response;

            if (!Connection.readSettingsFromFile())
            {
                Console.WriteLine("Set up your server connection using 'setup-connection' to begin working.");

            }
            else
            {
                populateTotalDatabases();
            }
            
           
            
            
            nonParameterCommandsExecution.Add("help", execHelp);
            nonParameterCommandsExecution.Add("exit", execExit);
            nonParameterCommandsExecution.Add("setup-connection", execSetupConnection);
            nonParameterCommandsExecution.Add("show-data", execShowData);
            nonParameterCommandsExecution.Add("db-all", execDatabaseAll);
            nonParameterCommandsExecution.Add("tables-all", execTablesAll);
            nonParameterCommandsExecution.Add("clear", execClear);
            
            //---
            parameterCommandsExecution.Add("find", execFind);
            parameterCommandsExecution.Add("enter-table", execEnterTable);
            parameterCommandsExecution.Add("enter-database", execEnterDatabase);
            parameterCommandsExecution.Add("delete", execDeleteData);
            parameterCommandsExecution.Add("create-database", execCreateDatabase);
            parameterCommandsExecution.Add("create-table", execCreateTable);
            parameterCommandsExecution.Add("set-key-relations", execSetKeyRelations);
            parameterCommandsExecution.Add("select-data", execSelectColumns);
            parameterCommandsExecution.Add("update", execUpdateData);
            parameterCommandsExecution.Add("insert-data", execInsertData);
            parameterCommandsExecution.Add("task", taskCommand);
            Models.Task.initTasks();

        }

        public static void parseCommand(string cmd)
        {
            string[] command = cmd.Split(' ');
            if(command.Length == 1)
            {
                try
                {
                    nonParameterCommandsExecution[command[0]]();
                }
                catch (Exception)
                {

                    execError();
                    return;
                }
                return;
            }
            try
            {
                parameterCommandsExecution[command[0]](command);
            }
            catch (Exception)
            {
                execError();
                return;
            }
            return;
        
        }

        public static string shellInput(string[] location)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string item in location)
            {
                Console.Write($"{item}");
            }
            Console.Write(" ");
            return Console.ReadLine();
        }
        //--------------------------------------------------------
        public static void execHelp()
        {
            Console.ForegroundColor = response;
            Console.WriteLine("help -> Allows the user to check all available commands.", response);
            Console.WriteLine("clear -> Clears the shell screen.");
            Console.WriteLine("setup-connection -> Allows the user to setup connection to a different server.", response);
            Console.WriteLine("enter-database [Database_Name] -> Enters the forward nested space(Server->DB, DB->Table).", response);
            Console.WriteLine("enter-table [Table_Name] -> Enters the specified table in the current database's context.",response);
            Console.WriteLine("exit -> Exits to the previous nested space(Table->DB, DB->Server).", response);
            Console.WriteLine("db-all -> Displays a list of all the existing databases within the space of the current server.", response);
            Console.WriteLine("tables-all -> Displays a list of all the existing tables within the space of the current database.", response);
            Console.WriteLine("create-database [Database_Name] -> Allows the user to create a new database.", response);
            Console.WriteLine("create-table [Table_Name] -> Allows the user to create a new table within the context of the current database.", response);
            Console.WriteLine("insert-data [Table_Name] -> Allows the user to insert data into the current space of the table.", response);
            Console.WriteLine("update [Table_Name] -> Allows the user to update data in a table based on a predicate.", response);
            Console.WriteLine("delete -> Allows the user to remove data from the current space of the table.", response);
            Console.WriteLine("find [Column_Name] [Data]-> Displays all the data according to the command's parameters.", response);
            Console.WriteLine("show-data -> Allows the user to view all the data inside the context of the current table.", response);
            Console.WriteLine("select-data [Column_Name ...] -> Allows the user to view the data from the selected columns.", response);
            Console.WriteLine("Task [Sum/Avg/Max/Min] [Column_Name] -> Allows the user to perform a task on the data of a particular column.", response);
            Console.WriteLine("set-key-relations [Referencer_Table-Referencer_Column] [Referenced_Table-Referenced_Column] -> Allows the user to set up Foreign Key - Primary Key relations.", response);
            Console.WriteLine("quit -> Terminates the program.", response);
        }

        public static void execClear()
        {
            Console.Clear();
        }
        public static void execExit()
        {
            Console.ForegroundColor = response;
            if(contextDatabase != null && contextTable != null)
            {
                Console.WriteLine($"Exitting table {contextTable.TName}");
                contextTable = null;
                location[2] = "";
                return;
            }
            else if(contextDatabase != null && contextTable == null)
            {
                Console.WriteLine($"Exitting database {contextDatabase.Name}");
                contextDatabase = null;
                location[1] = "";
                return;
            }
            Console.WriteLine("Cannot exit further.");
            
        }
        public static void execCreateDatabase(string[] cmd)
        {
            Console.ForegroundColor = response;
            if(cmd.Length == 1)
            {
                Console.WriteLine("Name of the database to be created is needed.");
                return;
            }
            else if (Database.databaseExists(totalDatabases, cmd[1]))
            {
                Console.WriteLine($"Database named {cmd[1]} already exists.");
                return;
            }

            if(Database.createDatabase(new Database(cmd[1])))
            {
                Console.WriteLine($"Database named {cmd[1]} was created in {location[0]}");
                return;
            }
            execError();
            
            

        }
        public static void execCreateTable(string[] cmd)
        {
            if(cmd.Length == 1)
            {
                Console.WriteLine("Name of the table to be created is needed.");
                return;
            }
            else if(contextDatabase != null && contextTable != null)
            {
                Console.WriteLine($"Exit the context of table {contextTable.TName} to create a table.");
                return;
            }
            else if(contextDatabase == null)
            {
                Console.WriteLine("Enter the context of a database to create a table.");
                return;
            }
            else if(contextDatabase != null && !contextDatabase.tableExists(cmd[1]))
            {
                Dictionary<string, string> userInputBuffer = new Dictionary<string, string>(7);
                userInputBuffer.Add("Col Name", "");
                userInputBuffer.Add("Col Type", "");
                userInputBuffer.Add("Col Type Length", "0");
                userInputBuffer.Add("Is-Nullable", "");
                userInputBuffer.Add("Is-PrimaryKey", "");


                int i = 1;
                Table newTable = new Table(cmd[1]);
                
                Console.Clear();
                Console.WriteLine($" Table {newTable.TName} creation screen:");
                while (true)
                {
                    Console.Write($"Enter the name of the {i}th column:");
                    userInputBuffer["Col Name"] = Console.ReadLine();
                    while (newTable.columnExists(userInputBuffer["Col Name"]))
                    {
                        Console.WriteLine($"Column {userInputBuffer["Col Name"]} already exists.");
                        userInputBuffer["Col Name"] = Console.ReadLine();

                    }
                    Console.Write($"Enter the type of the {i}th column:");
                    userInputBuffer["Col Type"] = Console.ReadLine();
                    while (!Models.Type.isValidType(userInputBuffer["Col Type"]))
                    {
                        Console.WriteLine("Invalid type, all the types which allowed are:");
                        for(int k = 0; k < Models.Type.sqlValidationTypes.Count; k++)
                        {
                            Console.Write($"{Models.Type.sqlValidationTypes[k]}, ");
                        }
                        Console.WriteLine();
                        Console.Write($"Enter the type of the {i}th column:");
                        userInputBuffer["Col Type"] = Console.ReadLine();

                        

                    }
                    if(Models.Type.isStringType(userInputBuffer["Col Type"]))
                     {
                       Console.Write("Enter the length of the characters (up to 100):");
                       userInputBuffer["Col Type Length"] = Console.ReadLine();
                       int test;
                       while (!int.TryParse(userInputBuffer["Col Type Length"], out test) || test > 100)
                        {
                         Console.Write("Enter the length of the characters (up to 100):");
                          userInputBuffer["Col Type Length"] = Console.ReadLine();
                       }
                    }
                    Console.WriteLine($"Is column {userInputBuffer["Col Name"]} nullable?(Yes/No)");
                    userInputBuffer["Is-Nullable"] = Console.ReadLine();
                    while (userInputBuffer["Is-Nullable"].ToLower() != "no" && userInputBuffer["Is-Nullable"].ToLower() != "yes")
                    {
                        Console.WriteLine($"Yes or No, is {userInputBuffer["Col Name"]} nullable?");
                        userInputBuffer["Is-Nullable"] = Console.ReadLine();
                    }
                    bool nullable = false;
                    if (userInputBuffer["Is-Nullable"].ToLower() == "yes") nullable = true;
                    Column col = new Column(i - 1,
                                        userInputBuffer["Col Name"],
                                        userInputBuffer["Col Type"],
                                        int.Parse(userInputBuffer["Col Type Length"]),
                                        false, nullable, null, null);
                    newTable.appendColumn(col);
                    Console.WriteLine($"Column {col.ColName} appended and queued.");
                    Console.WriteLine($"Current columns queued for table {newTable.TName}:");
                    for (int t = 0; t < newTable.totalColumns; t++)
                    {
                        Console.Write($"{newTable[t].ColName} - {newTable[t].ColType.CompleteType} |");

                    }
                    Console.WriteLine();
                    i++;
                    Console.Write("If you have finished appending columns to the queue, type 'stop' to continue type anything else. ");
                    userInputBuffer["Col Name"] = Console.ReadLine();
                    if (userInputBuffer["Col Name"].ToLower() == "stop") break;

                }
                Console.WriteLine($"You have finished appending columns to table {newTable.TName}.");
                Console.WriteLine("Keys configuration:");
                Console.Write($"Which column of {newTable.TName} would be the primary key:");
                userInputBuffer["Is-PrimaryKey"] = Console.ReadLine();
                while (!newTable.columnExists(userInputBuffer["Is-PrimaryKey"]))
                {
                    Console.WriteLine($"Column {userInputBuffer["Is-PrimaryKey"]} not exists in the queue.");
                    Console.Write("Pick a column from the queue: ");
                    userInputBuffer["Is-PrimaryKey"] = Console.ReadLine();
                }
                newTable[userInputBuffer["Is-PrimaryKey"]].setPrimaryKey = true;
                newTable[userInputBuffer["Is-PrimaryKey"]].setNullAble = false;

                for (int t = 0; t < newTable.totalColumns; t++)
                {
                    Console.Write($"{newTable[t].ColName} - {newTable[t].ColType.CompleteType} |");

                }
                Console.WriteLine();
                Console.Write("To halt the table's creation type 'halt', to proceed type anything else.");
                string toProceed = Console.ReadLine();

                if(toProceed.ToLower() == "halt")
                {
                    Console.WriteLine($"Halting {newTable.TName}'s creation.");
                    return;
                }

                contextDatabase.addTable(newTable);
                Console.WriteLine($"Table {newTable.TName} created successfully.");
                return;

            }
            execError();
        }
        public static void execSetKeyRelations(string[] cmd)
        {

            if(cmd.Length == 1)
            {
                Console.WriteLine("Referencer/Referenced tables and columns required.");
                return;
            }
            else if(cmd.Length == 2)
            {
                Console.WriteLine("One of your parameters is missing.");
                return;
            }
            else if(cmd.Length == 3 && contextDatabase != null)
            {
                try
                {
                    string[] subParameters;
                    int k;
                    bool[] correctSyntax = new bool[2];
                    for (int t = 1; t < cmd.Length; t++)
                    {
                        correctSyntax[0] = false;
                        correctSyntax[1] = false;
                        subParameters = cmd[t].Split('-');
                        
                        if (subParameters.Length != 2)
                        {
                            Console.WriteLine("Error occured in the parameters' syntax.");
                            return;
                        }
                        for (k = 0; k < contextDatabase.tableCount(); k++)
                        {
                            if (contextDatabase[k].TName == subParameters[0])
                            {
                                correctSyntax[0] = true;
                                for (int x = 0; x < contextDatabase[k].totalColumns; x++)
                                {
                                    if (contextDatabase[k][x].ColName == subParameters[1])
                                    {
                                        correctSyntax[1] = true;
                                        continue;
                                    }
                                }
                            }
                        }
                        if (!correctSyntax[0] || !correctSyntax[1])
                        {
                            Console.WriteLine($"Error occured in referncer/referenced table {subParameters[0]} or referencer/referenced column {subParameters[1]}.");
                            return;
                        }

                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error occured at naming parameters.");
                    return;

                    
                }

                if(contextDatabase != null)
                {
                    //set-key-relations ReferencerTable-Column ReferencedTable-Column
                    // ReferencerTable = cmd[1][0] | ReferencerColumn = cmd[1][1]
                    // ReferencedTable = cmd[2][0] | ReferencedColumn = cmd[2][1]
                    
                    SqlConnection conn = new SqlConnection(Connection.connectionString);
                    try
                    {
                        Dictionary<string, string[]> referencerAndReferenced = new Dictionary<string, string[]>(2);

                        
                        referencerAndReferenced.Add("referencerTable-Column", cmd[1].Split('-'));
                        referencerAndReferenced.Add("referencedTable-Column", cmd[2].Split('-'));

                       


                        Table referencerTable = contextDatabase[referencerAndReferenced["referencerTable-Column"][0]];
                        
                        Column referencerColumn = referencerTable[referencerAndReferenced["referencerTable-Column"][1]];
                        
                        Table referencedTable = contextDatabase[referencerAndReferenced["referencedTable-Column"][0]];
                        
                        Column referencedColumn = referencedTable[referencerAndReferenced["referencedTable-Column"][1]];
                        

                        if (referencerColumn.ColType.CompleteType != referencedColumn.ColType.CompleteType)
                        {
                            Console.WriteLine($"Columns are not compatible for key relations.");
                            Console.WriteLine($"Referencer Column: {referencerColumn.ColName}({referencerColumn.ColType.CompleteType})" +
                                $" is not compatible with the referenced column {referencedColumn.ColName}({referencedColumn.ColType.CompleteType})");
                            return;
                        }
                        if (!referencedTable[referencedColumn.ColName].isPrimaryKey)
                        {
                            Console.WriteLine(referencedTable[referencedColumn.ColName].ColName);

                            Console.WriteLine("The referenced column must be a primary key.");
                            return;
                        }
                        contextDatabase[referencerTable.TName][referencerColumn.ColName].setForeignKeyAccessTable = referencedTable;
                        contextDatabase[referencerTable.TName][referencerColumn.ColName].setForeignKeyAccessColumn = referencedColumn;

                        

                        //FK@[ReferencerTable]@[ReferencerColumn]@[ReferencedTable]@[ReferencedColumn]

                        
                        SqlCommand command = conn.CreateCommand();
                        command.CommandText = $@"use [{contextDatabase.Name}] alter table [{referencerTable.TName}]
                                                add constraint FK@{referencerTable.TName}@{referencerColumn.ColName}@{referencedTable.TName}@{referencedColumn.ColName}
                                                foreign key({referencerColumn.ColName}) references {referencedTable.TName}({referencedColumn.ColName});";
                        conn.Open();
                        command.ExecuteNonQuery();


                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        conn.Close();
                        return;

                        
                    }
                    conn.Close();
                    Console.WriteLine($"Key relations between the tables has been established.");
                    return;

                }
                Console.WriteLine("Must be within the context of a database to continue.");
                return;
            }

            execError();

            //Dictionary<string, string> userInputBuffer = new Dictionary<string, string>(3);
            //userInputBuffer.Add("FK-ReferencedTable", "");
            //userInputBuffer.Add("FK-ReferencedColumn", "");
            //userInputBuffer.Add("FK-ReferencerColumn", "");

            //Table newTable = null;
            //Console.Write("Would you like a column to reference another column in another table(Yes/No):");
            //userInputBuffer["FK-ReferencerColumn"] = Console.ReadLine();
            ////
            //while (userInputBuffer["FK-ReferencerColumn"].ToLower() != "yes" && userInputBuffer["FK-ReferencerColumn"].ToLower() != "no")
            //{
            //    Console.WriteLine("Yes or no, would you like a column to reference another column in another table?");
            //    userInputBuffer["FK-ReferencerColumn"] = Console.ReadLine();
            //}
            //if (userInputBuffer["FK-ReferencerColumn"].ToLower() == "no")
            //{

            //}
            ////FK paring functionality
            //Console.WriteLine("In order for a column to reference another column, the referenced column must be a primary key and must be of the same type and of the same length.");
            //Console.WriteLine("The foreign key must not also be a primary key.");
            //Console.WriteLine($"Table {newTable.TName} has the following columns queued:");
            //for (int t = 0; t < newTable.totalColumns; t++)
            //{
            //    Console.WriteLine($"{newTable[t].ColName} - {newTable[t].ColType.CompleteType} - Primary Key: {newTable[t].isPrimaryKey}");
            //}
            //while (true)
            //{


            //    Console.Write("Enter the name of the referencer column:");
            //    userInputBuffer["FK-ReferencerColumn"] = Console.ReadLine();
            //    while (!newTable.columnExists(userInputBuffer["FK-ReferencerColumn"]))
            //    {
            //        Console.WriteLine($"Column {userInputBuffer["FK-ReferencerColumn"]} doesn't exist in {newTable.TName}.");

            //        Console.WriteLine("Choose one of the above:");
            //        userInputBuffer["FK-ReferencerColumn"] = Console.ReadLine();


            //    }
            //    while (newTable[userInputBuffer["FK-ReferencerColumn"]].isPrimaryKey == true)
            //    {
            //        Console.Write("Foreign key must not be a primary key:");
            //        userInputBuffer["FK-ReferencerColumn"] = Console.ReadLine();
            //        while (!newTable.columnExists(userInputBuffer["FK-ReferencerColumn"]))
            //        {
            //            Console.WriteLine($"Column {userInputBuffer["FK-ReferencerColumn"]} doesn't exist in {newTable.TName}.");
            //            Console.WriteLine("Choose a column which isn't the primary key.");
            //            userInputBuffer["FK-ReferencerColumn"] = Console.ReadLine();
            //        }
            //    }
            //    Console.Write("Choose a foreign table:");
            //    userInputBuffer["FK-ReferencedTable"] = Console.ReadLine();
            //    while (!contextDatabase.tableExists(userInputBuffer["FK-ReferencedTable"]))
            //    {
            //        Console.WriteLine($"Table {userInputBuffer["FK-ReferencedTable"]} doesn't exist in {contextDatabase.Name}");
            //        for (int t = 0; t < contextDatabase.tableCount(); t++)
            //        {
            //            Console.Write($"{contextDatabase[t].TName} |");
            //        }
            //        Console.WriteLine("Pick one from the above:");
            //        userInputBuffer["FK-ReferencedTable"] = Console.ReadLine();

            //    }


            //    Console.Write($"Choose a foreign column from {userInputBuffer["FK-ReferencedTable"]}");
            //    userInputBuffer["FK-ReferencedColumn"] = Console.ReadLine();
            //    while (!contextDatabase[userInputBuffer["FK-ReferencedTable"]].columnExists(userInputBuffer["FK-ReferencedColumn"]))
            //    {
            //        Console.WriteLine($"Column {userInputBuffer["FK-ReferencedColumn"]} doesn't exist in {contextDatabase[userInputBuffer["FK-ReferencedTable"]].TName}.");
            //        Console.Write("Choose one that exists:");
            //        userInputBuffer["FK-ReferencedColumn"] = Console.ReadLine();
            //    }
            //    if (newTable[userInputBuffer["FK-ReferencerColumn"]].ColType.CompleteType !=
            //        contextDatabase[userInputBuffer["FK-ReferencedTable"]][userInputBuffer["FK-ReferencedColumn"]].ColType.CompleteType)
            //    {
            //        Console.WriteLine($"Column - {newTable[userInputBuffer["FK-ReferencerColumn"]].ColName} ({newTable[userInputBuffer["FK-ReferencerColumn"]].ColType.CompleteType})");
            //        Console.WriteLine($"Table {contextDatabase[userInputBuffer["FK-ReferencedTable"]].TName} -> Column - {contextDatabase[userInputBuffer["FK-ReferencedTable"]][userInputBuffer["FK-ReferencedColumn"]]}" +
            //            $" ({contextDatabase[userInputBuffer["FK-ReferencedTable"]][userInputBuffer["FK-ReferencedColumn"]].ColType.CompleteType})");
            //        Console.WriteLine("Types are not compatible.");
            //        continue;
            //    }
            //    if (!contextDatabase[userInputBuffer["FK-ReferencedTable"]][userInputBuffer["FK-ReferencedColumn"]].isPrimaryKey)
            //    {
            //        Console.WriteLine($"Table {contextDatabase[userInputBuffer["FK-ReferencedTable"]].TName} -> Column - {contextDatabase[userInputBuffer["FK-ReferencedTable"]][userInputBuffer["FK-ReferencedColumn"]]} is not a primary key.");
            //        continue;
            //    }
            //    Console.WriteLine($"Column - {newTable[userInputBuffer["FK-ReferencerColumn"]].ColName} ({newTable[userInputBuffer["FK-ReferencerColumn"]].ColType.CompleteType})");
            //    Console.WriteLine($"Table {contextDatabase[userInputBuffer["FK-ReferencedTable"]].TName} -> Column - {contextDatabase[userInputBuffer["FK-ReferencedTable"]][userInputBuffer["FK-ReferencedColumn"]]}" +
            //        $" ({contextDatabase[userInputBuffer["FK-ReferencedTable"]][userInputBuffer["FK-ReferencedColumn"]].ColType.CompleteType})");
            //    Console.WriteLine("Both columns are compatible for foreign key - primary key relations, procceed?(Yes/No)");
            //    userInputBuffer["Col Name"] = Console.ReadLine();
            //    while (userInputBuffer["Col Name"].ToLower() != "no" && userInputBuffer["Col Name"].ToLower() != "yes")
            //    {
            //        Console.WriteLine("Yes or No, proceed with with the keys relations?");
            //        userInputBuffer["Col Name"] = Console.ReadLine();
            //    }
            //    if (userInputBuffer["Col Name"].ToLower() == "yes")
            //    {
            //        newTable[userInputBuffer["FK-ReferencerColumn"]].setForeignKeyAccessTable = contextDatabase[userInputBuffer["FK-ReferencedTable"]];
            //        newTable[userInputBuffer["FK-ReferencerColumn"]].setForeignKeyAccessColumn = contextDatabase[userInputBuffer["FK-ReferencedTable"]][userInputBuffer["FK-ReferencedColumn"]];
            //        Console.WriteLine("Foreign key - Primary key relations has been established.");
            //        Console.WriteLine("Type 'quit' to finish the configuration or 'continue' to continue the configuration.");
            //        userInputBuffer["Col Name"] = Console.ReadLine();
            //        while (userInputBuffer["Col Name"].ToLower() != "quit" && userInputBuffer["Col Name"].ToLower() != "continue")
            //        {
            //            Console.WriteLine(" 'quit' to finish or 'continue' to continue. ");
            //            userInputBuffer["Col Name"] = Console.ReadLine();
            //        }

            //        if (userInputBuffer["Col Name"].ToLower() == "quit")
            //        {
            //            Console.WriteLine("Finishing configuration and creating the table...");
            //            contextDatabase.addTable(newTable);
            //            return;
            //        }
            //        continue;
            //    }

            //    //check it is primary key and same type


            //}
        }
        public static void execSelectColumns(string[] cmd)
        {
            if(cmd.Length == 1)
            {
                Console.WriteLine("Column names are needed.");
                return;
            }
            if(contextDatabase != null && contextTable != null)
            {
                int i;
                
                for(i = 1; i < cmd.Length ; i++)
                {
                    if (!contextTable.columnExists(cmd[i]))
                    {
                        Console.WriteLine($"Column {cmd[i]} doesn't exist in {contextTable.TName}");
                        return;
                    }
                    
                }
                
                int k;
                for(i = 1; i < cmd.Length; i++)
                {
                   
                        Console.Write($"{contextTable[cmd[i]].ColName}:> ", Console.ForegroundColor = ConsoleColor.Gray);
                        
                        for(k = 0; k < contextTable[cmd[i]].retrieveAllData.Count; k++)
                        {
                            Console.Write($"{contextTable[cmd[i]].retrieveAllData[k].getData}, ", response);
                        }
                        Console.WriteLine();
                       
                }
                return;

            }
            else
            {
                execError();
            }

        }
        public static void execTablesAll()
        {
            Console.ForegroundColor = response;
            if(contextDatabase == null)
            {

                Console.WriteLine("Context of a database is needed to execute this command.");
                return;
            }
            for(int i = 0; i < contextDatabase.tableCount(); i++)
            {
                if (contextDatabase[i].TName != "sysdiagrams") 
                Console.Write($"{contextDatabase[i].TName} | ");
            }
            Console.WriteLine();

        }
        public static void execDatabaseAll()
        {
            Console.ForegroundColor = response;
            if(location[0] == null)
            {
                Console.WriteLine("Error occured at loading time, make sure the connection settings are valid.");
                return;
            }
            else if(totalDatabases.Count == 0)
            {
                Console.WriteLine("No databases found.");
                return;
            }
            foreach (Database item in totalDatabases)
            {
                Console.Write($"{item.Name} | ");
            }
            Console.WriteLine();
        }
        public static void execSetupConnection()
        {
            Connection.connectionString = null;
            
            string inputBuffer = "";
            Console.Write("Enter the name of the server:");
            inputBuffer = Console.ReadLine();
            while (inputBuffer == "")
            {
                Console.WriteLine("Enter a valid name for the server:");
                inputBuffer = Console.ReadLine();
            }
            Connection.connectionString += $"Server={inputBuffer};";
            location[0] = $"{inputBuffer}\\";
            Console.Write("Enter the type of the server's security:");
            inputBuffer = Console.ReadLine();
            while (inputBuffer == "")
            {
                Console.WriteLine("Enter a valid name for the security type:");
                inputBuffer = Console.ReadLine();
            }
            Connection.connectionString += $"Integrated Security={inputBuffer};";
            totalDatabases.Clear();
            Models.Type.sqlValidationTypes.Clear();
            
            Connection.setupConnection();
            populateTotalDatabases();
            

        }
        public static void execShowData()
        {
            if(contextTable == null || contextDatabase == null)
            {
                Console.ForegroundColor = response;
                Console.WriteLine("Context of a table is needed to execute this command.");
                return;
            }

            displayAllDetails();
        }
        public static void execFind(string[] cmd)
        {
            Console.ForegroundColor = response;
            if(contextDatabase != null && contextTable != null)
            {
                if (!contextTable.columnExists(cmd[1]) || cmd[2] == null)
                {
                    Console.WriteLine($"Error occured while searching through table {contextTable.TName}");
                    return;
                }

                if (Models.Type.isStringType(contextTable[cmd[1]].ColType.TypeName))
                {
                    cmd[2] = cmd[2].Insert(0, "\'");
                    cmd[2] += "\'";
                }
                Console.WriteLine(cmd[2]);
                SqlConnection conn = new SqlConnection(Connection.connectionString);

                SqlDataReader read = null;

               
                try
                {


                    Dictionary<string, List<string>> queriedData = new Dictionary<string, List<string>>(contextTable.totalColumns);
                    
                    SqlCommand sqlCmd = conn.CreateCommand();
                    sqlCmd.CommandText = $@"use[{contextDatabase.Name}]
                                        select * from [{contextTable.TName}]
                                        where {cmd[1]} = {cmd[2]}";
                    conn.Open();
                    int t;
                    for(t = 0; t < contextTable.totalColumns; t++)
                    {
                        queriedData.Add(contextTable[t].ColName, new List<string>());
                        
                    }
                    Console.WriteLine();
                    read = sqlCmd.ExecuteReader();
                    
                    while (read.Read())
                    {
                        for (int i = 0; i < read.FieldCount; i++)
                        {
                            queriedData[read.GetName(i)].Add(read[i].ToString());
                            
                        }
                        
                    }
                    read.Close();

                    int j;
                    for(t = 0; t < contextTable.totalColumns; t++)
                    {
                        Console.Write($"{contextTable[t].ColName}: ",response);
                        for(j = 0; j < queriedData[contextTable[t].ColName].Count; j++)
                        {
                            Console.Write($"{queriedData[contextTable[t].ColName][j]} || ");

                        }
                        Console.WriteLine();
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                   
                    conn.Close();
                    return;
                }
                finally
                {
                    
                    conn.Close();
                    
                }


            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("You need to be within the context of a database and a table to execute this command.");
                return;

            }
        }
        public static void execEnterTable(string[] cmd)
        {
            
            Console.ForegroundColor = response;
            if (contextDatabase == null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("You need to be within the context of a database to execute this command.");
                return;

            }
            if (!contextDatabase.tableExists(cmd[1]))
            {
                Console.WriteLine($"Table {cmd[1]} doesn't exist.");
                return;
            }
            
            contextTable = contextDatabase[cmd[1]];
            location[2] = $"\\{contextTable.TName}";
        }
        public static void execEnterDatabase(string[] cmd)
        {
            Console.ForegroundColor = response;
            if (location[0] == "")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("You need to be connected to the server to execute this command.");
                return;
            }
            if(!Database.databaseExists(totalDatabases, cmd[1]))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Database {cmd[1]} doesn't exist.");
                return;
            }
            contextDatabase = Database.databaseFind(totalDatabases, cmd[1]);
            location[1] = $"\\{contextDatabase.Name}";
        }
        public static void execDeleteData(string[] cmd)
        {
            if (cmd.Length == 1)
            {
                Console.WriteLine("Name of the table is needed.", response);
                return;
            }

            if (contextDatabase != null)
            {


                Table oldContext = contextTable;
                if (!contextDatabase.tableExists(cmd[1]))
                {
                    Console.WriteLine($"Table {cmd[1]} doesn't exist in {contextDatabase.Name} database.");
                    return;
                }
                contextTable = contextDatabase[cmd[1]];

                Console.Clear();
                Console.WriteLine();
                Console.WriteLine($"Records deletion screen for {cmd[1]}");
                Console.WriteLine($"Current records for {cmd[1]}:");
                displayAllDetails();
                Console.Write("Select a column and a value as a predicate for the records deletion:\n");
                Console.WriteLine("Example [Column_Name]=[Value]");

                SqlConnection conn = new SqlConnection(Connection.connectionString);
                SqlCommand command = conn.CreateCommand();

                command.CommandText = $"use [{contextDatabase.Name}] delete from {contextTable.TName}";
                List<string> columnsToRefresh = new List<string>();
                string predicateInput = Console.ReadLine();


                string[] predicateVerification = predicateInput.Split('=');
                
                if (predicateVerification.Length != 2)
                {
                    Console.WriteLine("Invalid predicate assignment.");
                    contextTable = oldContext;
                    return;
                }
                if (Models.Type.isStringType(contextTable[predicateVerification[0]].ColType.TypeName))
                {
                    command.CommandText += $" where {predicateVerification[0]} = '{predicateVerification[1]}';";

                }
                else
                {
                    command.CommandText += $" where {predicateVerification[0]} = {predicateVerification[1]};";
                }
                int i;
                for(i = 0; i < contextTable.totalColumns; i++)
                {
                    
                   columnsToRefresh.Add(contextTable[i].ColName);
                    
                }
                
                try
                {
                    conn.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    Console.WriteLine("Error occured at deletion time, check your connection.");
                    contextTable = oldContext;
                    conn.Close();
                    
                    
                    return;
                }
                conn.Close();
                Console.WriteLine("Records deleted successfully.");
                refreshDbList(columnsToRefresh);
                Console.WriteLine();
                displayAllDetails();
                contextTable = oldContext;
                return;
            }
            execError();
        }
        public static void execInsertData(string[] cmd)
        {

            if(cmd.Length == 1)
            {
                Console.WriteLine("The name of the table is needed.", response);
                return;
            }

            if(contextDatabase != null)
            {
                if (!contextDatabase.tableExists(cmd[1]))
                {
                    Console.WriteLine($"Table {cmd[1]} doesn't exist in {contextDatabase.Name}", response);
                    return;
                }
                Table oldContext = contextTable;
                contextTable = contextDatabase[cmd[1]];
                int i;
                Dictionary<string, string> inputBuffer = new Dictionary<string, string>(contextDatabase[cmd[1]].totalColumns);
                for(i = 0; i < contextDatabase[cmd[1]].totalColumns; i++)
                {
                    inputBuffer.Add(contextDatabase[cmd[1]][i].ColName, "");
                    Console.WriteLine($"Column Name: {contextDatabase[cmd[1]][i].ColName} - Data type: {contextDatabase[cmd[1]][i].ColType.CompleteType}");
                }
                Console.WriteLine("Enter the values you wish to append to the table.");
                for(i = 0; i < contextDatabase[cmd[1]].totalColumns; i++)
                {
                    Console.Write($"{contextDatabase[cmd[1]][i].ColName} ({contextDatabase[cmd[1]][i].ColType.CompleteType}) : ");
                    inputBuffer[contextDatabase[cmd[1]][i].ColName] = Console.ReadLine();
                    
                    if (Models.Type.isNumeric(contextDatabase[cmd[1]][i].ColType.TypeName))
                    {
                        float test;
                        while(!float.TryParse(inputBuffer[contextDatabase[cmd[1]][i].ColName], out test))
                        {
                            Console.Write("Type has to be numeric:");
                            inputBuffer[contextDatabase[cmd[1]][i].ColName] = Console.ReadLine();
                        }
                        
                    }
                    else if (Models.Type.isStringType(inputBuffer[contextDatabase[cmd[1]][i].ColName]))
                    {
                        while(inputBuffer[contextDatabase[cmd[1]][i].ColName].Length > contextDatabase[cmd[1]][i].ColType.characterLen)
                        {
                            Console.Write($"{contextDatabase[cmd[1]][i].ColName} allows only {contextDatabase[cmd[1]][i].ColType.characterLen} chars:");
                            inputBuffer[contextDatabase[cmd[1]][i].ColName] = Console.ReadLine();
                        }

                    }
                    
                }

                int t = 0;

                Console.WriteLine("You have queued the following data:");
                for(t = 0; t < contextTable.totalColumns; t++)
                {
                    Console.Write($"{contextTable[t].ColName}({contextTable[t].ColType.CompleteType}): {inputBuffer[contextTable[t].ColName]}");
                    Console.WriteLine();
                }
                string wantsToCancel = "";
                Console.Write($"If you are not satisified with the data, type 'stop', for anything else type anything else: ");
                wantsToCancel = Console.ReadLine();
                if(wantsToCancel.ToLower() == "stop".ToLower())
                {
                    Console.WriteLine($"You have cancelled the data insertion for table {contextTable.TName}");
                    contextTable = oldContext;
                    return;
                }

                for(i = 0; i < contextDatabase[cmd[1]].totalColumns; i++)
                {
                    contextDatabase[cmd[1]][i].addData(new Data(inputBuffer[contextDatabase[cmd[1]][i].ColName]));

                    if (Models.Type.isStringType(contextDatabase[cmd[1]][i].ColType.TypeName))
                    {
                        inputBuffer[contextDatabase[cmd[1]][i].ColName] = inputBuffer[contextDatabase[cmd[1]][i].ColName].Insert(0, "\'");
                        inputBuffer[contextDatabase[cmd[1]][i].ColName] += "\'";
                    }


                }
                SqlConnection conn = new SqlConnection(Connection.connectionString);
                SqlCommand command = conn.CreateCommand();
                try
                {
                    
                    command.CommandText = $"use [{contextDatabase.Name}] insert into [{cmd[1]}] values (";
                    for(i = 0; i < contextDatabase[cmd[1]].totalColumns; i++)
                    {
                        command.CommandText += $"{inputBuffer[contextDatabase[cmd[1]][i].ColName]},";
                        
                    }
                    
                    command.CommandText = command.CommandText.Remove(command.CommandText.Length - 1, 1);
                    command.CommandText += ")";
                    conn.Open();
                    command.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occured during the previous operation.");
                    Console.WriteLine(e.Message);
                    conn.Close();
                    return;
                }
                Console.WriteLine("Records inserted successfully.");
                displayAllDetails();
                contextTable = oldContext;
                conn.Close();
                return;

            }
            else
            {
                execError();
                
            }
        }
        public static void execUpdateData(string[] cmd)
        {
            if(cmd.Length == 1)
            {
                Console.WriteLine("Name of the table is needed.", response);
                return;
            }
            
            if(contextDatabase != null)
            {
                
                
                Table oldContext = contextTable;
                if (!contextDatabase.tableExists(cmd[1]))
                {
                    Console.WriteLine($"Table {cmd[1]} doesn't exist in {contextDatabase.Name} database.");
                    return;
                }
                contextTable = contextDatabase[cmd[1]];

                Dictionary<string, string> inputBuffer = new Dictionary<string, string>(contextTable.totalColumns);

                Console.Clear();
                Console.WriteLine();
                Console.WriteLine($"Records updating screen for {cmd[1]}");
                Console.WriteLine($"Current records for {cmd[1]}:");
                displayAllDetails();
                
                Console.WriteLine("If you wish for a column to remain unchanged, type nothing.");
                int i;
                for(i = 0; i < contextTable.totalColumns; i++)
                {
                    inputBuffer.Add(contextTable[i].ColName, "");

                    Console.Write($"{contextTable[i].ColName} ({contextTable[i].ColType.CompleteType}): ");
                    inputBuffer[contextTable[i].ColName] = Console.ReadLine();
                    if(inputBuffer[contextTable[i].ColName] != "")
                    {
                        if (Models.Type.isStringType(contextTable[i].ColType.TypeName))
                        {
                            inputBuffer[contextTable[i].ColName] = inputBuffer[contextTable[i].ColName].Insert(0, "\'");
                            inputBuffer[contextTable[i].ColName] += "\'";
                        }
                        else if (Models.Type.isNumeric(contextTable[i].ColType.TypeName))
                        {
                            float test;
                            while (!float.TryParse(inputBuffer[contextTable[i].ColName], out test))
                            {
                                Console.Write($"{contextTable[i].ColName} ({contextTable[i].ColType.CompleteType}) is numeric: ");
                                inputBuffer[contextTable[i].ColName] = Console.ReadLine();
                            }
                        }
                    }
                    Console.WriteLine(inputBuffer[contextTable[i].ColName]);
                    
                }
                List<string> columnsToRefresh = new List<string>();
                SqlConnection conn = new SqlConnection(Connection.connectionString);
                SqlCommand command = conn.CreateCommand();
                command.CommandText = $"use[{contextDatabase.Name}] update {contextTable.TName} set ";
                for(int t = 0; t < contextTable.totalColumns; t++)
                {
                    if(inputBuffer[contextTable[t].ColName] != "")
                    {
                        columnsToRefresh.Add(contextTable[t].ColName);
                        command.CommandText += $"{contextTable[t].ColName} = {inputBuffer[contextTable[t].ColName]},";
                    }
                }
                command.CommandText = command.CommandText.Remove(command.CommandText.Length - 1, 1);
                Console.WriteLine();
                displayAllDetails();
                //-----------------------

                Console.Write("Select a column and a value as a predicate for the update:\n");
                Console.WriteLine("Example [Column_Name] [=/</>/<=/>=/!=] [Value]");
               
                string predicateInput = Console.ReadLine();


                string[] predicateVerification = predicateInput.Split(' ');
                if (Models.Type.isNumeric(contextTable[predicateVerification[0]].ColType.TypeName))
                {
                    while (!float.TryParse(predicateVerification[2], out float test))
                    {
                        Console.WriteLine($"Incompatible types between {contextTable[predicateVerification[0]].ColName}({contextTable[predicateVerification[0]].ColType.CompleteType}) " +
                            $"and {predicateVerification[2]}");
                        
                        Console.WriteLine("Syntax: [Column_Name] [=/</>/<=/>=/!=] [Value]");
                        Console.Write("Re-enter your predicate: ");
                        predicateInput = Console.ReadLine();


                        predicateVerification = predicateInput.Split(' ');
                        if(predicateVerification.Length != 3)
                        {
                            Console.WriteLine("Update process has been cancelled.");
                            contextTable = oldContext;
                            return;
                        }
                    }
                }
                
                if(predicateVerification.Length != 3)
                {
                    Console.WriteLine("Invalid predicate assignment.");
                    contextTable = oldContext;
                    return;
                }
                if (!SyntaxParser.operatorExists(predicateVerification[1]))
                {
                    Console.WriteLine("Invalid predicate operator.");
                    contextTable = oldContext;
                    return;
                }


                if (Models.Type.isStringType(contextTable[predicateVerification[0]].ColType.TypeName))
                {
                    command.CommandText += $" where {predicateVerification[0]} {predicateVerification[1]} '{predicateVerification[2]}';";

                }
                else
                {
                    command.CommandText += $" where {predicateVerification[0]} {predicateVerification[1]} {predicateVerification[2]};";
                }

                try
                {
                    
                    conn.Open();
                    command.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Error occured at updating time.");
                    contextTable = oldContext;
                    conn.Close();
                    return;
                }
                conn.Close();
                Console.WriteLine("Successfully updated the records.");
                Console.WriteLine();
                refreshDbList(columnsToRefresh);
                displayAllDetails();
                contextTable = oldContext;
                return;
                

            }
            execError();

        }
        public static void refreshDbList(List<string> colsToRefresh)
        {
            foreach (string item in colsToRefresh)
            {
                contextTable[item].retrieveAllData.Clear();
                
            }
            SqlConnection conn = new SqlConnection(Connection.connectionString);
            SqlCommand cmd = conn.CreateCommand();
            SqlDataReader reader;
            conn.Open();
            int i;
            try
            {

                for (i = 0; i < colsToRefresh.Count; i++)
                {
                    cmd.CommandText = $"use [{contextDatabase.Name}] select {colsToRefresh[i]} from {contextTable.TName}";
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        contextTable[colsToRefresh[i]].addData(new Data(reader[0].ToString()));
                    }
                    reader.Close();

                }
                conn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return;
            }

        }
        public static void execError()
        {

            Console.WriteLine("Error occured during the previous operation, make sure you typed the correct command or made a valid operation.");
            Console.WriteLine("For additional help, type 'help'.");
        }
        public static void populateTotalDatabases()
        {
            if (totalDatabases.Count == 0)
            {
                SqlConnection conn = new SqlConnection();

                try
                {
                    // All databases created and populated
                    conn.ConnectionString = Connection.connectionString;
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = $"use [master] select name as 'Name' from sys.databases where owner_sid > 0x01 and is_distributor = 0";
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();


                    while (reader.Read())
                    {
                        totalDatabases.Add(new Database(reader["Name"].ToString()));


                    }
                    reader.Close();
                    //Populating tables
                    int dbIndex;
                    for (dbIndex = 0; dbIndex < totalDatabases.Count; dbIndex++)
                    {
                        cmd.CommandText = $@"use [{totalDatabases[dbIndex].Name}]
                                            select TABLE_NAME as 'Name' from INFORMATION_SCHEMA.TABLES";
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            totalDatabases[dbIndex].appendTable(new Table(reader["Name"].ToString()));

                        }
                        reader.Close();
                        //Populating columns
                        int tableIndex;
                        for (tableIndex = 0; tableIndex < totalDatabases[dbIndex].tableCount(); tableIndex++)
                        {

                            cmd.CommandText = $@"use [{totalDatabases[dbIndex].Name}] 
                                           select ORDINAL_POSITION as 'Index' ,COLUMN_NAME as 'Column Name', DATA_TYPE as 'Data Type',
                                            CHARACTER_MAXIMUM_LENGTH as 'Character Length', IS_NULLABLE as 'Nullable'
                                           from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{totalDatabases[dbIndex][tableIndex].TName}'";

                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                int index = int.Parse(reader["Index"].ToString());
                                int charLen = 0;
                                if (!int.TryParse(reader["Character Length"].ToString(), out charLen))
                                {
                                    charLen = 0;
                                }
                                bool nullable = false;
                                if (reader["Nullable"].ToString() == "YES")
                                {
                                    nullable = true;
                                }
                                totalDatabases[dbIndex][tableIndex].appendColumn(new Column(index, reader["Column Name"].ToString(), reader["Data Type"].ToString(), charLen, false, nullable, null, null));

                            }
                            
                            reader.Close();
                            cmd.CommandText = $@"use [{totalDatabases[dbIndex].Name}]
                                               select * from [{totalDatabases[dbIndex][tableIndex].TName}]";

                            reader = cmd.ExecuteReader();

                            int t;
                            while (reader.Read())
                            {
                                for (t = 0; t < totalDatabases[dbIndex][tableIndex].totalColumns; t++)
                                {

                                    totalDatabases[dbIndex][tableIndex][t].addData(new Data(reader[t].ToString()));

                                }

                            }
                            reader.Close();
                        }
                    }

                    
                    int dIndex = 0;
                    int tIndex = 0;
                    string[] referencerAndRefrenced;
                    SqlDataReader constraintsReader;
                    for(dIndex = 0; dIndex < totalDatabases.Count; dIndex++)
                    {
                        for(tIndex = 0; tIndex < totalDatabases[dIndex].tableCount(); tIndex++)
                        {
                            cmd.CommandText = $@"use [{totalDatabases[dIndex].Name}]
                                                select COLUMN_NAME as 'Column Name', CONSTRAINT_NAME as 'Key' from INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
                                                where TABLE_NAME = '{totalDatabases[dIndex][tIndex].TName}'";
                            constraintsReader = cmd.ExecuteReader();
                            while (constraintsReader.Read())
                            {
                                if (constraintsReader["Key"].ToString() == $"PK@{totalDatabases[dIndex][tIndex].TName}@{constraintsReader["Column Name"]}")
                                {
                                    totalDatabases[dIndex][tIndex][constraintsReader["Column Name"].ToString()].setPrimaryKey = true;

                                }
                                if (constraintsReader["Key"].ToString().Contains("FK"))
                                {
                                    //FK@[ReferencerTable]@[ReferencerColumn]@[ReferencedTable]@[ReferencedColumn]
                                    referencerAndRefrenced = constraintsReader["Key"].ToString().Split('@');
                                    
                                    
                                    //0 -> FK
                                    //1 -> referncer-Table
                                    //2 -> referencer-Column
                                    //3 -> referenced-Table
                                    //4 -> referenced-Column
                                    totalDatabases[dbIndex][referencerAndRefrenced[1]][referencerAndRefrenced[2]].setForeignKeyAccessTable = totalDatabases[dbIndex][referencerAndRefrenced[3]];
                                    totalDatabases[dbIndex][referencerAndRefrenced[1]][referencerAndRefrenced[2]].setForeignKeyAccessColumn = totalDatabases[dbIndex][referencerAndRefrenced[3]][referencerAndRefrenced[4]];
                                    

                                    
                                }
                            }
                            constraintsReader.Close();
                        }
                    }


                    
                  
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = response;
                    conn.Close();
                    contextDatabase = null;
                    contextTable = null;
                    totalDatabases.Clear();
                    Console.WriteLine("Error occured at connection time.");
                    Console.WriteLine($"{e.Message}");
                    
                    return;
                }
                conn.Close();
                Console.ForegroundColor = response;
                Console.WriteLine($"Successfully loaded all data from {location[0]}.");
                Models.Type.populateSqlTypes(Models.Type.sqlValidationTypes);

            }



        }
        public static void displayAllDetails()
        {
           
            try
            {
                Console.ForegroundColor = response;
                int i;
                
                int t = 0;
                for(i = 0; i < contextTable.totalColumns; i++)
                {
                    Console.Write($"{contextTable[i].ColName} : ",Console.ForegroundColor = ConsoleColor.Gray);
                    for(t = 0; t < contextTable[i].retrieveAllData.Count; t++)
                    {
                        Console.Write($"{contextTable[i].retrieveAllData[t].getData}||");
                    }
                    Console.WriteLine();
                }

               
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                execError();
                return;
                
            }

            
        }
        public static void taskCommand(string[] cmd)
        {
            if(cmd.Length == 3)
            {
                if(contextDatabase != null && contextTable != null)
                {
                    if (!contextTable.columnExists(cmd[2]))
                    {
                        Console.WriteLine($"Column {cmd[2]} doesn't exist in {contextTable.TName}");
                        return;
                    }
                    Data outputData = null;
                    try
                    {
                        outputData = Models.Task.operationsList[cmd[1]](contextTable[cmd[2]]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        execError();
                        return;
                        
                    }
                    if(outputData == null)
                    {
                        Console.WriteLine($"Invalid input data from column {cmd[2]}.");
                        return;
                    }
                    displayAllDetails();
                    Console.WriteLine($"The result of operation {cmd[1]} in column {cmd[2]} is: {outputData.getData}");
                    return;
                }
                Console.WriteLine("To perform the task you must be within the context of a database and a table.");
                return;
            }
            execError();
            return;

        }
    }
}
