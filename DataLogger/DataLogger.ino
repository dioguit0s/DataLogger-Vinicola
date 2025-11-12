//Autor: Macaco Engenheiro
//Resumo: Esse programa faz a funcao de um dataLogger de ambiente enviando informacoes de status de temp, hum, e luminosidade para o Broker MQTT
//possibilitando controle via sistemas externos
#include <DHT.h>
#include <WiFi.h>
#include <PubSubClient.h>
#define DHT11_PIN 21

DHT dht11 (DHT11_PIN, DHT11);

// Configurações - variáveis editáveis
const char* default_SSID = "Diogo";                       // Nome da rede Wi-Fi
const char* default_PASSWORD = "12345678";                              // Senha da rede Wi-Fi
const char* default_BROKER_MQTT = "54.221.149.162";                // IP do Broker MQTT
const int default_BROKER_PORT = 1883;                           // Porta do Broker MQTT
const char* default_TOPICO_SUBSCRIBE = "/TEF/logger001/cmd";      // Tópico MQTT de escuta
const char* default_TOPICO_PUBLISH_1 = "/TEF/logger001/attrs";    // Tópico MQTT de envio de informações para Broker
const char* default_TOPICO_PUBLISH_2 = "/TEF/logger001/attrs/l,";  // Tópico MQTT de envio de informações para Broker de luminosidade
const char* default_TOPICO_PUBLISH_3 = "/TEF/logger001/attrs/t";  // Tópico MQTT de envio de informações para Broker de humidade
const char* default_TOPICO_PUBLISH_4 = "/TEF/logger001/attrs/h";  // Tópico MQTT de envio de informações para Broker de temperatura

const char* default_ID_MQTT = "fiware_001";                     // ID MQTT
const int default_D4 = 2;                                       // Pino do LED onboard
// Declaração da variável para o prefixo do tópico
const char* topicPrefix = "logger001";

// Variáveis para configurações editáveis
char* SSID = const_cast<char*>(default_SSID);
char* PASSWORD = const_cast<char*>(default_PASSWORD);
char* BROKER_MQTT = const_cast<char*>(default_BROKER_MQTT);
int BROKER_PORT = default_BROKER_PORT;
char* TOPICO_SUBSCRIBE = const_cast<char*>(default_TOPICO_SUBSCRIBE);
char* TOPICO_PUBLISH_1 = const_cast<char*>(default_TOPICO_PUBLISH_1);
char* TOPICO_PUBLISH_2 = const_cast<char*>(default_TOPICO_PUBLISH_2);
char* TOPICO_PUBLISH_3 = const_cast<char*>(default_TOPICO_PUBLISH_3);
char* TOPICO_PUBLISH_4 = const_cast<char*>(default_TOPICO_PUBLISH_4);
char* ID_MQTT = const_cast<char*>(default_ID_MQTT);
int D4 = default_D4;

WiFiClient espClient;
PubSubClient MQTT(espClient);
char EstadoSaida = '0';



void initSerial() {
  Serial.begin(115200);
}

void initWiFi() {
  delay(10);
  Serial.println("------Conexao WI-FI------");
  Serial.print("Conectando-se na rede: ");
  Serial.println(SSID);
  Serial.println("Aguarde");
  reconectWiFi();
}

void initMQTT() {
  MQTT.setServer(BROKER_MQTT, BROKER_PORT);
  MQTT.setCallback(mqtt_callback);
}

void setup() {
  dht11.begin();//inicializar o sensor dht
  InitOutput();
  initSerial();
  initWiFi();
  initMQTT();
  delay(5000);
  MQTT.publish(TOPICO_PUBLISH_1, "s|on");
}

void loop() {
  VerificaConexoesWiFIEMQTT();
  EnviaEstadoOutputMQTT();
  handleLuminosity();
  handleTempAndHum();
  MQTT.loop();
}

void reconectWiFi() {
  if (WiFi.status() == WL_CONNECTED)
    return;
  WiFi.begin(SSID, PASSWORD);
  while (WiFi.status() != WL_CONNECTED) {
    delay(100);
    Serial.print(".");
  }
  Serial.println();
  Serial.println("Conectado com sucesso na rede ");
  Serial.print(SSID);
  Serial.println("IP obtido: ");
  Serial.println(WiFi.localIP());

  // Garantir que o LED inicie desligado
  digitalWrite(D4, LOW);
}

void mqtt_callback(char* topic, byte* payload, unsigned int length) {
  String msg;
  for (int i = 0; i < length; i++) {
    char c = (char)payload[i];
    msg += c;
  }
  Serial.print("- Mensagem recebida: ");
  Serial.println(msg);

  // Forma o padrão de tópico para comparação
  String onTopic = String(topicPrefix) + "@on|";
  String offTopic = String(topicPrefix) + "@off|";

  // Compara com o tópico recebido
  if (msg.equals(onTopic)) {
    digitalWrite(D4, HIGH);
    EstadoSaida = '1';
  }

  if (msg.equals(offTopic)) {
    digitalWrite(D4, LOW);
    EstadoSaida = '0';
  }
}

void VerificaConexoesWiFIEMQTT() {
  if (!MQTT.connected())
    reconnectMQTT();
  reconectWiFi();
}

void EnviaEstadoOutputMQTT() {
  if (EstadoSaida == '1') {
    MQTT.publish(TOPICO_PUBLISH_1, "s|on");
    Serial.println("- Led Ligado");
  }

  if (EstadoSaida == '0') {
    MQTT.publish(TOPICO_PUBLISH_1, "s|off");
    Serial.println("- Led Desligado");
  }
  Serial.println("- Estado do LED onboard enviado ao broker!");
  delay(1000);
}

void InitOutput() {
  pinMode(D4, OUTPUT);
  digitalWrite(D4, HIGH);
  boolean toggle = false;

  for (int i = 0; i <= 10; i++) {
    toggle = !toggle;
    digitalWrite(D4, toggle);
    delay(200);
  }
}

void reconnectMQTT() {
  while (!MQTT.connected()) {
    Serial.print("* Tentando se conectar ao Broker MQTT: ");
    Serial.println(BROKER_MQTT);
    if (MQTT.connect(ID_MQTT)) {
      Serial.println("Conectado com sucesso ao broker MQTT!");
      MQTT.subscribe(TOPICO_SUBSCRIBE);
    } else {
      Serial.println("Falha ao reconectar no broker.");
      Serial.println("Haverá nova tentativa de conexão em 2s");
      delay(2000);
    }
  }
}

void handleTempAndHum() {
  float hum = dht11.readHumidity();
  float temp = dht11.readTemperature();

  Serial.print("Valor de temperatura: ");
  String mensagemTemp = String(temp);
  Serial.println(mensagemTemp);
  Serial.print("Valor de humidade: ");
  String mensagemHum = String(hum);
  Serial.println(mensagemHum);
  MQTT.publish(TOPICO_PUBLISH_3, mensagemTemp.c_str());
  MQTT.publish(TOPICO_PUBLISH_4, mensagemHum.c_str());
}

void handleLuminosity() {
  const int potPin = 35;
  int sensorValue = analogRead(potPin);
  int luminosity = map(sensorValue, 0, 4095, 0, 100);
  String mensagem = String(luminosity);
  Serial.print("Valor da luminosidade: ");
  Serial.println(mensagem.c_str());
  MQTT.publish(TOPICO_PUBLISH_2, mensagem.c_str());
}
