namespace inventory___sales_management_system.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateProductsTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "Name", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false));
        }
    }
}
