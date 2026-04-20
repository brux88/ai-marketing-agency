import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../api/api_client.dart';
import '../models.dart';

class _CalendarEntry {
  final String id;
  final String contentId;
  final String platform;
  final DateTime scheduledAt;
  final String status;
  final String title;
  final String contentType;
  final String? imageUrl;

  _CalendarEntry({
    required this.id,
    required this.contentId,
    required this.platform,
    required this.scheduledAt,
    required this.status,
    required this.title,
    required this.contentType,
    this.imageUrl,
  });

  factory _CalendarEntry.fromJson(Map<String, dynamic> j) => _CalendarEntry(
        id: j['id'] ?? '',
        contentId: j['contentId'] ?? '',
        platform: j['platform'] ?? j['contentType'] ?? '',
        scheduledAt: DateTime.parse(j['scheduledAt']),
        status: j['status']?.toString() ?? '',
        title: j['contentTitle'] ?? (j['content']?['title'] as String?) ?? '',
        contentType: j['contentType'] ?? '',
        imageUrl: j['content']?['imageUrl'] as String?,
      );
}

class CalendarScreen extends StatefulWidget {
  final Agency agency;
  const CalendarScreen({super.key, required this.agency});
  @override
  State<CalendarScreen> createState() => _CalendarScreenState();
}

class _CalendarScreenState extends State<CalendarScreen> {
  late Future<Map<String, List<_CalendarEntry>>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<Map<String, List<_CalendarEntry>>> _load() async {
    final res = await ApiClient.get(
        '/api/v1/agencies/${widget.agency.id}/calendar');
    final list = (res?['data'] as List?) ?? const [];
    final entries = list
        .map((j) => _CalendarEntry.fromJson(j as Map<String, dynamic>))
        .toList()
      ..sort((a, b) => a.scheduledAt.compareTo(b.scheduledAt));

    final grouped = <String, List<_CalendarEntry>>{};
    final dateFmt = DateFormat('yyyy-MM-dd');
    for (final e in entries) {
      final key = dateFmt.format(e.scheduledAt.toLocal());
      grouped.putIfAbsent(key, () => []).add(e);
    }
    return grouped;
  }

  IconData _platformIcon(String platform) {
    switch (platform) {
      case 'Facebook':
        return Icons.facebook;
      case 'Instagram':
        return Icons.camera_alt;
      case 'LinkedIn':
        return Icons.work;
      case 'Twitter':
        return Icons.tag;
      case 'Blog':
        return Icons.article;
      case 'Newsletter':
        return Icons.email;
      default:
        return Icons.public;
    }
  }

  String _statusLabel(String status) {
    switch (status) {
      case 'Scheduled':
        return 'Pianificato';
      case 'Published':
        return 'Pubblicato';
      case 'Failed':
        return 'Fallito';
      default:
        return status;
    }
  }

  Color _statusColor(String status) {
    switch (status) {
      case 'Scheduled':
        return Colors.blue;
      case 'Published':
        return Colors.green;
      case 'Failed':
        return Colors.red;
      default:
        return Colors.grey;
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: Text('Calendario - ${widget.agency.name}'),
      ),
      body: FutureBuilder<Map<String, List<_CalendarEntry>>>(
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
          final grouped = snap.data ?? {};
          if (grouped.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.calendar_month_outlined,
                      size: 64,
                      color: cs.onSurfaceVariant.withValues(alpha: 0.5)),
                  const SizedBox(height: 16),
                  Text('Nessun post in calendario',
                      style: Theme.of(context)
                          .textTheme
                          .titleMedium
                          ?.copyWith(color: cs.onSurfaceVariant)),
                ],
              ),
            );
          }
          final sortedDates = grouped.keys.toList()..sort();
          final dateLabelFmt = DateFormat('EEEE d MMMM yyyy', 'it_IT');
          final timeFmt = DateFormat('HH:mm');
          return RefreshIndicator(
            onRefresh: () async => setState(() => _future = _load()),
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(vertical: 8),
              itemCount: sortedDates.length,
              itemBuilder: (_, i) {
                final dateKey = sortedDates[i];
                final entries = grouped[dateKey]!;
                final parsed = DateTime.parse(dateKey);
                return Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Padding(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 16, vertical: 8),
                      child: Text(
                        dateLabelFmt.format(parsed),
                        style: Theme.of(context)
                            .textTheme
                            .titleSmall
                            ?.copyWith(
                              fontWeight: FontWeight.w600,
                              color: cs.primary,
                            ),
                      ),
                    ),
                    ...entries.map((e) => Card(
                          margin: const EdgeInsets.symmetric(
                              horizontal: 12, vertical: 4),
                          child: Padding(
                            padding: const EdgeInsets.all(12),
                            child: Row(
                              children: [
                                Container(
                                  padding: const EdgeInsets.all(8),
                                  decoration: BoxDecoration(
                                    color: cs.primaryContainer,
                                    borderRadius:
                                        BorderRadius.circular(8),
                                  ),
                                  child: Icon(
                                    _platformIcon(e.platform),
                                    color: cs.onPrimaryContainer,
                                    size: 20,
                                  ),
                                ),
                                const SizedBox(width: 12),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        e.title.isNotEmpty
                                            ? e.title
                                            : e.platform,
                                        style: Theme.of(context)
                                            .textTheme
                                            .titleSmall
                                            ?.copyWith(
                                                fontWeight:
                                                    FontWeight.w600),
                                        maxLines: 2,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                      const SizedBox(height: 4),
                                      Row(
                                        children: [
                                          Text(
                                            timeFmt.format(
                                                e.scheduledAt.toLocal()),
                                            style: TextStyle(
                                              fontSize: 12,
                                              color: cs.onSurfaceVariant,
                                            ),
                                          ),
                                          if (e.contentType.isNotEmpty) ...[
                                            const SizedBox(width: 8),
                                            Container(
                                              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
                                              decoration: BoxDecoration(
                                                color: cs.surfaceContainerHighest,
                                                borderRadius: BorderRadius.circular(8),
                                              ),
                                              child: Text(
                                                e.contentType,
                                                style: TextStyle(fontSize: 10, color: cs.onSurfaceVariant),
                                              ),
                                            ),
                                          ],
                                        ],
                                      ),
                                    ],
                                  ),
                                ),
                                const SizedBox(width: 8),
                                Container(
                                  padding: const EdgeInsets.symmetric(
                                      horizontal: 8, vertical: 4),
                                  decoration: BoxDecoration(
                                    color: _statusColor(e.status)
                                        .withValues(alpha: 0.12),
                                    borderRadius:
                                        BorderRadius.circular(12),
                                  ),
                                  child: Text(
                                    _statusLabel(e.status),
                                    style: TextStyle(
                                      fontSize: 11,
                                      fontWeight: FontWeight.w600,
                                      color: _statusColor(e.status),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        )),
                  ],
                );
              },
            ),
          );
        },
      ),
    );
  }
}
