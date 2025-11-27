//Autor: Macaco Engenheiro
//Resumo: Esse programa faz a funcao de um dataLogger de ambiente enviando informacoes de status de temp, hum, e luminosidade para o Broker MQTT
//possibilitando controle via sistemas externos
#include <DHT.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <LiquidCrystal_I2C.h>
#define DHT11_PIN 25

DHT dht11 (DHT11_PIN, DHT11);

// Configurações - variáveis editáveis
const char* default_SSID = "digsbionic_2G";                       // Nome da rede Wi-Fi
const char* default_PASSWORD = "Diogos1239";                // Senha da rede Wi-Fi
const char* default_BROKER_MQTT = "54.242.94.244";       // IP do Broker MQTT
const int default_BROKER_PORT = 1883;                           // Porta do Broker MQTT
const char* default_TOPICO_SUBSCRIBE = "/TEF/logger007/cmd";      // Tópico MQTT de escuta
const char* default_TOPICO_PUBLISH_1 = "/TEF/logger007/attrs";    // Tópico MQTT de envio de informações para Broker
const char* default_TOPICO_PUBLISH_2 = "/TEF/logger007/attrs/l";  // Tópico MQTT de envio de informações para Broker de luminosidade
const char* default_TOPICO_PUBLISH_3 = "/TEF/logger007/attrs/t";  // Tópico MQTT de envio de informações para Broker de humidade
const char* default_TOPICO_PUBLISH_4 = "/TEF/logger007/attrs/h";  // Tópico MQTT de envio de informações para Broker de temperatura

const char* default_ID_MQTT = "fiware_001";                     // ID MQTT

// Definição dos pinos do LED RGB
const int PIN_LED_RED = 12;
const int PIN_LED_GREEN = 13;
const int PIN_LED_BLUE = 14;

//Definindo pino do buzzer
const int PIN_BUZZER = 4;

//Definindo pinos da tela
LiquidCrystal_I2C lcd(0x27, 16, 2);
//Ligar SCL no pino 22
//Ligar SDA no pino 21

// Declaração da variável para o prefixo do tópico
const char* topicPrefix = "logger007";

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
  initSerial();
  initWiFi();
  initMQTT();
  lcd.init();
  lcd.backlight();
  lcd.setCursor(0, 0);
  lcd.print("Inicializando...");
  delay(5000);
}

void loop() {
  VerificaConexoesWiFIEMQTT();
  writeSensorValues();
  handleLuminosity();
  handleTempAndHum();
  MQTT.loop();
  delay(5000);
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
  for (int i = 0; i < length; i++) {
    char c = (char)payload[i];
    msg += c;
  }

  Serial.print("- Mensagem recebida: ");
  Serial.println(msg);

  int startIndex = msg.indexOf('@');
  int endIndex = msg.indexOf('|');

  String message; 

  if (startIndex != -1 && endIndex != -1 && endIndex > startIndex) {
    message = msg.substring(startIndex + 1, endIndex);
  } else {
    Serial.println("- Formato de mensagem inválido.");
    message = "";
  }

  if (message.equals("TEMP_ALARM")) {
    // Estado 1: Temperatura fora dos limites - VERMELHO
    Serial.println(">>> ALARME: Temperatura. Ligando LED Vermelho.");
    setRGB(HIGH, LOW, LOW); // Vermelho
    tone(PIN_BUZZER, 2000); //liga o buzzer com 2000 de frequencia
    
  } else if (message.equals("HUM_ALARM")) {
    // Estado 2: Umidade fora dos limites - AZUL
    Serial.println(">>> ALARME: Umidade. Ligando LED Azul.");
    setRGB(LOW, LOW, HIGH); // Azul
    tone(PIN_BUZZER, 3000); //liga o buzzer com 3000 de frequencia

    
  } else if (message.equals("LUM_ALARM")) {
    // Estado 3: Luminosidade fora dos limites - AMARELO
    Serial.println(">>> ALARME: Luminosidade. Ligando LED Amarelo.");
    setRGB(HIGH, HIGH, LOW); // Amarelo (Vermelho + Verde)
    tone(PIN_BUZZER, 4000, 3000); //liga o buzzer com 4000 de frequencia
    
  } else if (message.equals("OFF") || message.equals("NORMAL")) {
    // Estado 4: LED Desligado (Tudo normal)
    Serial.println(">>> ESTADO: Normal. Desligando LED.");
    setRGB(LOW, LOW, LOW); // Desligado
    noTone(PIN_BUZZER);
    
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
      Serial.println("Enviando pacote de inicialização para criar entidade...");
      MQTT.publish(default_TOPICO_PUBLISH_1, "s|boot|t|0|h|0|l|0");
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

void writeStartMessage(){
  lcd.setCursor(0, 0);
  lcd.print("Inicializando...");
}

void writeSensorValues(){
  String lumValue = String(map(analogRead(35), 0, 4095, 0, 100));
  String humValue = String(dht11.readHumidity());
  String tempValue = String(dht11.readTemperature());

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Hum  Temp  Lum");
  lcd.setCursor(0, 1);
  lcd.print(humValue + "  " + tempValue + "   " + lumValue);
} 
