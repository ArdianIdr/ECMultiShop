namespace ECMultiShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingRefundReason : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "RefundReason", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "RefundReason");
        }
    }
}
