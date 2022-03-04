int out(string format, ...) {
    printf(format, ...);
    return 1;
}

int main() {
    string msg = "test";
    out(msg);
    return 1;
}
