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
push 4
pop rax
mov [rbp-8], rax
push QWORD[rbp-8]
push rbp
call factorial
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
factorial:
mov rbp, rsp
sub rsp, 0
push QWORD[rbp+16]
push 0
pop rbx
pop rax
cmp rax, rbx
mov rax, 1
je _temp$$$2
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
cmp rax, rbx
mov rax, 1
je _temp$$$5
mov rax, 0
_temp$$$5:
push rax
pop rax
cmp rax, 1
jne _else$$$4
push 1
pop rax
push QWORD[rbp]
ret
jmp _endif$$$3
_else$$$4:
_endif$$$3:
push QWORD[rbp+16]
push 1
pop rbx
pop rax
sub rax, rbx
push rax
push rbp
call factorial
mov rsp, rbp
add rsp, 8
pop rbp
add rsp, 8
push rax
push QWORD[rbp+16]
pop rbx
pop rax
xor rdx, rdx
imul rbx
push rax
pop rax
push QWORD[rbp]
ret

mov rax, 0
push QWORD[rbp]
ret
