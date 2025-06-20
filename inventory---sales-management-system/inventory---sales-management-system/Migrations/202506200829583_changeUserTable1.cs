namespace inventory___sales_management_system.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeUserTable1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "PasswordHash", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(nullable: false));
        }
    }
}
