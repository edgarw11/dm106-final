using ExemploAuth.br.com.correios.ws;
using ExemploAuth.CRMClient;
using ExemploAuth.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Globalization;
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

        public const string Novo = "Novo";
        public const string Fechado = "Fechado";

        // GET: api/orders/getfrete?id={id}
        [ResponseType(typeof(Customer))]
        [HttpGet]
        [Route("getfrete")]
        public IHttpActionResult GetFrete(int id)
        {
            Order order = db.Orders.Find(id);
            Customer customer = null;
            cResultado result = null;

            Trace.TraceInformation("Nome do usuário: " + User.Identity.Name);
            if (order == null)
            {
                Trace.TraceInformation("Pedido não encontrado.");
                return BadRequest("Pedido não encontrado.");
            }

            if (IsAuthorized(order))
            {
                if (order.OrderItems.Count == 0)
                {
                    return BadRequest("Erro - Pedido sem itens.");
                }

                if (!order.Status.Equals(Novo))
                {
                    return BadRequest("Pedido com status diferente de “novo”.");
                }

                try
                {                    
                    CRMRestClient crmClient = new CRMRestClient();
                    customer = crmClient.GetCustomerByEmail(order.userName);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.StackTrace);
                    return BadRequest("Não foi possível acessar o serviço de CRM. Por favor tente novamente mais tarde.");
                }

                if (customer == null)
                {
                    Trace.TraceInformation("Erro - Não foi possível localizar cadastro do usuário.");
                    return BadRequest("Erro - Não foi possível localizar cadastro do usuário.");
                }

                decimal maiorLargura = 0;
                decimal maiorComprimento = 0;
                decimal alturaTotal = 0;
                decimal diametroTotal = 0;
                decimal pesoTotal = 0;
                decimal precoTotal = 0;

                foreach (OrderItem orderItem in order.OrderItems)
                {
                    alturaTotal += (orderItem.Product.altura * orderItem.Quantidade);
                    pesoTotal += (orderItem.Product.peso * orderItem.Quantidade);
                    precoTotal += (orderItem.Product.preco * orderItem.Quantidade);
                    diametroTotal += (orderItem.Product.diametro * orderItem.Quantidade);

                    if (orderItem.Product.largura > maiorLargura)
                        maiorLargura = orderItem.Product.largura;
                    if (orderItem.Product.comprimento > maiorComprimento)
                        maiorComprimento = orderItem.Product.comprimento;
                    
                }

                try
                {
                    result = getFreteAndDate(customer.zip, precoTotal, pesoTotal.ToString(), maiorComprimento, alturaTotal, maiorLargura, diametroTotal);                    
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.StackTrace);
                    return BadRequest("Não foi possível acessar o serviço dos Correios. Por favor tente novamente mais tarde.");
                }                

                if (!result.Servicos[0].Erro.Equals("0"))
                {
                    return BadRequest("Erro ao calcular o frete: " + result.Servicos[0].MsgErro);
                }


                // Gets a NumberFormatInfo associated with the en-US culture.
                NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
                
                decimal precoFrete = decimal.Parse(result.Servicos[0].Valor, nfi);
                
                int prazoEnt = int.Parse(result.Servicos[0].PrazoEntrega);
                
                DateTime dataEntrega = DateTime.Now;
                dataEntrega = dataEntrega.AddDays(prazoEnt);

                Trace.TraceInformation("Preco dos produtos: " + precoTotal);
                Trace.TraceInformation("Preco do frete: " + precoFrete);

                precoTotal += precoFrete;

                Trace.TraceInformation("Preco total: " + precoTotal);

                // UPDATE THE ORDER
                order.PrecoFrete = precoFrete;
                order.DataEntrega = dataEntrega;
                order.PesoTotal = pesoTotal;
                order.PrecoTotal = precoTotal;
                
                return UpdatedOrder(id, order); //Return the order updated
            }
            else
            {
                Trace.TraceInformation("Usuário não autorizado");
                return BadRequest("Usuário não autorizado");
            }
        }

        private cResultado getFreteAndDate(string sCepDestino, decimal nVlValorDeclarado, string nVlPeso, decimal nVlComprimento, decimal nVlAltura, decimal nVlLargura, decimal nVlDiametro)
        {
            Trace.TraceInformation("getFreteAndDate: - sCepDestino: " + sCepDestino + " - nVlValorDeclarado: " + nVlValorDeclarado + " - nVlPeso: " + nVlPeso
                + " - nVlComprimento: " + nVlComprimento + " - nVlAltura: " + nVlAltura + " - nVlLargura: " + nVlLargura + " - nVlDiametro: " + nVlDiametro);
            string SEDEXVarejo = "40010";
            //string SEDEXaCobrarVarejo = "40045";
            //string SEDEX10Varejo = "40215";
            //string SEDEXHojeVarejo = "40290";
            //string PACVarejo = "41106";

            //1 – Formato caixa/pacote - 2 – Formato rolo/prisma - 3 - Envelope
            int nCdFormato = 1;

            string sCepOrigem = "20081902";// 20081-902 Submariono RJ
            CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();

            cResultado resultado = correios.CalcPrecoPrazo("", "", SEDEXVarejo, sCepOrigem,
            sCepDestino, nVlPeso, nCdFormato, nVlComprimento, nVlAltura, nVlLargura,
            nVlDiametro, "N", nVlValorDeclarado, "N");

            if (resultado.Servicos[0].Erro.Equals("0"))
            {
                Trace.TraceInformation("Valor do frete: " + resultado.Servicos[0].Valor);
                Trace.TraceInformation("Prazo de entrega: " + resultado.Servicos[0].PrazoEntrega);

            }
            else
            {
                Trace.TraceInformation("Erro ao calcular o frete: " + resultado.Servicos[0].Erro);
                Trace.TraceInformation("Detalhes do erro: " + resultado.Servicos[0].MsgErro);
            }

            return resultado;
        }
               
        // GET: api/orders/closeorder?id={id}
        [ResponseType(typeof(Order))]
        [HttpGet]
        [Route("closeorder")]
        public IHttpActionResult CloseOrder(int id)
        {
            Trace.TraceInformation("Nome do usuário: " + User.Identity.Name);
            
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                Trace.TraceInformation("Pedido não encontrado.");
                return BadRequest("Pedido não encontrado.");
            }

            if (IsAuthorized(order))
            {
                if (order.PrecoFrete == 0)
                {
                    Trace.TraceInformation("Erro - O frete deve ser calculado antes de fechar o pedido.");
                    return BadRequest("Erro - O frete deve ser calculado antes de fechar o pedido.");
                }
                else
                {
                    order.Status = Fechado;
                    return UpdatedOrder(id, order);
                }
            }
            else
            {
                Trace.TraceInformation("Usuário não autorizado");
                return BadRequest("Usuário não autorizado");
            }
           
        }
        
        // GET: api/Orders/byname?name={name}
        [ResponseType(typeof(Order))]
        [HttpGet]
        [Route("byname")]
        public IHttpActionResult GetOrdersByName(string name)
        {           
            List<Order> orders = db.Orders.Where(p => p.userName == name).ToList();

            Trace.TraceInformation("Nome do usuário: " + User.Identity.Name);
            if (orders.Count() == 0)
            {
                Trace.TraceInformation("Nenhum pedido foi encontrado para o usuário: " + name);
                return BadRequest("Nenhum pedido foi encontrado para o usuário: " + name);
            }
           
            if (IsAuthorized(orders.First()))
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
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {           
            Order order = db.Orders.Find(id);

            Trace.TraceInformation("Nome do usuário: " + User.Identity.Name);
            if (order == null)
            {
                Trace.TraceInformation("Pedido não encontrado.");
                return BadRequest("Pedido não encontrado.");
            }
                        
            if (IsAuthorized(order))
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

            return UpdatedOrder(id, order);
        }
        
        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (order == null)
            {
                return BadRequest("O pedido está vazio.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            foreach (OrderItem item in order.OrderItems)
            {
                if (item.Quantidade <= 0)
                {
                    return BadRequest("Um ou mais produtos não contém uma quantidade válida.");
                }
            }
            order.Status = Novo;
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
                Trace.TraceInformation("Pedido não encontrado.");
                return BadRequest("Pedido não encontrado.");
            }

            if (IsAuthorized(order))
            {
                db.Orders.Remove(order);
                db.SaveChanges();

                return Ok(order);                
            }
            else
            {
                Trace.TraceInformation("Usuário não autorizado");
                return BadRequest("Usuário não autorizado");
            }

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

        private bool IsAuthorized(Order order)
        {
            bool isAuthorized = false;
            if (User.IsInRole("ADMIN"))
            {
                Trace.TraceInformation("Usuário com papel ADMIN");
                isAuthorized = true;

            }
            if (User.Identity.Name.Equals(order.userName))
            {
                Trace.TraceInformation("Usuário dono do pedido.");
                isAuthorized = true;
            }
            return isAuthorized;
        }

        private IHttpActionResult UpdatedOrder(int id, Order order)
        {
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

            Order orderUpdated = db.Orders.Find(id);

            return Ok(orderUpdated);
        }
    }
}