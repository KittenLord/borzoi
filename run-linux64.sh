dotnet run
nasm -o Resources/test.o -f elf64 Resources/test.S
gcc -fPIC Resources/test.o --entry=_start -nostartfiles -o Resources/test
./Resources/test
echo $?
