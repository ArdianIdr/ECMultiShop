namespace ECMultiShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class couponUsage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Coupons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 20),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsActive = c.Boolean(nullable: false),
                        ExpirationDate = c.DateTime(nullable: false),
                        UsageLimit = c.Int(),
                        UsageCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Coupons");
        }
    }
}
