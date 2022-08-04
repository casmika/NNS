# NNS
Non-nutritive suck (NNS) assessment tool (CMIdp)

This application was developed by Casmika Saputra.
This application can be used for various purposes (not only for NNS) related to real time pressure measurement.

The input can come from serial communication with a baud rate of 9600 or by a data loading.

1. Sending data via serial:
Arduino code example:
```
Serial.println(PressureValue);
```
2. Loading data form file .txt
File contents:
```
time \tab value
```
Example:
```
0 7000
0.5 7002
1.0 7001
```
