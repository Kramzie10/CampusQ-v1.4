using System;

namespace CampusQ.MVP.Models
{
 public class PersistedUser
 {
 public string Username { get; set; } = "";
 public string PasswordHash { get; set; } = "";
 public string Salt { get; set; } = "";
 public string Role { get; set; } = "Staff";
 public DateTime CreatedAt { get; set; }
 }
}