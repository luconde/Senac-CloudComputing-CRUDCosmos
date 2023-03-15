using Gremlin.Net.Driver;
using Gremlin.Net.Process.Traversal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Senac_WebCRUDCosmosDB.Models.GremilinSerializationJSON;
using Senac_WebCRUDCosmosDB.Models.GremilinSerializationJSON.Report;

namespace Senac_WebCRUDCosmosDB.Controllers
{
    public class ReportController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GraphTraversalSource _g;
        private readonly GremlinClient _client;
        public ReportController(ILogger<HomeController> logger, GremlinClient client, GraphTraversalSource g)
        {
            _logger = logger;

            // Adicionado o SingleInstance oriundo do Startup.cs
            _g = g;
            _client = client;
        }
        
        /// <summary>
        /// Exibe a View da estrutura da rede
        /// GET /Report/Network
        /// </summary>
        /// <returns></returns>
        public IActionResult Network()
        {
            return View("Network");
        }

        /// <summary>
        /// Exibe a View do Relatório sobre o desempenho da rede
        /// GET /Report
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IndexAsync()
        {
            // Variaveis de uso geral
            String strCommand;
            string strOuput;

            #region Conhecem mais pessoas
            //
            // Conhecem mais pessoas
            //

            // Cosmos não implementou ainda Bytecode para Fluent API
            // desta forma, é necessário reescrever a query para modo de texto
            //
            strCommand = string.Format("g.V().hasLabel('person').project('Id','Email','FirstName','LastName','Age', 'NumberOfKnows').by(T.Id).by('email').by('firstName').by('lastName').by('age').by(outE().count()).order().by(select('NumberOfKnows'), decr)");

            //// Executa o comando
            var varTinkerReturnPersonsKnowsMore = await _client.SubmitAsync<dynamic>(strCommand);

            //// Como o dado retorna em JSON, deserializa de acordo com a class
            strOuput = JsonConvert.SerializeObject(varTinkerReturnPersonsKnowsMore);
            var objPersonsKnowsMore = JsonConvert.DeserializeObject<List<CJSONPersonNumberOfKnows>>(strOuput);

            ViewBag.KnowsMore = objPersonsKnowsMore;

            #endregion

            #region São os mais conhecidos
            //
            // Conhecem mais pessoas
            //

            // Cosmos não implementou ainda Bytecode para Fluent API
            // desta forma, é necessário reescrever a query para modo de texto
            //
            strCommand = string.Format("g.V().hasLabel('person').project('Id','Email','FirstName','LastName','Age', 'NumberOfKnowed').by(T.Id).by('email').by('firstName').by('lastName').by('age').by(inE().count()).order().by(select('NumberOfKnowed'), decr)");

            //// Executa o comando
            var varTinkerReturnPersonsKnowed = await _client.SubmitAsync<dynamic>(strCommand);

            //// Como o dado retorna em JSON, deserializa de acordo com a class
            strOuput = JsonConvert.SerializeObject(varTinkerReturnPersonsKnowed);
            var objPersonsKnown = JsonConvert.DeserializeObject<List<CJSONPersonNumberOfKnown>>(strOuput);

            ViewBag.Known = objPersonsKnown;

            #endregion

            #region Não conhece ninguem
            //
            // Não conhece ninguem
            //

            // Cosmos não implementou ainda Bytecode para Fluent API
            // desta forma, é necessário reescrever a query para modo de texto
            //
            strCommand = string.Format("g.V().not(outE()).project('Id','Email','FirstName','LastName','Age').by(T.Id).by('email').by('firstName').by('lastName').by('age')");

            //// Executa o comando
            var varTinkerReturnPersonsKnowsNoOne = await _client.SubmitAsync<dynamic>(strCommand);

            //// Como o dado retorna em JSON, deserializa de acordo com a class
            strOuput = JsonConvert.SerializeObject(varTinkerReturnPersonsKnowsNoOne);
            var objPersonsKnowsNoOne = JsonConvert.DeserializeObject<List<CJSONPerson>>(strOuput);

            ViewBag.KnowsNoOne = objPersonsKnowsNoOne;
            #endregion

            #region Não conhece ninguem e nem é conhecido
            //
            // Não conhece ninguem
            //

            // Cosmos não implementou ainda Bytecode para Fluent API
            // desta forma, é necessário reescrever a query para modo de texto
            //
            strCommand = string.Format("g.V().not(bothE()).project('Id','Email','FirstName','LastName','Age').by(T.Id).by('email').by('firstName').by('lastName').by('age')");

            //// Executa o comando
            var varTinkerReturnPersonsKnowsNoOneNeitherNoOneKnowsHim = await _client.SubmitAsync<dynamic>(strCommand);

            //// Como o dado retorna em JSON, deserializa de acordo com a class
            strOuput = JsonConvert.SerializeObject(varTinkerReturnPersonsKnowsNoOneNeitherNoOneKnowsHim);
            var objPersonsKnowsNoOneNeitherNoOneKnowsHim = JsonConvert.DeserializeObject<List<CJSONPerson>>(strOuput);

            ViewBag.KnowsNoOneNeitherNoOneKnowsHim = objPersonsKnowsNoOneNeitherNoOneKnowsHim;
            #endregion

            return View("Index");
        }
    }
}
