cmake_minimum_required(VERSION 3.21)
project(neptyne)

set(CMAKE_CXX_STANDARD 17)

add_executable(neptyne src/main.cpp src/builder.cpp src/builder.h src/logger.cpp src/logger.h src/compiler/nptc_compiler/compiler.cpp src/compiler/nptc_compiler/compiler.h src/utils/file_utils.h src/compiler/nptc_compiler/models/NeptyneScript.h src/compiler/nptc_tokenizer/tokenizer.cpp src/compiler/nptc_tokenizer/tokenizer.h src/common_includes.h src/compiler/nptc_tokenizer/models/Token.h src/compiler/nptc_tokenizer/models/TokenType.h src/utils/string_utils.h src/compiler/nptc_compiler_errors/models/CompilerException.h src/utils/string_utils.cpp src/utils/file_utils.cpp src/compiler/nptc_compiler_errors/compiler_errors.cpp src/compiler/nptc_compiler_errors/compiler_errors.h src/compiler/nptc_compiler_errors/models/CompilerErrorInfo.h src/compiler/nptc_parser/parser.cpp src/compiler/nptc_parser/parser.h src/compiler/nptc_parser/models/ParserToken.h src/compiler/nptc_asm/models/AssemblyScope.h src/compiler/nptc_asm/models/AssemblyScript.h src/compiler/nptc_asm/models/AssemblyFunction.h src/compiler/nptc_asm/models/AssemblyStatement.h src/compiler/nptc_asm/models/AssemblyVariable.h)
