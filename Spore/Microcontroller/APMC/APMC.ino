#include <Ethernet.h>
#include <EthernetUdp.h>
#include <Arduino_MachineControl.h>
#include "Wire.h"
using namespace machinecontrol;

enum MainCommand {
  Get,
  Set,
};

enum SubCommand {
  Analog,
  Digital,
};

const int NUM_PINS = 76; // 76 pins total
int SerialVals[NUM_PINS];
// 1 (byte) * 60 digital + 4 (int) * 16 analog = 124
// 124 + start (byte) + end (byte) + command (byte) = 127
const int UDP_PACKET_SIZE = 127;
const int NULL_PLACEHOLDER = 250;
byte Packet[UDP_PACKET_SIZE];

IPAddress ip(169, 254, 198, 3);
unsigned int localPort = 142;

EthernetUDP Udp;

// For Analog Input voltage divider
float ResDivider = 0.28057;
float Reference = 3.3;

void setup() {
  Serial.begin(9600);
  // Start UDP
  Ethernet.begin(ip);
  Udp.begin(localPort);
  IPAddress ip = Ethernet.localIP();
  Serial.print("IP Address: ");
  Serial.println(ip);
  // Setup APMC
  Wire.begin();
  // J3: Digital Inputs
  digital_inputs.init();
  // J6: Digital Outputs
  digital_outputs.setLatch();
  digital_outputs.setAll(0);
  // J8: Programmable Digital I/O
  digital_programmables.init();
  digital_programmables.setLatch();
  // J11: Analog Out
  //For reference: analog_out.period_ms(CHANNEL, PERIOD_MILLISECONDS);
  analog_out.period_ms(0, 4);
  analog_out.period_ms(1, 4);
  analog_out.period_ms(2, 4);
  analog_out.period_ms(3, 4);
  // J9: Analog In
  analogReadResolution(16);
  analog_in.set0_10V();
}

void loop() {
  // If there's data available, read a packet
  int packetSize = Udp.parsePacket();
  if (packetSize == UDP_PACKET_SIZE) {
    // Read the packet into packetBufffer
    Udp.read(Packet, UDP_PACKET_SIZE);
    // Carry out the command
    int cmd = int(Packet[1]);
    Execute(cmd);
    // Send a reply to the IP address if it is a read command
    if (cmd == Get) {
      Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
      Udp.write(Packet, UDP_PACKET_SIZE);
      Udp.endPacket();
    }
  }
  for (int i = 0; i < NUM_PINS; i++) {
    Serial.print(SerialVals[i]);
    Serial.print("\t");
  }
  Serial.println("");
  delay(10);
}

// Convert 4 bytes into an int
int BytesToInt(byte* valueTemp, byte startIndex) {
  int temp;
  byte intPart[4] = {
    valueTemp[startIndex],
    valueTemp[startIndex + 1],
    valueTemp[startIndex + 2],
    valueTemp[startIndex + 3],
  };
  memcpy(&temp, intPart, 4);
  return temp;
}

void Execute(byte cmd) {
  switch (cmd) {
    case Get:
      ConstructPacket();
      break;
    case Set:
      int subCmd = int(Packet[2]);
      int pin = BytesToInt(Packet, 3);
      int value = BytesToInt(Packet, 7);
      switch (subCmd) {
        case Analog:
          SetAnalogValue(pin, value);
          break;
        case Digital:
          SetDigitalValue(pin, value);
          break;
      }
      break;
  }
}

void ConstructPacket() {
  // The packet from C# indicates which pins to read
  // and what type of pin it is
  int i = 2;
  while (i < UDP_PACKET_SIZE - 1) {
    if (Packet[i] == NULL_PLACEHOLDER) {
      i++;
    } else if (Packet[i] == 1) {  // Read digital pin
      int pin = i - 2;            // Shift for start and cmd bytes
      Packet[i] = GetDigitalValue(pin);
      i++;
    } else {  // Read analog pin
      int reading = GetAnalogValue(Packet[i]);
      memcpy(Packet + i, &reading, 4);
      i += 4;
    }
  }
  // Change start and end bytes for C# to read
  Packet[0]--;
  Packet[UDP_PACKET_SIZE - 1]++;
}

