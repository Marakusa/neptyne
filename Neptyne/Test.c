#include <stdio.h>
int out(const char * format, ...) {
    printf(format);
    printf("%c", 10);
    return 1;
}

char * in(const char * format, ...) {
    printf(format);
    char *inputValue = "";
    scanf("%s", &inputValue);
    return inputValue;
}

long len(const char * s) {
    long length = sizeof(s);
    return length;
}

int main() {
    out("Hello world!");
    in("Hello world!");
    return 1;
}
