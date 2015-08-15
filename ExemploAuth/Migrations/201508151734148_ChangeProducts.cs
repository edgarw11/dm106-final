namespace ExemploAuth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeProducts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "cor", c => c.String());
            AddColumn("dbo.Products", "modelo", c => c.String(nullable: false));
            AddColumn("dbo.Products", "peso", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "altura", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "largura", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "comprimento", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "diametro", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "url", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "url");
            DropColumn("dbo.Products", "diametro");
            DropColumn("dbo.Products", "comprimento");
            DropColumn("dbo.Products", "largura");
            DropColumn("dbo.Products", "altura");
            DropColumn("dbo.Products", "peso");
            DropColumn("dbo.Products", "modelo");
            DropColumn("dbo.Products", "cor");
        }
    }
}
