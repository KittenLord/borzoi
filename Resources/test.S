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
sub rsp, 16
pop rax
mov [rbp-8], rax
push QWORD[rbp-8]
pop rax
mov [rbp-16], rax
push 0
pop rax
push QWORD[rbp]
ret

mov rax, 0
push QWORD[rbp]
ret
