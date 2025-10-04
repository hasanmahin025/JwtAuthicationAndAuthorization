using Authentication.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Data;

public class MyDbcontext : DbContext
{
      public MyDbcontext(DbContextOptions<MyDbcontext> options) : base(options){}
      
     public DbSet<Users> Users { get; set; }
      
}