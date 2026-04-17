import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';

class GenerateContentScreen extends StatefulWidget {
  final Agency agency;
  final Project project;
  const GenerateContentScreen(
      {super.key, required this.agency, required this.project});
  @override
  State<GenerateContentScreen> createState() => _GenerateContentScreenState();
}

class _GenerateContentScreenState extends State<GenerateContentScreen> {
  final _topicController = TextEditingController();
  String _agentType = 'social-manager';
  int _imageMode = 0;
  int _imageCount = 3;
  bool _loading = false;

  static const _agentTypes = {
    'social-manager': 'Social Manager',
    'content-writer': 'Content Writer',
    'newsletter': 'Newsletter',
  };

  static const _imageModes = {
    0: 'Nessuna immagine',
    1: 'Immagine singola',
    2: 'Carousel',
  };

  @override
  void dispose() {
    _topicController.dispose();
    super.dispose();
  }

  Future<void> _generate() async {
    final input = _topicController.text.trim();
    if (input.isEmpty) return;

    setState(() => _loading = true);
    final messenger = ScaffoldMessenger.of(context);
    final navigator = Navigator.of(context);

    try {
      await ApiClient.post(
        '/api/v1/agencies/${widget.agency.id}/agents/$_agentType/run',
        {
          'input': input,
          'projectId': widget.project.id,
          'imageMode': _imageMode,
          'imageCount': _imageMode == 2 ? _imageCount : (_imageMode == 1 ? 1 : 0),
        },
      );
      messenger.showSnackBar(
          const SnackBar(content: Text('Generazione avviata!')));
      navigator.pop();
    } catch (e) {
      messenger.showSnackBar(SnackBar(content: Text('Errore: $e')));
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: Text('Genera contenuto - ${widget.project.name}'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(vertical: 8),
        child: Card(
          margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Nuovo contenuto',
                    style: Theme.of(context)
                        .textTheme
                        .titleMedium
                        ?.copyWith(fontWeight: FontWeight.w600)),
                const SizedBox(height: 16),

                // Topic input
                TextField(
                  controller: _topicController,
                  decoration: const InputDecoration(
                    labelText: 'Argomento / Tema',
                    border: OutlineInputBorder(),
                    hintText: 'Descrivi il tema del contenuto...',
                  ),
                  maxLines: 3,
                  textInputAction: TextInputAction.done,
                ),
                const SizedBox(height: 16),

                // Agent type dropdown
                DropdownButtonFormField<String>(
                  initialValue: _agentType,
                  decoration: const InputDecoration(
                    labelText: 'Tipo di agente',
                    border: OutlineInputBorder(),
                  ),
                  items: _agentTypes.entries
                      .map((e) => DropdownMenuItem(
                            value: e.key,
                            child: Text(e.value),
                          ))
                      .toList(),
                  onChanged: (v) {
                    if (v != null) setState(() => _agentType = v);
                  },
                ),
                const SizedBox(height: 16),

                // Image mode dropdown
                DropdownButtonFormField<int>(
                  initialValue: _imageMode,
                  decoration: const InputDecoration(
                    labelText: 'Generazione immagini',
                    border: OutlineInputBorder(),
                  ),
                  items: _imageModes.entries
                      .map((e) => DropdownMenuItem(
                            value: e.key,
                            child: Text(e.value),
                          ))
                      .toList(),
                  onChanged: (v) {
                    if (v != null) setState(() => _imageMode = v);
                  },
                ),

                // Image count (only for carousel)
                if (_imageMode == 2) ...[
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      Icon(Icons.collections,
                          size: 20, color: cs.onSurfaceVariant),
                      const SizedBox(width: 8),
                      Text('Numero immagini',
                          style: Theme.of(context)
                              .textTheme
                              .bodyMedium
                              ?.copyWith(color: cs.onSurfaceVariant)),
                      const Spacer(),
                      IconButton(
                        icon: const Icon(Icons.remove_circle_outline),
                        onPressed: _imageCount > 1
                            ? () => setState(() => _imageCount--)
                            : null,
                      ),
                      Text('$_imageCount',
                          style: Theme.of(context)
                              .textTheme
                              .titleMedium
                              ?.copyWith(fontWeight: FontWeight.w600)),
                      IconButton(
                        icon: const Icon(Icons.add_circle_outline),
                        onPressed: _imageCount < 5
                            ? () => setState(() => _imageCount++)
                            : null,
                      ),
                    ],
                  ),
                ],

                const SizedBox(height: 24),

                // Generate button
                SizedBox(
                  width: double.infinity,
                  child: FilledButton.icon(
                    onPressed: _loading ||
                            _topicController.text.trim().isEmpty
                        ? null
                        : _generate,
                    icon: _loading
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(
                                strokeWidth: 2, color: Colors.white),
                          )
                        : const Icon(Icons.auto_awesome),
                    label: Text(_loading
                        ? 'Generazione in corso...'
                        : 'Genera contenuto'),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
