namespace ECMultiShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsActiveToStockModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stocks", "IsActive", c => c.Boolean(nullable: false));
            DropColumn("dbo.Products", "IsActive");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "IsActive", c => c.Boolean(nullable: false));
            DropColumn("dbo.Stocks", "IsActive");
        }
    }
}
