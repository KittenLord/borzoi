BITS 64
extern malloc
extern realloc
extern calloc
extern free
extern printf
extern ExitProcess
extern signal
section .data
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
sub rsp, 16
call gcframe@@
mov QWORD [rbp-8], 10
sub rsp, 16
sub rsp, 16
push 0
push QWORD [rbp-8+0]
lea rdi, [rsp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
add rsp, 16
call fibonacci
add rsp, 16
lea rdi, [rbp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
call gcclear@@
mov rsp, rbp
pop rbp
ret
call gcclear@@
mov rsp, rbp
pop rbp
ret
fibonacci:
push rbp
mov rbp, rsp
sub rsp, 0
call gcframe@@
push 0
push QWORD [rbp+16+0]
push 0
push 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
jle _temp$$$2
mov rax, 0
_temp$$$2:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else$$$1
call gcframe@@
push 0
push 1
lea rdi, [rbp+32]
lea rsi, [rsp]
mov rcx, 8
rep movsb
call gcclear@@
call gcclear@@
mov rsp, rbp
pop rbp
ret
call gcclear@@
jmp _endif$$$0
_else$$$1:
_endif$$$0:
sub rsp, 16
sub rsp, 16
push 0
push QWORD [rbp+16+0]
push 0
push 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
sub rax, rbx
mov [rsp], rax
lea rdi, [rsp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
add rsp, 16
call fibonacci
add rsp, 16
sub rsp, 16
sub rsp, 16
push 0
push QWORD [rbp+16+0]
push 0
push 2
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
sub rax, rbx
mov [rsp], rax
lea rdi, [rsp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
add rsp, 16
call fibonacci
add rsp, 16
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
add rax, rbx
mov [rsp], rax
lea rdi, [rbp+32]
lea rsi, [rsp]
mov rcx, 8
rep movsb
call gcclear@@
mov rsp, rbp
pop rbp
ret
call gcclear@@
mov rsp, rbp
pop rbp
ret
