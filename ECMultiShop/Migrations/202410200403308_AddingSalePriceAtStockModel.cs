namespace ECMultiShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingSalePriceAtStockModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stocks", "SalePrice", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stocks", "SalePrice");
        }
    }
}
