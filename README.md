![LogoGithub](https://user-images.githubusercontent.com/29477753/157120167-e1b37d10-16be-4cf6-ab35-11352780bce3.svg)

Neptyne is a simple programming language created just for fun and learning the compile process. The Neptyne compiler compiles Neptyne code to Assembly language.

### NOTICE: THIS PROJECT IS UNDER DEVELOPMENT AND CANNOT RUN ANY KIND OF COMPLEX CODE YET
#### The example below is just a concept and may be changed over time
#### TODO list: https://github.com/Marakusa/neptyne/projects/1

## Syntax example
The syntax is a bit inspired from C and Python mixed.
```c
// This will include a script file here
bring System;

void main: {
    string hello = "Hello world!";

    // This will print out the 'hello' variable to the console
    out(hello);

    int n = 7;
    
    n = doMath(n);
    
    out("The number is now %d!", n);
}

int doMath(int n): {
    if n == 7: {
        n++;
    }
    else: {
        out("The number wasn't 7...");
    }

    while n > 2: {
        n--;
    }
    
    return n;
}
```
