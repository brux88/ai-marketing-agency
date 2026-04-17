import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';
import 'telegram_screen.dart';
import 'api_keys_screen.dart';
import 'social_connectors_screen.dart';

class AgencySettingsScreen extends StatefulWidget {
  final Agency agency;
  const AgencySettingsScreen({super.key, required this.agency});
  @override
  State<AgencySettingsScreen> createState() => _AgencySettingsScreenState();
}

class _AgencySettingsScreenState extends State<AgencySettingsScreen> {
  late Future<Map<String, dynamic>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<Map<String, dynamic>> _load() async {
    final res =
        await ApiClient.get('/api/v1/agencies/${widget.agency.id}');
    return (res['data'] ?? res) as Map<String, dynamic>;
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: const Text('Impostazioni'),
      ),
      body: FutureBuilder<Map<String, dynamic>>(
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
          final data = snap.data!;
          final textLlmConfigured = data['defaultLlmProviderKeyId'] != null;
          final imageLlmConfigured = data['imageLlmProviderKeyId'] != null;

          return RefreshIndicator(
            onRefresh: () async => setState(() => _future = _load()),
            child: ListView(
              padding: const EdgeInsets.symmetric(vertical: 8),
              children: [
                // Informazioni card
                Card(
                  margin:
                      const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Icon(Icons.info_outline,
                                color: cs.primary, size: 20),
                            const SizedBox(width: 8),
                            Text('Informazioni',
                                style: Theme.of(context)
                                    .textTheme
                                    .titleMedium
                                    ?.copyWith(fontWeight: FontWeight.w600)),
                          ],
                        ),
                        const Divider(height: 24),
                        _infoRow(context, 'Nome', widget.agency.name),
                        const SizedBox(height: 8),
                        _infoRow(
                            context, 'Prodotto', widget.agency.productName),
                        if (widget.agency.description != null &&
                            widget.agency.description!.isNotEmpty) ...[
                          const SizedBox(height: 8),
                          _infoRow(context, 'Descrizione',
                              widget.agency.description!),
                        ],
                      ],
                    ),
                  ),
                ),

                // Navigazione card
                Card(
                  margin:
                      const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 8),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Padding(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 16, vertical: 8),
                          child: Row(
                            children: [
                              Icon(Icons.menu_outlined,
                                  color: cs.primary, size: 20),
                              const SizedBox(width: 8),
                              Text('Navigazione',
                                  style: Theme.of(context)
                                      .textTheme
                                      .titleMedium
                                      ?.copyWith(
                                          fontWeight: FontWeight.w600)),
                            ],
                          ),
                        ),
                        const Divider(height: 1),
                        ListTile(
                          leading: const Icon(Icons.telegram),
                          title: const Text('Telegram'),
                          subtitle:
                              const Text('Configura il bot Telegram'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.push(
                              context,
                              MaterialPageRoute(
                                  builder: (_) => TelegramScreen(
                                      agency: widget.agency))),
                        ),
                        ListTile(
                          leading: const Icon(Icons.vpn_key_outlined),
                          title: const Text('Chiavi API'),
                          subtitle: const Text(
                              'Gestisci le chiavi dei provider'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.push(
                              context,
                              MaterialPageRoute(
                                  builder: (_) => ApiKeysScreen(
                                      agency: widget.agency))),
                        ),
                        ListTile(
                          leading: const Icon(Icons.share_outlined),
                          title: const Text('Connettori Social'),
                          subtitle: const Text(
                              'Collega i tuoi account social'),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () => Navigator.push(
                              context,
                              MaterialPageRoute(
                                  builder: (_) => SocialConnectorsScreen(
                                      agency: widget.agency))),
                        ),
                      ],
                    ),
                  ),
                ),

                // LLM Predefiniti card
                Card(
                  margin:
                      const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Icon(Icons.psychology_outlined,
                                color: cs.primary, size: 20),
                            const SizedBox(width: 8),
                            Text('LLM Predefiniti',
                                style: Theme.of(context)
                                    .textTheme
                                    .titleMedium
                                    ?.copyWith(fontWeight: FontWeight.w600)),
                          ],
                        ),
                        const Divider(height: 24),
                        _llmRow(context, cs, 'Testo', textLlmConfigured),
                        const SizedBox(height: 12),
                        _llmRow(
                            context, cs, 'Immagine', imageLlmConfigured),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _infoRow(BuildContext context, String label, String value) {
    final cs = Theme.of(context).colorScheme;
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 90,
          child: Text(label,
              style: Theme.of(context)
                  .textTheme
                  .bodySmall
                  ?.copyWith(color: cs.onSurfaceVariant)),
        ),
        Expanded(
          child: Text(value,
              style: Theme.of(context).textTheme.bodyMedium),
        ),
      ],
    );
  }

  Widget _llmRow(
      BuildContext context, ColorScheme cs, String label, bool configured) {
    return Row(
      children: [
        Expanded(
          child: Text(label,
              style: Theme.of(context).textTheme.bodyMedium),
        ),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
          decoration: BoxDecoration(
            color: configured
                ? cs.primaryContainer
                : cs.surfaceContainerHighest,
            borderRadius: BorderRadius.circular(12),
          ),
          child: Text(
            configured ? 'Configurato' : 'Non configurato',
            style: TextStyle(
              fontSize: 12,
              color: configured
                  ? cs.onPrimaryContainer
                  : cs.onSurfaceVariant,
            ),
          ),
        ),
      ],
    );
  }
}
