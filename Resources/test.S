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
push 0
pop rax
mov [rbp-8], rax
push QWORD[rbp-8]
push 5
pop rbx
pop rax
cmp rax, rbx
mov rax, 1
jg _temp$$$2
mov rax, 0
_temp$$$2:
push rax
pop rax
cmp rax, 1
jne _else$$$1
push 1
pop rax
push QWORD[rbp]
ret
jmp _endif$$$0
_else$$$1:
push QWORD[rbp-8]
push 5
pop rbx
pop rax
cmp rax, rbx
mov rax, 1
jl _temp$$$5
mov rax, 0
_temp$$$5:
push rax
pop rax
cmp rax, 1
jne _else$$$4
push 2
pop rax
push QWORD[rbp]
ret
jmp _endif$$$3
_else$$$4:
push 3
pop rax
push QWORD[rbp]
ret
_endif$$$3:
_endif$$$0:
push 0
pop rax
push QWORD[rbp]
ret

push QWORD[rbp]
ret
