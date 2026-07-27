using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CampusQ.MVP.Models;

namespace CampusQ.MVP.Data
{
 public class UserRepository
 {
 private readonly string _conn;
 public UserRepository(string connectionString)
 {
 _conn = connectionString;
 }

 public List<UserAccount> GetAll()
 {
 var result = new List<UserAccount>();
 using var conn = new SqlConnection(_conn);
 conn.Open();
 using var cmd = conn.CreateCommand();
 cmd.CommandText = "SELECT Username, PasswordHash, Salt, Role, CreatedAt FROM dbo.Users";
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 result.Add(new UserAccount
 {
 Username = reader.GetString(0),
 PasswordHash = reader.GetString(1),
 Salt = reader.GetString(2),
 Role = reader.GetString(3),
 CreatedAt = reader.GetDateTime(4)
 });
 }
 return result;
 }

 public void Add(UserAccount account)
 {
 using var conn = new SqlConnection(_conn);
 conn.Open();
 using var cmd = conn.CreateCommand();
 cmd.CommandText = "INSERT INTO dbo.Users (Username, PasswordHash, Salt, Role, CreatedAt) VALUES (@u,@p,@s,@r,@c)";
 cmd.Parameters.AddWithValue("@u", account.Username);
 cmd.Parameters.AddWithValue("@p", account.PasswordHash);
 cmd.Parameters.AddWithValue("@s", account.Salt);
 cmd.Parameters.AddWithValue("@r", account.Role);
 cmd.Parameters.AddWithValue("@c", account.CreatedAt);
 cmd.ExecuteNonQuery();
 }

 public void Remove(string username)
 {
 using var conn = new SqlConnection(_conn);
 conn.Open();
 using var cmd = conn.CreateCommand();
 cmd.CommandText = "DELETE FROM dbo.Users WHERE Username = @u";
 cmd.Parameters.AddWithValue("@u", username);
 cmd.ExecuteNonQuery();
 }
 }
}