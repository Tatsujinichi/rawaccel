#pragma once

#include <rawaccel-settings.h>

using namespace rawaccel;
using namespace System;

using w32_bool = int;

struct wrapper_io {
	static void writeToDriver(const settings&);
	static void readFromDriver(settings&);
	static void sendLastInExtra(w32_bool enable);
};

public ref struct DriverIOException : public IO::IOException {
public:
	DriverIOException() {}
	DriverIOException(String^ what) : IO::IOException(what) {}
};

public ref struct DriverNotInstalledException : public DriverIOException {};
