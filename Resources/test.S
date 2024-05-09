BITS 64
extern ExitProcess
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
sub rsp, 16
sub rsp, 16
mov QWORD [rsp], 4
lea rsi, [rsp]
lea rdi, [rbp-8]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
sub rsp, 16
sub rsp, 16
lea rsi, [rbp-8]
lea rdi, [rsp]
mov rcx, 8
rep movsb
lea rdi, [rsp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
add rsp, 16
call factorial
add rsp, 16
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
factorial:
push rbp
mov rbp, rsp
sub rsp, 0
sub rsp, 16
lea rsi, [rbp+16]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
mov QWORD [rsp], 0
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
je _temp$$$2
mov rax, 0
_temp$$$2:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else$$$1
sub rsp, 16
mov QWORD [rsp], 1
lea rdi, [rbp+32]
lea rsi, [rsp]
mov rcx, 8
rep movsb
mov rsp, rbp
pop rbp
ret
jmp _endif$$$0
_else$$$1:
_endif$$$0:
sub rsp, 16
lea rsi, [rbp+16]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
mov QWORD [rsp], 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
je _temp$$$5
mov rax, 0
_temp$$$5:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else$$$4
sub rsp, 16
mov QWORD [rsp], 1
lea rdi, [rbp+32]
lea rsi, [rsp]
mov rcx, 8
rep movsb
mov rsp, rbp
pop rbp
ret
jmp _endif$$$3
_else$$$4:
_endif$$$3:
sub rsp, 16
sub rsp, 16
sub rsp, 16
lea rsi, [rbp+16]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
mov QWORD [rsp], 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
sub rax, rbx
mov [rsp], rax
lea rdi, [rsp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
add rsp, 16
call factorial
add rsp, 16
sub rsp, 16
lea rsi, [rbp+16]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
xor rdx, rdx
imul rbx
mov [rsp], rax
lea rdi, [rbp+32]
lea rsi, [rsp]
mov rcx, 8
rep movsb
mov rsp, rbp
pop rbp
ret
mov rsp, rbp
pop rbp
ret
