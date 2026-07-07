var VibrationPlugin = {
	
    Vibrate: function(duration) {
        if (navigator.vibrate) {
            navigator.vibrate(duration);
        }
    },
    
    GamepadVibrate: function (duration, weak, strong)
    {
        if (typeof navigator === "undefined" || !navigator.getGamepads)
            return;

        var gamepads = navigator.getGamepads();

        for (var i = 0; i < gamepads.length; i++)
        {
            var gp = gamepads[i];
            if (!gp) continue;

            if (gp.vibrationActuator && gp.vibrationActuator.playEffect)
            {
                gp.vibrationActuator.playEffect("dual-rumble", {
                    duration: duration,
                    weakMagnitude: weak,
                    strongMagnitude: strong
                });
            }
        }
    }

};

mergeInto(LibraryManager.library, VibrationPlugin);