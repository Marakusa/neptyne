cmake_minimum_required(VERSION 3.21)
project(neptyne)

set(CMAKE_CXX_STANDARD 17)

add_executable(neptyne src/main.cpp src/builder.cpp src/builder.h src/logger.cpp src/logger.h)
