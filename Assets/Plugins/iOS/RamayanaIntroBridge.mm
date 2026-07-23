// RamayanaIntroBridge.mm
// Round 28 — iOS native bridge for VerseIntroTTS using AVSpeechSynthesizer.

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>

@interface RamayanaIntroSpeaker : NSObject <AVSpeechSynthesizerDelegate>
@property (nonatomic, strong) AVSpeechSynthesizer *synth;
@end

@implementation RamayanaIntroSpeaker
+ (instancetype)shared {
    static RamayanaIntroSpeaker *s = nil;
    static dispatch_once_t once;
    dispatch_once(&once, ^{ s = [[RamayanaIntroSpeaker alloc] init]; });
    return s;
}
- (instancetype)init {
    if ((self = [super init])) {
        _synth = [[AVSpeechSynthesizer alloc] init];
        _synth.delegate = self;
    }
    return self;
}
@end

extern "C" {
    void _rama_intro_speak(const char *text) {
        if (text == NULL) return;
        NSString *s = [NSString stringWithUTF8String:text];
        dispatch_async(dispatch_get_main_queue(), ^{
            [[RamayanaIntroSpeaker shared].synth stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
            AVSpeechUtterance *u = [AVSpeechUtterance speechUtteranceWithString:s];
            u.voice = [AVSpeechSynthesisVoice voiceWithLanguage:@"en-US"];
            u.rate = AVSpeechUtteranceDefaultSpeechRate * 0.95f;
            u.pitchMultiplier = 0.95f;
            u.volume = 1.0f;
            [[RamayanaIntroSpeaker shared].synth speakUtterance:u];
        });
    }
}
