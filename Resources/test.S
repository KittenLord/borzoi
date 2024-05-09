BITS 64
extern ExitProcess
extern malloc
section .text
global _start
_start:
and rsp, -16
mov rbp, rsp
sub rsp, 32
call main
mov rcx, [rsp]
call ExitProcess
mov rbx, rax
mov rax, 1
int 80h
main:
push rbp
mov rbp, rsp
sub rsp, 64
lea rsi, [rsp]
lea rdi, [rbp-16]
mov rcx, 16
rep movsb
add rsp, 16
lea rsi, [rsp]
lea rdi, [rbp-32]
mov rcx, 16
rep movsb
add rsp, 16
lea rsi, [rsp]
lea rdi, [rbp-48]
mov rcx, 16
rep movsb
add rsp, 16
lea rsi, [rsp]
lea rdi, [rbp-64]
mov rcx, 16
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-32]
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rdi, [rbp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
mov rsp, rbp
pop rbp
ret
mov rsp, rbp
pop rbp
ret
