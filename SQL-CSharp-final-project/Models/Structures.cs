using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using SQL_CSharp_final_project.Shell;
namespace SQL_CSharp_final_project.Models
{


    public static class Connection
    {
        public static string connectionString;
        private static string connectionFile = "connection_settings.ini";

        public static void setupConnection()
        {
            
            File.WriteAllText(connectionFile, connectionString);

        }

        public static bool readSettingsFromFile()
        {

            if (!File.Exists(connectionFile)) return false;

            connectionString = File.ReadAllText(connectionFile);
            
            int i;
            for(i = 0; i < connectionString.Length; i++)
            {
                for (; i < connectionString.Length && connectionString[i] != '='; i++) ;
                break;
            }
            i++;
            for(; i < connectionString.Length && connectionString[i] != ';'; i++)
            {
                Shell.Shell.location[0] += connectionString[i];
            }

            return true;

        }


    }


    





    
    //TODO: Implement remove table with functionalities to send it to SQL while removing it from the DB's list.
    //TODO: Implement alter table with functionalities to send it to SQL while altering it in the DB's list.
    public class Database
    {

        private string _Name { get; set; }

        private List<Table> Tables = new List<Table>();



        public Database()
        {

        }
        public Database(string name)
        {
            _Name = name;


        }

        public Table this[string tName]
        {
            get
            {
                int i;
                for (i = 0; i < Tables.Count; i++)
                {
                    if (Tables[i].TName == tName) return Tables[i];
                }
                return null;
            }
            set
            {
                int i;
                for (i = 0; i < Tables.Count; i++)
                {
                    if (Tables[i].TName == value.TName) Tables[i] = value;
                }
            }
        }
        public Table this[int index]
        {
            get
            {
                if (index < Tables.Count)
                {
                    return Tables[index];
                }
                return null;
            }
        }
        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public int tableCount()
        {
            return Tables.Count;
        }


        //Foreign Key naming convention: FK_[Referencer_Table/FK_Column]_[Referenced_Table/PK_Column]
        //CONSTRAINT FK_[RefrencerTable]_[Referenced] FOREIGN KEY([References_Column])
            //REFERENCES[Referenced_Table]([Referenced_Column])
            //CONSTRAINT PK_Person PRIMARY KEY (ID,LastName)
        public bool addTable(Table t)
        {

            if (tableExists(t.TName)) return false;

            
            int i = 0;
            string columnsBuffer = "";
            for (i = 0; i < t.totalColumns; i++)
            { 
                columnsBuffer += $"{t[i].ColName} {t[i].ColType.CompleteType} ";
                if(t[i].isPrimaryKey)
                {
                    columnsBuffer += $"constraint PK_{t.TName}_{t[i].ColName} Primary Key({t[i].ColName})";
                }


                if (!t[i].isNullAble)
                {
                    columnsBuffer += " NOT NULL";
                }
                else
                {
                    columnsBuffer += " NULL";
                }
                columnsBuffer += ",";

            }

            columnsBuffer = columnsBuffer.Remove(columnsBuffer.Length - 1);
            
            try
            {
                SqlConnection conn = new SqlConnection(Connection.connectionString);
                SqlCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandText = $"use [{Name}] create table [{t.TName}] ({columnsBuffer});";
                cmd.ExecuteNonQuery();
                conn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                throw;
            }
            
            
            Tables.Add(t);
            return true;


        }

        public static Database databaseFind(List<Database> dbList, string dbName)
        {
            int i;
            for (i = 0; i < dbList.Count; i++)
            {
                if (dbList[i].Name == dbName) return dbList[i];
            }
            return null;
        }

        public static bool databaseExists(List<Database> dbList, string dbName)
        {
            int i;
            for (i = 0; i < dbList.Count; i++)
            {
                if (dbList[i].Name == dbName) return true;
            }
            return false;

        }
        public static bool createDatabase(Database db)
        {
            SqlConnection conn = new SqlConnection(Connection.connectionString);
            try
            {
                SqlCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandText = $"use [master] create database {db._Name}";
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                Console.WriteLine($"Cannot create a new database: {e.Message}");
                conn.Close();

                return false;
            }
            conn.Close();
            Shell.Shell.totalDatabases.Add(db);
            return true;

        }
        public bool tableExists(string tableName)
        {
            foreach (Table item in Tables)
            {
                if (item.TName == tableName) return true;
            }
            return false;
        }
        public void appendTable(Table t)
        {
            Tables.Add(t);
        }

    }


