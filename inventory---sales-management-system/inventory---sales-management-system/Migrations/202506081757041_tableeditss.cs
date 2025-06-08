namespace inventory___sales_management_system.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tableeditss : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "DateEdited", c => c.DateTime(nullable: false));
            DropColumn("dbo.Products", "DateAdded");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "DateAdded", c => c.DateTime(nullable: false));
            DropColumn("dbo.Products", "DateEdited");
        }
    }
}
