# Borzoi

## General info

Borzoi is a toy programming language I've created to learn about compilers
(spoiler: I did learn a bit). It is a C-like general purpose low level language, 
featuring a frame-based garbage collector and lack of semicolons, all while being 
whitespace insensitive. It is compiled to x64 Assembly, and then uses NASM to generate
the object file, which gets linked by GCC (LD)

***

To see the language in action you can view 
https://github.com/KittenLord/minesweeper.bz, but there will also be plenty of
examples in this file.

## Disclaimer

This is an incredibly unstable language with a lot of duct-tape solutions and terrible
error reporting, so it is not at all aimed to be used for serious projects. 
Unless you want to goof around of course lol

## Introduction

*To try out the following examples check the Installation and Usage sections*

Being a really C inspired language, most of the concepts are probably already 
familiar to you

Here is an example of a Hello World program in Borzoi

```cs
cfn print from printf(byte[] format, *) int
fn main() int {
    let int a = 3
    let int b = 8
    print("Hello, World!\n")
    print("%d\n", a + b)
    ret 69
}
```

Apart from the gorgeous absence of semicolons, the most interesting feature you may 
notice is the definition of `print/printf`. Here, we define the type signature of 
`printf` (which is there, because we automatically link with libc), and alias it with 
`print`. You may also not alias it, like this:

```cs
cfn printf(byte[] format, *) int
```

As `fn` denotes a function, `cfn` denotes a C-function, imported from somewhere else. `*`
is simply varargs in this context

Here are a couple more familiar features:

```cs
cfn printf(byte[] fmt, *) int
fn main() {
    let int a = 8
    let int b = 3

    if a > b printf("If statement works!\n")

    for i from 0 until a printf("For loop hopefully works as well #%d\n", i+1)

    while a > b {
        if a == 5 { mut a = a - 1 continue } # sneaky skip
        printf("Despite its best efforts, a is still greater than b\n")
        mut a = a - 1
    }
    
    printf("What a turnaround\n")

    do while a > b 
        printf("This loop will first run its body, and only then check the condition %d > %d\n", a, b)

    while true {
        mut a = a + 1
        if a == 10 break
    }

    printf("After a lot of struggle, a has become %d\n", a)
}
```

