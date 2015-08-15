namespace ExemploAuth.Migrations
{
    using ExemploAuth.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ExemploAuth.Models.ExemploAuthContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ExemploAuth.Models.ExemploAuthContext context)
        {
            //  This method will be called after migrating to the latest version.

            context.Products.AddOrUpdate(
                p => p.Id,
                new Product
                {
                    Id = 1,
                    nome = "produto 1",
                    codigo = "COD1",
                    descricao =
                        "descrição produto 1",
                    preco = 10
                },
                new Product
                {
                    Id = 2,
                    nome = "produto 2",
                    codigo = "COD2",
                    descricao =
                        "descrição produto 2",
                    preco = 20
                },
                new Product
                {
                    Id = 3,
                    nome = "produto 3",
                    codigo = "COD3",
                    descricao =
                        "descrição produto 3",
                    preco = 30
                }
            );
        }
    }
}
