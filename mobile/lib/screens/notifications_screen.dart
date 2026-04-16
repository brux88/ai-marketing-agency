import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:timeago/timeago.dart' as timeago;
import '../api/api_client.dart';
import '../models.dart';
import '../services/app_state.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});
  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  late Future<_NotificationResult> _future;

  @override
  void initState() {
    super.initState();
    timeago.setLocaleMessages('it', timeago.ItMessages());
    _future = _load();
  }

  Future<_NotificationResult> _load() async {
    final res = await ApiClient.get('/api/v1/notifications?take=100');
    final data = res['data'];
    final items = (data['items'] as List)
        .map((j) => AppNotification.fromJson(j))
        .toList();
    final unreadCount = data['unreadCount'] as int? ?? 0;

    // Update global badge
    if (mounted) {
      context.read<AppState>().setUnreadCount(unreadCount);
    }

    return _NotificationResult(items: items, unreadCount: unreadCount);
  }

  Future<void> _markRead(AppNotification n) async {
    if (n.read) return;
    try {
      await ApiClient.post('/api/v1/notifications/${n.id}/read');
      if (mounted) {
        context.read<AppState>().decrementUnread();
        setState(() => _future = _load());
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Errore: $e')));
      }
    }
  }

  Future<void> _markAllRead() async {
    try {
      await ApiClient.post('/api/v1/notifications/read-all');
      if (mounted) {
        context.read<AppState>().setUnreadCount(0);
        setState(() => _future = _load());
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Errore: $e')));
      }
    }
  }

  IconData _typeIcon(String type) {
    final t = type.toLowerCase();
    if (t.contains('approval') || t.contains('review')) {
      return Icons.approval;
    }
    if (t.contains('publish')) return Icons.public;
    if (t.contains('generat')) return Icons.auto_awesome;
    if (t.contains('error') || t.contains('fail')) return Icons.error_outline;
    if (t.contains('schedule')) return Icons.schedule;
    return Icons.notifications_outlined;
  }

  Color _typeColor(String type, ColorScheme cs) {
    final t = type.toLowerCase();
    if (t.contains('error') || t.contains('fail')) return cs.error;
    if (t.contains('approval') || t.contains('review')) return Colors.orange;
    if (t.contains('publish')) return Colors.green;
    return cs.primary;
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: const Text('Notifiche'),
        centerTitle: false,
        actions: [
          TextButton.icon(
            icon: const Icon(Icons.done_all, size: 18),
            label: const Text('Segna tutte'),
            onPressed: _markAllRead,
          ),
        ],
      ),
      body: FutureBuilder<_NotificationResult>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.error_outline,
                      size: 48, color: Colors.red),
                  const SizedBox(height: 16),
                  Text('Errore: ${snap.error}',
                      textAlign: TextAlign.center),
                  const SizedBox(height: 16),
                  FilledButton.tonal(
                    onPressed: () => setState(() => _future = _load()),
                    child: const Text('Riprova'),
                  ),
                ],
              ),
            );
          }
          final items = snap.data?.items ?? [];
          if (items.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.notifications_none,
                      size: 64,
                      color: cs.onSurfaceVariant.withValues(alpha: 0.5)),
                  const SizedBox(height: 16),
                  Text('Nessuna notifica',
                      style: Theme.of(context)
                          .textTheme
                          .titleMedium
                          ?.copyWith(color: cs.onSurfaceVariant)),
                ],
              ),
            );
          }
          return RefreshIndicator(
            onRefresh: () async => setState(() => _future = _load()),
            child: ListView.separated(
              padding: const EdgeInsets.symmetric(vertical: 8),
              itemCount: items.length,
              separatorBuilder: (context, index) =>
                  const Divider(height: 1, indent: 72),
              itemBuilder: (_, i) {
                final n = items[i];
                final color = _typeColor(n.type, cs);
                return ListTile(
                  leading: Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: color.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child:
                        Icon(_typeIcon(n.type), color: color, size: 22),
                  ),
                  title: Text(n.title,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(
                          fontWeight:
                              n.read ? FontWeight.normal : FontWeight.w600)),
                  subtitle: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      if (n.body != null && n.body!.isNotEmpty)
                        Text(n.body!,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                            style: Theme.of(context)
                                .textTheme
                                .bodySmall
                                ?.copyWith(color: cs.onSurfaceVariant)),
                      const SizedBox(height: 4),
                      Text(
                        timeago.format(n.createdAt, locale: 'it'),
                        style: Theme.of(context)
                            .textTheme
                            .bodySmall
                            ?.copyWith(
                                color: cs.onSurfaceVariant,
                                fontSize: 11),
                      ),
                    ],
                  ),
                  trailing: !n.read
                      ? Container(
                          width: 10,
                          height: 10,
                          decoration: BoxDecoration(
                            color: cs.primary,
                            shape: BoxShape.circle,
                          ),
                        )
                      : null,
                  tileColor:
                      n.read ? null : cs.primaryContainer.withValues(alpha: 0.15),
                  onTap: () => _markRead(n),
                );
              },
            ),
          );
        },
      ),
    );
  }
}

class _NotificationResult {
  final List<AppNotification> items;
  final int unreadCount;
  _NotificationResult({required this.items, required this.unreadCount});
}
