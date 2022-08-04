//CMI-201 arduino sketch program

String inputString = "";         // a String to hold incoming data
bool stringComplete = false;  // whether the string is complete

void setup() {
  // 7 Segment Modul
  pinMode(13, OUTPUT); // D4
  pinMode(12, OUTPUT); // D3
  pinMode(2, OUTPUT); // D2
  pinMode(3, OUTPUT); // D1
  pinMode(4, OUTPUT); // A
  pinMode(5, OUTPUT); // B
  pinMode(6, OUTPUT); // C
  pinMode(7, OUTPUT); // D
  pinMode(8, OUTPUT); // E
  pinMode(9, OUTPUT); // F
  pinMode(10, OUTPUT); // G
  pinMode(11, OUTPUT); // H  

  // initialize timer1 
  noInterrupts();           // disable all interrupts
  TCCR1A = 0;
  TCCR1B = 0;

  TCNT1 = 65380;            // preload timer 65536-16MHz/256/4Hz
  TCCR1B |= (1 << CS12);    // 256 prescaler 
  TIMSK1 |= (1 << TOIE1);   // enable timer overflow interrupt
  interrupts();             // enable all interrupts

  Serial.begin(9600);
  // reserve 200 bytes for the inputString:
  inputString.reserve(10);
}

int number = 0;
int count = 0;
long buffSensor = 0;
int offset = 0;
bool firstMeasure = true;
int numAvg = 200;
bool serialSend = false;

ISR(TIMER1_OVF_vect)        // interrupt service routine that wraps a user defined function supplied by attachInterrupt
{
  TCNT1 = 65380;            // preload timer
  
  buffSensor += analogRead(A4);
  count++;

  if(count > numAvg) {
    float sensorValue = 10.861 * (float)buffSensor / (float)count ;
    number = (int)sensorValue - offset;
    buffSensor = 0;
    count = 0;  
    if(firstMeasure) {
      offset = number;
      number = number - offset;
      firstMeasure = false;
    }
    if(serialSend) Serial.println(number);  // Sending Data via Serial
  }
}

void loop() {
  // put your main code here, to run repeatedly:
  displayLCD(number);
  
  if (stringComplete) {
    Serial.println("Code: " + inputString);
    String code = inputString.substring(0, 1);
    if(code == "S") serialSend = true;    // Send data via Seial
    if(code == "M") serialSend = false;   // Manual (only display in 7 Segment)
    if(code == "T") offset = offset + number; // Tare
    if(code == "A") numAvg = inputString.substring(1, inputString.length()).toInt(); // Number of data to average
    
    // clear the string:
    inputString = "";
    stringComplete = false;
  }     
}

void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag so the main loop can
    // do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}

void displayLCD(int num) {
  bool dec = false;
  if(num >= 1000 && num < 10000) {    
    sevenSegmentDisplay(1, num/1000, false);
    sevenSegmentDisplay(2,(num/100)%10, false);
    sevenSegmentDisplay(3,(num/10)%10, false);
    sevenSegmentDisplay(4, num%10, false);
  } else if(num >= 100 && num < 1000) {
    sevenSegmentDisplay(1, 14, false);
    sevenSegmentDisplay(2, num/100, false);
    sevenSegmentDisplay(3,(num/10)%10, false);
    sevenSegmentDisplay(4, num%10, false);    
  } else if(num >= 10 && num < 100) {
    sevenSegmentDisplay(1, 14, false);
    sevenSegmentDisplay(2, 14, false);
    sevenSegmentDisplay(3, num/10, false);
    sevenSegmentDisplay(4, num%10, false);    
  } else if(num >= 0 && num < 10) {
    sevenSegmentDisplay(1, 14, false);
    sevenSegmentDisplay(2, 14, false);
    sevenSegmentDisplay(3, 14, false);
    sevenSegmentDisplay(4, num, false);    
  } else if(num >= -10 && num < 0) {
    num = -num;
    sevenSegmentDisplay(1, 14, false);
    sevenSegmentDisplay(2, 14, false);
    sevenSegmentDisplay(3, 13, false);
    sevenSegmentDisplay(4, num, false);    
  } else if(num >= -100 && num < -10) {
    num = -num;
    sevenSegmentDisplay(1, 14, false);
    sevenSegmentDisplay(2, 13, false);
    sevenSegmentDisplay(3, num/10, false);
    sevenSegmentDisplay(4, num%10, false);    
  } else {
    sevenSegmentDisplay(1, 0, false);
    sevenSegmentDisplay(2, 10, false);
    sevenSegmentDisplay(3, 11, false);
    sevenSegmentDisplay(4, 12, false);  
  }
}

