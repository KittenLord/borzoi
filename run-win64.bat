dotnet run
nasm -o .\Resources\test.o -f win64 .\Resources\test.S
gcc .\Resources\test.o -nostdlib -lkernel32 -o .\Resources\test.exe
.\Resources\test.exe
echo %errorlevel%
