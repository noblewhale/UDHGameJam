namespace Noble.DungeonCrawler
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Mono.Data.Sqlite;
    using System.Data;
    using System.IO;
    using System;

    public class RogueDB : MonoBehaviour
    {

        public static RogueDB instance;

        public TextAsset GetPowerByRuneSQL;
        public TextAsset GetPowerByRunesSQL;
        public TextAsset GetShapeByRuneSQL;
        public TextAsset GetShapeByRunesSQL;
        public TextAsset GetRangeByShapeAndLevelSQL;
        public TextAsset GetDamageByRuneAndLevelSQL;

        IDbConnection db;

        public void Awake()
        {
            instance = this;

            var dbDump = Resources.Load<TextAsset>("db");
            string uri = "URI=file:" + Application.persistentDataPath + "/rogue_elements.db";


            if (!File.Exists(Application.persistentDataPath + "/rogue_elements.db"))
            {
                // First time import from static text in game resources. This writes the db to the game data folder somewhere on the os.
                // Really only doing this because there's no way to access the db directly from the resources
                // It does open up the potential for using the database for persistent storage, but for now I'm sticking to read only.
                IDbConnection tempDB = new SqliteConnection(uri);
                tempDB.Open();
                IDbCommand command = tempDB.CreateCommand();
                command.CommandText = dbDump.text;
                command.ExecuteNonQuery();
                tempDB.Close();
            }

            // Open in read only mode after first import
            db = new SqliteConnection(uri + ";Read Only=True");
            db.Open();
        }

        public void OnDestroy()
        {
            db.Close();
        }

        internal string GetPowerFromParts(CrystalBehaviour.CrystalType[] powerParts)
        {
            IDbCommand command = db.CreateCommand();
            command.CommandType = CommandType.Text;
            if (powerParts.Length == 1)
            {
                command.CommandText = GetPowerByRuneSQL.text;
                command.Parameters.Add(new SqliteParameter("@rune_id", powerParts[0]));
            }
            else
            {
                command.CommandText = GetPowerByRunesSQL.text;
                command.Parameters.Add(new SqliteParameter("@rune1_id", powerParts[0]));
                command.Parameters.Add(new SqliteParameter("@rune2_id", powerParts[1]));
            }

            IDataReader dataReader = command.ExecuteReader();
            dataReader.Read();
            return dataReader.GetString(0);
        }

        internal (float min, float max) GetMinMaxDamage(int runeID, int level)
        {
            IDbCommand command = db.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = GetDamageByRuneAndLevelSQL.text;
            command.Parameters.Add(new SqliteParameter("@rune_id", runeID));
            command.Parameters.Add(new SqliteParameter("@level", level));

            IDataReader dataReader = command.ExecuteReader();
            dataReader.Read();
            return (dataReader.GetFloat(0), dataReader.GetFloat(1));
        }

        internal float GetRange(string shape, int level)
        {
            IDbCommand command = db.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = GetRangeByShapeAndLevelSQL.text;
            command.Parameters.Add(new SqliteParameter("@shape", shape));
            command.Parameters.Add(new SqliteParameter("@level", level));

            IDataReader dataReader = command.ExecuteReader();
            dataReader.Read();
            return dataReader.GetFloat(0);
        }

        internal string GetShapeFromParts(CrystalBehaviour.CrystalType[] shapeParts)
        {
            IDbCommand command = db.CreateCommand();
            command.CommandType = CommandType.Text;
            if (shapeParts.Length == 1)
            {
                command.CommandText = GetShapeByRuneSQL.text;
                command.Parameters.Add(new SqliteParameter("@rune_id", shapeParts[0]));
            }
            else
            {
                command.CommandText = GetShapeByRunesSQL.text;
                command.Parameters.Add(new SqliteParameter("@rune1_id", shapeParts[0]));
                command.Parameters.Add(new SqliteParameter("@rune2_id", shapeParts[1]));
            }

            IDataReader dataReader = command.ExecuteReader();
            dataReader.Read();
            return dataReader.GetString(0);
        }
    }
}