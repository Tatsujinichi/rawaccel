#pragma once

#include <type_traits>

#include <rawaccel.hpp>

#include "wrapper_io.hpp"

using namespace System;
using namespace System::Runtime::InteropServices;

using namespace Newtonsoft::Json;

[JsonConverter(Converters::StringEnumConverter::typeid)]
public enum class GainMode
{
    tanh, gd, erf, clamp, softplus
};

[JsonObject(ItemRequired = Required::Always)]
[StructLayout(LayoutKind::Sequential)]
public value struct AccelArgs
{
    double motivity;
    double synchronousSpeed;
    double gamma;
    [JsonProperty("legacyCap")]
    double scaleCap;
};

generic <typename T>
[JsonObject(ItemRequired = Required::Always)]
[StructLayout(LayoutKind::Sequential)]
public value struct Vec2
{
    T x;
    T y;
};

[JsonObject(ItemRequired = Required::Always)]
[StructLayout(LayoutKind::Sequential)]
public ref struct DriverSettings
{
    literal String^ Key = "Driver settings";

    [JsonProperty("Degrees of rotation")]
    double rotation;

    [MarshalAs(UnmanagedType::U1)]
    bool applyAccel;

    [JsonProperty("Use x as whole/combined accel")]
    [MarshalAs(UnmanagedType::U1)]
    bool combineMagnitudes;

    [JsonProperty("Gain function modes")]
    Vec2<GainMode> modes;

    [JsonProperty("Accel parameters")]
    Vec2<AccelArgs> args;

    [JsonProperty("Sensitivity multipliers")]
    Vec2<double> sensitivity;
    
    [JsonProperty(Required = Required::Default)]
    double minimumTime;

    bool ShouldSerializeminimumTime() 
    { 
        return minimumTime > 0 && minimumTime != DEFAULT_TIME_MIN;
    }
};


template <typename NativeSettingsFunc>
void as_native(DriverSettings^ managed_args, NativeSettingsFunc fn)
{
#ifndef NDEBUG
    if (Marshal::SizeOf(managed_args) != sizeof(settings))
        throw gcnew InvalidOperationException("setting sizes differ");
#endif
    settings args;
    Marshal::StructureToPtr(managed_args, (IntPtr)&args, false);
    fn(args);
    if constexpr (!std::is_invocable_v<NativeSettingsFunc, const settings&>) {
        Marshal::PtrToStructure((IntPtr)&args, managed_args);
    }
}

DriverSettings^ get_default()
{
    DriverSettings^ managed = gcnew DriverSettings();
    as_native(managed, [](settings& args) {
        args = {};
    });
    return managed;
}

void set_active(DriverSettings^ managed)
{
    as_native(managed, [](const settings& args) {
        wrapper_io::writeToDriver(args);
    });
}

DriverSettings^ get_active()
{
    DriverSettings^ managed = gcnew DriverSettings();
    as_native(managed, [](settings& args) {
        wrapper_io::readFromDriver(args);
    });
    return managed;
}

void update_modifier(mouse_modifier& mod, DriverSettings^ managed)
{
    as_native(managed, [&](const settings& args) {
        mod = { args };
    });
}

using error_list_t = Collections::Generic::List<String^>;

error_list_t^ get_accel_errors(GainMode mode, AccelArgs^ args)
{
    gain::mode m = (gain::mode)mode;

    auto is_mode = [m](auto... modes) { return ((m == modes) || ...); };
    
    using gm = gain::mode;

    auto error_list = gcnew error_list_t();
    
    if (args->gamma == 0)
        error_list->Add("gamma can not be 0");
    if (args->motivity <= 1)
        error_list->Add("motivity must be greater than 1");
    if (args->synchronousSpeed <= 0) {
        error_list->Add("synchronous speed must be positive");
    }

    return error_list;
}

public ref class SettingsErrors
{
public:
    error_list_t^ x;
    error_list_t^ y;

    bool Empty()
    {
        return x->Count == 0 && y->Count == 0;
    }
};

public ref struct DriverInterop
{
    literal double WriteDelayMs = WRITE_DELAY;
    static initonly DriverSettings^ DefaultSettings = get_default();

    static DriverSettings^ GetActiveSettings()
    {
        return get_active();
    }

    static void Write(DriverSettings^ args)
    {
        set_active(args);
    }

    static DriverSettings^ GetDefaultSettings()
    {
        return get_default();
    }

    static SettingsErrors^ GetSettingsErrors(DriverSettings^ args)
    {
        auto errors = gcnew SettingsErrors();

        errors->x = get_accel_errors(args->modes.x, args->args.x);

        if (args->combineMagnitudes) errors->y = gcnew error_list_t();
        else errors->y = get_accel_errors(args->modes.y, args->args.y);

        return errors;
    }

    static error_list_t^ GetAccelErrors(GainMode mode, AccelArgs^ args)
    {
        return get_accel_errors(mode, args);
    }
};

public ref class ManagedAccel
{
    mouse_modifier* const modifier_instance = new mouse_modifier();

public:

    virtual ~ManagedAccel()
    {
        delete modifier_instance;
    }

    !ManagedAccel()
    {
        delete modifier_instance;
    }

    Tuple<double, double>^ Accelerate(int x, int y, double time)
    {
        vec2d in_out_vec = {
            (double)x,
            (double)y
        };
        modifier_instance->modify(in_out_vec, time);

        return gcnew Tuple<double, double>(in_out_vec.x, in_out_vec.y);
    }

    void UpdateFromSettings(DriverSettings^ args)
    {
        update_modifier(
            *modifier_instance, 
            args
        );
    }

    static ManagedAccel^ GetActiveAccel()
    {
        settings args;
        wrapper_io::readFromDriver(args);

        auto active = gcnew ManagedAccel();
        *active->modifier_instance = { 
            args
        };
        return active;
    }
};
