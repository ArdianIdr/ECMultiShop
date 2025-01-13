namespace ECMultiShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColumnRefundQuantity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderItems", "RefundedQuantity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderItems", "RefundedQuantity");
        }
    }
}
