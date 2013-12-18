@echo off
if not defined include set include=include
fasm fibonacci.asm test.bin
