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
const char* default_TOPICO_SUBSCRIBE = "/TEF/lamp001/cmd";      // Tópico MQTT de escuta
const char* default_TOPICO_PUBLISH_1 = "/TEF/lamp001/attrs";    // Tópico MQTT de envio de informações para Broker
const char* default_TOPICO_PUBLISH_2 = "/TEF/lamp001/attrs/l";  // Tópico MQTT de envio de informações para Broker
const char* default_ID_MQTT = "fiware_001";                     // ID MQTT

// Definição dos pinos do LED RGB
const int PIN_LED_RED = 12;
const int PIN_LED_GREEN = 13;
const int PIN_LED_BLUE = 14;

// Declaração da variável para o prefixo do tópico
const char* topicPrefix = "lamp001";

// Variáveis para configurações editáveis
char* SSID = const_cast<char*>(default_SSID);
char* PASSWORD = const_cast<char*>(default_PASSWORD);
char* BROKER_MQTT = const_cast<char*>(default_BROKER_MQTT);
int BROKER_PORT = default_BROKER_PORT;
char* TOPICO_SUBSCRIBE = const_cast<char*>(default_TOPICO_SUBSCRIBE);
char* TOPICO_PUBLISH_1 = const_cast<char*>(default_TOPICO_PUBLISH_1);
char* TOPICO_PUBLISH_2 = const_cast<char*>(default_TOPICO_PUBLISH_2);
char* ID_MQTT = const_cast<char*>(default_ID_MQTT);

WiFiClient espClient;
PubSubClient MQTT(espClient);
char EstadoSaida = '0';

void setRGB(int red, int green, int blue) {
  digitalWrite(PIN_LED_RED, red);
  digitalWrite(PIN_LED_GREEN, green);
  digitalWrite(PIN_LED_BLUE, blue);
}


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
  initRGB();
  initOutput();
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
  Serial.println("TESTWEEEEEEEEEEEEEEEEEEE");
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
  setRGB(LOW, LOW, LOW);
}

void mqtt_callback(char* topic, byte* payload, unsigned int length) {
  String msg;
  for (int i = 0; i < length; i++) { [cite: 18]
    char c = (char)payload[i];
    msg += c; [cite: 19]
  }
  Serial.print("- Mensagem recebida: ");
  Serial.println(msg);

  // Lógica para os 4 estados do LED RGB
  // Seu backend deve enviar EXATAMENTE estas strings.
  
  if (msg.equals("TEMP_ALARM")) {
    // Estado 1: Temperatura fora dos limites - VERMELHO
    Serial.println(">>> ALARME: Temperatura. Ligando LED Vermelho.");
    setRGB(HIGH, LOW, LOW); // Vermelho
    
  } else if (msg.equals("HUM_ALARM")) {
    // Estado 2: Umidade fora dos limites - AZUL
    Serial.println(">>> ALARME: Umidade. Ligando LED Azul.");
    setRGB(LOW, LOW, HIGH); // Azul
    
  } else if (msg.equals("LUM_ALARM")) {
    // Estado 3: Luminosidade fora dos limites - AMARELO
    Serial.println(">>> ALARME: Luminosidade. Ligando LED Amarelo.");
    setRGB(HIGH, HIGH, LOW); // Amarelo (Vermelho + Verde)
    
  } else if (msg.equals("OFF") || msg.equals("NORMAL")) {
    // Estado 4: LED Desligado (Tudo normal)
    Serial.println(">>> ESTADO: Normal. Desligando LED.");
    setRGB(LOW, LOW, LOW); // Desligado
    
  } else {
    // Mensagem não reconhecida (pode ser as antigas 'lamp001@on|')
    Serial.println("Mensagem MQTT não reconhecida para controle do LED.");
  }
}

void VerificaConexoesWiFIEMQTT() {
  if (!MQTT.connected())
    reconnectMQTT();
  reconectWiFi();
}

//  void EnviaEstadoOutputMQTT() {
//    if (EstadoSaida == '1') {
//      MQTT.publish(TOPICO_PUBLISH_1, "s|on");
//      Serial.println("- Led Ligado");
//    }
//  
//    if (EstadoSaida == '0') {
//      MQTT.publish(TOPICO_PUBLISH_1, "s|off");
//      Serial.println("- Led Desligado");
//    }
//    Serial.println("- Estado do LED onboard enviado ao broker!");
//    delay(1000);
//  }

void initRGB() {
  pinMode(PIN_LED_RED, OUTPUT);
  pinMode(PIN_LED_GREEN, OUTPUT);
  pinMode(PIN_LED_BLUE, OUTPUT);
  
  // Garante que o LED comece desligado
  setRGB(LOW, LOW, LOW);
  Serial.println("LED RGB inicializado como DESLIGADO.");
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
  Serial.print(temp);
  Serial.print("Valor de humidade: ");
  Serial.print(hum);

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
