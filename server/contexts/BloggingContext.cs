using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class BloggingContext : DbContext
{
	public DbSet<Blog> Blogs { get; set; }
	public DbSet<User> Users { get; set; }

	public string DbPath { get; }

	public BloggingContext()
	{
		var folder = Environment.SpecialFolder.LocalApplicationData;
		var path = Environment.GetFolderPath(folder);
		DbPath = System.IO.Path.Join(path, "blogging.db");
	}

	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		options.UseSqlite($"Data Source={DbPath}");
	}
}