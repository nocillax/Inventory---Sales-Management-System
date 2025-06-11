namespace inventory___sales_management_system.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateStockEntryAnnot : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StockEntries", "Supplier", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StockEntries", "Supplier", c => c.String(nullable: false));
        }
    }
}
