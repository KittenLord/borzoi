BITS 64
extern malloc
extern realloc
extern calloc
extern free
extern printf
extern ExitProcess
extern signal
section .data
STR_0@@: db 37,102,10,0,0,0,0
STR_0@@LEN: dq 3
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
tofloat:
push rbp
cvtsi2ss xmm0, rcx
movq rax, xmm0
pop rbp
ret
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
mov rax, __?float32?__(15.3)
push 0
push rax
lea rsi, [rsp]
lea rdi, [rbp-4]
mov rcx, 4
rep movsb
add rsp, 16
mov rax, __?float32?__(54.3)
push 0
push rax
lea rsi, [rsp]
lea rdi, [rbp-8]
mov rcx, 4
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-4]
lea rdi, [rsp]
mov rcx, 4
rep movsb
sub rsp, 16
lea rsi, [rbp-8]
lea rdi, [rsp]
mov rcx, 4
rep movsb
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
movq xmm0, rax
movq xmm1, rbx
addss xmm0, xmm1
movq [rsp], xmm0
lea rsi, [rsp]
lea rdi, [rbp-12]
mov rcx, 4
rep movsb
add rsp, 16
sub rsp, 16
cvtss2sd xmm0, [rsp]
movq [rsp], xmm0
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
mov rdx, [rsp+16]
movq xmm1, [rsp+16]
mov rcx, [rsp+0]
sub rsp, 32
call printf
add rsp, 64
mov [rsp], rax
add rsp, 16
call gcclear@@
mov rsp, rbp
pop rbp
ret
