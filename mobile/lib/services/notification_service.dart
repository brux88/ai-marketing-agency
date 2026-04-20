import 'dart:io';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import '../api/api_client.dart';

/// Background message handler must be a top-level function.
@pragma('vm:entry-point')
Future<void> _firebaseBackgroundHandler(RemoteMessage message) async {
  // Background isolate: nothing to do beyond letting FCM show the system notif.
}

/// Handles Firebase init, FCM token registration with the backend, and local
/// notification display for foreground messages.
///
/// Requires google-services.json (Android) and GoogleService-Info.plist (iOS).
/// Without those, Firebase init fails softly and push is disabled — the app
/// still works.
class NotificationService {
  static final NotificationService _instance = NotificationService._();
  factory NotificationService() => _instance;
  NotificationService._();

  final FlutterLocalNotificationsPlugin _local =
      FlutterLocalNotificationsPlugin();

  bool _initialized = false;
  bool _firebaseReady = false;
  String? _registeredToken;

  bool get isFirebaseReady => _firebaseReady;

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

    if (Platform.isAndroid) {
      await _local
          .resolvePlatformSpecificImplementation<
              AndroidFlutterLocalNotificationsPlugin>()
          ?.requestNotificationsPermission();
    }

    try {
      await Firebase.initializeApp();
      _firebaseReady = true;
      FirebaseMessaging.onBackgroundMessage(_firebaseBackgroundHandler);
      FirebaseMessaging.onMessage.listen(_handleForegroundMessage);
      FirebaseMessaging.instance.onTokenRefresh.listen(_handleTokenRefresh);
    } catch (e) {
      debugPrint('Firebase init skipped (missing config?): $e');
      _firebaseReady = false;
    }
  }

  /// Request notification permission and register the FCM token with the
  /// backend. Call this after a successful login.
  Future<void> registerAfterLogin() async {
    if (!_firebaseReady) return;
    try {
      final messaging = FirebaseMessaging.instance;
      await messaging.requestPermission(alert: true, badge: true, sound: true);
      final token = await messaging.getToken();
      if (token == null || token.isEmpty) return;
      await _sendTokenToBackend(token);
    } catch (e) {
      debugPrint('FCM register error: $e');
    }
  }

  /// Unregister the current token. Call this before logging out.
  Future<void> unregisterBeforeLogout() async {
    if (!_firebaseReady) return;
    final token = _registeredToken;
    if (token == null) return;
    try {
      await ApiClient.post('/api/v1/devices/unregister', {'fcmToken': token});
    } catch (e) {
      debugPrint('FCM unregister error: $e');
    } finally {
      _registeredToken = null;
    }
  }

  Future<void> _sendTokenToBackend(String token) async {
    try {
      await ApiClient.post('/api/v1/devices/register', {
        'fcmToken': token,
        'platform': Platform.isIOS ? 'ios' : 'android',
        'deviceName': Platform.operatingSystemVersion,
      });
      _registeredToken = token;
    } catch (e) {
      debugPrint('FCM register backend error: $e');
    }
  }

  Future<void> _handleTokenRefresh(String token) async {
    if (token == _registeredToken) return;
    await _sendTokenToBackend(token);
  }

  Future<void> _handleForegroundMessage(RemoteMessage message) async {
    final title = message.notification?.title ?? 'weposteai';
    final body = message.notification?.body ?? '';
    await showLocalNotification(
      id: message.messageId?.hashCode ?? DateTime.now().millisecondsSinceEpoch,
      title: title,
      body: body,
    );
  }

  Future<void> showLocalNotification({
    required int id,
    required String title,
    String? body,
  }) async {
    const androidDetails = AndroidNotificationDetails(
      'weposteai_channel',
      'weposteai',
      channelDescription: 'weposteai notifications',
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
}