void sevenSegmentDisplay(int digit, int num, bool decimal) {

    digitalWrite(4, HIGH); // A
    digitalWrite(5, HIGH); // B
    digitalWrite(6, HIGH); // C
    digitalWrite(7, HIGH); // D
    digitalWrite(8, HIGH); // E
    digitalWrite(9, HIGH); // F
    digitalWrite(10, HIGH); // G     
    
    switch(digit) {
      case 1: 
        digitalWrite(3, LOW); // D1 
        digitalWrite(2, HIGH); // D2
        digitalWrite(12, HIGH); // D3
        digitalWrite(13, HIGH); // D4
        break;
      case 2: 
        digitalWrite(3, HIGH); // D1 
        digitalWrite(2, LOW); // D2
        digitalWrite(12, HIGH); // D3
        digitalWrite(13, HIGH); // D4
        break;   
      case 3: 
        digitalWrite(3, HIGH); // D1 
        digitalWrite(2, HIGH); // D2
        digitalWrite(12, LOW); // D3
        digitalWrite(13, HIGH); // D4
        break;  
      case 4: 
        digitalWrite(3, HIGH); // D1 
        digitalWrite(2, HIGH); // D2
        digitalWrite(12, HIGH); // D3
        digitalWrite(13, LOW); // D4
        break;                         
    }  
    sevenSegment(num, decimal); 
    delay(5);
}

void sevenSegment(int num, bool decimal) {
  if(decimal) digitalWrite(11, LOW); // H
  else digitalWrite(11, HIGH); // H
  
  switch(num) {
    case 0:
      digitalWrite(4, LOW); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, LOW); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, HIGH); // G      
      break;  
    case 1:
      digitalWrite(4, HIGH); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, HIGH); // D
      digitalWrite(8, HIGH); // E
      digitalWrite(9, HIGH); // F
      digitalWrite(10, HIGH); // G
      break;
    case 2:
      digitalWrite(4, LOW); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, HIGH); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, LOW); // E
      digitalWrite(9, HIGH); // F
      digitalWrite(10, LOW); // G
      break;
    case 3:
      digitalWrite(4, LOW); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, HIGH); // E
      digitalWrite(9, HIGH); // F
      digitalWrite(10, LOW); // G
      break;      
    case 4:
      digitalWrite(4, HIGH); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, HIGH); // D
      digitalWrite(8, HIGH); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, LOW); // G
      break;  
    case 5:
      digitalWrite(4, LOW); // A
      digitalWrite(5, HIGH); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, HIGH); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, LOW); // G
      break;              
    case 6:
      digitalWrite(4, LOW); // A
      digitalWrite(5, HIGH); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, LOW); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, LOW); // G
      break;  
    case 7:
      digitalWrite(4, LOW); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, HIGH); // D
      digitalWrite(8, HIGH); // E
      digitalWrite(9, HIGH); // F
      digitalWrite(10, HIGH); // G
      break;  
    case 8:
      digitalWrite(4, LOW); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, LOW); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, LOW); // G
      break;        
    case 9:
      digitalWrite(4, LOW); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, HIGH); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, LOW); // G
      break;  
    case 10:
      digitalWrite(4, HIGH); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, LOW); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, LOW); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, HIGH); // G
      break;       
    case 11:    
      digitalWrite(4, LOW); // A
      digitalWrite(5, LOW); // B
      digitalWrite(6, HIGH); // C
      digitalWrite(7, LOW); // D
      digitalWrite(8, LOW); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, LOW); // G
      break;   
    case 12:
      digitalWrite(4, LOW); // A
      digitalWrite(5, HIGH); // B
      digitalWrite(6, HIGH); // C
      digitalWrite(7, HIGH); // D
      digitalWrite(8, LOW); // E
      digitalWrite(9, LOW); // F
      digitalWrite(10, HIGH); // G
      break;
    case 13:
      digitalWrite(4, HIGH); // A
      digitalWrite(5, HIGH); // B
      digitalWrite(6, HIGH); // C
      digitalWrite(7, HIGH); // D
      digitalWrite(8, HIGH); // E
      digitalWrite(9, HIGH); // F
      digitalWrite(10, LOW); // G
      break;                
    default:
      digitalWrite(4, HIGH); // A
      digitalWrite(5, HIGH); // B
      digitalWrite(6, HIGH); // C
      digitalWrite(7, HIGH); // D
      digitalWrite(8, HIGH); // E
      digitalWrite(9, HIGH); // F
      digitalWrite(10, HIGH); // G       
  }
}
