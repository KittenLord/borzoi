BITS 64
extern ExitProcess
section .text
global _start
_start:
mov rbp, rsp
push rbp
call main
mov rcx, rax
call ExitProcess
mov rbx, rax
mov rax, 1
int 80h
main:
mov rbp, rsp
sub rsp, 24
pop rax
mov [rbp-8], rax
pop rax
mov [rbp-16], rax
push QWORD[rbp-16]
pop rax
mov [rbp-24], rax

mov rax, 0
push QWORD[rbp]
ret
