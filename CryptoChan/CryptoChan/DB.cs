﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptChan
{
    class DB
    {
        const int MAX_ROWDATE = 100;

        private SQLiteConnection conn = null;

        public static int todayTotalOrder = 0;

        //public string Path => @"C:\CryptoChan\db.sqlite"; 
        public string dBPath => $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\CryptoChan\\db.sqlite";

        public DB() { }

        public bool CreateDataBase(string path)
        {            
            try
            {
                SQLiteConnection.CreateFile(path);                
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        } 

        public bool ConnectionDataBase()
        {
            bool isCreateDB = false;

            try
            {
                FileInfo fileInfo = new FileInfo(dBPath);

                if (!fileInfo.Exists)
                {
                    isCreateDB = true;
                }

                if (isCreateDB)
                {
                    if (!Directory.Exists(dBPath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dBPath));
                    }

                    if (CreateDataBase(dBPath))
                    {
                        //Success
                        ConnectionDataBase(dBPath);
                        CreateTable();
                    }
                    else
                    {
                        //Fail
                    }
                } 

                if (ConnectionDataBase(dBPath))
                {
                    //Success
                }
                else
                {
                    //Fail
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool ConnectionDataBase(string path)
        {
            try
            {
                if (conn is null)
                {
                    conn = new SQLiteConnection($"Data Source={path};Version=3;");
                    conn.Open();

                    todayTotalOrder = GetTotalOrder();
                }
            }
            catch (Exception e)
            {
                throw e;
            } 
            return true;
        } 

        public int CreateTable()
        {
            int result;

            try
            {
                StringBuilder sbQuery = new StringBuilder();

                sbQuery.Append("create table files (");
                sbQuery.Append(" [index] integer PRIMARY KEY AUTOINCREMENT, ");
                sbQuery.Append(" create_at varchar(8) Not NULL, ");
                sbQuery.Append(" file_name varchar(256) Not NULL, ");
                sbQuery.Append(" file_order integer Not NULL)");

                SQLiteCommand command = new SQLiteCommand(sbQuery.ToString(), conn);
                result = command.ExecuteNonQuery();                
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }

        public bool InsertData(string fileName)
        { 
            try
            {
                if (todayTotalOrder > MAX_ROWDATE)
                    return false;

                StringBuilder sbQuery = new StringBuilder(); 

                sbQuery.Append("insert into files (create_at, file_name, file_order) ");
                sbQuery.Append($"values ({DateTime.Now.ToString("yyyyMMdd")}, \"{fileName}\", {++todayTotalOrder}); "); 
                SQLiteCommand command = new SQLiteCommand(sbQuery.ToString(), conn);
                command.ExecuteNonQuery(); 
            }
            catch (Exception e)
            { 
                return false;
            }

            return true;
        }

        public List<string> GetCurrentFiles()
        {
            List<string> files = new List<string>();

            try
            {
                StringBuilder sbQuery = new StringBuilder();

                //select
                sbQuery.Append("select * ");
                //from
                sbQuery.Append("from files ");
                //where
                sbQuery.Append($"where create_at = \"{DateTime.Now.ToString("yyyyMMdd")}\" "); 
                //order by
                sbQuery.Append($"order by file_order desc ");
                //limit
                sbQuery.Append($"limit 6 ");

                SQLiteCommand command = new SQLiteCommand(sbQuery.ToString(), conn);
                SQLiteDataReader queryReader = command.ExecuteReader();

                while (queryReader.Read())
                {
                    files.Add(queryReader[2].ToString());
                }
            }
            catch (Exception e)
            {
                return null;
            }

            return files;
        } 

        public Dictionary<string, string> GetTotalFiles()
        {
            Dictionary<string, string> totalFiles = new Dictionary<string, string>();

            try
            {
                StringBuilder sbQuery = new StringBuilder();

                //select
                sbQuery.Append("select create_at, count(*) Count ");
                //from
                sbQuery.Append("from files ");
                //group by
                sbQuery.Append($"group by create_at ");
                //order by
                sbQuery.Append($"order by create_at desc ");
                //limit
                sbQuery.Append($"limit 5 ");

                SQLiteCommand command = new SQLiteCommand(sbQuery.ToString(), conn);
                SQLiteDataReader queryReader = command.ExecuteReader();

                while (queryReader.Read())
                {
                    string dateName = string.Empty;

                    //2020908 -> 9.8
                    dateName = queryReader[0].ToString(); 

                    dateName = dateName.Substring(4, 4).Insert(2, ".");

                    if (dateName[3] == '0')
                        dateName = dateName.Remove(3, 1);

                    if (dateName[0] == '0')
                        dateName = dateName.Remove(0, 1);

                    totalFiles.Add(dateName, queryReader[1].ToString());
                }

                totalFiles = totalFiles.Reverse().ToDictionary(dict => dict.Key, dict => dict.Value);
            }
            catch (Exception e)
            {
                return null;
            }

            return totalFiles;
        }

        public bool CloseConnection()
        {
            try
            {
                conn.Close();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        private int GetTotalOrder()
        {
            int result = 0;

            try
            {
                StringBuilder sbQuery = new StringBuilder();

                //select
                sbQuery.Append("select count(*) Count ");
                //from
                sbQuery.Append("from files ");
                //where
                sbQuery.Append($"where create_at = \"{DateTime.Now.ToString("yyyyMMdd")}\""); 

                SQLiteCommand command = new SQLiteCommand(sbQuery.ToString(), conn);  
                SQLiteDataReader queryReader = command.ExecuteReader();
                 
                while (queryReader.Read())
                {
                    result = int.Parse(queryReader[0].ToString());
                }
            }
            catch (Exception e)
            {
                return -1;
            }

            return result;
        }
    }
}
