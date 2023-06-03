using Microsoft.AspNetCore.Mvc;
using Senac_WebCRUDCosmosDB.Models;
using System.Diagnostics;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Driver;
using Newtonsoft.Json;
using Senac_WebCRUDCosmosDB.Models.Form;
using Senac_WebCRUDCosmosDB.Models.GremilinSerializationJSON;

namespace Senac_WebCRUDCosmosDB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GraphTraversalSource _g;
        private readonly GremlinClient _client;
        public HomeController(ILogger<HomeController> logger, GremlinClient client, GraphTraversalSource g)
        {
            _logger = logger;

            // Adicionado o SingleInstance oriundo do Startup.cs
            _g = g;
            _client = client;
        }

        /// <summary>
        /// Retorna os dados de uma pessoa
        /// GET /Home/{id}
        /// </summary>
        /// <param name="Id">GUID de uma pessoa</param>
        /// <returns></returns>
        public async Task<IActionResult> Item(string? Id)
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Retorna os dados de uma pessoa
            ////////////////////////////////////////////////////////////////////////////////
            String strCommand = string.Format("g.V('{0}').project('Id', 'Email', 'FirstName', 'LastName', 'Age').by(T.Id).by('email').by('firstName').by('lastName').by('age')",
                Id);

            // Executa o comando
            var varTinkerReturn = await _client.SubmitWithSingleResultAsync<dynamic>(strCommand);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            string strOuput = JsonConvert.SerializeObject(varTinkerReturn);
            var objPerson = JsonConvert.DeserializeObject<CJSONPerson>(strOuput);

            // Gera o modelo para ser inserido na View
            ViewData.Model = objPerson;

            return View("Item");
        }

        /// <summary>
        /// Retorna os dados da lista de pessoas disponíveis
        /// GET /Home/
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IndexAsync()
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Retorna os dados da lista de pessoas disponíveis
            ////////////////////////////////////////////////////////////////////////////////
            string strCommand = "g.V().hasLabel('person').project('Id', 'Email', 'FirstName', 'LastName', 'Age').by(T.Id).by('email').by('firstName').by('lastName').by('age')";

            // Executa o comando
            var varTinkerReturn = await _client.SubmitAsync<dynamic>(strCommand);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            string strOuput = JsonConvert.SerializeObject(varTinkerReturn);
            var objPeople = JsonConvert.DeserializeObject<List<CJSONPerson>>(strOuput);

            // Gera o modelo para ser inserido na View
            ViewData.Model = objPeople;
            return View();
        }

        /// <summary>
        /// Retorna a view para criar uma nova pessoa
        /// GET /Home/Create
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View("CreateForm");
        }

        /// <summary>
        /// Insere uma nova pessoa na rede
        /// POST /Home/Create
        /// </summary>
        /// <param name="objPerson">Dados do formulario</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([Bind("Email", "FirstName, LastName, Age")] CFormPerson objPerson)
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Insere uma nova pessoa na rede
            ////////////////////////////////////////////////////////////////////////////////
            String strCommand = String.Format("g.addV('person').property('email', '{0}').property('firstName', '{1}').property('lastName', '{2}').property('age', '{3}')",
                objPerson.Email,
                objPerson.FirstName,
                objPerson.LastName,
                objPerson.Age
                );

            // Executa o comando
            var varTinkerReturn = await _client.SubmitAsync<dynamic>(strCommand);

            return View();
        }

        /// <summary>
        /// Retorna os dados da lista de pessoas disponíveis
        /// GET /Home/Update/{id};
        /// </summary>
        /// <param name="Id">GUID de uma pessoa</param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateAsync(string? Id)
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Retorna os dados da lista de pessoas disponíveis
            ////////////////////////////////////////////////////////////////////////////////
            String strCommand = string.Format("g.V('{0}').project('Id', 'Email', 'FirstName', 'LastName', 'Age').by(T.Id).by('email').by('firstName').by('lastName').by('age')",
                Id);

            // Executa o comando
            var varTinkerReturn = await _client.SubmitWithSingleResultAsync<dynamic>(strCommand);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            string strOuput = JsonConvert.SerializeObject(varTinkerReturn);
            var objPerson = JsonConvert.DeserializeObject<CJSONPerson>(strOuput);

            // Gera o modelo para ser inserido na View
            ViewData.Model = objPerson;

            return View("UpdateForm");
        }

        /// <summary>
        /// Atualiza os dados de uma pessoa
        /// POST /Home/Update/{id}
        /// </summary>
        /// <param name="Id">GUID de uma pessoa</param>
        /// <param name="objPersonForm">Dados do formulario</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateAsync(string Id, [Bind("Email, FirstName, LastName, Age")] CFormPerson objPersonForm)
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Atualiza os dados de uma pessoa
            ////////////////////////////////////////////////////////////////////////////////
            String strCommand = string.Format("g.V('{0}').property('firstName','{1}').property('lastName','{2}').property('age','{3}')",
                Id,
                objPersonForm.FirstName,
                objPersonForm.LastName,
                objPersonForm.Age
                );

            // Como o dado retorna em JSON, deserializa de acordo com a class
            var varTinkerReturn = await _client.SubmitAsync<dynamic>(strCommand);

            return View();
        }

        /// <summary>
        /// Apaga uma pessoa especifica
        /// GET /Home/Delete/{id}
        /// </summary>
        /// <param name="Id">GUID de uma pessoa</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteAsync(string Id)
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Apaga uma pessoa especifica
            ////////////////////////////////////////////////////////////////////////////////
            String strCommand = string.Format("g.V('{0}').drop()",
                Id);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            var varTinkerReturn = await _client.SubmitAsync<dynamic>(strCommand);

            return View();
        }

        /// <summary>
        /// Exibir a pagina de sobre
        /// GET /Home/About
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}