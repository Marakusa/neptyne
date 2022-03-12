cmake_minimum_required(VERSION 3.21)
project(neptyne)

set(CMAKE_CXX_STANDARD 17)

add_executable(neptyne src/main.cpp src/builder.cpp src/builder.h src/logger.cpp src/logger.h src/compiler/compiler.cpp src/compiler/compiler.h src/utils/file_utils.h src/compiler/models/NeptyneScript.h src/compiler/tokenizer.cpp src/compiler/tokenizer.h src/common_includes.h src/compiler/models/Token.h src/compiler/models/TokenType.h src/utils/string_utils.h src/compiler/models/CompilerException.h src/utils/string_utils.cpp src/utils/file_utils.cpp src/compiler/compiler_errors.cpp src/compiler/compiler_errors.h src/compiler/models/CompilerErrorInfo.h src/compiler/parser.cpp src/compiler/parser.h src/compiler/models/ParserToken.h)
