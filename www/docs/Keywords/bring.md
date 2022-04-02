# bring (Keyword)
The `bring` keyword allows you to use other scripts implemented in the project or inside the base Neptyne library. The `bring` keyword is used to form a `bring` statement with adding a library or a scripts location after the keyword. When using the `bring` keyword the compiler copies the contents of a script into the line the statement is declared.

This example brings a base Neptyne library `System.Text`.

```npt
bring System.Text;
```

And the following example brings a script implemented in the project.

```npt
bring "Example.npt";
```

The `bring` statement can appear in any source code file and outside any declarations. Otherwise, compiler error [NPT1101](../Compiler%20Errors/NPT1101) is generated.