Pretty much all of these are self explanatory, except maybe `mut`, which is used to mutate
a variable (it's easier to parse), and the unusual spelling of `do while`, which is this way
because it is objectively the best spelling.

You may have noticed that `main` is missing the return type. No return type is equivalent to
a `void` function, and in Borzoi `main` can return any type you want. The return value is used 
as the exit code for your program, which can be viewed using the `--exit-code` compiler flag

Now, after convincing you that this language has some basic features, let me show you more
interesting examples, which also showcase some interesting stuff

```cs
fn main() {
    let byte[] a = "Hello, "
    let byte[] b = "World!"
    printf(concat(a, b))
}

fn concat(byte[] a, byte[] b) byte[] && {
    let byte[] c = *(a.len + b.len)
    for i from 0 until a.len mut c[i] = a[i]
    for i from a.len until b.len+a.len mut c[i] = b[i-a.len]
    ret c
}

cfn printf(byte[] fmt, *) int
```

First of all, function definition order doesn't matter at all, a feature which all reasonable
languages should have. But you also may have noticed the weird `&&` and `*` operators.

`*` is a unary operator which creates an array with a runtime defined length. I should also
mention, that arrays in Borzoi come with runtime bounds checking

`&&` is more complex than that, and to comprehend it we need to check out how the garbage
collection works

So, because of my skill issues, all arrays (including strings) are heap allocated using `malloc`.
The thing is, what is `malloc`'ed should be `free`'d, and doing that for every single throwaway
string you create is absurd. In order to make Borzoi usable, whenever you create an array, a
pointer to it is stored on the secondary stack, and when you exit the frame, all the arrays in
that frame get `free`'d

```cs
{
    let byte[] a = "asbdaiusuibds" #------------|
    if true { #                                 |
#                                               |
        let byte[] b = "asndisanisnd" #-----|   |
#                                           |   |
        let byte[] c = &"cccccccccccc"#-----|   |
#                                           |   |
                       # b gets free'd here |   |
#                                               |
    }#                                          |
    let byte[] d = "asdbasbidas" # -------------|
#                                               |
                      # a and d get free'd here |
}
```

But sometimes (a lot of the times) you want the array to stay. Marking it with `&` (string `c`
in the above example) makes garbage collector simply ignore it.

But lets get back to the `concat` example. Here neither of these approaches work - we don't want
the result of concatenation to get `free`'d before it even exits the function, but we also *do* 
want it to get `free`'d after it exits the scope of `main` (or any other function calling `concat`). 
We pretty much want the garbage collector to move the result of `concat` into the `main` gc frame, 
or, more simply, to deactivate garbage collector for the duration of `concat`. That is exactly 
what the `&&` operator is doing

```cs
{
    let byte[] a = "asbdauisbd"

    if true && { let byte[] b = "aduasbais" }

    let byte[] c = "adasbdisas"

    let byte[] d = &"dddddddddd"
    collect d # d was removed from garbage collection using &, and put back using collect

    # a, b, c and d all get destroyed here, despite b being declared in a separate block
}
```

That's all regarding the "garbage collection". It may be simple, but I think it is a relatively
interesting feature, albeit kinda primitive. Just don't accidentally double-free anything lol

And the last feature I wanted to talk about - pointers. They are pretty much exactly like in C,
including their unsafety, but with different syntax.

```cs
fn main() {
    let int a = 5

    let int@ ap = @a
    
    let int@@ app = @ap

    let int@ bp = app@ # dereference syntax

    let int b = bp@
    mut b = app@@

    mut ap@ = 3 # a is now 3, b is an old copy, so it remains as 5
}
```

I was thinking that the `@` syntax could be interesting, since we have an intuition for 
"mentioning" someone using `@` symbol, which is a cool analogy to pointers.

Related to this, we have syntax for heap allocation

```cs
fn main() {
    let@ int a = 5 # a is of type int@
    let int b = a@

    # a DOES NOT get freed (cuz why would you heap allocate it then lol)
}
```

Actually, no. Here's one more feature I somehow forgot about - types.

You can define and use a type (basically a C struct) like this:

```cs
type labeledv3 {
    byte[] label,
    int x,
    int y,
    int z
}

cfn printf(byte[] fmt, *) int

fn main() {
    let labeledv3 a = labeledv3!{&"Cool vector", 1, 2, 3}
    let labeledv3 b = labeledv3!{label = &"Cooler vector", x=1, y=2, z=3}
    mut a.x = 4

    let int x = a.x + b.x
    let int y = a.y * b.z

    printf(a.label)
    printf("%d %d\n", x, y)

    let labeledv3@ ap = @a # pointer to a struct
    mut ap@.x = 7 # accessing stuff

    printf("%d %d %d", a.x, ap@.y, a.z)
}
```

Here are also a couple of features that I kinda left out

```cs
link "raylib" # Link with a library
embed "assets/image.png" as sprite # embed files into the executable (accessible globally, "sprite" has type byte[])
embed "text.txt" as text # can be any file type
fn main() {
    let int[] a = [1, 2, 3, 4] 
        # Array literals look pretty (unlike C#'s "new int[] {1, 2, 3}" [I know they added it recently, it's still bad])

    let int[4] b = [1, 2, 3, 4] # Compile-time sized array type
    let int[4] b1 = [] # Can be left uninitialized
    # let int[4] bb = [1, 2, 3] # A compile-time error

    let int num = 5
    let byte by = num->byte # Pretty cast syntax, will help you when type inference inevitably fails you
    let float fl = num->float # Actual conversion occurs
    mut fl = 6.9 # Also floats do exist, yea

    if true and false {}
    if true or false {} # boolean operators, for those wondering about &&

    let void@ arrp = a.ptr # you can access the pointer behind the array if you really want to
        # Though when you pass an array type to a C function it already passes only the pointer
}
```

And that's pretty much it, ayy.

Again, if you want a working example of a (not that big) Borzoi codebase, here is a game I
made in it https://github.com/KittenLord/minesweeper.bz

## Moral of the story

In the end, I really enjoyed the process, and learned a lot about Assembly and ABIs while 
researching the topic (also obviously a lot about programming languages in general). I 
absolutely recommend trying it out, but it definitely requires quite some theory knowledge

My takeaways
- 16 byte alignment is craaaazy
- Print debugging Assembly is not a good idea
- Debugging Assembly in any form is tough actually
- Parsing is trivial, as long as you don't care about recovery (i.e. you hault after the first error)
- You have to plan very seriously if you want an optimizing compiler (this one is not)

What I want to do next
- Try making a functional programming language, hopefully with actual garbage collection this time
- Look into how LSPs, linters and other tools are made
- Try creating a programming language, but not in C# (pretty much any language with sum types would have been better lol)

## Installation

You can clone the source code with

```
git clone https://github.com/KittenLord/borzoi.git
```

And build the compiler from the source using

```
dotnet build
```

Which will result in executable somewhere in the `bin` folder

Maybe I'll add the executable by itself in the Releases later

## Usage

To build your project use
```
borzoi build <...files>
```

If your project consists only of a singular "main.bz" file, you can simply run

```
borzoi build
```

***

To run the project, replace `build` with `run`, which is equivalent to building the
project and then running the executable

***

To view compiler flags/details run

```
borzoi help
```

or

```
borzoi help <subcommand>
```

***

If you want something funny to happen run

```
borzoi
```

## Known issues, TODOs and alike

- Type inferrence sometimes trolls you really badly
- Really unoptimized Assembly
- A lot of unhandled compiler-errors which throw exceptions, cuz I was too lazy
- Support for Linux (I've already read SystemV ABI, but didn't get around to implementing it)
- Protect arrays' `len` and `ptr` from being modified (readonly fields?)
- Replace gcc with ld
- Some errors do not show correct/meaningful locations
- Perhaps change `.bz` to `.brz`?
- Module system and/or file inclusion
