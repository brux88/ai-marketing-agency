import 'package:flutter/material.dart';
import 'package:timeago/timeago.dart' as timeago;
import '../api/api_client.dart';
import '../models.dart';

class JobsScreen extends StatefulWidget {
  const JobsScreen({super.key});
  @override
  State<JobsScreen> createState() => _JobsScreenState();
}

class _JobsScreenState extends State<JobsScreen> {
  late Future<List<Job>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<Job>> _load() async {
    final res = await ApiClient.get('/api/v1/jobs?take=50');
    return (res['data'] as List).map((j) => Job.fromJson(j)).toList();
  }

  Color _statusColor(String status) {
    final s = status.toLowerCase();
    if (s == 'completed' || s == 'success') return Colors.green;
    if (s == 'running' || s == 'processing') return Colors.blue;
    if (s == 'failed' || s == 'error') return Colors.red;
    if (s == 'pending' || s == 'queued') return Colors.orange;
    if (s == 'cancelled') return Colors.grey;
    return Colors.grey;
  }

  IconData _statusIcon(String status) {
    final s = status.toLowerCase();
    if (s == 'completed' || s == 'success') return Icons.check_circle;
    if (s == 'running' || s == 'processing') return Icons.sync;
    if (s == 'failed' || s == 'error') return Icons.error;
    if (s == 'pending' || s == 'queued') return Icons.hourglass_empty;
    if (s == 'cancelled') return Icons.cancel;
    return Icons.help_outline;
  }

  String _agentTypeLabel(String agentType) {
    // Convert PascalCase to readable
    return agentType.replaceAllMapped(
        RegExp(r'([a-z])([A-Z])'), (m) => '${m[1]} ${m[2]}');
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: const Text('Jobs'),
        centerTitle: false,
      ),
      body: FutureBuilder<List<Job>>(
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
          final jobs = snap.data ?? [];
          if (jobs.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.work_off_outlined,
                      size: 64,
                      color: cs.onSurfaceVariant.withValues(alpha: 0.5)),
                  const SizedBox(height: 16),
                  Text('Nessun job',
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
              itemCount: jobs.length,
              itemBuilder: (_, i) {
                final job = jobs[i];
                final color = _statusColor(job.status);
                return Card(
                  margin: const EdgeInsets.symmetric(
                      horizontal: 12, vertical: 4),
                  clipBehavior: Clip.antiAlias,
                  child: ExpansionTile(
                    leading: Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: color.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Icon(_statusIcon(job.status),
                          color: color, size: 20),
                    ),
                    title: Text(_agentTypeLabel(job.agentType),
                        style: const TextStyle(
                            fontWeight: FontWeight.w600)),
                    subtitle: Row(
                      children: [
                        Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 8, vertical: 2),
                          decoration: BoxDecoration(
                            color: color.withValues(alpha: 0.12),
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: Text(job.status,
                              style: TextStyle(
                                  fontSize: 11,
                                  color: color,
                                  fontWeight: FontWeight.w600)),
                        ),
                        const SizedBox(width: 8),
                        Text(
                          timeago.format(job.createdAt, locale: 'it'),
                          style: Theme.of(context)
                              .textTheme
                              .bodySmall
                              ?.copyWith(color: cs.onSurfaceVariant),
                        ),
                      ],
                    ),
                    children: [
                      Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            if (job.input != null &&
                                job.input!.isNotEmpty) ...[
                              Text('Input',
                                  style: Theme.of(context)
                                      .textTheme
                                      .labelMedium
                                      ?.copyWith(
                                          fontWeight: FontWeight.w600)),
                              const SizedBox(height: 4),
                              Container(
                                width: double.infinity,
                                padding: const EdgeInsets.all(12),
                                decoration: BoxDecoration(
                                  color: cs.surfaceContainerHighest,
                                  borderRadius:
                                      BorderRadius.circular(8),
                                ),
                                child: Text(job.input!,
                                    maxLines: 5,
                                    overflow: TextOverflow.ellipsis,
                                    style: Theme.of(context)
                                        .textTheme
                                        .bodySmall),
                              ),
                              const SizedBox(height: 12),
                            ],
                            if (job.output != null &&
                                job.output!.isNotEmpty) ...[
                              Text('Output',
                                  style: Theme.of(context)
                                      .textTheme
                                      .labelMedium
                                      ?.copyWith(
                                          fontWeight: FontWeight.w600)),
                              const SizedBox(height: 4),
                              Container(
                                width: double.infinity,
                                padding: const EdgeInsets.all(12),
                                decoration: BoxDecoration(
                                  color: cs.surfaceContainerHighest,
                                  borderRadius:
                                      BorderRadius.circular(8),
                                ),
                                child: Text(job.output!,
                                    maxLines: 8,
                                    overflow: TextOverflow.ellipsis,
                                    style: Theme.of(context)
                                        .textTheme
                                        .bodySmall),
                              ),
                              const SizedBox(height: 12),
                            ],
                            if (job.errorMessage != null &&
                                job.errorMessage!.isNotEmpty) ...[
                              Text('Errore',
                                  style: Theme.of(context)
                                      .textTheme
                                      .labelMedium
                                      ?.copyWith(
                                          fontWeight: FontWeight.w600,
                                          color: cs.error)),
                              const SizedBox(height: 4),
                              Container(
                                width: double.infinity,
                                padding: const EdgeInsets.all(12),
                                decoration: BoxDecoration(
                                  color:
                                      cs.errorContainer.withValues(alpha: 0.3),
                                  borderRadius:
                                      BorderRadius.circular(8),
                                ),
                                child: Text(job.errorMessage!,
                                    style: TextStyle(
                                        fontSize: 13,
                                        color: cs.error)),
                              ),
                            ],
                            const SizedBox(height: 8),
                            // Timing info
                            Row(
                              children: [
                                Icon(Icons.schedule,
                                    size: 14,
                                    color: cs.onSurfaceVariant),
                                const SizedBox(width: 4),
                                Text(
                                    'Creato: ${_formatDate(job.createdAt)}',
                                    style: TextStyle(
                                        fontSize: 11,
                                        color: cs.onSurfaceVariant)),
                                if (job.completedAt != null) ...[
                                  const SizedBox(width: 12),
                                  Text(
                                      'Completato: ${_formatDate(job.completedAt!)}',
                                      style: TextStyle(
                                          fontSize: 11,
                                          color:
                                              cs.onSurfaceVariant)),
                                ],
                              ],
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                );
              },
            ),
          );
        },
      ),
    );
  }

  String _formatDate(DateTime dt) {
    final local = dt.toLocal();
    return '${local.day.toString().padLeft(2, '0')}/${local.month.toString().padLeft(2, '0')} ${local.hour.toString().padLeft(2, '0')}:${local.minute.toString().padLeft(2, '0')}';
  }
}
