using Gremlin.Net.Driver;
using Gremlin.Net.Process.Traversal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Senac_WebCRUDCosmosDB.Models.GremilinSerializationJSON;
using Senac_WebCRUDCosmosDB.Models.GremilinSerializationJSON.Association;

namespace Senac_WebCRUDCosmosDB.Controllers
{
    public class AssociationController : Controller
    {
        private readonly ILogger<AssociationController> _logger;
        private readonly GraphTraversalSource _g;
        private readonly GremlinClient _client;

        public AssociationController(ILogger<AssociationController> logger, GremlinClient client,  GraphTraversalSource g)
        {
            _logger = logger;

            // Adicionado o SingleInstance oriundo do Startup.cs
            _g = g;
            _client = client;
        }

        /// <summary>
        /// Lista as associações entre as pessoas
        /// GET /Association/{id} 
        /// </summary>
        /// <param name="Id">GUID de uma pessoa</param>
        /// <returns></returns>
        public async Task<IActionResult> ListAsync(string Id)
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Lista as associações entre as pessoas
            ////////////////////////////////////////////////////////////////////////////////
            String strCommand = string.Format("g.V('{0}').project('Id', 'Email','FirstName','LastName').by(T.Id).by('email').by('firstName').by('lastName')",
                Id);

            // Executa o comando
            var varTinkerReturn = await _client.SubmitWithSingleResultAsync<dynamic>(strCommand);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            string strOuput = JsonConvert.SerializeObject(varTinkerReturn);
            var objPerson = JsonConvert.DeserializeObject<CJSONPerson>(strOuput);

            strCommand = string.Format("g.V().hasLabel('person').not(has('email', '{0}')).project('Id','Email','FirstName','LastName','Conhece','Conhecido').by(T.Id).by('email').by('firstName').by('lastName').by(__.in('knows').has('email','{1}').fold().coalesce(unfold().constant(true), constant(false))).by(__.out('knows').has('email','{2}').fold().coalesce(unfold().constant(true), constant(false)))\r\n",
                objPerson.Email,
                objPerson.Email,
                objPerson.Email
                );

            // Executa o comando
            varTinkerReturn = await _client.SubmitAsync<dynamic>(strCommand);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            strOuput = JsonConvert.SerializeObject(varTinkerReturn);
            var objPeople = JsonConvert.DeserializeObject<List<CJSONPersonAssociation>>(strOuput);

            // Gera o modelo para a View
            ViewData.Model = objPeople;
            ViewBag.Id = Id;
            ViewBag.Person = objPerson;

            return View("ListForm");

        }

        /// <summary>
        /// Atualiza as relações entre as pessoas
        /// POST /Association/Process
        /// </summary>
        /// <param name="Id">GUID da pessoa a ter a as suas associações atualizadas, enviado via método POST</param>
        /// <param name="ids">GUIDs das pessoas que precisam ser atualizadas, enviado via método POST</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ProcessAsync(string Id, string[] ids) 
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Atualiza as relações entre as pessoas
            ////////////////////////////////////////////////////////////////////////////////
            String strCommand = string.Format("g.V('{0}').project('Id', 'Email','FirstName','LastName').by(T.Id).by('email').by('firstName').by('lastName')",
                                                Id);

            // Executa o comando
            var varTinkerReturn = await _client.SubmitWithSingleResultAsync<dynamic>(strCommand);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            string strOuput = JsonConvert.SerializeObject(varTinkerReturn);
            var objPerson = JsonConvert.DeserializeObject<CJSONPersonAssociation>(strOuput);

            strCommand = string.Format("g.V().has('email','{0}').outE().drop()",
                                        objPerson.Email);

            // Executa o comando
            await _client.SubmitWithSingleResultAsync<dynamic>(strCommand);

            foreach (var item in ids)
            {
                strCommand = string.Format("g.V().has('email','{0}').addE('knows').to(__.V().has('email','{1}'))",
                                            objPerson.Email,
                                            item
                                            );
                // Executa o comando
                await _client.SubmitWithSingleResultAsync<dynamic>(strCommand);

            }

            return View("Process");
        }
    }
}
