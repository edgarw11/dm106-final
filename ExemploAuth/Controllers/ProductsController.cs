using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ExemploAuth.Models;
using System.Diagnostics;

namespace ExemploAuth.Controllers
{
    [Authorize]
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private ExemploAuthContext db = new ExemploAuthContext();

        // GET: api/products/byname?name={name}
        [ResponseType(typeof(Product))]
        [HttpGet]
        [Route("byname")]
        public IHttpActionResult GetProductByName(string name)
        {
            var product = db.Products.Where(p => p.nome == name);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // GET: api/Products
        public IQueryable<Product> GetProducts()
        {
            Trace.TraceInformation("Nome do usuário: " + User.Identity.Name);
            if (User.IsInRole("USER"))
            {
                Trace.TraceInformation("Usuário com papel USER");
            }
            else if (User.IsInRole("ADMIN"))
            {
                Trace.TraceInformation("Usuário com papel ADMIN");
            }
            return db.Products;
        }

        // GET: api/Products/5
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/Products/5
        //[Authorize(Users="inatel@inatel.br")]
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return BadRequest("O id informado não corresponde ao id do modelo.");
            }

            //Não deverá permitir a alteração do código e/ou do modelo 
            //para um valor que já exista na tabela
            var productsCode = db.Products.Where(p => (p.codigo == product.codigo || p.modelo == product.modelo) && p.Id != id);
            List<Product> productsRes = productsCode.ToList();
            if (productsRes.Count > 0)
            {
                foreach (Product prod in productsRes)
                {
                    if (prod.codigo == product.codigo)
                        return BadRequest("Erro: O codigo informado já existe: " + product.codigo);
                    else if (prod.modelo == product.modelo)
                        return BadRequest("Erro: O modelo informado já existe: " + product.modelo);

                }
                
            }

            db.Entry(product).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            Product productUpdated = db.Products.Find(id);

            return Ok(productUpdated);
        }

        // POST: api/Products
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Products.Add(product);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();

            return Ok(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.Id == id) > 0;
        }
    }
}