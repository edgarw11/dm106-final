namespace ExemploAuth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeOrderAndOrderItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "DataPedido", c => c.DateTime(nullable: false));
            AddColumn("dbo.Orders", "DataEntrega", c => c.DateTime(nullable: false));
            AddColumn("dbo.Orders", "Status", c => c.String());
            AddColumn("dbo.Orders", "PrecoTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "PesoTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.OrderItems", "Quantidade", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderItems", "Quantidade");
            DropColumn("dbo.Orders", "PesoTotal");
            DropColumn("dbo.Orders", "PrecoTotal");
            DropColumn("dbo.Orders", "Status");
            DropColumn("dbo.Orders", "DataEntrega");
            DropColumn("dbo.Orders", "DataPedido");
        }
    }
}
