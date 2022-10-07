//
// Created by Markus Kannisto on 09/03/2022.
//

#pragma once

#include <iostream>
#include <cstring>
#include <stdarg.h>

using namespace std;

const char *formatStrings(const char *format, ...) {
    va_list args;
    va_start(args, format);
    char *buffer = new char[1024];
    vsprintf(buffer, format, args);
    va_end(args);
    return buffer;
}

void LogDebug(const char *format, ...) {
    const char *formattedMessage = formatStrings(format);
	printf("%.*s", 8, "DEBUG | ");
	printf(formattedMessage);
	cout << endl;
}
void LogInfo(const char *format, ...) {
    const char *formattedMessage = formatStrings(format);
	printf("%.*s", 7, "INFO | ");
	printf(formattedMessage);
	cout << endl;
}
void LogWarning(const char *format, ...) {
    const char *formattedMessage = formatStrings(format);
	printf("%.*s", 7, "WARN | ");
	printf(formattedMessage);
	cout << endl;
}
void LogError(const char *format, ...) {
    const char *formattedMessage = formatStrings(format);
	printf("%.*s", 8, "ERROR | ");
	printf(formattedMessage);
	cout << endl;
}
void LogCritical(const char *format, ...) {
    const char *formattedMessage = formatStrings(format);
	printf("%.*s", 11, "CRITICAL | ");
	printf(formattedMessage);
	cout << endl;
}
