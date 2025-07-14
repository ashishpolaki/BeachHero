By SOLO :)
This system is related to MOST IN ONE Package
Check https://assetstore.unity.com/packages/slug/295013

Documentation and API https://solo-player.gitbook.io/most-in-one/most-systems/most-haptic-feedback

If you have any questions about MOST IN ONE,
Feel free to reach out!
soloplayerdev101@gmail.com

____________________________________________________


 The baisc haptics List names are exactly the same as the iOS UIToolKit (limited only to these feedbacks)
 + i have used these names on android as well to make an eqv haptics for android (default-like haptics)...

 also using iOS Haptics, i have created ready to use haptics for android and give it Pattern and amplitudes (what android haptic system uses)
 check Most_HapticFeedback - IOSDefaultHapticsToAndroidPatterns() - line 84 if you want to modify these default values

____________________________________________________


 Haptic With Cooldown is very useful on continuous haptic spamming, for example... Sliders, continuous damage...
 the cooldown value you will add will disable haptic feedback only from 'Haptic With Cooldown' function

____________________________________________________


 You can't spam Haptics according to iOS API, this will leading to ignore haptic calls and send error message, that's why "Haptic With Cooldown" system :)

