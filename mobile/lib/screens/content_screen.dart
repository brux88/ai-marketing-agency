import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';

class ContentScreen extends StatefulWidget {
  final Agency agency;
  const ContentScreen({super.key, required this.agency});
  @override
  State<ContentScreen> createState() => _ContentScreenState();
}

class _ContentScreenState extends State<ContentScreen> {
  late Future<List<GeneratedContent>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<GeneratedContent>> _load() async {
    final res = await ApiClient.get(
        '/api/v1/agencies/${widget.agency.id}/content');
    return (res['data'] as List)
        .map((j) => GeneratedContent.fromJson(j))
        .toList();
  }

  Future<void> _approve(String id) async {
    final messenger = ScaffoldMessenger.of(context);
    try {
      await ApiClient.post(
          '/api/v1/agencies/${widget.agency.id}/content/$id/approve');
      messenger.showSnackBar(const SnackBar(content: Text('Approvato')));
      if (mounted) setState(() => _future = _load());
    } catch (e) {
      messenger.showSnackBar(SnackBar(content: Text('Errore: $e')));
    }
  }

  Color _statusColor(int s) {
    switch (s) {
      case 1: return Colors.grey;
      case 2: return Colors.orange;
      case 3: return Colors.blue;
      case 4: return Colors.green;
      case 5: return Colors.red;
      default: return Colors.grey;
    }
  }

  String _statusLabel(int s) {
    switch (s) {
      case 1: return 'Bozza';
      case 2: return 'In review';
      case 3: return 'Approvato';
      case 4: return 'Pubblicato';
      case 5: return 'Rifiutato';
      default: return '?';
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.agency.name)),
      body: FutureBuilder<List<GeneratedContent>>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Center(child: Text('Errore: ${snap.error}'));
          }
          final items = snap.data ?? [];
          if (items.isEmpty) {
            return const Center(child: Text('Nessun contenuto'));
          }
          return RefreshIndicator(
            onRefresh: () async => setState(() => _future = _load()),
            child: ListView.builder(
              itemCount: items.length,
              itemBuilder: (_, i) {
                final c = items[i];
                return Card(
                  margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  child: ExpansionTile(
                    leading: CircleAvatar(
                        backgroundColor: _statusColor(c.status),
                        radius: 8),
                    title: Text(c.title,
                        maxLines: 2, overflow: TextOverflow.ellipsis),
                    subtitle: Text(
                        '${_statusLabel(c.status)} · Score ${c.overallScore.toStringAsFixed(1)}/10'),
                    children: [
                      Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            if (c.imageUrl != null)
                              Padding(
                                padding: const EdgeInsets.only(bottom: 12),
                                child: Image.network(c.imageUrl!,
                                    height: 200, fit: BoxFit.cover),
                              ),
                            Text(c.body),
                            const SizedBox(height: 12),
                            if (c.status == 2)
                              Row(
                                children: [
                                  FilledButton.icon(
                                    icon: const Icon(Icons.check),
                                    label: const Text('Approva'),
                                    onPressed: () => _approve(c.id),
                                  ),
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
}
