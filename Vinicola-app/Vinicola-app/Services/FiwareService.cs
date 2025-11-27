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

        // Alterado para HttpResponseMessage para permitir leitura de erro no Controller
        public async Task<HttpResponseMessage> CriarDispositivoFiware(string deviceId)
        {
            var urlBase = _configuration["Fiware:Url"];
            var portaIot = _configuration["Fiware:PortIOT"];      // 4041
            var portaBroker = _configuration["Fiware:PortBroker"]; // 1026 (Orion)

            // -----------------------------------------------------------------
            // PASSO 1: Criar Dispositivo no IoT Agent (Porta 4041)
            // -----------------------------------------------------------------
            var endpointIot = $"http://{urlBase}:{portaIot}/iot/devices";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("fiware-service", _configuration["Fiware:Service"]);
            _httpClient.DefaultRequestHeaders.Add("fiware-servicepath", _configuration["Fiware:ServicePath"]);

            var payloadDevice = new
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

            var jsonContentDevice = new StringContent(
                JsonSerializer.Serialize(payloadDevice),
                Encoding.UTF8,
                "application/json");

            try
            {
                // Executa a primeira chamada
                var responseIot = await _httpClient.PostAsync(endpointIot, jsonContentDevice);

                // Se falhar a primeira, retorna o erro imediatamente
                if (!responseIot.IsSuccessStatusCode)
                    return responseIot;

                // -----------------------------------------------------------------
                // PASSO 2: Registrar Comandos no Orion Context Broker (Porta 1026)
                // -----------------------------------------------------------------
                // Isso vincula os atributos de comando (provider) ao IoT Agent

                var endpointBroker = $"http://{urlBase}:{portaBroker}/v2/registrations";

                var payloadRegistration = new
                {
                    description = "Logger Commands",
                    dataProvided = new
                    {
                        entities = new[]
                        {
                            new { id = $"urn:ngsi-ld:Logger:{deviceId}", type = "Logger" }
                        },
                        attrs = new[] { "TEMP_ALARM", "HUM_ALARM", "LUM_ALARM", "OFF" }
                    },
                    provider = new
                    {
                        http = new { url = $"http://{urlBase}:{portaIot}" }, // Aponta de volta para o IoT Agent (4041)
                        legacyForwarding = true
                    }
                };

                var jsonContentBroker = new StringContent(
                    JsonSerializer.Serialize(payloadRegistration),
                    Encoding.UTF8,
                    "application/json");

                // Executa a segunda chamada
                var responseBroker = await _httpClient.PostAsync(endpointBroker, jsonContentBroker);

                // Retorna o resultado final (se o passo 2 falhar, o erro será retornado aqui)
                return responseBroker;
            }
            catch (Exception)
            {
                // Retorna um erro genérico de serviço indisponível caso a conexão caia
                return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("Erro de conexão com o servidor FIWARE.")
                };
            }
        }

        public async Task<string> ListarDispositivos()
        {
            var urlBase = _configuration["Fiware:Url"];
            var portaIot = _configuration["Fiware:PortIOT"];
            var endpoint = $"http://{urlBase}:{portaIot}/iot/devices";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("fiware-service", _configuration["Fiware:Service"]);
            _httpClient.DefaultRequestHeaders.Add("fiware-servicepath", _configuration["Fiware:ServicePath"]);

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Erro ao consultar FIWARE: {ex.Message}";
            }
        }

        public async Task<string> ObterDadosAtuais(string deviceId)
        {
            var urlBase = _configuration["Fiware:Url"];
            var portaBroker = _configuration["Fiware:PortBroker"];
            var endpoint = $"http://{urlBase}:{portaBroker}/v2/entities/urn:ngsi-ld:Logger:{deviceId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("fiware-service", _configuration["Fiware:Service"]);
            _httpClient.DefaultRequestHeaders.Add("fiware-servicepath", _configuration["Fiware:ServicePath"]);

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> ObterHistorico(string deviceId, string atributo)
        {
            var urlBase = _configuration["Fiware:Url"];
            var portaSTH = _configuration["Fiware:PortSTH"] ?? "8666";

            var endpoint = $"http://{urlBase}:{portaSTH}/STH/v1/contextEntities/type/Logger/id/urn:ngsi-ld:Logger:{deviceId}/attributes/{atributo}?lastN=12";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("fiware-service", _configuration["Fiware:Service"]);
            _httpClient.DefaultRequestHeaders.Add("fiware-servicepath", _configuration["Fiware:ServicePath"]);

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                return "[]";
            }
            catch
            {
                return "[]";
            }
        }
    }
}