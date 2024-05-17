BITS 64
extern malloc
extern realloc
extern calloc
extern free
extern printf
extern ExitProcess
extern signal
extern InitWindow
extern WindowShouldClose
extern BeginDrawing
extern EndDrawing
extern ClearBackground
extern SetTargetFPS
extern LoadTexture
extern LoadImage
extern DrawTextureRec
extern GetWorkingDirectory
extern InitAudioDevice
extern strlen
section .data
STR_0@@: db 77,105,110,101,115,119,101,101,112,101,114,0,0,0,0
STR_0@@LEN: dq 11
STR_1@@: db "assets/1.png",0,0,0,0
STR_1@@LEN: dq 12
error@@OutOfBounds: db "You a big stupid, the array has only %d elements and you're trying to access index %d? Are you crazy?!",0xA,0
error@@SEGFAULT: db "Oopsie daisy, a segfowolt has occurred",0xA,0
gclen@@: dq -1
gccap@@: dq 4
gcptr@@: dq 0
section .text
global main
main:
and rsp, -32
mov rbp, rsp
sub rsp, 32
mov rcx, 32
call malloc
mov [rel gcptr@@], rax
mov rcx, 11
mov rdx, handler@@sigsegv
call signal
sub rsp, 32
mov QWORD [rsp], 0
mov QWORD [rsp+8], 0
 mov QWORD [rsp+16], 0
