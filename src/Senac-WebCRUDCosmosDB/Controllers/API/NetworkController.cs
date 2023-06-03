using Gremlin.Net.Driver;
using Gremlin.Net.Process.Traversal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Senac_WebCRUDCosmosDB.Models.API;
using Senac_WebCRUDCosmosDB.Models.GremilinSerializationJSON;
using Senac_WebCRUDCosmosDB.Models.GremilinSerializationJSON.Network;
using System.Linq;

namespace Senac_WebCRUDCosmosDB.Controllers.API
{
    [ApiController]
    public class NetworkController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GraphTraversalSource _g;
        private readonly GremlinClient _client;
        public NetworkController(ILogger<HomeController> logger, GremlinClient client, GraphTraversalSource g)
        {
            _logger = logger;

            // Adicionado o SingleInstance oriundo do Startup.cs
            _g = g;
            _client = client;
        }

        /// <summary>
        /// Lista as relações Nodes e Edges para exibir os dados para em formato de Rede
        /// GET /api/getnetwork
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/getnetwork")]
        public async Task<string> GetNetworkAsync()
        {
            //variaveis de uso amplo
            string strCommand;
            string strOutput;

            ////////////////////////////////////////////////////////////////////////////////
            // Captura os Nodes
            ////////////////////////////////////////////////////////////////////////////////
            strCommand = "g.V().hasLabel('person').project('Id', 'Email', 'FirstName', 'LastName', 'Age').by(T.Id).by('email').by('firstName').by('lastName').by('age')";

            // Executa o comando
            var varTinkerNodesReturn = await _client.SubmitAsync<dynamic>(strCommand);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            strOutput = JsonConvert.SerializeObject(varTinkerNodesReturn);
            var objPeople = JsonConvert.DeserializeObject<List<CJSONPerson>>(strOutput);

            // Gera o modelo para ser inserido na View
            var objOutputNodes = (from objPerson in objPeople
                         select new node()
                         {
                             id = objPerson.Id,
                             label = string.Concat(objPerson.FirstName, ' ', objPerson.LastName)
                         }
                         );

            ////////////////////////////////////////////////////////////////////////////////
            // Monta a Rede
            ////////////////////////////////////////////////////////////////////////////////
            strCommand = "g.V().outE('knows').project('id','from','to').by(id).by(outV().project('Id','Email','FirstName','LastName','Age').by(id).by('email').by('firstName').by('lastName').by('age')).by(inV().project('Id','Email','FirstName','LastName','Age').by(id).by('email').by('firstName').by('lastName').by('age'))";

            // Executa o comando
            var varTinkerReturn = await _client.SubmitAsync<dynamic>(strCommand);

            // Como o dado retorna em JSON, deserializa de acordo com a class
            strOutput = JsonConvert.SerializeObject(varTinkerReturn);
            var objNetwork = JsonConvert.DeserializeObject<List<CJSONNetwork>>(strOutput);

            var objOutputEdges = (from objEdge in objNetwork
                         select new edge()
                         {
                             id     = objEdge.id,
                             @from   = objEdge.@from.Id,
                             to     = objEdge.to.Id
                         }
                         );;


            ////////////////////////////////////////////////////////////////////////////////
            // Formatr a saida conforme o esperado pelo Vis (data.nodes & data.edges)
            ////////////////////////////////////////////////////////////////////////////////
            string strNodes = JsonConvert.SerializeObject(objOutputNodes);
            string strEdges = JsonConvert.SerializeObject(objOutputEdges);

            string strJson = string.Format("{{\"nodes\":{0},\"edges\":{1}}}", strNodes, strEdges);

            return strJson;
        }

    }
}