    //TODO: Implemnt append column with functionalities to send it to SQL while appending it to the Table's list.
    //TODO: Implemnt remove column with functionalities to send it to SQL while removing it from the Table's list.
    //TODO: Implemnt alter column with functionalities to send it to SQL while altering it in the Table's list.
    public class Table
    {

        private string _TName { get; set; }

        private List<Column> Columns;

        public Table(string name)
        {
            Columns = new List<Column>();
            _TName = name;
        }

        public Table(string name, int columnAmount)
        {
            Columns = new List<Column>(columnAmount);
            _TName = name;
        }
        public string TName
        {
            get
            {
                return _TName;
            }
        }

        public Column this[int index]
        {
            get
            {
                if (index > Columns.Count) return default;

                return Columns[index];
            }
        }
        public Column this[string columnName]
        {
            get
            {
                int i;
                for (i = 0; i < Columns.Count; i++)
                {
                    if (Columns[i].ColName == columnName)
                    {
                        return Columns[i];
                    }
                }
                return default;
            }
        }
        public int totalColumns
        {
            get
            {
                return Columns.Count;
            }
        }
        public bool addColumn(Column col)
        {
            foreach (Column item in Columns)
            {
                if (columnExists(item.ColName)) return false;
            }
            Columns.Add(col);
            SqlConnection conn = new SqlConnection(Connection.connectionString);
            SqlCommand cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = $@"alter table [{TName}]
                                add column {col.ColName} {col.ColType.CompleteType}";
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;

        }
        public void appendColumn(Column col)
        {
            Columns.Add(col);

        }
        public void removeColumn(Column col)
        {
            Columns.Remove(col);
        }
        public bool deleteColumn(Column col)
        {
            foreach (Column item in Columns)
            {
                if (!columnExists(item.ColName) || (item.foreignKeyAccessTable != null && item.foreignKeyAccessColumn != null)) return false;
            }
            Columns.Remove(col);
            SqlConnection conn = new SqlConnection(Connection.connectionString);
            SqlCommand cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = $@"alter table [{TName}]
                                drop column {col.ColName}";
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;       
        }
        public void setColumn(int index, Column col)
        {
            if (index < Columns.Count)
            {
                Columns[index] = col;
            }
        }

