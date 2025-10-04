using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.Entities;

[Table("Users" , Schema = "test")]
public class Users
{
     public Guid Id { get; set; }
     public string UserName { get; set; } = string.Empty;
     public string  PasswordHash { get; set; } = string.Empty;
     
     public  string Role { get; set; } = string.Empty;
     
     public string RefreshToken { get; set; } = string.Empty;
     
     public DateTime RefreshTokenExpiration { get; set; }
     
}