import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';
import 'content_screen.dart';
import 'login_screen.dart';

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

  Future<void> _logout() async {
    await ApiClient.clearToken();
    if (!mounted) return;
    Navigator.pushReplacement(context,
        MaterialPageRoute(builder: (_) => const LoginScreen()));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Le tue agenzie'),
        actions: [
          IconButton(icon: const Icon(Icons.logout), onPressed: _logout),
        ],
      ),
      body: FutureBuilder<List<Agency>>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Center(child: Text('Errore: ${snap.error}'));
          }
          final agencies = snap.data ?? [];
          if (agencies.isEmpty) {
            return const Center(child: Text('Nessuna agenzia'));
          }
          return RefreshIndicator(
            onRefresh: () async => setState(() => _future = _load()),
            child: ListView.builder(
              itemCount: agencies.length,
              itemBuilder: (_, i) {
                final a = agencies[i];
                return Card(
                  margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  child: ListTile(
                    title: Text(a.name,
                        style: const TextStyle(fontWeight: FontWeight.bold)),
                    subtitle: Text(
                        '${a.productName}\n${a.generatedContentsCount} contenuti · ${a.contentSourcesCount} fonti'),
                    isThreeLine: true,
                    trailing: const Icon(Icons.chevron_right),
                    onTap: () => Navigator.push(context,
                        MaterialPageRoute(
                            builder: (_) => ContentScreen(agency: a))),
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
