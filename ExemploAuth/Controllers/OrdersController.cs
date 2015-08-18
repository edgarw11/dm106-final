using ExemploAuth.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace ExemploAuth.Controllers
{
    [Authorize]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private ExemploAuthContext db = new ExemploAuthContext();

        // GET: api/Orders/byname?name={name}
        [ResponseType(typeof(Order))]
        [HttpGet]
        [Route("byname")]
        public IHttpActionResult GetOrdersByName(string name)
        {
            bool authorized = false;
            List<Order> orders = db.Orders.Where(p => p.userName == name).ToList();

            Trace.TraceInformation("Nome do usuário: " + User.Identity.Name);
            if (orders.Count() == 0)
            {
                Trace.TraceInformation("Nenhum pedido foi encontrado para o usuário: " + name);
                return BadRequest("Nenhum pedido foi encontrado para o usuário: " + name);
            }
            else
            {
                if (User.IsInRole("ADMIN"))
                {
                    Trace.TraceInformation("Usuário com papel ADMIN");
                    authorized = true;

                }
                if (User.Identity.Name.Equals(orders.First().userName))
                {
                    Trace.TraceInformation("Usuário dono do pedido.");
                    authorized = true;
                }

            }

            if (authorized)
            {
                return Ok(orders);
            }
            else
            {
                Trace.TraceInformation("Usuário não autorizado");
                return BadRequest("Usuário não autorizado");
            }
        }

        // GET: api/Orders
        //public IQueryable<Order> GetOrders()
        [Authorize(Roles = "ADMIN")]
        public List<Order> GetOrders()
        {
            Trace.TraceInformation("INFO - GetOrders");
                        
            //return db.Orders;
            return db.Orders.Include(order => order.OrderItems).ToList();
        }

        // GET: api/Orders/5
        // TODO: Essa operação poderá ser acessível somente pelo administrador ou pelo usuário que é dono do pedido
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            bool authorized = false;
            Order order = db.Orders.Find(id);

            Trace.TraceInformation("Nome do usuário: " + User.Identity.Name);
            if (order == null)
            {
                Trace.TraceInformation("Pedido não encontrado.");
                return BadRequest("Pedido não encontrado.");
            }
            else
            {
                if (User.IsInRole("ADMIN"))
                {
                        Trace.TraceInformation("Usuário com papel ADMIN");
                        authorized = true;

                }
                if (User.Identity.Name.Equals(order.userName))
                {
                    Trace.TraceInformation("Usuário dono do pedido.");
                    authorized = true;
                }

            }
            
            if (authorized)
            {                
                return Ok(order);
            }
            else
            {
                Trace.TraceInformation("Usuário não autorizado");
                return BadRequest("Usuário não autorizado");
            }
        }

        // PUT: api/Orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            order.Status = "Novo";
            order.PesoTotal = 0;
            order.PrecoFrete = 0;
            order.PrecoTotal = 0;
            order.DataPedido = DateTime.Now;
            order.DataEntrega = DateTime.Now;

            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            db.Orders.Remove(order);
            db.SaveChanges();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }
    }
}