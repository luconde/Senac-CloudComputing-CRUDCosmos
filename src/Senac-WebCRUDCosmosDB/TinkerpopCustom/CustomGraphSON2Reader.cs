using Gremlin.Net.Structure.IO.GraphSON;
using System.Text.Json;

namespace Senac_WebCRUDCosmosDB.TinkerpopCustom
{
    /// <summary>
    /// Esta class foi criada para tratar numeros, de acordo com a documentação Gremilin.NET > 3.5.0 tem problema
    /// de serialização
    /// link: https://stackoverflow.com/questions/68092798/gremlin-net-deserialize-number-property
    /// </summary>
    public class CustomGraphSON2Reader : GraphSON2Reader
    {
        public override dynamic ToObject(JsonElement graphSon) =>
            graphSon.ValueKind switch
            {
                // numbers
                JsonValueKind.Number when graphSon.TryGetInt32(out var intValue) => intValue,
                JsonValueKind.Number when graphSon.TryGetInt64(out var longValue) => longValue,
                JsonValueKind.Number when graphSon.TryGetDecimal(out var decimalValue) => decimalValue,


                _ => base.ToObject(graphSon)
            };
    }
}
