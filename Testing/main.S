BITS 64
extern malloc
extern realloc
extern calloc
extern free
extern printf
extern ExitProcess
section .data
$STR_0$: db 65,98,111,98,97,0,0,0,0
$STR_0$LEN: dq 5
$OutOfBounds: db "Attempted to get item %d of an array with length %d",0xA,0
$gclen: dq -1
$gccap: dq 4
$gcptr: dq 0
section .text
global main
main:
and rsp, -32
mov rbp, rsp
sub rsp, 32
mov rcx, 32
call malloc
mov [rel $gcptr], rax
sub rsp, 32
call _$main
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
_$main:
push rbp
mov rbp, rsp
sub rsp, 0
call $gcframe
sub rsp, 16
mov rdi, [rel $STR_0$LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 4
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call $gcpush
mov rdi, r12
mov rsi, $STR_0$
mov rcx, [rel $STR_0$LEN]
rep movsb
mov rcx, [rsp+0]
sub rsp, 32
call printf
add rsp, 48
mov [rsp], rax
add rsp, 16
call $gcclear
mov rsp, rbp
pop rbp
ret