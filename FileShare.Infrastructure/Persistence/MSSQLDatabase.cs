using FileShare.Entities;
using FileShare.Services;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Globalization;

namespace FileShare.Infrastructure.Persistence
{
    public class MSSQLDatabase : IFileRepository, ILinkRepository
    {
        #region Consts
        const string DateTimeFormat = "yyyy.MM.dd HH:mm:ss";

        #region TableNames
        const string FilesTable = "Files";
        const string LinksTable= "Links";
        #endregion

        #region TableCreateCommand
        const string CreateFilesTable = @$"CREATE TABLE {FilesTable}
                                        (
                                            Id INTEGER NOT NULL IDENTITY(1,1) PRIMARY KEY,
                                            Name nvarchar(255) NOT NULL,
                                            Time varchar(19) NOT NULL,
                                            Size BIGINT NOT NULL
                                        );";

        const string CreateLinksTable = @$"CREATE TABLE {LinksTable}
                                        (
                                            Id varchar(10) NOT NULL PRIMARY KEY,
                                            FileId INTEGER NOT NULL UNIQUE,
                                            FOREIGN KEY (FileId)  REFERENCES {FilesTable} (Id)
                                        );";

        #endregion
        #endregion

        private string connectionString;
        private ConcurrentBag<SqlConnection> connectionPool = new ConcurrentBag<SqlConnection>();
        private SqlConnection GetConnection() 
        {
            for (int i = 0; i < 10000; i++)
            {
                if(connectionPool.TryTake(out var connection) && connection != null)
                    return connection;
            }
            return new SqlConnection(connectionString);
        }

        public MSSQLDatabase(string connectionString)
        {
            this.connectionString = connectionString;

            connectionPool.Add(new SqlConnection(connectionString));

            InitializeTables();
        }

        #region TablesInitialize
        private void InitializeTables()
        {
            var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();

            if (!HasTable(command, FilesTable)) 
            {
                command.CommandText = CreateFilesTable;
                command.ExecuteNonQuery();
            }

            if (!HasTable(command, LinksTable))
            {
                command.CommandText = CreateLinksTable;
                command.ExecuteNonQuery();
            }

            connection.Close();
            connectionPool.Add(connection);
        }

        private bool HasTable(SqlCommand command, string tableName)
        {
            var querry = $"SELECT COUNT(*) FROM sys.tables WHERE name = '{tableName}';";

            command.CommandText = querry;

            return Contains(command, querry);
        }
        #endregion

        #region Additional Methods
        private bool Contains(SqlCommand command, string countQuerry)
        {
            var count = command.ExecuteScalar();

            return Convert.ToInt32(count) > 0;
        }

        public bool Contains(string countQuerry)
        {
            var connection = GetConnection();
            try
            {
                connection.Open();

                var command = connection.CreateCommand();

                var isContains = Contains(command, countQuerry);

                return isContains;
            }
            finally
            {
                connection.Close();
                connectionPool.Add(connection);
            }
        }

        private void ExecuteCommand(string sqlCommand) 
        {
            var connection = GetConnection();
            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText = sqlCommand;
                command.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
                connectionPool.Add(connection);
            }
        }
        #endregion

        #region Files Methods
        public void SaveFile(Entities.File file) 
        {
            var command = $"INSERT INTO {FilesTable} (Name, Time, Size) VALUES (N'{file.Name}', '{ToStringTime(file.Time)}', {file.Size})";

            ExecuteCommand(command);
        }

        public void DeleteFile(int id)
        {
            var command = $"DELETE FROM {FilesTable} WHERE Id = {id};";

            ExecuteCommand(command);
        }

        public Entities.File? GetFile(int id)
        {
            string querry = @$"SELECT Id, Name, Time, Size FROM {FilesTable} WHERE Id = {id};";

            return GetOne(querry, reader =>
            {
                return new Entities.File()
                { 
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = Convert.ToString(reader["Name"]),
                    Time = ToDateTime(Convert.ToString(reader["Time"])),
                    Size = Convert.ToInt64(reader["Size"])
                };
            });
        }

        public List<Entities.File> GetAllFiles()
        {
            string querry = @$"SELECT Id, Name, Time, Size FROM {FilesTable};";

            return GetMany(querry, (reader) =>
            {
                return new Entities.File()
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = Convert.ToString(reader["Name"]),
                    Time = ToDateTime(Convert.ToString(reader["Time"])),
                    Size = Convert.ToInt64(reader["Size"])
                };
            });
        }
        #endregion

        #region Links Methods
        public void SaveLink(Link link)
        {
            var command = $"INSERT INTO {LinksTable} (Id, FileId) VALUES ('{link.Id}', {link.File.Id})";

            ExecuteCommand(command);
        }

        public void DeleteLink(string id)
        {
            var command = $"DELETE FROM {LinksTable} WHERE Id = '{id}';";

            ExecuteCommand(command);
        }

        public Link? GetLink(string id)
        {
            string querry = @$"SELECT {LinksTable}.Id, FileId, Name, Time, Size FROM {LinksTable} LEFT JOIN {FilesTable} ON {LinksTable}.FileId={FilesTable}.Id WHERE {LinksTable}.Id = '{id}';";

            return GetOne(querry, reader =>
            {
                return new Entities.Link()
                {
                    Id = Convert.ToString(reader["Id"]),
                    File = new Entities.File() 
                    {
                        Id = Convert.ToInt32(reader["FileId"]),
                        Name = Convert.ToString(reader["Name"]),
                        Time = ToDateTime(Convert.ToString(reader["Time"])),
                        Size = Convert.ToInt64(reader["Size"])
                    }
                };
            });
        }

        public Link? GetLinkByFile(int fileId)
        {
            string querry = @$"SELECT {LinksTable}.Id, FileId, Name, Time, Size FROM {LinksTable} LEFT JOIN {FilesTable} ON {LinksTable}.FileId={FilesTable}.Id WHERE {FilesTable}.Id = {fileId};";

            return GetOne(querry, reader =>
            {
                return new Entities.Link()
                {
                    Id = Convert.ToString(reader["Id"]),
                    File = new Entities.File()
                    {
                        Id = Convert.ToInt32(reader["FileId"]),
                        Name = Convert.ToString(reader["Name"]),
                        Time = ToDateTime(Convert.ToString(reader["Time"])),
                        Size = Convert.ToInt64(reader["Size"])
                    }
                };
            });
        }

        #endregion

        #region Datetime Convert Methods
        private DateTime ToDateTime(string dateTimeStr)
        {
            return DateTime.ParseExact(dateTimeStr, DateTimeFormat, CultureInfo.InvariantCulture);
        }

        private string ToStringTime(DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        }
        #endregion

        #region Object mapping
        delegate T Mapper<T>(SqlDataReader reader);

        private T? GetOne<T>(string selectQuerry, Mapper<T> mapFunc)
        {
            var connection = GetConnection();
            try
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = selectQuerry;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        return mapFunc(reader);
                    }
                    return default(T);
                }
            }
            finally 
            {
                connection.Close();
                connectionPool.Add(connection);
            }
        }

        private List<T> GetMany<T>(string selectQuerry, Mapper<T> mapFunc)
        {
            var connection = GetConnection();
            try
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = selectQuerry;

                List<T> entities = new List<T>();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            T entity = mapFunc(reader);
                            entities.Add(entity);
                        }
                    }
                }
                return entities;
            }
            finally
            {
                connection.Close();
                connectionPool.Add(connection);
            }
        }
        #endregion
    }
}
