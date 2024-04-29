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
sub rsp, 8
push 5
pop rax
mov [rbp-8], rax
_do$$$1:
push 0
pop rax
cmp rax, 1
jne _endwhile$$$0
push QWORD[rbp-8]
push 1
pop rbx
pop rax
add rax, rbx
push rax
pop rax
mov [rbp-8], rax
jmp _do$$$1
_endwhile$$$0:
push QWORD[rbp-8]
pop rax
push QWORD[rbp]
ret

push QWORD[rbp]
ret
