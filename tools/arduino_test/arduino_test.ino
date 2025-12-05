// Symulator przycisków Kiosku
// Zwarcie PIN 2 do GND wysyła "1"
// Zwarcie PIN 3 do GND wysyła "2"

const int BTN_LANG_PIN = 2;
const int BTN_SELECT_PIN = 3;

void setup() {

  Serial.begin(9600);
  
  // INPUT_PULLUP sprawia, że stan wysoki jest domyślny.
  // Zwarcie do GND daje stan niski (LOW).
  pinMode(BTN_LANG_PIN, INPUT_PULLUP);
  pinMode(BTN_SELECT_PIN, INPUT_PULLUP);
}

void loop() {
  // Przycisk 1 (Zmiana języka)
  if (digitalRead(BTN_LANG_PIN) == LOW) {
    Serial.println("1"); 
    delay(300); // Prosty hardware debounce
  }

  // Przycisk 2 (Select / Back)
  if (digitalRead(BTN_SELECT_PIN) == LOW) {
    Serial.println("2");
    delay(300);
  }
}