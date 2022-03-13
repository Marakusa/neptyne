# alias (Keyword)
The `alias` keyword allows you to create an alias for the compiler. It can be used to make your code easier to read and manage. For example a function from another script or a library can be assigned into an `alias`.

This example assigns a function from a library into an `alias`.

```npt
bring System.Text;

// string.format function from `System.Text` library
alias format = string.format;

void main: {

    int sum = 4 + 12;

    // Instead of calling this
    string a = string.format("%d", sum);

    // You can call it with an alias
    string b = format("%d", sum);

}
```

The `alias` keyword can also be used for types.

```npt
alias int = INTEGER;

void main: {

    // Function with the same type `int` assigned with an alias `INTEGER`
    int a = addFunc(2, 4);

    // An alias `INTEGER` assigned for a value of a type `int`
    INTEGER b = 10;

    // A variable of a type of the alias `INTEGER` added to a variable of a type `int`
    b = a + b;

}

// Function of a type of the alias `INTEGER`
INTEGER addFunc(int a, int b) {
    return a + b;
}

```
