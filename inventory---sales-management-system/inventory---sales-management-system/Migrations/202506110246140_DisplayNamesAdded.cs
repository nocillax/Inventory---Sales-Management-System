namespace inventory___sales_management_system.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DisplayNamesAdded : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Sales", "BuyerName", c => c.String(nullable: false));
            AlterColumn("dbo.StockEntries", "Supplier", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StockEntries", "Supplier", c => c.String());
            AlterColumn("dbo.Sales", "BuyerName", c => c.String());
        }
    }
}
