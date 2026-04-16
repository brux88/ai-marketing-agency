import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';
import 'projects_screen.dart';

class AgenciesScreen extends StatefulWidget {
  const AgenciesScreen({super.key});
  @override
  State<AgenciesScreen> createState() => _AgenciesScreenState();
}

class _AgenciesScreenState extends State<AgenciesScreen> {
  late Future<List<Agency>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<Agency>> _load() async {
    final res = await ApiClient.get('/api/v1/agencies');
    return (res['data'] as List).map((j) => Agency.fromJson(j)).toList();
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: const Text('Le tue agenzie'),
        centerTitle: false,
      ),
      body: FutureBuilder<List<Agency>>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return _buildError(snap.error.toString());
          }
          final agencies = snap.data ?? [];
          if (agencies.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.business_outlined,
                      size: 64, color: cs.onSurfaceVariant.withValues(alpha: 0.5)),
                  const SizedBox(height: 16),
                  Text('Nessuna agenzia',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          color: cs.onSurfaceVariant)),
                ],
              ),
            );
          }
          return RefreshIndicator(
            onRefresh: () async => setState(() => _future = _load()),
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(vertical: 8),
              itemCount: agencies.length,
              itemBuilder: (_, i) {
                final a = agencies[i];
                return Card(
                  margin:
                      const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  child: InkWell(
                    borderRadius: BorderRadius.circular(12),
                    onTap: () => Navigator.push(
                        context,
                        MaterialPageRoute(
                            builder: (_) => ProjectsScreen(agency: a))),
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Row(
                        children: [
                          CircleAvatar(
                            backgroundColor: cs.primaryContainer,
                            child: Text(
                              a.name.isNotEmpty
                                  ? a.name[0].toUpperCase()
                                  : '?',
                              style:
                                  TextStyle(color: cs.onPrimaryContainer),
                            ),
                          ),
                          const SizedBox(width: 16),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(a.name,
                                    style: Theme.of(context)
                                        .textTheme
                                        .titleMedium
                                        ?.copyWith(
                                            fontWeight: FontWeight.w600)),
                                const SizedBox(height: 4),
                                Text(a.productName,
                                    style: Theme.of(context)
                                        .textTheme
                                        .bodySmall
                                        ?.copyWith(
                                            color: cs.onSurfaceVariant)),
                                const SizedBox(height: 4),
                                Row(
                                  children: [
                                    _chip(Icons.article_outlined,
                                        '${a.generatedContentsCount}', cs),
                                    const SizedBox(width: 8),
                                    _chip(Icons.source_outlined,
                                        '${a.contentSourcesCount}', cs),
                                  ],
                                ),
                              ],
                            ),
                          ),
                          Icon(Icons.chevron_right,
                              color: cs.onSurfaceVariant),
                        ],
                      ),
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

  Widget _chip(IconData icon, String label, ColorScheme cs) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: cs.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: cs.onSurfaceVariant),
          const SizedBox(width: 4),
          Text(label,
              style: TextStyle(fontSize: 12, color: cs.onSurfaceVariant)),
        ],
      ),
    );
  }

  Widget _buildError(String error) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 48, color: Colors.red),
            const SizedBox(height: 16),
            Text('Errore: $error', textAlign: TextAlign.center),
            const SizedBox(height: 16),
            FilledButton.tonal(
              onPressed: () => setState(() => _future = _load()),
              child: const Text('Riprova'),
            ),
          ],
        ),
      ),
    );
  }
}
