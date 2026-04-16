import 'package:flutter/foundation.dart';
import '../api/api_client.dart';

/// Global app state using ChangeNotifier for notification badge count
class AppState extends ChangeNotifier {
  int _unreadNotificationCount = 0;
  int get unreadNotificationCount => _unreadNotificationCount;

  Future<void> fetchUnreadCount() async {
    try {
      final res =
          await ApiClient.get('/api/v1/notifications?unreadOnly=true&take=1');
      final data = res['data'];
      _unreadNotificationCount = data['unreadCount'] ?? 0;
      notifyListeners();
    } catch (e) {
      debugPrint('Error fetching unread count: $e');
    }
  }

  void setUnreadCount(int count) {
    _unreadNotificationCount = count;
    notifyListeners();
  }

  void decrementUnread() {
    if (_unreadNotificationCount > 0) {
      _unreadNotificationCount--;
      notifyListeners();
    }
  }
}
