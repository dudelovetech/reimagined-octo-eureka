#include <Time.h>
#include <TimeLib.h>
#include <LiquidCrystal.h>

LiquidCrystal lcd(12, 11, 5, 4, 3, 2);

int ss; //seconds
int m = 00; //minutes
int h = 00; //hours
int setM = 0; //set minute
int setH = 0 ; //set hour

void setup() {
  // set up the LCD's number of columns and rows:

  lcd.begin(16, 2);
  lcd.setCursor(2, 0);
  lcd.print("Digital Clock");
  //  pinMode(10, INPUT_PULLUP); //set pin0 as input pin
//  pinMode(13, INPUT); //set pin13 as input pin
  ispm();  //check am or pm
  lcd.setCursor(6, 1); //set up colon to separate hr,min,sec
  lcd.print(":");
  lcd.setCursor(9, 1);
  lcd.print(":");

}

void loop() {
  setH = digitalRead(10);  // set push button for hour
  if (setH == LOW) { //hour button pushed
    delay(300);
    hr();   //increment hour
  }
  setM = digitalRead(7);  // set push button for minute
  if (setM == LOW) { //hour button pushed
    delay(100);
    minute();//increment minute
  }


  lcd.setCursor(7, 1); //set up minute
  lcd.print(m / 10);
  lcd.setCursor(8, 1);
  lcd.print(m % 10);
  if (h % 12 == 0) {
    lcd.setCursor(4, 1);
    lcd.print("12");
  } else {
    lcd.setCursor(4, 1);
    lcd.print(h % 12 / 10);
    lcd.setCursor(5, 1);
    lcd.print(h % 12 % 10);
  }
  sec();
}

void sec() {

  ss = (millis() / 1000) % (60); //gives time since program start in seconds
  lcd.setCursor(10, 1); //print second
  lcd.print(ss / 10);
  lcd.setCursor(11, 1);
  lcd.print(ss % 10);
  if (ss == 59) {
    delay(1000);
    minute();
  }
}
void minute() {

  m++;
  if (m >= 60) {
    m = 0;
    hr();
  }
}

void hr() {

  h++;
  ispm();
  if (h == 24) {
    h = 0;
  }
}

void ispm() {

  if (h < 12  ||  h == 24) {
    lcd.setCursor(12, 1);
    lcd.print("AM");
  }
  if (h >= 12 && h < 24) {
    lcd.setCursor(12, 1);
    lcd.print("PM");
  }
}

