namespace inventory___sales_management_system.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixProductTabel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Products", new[] { "CategoryId" });
            AlterColumn("dbo.Products", "CategoryId", c => c.Int());
            CreateIndex("dbo.Products", "CategoryId");

            // Add FK with ON DELETE SET NULL using SQL
            Sql(@"
        ALTER TABLE dbo.Products DROP CONSTRAINT FK_dbo.Products_dbo.Categories_CategoryId;
        ALTER TABLE dbo.Products
        ADD CONSTRAINT FK_dbo.Products_dbo.Categories_CategoryId
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId) ON DELETE SET NULL;
    ");
        }


        public override void Down()
        {
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Products", new[] { "CategoryId" });
            AlterColumn("dbo.Products", "CategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.Products", "CategoryId");
            AddForeignKey("dbo.Products", "CategoryId", "dbo.Categories", "CategoryId", cascadeDelete: true);
        }

    }
}
