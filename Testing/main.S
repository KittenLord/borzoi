BITS 64
extern malloc
extern realloc
extern calloc
extern free
extern printf
extern ExitProcess
extern signal
extern InitWindow
extern WindowShouldClose
extern BeginDrawing
extern EndDrawing
extern ClearBackground
extern SetTargetFPS
extern LoadTexture
extern LoadImage
extern DrawTextureRec
extern DrawTexturePro
extern GetWorkingDirectory
extern InitAudioDevice
extern strlen
extern tofloat
section .data
STR_0@@: db 77,105,110,101,115,119,101,101,112,101,114,0,0,0,0
STR_0@@LEN: dq 11
STR_1@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,99,108,111,115,101,100,46,112,110,103,0,0,0,0
STR_1@@LEN: dq 26
STR_2@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,111,112,101,110,101,100,46,112,110,103,0,0,0,0
STR_2@@LEN: dq 26
STR_3@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,49,46,112,110,103,0,0,0,0
STR_3@@LEN: dq 21
STR_4@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,50,46,112,110,103,0,0,0,0
STR_4@@LEN: dq 21
STR_5@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,51,46,112,110,103,0,0,0,0
STR_5@@LEN: dq 21
STR_6@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,52,46,112,110,103,0,0,0,0
STR_6@@LEN: dq 21
STR_7@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,53,46,112,110,103,0,0,0,0
STR_7@@LEN: dq 21
STR_8@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,54,46,112,110,103,0,0,0,0
STR_8@@LEN: dq 21
STR_9@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,55,46,112,110,103,0,0,0,0
STR_9@@LEN: dq 21
STR_10@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,56,46,112,110,103,0,0,0,0
STR_10@@LEN: dq 21
STR_11@@: db 37,100,10,0,0,0,0
STR_11@@LEN: dq 3
error@@OutOfBounds: db "You a big stupid, the array has only %d elements and you're trying to access index %d? Are you crazy?!",0xA,0
error@@SEGFAULT: db "Oopsie daisy, a segfowolt has occurred",0xA,0
gclen@@: dq -1
gccap@@: dq 4
gcptr@@: dq 0
section .text
global main
main:
and rsp, -32
mov rbp, rsp
sub rsp, 32
mov rcx, 32
call malloc
mov [rel gcptr@@], rax
mov rcx, 11
mov rdx, handler@@sigsegv
call signal
sub rsp, 32
mov QWORD [rsp], 0
mov QWORD [rsp+8], 0
 mov QWORD [rsp+16], 0
