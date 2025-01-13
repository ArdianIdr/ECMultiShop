namespace ECMultiShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CartItemsValues : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartItems", "Size", c => c.String());
            AddColumn("dbo.CartItems", "VariantDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartItems", "VariantDescription");
            DropColumn("dbo.CartItems", "Size");
        }
    }
}