        public void setColumn(string colName, Column newCol)
        {
            int i;
            for (i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].ColName == colName)
                {
                    Columns[i] = newCol;
                }
            }

        }
        public bool columnExists(string colName)
        {

            int i;
            for (i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].ColName == colName) return true;
            }


            return false;
        }
        public Column findPrimaryKey()
        {
            int i;
            for(i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].isPrimaryKey) return Columns[i];
            }
            return null;
        }
    }


    public class Type
    {

        private string _typeName { get; set; }
        private int _characterLength { get; set; }

        public static List<string> sqlTypes = new List<string>();

        public Type(string typeName, int charLen)
        {
            _typeName = typeName;
            _characterLength = charLen;
        }
        public string TypeName
        {
            get
            {

                return _typeName;
            }
        }
        public int characterLen
        {
            get
            {
                return _characterLength;
            }
        }

        public string CompleteType
        {
            get
            {
                if (_typeName == "nvarchar" || _typeName == "varchar" || _typeName == "varbinary" || _typeName == "binary" || _typeName == "nchar")
                {
                    return $"{_typeName}({_characterLength})";

                }
                return _typeName;


            }
        }

        public static void populateSqlTypes(List<string> types)
        {
            SqlConnection conn = new SqlConnection(Connection.connectionString);
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                cmd.CommandText = $"use [master] select * from sys.types";
                conn.Open();
                SqlDataReader read = cmd.ExecuteReader();
                while (read.Read())
                {
                    sqlTypes.Add(read["name"].ToString());
                }
                read.Close();


            }
            catch (Exception e)
            {
                Console.ForegroundColor = Shell.Shell.response;
                conn.Close();
                Shell.Shell.contextDatabase = null;
                Shell.Shell.contextTable = null;
                Shell.Shell.totalDatabases.Clear();
                Console.WriteLine("Error occured at connection time.");
                Console.WriteLine($"{e.Message}"); 
               

                
                
            }
            
           
            conn.Close();
        }
        public static bool isStringType(string type)
        {
            string[] stringTypes = { "nvarchar", "varchar", "nchar" };
            int i;
            for(i = 0; i < stringTypes.Length; i++)
            {
                if (type == stringTypes[i]) return true;
            }
            return false;
            
        }




        public static bool isNumeric(string type)
        {
            string[] numericTypes = { "tinyint", "smallint", "int","real", "money", "float", "bit", "decimal", "numberic" };

            int i;
            for (i = 0; i < numericTypes.Length; i++)
            {
                if (type == numericTypes[i]) return true;
            }
            return false;
        }

        public static bool isValidType(string type)
        {
            return sqlTypes.Contains(type);
        }

    }


    public class Column
    {
        private int _Index { get; set; }
        private string _Name { get; set; }
        private Type _Type { get; set; }
        private bool _isPrimaryKey { get; set; }
        private Table _ForeignKeyAccessorTable { get; set; }
        private Column _ForeignKeyAccessorColumn { get; set; }
        private bool _Nullable { get; set; }


        private List<Data> _Contents = new List<Data>();



        public Column(int index, string name, string type, int characterLen, bool primaryKey, bool nullable, Table foreignKeyAccessTable, Column foreignKeyAccessColumn)
        {
            _Index = index - 1;
            _Name = name;
            _Type = new Type(type, characterLen);
            _isPrimaryKey = primaryKey;
            _Nullable = nullable;
            _ForeignKeyAccessorColumn = foreignKeyAccessColumn;
            _ForeignKeyAccessorTable = foreignKeyAccessTable;

        }

        public string ColName
        {
            get
            {
                return _Name;
            }
        }

        public int ColIndex
        {
            get
            {
                return _Index;
            }
        }
        public Type ColType
        {
            get
            {
                return _Type;
            }
        }
        public bool isNullAble
        {
            get
            {
                return _Nullable;
            }
        }
        public bool setNullAble
        {
            set
            {
                _Nullable = value;
            }
        }
        public bool isPrimaryKey
        {
            get
            {
                return _isPrimaryKey;
            }
        }
        public bool setPrimaryKey
        {
            set
            {
                _isPrimaryKey = value;
            }
        }

        public Table foreignKeyAccessTable
        {
            get
            {
                return _ForeignKeyAccessorTable;
            }
        }
        public Table setForeignKeyAccessTable
        {
            set
            {
                _ForeignKeyAccessorTable = value;
            }
        }
        public Column foreignKeyAccessColumn
        {
            get
            {
                return _ForeignKeyAccessorColumn;
            }
        }
        public Column setForeignKeyAccessColumn
        {
            set
            {
                _ForeignKeyAccessorColumn = value;
            }
        }
        public List<Data> retrieveAllData
        {
            get
            {
                return _Contents;
            }
        }

        public static void setData(int dataIndex, string newData, ref List<Data> allData)
        {
            allData[dataIndex].setData = newData;
        }

        public List<Data> retrieveDataByValue(Data value)
        {
            List<Data> allValues = new List<Data>();
            foreach (Data item in _Contents)
            {
                if (item == value)
                {
                    allValues.Add(item);
                }
            }
            if (allValues.Count == 0) return null;
            return allValues;
        }

        public void addData(Data data)
        {
            _Contents.Add(data);
        }

        public static int parseColumnDataInt(Column column, Data data)
        {
            int output;
            if (!int.TryParse(data.getData, out output))
            {
                return -1;
            }
            return output;
        }
        public static float parseColumnDataFloat(Column column, Data data)
        {
            float output;
            if (!float.TryParse(data.getData, out output))
            {
                return -1;
            }
            return output;
        }
        public static double parseColumnDateDouble(Column column, Data data)
        {
            double output;
            if (!double.TryParse(data.getData, out output))
            {
                return -1;
            }
            return output;
        }





    }

    public class Data
    {
        private string _Data { get; set; }
        public Data(string data)
        {
            _Data = data;
        }
        public string getData
        {
            get
            {
                return _Data;
            }
        }
        public string setData
        {
            set
            {
                if (_Data.Equals(value))
                {
                    _Data = value;
                }

            }
        }
        

    }

    //TODO: Implement simple task-cmds(sum, average, max, min, count)
    public static class Task
    {

    }
}
