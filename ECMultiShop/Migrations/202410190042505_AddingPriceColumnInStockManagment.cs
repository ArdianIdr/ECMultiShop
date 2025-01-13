namespace ECMultiShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingPriceColumnInStockManagment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stocks", "Price", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stocks", "Price");
        }
    }
}
