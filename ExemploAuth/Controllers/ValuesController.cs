using ExemploAuth.br.com.correios.ws;
using ExemploAuth.CRMClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ExemploAuth.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/?email={email}
        //e.g.: api/values/?email=inatel_mobile@inatel.br
        public IHttpActionResult GetCustomerByEmail(String email)
        {
            CRMRestClient crmClient = new CRMRestClient();
            Customer customer = crmClient.GetCustomerByEmail(email);

            return Ok(customer);
        }

        // GET api/values/?cepOrig={cepOrig}&cepDest={cepDest}
        //e.g.: api/values/?cepOrig=37540000&cepDest=37410000
        public string GetFreteByCeps(String cepOrig, String cepDest)
        {
            CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
            cResultado resultado = correios.CalcPrecoPrazo("", "", "40010", cepOrig,
            cepDest, "1", 1, 40, 40, 40,
            80, "N", 30, "S");

            if (resultado.Servicos[0].Erro.Equals("0"))
            {
                Trace.TraceInformation("Valor do frete: " + resultado.Servicos[0].Valor);

            } else
            {
                Trace.TraceInformation("Erro ao calcular o frete: " + resultado.Servicos[0].Erro);
                Trace.TraceInformation("Detalhes do erro: " + resultado.Servicos[0].MsgErro);
            }

            return resultado.Servicos[0].Valor;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