mov QWORD [rsp+24], 0
call main@@
mov rcx, [rsp]
call ExitProcess
mov rbx, [rsp]
mov rax, 1
int 80h
tofloat:
push rbp
cvtsi2ss xmm0, rcx
movq rax, xmm0
pop rbp
ret
error@@:
push rbp
sub rsp, 32
call printf
mov rcx, -1
call ExitProcess
mov rbx, 1
mov rax, 0
int 80h
handler@@sigsegv:
push rbp
mov rcx, error@@SEGFAULT
call error@@
gccheck@@:
push rbp
mov rax, [rel gccap@@]
sub rax, 1
mov rbx, [rel gclen@@]
cmp rax, rbx
jg gccheckret@@
xor rdx, rdx
mov rax, [rel gccap@@]
mov rbx, 2
mul rbx
mov [rel gccap@@], rax
sub rsp, 32
mov rcx, [rel gcptr@@]
mov rax, [rel gccap@@]
mov rbx, 8
mul rax
mov rdx, rax
call realloc
mov [rel gcptr@@], rax
add rsp, 32
gccheckret@@:
pop rbp
ret
gcpush@@:
push rbp
cmp r12, 0
jne gcpush@@body
cmp r13, 0
jl gcpush@@exit
gcpush@@body:
mov rbx, [rel gclen@@]
inc rbx
mov [rel gclen@@], rbx
call gccheck@@
mov rax, [rel gcptr@@]
mov rbx, [rel gclen@@]
mov [rax + rbx*8], r12
gcpush@@exit:
pop rbp
ret
gcframe@@:
push rbp
mov r12, 0
mov r13, 69
call gcpush@@
mov r13, -69
pop rbp
ret
gcclear@@:
push rbp
gcclearloop@@:
call gcpop@@
cmp r12, 0
jne gcclearloop@@
pop rbp
ret
gcpop@@:
push rbp
mov rbx, [rel gclen@@]
mov r12, [rel gcptr@@]
mov rdi, [r12 + rbx*8]
mov rcx, rdi
mov r12, rdi
sub rsp, 32
call free
add rsp, 32
mov rax, [rel gclen@@]
sub rax, 1
mov [rel gclen@@], rax
pop rbp
ret
gctrypop@@:
push rbp
mov rbx, [rel gclen@@]
mov rax, [rel gcptr@@]
mov rax, [rax + rbx*8]
cmp rax, r12
jne .gctrypop@@exit
mov rbx, [rel gclen@@]
dec rbx
mov [rel gclen@@], rbx
.gctrypop@@exit:
pop rbp
ret
main@@:
push rbp
mov rbp, rsp
sub rsp, 336
call gcframe@@
sub rsp, 0
mov rdi, [rel STR_0@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_0@@
mov rcx, [rel STR_0@@LEN]
rep movsb
push 0
push 450
push 0
push 800
mov r8, [rsp+32]
mov rdx, [rsp+16]
mov rcx, [rsp+0]
sub rsp, 32
call InitWindow
add rsp, 80
add rsp, 0
push 0
push 200
push 0
push 200
push 0
push 200
push 0
push 255
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+64]
mov rcx, 1
rep movsb
lea rdi, [rsp+1]
lea rsi, [rsp+48]
mov rcx, 1
rep movsb
lea rdi, [rsp+2]
lea rsi, [rsp+32]
mov rcx, 1
rep movsb
lea rdi, [rsp+3]
lea rsi, [rsp+16]
mov rcx, 1
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+64]
mov rcx, 4
rep movsb
add rsp, 64
lea rsi, [rsp]
lea rdi, [rbp-4]
mov rcx, 4
rep movsb
add rsp, 16
sub rsp, 32
mov rdi, [rel STR_1@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_1@@
mov rcx, [rel STR_1@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-24]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_2@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_2@@
mov rcx, [rel STR_2@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-44]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_3@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_3@@
mov rcx, [rel STR_3@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-64]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_4@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_4@@
mov rcx, [rel STR_4@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-84]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_5@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_5@@
mov rcx, [rel STR_5@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-104]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_6@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_6@@
mov rcx, [rel STR_6@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-124]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_7@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_7@@
mov rcx, [rel STR_7@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-144]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_8@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_8@@
mov rcx, [rel STR_8@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-164]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_9@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_9@@
mov rcx, [rel STR_9@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-184]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 32
mov rdi, [rel STR_10@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_10@@
mov rcx, [rel STR_10@@LEN]
rep movsb
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-204]
mov rcx, 20
rep movsb
add rsp, 32
sub rsp, 16
mov QWORD [rsp+8], 9
mov rcx, 10
mov rdx, 20
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-44]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 0]
mov rcx, 20
rep movsb
add rsp, 32
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-64]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 20]
mov rcx, 20
rep movsb
add rsp, 32
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-84]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 40]
mov rcx, 20
rep movsb
add rsp, 32
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-104]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 60]
mov rcx, 20
rep movsb
add rsp, 32
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-124]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 80]
mov rcx, 20
rep movsb
add rsp, 32
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-144]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 100]
mov rcx, 20
rep movsb
add rsp, 32
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-164]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 120]
mov rcx, 20
rep movsb
add rsp, 32
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-184]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 140]
mov rcx, 20
rep movsb
add rsp, 32
mov r12, [rsp]
sub rsp, 32
lea rsi, [rbp-204]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 160]
mov rcx, 20
rep movsb
add rsp, 32
lea rsi, [rsp]
lea rdi, [rbp-224]
mov rcx, 16
rep movsb
add rsp, 16
push 0
push __float32__(0.0)
push 0
push __float32__(0.0)
push 0
push __float32__(51.0)
push 0
push __float32__(51.0)
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+64]
mov rcx, 4
rep movsb
lea rdi, [rsp+4]
lea rsi, [rsp+48]
mov rcx, 4
rep movsb
lea rdi, [rsp+8]
lea rsi, [rsp+32]
mov rcx, 4
rep movsb
lea rdi, [rsp+12]
lea rsi, [rsp+16]
mov rcx, 4
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+64]
mov rcx, 16
rep movsb
add rsp, 64
lea rsi, [rsp]
lea rdi, [rbp-240]
mov rcx, 16
rep movsb
add rsp, 16
push 0
push __float32__(0.0)
push 0
push __float32__(0.0)
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+32]
mov rcx, 4
rep movsb
lea rdi, [rsp+4]
lea rsi, [rsp+16]
mov rcx, 4
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+32]
mov rcx, 8
rep movsb
add rsp, 32
lea rsi, [rsp]
lea rdi, [rbp-248]
mov rcx, 8
rep movsb
add rsp, 16
push 0
push 255
push 0
push 255
push 0
push 255
push 0
push 255
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+64]
mov rcx, 1
rep movsb
lea rdi, [rsp+1]
lea rsi, [rsp+48]
mov rcx, 1
rep movsb
lea rdi, [rsp+2]
lea rsi, [rsp+32]
mov rcx, 1
rep movsb
lea rdi, [rsp+3]
lea rsi, [rsp+16]
mov rcx, 1
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+64]
mov rcx, 4
rep movsb
add rsp, 64
lea rsi, [rsp]
lea rdi, [rbp-252]
mov rcx, 4
rep movsb
add rsp, 16
push 0
push 20
lea rsi, [rsp]
lea rdi, [rbp-264]
mov rcx, 8
rep movsb
add rsp, 16
push 0
push 20
lea rsi, [rsp]
lea rdi, [rbp-272]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-264]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
add rax, 8
mov rcx, rax
mov rdx, 16
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
lea rsi, [rsp]
lea rdi, [rbp-288]
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 0
sub rsp, 16
lea rsi, [rbp-288]
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp+8]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 8
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+16]
mov rcx, 8
rep movsb
add rsp, 16
pop rax
add rsp, 8
pop rbx
mov [rsp], rbx
push rax
mov [rbp-296], rbx
jmp _forcheck$$$0
_forstep$$$2:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp$$$3
sub rbx, 2
_fortemp$$$3:
inc rbx
mov [rsp+8], rbx
mov [rbp-296], rbx
_forcheck$$$0:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend$$$1
call gcframe@@
sub rsp, 16
lea rsi, [rbp-272]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rax, [rsp]
add rsp, 16
sub rsp, 16
mov QWORD [rsp+8], rax
add rax, 8
mov rcx, rax
mov rdx, 4
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
lea rax, [rbp-288]
mov rax, [rax]
push rax
push 0
sub rsp, 16
lea rsi, [rbp-296]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
mov rcx, 16
mul rcx
add rsp, 16
pop rbx
lea rax, [rax+rbx]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 0
sub rsp, 16
lea rsi, [rbp-288]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-296]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds$$$8
cmp rax, 0
jge _InBounds$$$9
_OutOfBounds$$$8:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds$$$9:
mov rcx, 16
mul rcx
lea rsi, [rax+rbx]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp+8]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 8
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+16]
mov rcx, 8
rep movsb
add rsp, 16
pop rax
add rsp, 8
pop rbx
mov [rsp], rbx
push rax
mov [rbp-304], rbx
jmp _forcheck$$$4
_forstep$$$6:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp$$$7
sub rbx, 2
_fortemp$$$7:
inc rbx
mov [rsp+8], rbx
mov [rbp-304], rbx
_forcheck$$$4:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend$$$5
call gcframe@@
push 0
push 0
push 0
push 1
push 0
push 0
push 0
push 0
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+64]
mov rcx, 1
rep movsb
lea rdi, [rsp+1]
lea rsi, [rsp+48]
mov rcx, 1
rep movsb
lea rdi, [rsp+2]
lea rsi, [rsp+32]
mov rcx, 1
rep movsb
lea rdi, [rsp+3]
lea rsi, [rsp+16]
mov rcx, 1
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+64]
mov rcx, 4
rep movsb
add rsp, 64
lea rax, [rbp-288]
mov rax, [rax]
push rax
push 0
sub rsp, 16
lea rsi, [rbp-296]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
mov rcx, 16
mul rcx
add rsp, 16
pop rbx
lea rax, [rax+rbx]
mov rax, [rax]
push rax
push 0
sub rsp, 16
lea rsi, [rbp-304]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
mov rcx, 4
mul rcx
add rsp, 16
pop rbx
lea rax, [rax+rbx]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 4
rep movsb
add rsp, 16
sub rsp, 16
sub rsp, 16
lea rsi, [rbp-304]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rdi, [rel STR_11@@LEN]
mov rsi, 1
mov rdx, 1
push rdi
push 0
add rdi, 8
mov rcx, rdi
sub rsp, 32
call calloc
add rsp, 32
mov [rsp], rax
mov r12, rax
call gcpush@@
mov rdi, r12
mov rsi, STR_11@@
mov rcx, [rel STR_11@@LEN]
rep movsb
mov rdx, [rsp+16]
mov rcx, [rsp+0]
sub rsp, 32
call printf
add rsp, 64
mov [rsp], rax
add rsp, 16
call gcclear@@
jmp _forstep$$$6
_forend$$$5:
add rsp, 16
call gcclear@@
jmp _forstep$$$2
_forend$$$1:
add rsp, 16
sub rsp, 0
push 0
push 60
mov rcx, [rsp+0]
sub rsp, 32
call SetTargetFPS
add rsp, 48
add rsp, 0
_do$$$11:
sub rsp, 16
sub rsp, 32
call WindowShouldClose
add rsp, 32
mov [rsp], rax
pop rax
cmp rax, 0
push 1
je _not$$$12
add rsp, 8
push 0
_not$$$12:
mov rax, [rsp]
add rsp, 16
cmp rax, 1
jne _endwhile$$$10
call gcframe@@
sub rsp, 0
sub rsp, 32
call BeginDrawing
add rsp, 32
add rsp, 0
sub rsp, 0
sub rsp, 16
lea rsi, [rbp-4]
lea rdi, [rsp]
mov rcx, 4
rep movsb
mov rcx, [rsp+0]
sub rsp, 32
call ClearBackground
add rsp, 48
add rsp, 0
push 0
push 0
sub rsp, 16
lea rsi, [rbp-288]
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp+8]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 8
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+16]
mov rcx, 8
rep movsb
add rsp, 16
pop rax
add rsp, 8
pop rbx
mov [rsp], rbx
push rax
mov [rbp-312], rbx
jmp _forcheck$$$13
_forstep$$$15:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp$$$16
sub rbx, 2
_fortemp$$$16:
inc rbx
mov [rsp+8], rbx
mov [rbp-312], rbx
_forcheck$$$13:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend$$$14
call gcframe@@
sub rsp, 16
sub rsp, 16
lea rsi, [rbp-312]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rcx, [rsp+0]
sub rsp, 32
call tofloat
add rsp, 48
mov [rsp], rax
lea rsi, [rsp]
lea rdi, [rbp-316]
mov rcx, 4
rep movsb
add rsp, 16
push 0
push 0
sub rsp, 16
lea rsi, [rbp-288]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-312]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds$$$21
cmp rax, 0
jge _InBounds$$$22
_OutOfBounds$$$21:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds$$$22:
mov rcx, 16
mul rcx
lea rsi, [rax+rbx]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp+8]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 8
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+16]
mov rcx, 8
rep movsb
add rsp, 16
pop rax
add rsp, 8
pop rbx
mov [rsp], rbx
push rax
mov [rbp-328], rbx
jmp _forcheck$$$17
_forstep$$$19:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp$$$20
sub rbx, 2
_fortemp$$$20:
inc rbx
mov [rsp+8], rbx
mov [rbp-328], rbx
_forcheck$$$17:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend$$$18
call gcframe@@
sub rsp, 16
sub rsp, 16
lea rsi, [rbp-328]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rcx, [rsp+0]
sub rsp, 32
call tofloat
add rsp, 48
mov [rsp], rax
lea rsi, [rsp]
lea rdi, [rbp-332]
mov rcx, 4
rep movsb
add rsp, 16
sub rsp, 0
sub rsp, 16
lea rsi, [rbp-252]
lea rdi, [rsp]
mov rcx, 4
rep movsb
sub rsp, 16
lea rsi, [rbp-316]
lea rdi, [rsp]
mov rcx, 4
rep movsb
sub rsp, 16
lea rsi, [rbp-332]
lea rdi, [rsp]
mov rcx, 4
rep movsb
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+32]
mov rcx, 4
rep movsb
lea rdi, [rsp+4]
lea rsi, [rsp+16]
mov rcx, 4
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+32]
mov rcx, 8
rep movsb
add rsp, 32
sub rsp, 16
lea rsi, [rbp-240]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 32
lea rsi, [rbp-64]
lea rdi, [rsp]
mov rcx, 20
rep movsb
mov r9, [rsp+64]
mov r8, [rsp+48]
lea rdx, [rsp+32]
lea rcx, [rsp+0]
sub rsp, 32
call DrawTextureRec
add rsp, 112
add rsp, 0
call gcclear@@
jmp _forstep$$$19
_forend$$$18:
add rsp, 16
call gcclear@@
jmp _forstep$$$15
_forend$$$14:
add rsp, 16
sub rsp, 0
sub rsp, 32
call EndDrawing
add rsp, 32
add rsp, 0
call gcclear@@
jmp _do$$$11
_endwhile$$$10:
call gcclear@@
mov rsp, rbp
pop rbp
ret
