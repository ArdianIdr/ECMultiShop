namespace ECMultiShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class makingUsageCountNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Coupons", "UsageCount", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Coupons", "UsageCount", c => c.Int(nullable: false));
        }
    }
}
