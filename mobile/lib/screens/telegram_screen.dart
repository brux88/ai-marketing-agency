import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';

class TelegramScreen extends StatefulWidget {
  final Agency agency;
  const TelegramScreen({super.key, required this.agency});
  @override
  State<TelegramScreen> createState() => _TelegramScreenState();
}

class _TelegramScreenState extends State<TelegramScreen> {
  late Future<Map<String, dynamic>> _botFuture;
  late Future<List<Map<String, dynamic>>> _connectionsFuture;
  final _tokenController = TextEditingController();
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _botFuture = _loadBot();
    _connectionsFuture = _loadConnections();
  }

  @override
  void dispose() {
    _tokenController.dispose();
    super.dispose();
  }

  Future<Map<String, dynamic>> _loadBot() async {
    final res = await ApiClient.get(
        '/api/v1/agencies/${widget.agency.id}/telegram/bot');
    return res['data'] as Map<String, dynamic>;
  }

  Future<List<Map<String, dynamic>>> _loadConnections() async {
    final res = await ApiClient.get(
        '/api/v1/agencies/${widget.agency.id}/telegram');
    return (res['data'] as List).cast<Map<String, dynamic>>();
  }

  Future<void> _saveToken() async {
    final token = _tokenController.text.trim();
    if (token.isEmpty) return;
    setState(() => _saving = true);
    try {
      await ApiClient.put(
        '/api/v1/agencies/${widget.agency.id}/telegram/bot',
        {'token': token},
      );
      _tokenController.clear();
      setState(() {
        _botFuture = _loadBot();
        _saving = false;
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Token salvato con successo')),
        );
      }
    } catch (e) {
      setState(() => _saving = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Errore: $e')),
        );
      }
    }
  }

  Future<void> _registerWebhook() async {
    try {
      await ApiClient.post(
        '/api/v1/agencies/${widget.agency.id}/telegram/register-webhook',
      );
      setState(() => _botFuture = _loadBot());
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Webhook registrato con successo')),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Errore: $e')),
        );
      }
    }
  }

  Future<void> _deleteConnection(String connectionId) async {
    try {
      await ApiClient.delete(
        '/api/v1/agencies/${widget.agency.id}/telegram/$connectionId',
      );
      setState(() => _connectionsFuture = _loadConnections());
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Connessione eliminata')),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Errore: $e')),
        );
      }
    }
  }

  Future<void> _sendTestMessage() async {
    try {
      await ApiClient.post(
        '/api/v1/agencies/${widget.agency.id}/telegram/test',
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Messaggio di test inviato')),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Errore: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: Text('Telegram - ${widget.agency.name}'),
      ),
      body: RefreshIndicator(
        onRefresh: () async {
          setState(() {
            _botFuture = _loadBot();
            _connectionsFuture = _loadConnections();
          });
        },
        child: ListView(
          padding: const EdgeInsets.all(12),
          children: [
            // --- Bot Configuration Card ---
            _buildBotCard(cs),
            const SizedBox(height: 12),
            // --- Connections List Card ---
            _buildConnectionsCard(cs),
            const SizedBox(height: 12),
            // --- Test Message Card ---
            _buildTestCard(cs),
          ],
        ),
      ),
    );
  }

  Widget _buildBotCard(ColorScheme cs) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: FutureBuilder<Map<String, dynamic>>(
          future: _botFuture,
          builder: (context, snap) {
            if (snap.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snap.hasError) {
              return Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.error_outline,
                      size: 48, color: Colors.red),
                  const SizedBox(height: 8),
                  Text('Errore: ${snap.error}',
                      textAlign: TextAlign.center),
                  const SizedBox(height: 8),
                  FilledButton.tonal(
                    onPressed: () =>
                        setState(() => _botFuture = _loadBot()),
                    child: const Text('Riprova'),
                  ),
                ],
              );
            }
            final bot = snap.data!;
            final hasToken = bot['hasToken'] == true;
            final username = bot['botUsername'] as String?;
            final webhookUrl = bot['webhookUrl'] as String?;
            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Icon(Icons.smart_toy_outlined,
                        color: cs.primary),
                    const SizedBox(width: 8),
                    Text('Configurazione Bot',
                        style: Theme.of(context)
                            .textTheme
                            .titleMedium
                            ?.copyWith(fontWeight: FontWeight.w600)),
                  ],
                ),
                const SizedBox(height: 16),
                // Status row
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 10, vertical: 4),
                      decoration: BoxDecoration(
                        color: (hasToken ? Colors.green : Colors.grey)
                            .withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            hasToken
                                ? Icons.check_circle
                                : Icons.cancel_outlined,
                            size: 16,
                            color:
                                hasToken ? Colors.green : Colors.grey,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            hasToken ? 'Connesso' : 'Non connesso',
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                              color: hasToken
                                  ? Colors.green
                                  : Colors.grey,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                if (hasToken && username != null && username.isNotEmpty) ...[
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Icon(Icons.alternate_email,
                          size: 14, color: cs.onSurfaceVariant),
                      const SizedBox(width: 4),
                      Text(username,
                          style: TextStyle(
                              fontSize: 13,
                              color: cs.onSurfaceVariant)),
                    ],
                  ),
                ],
                if (hasToken &&
                    webhookUrl != null &&
                    webhookUrl.isNotEmpty) ...[
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Icon(Icons.link,
                          size: 14, color: cs.onSurfaceVariant),
                      const SizedBox(width: 4),
                      Expanded(
                        child: Text(webhookUrl,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(
                                fontSize: 12,
                                color: cs.onSurfaceVariant)),
                      ),
                    ],
                  ),
                ],
                const SizedBox(height: 16),
                TextField(
                  controller: _tokenController,
                  decoration: const InputDecoration(
                    labelText: 'Bot Token',
                    hintText: 'Incolla il token del bot',
                    border: OutlineInputBorder(),
                    prefixIcon: Icon(Icons.vpn_key_outlined),
                  ),
                  obscureText: true,
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    FilledButton.icon(
                      onPressed: _saving ? null : _saveToken,
                      icon: _saving
                          ? const SizedBox(
                              width: 16,
                              height: 16,
                              child: CircularProgressIndicator(
                                  strokeWidth: 2),
                            )
                          : const Icon(Icons.save_outlined),
                      label: const Text('Salva token'),
                    ),
                    const SizedBox(width: 8),
                    if (hasToken)
                      FilledButton.tonal(
                        onPressed: _registerWebhook,
                        child: const Text('Registra webhook'),
                      ),
                  ],
                ),
              ],
            );
          },
        ),
      ),
    );
  }

  Widget _buildConnectionsCard(ColorScheme cs) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.people_outline, color: cs.primary),
                const SizedBox(width: 8),
                Text('Connessioni',
                    style: Theme.of(context)
                        .textTheme
                        .titleMedium
                        ?.copyWith(fontWeight: FontWeight.w600)),
              ],
            ),
            const SizedBox(height: 12),
            FutureBuilder<List<Map<String, dynamic>>>(
              future: _connectionsFuture,
              builder: (context, snap) {
                if (snap.connectionState == ConnectionState.waiting) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (snap.hasError) {
                  return Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Icon(Icons.error_outline,
                          size: 48, color: Colors.red),
                      const SizedBox(height: 8),
                      Text('Errore: ${snap.error}',
                          textAlign: TextAlign.center),
                      const SizedBox(height: 8),
                      FilledButton.tonal(
                        onPressed: () => setState(
                            () => _connectionsFuture = _loadConnections()),
                        child: const Text('Riprova'),
                      ),
                    ],
                  );
                }
                final connections = snap.data ?? [];
                if (connections.isEmpty) {
                  return Padding(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    child: Center(
                      child: Column(
                        children: [
                          Icon(Icons.chat_bubble_outline,
                              size: 40,
                              color: cs.onSurfaceVariant
                                  .withValues(alpha: 0.5)),
                          const SizedBox(height: 8),
                          Text('Nessuna connessione',
                              style: TextStyle(
                                  color: cs.onSurfaceVariant)),
                        ],
                      ),
                    ),
                  );
                }
                return Column(
                  children: connections.map((c) {
                    final chatTitle = c['chatTitle'] as String? ?? '';
                    final username = c['username'] as String? ?? '';
                    final isActive = c['isActive'] == true;
                    final id = c['id'] as String;
                    return ListTile(
                      contentPadding: EdgeInsets.zero,
                      leading: CircleAvatar(
                        backgroundColor: (isActive
                                ? Colors.green
                                : Colors.grey)
                            .withValues(alpha: 0.12),
                        child: Icon(
                          isActive
                              ? Icons.chat
                              : Icons.chat_bubble_outline,
                          color: isActive ? Colors.green : Colors.grey,
                          size: 20,
                        ),
                      ),
                      title: Text(
                          chatTitle.isNotEmpty ? chatTitle : username),
                      subtitle: username.isNotEmpty && chatTitle.isNotEmpty
                          ? Text('@$username')
                          : null,
                      trailing: IconButton(
                        icon: const Icon(Icons.delete_outline,
                            color: Colors.red),
                        onPressed: () async {
                          final confirmed = await showDialog<bool>(
                            context: context,
                            builder: (ctx) => AlertDialog(
                              title: const Text('Elimina connessione'),
                              content: const Text(
                                  'Sei sicuro di voler eliminare questa connessione?'),
                              actions: [
                                TextButton(
                                  onPressed: () =>
                                      Navigator.pop(ctx, false),
                                  child: const Text('Annulla'),
                                ),
                                FilledButton(
                                  onPressed: () =>
                                      Navigator.pop(ctx, true),
                                  child: const Text('Elimina'),
                                ),
                              ],
                            ),
                          );
                          if (confirmed == true) {
                            _deleteConnection(id);
                          }
                        },
                      ),
                    );
                  }).toList(),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTestCard(ColorScheme cs) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.send_outlined, color: cs.primary),
                const SizedBox(width: 8),
                Text('Test',
                    style: Theme.of(context)
                        .textTheme
                        .titleMedium
                        ?.copyWith(fontWeight: FontWeight.w600)),
              ],
            ),
            const SizedBox(height: 12),
            FilledButton.icon(
              onPressed: _sendTestMessage,
              icon: const Icon(Icons.send),
              label: const Text('Invia messaggio di test'),
            ),
          ],
        ),
      ),
    );
  }
}
