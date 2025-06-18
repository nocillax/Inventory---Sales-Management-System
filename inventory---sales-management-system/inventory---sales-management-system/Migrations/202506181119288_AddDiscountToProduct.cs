namespace inventory___sales_management_system.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDiscountToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "IsOnSale", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "DiscountPercent", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "DiscountPercent");
            DropColumn("dbo.Products", "IsOnSale");
        }
    }
}
