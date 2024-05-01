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
push 10
pop rax
mov [rbp-8], rax
push QWORD[rbp-8]
push rbp
call fibonacci
mov rsp, rbp
add rsp, 8
pop rbp
add rsp, 8
push rax
pop rax
push QWORD[rbp]
ret

mov rax, 0
push QWORD[rbp]
ret
fibonacci:
mov rbp, rsp
sub rsp, 0
push QWORD[rbp+16]
push 1
pop rbx
pop rax
cmp rax, rbx
mov rax, 1
jle _temp$$$2
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
_endif$$$0:
push QWORD[rbp+16]
push 1
pop rbx
pop rax
sub rax, rbx
push rax
push rbp
call fibonacci
mov rsp, rbp
add rsp, 8
pop rbp
add rsp, 8
push rax
push QWORD[rbp+16]
push 2
pop rbx
pop rax
sub rax, rbx
push rax
push rbp
call fibonacci
mov rsp, rbp
add rsp, 8
pop rbp
add rsp, 8
push rax
pop rbx
pop rax
add rax, rbx
push rax
pop rax
push QWORD[rbp]
ret

mov rax, 0
push QWORD[rbp]
ret
