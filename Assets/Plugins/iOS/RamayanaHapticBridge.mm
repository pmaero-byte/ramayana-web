// RamayanaHapticBridge.mm
// Round 25 — iOS native bridge for VerseHapticFeedback.
// Calls UIImpactFeedbackGenerator / UINotificationFeedbackGenerator on the main thread.

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern "C" {
    void _rama_impact(int style) {
        dispatch_async(dispatch_get_main_queue(), ^{
            UIImpactFeedbackStyle s = UIImpactFeedbackStyleMedium;
            switch (style) {
                case 0: s = UIImpactFeedbackStyleLight; break;
                case 1: s = UIImpactFeedbackStyleMedium; break;
                case 2: s = UIImpactFeedbackStyleHeavy; break;
                case 3: s = UIImpactFeedbackStyleSoft; break;
                case 4: s = UIImpactFeedbackStyleRigid; break;
            }
            UIImpactFeedbackGenerator *gen = [[UIImpactFeedbackGenerator alloc] initWithStyle:s];
            [gen prepare];
            [gen impactOccurred];
        });
    }

    void _rama_notify(int kind) {
        dispatch_async(dispatch_get_main_queue(), ^{
            UINotificationFeedbackType t = UINotificationFeedbackTypeSuccess;
            switch (kind) {
                case 0: t = UINotificationFeedbackTypeSuccess; break;
                case 1: t = UINotificationFeedbackTypeWarning; break;
                case 2: t = UINotificationFeedbackTypeError; break;
            }
            UINotificationFeedbackGenerator *gen = [[UINotificationFeedbackGenerator alloc] init];
            [gen prepare];
            [gen notificationOccurred:t];
        });
    }
}
