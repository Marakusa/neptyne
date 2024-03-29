cmake_minimum_required(VERSION 3.21)
project(neptyne)

set(CMAKE_CXX_STANDARD 17)

set(CMAKE_EXE_LINKER_FLAGS " -static-libgcc -static-libstdc++ -static")

add_executable(
        neptyne
        src/main.cpp
        src/builder/builder.cpp
        src/builder/builder.h
        src/logger/logger.h
        src/compiler/nptc_compiler/compiler.cpp
        src/compiler/nptc_compiler/compiler.h
        src/utils/file_utils.h
        src/compiler/nptc_compiler/models/NeptyneScript.h
        src/compiler/nptc_lexer/lexer.cpp
        src/compiler/nptc_lexer/lexer.h
        src/common/common.h
        src/compiler/nptc_lexer/models/Token.h
        src/compiler/nptc_lexer/models/TokenType.h
        src/utils/string_utils.h
        src/compiler/nptc_compiler_errors/models/CompilerException.h
        src/utils/string_utils.cpp
        src/utils/file_utils.cpp
        src/compiler/nptc_compiler_errors/compiler_errors.cpp
        src/compiler/nptc_compiler_errors/compiler_errors.h
        src/compiler/nptc_compiler_errors/models/CompilerErrorInfo.h
        src/compiler/nptc_parser/parser.cpp
        src/compiler/nptc_parser/parser.h
        src/compiler/nptc_parser/models/ParserToken.h
        src/compiler/nptc_asm/models/AssemblyScope.h
        src/compiler/nptc_asm/models/AssemblyScript.h
        src/compiler/nptc_asm/models/AssemblyFunction.h
        src/compiler/nptc_asm/models/AssemblyStatement.h
        src/compiler/nptc_asm/models/AssemblyVariable.h
        src/compiler/nptc_project/project_parser.cpp
        src/compiler/nptc_project/project_parser.h
        src/compiler/nptc_compiler/models/NeptyneProject.h src/compiler/nptc_bring/bring_libraries.cpp src/compiler/nptc_bring/bring_libraries.h)

file(COPY vendor DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
file(COPY neptyne/lib DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
