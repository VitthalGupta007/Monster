#import <Foundation/Foundation.h>
#import <CoreHaptics/CoreHaptics.h>
#import "Vibration.h"

API_AVAILABLE(ios(13.0))
static CHHapticEngine *hapticEngine = nil;

extern "C"
{
    void Initialize()
	{
		if (@available(iOS 13.0, *)) 
		{
			NSError *error = nil;
			hapticEngine = [[CHHapticEngine alloc] initAndReturnError:&error];
			if (error) {
				NSLog(@"Error creating haptic engine: %@", error);
				return;
			}
			[hapticEngine startAndReturnError:&error];
			if (error) {
				NSLog(@"Error starting haptic engine: %@", error);
				return;
			}
		} 
		else 
		{
			NSLog(@"Haptic feedback not supported on this iOS version.");
		}
	}
	
	void Play(float duration, float intensityValue)
	{
		if (@available(iOS 13.0, *)) // Check if the device is running iOS 13.0 or later, as Core Haptics is available only on iOS 13+.
		{  
			NSError *error = nil;  // Declare an NSError object to capture any errors that may occur.

			// Create a haptic intensity parameter
			CHHapticEventParameter *intensity = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:intensityValue];

			// Create a haptic sharpness parameter
			CHHapticEventParameter *sharpness = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:0.0];

			// Create a continuous haptic event that uses the intensity and sharpness parameters.
			CHHapticEvent *event = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticContinuous parameters:@[intensity, sharpness] relativeTime:0 duration:duration];

			// Create a haptic pattern using the event created above. This pattern can be played by the haptic engine.
			CHHapticPattern *pattern = [[CHHapticPattern alloc] initWithEvents:@[event] parameters:@[] error:&error];
			if (error) {  // Check if there was an error when creating the haptic pattern.
				NSLog(@"Error creating haptic pattern: %@", error);  // Log the error message.
				return;  // Exit the function if there was an error.
			}

			// Create a pattern player using the pattern created above. This player will handle playing the haptic pattern.
			id<CHHapticPatternPlayer> player = [hapticEngine createPlayerWithPattern:pattern error:&error];
			if (error) {  // Check if there was an error when creating the pattern player.
				NSLog(@"Error creating haptic player: %@", error);  // Log the error message.
				return;  // Exit the function if there was an error.
			}

			// Start playing the haptic pattern at time 0.
			[player startAtTime:0 error:&error];
			if (error) {  // Check if there was an error when starting the player.
				NSLog(@"Error playing haptic: %@", error);  // Log the error message.
				return;  // Exit the function if there was an error.
			}
		} 
		else 
		{
			NSLog(@"Haptic feedback not supported on this iOS version.");  // Log a message if the device is running an unsupported iOS version.
		}
	}
}
