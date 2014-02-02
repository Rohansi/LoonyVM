@echo off
if not defined include set include=include;.
fasm test.asm ../LoonyVM/bin/Debug/bios.bin
