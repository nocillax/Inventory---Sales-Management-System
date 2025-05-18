using inventory___sales_management_system.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace inventory___sales_management_system.Context
{
    public class ISMSDBContext : DbContext
    {
        // Your context has been configured to use a 'ISMSDBContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'inventory___sales_management_system.Context.ISMSDBContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'ISMSDBContext' 
        // connection string in the application configuration file.
        public ISMSDBContext()
            : base("name=ISMSDBContext")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}