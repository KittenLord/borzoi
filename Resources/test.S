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
sub rsp, 32
sub rsp, 16
mov QWORD [rsp], 11
lea rsi, [rsp]
lea rdi, [rbp-8]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
mov QWORD [rsp], 2
lea rsi, [rsp]
lea rdi, [rbp-16]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-16]
lea rdi, [rsp]
mov rcx, 8
rep movsb
lea rsi, [rsp]
lea rdi, [rbp-24]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-8]
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
lea rdi, [rbp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
mov rsp, rbp
pop rbp
ret
jmp _endif$$$0
_else$$$1:
_endif$$$0:
_do$$$4:
sub rsp, 16
lea rsi, [rbp-8]
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
jg _temp$$$5
mov rax, 0
_temp$$$5:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _endwhile$$$3
sub rsp, 16
lea rsi, [rbp-16]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
lea rsi, [rbp-24]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
xor rdx, rdx
imul rbx
mov [rsp], rax
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-8]
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
lea rax, [rbp-8]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 8
rep movsb
add rsp, 16
jmp _do$$$4
_endwhile$$$3:
sub rsp, 16
lea rsi, [rbp-16]
lea rdi, [rsp]
mov rcx, 8
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
