// By SOLO :)
// Check MOST IN ONE package https://assetstore.unity.com/packages/slug/295013

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern "C"
{
    void MOST_HapticFeedback(int type)
    {
        if (@available(iOS 10.0, *))
        {
            switch (type)
            {
                case 0: // Selection (iOS 10+)
                    {
                        UISelectionFeedbackGenerator* generator = [[UISelectionFeedbackGenerator alloc] init];
                        [generator prepare] ;
                        [generator selectionChanged] ;
                        break;
                    }
                case 1: // Success (iOS 10+)
                    {
                        UINotificationFeedbackGenerator* generator = [[UINotificationFeedbackGenerator alloc] init];
                        [generator prepare] ;
                        [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
                        break;
                    }
                case 2: // Warning (iOS 10+)
                    {
                        UINotificationFeedbackGenerator* generator = [[UINotificationFeedbackGenerator alloc] init];
                        [generator prepare] ;
                        [generator notificationOccurred:UINotificationFeedbackTypeWarning] ;
                        break;
                    }
                case 3: // Failure (iOS 10+)
                    {
                        UINotificationFeedbackGenerator* generator = [[UINotificationFeedbackGenerator alloc] init];
                        [generator prepare] ;
                        [generator notificationOccurred:UINotificationFeedbackTypeError] ;
                        break;
                    }
                case 4: // LightImpact (iOS 10+)
                    {
                        UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: UIImpactFeedbackStyleLight];
                        [generator prepare] ;
                        [generator impactOccurred] ;
                        break;
                    }
                case 5: // MediumImpact (iOS 10+)
                    {
                        UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: UIImpactFeedbackStyleMedium];
                        [generator prepare] ;
                        [generator impactOccurred] ;
                        break;
                    }
                case 6: // HeavyImpact (iOS 10+)
                    {
                        UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: UIImpactFeedbackStyleHeavy];
                        [generator prepare] ;
                        [generator impactOccurred] ;
                        break;
                    }
                case 7: // RigidImpact (iOS 13+)
                    {
                        if (@available(iOS 13.0, *))
                        {
                            UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: UIImpactFeedbackStyleRigid];
                            [generator prepare] ;
                            [generator impactOccurred] ;
                        }
                        break;
                    }
                case 8: // SoftImpact (iOS 13+)
                    {
                        if (@available(iOS 13.0, *))
                        {
                            UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: UIImpactFeedbackStyleSoft];
                            [generator prepare] ;
                            [generator impactOccurred] ;
                        }
                        break;
                    }
            }
        }
    }
}
