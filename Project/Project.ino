// DEĞERLERİ TANIMLAMA BÖLÜMÜ
#include "NewPing.h"  // distance measuring sensor library
#include <DHT.h>      //  temperature and humidity measuring sensor library
#include <stdlib.h>  //  string library converts float to string


#define TRIGGER_PIN 5 // ultrasonik sensörden çıkış pini, uno'da 5.pine bağlanır
#define ECHO_PIN 6   // ultrasonik sensörden çıkış pini, uno'da 6.pine bağlanır
#define MAX_DISTANCE 400 // ultrasonik sensör en fazla 4m'ye kadar ölçer
#define DHT21_PIN 3  // ısı ve nem sensöründen çıkış pini, uno'da 3.pine bağlanır
#define TYPE DHT11  

#define SSID "iPhone"      // "SSID-WiFiname"
#define PASS "bm1bm1bm"       // "password"

#define IP "184.106.153.149"// thingspeak.com ip, uno wifi modülü ile thingspeak.com ip'ye bağlanır


const int redLedPin = 13; // kırmızı led uno'da 13.pine bağlanır
const int yellowLedPin = 12; // yellow led uno'da 12.pine bağlanır
const int inputPin = 2; // hareket sensörü sinyal çıkış pini, uno'da 2.pine bağlanır
const int pinSpeaker = 7; //buzzer çıkış pini, uno'da 7.pine bağlanır

const int AOUTpin = A3; // Hava kalitesi Analog çıkış pini, uno'da A3 pinine bağlanır
const int DOUTpin = 8; //  Hava kalitesi Dijital çıkış pini, uno'da 8.pine bağlanır

DHT dht(DHT21_PIN, TYPE); // kütüphaneden değişken oluşturma
NewPing sonar(TRIGGER_PIN, ECHO_PIN, MAX_DISTANCE);

int val; // hareket sensöründen okunan değer
int state = LOW; // hareket olmadığında durum=0 olacak
int esik;  // Eşik değeri
int ppm; //hava kalitesinden okunacak değer
int error; // thinkSpeak'e gönderilemeyen paket hata sayacı

float duration; //duration: buzzer çalma süresi ayarlama
float distance; //distance: sensörden alınan mesafe değeri
float humidity; //sensörden okunan nem değeri
float temp_cel; //sensörden okunan C sıcaklık değeri
float temp_farh; //sensörden okunan Fahr sıcaklık değeri
long paketId; //uno'dan wifi modülü ile thinkSpeak'e giden paket sayacı (ThinkSpeak;cloud db) 
long errorPaketId; //gidemeyen paketId


String msg = "GET /update?api_key=C0G5MYN7BPNXGGI9"; //thinkSpeak'te oluşturulan, sensör verilerinin kaydedildiği kanala giden Ip
String tempC; // okunan float C değeri stringe çevirir çünkü değerler thinkSpeak'e string olarak gönderilmelidir

