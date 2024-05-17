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
extern time
extern srand
extern rand
extern tofloat
section .data
STR_0@@: db 77,105,110,101,115,119,101,101,112,101,114,0,0,0,0
STR_0@@LEN: dq 11
STR_1@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,99,108,111,115,101,100,46,112,110,103,0,0,0,0
STR_1@@LEN: dq 26
STR_2@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,109,105,110,101,46,112,110,103,0,0,0,0
STR_2@@LEN: dq 24
STR_3@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,111,112,101,110,101,100,46,112,110,103,0,0,0,0
STR_3@@LEN: dq 26
STR_4@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,49,46,112,110,103,0,0,0,0
STR_4@@LEN: dq 21
STR_5@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,50,46,112,110,103,0,0,0,0
STR_5@@LEN: dq 21
STR_6@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,51,46,112,110,103,0,0,0,0
STR_6@@LEN: dq 21
STR_7@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,52,46,112,110,103,0,0,0,0
STR_7@@LEN: dq 21
STR_8@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,53,46,112,110,103,0,0,0,0
STR_8@@LEN: dq 21
STR_9@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,54,46,112,110,103,0,0,0,0
STR_9@@LEN: dq 21
STR_10@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,55,46,112,110,103,0,0,0,0
STR_10@@LEN: dq 21
STR_11@@: db 97,115,115,101,116,115,47,114,101,115,99,97,108,101,100,47,56,46,112,110,103,0,0,0,0
STR_11@@LEN: dq 21
STR_12@@: db 73,84,69,82,32,37,100,32,37,100,10,0,0,0,0
STR_12@@LEN: dq 11
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
sub rsp, 464
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
push 900
push 0
push 1600
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
sub rsp, 32
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
lea r14, [rsp+16]
mov rdx, [rsp+0]
mov rcx, r14
sub rsp, 32
call LoadTexture
add rsp, 48
lea rsi, [rsp]
lea rdi, [rbp-224]
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
lea rsi, [rbp-64]
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
lea rsi, [rbp-84]
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
lea rsi, [rbp-104]
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
lea rsi, [rbp-124]
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
lea rsi, [rbp-144]
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
lea rsi, [rbp-164]
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
lea rsi, [rbp-184]
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
lea rsi, [rbp-204]
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
lea rsi, [rbp-224]
lea rdi, [rsp]
mov rcx, 20
rep movsb
lea rsi, [rsp]
lea rdi, [r12 + 160]
mov rcx, 20
rep movsb
add rsp, 32
lea rsi, [rsp]
lea rdi, [rbp-240]
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
lea rdi, [rbp-256]
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
lea rdi, [rbp-264]
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
lea rdi, [rbp-268]
mov rcx, 4
rep movsb
add rsp, 16
sub rsp, 0
sub rsp, 16
push 0
push 0
mov rcx, [rsp+0]
sub rsp, 32
call time
add rsp, 48
mov [rsp], rax
mov rcx, [rsp+0]
sub rsp, 32
call srand
add rsp, 48
add rsp, 0
push 0
push 30
lea rsi, [rsp]
lea rdi, [rbp-280]
mov rcx, 8
rep movsb
add rsp, 16
push 0
push 16
lea rsi, [rsp]
lea rdi, [rbp-288]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-280]
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
lea rdi, [rbp-304]
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 0
sub rsp, 16
lea rsi, [rbp-304]
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
jmp _forcheck@@@0
_forstep@@@2:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp@@@3
sub rbx, 2
_fortemp@@@3:
inc rbx
mov [rsp+8], rbx
mov [rbp-312], rbx
_forcheck@@@0:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend@@@1
sub rsp, 16
lea rsi, [rbp-288]
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
lea rax, [rbp-304]
mov rax, [rax]
push rax
push 0
sub rsp, 16
lea rsi, [rbp-312]
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
lea rsi, [rbp-304]
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
jge _OutOfBounds@@@8
cmp rax, 0
jge _InBounds@@@9
_OutOfBounds@@@8:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@9:
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
mov [rbp-320], rbx
jmp _forcheck@@@4
_forstep@@@6:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp@@@7
sub rbx, 2
_fortemp@@@7:
inc rbx
mov [rsp+8], rbx
mov [rbp-320], rbx
_forcheck@@@4:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend@@@5
sub rsp, 16
sub rsp, 32
call rand
add rsp, 32
mov [rsp], rax
push 0
push 100
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
xor rdx, rdx
cqo
idiv rbx
mov rax, rdx
add rax, rbx
xor rdx, rdx
cqo
idiv rbx
mov rax, rdx
mov [rsp], rax
lea rsi, [rsp]
lea rdi, [rbp-328]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-328]
lea rdi, [rsp]
mov rcx, 8
rep movsb
push 0
push 85
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
jg _temp@@@11
mov rax, 0
_temp@@@11:
mov [rsp], rax
lea rsi, [rsp]
lea rdi, [rbp-329]
mov rcx, 1
rep movsb
add rsp, 16
push 0
push 0
push 0
push 1
sub rsp, 16
lea rsi, [rbp-329]
lea rdi, [rsp]
mov rcx, 1
rep movsb
push 0
push 0
sub rsp, 16
lea rdi, [rsp+0]
lea rsi, [rsp+64]
mov rcx, 8
rep movsb
lea rdi, [rsp+8]
lea rsi, [rsp+48]
mov rcx, 1
rep movsb
lea rdi, [rsp+9]
lea rsi, [rsp+32]
mov rcx, 1
rep movsb
lea rdi, [rsp+10]
lea rsi, [rsp+16]
mov rcx, 1
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+64]
mov rcx, 16
rep movsb
add rsp, 64
lea rax, [rbp-304]
mov rax, [rax]
push rax
push 0
sub rsp, 16
lea rsi, [rbp-312]
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
lea rsi, [rbp-320]
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
jmp _forstep@@@6
_forend@@@5:
add rsp, 16
jmp _forstep@@@2
_forend@@@1:
add rsp, 16
push 0
push 0
sub rsp, 16
lea rsi, [rbp-304]
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
mov [rbp-344], rbx
jmp _forcheck@@@12
_forstep@@@14:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp@@@15
sub rbx, 2
_fortemp@@@15:
inc rbx
mov [rsp+8], rbx
mov [rbp-344], rbx
_forcheck@@@12:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend@@@13
call gcframe@@
push 0
push 0
sub rsp, 16
lea rsi, [rbp-304]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-344]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds@@@20
cmp rax, 0
jge _InBounds@@@21
_OutOfBounds@@@20:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@21:
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
mov [rbp-352], rbx
jmp _forcheck@@@16
_forstep@@@18:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp@@@19
sub rbx, 2
_fortemp@@@19:
inc rbx
mov [rsp+8], rbx
mov [rbp-352], rbx
_forcheck@@@16:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend@@@17
call gcframe@@
push 0
push 0
lea rsi, [rsp]
lea rdi, [rbp-360]
mov rcx, 8
rep movsb
add rsp, 16
push 0
push 0
push 0
push 3
pop rax
add rsp, 8
pop rbx
mov [rsp], rbx
push rax
mov [rbp-368], rbx
jmp _forcheck@@@22
_forstep@@@24:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp@@@25
sub rbx, 2
_fortemp@@@25:
inc rbx
mov [rsp+8], rbx
mov [rbp-368], rbx
_forcheck@@@22:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend@@@23
call gcframe@@
push 0
push 0
push 0
push 3
pop rax
add rsp, 8
pop rbx
mov [rsp], rbx
push rax
mov [rbp-376], rbx
jmp _forcheck@@@26
_forstep@@@28:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp@@@29
sub rbx, 2
_fortemp@@@29:
inc rbx
mov [rsp+8], rbx
mov [rbp-376], rbx
_forcheck@@@26:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend@@@27
call gcframe@@
sub rsp, 16
sub rsp, 16
lea rsi, [rbp-376]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
lea rsi, [rbp-368]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rdi, [rel STR_12@@LEN]
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
mov rsi, STR_12@@
mov rcx, [rel STR_12@@LEN]
rep movsb
mov r8, [rsp+32]
mov rdx, [rsp+16]
mov rcx, [rsp+0]
sub rsp, 32
call printf
add rsp, 80
mov [rsp], rax
add rsp, 16
sub rsp, 16
lea rsi, [rbp-368]
lea rdi, [rsp]
mov rcx, 8
rep movsb
push 0
push 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
je _temp@@@32
mov rax, 0
_temp@@@32:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else@@@31
call gcframe@@
sub rsp, 16
lea rsi, [rbp-376]
lea rdi, [rsp]
mov rcx, 8
rep movsb
push 0
push 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
je _temp@@@35
mov rax, 0
_temp@@@35:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else@@@34
call gcframe@@
jmp _forstep@@@28
call gcclear@@
jmp _endif@@@33
_else@@@34:
_endif@@@33:
call gcclear@@
jmp _endif@@@30
_else@@@31:
_endif@@@30:
sub rsp, 16
lea rsi, [rbp-344]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
lea rsi, [rbp-368]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
add rax, rbx
mov [rsp], rax
push 0
push 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
sub rax, rbx
mov [rsp], rax
lea rsi, [rsp]
lea rdi, [rbp-384]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-352]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
lea rsi, [rbp-376]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
add rax, rbx
mov [rsp], rax
push 0
push 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
sub rax, rbx
mov [rsp], rax
lea rsi, [rsp]
lea rdi, [rbp-392]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-384]
lea rdi, [rsp]
mov rcx, 8
rep movsb
push 0
push 0
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
jl _temp@@@42
mov rax, 0
_temp@@@42:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else@@@41
call gcframe@@
jmp _forstep@@@28
call gcclear@@
jmp _endif@@@40
_else@@@41:
_endif@@@40:
sub rsp, 16
lea rsi, [rbp-384]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
lea rsi, [rbp-304]
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
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
jge _temp@@@45
mov rax, 0
_temp@@@45:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else@@@44
call gcframe@@
jmp _forstep@@@28
call gcclear@@
jmp _endif@@@43
_else@@@44:
_endif@@@43:
sub rsp, 16
lea rsi, [rbp-392]
lea rdi, [rsp]
mov rcx, 8
rep movsb
push 0
push 0
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
jl _temp@@@48
mov rax, 0
_temp@@@48:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else@@@47
call gcframe@@
jmp _forstep@@@28
call gcclear@@
jmp _endif@@@46
_else@@@47:
_endif@@@46:
sub rsp, 16
lea rsi, [rbp-392]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
lea rsi, [rbp-304]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-344]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds@@@51
cmp rax, 0
jge _InBounds@@@52
_OutOfBounds@@@51:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@52:
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
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
cmp rax, rbx
mov rax, 1
jge _temp@@@53
mov rax, 0
_temp@@@53:
mov [rsp], rax
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else@@@50
call gcframe@@
jmp _forstep@@@28
call gcclear@@
jmp _endif@@@49
_else@@@50:
_endif@@@49:
sub rsp, 16
lea rsi, [rbp-304]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-384]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds@@@56
cmp rax, 0
jge _InBounds@@@57
_OutOfBounds@@@56:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@57:
mov rcx, 16
mul rcx
lea rsi, [rax+rbx]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-392]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds@@@58
cmp rax, 0
jge _InBounds@@@59
_OutOfBounds@@@58:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@59:
mov rcx, 16
mul rcx
lea rsi, [rax+rbx]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp+9]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 1
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+16]
mov rcx, 1
rep movsb
add rsp, 16
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else@@@55
call gcframe@@
sub rsp, 16
lea rsi, [rbp-360]
lea rdi, [rsp]
mov rcx, 8
rep movsb
push 0
push 1
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
add rax, rbx
mov [rsp], rax
lea rax, [rbp-360]
lea rsi, [rsp]
mov rdi, rax
mov rcx, 8
rep movsb
add rsp, 16
call gcclear@@
jmp _endif@@@54
_else@@@55:
_endif@@@54:
call gcclear@@
jmp _forstep@@@28
_forend@@@27:
add rsp, 16
call gcclear@@
jmp _forstep@@@24
_forend@@@23:
add rsp, 16
sub rsp, 16
lea rsi, [rbp-360]
lea rdi, [rsp]
mov rcx, 8
rep movsb
lea rax, [rbp-304]
mov rax, [rax]
push rax
push 0
sub rsp, 16
lea rsi, [rbp-344]
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
lea rsi, [rbp-352]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
mov rcx, 16
mul rcx
add rsp, 16
pop rbx
lea rax, [rax+rbx]
add rax, 0
lea rsi, [rsp]
mov rdi, rax
mov rcx, 8
rep movsb
add rsp, 16
call gcclear@@
jmp _forstep@@@18
_forend@@@17:
add rsp, 16
call gcclear@@
jmp _forstep@@@14
_forend@@@13:
add rsp, 16
sub rsp, 0
push 0
push 60
mov rcx, [rsp+0]
sub rsp, 32
call SetTargetFPS
add rsp, 48
add rsp, 0
_whiledo@@@62:
_whilecondition@@@63:
sub rsp, 16
sub rsp, 32
call WindowShouldClose
add rsp, 32
mov [rsp], rax
pop rax
cmp rax, 0
push 1
je _not@@@64
add rsp, 8
push 0
_not@@@64:
mov rax, [rsp]
add rsp, 16
cmp rax, 1
jne _whileend@@@61
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
sub rsp, 16
lea rsi, [rbp-304]
lea rdi, [rsp]
mov rcx, 16
rep movsb
push 0
push 0
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds@@@65
cmp rax, 0
jge _InBounds@@@66
_OutOfBounds@@@65:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@66:
mov rcx, 16
mul rcx
lea rsi, [rax+rbx]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp]
lea rdi, [rbp-408]
mov rcx, 16
rep movsb
add rsp, 16
push 0
push 0
sub rsp, 16
lea rsi, [rbp-280]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
mov [rsp], rbx
push rax
mov [rbp-416], rbx
jmp _forcheck@@@67
_forstep@@@69:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp@@@70
sub rbx, 2
_fortemp@@@70:
inc rbx
mov [rsp+8], rbx
mov [rbp-416], rbx
_forcheck@@@67:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend@@@68
call gcframe@@
sub rsp, 16
lea rsi, [rbp-416]
lea rdi, [rsp]
mov rcx, 8
rep movsb
push 0
push 51
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
xor rdx, rdx
imul rbx
mov [rsp], rax
lea rsi, [rsp]
lea rdi, [rbp-424]
mov rcx, 8
rep movsb
add rsp, 16
push 0
push 0
sub rsp, 16
lea rsi, [rbp-288]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
mov [rsp], rbx
push rax
mov [rbp-432], rbx
jmp _forcheck@@@72
_forstep@@@74:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rbx, rax
jl _fortemp@@@75
sub rbx, 2
_fortemp@@@75:
inc rbx
mov [rsp+8], rbx
mov [rbp-432], rbx
_forcheck@@@72:
mov rax, [rsp]
mov rbx, [rsp+8]
cmp rax, rbx
je _forend@@@73
call gcframe@@
sub rsp, 16
lea rsi, [rbp-432]
lea rdi, [rsp]
mov rcx, 8
rep movsb
push 0
push 51
mov rax, [rsp+16]
mov rbx, [rsp]
add rsp, 16
xor rdx, rdx
imul rbx
mov [rsp], rax
lea rsi, [rsp]
lea rdi, [rbp-440]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
sub rsp, 16
lea rsi, [rbp-424]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rcx, [rsp+0]
sub rsp, 32
call tofloat
add rsp, 48
mov [rsp], rax
sub rsp, 16
sub rsp, 16
lea rsi, [rbp-440]
lea rdi, [rsp]
mov rcx, 8
rep movsb
mov rcx, [rsp+0]
sub rsp, 32
call tofloat
add rsp, 48
mov [rsp], rax
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
lea rdi, [rbp-448]
mov rcx, 8
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-304]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-416]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds@@@77
cmp rax, 0
jge _InBounds@@@78
_OutOfBounds@@@77:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@78:
mov rcx, 16
mul rcx
lea rsi, [rax+rbx]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-432]
lea rdi, [rsp]
mov rcx, 8
rep movsb
pop rax
add rsp, 8
pop rbx
pop rcx
cmp rax, rcx
jge _OutOfBounds@@@79
cmp rax, 0
jge _InBounds@@@80
_OutOfBounds@@@79:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@80:
mov rcx, 16
mul rcx
lea rsi, [rax+rbx]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp]
lea rdi, [rbp-464]
mov rcx, 16
rep movsb
add rsp, 16
sub rsp, 16
lea rsi, [rbp-464]
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp+9]
sub rsp, 16
lea rdi, [rsp]
mov rcx, 1
rep movsb
lea rsi, [rsp]
lea rdi, [rsp+16]
mov rcx, 1
rep movsb
add rsp, 16
mov rax, [rsp]
add rsp, 16
and rax, 1
cmp rax, 1
jne _else@@@82
call gcframe@@
sub rsp, 0
sub rsp, 16
lea rsi, [rbp-268]
lea rdi, [rsp]
mov rcx, 4
rep movsb
sub rsp, 16
lea rsi, [rbp-448]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
lea rsi, [rbp-256]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 32
lea rsi, [rbp-44]
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
jmp _endif@@@81
_else@@@82:
call gcframe@@
sub rsp, 0
sub rsp, 16
lea rsi, [rbp-268]
lea rdi, [rsp]
mov rcx, 4
rep movsb
sub rsp, 16
lea rsi, [rbp-448]
lea rdi, [rsp]
mov rcx, 8
rep movsb
sub rsp, 16
lea rsi, [rbp-256]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-240]
lea rdi, [rsp]
mov rcx, 16
rep movsb
sub rsp, 16
lea rsi, [rbp-464]
lea rdi, [rsp]
mov rcx, 16
rep movsb
lea rsi, [rsp+0]
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
pop rcx
cmp rax, rcx
jge _OutOfBounds@@@83
cmp rax, 0
jge _InBounds@@@84
_OutOfBounds@@@83:
mov r8, rcx
mov rdx, rax
mov rcx, error@@OutOfBounds
call error@@
_InBounds@@@84:
mov rcx, 20
mul rcx
lea rsi, [rax+rbx]
sub rsp, 32
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
_endif@@@81:
call gcclear@@
jmp _forstep@@@74
_forend@@@73:
add rsp, 16
call gcclear@@
jmp _forstep@@@69
_forend@@@68:
add rsp, 16
sub rsp, 0
sub rsp, 32
call EndDrawing
add rsp, 32
add rsp, 0
call gcclear@@
jmp _whiledo@@@62
_whileend@@@61:
call gcclear@@
mov rsp, rbp
pop rbp
ret