mov QWORD [rsp+24], 0
call main@@
mov rcx, [rsp]
call ExitProcess
mov rbx, [rsp]
mov rax, 1
int 80h
error@@:
push rbp
sub rsp, 32
call printf
mov rcx, -1
call ExitProcess
mov rbx, 1
mov rax, 0
int 80h
handler@@sigsegv:
push rbp
mov rcx, error@@SEGFAULT
call error@@
gccheck@@:
push rbp
mov rax, [rel gccap@@]
sub rax, 1
mov rbx, [rel gclen@@]
cmp rax, rbx
jg gccheckret@@
xor rdx, rdx
mov rax, [rel gccap@@]
mov rbx, 2
mul rbx
mov [rel gccap@@], rax
sub rsp, 32
mov rcx, [rel gcptr@@]
mov rax, [rel gccap@@]
mov rbx, 8
mul rax
mov rdx, rax
call realloc
mov [rel gcptr@@], rax
add rsp, 32
gccheckret@@:
pop rbp
ret
gcpush@@:
push rbp
cmp r12, 0
jne gcpush@@body
cmp r13, 0
jl gcpush@@exit
gcpush@@body:
mov rbx, [rel gclen@@]
inc rbx
mov [rel gclen@@], rbx
call gccheck@@
mov rax, [rel gcptr@@]
mov rbx, [rel gclen@@]
mov [rax + rbx*8], r12
gcpush@@exit:
pop rbp
ret
gcframe@@:
push rbp
mov r12, 0
mov r13, 69
call gcpush@@
mov r13, -69
pop rbp
ret
gcclear@@:
push rbp
gcclearloop@@:
call gcpop@@
cmp r12, 0
jne gcclearloop@@
pop rbp
ret
gcpop@@:
push rbp
mov rbx, [rel gclen@@]
mov r12, [rel gcptr@@]
mov rdi, [r12 + rbx*8]
mov rcx, rdi
mov r12, rdi
sub rsp, 32
call free
add rsp, 32
mov rax, [rel gclen@@]
sub rax, 1
mov [rel gclen@@], rax
pop rbp
ret
gctrypop@@:
push rbp
mov rbx, [rel gclen@@]
mov rax, [rel gcptr@@]
mov rax, [rax + rbx*8]
cmp rax, r12
jne .gctrypop@@exit
mov rbx, [rel gclen@@]
dec rbx
mov [rel gclen@@], rbx
.gctrypop@@exit:
pop rbp
ret
main@@:
push rbp
mov rbp, rsp
sub rsp, 48
call gcframe@@
sub rsp, 0
mov rdi, [rel STR_0@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_0@@
mov rcx, [rel STR_0@@LEN]
rep movsb
push 0
push 450
push 0
push 800
mov r8, [rsp+32]
mov rdx, [rsp+16]
mov rcx, [rsp+0]
sub rsp, 32
call InitWindow
add rsp, 80
add rsp, 0
sub rsp, 0
sub rsp, 32
call InitAudioDevice
add rsp, 32
add rsp, 0
push 0
push 200
push 0
push 200
push 0
push 200
push 0
push 255
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+64]
mov rcx, 1
rep movsb
lea rdi, [rsp+1]
lea rsi, [rsp+48]
mov rcx, 1
rep movsb
lea rdi, [rsp+2]
lea rsi, [rsp+32]
mov rcx, 1
rep movsb
lea rdi, [rsp+3]
lea rsi, [rsp+16]
mov rcx, 1
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+64]
mov rcx, 4
rep movsb
add rsp, 64
lea rsi, [rsp]
lea rdi, [rbp-4]
mov rcx, 4
rep movsb
add rsp, 16
mov rdi, [rel STR_1@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_1@@
mov rcx, [rel STR_1@@LEN]
rep movsb
lea rsi, [rsp]
lea rdi, [rbp-24]
mov rcx, 16
rep movsb
add rsp, 16
sub rsp, 16
sub rsp, 16
lea rsi, [rbp-24]
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rcx, [rel STR_1@@]
mov rdx, rcx
sub rsp, 32
call LoadImage
add rsp, 48
mov [rsp], rax
add rsp, 16
push 0
push __float32__(30.0)
lea rsi, [rsp]
lea rdi, [rbp-28]
mov rcx, 4
rep movsb
add rsp, 16
push 0
push __float32__(0.0)
push 0
push __float32__(0.0)
sub rsp, 16
lea rsi, [rbp-28]
lea rdi, [rsp]
mov rcx, 4
rep movsb
sub rsp, 16
lea rsi, [rbp-28]
lea rdi, [rsp]
mov rcx, 4
rep movsb
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+64]
mov rcx, 4
rep movsb
lea rdi, [rsp+4]
lea rsi, [rsp+48]
mov rcx, 4
rep movsb
lea rdi, [rsp+8]
lea rsi, [rsp+32]
mov rcx, 4
rep movsb
lea rdi, [rsp+12]
lea rsi, [rsp+16]
mov rcx, 4
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+64]
mov rcx, 16
rep movsb
add rsp, 64
lea rsi, [rsp]
lea rdi, [rbp-44]
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 255
push 0
push 255
push 0
push 255
push 0
push 255
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+64]
mov rcx, 1
rep movsb
lea rdi, [rsp+1]
lea rsi, [rsp+48]
mov rcx, 1
rep movsb
lea rdi, [rsp+2]
lea rsi, [rsp+32]
mov rcx, 1
rep movsb
lea rdi, [rsp+3]
lea rsi, [rsp+16]
mov rcx, 1
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+64]
mov rcx, 4
rep movsb
add rsp, 64
lea rsi, [rsp]
lea rdi, [rbp-48]
mov rcx, 4
rep movsb
add rsp, 16
sub rsp, 0
push 0
push 60
mov rcx, [rsp+0]
sub rsp, 32
call SetTargetFPS
add rsp, 48
add rsp, 0
_do$$$1:
sub rsp, 16
sub rsp, 32
call WindowShouldClose
add rsp, 32
mov [rsp], rax
pop rax
cmp rax, 0
push 1
je _not$$$2
add rsp, 8
push 0
_not$$$2:
mov rax, [rsp]
add rsp, 16
cmp rax, 1
jne _endwhile$$$0
call gcframe@@
sub rsp, 0
sub rsp, 32
call BeginDrawing
add rsp, 32
add rsp, 0
sub rsp, 0
sub rsp, 16
lea rsi, [rbp-4]
lea rdi, [rsp]
mov rcx, 4
rep movsb
mov rcx, [rsp+0]
sub rsp, 32
call ClearBackground
add rsp, 48
add rsp, 0
sub rsp, 0
sub rsp, 32
call EndDrawing
add rsp, 32
add rsp, 0
call gcclear@@
jmp _do$$$1
_endwhile$$$0:
call gcclear@@
mov rsp, rbp
pop rbp
ret