// DEĞERLERİ ÇALIŞTIRMA BÖLÜMÜ
void setup() {
  paketId = 0; 
  errorPaketId = 0;
  pinMode(9, OUTPUT);// uno'da 9 pininden güç(5V) çıkışı sağlanır

  pinMode(redLedPin, OUTPUT); // uno'dan güç çıkışı sağlanır ve kırmızı led yanar
  pinMode(yellowLedPin, OUTPUT); //led pin çıkış tanımlama ve sarı led yanar
  pinMode(inputPin, INPUT); // hareket sensör pini giriş tanımlama,uno'ya veri girişi sağlanır
  pinMode(pinSpeaker, OUTPUT); //uno'da 5V güç çıkışı sağlanır ve buzzer çalışır
  pinMode(DOUTpin, INPUT); //hava kalitesi eşik değerinin okunduğu pin


  Serial.begin(115200); // wifi modülünü çalışması için baund değeri ayarlama
  Serial.println("AT"); //wifi modülüne bağlanma komut seti
  delay(3000); //wifi modülüne bağlanmak için gereken bağlanma süresi

  if (Serial.find("OK")) { //wifi modülünden OK geldiğinde
    Serial.println("Set"); //Wifi'ye bağlanır
    connectWiFi();
  } else
  {
    Serial.println("not set");
  }

  //Start
  dht.begin(); //ısı-nem değerlerini okumaya başla
}
//SÜREKLİ ÇALIŞTIRMA BÖLÜMÜ
void loop() {
  error = 0; // hata sayacı 0'lanır ve saymaya başlar
  paketId += 1; //Paketin işlenir
  Serial.println("Package:" + String(paketId) + " processing");
  digitalWrite(redLedPin, HIGH); // loop  başladığında led söner
  digitalWrite(yellowLedPin, HIGH); // loop başladığında led söner

  //delay(1000);
  val = digitalRead(inputPin); // hareket sensörü değeri okumaya başlar (1 ya da 0)
  Serial.println("M=" + String(val)); // okuduğun değeri Seri Port'ta yazar
  
  humidity = dht.readHumidity(); // nem değeri okunur
  temp_cel = dht.readTemperature(); // sıcaklık değeri okunur in Clecius
  temp_farh = dht.readTemperature(true); //fahr değeri okunur
  char buffer[10]; //okunan sıcaklık değerini stringe çevirmek için kullanılan 10 karakterlik ara değişken
  if (isnan(humidity) || isnan(temp_cel) || isnan(temp_farh)) {
    //isnan = is NOT A NUMBER which return true when it is not a number, okunan ısı-nem değeri boş olup olmadığı kontrol edilir
    Serial.println("# Sorry, Failed to Read Data From DHT Module");
    return;
  }
  else {

    
    Serial.println("temprature:" + String(humidity) + "|humidity:" + String(temp_cel)); //değerler seri port'a gönderilir
    distance = sonar.ping_cm(); //mesafe değeri okunur
    ppm = analogRead(AOUTpin); // ppm değerini Analog pinden oku (hava kalitesi)
    esik = digitalRead(DOUTpin); // eşik değerinin aşılıp aşılmadığını oku
    Serial.println("Distance:" + String(distance) + "|Air Quality:" + String(ppm)); //mesafe ve hava kalitesini stringe çevirip seri port'a gönderir
    tempC = dtostrf(temp_cel, 4, 1, buffer); // sıcaklık stringe çevrilir
    Serial.println(String(val)); //hareket sensörünü stringe çevirip seri port'a gönderir

  

  }
  // sensörü oku value değerine at
  if (String(val) == "1") { // eğer value değeri =1 ise(hareket var ise)
    digitalWrite(redLedPin, LOW); //kırmızı LED i yak
    digitalWrite(yellowLedPin, HIGH); //sarı LED i söndür

    playTone(4000, 10); // 4 sn boyunca , 10 frekans ile buzzer'ı çal

  }
  else if (String(val) == "0") { //eğer value değeri=0 ise (hareket yok)
    digitalWrite(redLedPin, HIGH); //kırmızı LED i söndür
    digitalWrite(yellowLedPin, LOW); //sare LED i yak
  }


  sendThingSpeakValues(); //bütün değerleri thinkSpeak'e gönderme işlemini başlat
//  if (error == 1) {
//
//  }
}

// THINKSPEAK'E GONDERME BÖLÜMÜ

void sendThingSpeakValues() {
  String cmd = "AT+CIPSTART=\"TCP\",\"";
  cmd += IP;
  cmd += "\",80";
  Serial.println(cmd);
  //delay(2000);
  if (Serial.find("Error")) {
    return;
  }
  cmd = msg ;
  cmd += "&field1=";     //field 1 for temperature
  cmd += tempC;
  cmd += "&field2=";  //field 2 for humidity
  cmd += String(humidity);
  cmd += "&field3=";  //field 3 for FIR
  cmd += String(val);
  cmd += "&field4=";  //field 4 for distance
  cmd += String(distance);
  cmd += "&field5=";  //field 5 for
  cmd += String(ppm);
  cmd += "&field6=";  //field 6 for thrashold
  cmd += String(esik);
  cmd += "\r\n";
  Serial.print("AT+CIPSEND=");
  Serial.println(cmd.length());
  if (Serial.find(">")) {
    Serial.print(cmd);
    error = 0;
    Serial.println("Send OK:" + String(paketId));

  }
  else {
    Serial.println("AT+CIPCLOSE");
    Serial.println("Error: " + String(paketId));

    //Resend...
    error = 1;

  }
}

//BUZZER ÇALMA
void playTone(long duration, int freq) { 
  duration *= 1000;
  int period = (1.0 / freq) * 1000000;
  long elapsed_time = 0;
  while (elapsed_time < duration) {
    digitalWrite(pinSpeaker, HIGH);
    delayMicroseconds(period / 2);
    digitalWrite(pinSpeaker, LOW);
    delayMicroseconds(period / 2);
    elapsed_time += (period);
  }
}

//WİFİ'YE BAĞLANMA
boolean connectWiFi() {
  Serial.println("AT+CWMODE=1");
  delay(2000);
  String cmd = "AT+CWJAP=\"";
  cmd += SSID;
  cmd += "\",\"";
  cmd += PASS;
  cmd += "\"";
  Serial.println(cmd);
  delay(6000);
  if (Serial.find("OK")) {
    Serial.println("connected");
    return true;
  } else {
    return false;
  }
}
