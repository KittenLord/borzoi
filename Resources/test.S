BITS 64
extern ExitProcess
extern malloc
extern realloc
extern calloc
extern free
extern printf
section .data
$OutOfBounds: db "Attempted to get item %d of an array with length %d %d",0xA,0
$gclen: dq -1
$gccap: dq 128
$gcptr: dq 0
section .text
global _start
_start:
and rsp, -32
mov rbp, rsp
sub rsp, 32
mov rcx, 1024
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
mov rdx, [rel $gccap]
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
sub rsp, 32
mov rbx, [rel $gclen]
mov r12, [rel $gcptr]
mov rdx, [r12 + rbx*8]
mov rcx, $OutOfBounds
mov r8, [rel $gclen]
mov r9, [rel $gcptr]
call printf
add rsp, 32
mov rbx, [rel $gclen]
mov r12, [rel $gcptr]
mov rdi, [r12 + rbx*8]
mov rcx, [r12 + rbx*8]
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
sub rsp, 16
call $gcframe
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
pop rax
mov [rbp-16], rax
pop rax
mov [rbp-16+8], rax
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 1
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
mov rcx, rax
mov rdx, 8
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
lea rax, [rbp-16]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
call $gcclear
mov rsp, rbp
pop rbp
ret
