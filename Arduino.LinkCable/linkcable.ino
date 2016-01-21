/* Pin definitions
Pin 2 and 3 are the only ones that accept interrupts
on every arduino version so it will be used as clock pin */
#define CLK_PIN = 2
#define SIN_PIN = 4
#define SOUT_PIN = 5
#define LED_PIN = 13

/* Emulator communication protcol codes */
#define LD_LOW = 0x10
#define LD_HIGH = 0x20
#define BYTE_REQ = 0x40

/* To make sure variables shared between an ISR
 and the main program are updated correctly, We
 need to declare them as volatile */
// Serial
int incomingByte = 0;
// Tick counters
volatile byte externalClockBitCount = 0;
byte ownClockBitCount = 0;
// Serial buffers (shift registers)
volatile byte emulatorBuffer = 0;
byte nextEmulatorBuffer = 0; // We need this since emulator is out of sync
volatile byte gameboyBuffer = 0;
// Who is the master clock?
volatile bool ownClock = false;
// Is there new data in the incoming shift register?
volatile byte emulatorUpdateRequired = false;

// Hardware initialization
void setup() {
  pinMode(CLK_PIN, INPUT);
  pinMode(SIN_PIN, INPUT);
  pinMode(SOUT_PIN, OUTPUT);
  pinMode(LED_PIN, OUTPUT);
  attachInterrupt(digitalPinToInterrupt(CLK_PIN), handleGameboyClock, CHANGE);
  Serial.begin(115200);
}

// Main loop, runs forever
void loop() {
  // Process data received in the handleGameboyClock ISR
  if (emulatorUpdateRequired) {
    if (externalClockBitCount < 8) {
      // Send high
      Serial.write(
        // Minimize the chances of a race condition where an extra bit has
        // arrived (i.e: externalClockBitCount = 5) at the time we write to
        // the serial port.
        LD_HIGH | (0x0F & (gameboyBuffer >> (externalClockBitCount - 4)))
      );
    } else {
      Serial.write(LD_LOW | (0x0F & gameboyBuffer));
      externalClockBitCount = 0;
    }
    emulatorUpdateRequired = false;
  }

  // Receive commands from the emulator
  if (Serial.available() > 0) {
    // read the incoming byte:
    incomingByte = Serial.read();

    // Load serial buffer (low)?
    if ( incomingByte & LD_LOW == LD_LOW) {
      nextEmulatorBuffer = (nextEmulatorBuffer & 0xF0) | (incomingByte & 0x0F);
    // Load serial buffer (high)?
    } else if ( incomingByte & LD_HIGH == LD_HIGH) {
      nextEmulatorBuffer = (incomingByte & 0xF0) | (nextEmulatorBuffer & 0x0F);
    // Request a byte, with master clock in the emulator side
    } else if ( incomingByte & BYTE_REQ == BYTE_REQ) {
      simulateClockAndExchangeByte();
    }
  }
}

// Simulates 8 ticks of a clock in the emulator side to the real gameboy,
// swaping the bytes between their serial buffers (shift registers).
// Updates the emulator buffer when the operation is completed.
void simulateClockAndExchangeByte() {
  // Take control of the clock pin
  ownClock = true; // Same pin, disable the ISR for external clock
  pinMode(CLK_PIN, OUTPUT);
  digitalWrite(CLK_PIN, HIGH);
  emulatorBuffer = nextEmulatorBuffer;

  while(ownClockBitCount < 8) {
    // Put the MSB of the serial buffer into the serial out pin
    digitalWrite(SOUT_PIN, emulatorBuffer & 0x80 == 0x80 ? HIGH : LOW);

    // 8192hz = 122 microseconds per cycle
    delayMicroseconds(60);
    digitalWrite(CLK_PIN, LOW);
    gameboyBuffer <<= 1;
    gameboyBuffer |= digitalRead(SIN_PIN) == HIGH ? 0x01 : 0x00;
    delayMicroseconds(60);
    digitalWrite(CLK_PIN, HIGH);

    // Prepare our shift register for the next write
    emulatorBuffer <<= 1;
    ++ownClockBitCount;

    // Update emulator buffer
    if (ownClockBitCount == 4) {
      // First 4 lower bits are the upper bits being shifted into the register
      // 0000uuuu <---
      Serial.write(LD_HIGH | (0x0F & emulatorBuffer));
    } else if (ownClockBitCount == 8) {
      // uuuullll <---
      Serial.write(LD_LOW | (0x0F & emulatorBuffer));
    }
  }

  // Listen again the external clock
  ownClockBitCount = 0;
  pinMode(CLK_PIN, INPUT);
  ownClock = false;
}

// What happens when the clock pin ticks
void handleGameboyClock() {
  if (owmClock) {
    return; // We are causing this interrupt
  }
  // else: Master clock tick from the real gameboy side.

  if (digitalRead(CLK_PIN) == HIGH) {
    return; // Nothing to do on rising edge
  }
  // else: It's a falling edge, read SIN, update SOUT

  // Receive
  gameboyBuffer <<= 1;
  gameboyBuffer = (gameboyBuffer & 0xFE) | (digitalRead(SIN_PIN) & 0x01);
  // Update
  emulatorBuffer <<= 1;
  digitalWrite(SOUT_PIN, emulatorBuffer & 0x80 == 0x80 ? HIGH : LOW);

  ++externalClockBitCount;
  if(externalClockBitCount == 4 || externalClockBitCount == 8) {
    // We should not write to the serial port from an ISR
    // so this variable will be checked for changes outside
    emulatorUpdateRequired = true
  }
}
