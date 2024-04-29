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
push 12
push 3
pop rbx
pop rax
xor rdx, rdx
cqo
idiv rbx
push rax
pop rax
mov [rbp-8], rax
push QWORD[rbp-8]
push 1
pop rbx
pop rax
add rax, rbx
push rax
push 6
push 2
pop rbx
pop rax
sub rax, rbx
push rax
pop rbx
pop rax
cmp rax, rbx
mov rax, 1
je _$$$temp3
mov rax, 0
_$$$temp3:
push rax
pop rax
push QWORD[rbp]
ret

push QWORD[rbp]
ret