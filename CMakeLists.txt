cmake_minimum_required(VERSION 3.21)
project(neptyne)

set(CMAKE_CXX_STANDARD 17)

add_executable(neptyne src/main.cpp src/builder.cpp src/builder.h src/logger.cpp src/logger.h src/compiler/compiler.cpp src/compiler/compiler.h src/utils/fileutils.h src/compiler/NeptyneScript.h src/compiler/tokenizer.cpp src/compiler/tokenizer.h src/common_includes.h src/compiler/Token.h src/compiler/TokenType.h)
