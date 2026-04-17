import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';

class ApiKeysScreen extends StatefulWidget {
  final Agency agency;
  const ApiKeysScreen({super.key, required this.agency});
  @override
  State<ApiKeysScreen> createState() => _ApiKeysScreenState();
}

class _ApiKeysScreenState extends State<ApiKeysScreen> {
  late Future<List<Map<String, dynamic>>> _keysFuture;

  static const _providerNames = {
    0: 'OpenAI',
    1: 'Google Gemini',
    2: 'Anthropic',
    3: 'Mistral',
    4: 'Groq',
    5: 'DeepSeek',
    6: 'Higgsfield',
  };

  static const _categoryNames = {
    0: 'Text',
    1: 'Image',
  };

  @override
  void initState() {
    super.initState();
    _keysFuture = _loadKeys();
  }

  Future<List<Map<String, dynamic>>> _loadKeys() async {
    final res = await ApiClient.get('/api/v1/llmkeys');
    return (res['data'] as List).cast<Map<String, dynamic>>();
  }

  Future<void> _deleteKey(String id) async {
    try {
      await ApiClient.delete('/api/v1/llmkeys/$id');
      setState(() => _keysFuture = _loadKeys());
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Chiave API eliminata')),
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

  void _showAddSheet() {
    int selectedProvider = 0;
    int selectedCategory = 0;
    final apiKeyController = TextEditingController();
    final baseUrlController = TextEditingController();
    final modelIdController = TextEditingController();
    bool saving = false;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (ctx, setSheetState) {
            return Padding(
              padding: EdgeInsets.only(
                left: 16,
                right: 16,
                top: 24,
                bottom: MediaQuery.of(ctx).viewInsets.bottom + 24,
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'Aggiungi chiave API',
                    style: Theme.of(ctx)
                        .textTheme
                        .titleLarge
                        ?.copyWith(fontWeight: FontWeight.w600),
                  ),
                  const SizedBox(height: 20),
                  DropdownButtonFormField<int>(
                    initialValue: selectedProvider,
                    decoration: const InputDecoration(
                      labelText: 'Provider',
                      border: OutlineInputBorder(),
                      prefixIcon: Icon(Icons.cloud_outlined),
                    ),
                    items: _providerNames.entries
                        .map((e) => DropdownMenuItem(
                              value: e.key,
                              child: Text(e.value),
                            ))
                        .toList(),
                    onChanged: (v) =>
                        setSheetState(() => selectedProvider = v!),
                  ),
                  const SizedBox(height: 12),
                  DropdownButtonFormField<int>(
                    initialValue: selectedCategory,
                    decoration: const InputDecoration(
                      labelText: 'Categoria',
                      border: OutlineInputBorder(),
                      prefixIcon: Icon(Icons.category_outlined),
                    ),
                    items: _categoryNames.entries
                        .map((e) => DropdownMenuItem(
                              value: e.key,
                              child: Text(e.value),
                            ))
                        .toList(),
                    onChanged: (v) =>
                        setSheetState(() => selectedCategory = v!),
                  ),
                  const SizedBox(height: 12),
                  TextField(
                    controller: apiKeyController,
                    obscureText: true,
                    decoration: const InputDecoration(
                      labelText: 'Chiave API',
                      border: OutlineInputBorder(),
                      prefixIcon: Icon(Icons.vpn_key_outlined),
                    ),
                  ),
                  const SizedBox(height: 12),
                  TextField(
                    controller: baseUrlController,
                    decoration: const InputDecoration(
                      labelText: 'Base URL (opzionale)',
                      border: OutlineInputBorder(),
                      prefixIcon: Icon(Icons.link),
                    ),
                  ),
                  const SizedBox(height: 12),
                  TextField(
                    controller: modelIdController,
                    decoration: const InputDecoration(
                      labelText: 'Model ID (opzionale)',
                      border: OutlineInputBorder(),
                      prefixIcon: Icon(Icons.model_training),
                    ),
                  ),
                  const SizedBox(height: 20),
                  FilledButton.icon(
                    onPressed: saving
                        ? null
                        : () async {
                            final apiKey = apiKeyController.text.trim();
                            if (apiKey.isEmpty) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(
                                    content:
                                        Text('La chiave API è obbligatoria')),
                              );
                              return;
                            }
                            setSheetState(() => saving = true);
                            try {
                              final body = <String, dynamic>{
                                'providerType': selectedProvider,
                                'category': selectedCategory,
                                'apiKey': apiKey,
                              };
                              final baseUrl = baseUrlController.text.trim();
                              if (baseUrl.isNotEmpty) body['baseUrl'] = baseUrl;
                              final modelId = modelIdController.text.trim();
                              if (modelId.isNotEmpty) {
                                body['modelId'] = modelId;
                              }
                              await ApiClient.post('/api/v1/llmkeys', body);
                              setState(() => _keysFuture = _loadKeys());
                              if (ctx.mounted) Navigator.pop(ctx);
                              if (mounted) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                      content: Text(
                                          'Chiave API aggiunta con successo')),
                                );
                              }
                            } catch (e) {
                              setSheetState(() => saving = false);
                              if (mounted) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  SnackBar(content: Text('Errore: $e')),
                                );
                              }
                            }
                          },
                    icon: saving
                        ? const SizedBox(
                            width: 16,
                            height: 16,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Icon(Icons.add),
                    label: const Text('Aggiungi'),
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: Text('Chiavi API - ${widget.agency.name}'),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _showAddSheet,
        child: const Icon(Icons.add),
      ),
      body: RefreshIndicator(
        onRefresh: () async {
          setState(() => _keysFuture = _loadKeys());
        },
        child: FutureBuilder<List<Map<String, dynamic>>>(
          future: _keysFuture,
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
                    const SizedBox(height: 8),
                    Text('Errore: ${snap.error}',
                        textAlign: TextAlign.center),
                    const SizedBox(height: 8),
                    FilledButton.tonal(
                      onPressed: () =>
                          setState(() => _keysFuture = _loadKeys()),
                      child: const Text('Riprova'),
                    ),
                  ],
                ),
              );
            }
            final keys = snap.data ?? [];
            if (keys.isEmpty) {
              return ListView(
                children: [
                  SizedBox(
                    height: MediaQuery.of(context).size.height * 0.6,
                    child: Center(
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.vpn_key_off_outlined,
                              size: 64,
                              color:
                                  cs.onSurfaceVariant.withValues(alpha: 0.5)),
                          const SizedBox(height: 16),
                          Text(
                            'Nessuna chiave API configurata',
                            style: TextStyle(
                              fontSize: 16,
                              color: cs.onSurfaceVariant,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              );
            }
            return ListView.builder(
              padding: const EdgeInsets.all(12),
              itemCount: keys.length,
              itemBuilder: (context, index) {
                final key = keys[index];
                final id = key['id'] as String;
                final providerType = key['providerType'] as int? ?? 0;
                final category = key['category'] as int? ?? 0;
                final isActive = key['isActive'] == true;
                final modelId = key['modelId'] as String?;
                final providerName =
                    _providerNames[providerType] ?? 'Sconosciuto';
                final categoryName =
                    _categoryNames[category] ?? 'Sconosciuto';

                return Card(
                  margin: const EdgeInsets.only(bottom: 10),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Row(
                      children: [
                        CircleAvatar(
                          backgroundColor:
                              (isActive ? Colors.green : Colors.grey)
                                  .withValues(alpha: 0.12),
                          child: Icon(
                            Icons.vpn_key_outlined,
                            color: isActive ? Colors.green : Colors.grey,
                            size: 20,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                providerName,
                                style: Theme.of(context)
                                    .textTheme
                                    .titleSmall
                                    ?.copyWith(fontWeight: FontWeight.w600),
                              ),
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  Container(
                                    padding: const EdgeInsets.symmetric(
                                        horizontal: 8, vertical: 2),
                                    decoration: BoxDecoration(
                                      color: cs.primaryContainer,
                                      borderRadius: BorderRadius.circular(8),
                                    ),
                                    child: Text(
                                      categoryName,
                                      style: TextStyle(
                                        fontSize: 11,
                                        fontWeight: FontWeight.w600,
                                        color: cs.onPrimaryContainer,
                                      ),
                                    ),
                                  ),
                                  const SizedBox(width: 8),
                                  Container(
                                    padding: const EdgeInsets.symmetric(
                                        horizontal: 8, vertical: 2),
                                    decoration: BoxDecoration(
                                      color: (isActive
                                              ? Colors.green
                                              : Colors.grey)
                                          .withValues(alpha: 0.12),
                                      borderRadius: BorderRadius.circular(8),
                                    ),
                                    child: Text(
                                      isActive ? 'Attiva' : 'Inattiva',
                                      style: TextStyle(
                                        fontSize: 11,
                                        fontWeight: FontWeight.w600,
                                        color: isActive
                                            ? Colors.green
                                            : Colors.grey,
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                              if (modelId != null &&
                                  modelId.isNotEmpty) ...[
                                const SizedBox(height: 4),
                                Text(
                                  modelId,
                                  style: TextStyle(
                                    fontSize: 12,
                                    color: cs.onSurfaceVariant,
                                  ),
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ],
                            ],
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.delete_outline,
                              color: Colors.red),
                          onPressed: () async {
                            final confirmed = await showDialog<bool>(
                              context: context,
                              builder: (ctx) => AlertDialog(
                                title: const Text('Elimina chiave API'),
                                content: const Text(
                                    'Sei sicuro di voler eliminare questa chiave API?'),
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
                              _deleteKey(id);
                            }
                          },
                        ),
                      ],
                    ),
                  ),
                );
              },
            );
          },
        ),
      ),
    );
  }
}