int GetDigitalValue(int pin) {
  int val = 0;
  switch (pin) {
    case 0: val = digital_inputs.read(DIN_READ_CH_PIN_00); break;
    case 1: val = digital_inputs.read(DIN_READ_CH_PIN_01); break;
    case 2: val = digital_inputs.read(DIN_READ_CH_PIN_02); break;
    case 3: val = digital_inputs.read(DIN_READ_CH_PIN_03); break;
    case 4: val = digital_inputs.read(DIN_READ_CH_PIN_04); break;
    case 5: val = digital_inputs.read(DIN_READ_CH_PIN_05); break;
    case 6: val = digital_inputs.read(DIN_READ_CH_PIN_06); break;
    case 7: val = digital_inputs.read(DIN_READ_CH_PIN_07); break;
    case 16: val = digital_programmables.read(IO_READ_CH_PIN_00); break;
    case 17: val = digital_programmables.read(IO_READ_CH_PIN_01); break;
    case 18: val = digital_programmables.read(IO_READ_CH_PIN_02); break;
    case 19: val = digital_programmables.read(IO_READ_CH_PIN_03); break;
    case 20: val = digital_programmables.read(IO_READ_CH_PIN_04); break;
    case 21: val = digital_programmables.read(IO_READ_CH_PIN_05); break;
    case 22: val = digital_programmables.read(IO_READ_CH_PIN_06); break;
    case 23: val = digital_programmables.read(IO_READ_CH_PIN_07); break;
    case 24: val = digital_programmables.read(IO_READ_CH_PIN_08); break;
    case 25: val = digital_programmables.read(IO_READ_CH_PIN_09); break;
    case 26: val = digital_programmables.read(IO_READ_CH_PIN_10); break;
    case 27: val = digital_programmables.read(IO_READ_CH_PIN_11); break;
    default: break;
  }
  SerialVals[pin] = val;
  return val;
}

int GetAnalogValue(int pin) {
  int val = 0;
  switch (pin) {
    case 60:
      {
        float raw_voltage_ch0 = analog_in.read(0);
        float voltage_ch0 = (raw_voltage_ch0 * Reference) / 65535 / ResDivider;
        val = int(voltage_ch0);
        break;
      }
    case 61:
      {
        float raw_voltage_ch1 = analog_in.read(1);
        float voltage_ch1 = (raw_voltage_ch1 * Reference) / 65535 / ResDivider;
        val = int(voltage_ch1);
        break;
      }
    case 62:
      {
        float raw_voltage_ch2 = analog_in.read(2);
        float voltage_ch2 = (raw_voltage_ch2 * Reference) / 65535 / ResDivider;
        val = int(voltage_ch2); 
        break;     
      }
    default: break;
  }
  SerialVals[pin] = val;
  return val;
}

void SetDigitalValue(int pin, int value) {
  int val = 0;  // OPEN or LOW
  if (value == 1) {
    val = 255;  // CLOSED or HIGH
  }
  switch (pin) {
    case 8: digital_outputs.set(0, val); break;
    case 9: digital_outputs.set(1, val); break;
    case 10: digital_outputs.set(2, val); break;
    case 11: digital_outputs.set(3, val); break;
    case 12: digital_outputs.set(4, val); break;
    case 13: digital_outputs.set(5, val); break;
    case 14: digital_outputs.set(6, val); break;
    case 15: digital_outputs.set(7, val); break;
    case 16:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_00, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_00, SWITCH_OFF);
      }
      break;
    case 17:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_01, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_01, SWITCH_OFF);
      }
      break;
    case 18:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_02, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_02, SWITCH_OFF);
      }
      break;
    case 19:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_03, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_03, SWITCH_OFF);
      }
      break;
    case 20:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_04, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_04, SWITCH_OFF);
      }
      break;
    case 21:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_05, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_05, SWITCH_OFF);
      }
      break;
    case 22:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_06, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_06, SWITCH_OFF);
      }
      break;
    case 23:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_07, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_07, SWITCH_OFF);
      }
      break;
    case 24:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_08, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_08, SWITCH_OFF);
      }
      break;
    case 25:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_09, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_09, SWITCH_OFF);
      }
      break;
    case 26:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_10, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_10, SWITCH_OFF);
      }
      break;
    case 27:
      if (value == 1) {
        digital_programmables.set(IO_WRITE_CH_PIN_11, SWITCH_ON);
      } else {
        digital_programmables.set(IO_WRITE_CH_PIN_11, SWITCH_OFF);
      }
      break;
    default: break;
  }
  SerialVals[pin] = value;
}

void SetAnalogValue(int pin, int value) {
  switch (pin) {
    case 28: analog_out.write(0, value); break;
    case 29: analog_out.write(1, value); break;
    case 30: analog_out.write(2, value); break;
    case 31: analog_out.write(3, value); break;
    default: break;
  }
  SerialVals[pin] = value;
}