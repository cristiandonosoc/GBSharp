#define CLK_PIN = 4
#define SIN_PIN = 5
#define SOUT_PIN = 6
#define LED_PIN = 13

#define LD_LOW = 0x10
#define LD_HIGH = 0x20
#define BYTE_REQ = 0x40

int incomingByte = 0;
byte bitCount = 0;
byte emulatorBuffer = 0;
byte gameboyBuffer = 0;

void setup() {
  pinMode(CLK_PIN, INPUT);
  pinMode(SIN_PIN, INPUT);
  pinMode(SOUT_PIN, OUTPUT);
  pinMode(LED_PIN, OUTPUT);
  attachInterrupt(digitalPinToInterrupt(CLK_PIN), handleGameboyClock, CHANGE);
  Serial.begin(115200);
}

void loop() {
  if (Serial.available() > 0) {
    // read the incoming byte:
    incomingByte = Serial.read();

    if ( incomingByte & LD_LOW == LD_LOW) {
      emulatorBuffer = (emulatorBuffer & 0xF0) | (incomingByte & 0x0F);
    } else if ( incomingByte & LD_HIGH == LD_HIGH) {
      emulatorBuffer = (incomingByte & 0xF0) | (emulatorBuffer & 0x0F);
    } else if ( incomingByte & BYTE_REQ == BYTE_REQ) {
      pinMode(CLK_PIN, OUTPUT);
      digitalWrite(LED_PIN, HIGH);

      while(bitCount < 8) {
        digitalWrite(SOUT_PIN, emulatorBuffer & 0x80 == 0x80 ? HIGH : LOW);
        digitalWrite(CLK_PIN, HIGH);
        delayMicroseconds(60); // ~8khz
        digitalWrite(CLK_PIN, LOW);
        delayMicroseconds(60); // ~8khz
        emulatorBuffer <<= 1;
        ++bitCount;
      }

      bitCount = 0;
      digitalWrite(LED_PIN, LOW);
      pinMode(CLK_PIN, INPUT);
    }
  }

  digitalWrite(SOUT_PIN, emulatorBuffer & 0x80 == 0x80 ? HIGH : LOW);
}

// What happens when
void handleGameboyClock() {
  // Receive
  gameboyBuffer <<= 1;
  gameboyBuffer = (gameboyBuffer & 0xFE) | (digitalRead(SIN_PIN));
  // Send
  emulatorBuffer <<= 1;
  digitalWrite(SOUT_PIN, emulatorBuffer & 0x80 == 0x80 ? HIGH : LOW);

  ++bitCount;
  if(bitCount == 4) {
    Serial.write(LD_HIGH | (0x0F & gameboyBuffer));
  } else if(bitCount == 8) {
    Serial.write(LD_LOW | (0x0F & gameboyBuffer));
    bitCount = 0;
  }
}
