import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';

class SchedulesScreen extends StatefulWidget {
  final Agency agency;
  final Project project;
  const SchedulesScreen(
      {super.key, required this.agency, required this.project});
  @override
  State<SchedulesScreen> createState() => _SchedulesScreenState();
}

class _SchedulesScreenState extends State<SchedulesScreen> {
  late Future<List<ContentSchedule>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<ContentSchedule>> _load() async {
    final res = await ApiClient.get(
        '/api/v1/agencies/${widget.agency.id}/schedules');
    final all = (res['data'] as List)
        .map((j) => ContentSchedule.fromJson(j))
        .toList();
    // Filter by project if applicable
    return all
        .where((s) =>
            s.projectId == null || s.projectId == widget.project.id)
        .toList();
  }

  String _formatNextRun(DateTime? dt) {
    if (dt == null) return 'Non pianificato';
    final local = dt.toLocal();
    final now = DateTime.now();
    final diff = local.difference(now);
    if (diff.isNegative) return 'In esecuzione...';
    if (diff.inMinutes < 60) return 'Tra ${diff.inMinutes} min';
    if (diff.inHours < 24) return 'Tra ${diff.inHours} ore';
    return '${local.day.toString().padLeft(2, '0')}/${local.month.toString().padLeft(2, '0')} ${local.hour.toString().padLeft(2, '0')}:${local.minute.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: Text('Pianificazione - ${widget.project.name}'),
      ),
      body: FutureBuilder<List<ContentSchedule>>(
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
          final schedules = snap.data ?? [];
          if (schedules.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.calendar_month_outlined,
                      size: 64,
                      color: cs.onSurfaceVariant.withValues(alpha: 0.5)),
                  const SizedBox(height: 16),
                  Text('Nessuna pianificazione',
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
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(vertical: 8),
              itemCount: schedules.length,
              itemBuilder: (_, i) {
                final s = schedules[i];
                return Card(
                  margin: const EdgeInsets.symmetric(
                      horizontal: 12, vertical: 4),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Container(
                              padding: const EdgeInsets.all(8),
                              decoration: BoxDecoration(
                                color: (s.isActive
                                        ? Colors.green
                                        : Colors.grey)
                                    .withValues(alpha: 0.12),
                                borderRadius:
                                    BorderRadius.circular(8),
                              ),
                              child: Icon(
                                s.isActive
                                    ? Icons.schedule
                                    : Icons.pause_circle_outline,
                                color: s.isActive
                                    ? Colors.green
                                    : Colors.grey,
                                size: 20,
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Column(
                                crossAxisAlignment:
                                    CrossAxisAlignment.start,
                                children: [
                                  Text(s.name,
                                      style: Theme.of(context)
                                          .textTheme
                                          .titleSmall
                                          ?.copyWith(
                                              fontWeight:
                                                  FontWeight.w600)),
                                  const SizedBox(height: 4),
                                  Text(
                                    s.isActive ? 'Attivo' : 'In pausa',
                                    style: TextStyle(
                                      fontSize: 12,
                                      color: s.isActive
                                          ? Colors.green
                                          : Colors.grey,
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),
                        // Schedule details
                        Row(
                          children: [
                            _detailItem(
                                Icons.access_time, s.timeOfDay, cs),
                            const SizedBox(width: 16),
                            _detailItem(
                                Icons.public, s.timeZone, cs),
                          ],
                        ),
                        const SizedBox(height: 8),
                        Row(
                          children: [
                            _detailItem(Icons.next_plan_outlined,
                                _formatNextRun(s.nextRunAt), cs),
                            if (s.lastRunAt != null) ...[
                              const SizedBox(width: 16),
                              _detailItem(
                                  Icons.history,
                                  'Ultimo: ${_formatNextRun(s.lastRunAt)}',
                                  cs),
                            ],
                          ],
                        ),
                        if (s.input != null &&
                            s.input!.isNotEmpty) ...[
                          const SizedBox(height: 8),
                          Container(
                            width: double.infinity,
                            padding: const EdgeInsets.all(10),
                            decoration: BoxDecoration(
                              color: cs.surfaceContainerHighest,
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Text(s.input!,
                                maxLines: 3,
                                overflow: TextOverflow.ellipsis,
                                style: Theme.of(context)
                                    .textTheme
                                    .bodySmall
                                    ?.copyWith(
                                        color: cs.onSurfaceVariant)),
                          ),
                        ],
                      ],
                    ),
                  ),
                );
              },
            ),
          );
        },
      ),
    );
  }

  Widget _detailItem(IconData icon, String text, ColorScheme cs) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 14, color: cs.onSurfaceVariant),
        const SizedBox(width: 4),
        Text(text,
            style: TextStyle(fontSize: 12, color: cs.onSurfaceVariant)),
      ],
    );
  }
}
