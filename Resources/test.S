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
sub rsp, 40
push 5
pop rax
mov [rbp-8], rax
push rbp
mov rax, rbp
sub rax, 8
push rax
call _1
mov rsp, rbp
add rsp, 16
pop rbp
push rax
push 100
pop rax
mov [rbp-16], rax
mov rax, rbp
sub rax, 16
push rax
pop rax
mov [rbp-24], rax
mov rax, rbp
sub rax, 24
push rax
pop rax
mov [rbp-32], rax
mov rax, rbp
sub rax, 32
push rax
pop rax
mov [rbp-40], rax
mov rax, rbp
sub rax, 40
mov rax, [rax]
mov rax, [rax]
mov rax, [rax]
push rax
push 1
pop rax
pop rbx
mov [rbx], rax
push QWORD[rbp-8]
push QWORD[rbp-16]
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
_1:
mov rbp, rsp
sub rsp, 0
push rbp
mov rax, rbp
add rax, 8
push rax
call _2
mov rsp, rbp
add rsp, 16
pop rbp
push rax
mov rax, 0
push QWORD[rbp]
ret
_2:
mov rbp, rsp
sub rsp, 0
push rbp
mov rax, rbp
add rax, 8
push rax
call _3
mov rsp, rbp
add rsp, 16
pop rbp
push rax
mov rax, 0
push QWORD[rbp]
ret
_3:
mov rbp, rsp
sub rsp, 0
mov rax, rbp
add rax, 8
mov rax, [rax]
mov rax, [rax]
mov rax, [rax]
push rax
push 68
pop rax
pop rbx
mov [rbx], rax
mov rax, 0
push QWORD[rbp]
ret
