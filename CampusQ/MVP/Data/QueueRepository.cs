using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Diagnostics;
using CampusQ.MVP.Models;

namespace CampusQ.MVP.Data
{
    public class QueueRepository
    {
        private readonly string _conn;
        public QueueRepository(string connectionString)
        {
            _conn = connectionString;
        }

        public void Add(QueueEntry entry)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();

            using var tran = conn.BeginTransaction(IsolationLevel.Serializable);
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;

            cmd.CommandText = "SELECT ISNULL(MAX(ServiceTicketNumber), 0) + 1 FROM dbo.Queue WHERE Service = @s AND Purpose = @p";
            cmd.Parameters.AddWithValue("@s", entry.Service ?? "");
            cmd.Parameters.AddWithValue("@p", entry.Purpose ?? "");
            var nextServiceNumber = Convert.ToInt32(cmd.ExecuteScalar() ?? 1);

            cmd.Parameters.Clear();
            cmd.CommandText = "INSERT INTO dbo.Queue (ServiceTicketNumber, Purpose, Service, TimeAdded) VALUES (@stn, @p, @s, @t); SELECT CAST(SCOPE_IDENTITY() as int);";
            cmd.Parameters.AddWithValue("@stn", nextServiceNumber);
            cmd.Parameters.AddWithValue("@p", entry.Purpose ?? "");
            cmd.Parameters.AddWithValue("@s", entry.Service ?? "");
            cmd.Parameters.AddWithValue("@t", entry.TimeAdded);
             var id = cmd.ExecuteScalar();
            if (id != null && int.TryParse(id.ToString(), out var ticket))
                entry.TicketNumber = ticket;

            entry.ServiceTicketNumber = nextServiceNumber;


            tran.Commit();
        }

        public List<QueueEntry> GetAll()
        {
            var list = new List<QueueEntry>();
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TicketNumber, ServiceTicketNumber, Purpose, Service, TimeAdded FROM dbo.Queue ORDER BY TicketNumber";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var entry = new QueueEntry
                {
                    TicketNumber = reader.GetInt32(0),
                    ServiceTicketNumber = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    Purpose = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Service = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    TimeAdded = reader.GetDateTime(4)
                };

                list.Add(entry);
            }

            Debug.WriteLine($"QueueRepository.GetAll returned {list.Count} rows from database.");

            return list;
        }

        public List<QueuePersistDto> GetHistoryAll()
        {
            var list = new List<QueuePersistDto>();
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TicketNumber, ServiceTicketNumber, Purpose, Service, TimeAdded, ServedAt FROM dbo.QueueHistory ORDER BY TicketNumber";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var dto = new QueuePersistDto
                {
                    TicketNumber = reader.GetInt32(0),
                    ServiceTicketNumber = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    Purpose = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Service = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    TimeAdded = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4)
                };

                list.Add(dto);
            }

            Debug.WriteLine($"QueueRepository.GetHistoryAll returned {list.Count} rows from database.");

            return list;
        }

        public void Remove(int ticketNumber)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = conn.CreateCommand();
            // Archive the row into QueueHistory before deleting so admin can review served entries later
            cmd.CommandText = @"INSERT INTO dbo.QueueHistory (TicketNumber, ServiceTicketNumber, Purpose, Service, TimeAdded, ServedAt)
SELECT TicketNumber, ServiceTicketNumber, Purpose, Service, TimeAdded, @servedAt FROM dbo.Queue WHERE TicketNumber = @t;
DELETE FROM dbo.Queue WHERE TicketNumber = @t;";
            cmd.Parameters.AddWithValue("@t", ticketNumber);
            cmd.Parameters.AddWithValue("@servedAt", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public void ClearAll()
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM dbo.Queue";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DBCC CHECKIDENT ('dbo.Queue', RESEED, 0);";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Remove all rows from the history table (dbo.QueueHistory).
        /// </summary>
        public void ClearHistory()
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM dbo.QueueHistory";
            cmd.ExecuteNonQuery();
        }
    }
}