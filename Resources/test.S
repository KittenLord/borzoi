BITS 64
extern ExitProcess
extern malloc
extern realloc
extern calloc
extern free
extern printf
section .data
$OutOfBounds: db "Attempted to get item %d of an array with length %d",0xA,0
$gclen: dq -1
$gccap: dq 4
$gcptr: dq 0
section .text
global _start
_start:
and rsp, -32
mov rbp, rsp
sub rsp, 32
mov rcx, 32
call malloc
mov [rel $gcptr], rax
sub rsp, 32
call main
mov rcx, [rsp]
call ExitProcess
mov rbx, [rsp]
mov rax, 1
int 80h
$error:
sub rsp, 32
call printf
mov rcx, -1
call ExitProcess
mov rbx, 1
mov rax, 0
int 80h
$gccheck:
push rbp
mov rax, [rel $gccap]
sub rax, 1
mov rbx, [rel $gclen]
cmp rax, rbx
jg $gccheckret
xor rdx, rdx
mov rax, [rel $gccap]
mov rbx, 2
mul rbx
mov [rel $gccap], rax
sub rsp, 32
mov rcx, [rel $gcptr]
mov rax, [rel $gccap]
mov rbx, 8
mul rax
mov rdx, rax
call realloc
mov [rel $gcptr], rax
add rsp, 32
$gccheckret:
pop rbp
ret
$gcpush:
push rbp
mov rbx, [rel $gclen]
inc rbx
mov [rel $gclen], rbx
call $gccheck
mov rax, [rel $gcptr]
mov rbx, [rel $gclen]
mov [rax + rbx*8], r12
pop rbp
ret
$gcframe:
push rbp
mov r12, 0
call $gcpush
pop rbp
ret
$gcclear:
push rbp
$gcclearloop:
call $gcpop
cmp r12, 0
jne $gcclearloop
pop rbp
ret
$gcpop:
push rbp
mov rbx, [rel $gclen]
mov r12, [rel $gcptr]
mov rdi, [r12 + rbx*8]
mov rcx, rdi
mov r12, rdi
sub rsp, 32
call free
add rsp, 32
mov rax, [rel $gclen]
sub rax, 1
mov [rel $gclen], rax
pop rbp
ret
main:
push rbp
mov rbp, rsp
sub rsp, 32
call $gcframe
sub rsp, 16
mov QWORD [rsp+8], 10
mov rcx, 10
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
mov r12, [rsp]
push 0
push 1
lea rsi, [rsp]
lea rdi, [r12 + 0]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 2
lea rsi, [rsp]
lea rdi, [r12 + 8]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 3
lea rsi, [rsp]
lea rdi, [r12 + 16]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 4
lea rsi, [rsp]
lea rdi, [r12 + 24]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 5
lea rsi, [rsp]
lea rdi, [r12 + 32]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 6
lea rsi, [rsp]
lea rdi, [r12 + 40]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 7
lea rsi, [rsp]
lea rdi, [r12 + 48]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 8
lea rsi, [rsp]
lea rdi, [r12 + 56]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 9
lea rsi, [rsp]
lea rdi, [r12 + 64]
mov rcx, 8
rep movsb
add rsp, 16
mov r12, [rsp]
push 0
push 10
lea rsi, [rsp]
lea rdi, [r12 + 72]
mov rcx, 8
rep movsb
add rsp, 16
pop rax
mov [rbp-16], rax
pop rax
mov [rbp-16+8], rax
mov QWORD [rbp-24], 0
mov QWORD [rbp-32], 0
_do$$$1:
push 0
push QWORD [rbp-24+0]
push 0
push 10
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
jl _temp$$$2
mov rax, 0
_temp$$$2:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
cmp rax, 1
jne _endwhile$$$0
call $gcframe
push 0
push QWORD [rbp-32+0]
push QWORD [rbp-16+8]
push QWORD [rbp-16+0]
push 0
push QWORD [rbp-24+0]
pop rbx
add rsp, 8
pop rax
pop rcx
cmp rbx, rcx
jge _OutOfBounds$$$3
cmp rbx, 0
jge _InBounds$$$4
_OutOfBounds$$$3:
mov r8, rcx
mov rdx, rbx
mov rcx, $OutOfBounds
call $error
_InBounds$$$4:
lea rsi, [rax+rbx*8]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
add rax, rbx
mov [rsp], rax
lea rax, [rbp-32]
pop rbx
mov [rax], rbx
add rsp, 8
push 0
push QWORD [rbp-24+0]
push 0
push 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
add rax, rbx
mov [rsp], rax
lea rax, [rbp-24]
pop rbx
mov [rax], rbx
add rsp, 8
call $gcclear
jmp _do$$$1
_endwhile$$$0:
push 0
push QWORD [rbp-32+0]
lea rdi, [rbp+16]
lea rsi, [rsp]
mov rcx, 8
rep movsb
call $gcclear
mov rsp, rbp
pop rbp
ret
call $gcclear
mov rsp, rbp
pop rbp
ret
