@ECHO OFF

:: Call nasm assembler
.\win64\nasm.exe -w+all -f win32 -o "%2/%1.obj" "%1.asm"

if exist "%2\%1.obj" (
    :: Call linker
    ld -s -o "%2\%1.exe" "%2\%1.obj"
    :: Cleanup
    rm "%2\%1.obj"
)

