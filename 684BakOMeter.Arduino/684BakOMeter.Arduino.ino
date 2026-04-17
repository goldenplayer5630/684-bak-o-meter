#include <Arduino.h>
#include <SPI.h>
#include <MFRC522.h>
#include "HX711.h"

// -----------------------------
// RC522 pins
// -----------------------------
constexpr uint8_t RFID_SS_PIN  = 10; // SDA / SS
constexpr uint8_t RFID_RST_PIN = 9;

// -----------------------------
// HX711 pins
// -----------------------------
constexpr uint8_t HX1_DOUT_PIN = 2;
constexpr uint8_t HX1_SCK_PIN  = 3;

constexpr uint8_t HX2_DOUT_PIN = 4;
constexpr uint8_t HX2_SCK_PIN  = 5;

// -----------------------------
// Objects
// -----------------------------
MFRC522 rfid(RFID_SS_PIN, RFID_RST_PIN);
HX711 scale1;
HX711 scale2;

// -----------------------------
// Calibration settings
// These are placeholders.
// Flip the sign if weight moves negative.
// -----------------------------
float scale1CalibrationFactor = 420.0f;
float scale2CalibrationFactor = 420.0f;

// -----------------------------
// Reporting settings
// Faster for chug meter use
// -----------------------------
unsigned long lastScaleReadMs = 0;
constexpr unsigned long SCALE_READ_INTERVAL_MS = 20;

// Keep averaging minimal for speed
constexpr uint8_t SCALE_READ_SAMPLES = 1;

// Snap tiny drift to zero
constexpr float ZERO_DEADBAND = 2.0f;

// -----------------------------
// RFID debounce
// -----------------------------
unsigned long lastTagSeenMs = 0;
String lastTagUid = "";
constexpr unsigned long TAG_DEBOUNCE_MS = 1500;

// -----------------------------
// Scale state tracking
// -----------------------------
bool scale1WasReady = false;
bool scale2WasReady = false;

// -----------------------------
// Function declarations
// -----------------------------
String readRfidUid();
void handleRfid();
void handleScales();
void handleSerialCommands();
void printScaleValue(const char* label, float value);
void tareScales();
float normalizeNearZero(float value);

void setup()
{
    Serial.begin(115200);

    Serial.println("SYSTEM:BOOT");

    // RFID init
    SPI.begin();
    rfid.PCD_Init();
    delay(50);
    Serial.println("SYSTEM:RFID_READY");

    // HX711 init
    scale1.begin(HX1_DOUT_PIN, HX1_SCK_PIN);
    scale2.begin(HX2_DOUT_PIN, HX2_SCK_PIN);

    if (scale1.is_ready()) {
        scale1.set_scale(scale1CalibrationFactor);
        scale1WasReady = true;
        Serial.println("SYSTEM:SCALE1_READY");
    } else {
        scale1WasReady = false;
        Serial.println("ERROR:SCALE1_NOT_READY");
    }

    if (scale2.is_ready()) {
        scale2.set_scale(scale2CalibrationFactor);
        scale2WasReady = true;
        Serial.println("SYSTEM:SCALE2_READY");
    } else {
        scale2WasReady = false;
        Serial.println("ERROR:SCALE2_NOT_READY");
    }

    // Short settle, not too long
    delay(250);
    tareScales();

    Serial.println("SYSTEM:READY");
}

void loop()
{
    handleSerialCommands();
    handleRfid();
    handleScales();
}

void tareScales()
{
    Serial.println("SYSTEM:TARING");

    if (scale1.is_ready()) {
        scale1.tare(5);
        Serial.println("SYSTEM:SCALE1_TARED");
    } else {
        Serial.println("ERROR:SCALE1_TARE_FAILED");
    }

    if (scale2.is_ready()) {
        scale2.tare(5);
        Serial.println("SYSTEM:SCALE2_TARED");
    } else {
        Serial.println("ERROR:SCALE2_TARE_FAILED");
    }
}

void handleSerialCommands()
{
    if (!Serial.available()) {
        return;
    }

    String command = Serial.readStringUntil('\n');
    command.trim();
    command.toUpperCase();

    if (command == "TARE") {
        tareScales();
    }
}

void handleRfid()
{
    if (!rfid.PICC_IsNewCardPresent()) {
        return;
    }

    if (!rfid.PICC_ReadCardSerial()) {
        return;
    }

    String uid = readRfidUid();
    unsigned long now = millis();

    bool isSameRecentTag =
        (uid == lastTagUid) &&
        ((now - lastTagSeenMs) < TAG_DEBOUNCE_MS);

    if (!isSameRecentTag) {
        Serial.print("RFID:");
        Serial.println(uid);

        lastTagUid = uid;
        lastTagSeenMs = now;
    }

    rfid.PICC_HaltA();
    rfid.PCD_StopCrypto1();
}

String readRfidUid()
{
    String uid = "";

    for (byte i = 0; i < rfid.uid.size; i++) {
        if (rfid.uid.uidByte[i] < 0x10) {
            uid += "0";
        }

        uid += String(rfid.uid.uidByte[i], HEX);
    }

    uid.toUpperCase();
    return uid;
}

void handleScales()
{
    unsigned long now = millis();

    if ((now - lastScaleReadMs) < SCALE_READ_INTERVAL_MS) {
        return;
    }

    lastScaleReadMs = now;

    // -----------------------------
    // Scale 1
    // -----------------------------
    bool scale1Ready = scale1.is_ready();

    if (scale1Ready) {
        if (!scale1WasReady) {
            Serial.println("SYSTEM:SCALE1_RECOVERED");
        }

        float value1 = scale1.get_units(SCALE_READ_SAMPLES);
        value1 = normalizeNearZero(value1);
        printScaleValue("SCALE1", value1);
    } else if (scale1WasReady) {
        Serial.println("ERROR:SCALE1_NOT_READY");
    }

    scale1WasReady = scale1Ready;

    // -----------------------------
    // Scale 2
    // -----------------------------
    bool scale2Ready = scale2.is_ready();

    if (scale2Ready) {
        if (!scale2WasReady) {
            Serial.println("SYSTEM:SCALE2_RECOVERED");
        }

        float value2 = scale2.get_units(SCALE_READ_SAMPLES);
        value2 = normalizeNearZero(value2);
        printScaleValue("SCALE2", value2);
    } else if (scale2WasReady) {
        Serial.println("ERROR:SCALE2_NOT_READY");
    }

    scale2WasReady = scale2Ready;
}

float normalizeNearZero(float value)
{
    if (value > -ZERO_DEADBAND && value < ZERO_DEADBAND) {
        return 0.0f;
    }

    return value;
}

void printScaleValue(const char* label, float value)
{
    Serial.print(label);
    Serial.print(":");
    Serial.println(value, 2);
}