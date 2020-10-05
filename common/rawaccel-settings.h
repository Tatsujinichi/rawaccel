#pragma once

#include "vec2.h"

namespace rawaccel {
    using milliseconds = double;

    inline constexpr milliseconds WRITE_DELAY = 1000;
    inline constexpr milliseconds DEFAULT_TIME_MIN = 0.4;

    struct gain {
        enum class mode {
            tanh, gd, erf, clamp, softplus
        };
        struct implementations;
        struct function;
        using lookup_value_t = double;
    };

    struct accel_args {
        double motivity = 2;
        double synchronous_speed = 10;
        double gamma = 1;
        double hard_cap = 0;
    };

    struct settings {
        double degrees_rotation = 0;
        bool apply_accel = false;
        bool combine_mags = true;
        vec2<gain::mode> modes = {};
        vec2<accel_args> argsv;
        vec2d sens = { 1, 1 };
        double time_min = DEFAULT_TIME_MIN;
    };

}
