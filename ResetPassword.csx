// Quick script to reset admin password
using BCrypt.Net;

var newPassword = "admin123";
var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);

Console.WriteLine($"New BCrypt hash for '{newPassword}':");
Console.WriteLine(hash);
