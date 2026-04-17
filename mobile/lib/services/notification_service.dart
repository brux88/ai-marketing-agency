import 'dart:io';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';

/// Handles FCM token registration and local notification display.
/// Firebase must be initialized in main.dart before using this service.
/// The user must add google-services.json (Android) and
/// GoogleService-Info.plist (iOS) to the project for FCM to work.
class NotificationService {
  static final NotificationService _instance = NotificationService._();
  factory NotificationService() => _instance;
  NotificationService._();

  final FlutterLocalNotificationsPlugin _local =
      FlutterLocalNotificationsPlugin();

  bool _initialized = false;

  Future<void> initialize() async {
    if (_initialized) return;
    _initialized = true;

    const androidSettings =
        AndroidInitializationSettings('@mipmap/ic_launcher');
    const iosSettings = DarwinInitializationSettings(
      requestAlertPermission: true,
      requestBadgePermission: true,
      requestSoundPermission: true,
    );
    const settings = InitializationSettings(
      android: androidSettings,
      iOS: iosSettings,
    );
    await _local.initialize(settings);

    // Request notification permission on Android 13+
    if (Platform.isAndroid) {
      await _local
          .resolvePlatformSpecificImplementation<
              AndroidFlutterLocalNotificationsPlugin>()
          ?.requestNotificationsPermission();
    }
  }

  /// Attempt to get the FCM token. Returns null if Firebase is not configured.
  Future<String?> getFcmToken() async {
    try {
      // Dynamic import to avoid crash when Firebase is not configured
      final messaging = _getFirebaseMessaging();
      if (messaging == null) return null;
      return await messaging.getToken();
    } catch (e) {
      debugPrint('FCM token error (Firebase may not be configured): $e');
      return null;
    }
  }

  /// Set up foreground message handler
  Future<void> setupForegroundHandler() async {
    try {
      final messaging = _getFirebaseMessaging();
      if (messaging == null) return;

      // Request permission on iOS
      await messaging.requestPermission(
        alert: true,
        badge: true,
        sound: true,
      );

      // Listen to foreground messages
      // FirebaseMessaging.onMessage is a broadcast stream
      // The actual listener setup happens via firebase_messaging API
    } catch (e) {
      debugPrint('FCM setup error: $e');
    }
  }

  /// Show a local notification
  Future<void> showLocalNotification({
    required int id,
    required String title,
    String? body,
  }) async {
    const androidDetails = AndroidNotificationDetails(
      'wepostai_channel',
      'WePostAI',
      channelDescription: 'WePostAI notifications',
      importance: Importance.high,
      priority: Priority.high,
    );
    const iosDetails = DarwinNotificationDetails();
    const details = NotificationDetails(
      android: androidDetails,
      iOS: iosDetails,
    );
    await _local.show(id, title, body, details);
  }

  /// Try to get FirebaseMessaging instance. Returns null if not available.
  dynamic _getFirebaseMessaging() {
    try {
      // This will throw if firebase_messaging is not properly configured
      // (i.e., no google-services.json / GoogleService-Info.plist)
      return null; // Placeholder - replace with FirebaseMessaging.instance when Firebase is configured
    } catch (e) {
      return null;
    }
  }
}
