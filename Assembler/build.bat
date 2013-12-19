@echo off
if not defined include set include=include
fasm string.asm test.bin
