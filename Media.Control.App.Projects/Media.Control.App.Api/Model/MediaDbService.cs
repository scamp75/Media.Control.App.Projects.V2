using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

using Microsoft.AspNetCore.Http.HttpResults;


namespace Media.Control.App.Api.Model
{

    public class MediaDbService
    {
        private string basePath = @$"{AppDomain.CurrentDomain.BaseDirectory}\Database";
        private static string LoggerPath = string.Empty;
        private static string MediaPath = string.Empty;   


        public  MediaDbService(string type) 
        {
            InitializeDatabase(type);
        }

        public void InitializeDatabase(string type)
        {
            if (!System.IO.Directory.Exists(basePath))
            {
                System.IO.Directory.CreateDirectory(basePath);
            }

            bool isCreate = false;
            if (type == "media")
            {
                MediaPath = @$"Data Source={basePath}\MediaInfo.db;Version=3;";


                if (!System.IO.File.Exists($@"{basePath}\MediaInfo.db"))
                {
                    SQLiteConnection.CreateFile($@"{basePath}\MediaInfo.db");

                    using (var Connection = new SQLiteConnection(MediaPath))
                    {
                        Connection.Open();
                        string createTableQuery = CreatTable.CreateMediaDataInfoTable();
                        using (var command = new SQLiteCommand(createTableQuery, Connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    };
                }
            }
            else
            {
                LoggerPath = @$"Data Source={basePath}\LoggerInfo.db;Version=3;";

                if (!System.IO.File.Exists($@"{basePath}\LoggerInfo.db"))
                {
                    SQLiteConnection.CreateFile($@"{basePath}\LoggerInfo.db");

                    using (var Connection = new SQLiteConnection(LoggerPath))
                    {
                        Connection.Open();
                    
                        string createTableQuery = CreatTable.CreateLogDataTable();
                        using (var command = new SQLiteCommand(createTableQuery, Connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    
                    }
                }
            }
        }



        public  List<LogData> GetLogDate(string channel , string dateTime)
        {
            List<LogData> Logs = new List<LogData>();
    
            try
            {
                string selectQuery = $"SELECT Num, Type,  Title, Message, Channel, CreateDate, Time " +
                                        $" FROM LogDataTB " +
                                        $" WHERE DATE(CreateDate) = '{dateTime}'";

                if (channel != "null")
                {
                    selectQuery += $"And Channel = '{channel}' ";
                }

                selectQuery += " order by Num desc";

                using (var Connection = new SQLiteConnection(LoggerPath))
                {
                    Connection.Open();
                    using (var cmd = new SQLiteCommand(selectQuery, Connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Logs.Add(new LogData
                                {
                                    Type = reader.GetString(1),
                                    Title = reader.GetString(2),
                                    Message = reader.GetString(3),
                                    Channel = reader.GetString(4),
                                    CreateDate = reader.GetDateTime(5),
                                    Time = reader.GetString(6)
                                });
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // 오류 처리
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            return Logs;
        }

        public bool InsertLogDate(LogData log)
        {
            bool result = false;

            try
            {
                string insertQuery = "INSERT INTO LogDataTB (Type,  Title, Message, Channel, CreateDate, Time)" +
                                    " VALUES (@Type, @Title, @Message, @Channel, @CreateDate, @Time)";

                using (var Connection = new SQLiteConnection(LoggerPath))
                {
                    Connection.Open();
                    using (var command = new SQLiteCommand(insertQuery, Connection))
                    {
                        command.Parameters.AddWithValue("@Type", log.Type);
                        command.Parameters.AddWithValue("@Title", log.Title);
                        command.Parameters.AddWithValue("@Message", log.Message);
                        command.Parameters.AddWithValue("@Channel", log.Channel);
                        command.Parameters.AddWithValue("@CreateDate", log.CreateDate);
                        command.Parameters.AddWithValue("@Time", log.Time);

                        result = command.ExecuteNonQuery() >= 0 ? true: false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return result;
        }

        public  bool DeleteLogDate(DateTime createDate)
        {
            bool result = false;
            
            try
            {     
                string DeleteQuery = $"DELETE FROM LogDataTB WHERE  DATE(CreateDate) < '{createDate}'";

                using (var Connection = new SQLiteConnection(LoggerPath))
                {
                    Connection.Open();
                    using (var command = new SQLiteCommand(DeleteQuery, Connection))
                    {
                        result = command.ExecuteNonQuery() >= 0 ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            

            return result;
        }
        public List<MediaDataInfo> GetMediaDate()
        {
            var mediaList = new List<MediaDataInfo>();

            try
            {
                string AddDate = string.Empty;
                  
                string selectQuery = $"SELECT Num, MediaId , Image , Name, Duration, Frame, " +
                                                $"CreateDate ,InPoint, InTimeCode, OutPoint,OutTimeCode, " +
                                                $"Type, Creator, Proxy, Path, Fps, Des, State " +
                                        $" FROM mediainfotb";
                        selectQuery += " order by Num desc";

                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var cmd = new SQLiteCommand(selectQuery, Connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mediaList.Add(new MediaDataInfo
                                {
                                    MediaId = reader.GetString(1),
                                    Image = reader.GetString(2),
                                    Name = reader.GetString(3),
                                    Duration = reader.GetString(4),
                                    Frame = reader.GetInt32(5),
                                    CreatDate = reader.GetDateTime(6),
                                    InPoint = reader.GetInt32(7),
                                    InTimeCode = reader.GetString(8),
                                    OutPoint = reader.GetInt32(9),
                                    OutTimeCode = reader.GetString(10),
                                    Type = reader.GetString(11),
                                    Creator = reader.GetString(12),
                                    Proxy = reader.GetString(13),
                                    Path = reader.GetString(14),
                                    Fps = reader.GetInt32(15),
                                    Des = reader.GetString(16),
                                    State = reader.GetString(17)

                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
            

            return mediaList;
        }

        public List<MediaDataInfo> GetMediaDate(string datetiem, bool isCreate, string creator, string name)
        {
            var mediaList = new List<MediaDataInfo>();


            try
            {
                string AddDate = string.Empty;
                if (isCreate)
                {
                    DateTime date = DateTime.Parse(datetiem); // 문자열을 DateTime으로 변환
                    DateTime newDate = date.AddDays(-7); // 7일 더하기
                    AddDate = newDate.ToString("yyyy-MM-dd"); // 다시 문자열로 변환
                }


                string selectQuery = $"SELECT Num, MediaId , Image , Name, Duration, Frame, " +
                                                $"CreateDate ,InPoint, InTimeCode, OutPoint,OutTimeCode, " +
                                                $"Type, Creator, Proxy, Path, Fps, Des, State " +
                                        $" FROM WHERE State = 'Done' ";
                                     
                if (!isCreate)
                {
                    selectQuery += $"  And DATE(CreateDate) = '{datetiem}' ";
                }
                else
                {
                    selectQuery += $"  And DATE(CreateDate) BETWEEN '{AddDate}' AND '{datetiem}'";
                }

                if (creator != "null" && name != "null")
                {
                    selectQuery += $"AND creator = '{creator}' AND Name Like '%{name}%' ";
                }
                else if (creator != "null")
                {
                    selectQuery += $"AND creator = '{creator}' ";
                }
                else if (name != "null")
                {
                    selectQuery += $" AND Name like '%{name}%' ";
                }

                selectQuery += " order by Num desc";

                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var cmd = new SQLiteCommand(selectQuery, Connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mediaList.Add(new MediaDataInfo
                                {
                                    MediaId = reader.GetString(1),
                                    Image = reader.GetString(2),
                                    Name = reader.GetString(3),
                                    Duration = reader.GetString(4),
                                    Frame = reader.GetInt32(5),
                                    CreatDate = reader.GetDateTime(6),
                                    InPoint = reader.GetInt32(7),
                                    InTimeCode = reader.GetString(8),
                                    OutPoint = reader.GetInt32(9),
                                    OutTimeCode = reader.GetString(10),
                                    Type = reader.GetString(11),
                                    Creator = reader.GetString(12),
                                    Proxy = reader.GetString(13),
                                    Path = reader.GetString(14),
                                    Fps = reader.GetInt32(15),
                                    Des = reader.GetString(16),
                                    State = reader.GetString(17)

                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
            

            return mediaList;
        }

        public bool InsertMeidaInfo(MediaDataInfo mediaData)
        {
            bool result = false;

            try
            {
                string insertQuery = "INSERT INTO mediainfotb " +
                            "(MediaId,  Image,  Name, Duration,  Frame,  CreateDate,  InPoint,  InTimeCode,  " +
                            "OutPoint,  OutTimeCode,  Type,  Creator,  Proxy,  Path, Fps,  Des, State)" +
                     " VALUES (@MediaId, @Image, @Name, @Duration, @Frame, @CreateDate, @InPoint, @InTimeCode, " +
                            "@OutPoint, @OutTimeCode, @Type, @Creator, @Proxy, @Path, @Fps, @Des, @State)";

                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var command = new SQLiteCommand(insertQuery, Connection))
                    {
                        command.Parameters.AddWithValue("@MediaId", mediaData.MediaId);
                        command.Parameters.AddWithValue("@Image", mediaData.Image);
                        command.Parameters.AddWithValue("@Name", mediaData.Name);
                        command.Parameters.AddWithValue("@Duration", mediaData.Duration);
                        command.Parameters.AddWithValue("@Frame", mediaData.Frame);
                        command.Parameters.AddWithValue("@CreateDate", mediaData.CreatDate);
                        command.Parameters.AddWithValue("@InPoint", mediaData.InPoint);
                        command.Parameters.AddWithValue("@InTimeCode", mediaData.InTimeCode);
                        command.Parameters.AddWithValue("@OutPoint", mediaData.OutPoint);
                        command.Parameters.AddWithValue("@OutTimeCode", mediaData.OutTimeCode);
                        command.Parameters.AddWithValue("@Creator", mediaData.Creator);
                        command.Parameters.AddWithValue("@Type", mediaData.Type);
                        command.Parameters.AddWithValue("@Proxy", mediaData.Proxy);
                        command.Parameters.AddWithValue("@Path", mediaData.Path);
                        command.Parameters.AddWithValue("@Fps", mediaData.Fps);
                        command.Parameters.AddWithValue("@Des", mediaData.Des);
                        command.Parameters.AddWithValue("@State", mediaData.State);

                        result = command.ExecuteNonQuery() >= 0 ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return result;

        }

        public MediaDataInfo GetMediaDataInfo(string id)
        {
            var media = new MediaDataInfo();

            try
            {
                string selectQuery = $"SELECT Num, MediaId , Image , Name, Duration, Frame, " +
                                            $"CreateDate ,InPoint, InTimeCode, OutPoint, OutTimeCode, " +
                                            $"Type, Creator, Proxy, Path, Fps,  Des, State" +
                                        $" FROM mediainfotb WHERE Mediaid = '{id}' ";
                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var cmd = new SQLiteCommand(selectQuery, Connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                media = new MediaDataInfo
                                {
                                    MediaId = reader.GetString(1),
                                    Image = reader.GetString(2),
                                    Name = reader.GetString(3),
                                    Duration = reader.GetString(4),
                                    Frame = reader.GetInt32(5),
                                    CreatDate = reader.GetDateTime(6),
                                    InPoint = reader.GetInt32(7),
                                    InTimeCode = reader.GetString(8),
                                    OutPoint = reader.GetInt32(9),
                                    OutTimeCode = reader.GetString(10),
                                    Type = reader.GetString(11),
                                    Creator = reader.GetString(12),
                                    Proxy = reader.GetString(13),
                                    Path = reader.GetString(14),
                                    Fps = reader.GetInt32(15),
                                    Des = reader.GetString(16),
                                    State = reader.GetString(17)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
            

            return media;
        }

        public bool UpDateInPoint(string id, int inPoint,  string inTimecode, int frame, string Duration)
        {
            bool result = false;
            
            try
            {
                string UpdateQuery = $"UPDATE " +
                                        $" mediainfotb " +
                                        $"   SET InPoint = {inPoint}, " +
                                        $"       Frame = {frame}, " +
                                        $"       InTimeCode = '{inTimecode}', " +
                                        $"       Duration = '{Duration}' " +
                                        $" WHERE MediaId = '{id}'";
                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var command = new SQLiteCommand(UpdateQuery, Connection))
                    {
                        result = command.ExecuteNonQuery() >= 0 ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return result;
        }

        public bool UpDateOutPoint(string id, int outPoint, string outTimecode, int frame, string Duration)
        {
            bool result = false;

            try
            {
                string UpdateQuery = $"UPDATE " +
                                        $"mediainfotb " +
                                        $"    SET OutPoint = {outPoint}, " +
                                        $"        Frame = {frame}, " +
                                        $"        OutTimeCode = '{outTimecode}', " +
                                        $"        Duration = '{Duration}' " +
                                        $"  WHERE MediaId = '{id}'";


                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var command = new SQLiteCommand(UpdateQuery, Connection))
                    {
                        result = command.ExecuteNonQuery() >= 0 ? true : false;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return result;
        }

        public bool UpDateDuration(string id, int frame, string Duration)
        {
            bool result = false;
            
            try
            {
                string UpdateQuery = $"UPDATE " +
                                    $" mediainfotb " +
                                    $"    SET Frame = {frame}, " +
                                    $"        Duration = '{Duration}' " +
                                    $"  WHERE MediaId = '{id}'";

                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var command = new SQLiteCommand(UpdateQuery, Connection))
                    {
                        result = command.ExecuteNonQuery() >= 0 ? true : false;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            
            return result;
        }

        public bool UpDateMediaState(string id , string state)
        {
            bool result = false;
            
            try
            {
                string UpdateQuery = $"UPDATE mediainfotb SET State = '{state}' WHERE MediaId = '{id}'";

                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var command = new SQLiteCommand(UpdateQuery, Connection))
                    {
                        result = command.ExecuteNonQuery() >= 0 ? true : false;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            
            return result;
        }

        public bool DeleteMeidaDate(string id)
        {
            bool result = false;
     
            try
            {
                string DeleteQuery = $"DELETE FROM  mediainfotb WHERE MediaId = '{id}'";

                using (var Connection = new SQLiteConnection(MediaPath))
                {
                    Connection.Open();
                    using (var command = new SQLiteCommand(DeleteQuery, Connection))
                    {
                        result = command.ExecuteNonQuery() >= 0 ? true : false;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

           return result;
        }
    }
}
