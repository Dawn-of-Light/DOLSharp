using System;
using System.IO;
using System.Windows.Forms;

using DOL.GS;
using System.Data;

namespace DOLConfig
{
    static class DOLConfigParser
    {
        /// <summary>
        /// The general serverconfig filename
        /// </summary>
        private static string configFileName = "serverconfig.xml";

        /// <summary>
        /// Gets the path to the folder of the config file
        /// </summary>
        /// <returns></returns>
        private static string getConfigPath()
        {
            string appConfigPath = Application.StartupPath + Path.DirectorySeparatorChar + "config";
            if (!Directory.Exists(appConfigPath))
            {
                Directory.CreateDirectory(appConfigPath);
            }
            return appConfigPath;
        }

        /// <summary>
        /// Get the path of the config file
        /// </summary>
        /// <returns></returns>
        public static string getCurrentConfigFile()
        {
            string configFilePath = getConfigPath() + Path.DirectorySeparatorChar + configFileName;
            return configFilePath;
        }


        /// <summary>
        /// Gets the current configuration of the GameServer
        /// </summary>
        /// <returns></returns>
        public static GameServerConfiguration getCurrentConfiguration() 
        {
            try
            {
                FileInfo configFileInfo = new FileInfo(getCurrentConfigFile());
                GameServerConfiguration config = new GameServerConfiguration();
                config.LoadFromXMLFile(configFileInfo);

                return config;
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// Save the GameServer configuration
        /// </summary>
        /// <param name="gsc">The GameServer configuration which should be saved.</param>
        /// <returns></returns>
        public static void saveCurrentConfiguration(GameServerConfiguration gsc)
        {
            try
            {
                FileInfo configFileInfo = new FileInfo(getCurrentConfigFile());
                gsc.SaveToXMLFile(configFileInfo);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Loads all extra properties of the Server configuration
        /// </summary>
        /// <returns></returns>
        public static DataSet loadExtraOptions()
        {
            string base_file = Application.StartupPath + Path.DirectorySeparatorChar + "dolconfig_extra_properties.xml";
            string config_file = getCurrentConfigFile();

            if (!File.Exists(config_file)) throw new FileNotFoundException();
            if (!File.Exists(base_file)) throw new FileNotFoundException();
            
            //these settings are set by the application
            string[] ignoreColumns = new string[] { "UdpIP", "Port", "DBConnectionString", "GameType", "RegionIP", "ServerNameShort", "RegionPort", "DBType", "IP", "DBAutosaveInterval", "DBAutosave", "AutoAccountCreation", "ServerName", "DetectRegionIP", "UdpPort" };

            //Get the dataset
            DataSet ds = new DataSet("ExtraProperties");
            ds.ReadXml(base_file);
            ds.Tables["Server"].PrimaryKey = new DataColumn[] { ds.Tables["Server"].Columns["property"] };

            //the data from the current serverconfig file
            DataSet ds_current = new DataSet();
            ds_current.ReadXml(config_file);

            foreach (DataColumn column in ds_current.Tables["Server"].Columns)
            {
                //skip ignored columns
                bool ignore = false;
                foreach (string col in ignoreColumns)
                {
                    if (col == column.ColumnName)
                    {
                        ignore = true;
                    }
                }
                if (ignore) continue;

                //Is this property allready present?
                if (ds.Tables["Server"].Rows.Contains(column.ColumnName))
                {
                    //Set the value
                    (ds.Tables["Server"].Rows.Find(column.ColumnName))["value"] = ds_current.Tables["Server"].Rows[0][column.ColumnName];
                }
                else
                {
                    ds.Tables["Server"].Rows.Add(column.ColumnName, "string", ds_current.Tables["Server"].Rows[0][column.ColumnName], "");
                }
            }

            return ds;
        }

        /// <summary>
        /// Adds a new Row to the properties DataSet
        /// </summary>
        /// <param name="ds">The DataSet which holds the data</param>
        /// <param name="name">The vame of the property</param>
        /// <param name="type">The type of the property</param>
        /// <param name="value">The value of the property</param>
        /// <param name="description">The description of the property</param>
        public static void addExtraOptionsRow(DataSet ds, string name, string type, object value, string description)
        {
            //Check if this property allready exists
            if (!ds.Tables["Server"].Rows.Contains(name))
            {
                ds.Tables["Server"].Rows.Add(name, type, value, description);
            }
            else
            {
                (ds.Tables["Server"].Rows.Find(name))["value"] = value;
            }
        }

        /// <summary>
        /// Removes a row from the properties DataSet
        /// </summary>
        /// <param name="ds">The DataSet which holds the data</param>
        /// <param name="name">the name of the property</param>
        public static void removeExtraOptionsRow(DataSet ds, object name)
        {
            if (!ds.Tables["Server"].Rows.Contains(name)) return;

            //Remove the row
            ds.Tables["Server"].Rows.Remove(ds.Tables["Server"].Rows.Find(name));
        }

        /// <summary>
        /// Saves the properties and writes the serverconfig file
        /// </summary>
        /// <param name="ds">The DataSet which holds the data</param>
        public static void saveExtraOptions(DataSet ds)
        {
            string config_file = getCurrentConfigFile();

            if (!File.Exists(config_file)) throw new FileNotFoundException();

            //the data from the current serverconfig file
            DataSet ds_current = new DataSet();
            ds_current.ReadXml(config_file);

            foreach (DataRow row in ds.Tables["Server"].Rows)
            {
                string property_name = (string)row["property"];
                string property_value = (string)row["value"];

                //Check if this values exists in current config
                if (ds_current.Tables["Server"].Columns.Contains(property_name))
                {
                    ds_current.Tables["Server"].Rows[0][property_name] = property_value;
                }
                else
                {
                    ds_current.Tables["Server"].Columns.Add(property_name);
                    ds_current.Tables["Server"].Rows[0][property_name] = property_value;
                }
            }

            //Write the file
            ds_current.WriteXml(config_file);
        }
    }
}
