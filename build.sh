#!/bin/bash

fsharpc \
  ./program.fs \
  -r:System.dll

mono ./program.exe
