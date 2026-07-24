using System;

namespace CampusQ.MVP.Models
{
 public class UserAccount
 {
 public string Username { get; set; } = "";
 public string PasswordHash { get; set; } = ""; // base64
 public string Salt { get; set; } = ""; // base64
 public string Role { get; set; } = "Staff";
 public DateTime CreatedAt { get; set; }
 }
}