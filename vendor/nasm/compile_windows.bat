@ECHO OFF

:: Call nasm assembler
"%4\vendor\nasm\win64\nasm.exe" -w+all -f win64 -o "%2" "%1"

:: if exist "%2" (
::     :: Call linker
::     ld -s -o "%3" "%2"
:: )

