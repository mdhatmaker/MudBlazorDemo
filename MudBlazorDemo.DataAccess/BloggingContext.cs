namespace MudBlazorDemo.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MudBlazorDemo.Models;
using System;
using System.Collections.Generic;

public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    //private IConfiguration _config { get; set; }

    public BloggingContext()
    {
    }

    public BloggingContext(DbContextOptions<BloggingContext> options)
        : base(options)
    {
    }

    //public BloggingContext(IConfiguration config)
    //{
    //    //_config = config;
    //    //var folder = Environment.SpecialFolder.LocalApplicationData;
    //    //var path = Environment.GetFolderPath(folder);
    //    //DbPath = System.IO.Path.Join(path, "blogging.db");
    //}

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=GAME-PC;Database=EFBloggingDb;User=sa;Password=Wikki6969;MultipleActiveResultSets=True;Trust Server Certificate=True;");
    //=> options.UseSqlServer($"{_config.GetConnectionString("Blogging")}");
}



//public class ApplicationDbContext : DbContext
//{
//    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//        : base(options)
//    {
//    }
//}

