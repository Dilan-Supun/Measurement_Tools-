# Fluke 8808A GUI

Small WinForms GUI for Fluke 8808A communication over COM port.

## Features
- COM port selection
- Connect / Disconnect
- Send `*IDN?`
- Set `VDC`, `VAC`, `OHMS`
- Read `VAL?`
- Manual SCPI command textbox
- Pass/fail limit check
- Command/response log

## Requirements
- Windows PC
- .NET Framework 4.8
- Fluke 8808A connected through serial/USB-to-serial interface

## Notes
- The 8808A supports remote operation through the RS-232 computer interface. [web:409][web:412]
- Instrument control can also be done through SCPI-style commands over supported interfaces. [web:402][web:400]
- This sample assumes CR/LF terminated serial responses and common commands such as `*IDN?` and `VAL?`.
