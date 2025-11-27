using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Vinicola_app.Services
{
    public class FiwareService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FiwareService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<bool> CriarDispositivoFiware(string deviceId)
        {
            var urlBase = _configuration["Fiware:Url"];
            var porta = _configuration["Fiware:PortIOT"];

            // Monta a URL: http://{{url}}:4041/iot/devices
            var endpoint = $"http://{urlBase}:{porta}/iot/devices";

            // Cabeçalhos padrão do FIWARE (ajuste conforme sua configuração do Docker)
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("fiware-service", _configuration["Fiware:Service"]);
            _httpClient.DefaultRequestHeaders.Add("fiware-servicepath", _configuration["Fiware:ServicePath"]);

            // Montagem do Payload (JSON)
            var payloadObj = new
            {
                devices = new[]
                {
                    new
                    {
                        device_id = deviceId,
                        entity_name = $"urn:ngsi-ld:Logger:{deviceId}", 
                        entity_type = "Logger",
                        protocol = "PDI-IoTA-UltraLight",
                        transport = "MQTT",
                        commands = new[]
                        {
                            new { name = "TEMP_ALARM", type = "command" },
                            new { name = "HUM_ALARM", type = "command" },
                            new { name = "LUM_ALARM", type = "command" },
                            new { name = "OFF", type = "command" }
                        },
                        attributes = new[]
                        {
                            new { object_id = "s", name = "state", type = "Text" },
                            new { object_id = "l", name = "luminosity", type = "Integer" },
                            new { object_id = "h", name = "humidity", type = "float" },
                            new { object_id = "t", name = "temperature", type = "float" }
                        }
                    }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payloadObj),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.PostAsync(endpoint, jsonContent);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Logar erro se necessário
                Console.WriteLine("Erro ao conectar no FIWARE: " + ex.Message);
                return false;
            }
        }
    }
}
