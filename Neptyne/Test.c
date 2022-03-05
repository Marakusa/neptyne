#include <stdio.h>
int out(const char * format, ...) {
    printf(format);
    printf("%c", 10);
    return 1;
}

char * in(const char * format, ...) {
    printf(format);
    int size = sizeof(char) * 1024;
    char *inputValue[1];
    return fgets (inputValue, size, stdin);;
}

long len(const char * s) {
    long length = sizeof(s);
    return length;
}

int main() {
    out("Hello world!");
    char *name = in("Please enter your name: ");
    out("Hello %s!", name);
    return 1;
}
