import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';

class AgencyNotificationSettingsScreen extends StatefulWidget {
  final Agency agency;
  const AgencyNotificationSettingsScreen({super.key, required this.agency});

  @override
  State<AgencyNotificationSettingsScreen> createState() =>
      _AgencyNotificationSettingsScreenState();
}

class _AgencyNotificationSettingsScreenState
    extends State<AgencyNotificationSettingsScreen> {
  final _emailCtrl = TextEditingController();
  bool _telegramEnabled = true;
  bool _emailOnSubscribed = true;
  bool _pushOnSubscribed = true;
  bool _telegramOnSubscribed = true;
  bool _loading = true;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _emailCtrl.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    try {
      final res = await ApiClient.get('/api/v1/agencies/${widget.agency.id}');
      final a = (res['data'] ?? res) as Map<String, dynamic>;
      if (!mounted) return;
      setState(() {
        _emailCtrl.text = (a['notificationEmail'] as String?) ?? '';
        _telegramEnabled = a['telegramNotificationsEnabled'] ?? true;
        _emailOnSubscribed = a['notifyEmailOnSubscribed'] ?? true;
        _pushOnSubscribed = a['notifyPushOnSubscribed'] ?? true;
        _telegramOnSubscribed = a['notifyTelegramOnSubscribed'] ?? true;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Errore: $e')));
    }
  }

  Future<void> _save() async {
    setState(() => _saving = true);
    final messenger = ScaffoldMessenger.of(context);
    try {
      final email = _emailCtrl.text.trim();
      await ApiClient.put(
        '/api/v1/agencies/${widget.agency.id}/notification-settings',
        {
          'notificationEmail': email.isEmpty ? null : email,
          'telegramNotificationsEnabled': _telegramEnabled,
          'notifyEmailOnSubscribed': _emailOnSubscribed,
          'notifyPushOnSubscribed': _pushOnSubscribed,
          'notifyTelegramOnSubscribed': _telegramOnSubscribed,
        },
      );
      messenger.showSnackBar(
          const SnackBar(content: Text('Impostazioni salvate')));
    } catch (e) {
      messenger.showSnackBar(SnackBar(content: Text('Errore: $e')));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(title: const Text('Notifiche agenzia')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView(
              padding: const EdgeInsets.all(16),
              children: [
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Icon(Icons.notifications_outlined,
                                color: cs.primary),
                            const SizedBox(width: 8),
                            Text('Notifiche agenzia',
                                style: Theme.of(context)
                                    .textTheme
                                    .titleMedium
                                    ?.copyWith(fontWeight: FontWeight.w600)),
                          ],
                        ),
                        const SizedBox(height: 16),
                        Text('Email notifiche agenzia',
                            style: Theme.of(context).textTheme.labelMedium),
                        const SizedBox(height: 6),
                        TextField(
                          controller: _emailCtrl,
                          keyboardType: TextInputType.emailAddress,
                          decoration: InputDecoration(
                            hintText: 'admin@esempio.com',
                            border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(10)),
                            isDense: true,
                            contentPadding: const EdgeInsets.symmetric(
                                horizontal: 12, vertical: 12),
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Destinatario delle notifiche a livello agenzia. Lascia vuoto per non ricevere email.',
                          style: Theme.of(context)
                              .textTheme
                              .bodySmall
                              ?.copyWith(color: cs.onSurfaceVariant),
                        ),
                        const SizedBox(height: 16),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Notifiche Telegram abilitate'),
                          subtitle: const Text(
                              'Interruttore generale per tutte le notifiche Telegram dell\'agenzia.'),
                          value: _telegramEnabled,
                          onChanged: (v) =>
                              setState(() => _telegramEnabled = v),
                        ),
                        const SizedBox(height: 8),
                        const Divider(),
                        const SizedBox(height: 8),
                        _sectionLabel('Iscrizione newsletter', cs),
                        Text(
                          'Quando qualcuno si iscrive a una newsletter dell\'agenzia.',
                          style: Theme.of(context)
                              .textTheme
                              .bodySmall
                              ?.copyWith(color: cs.onSurfaceVariant),
                        ),
                        const SizedBox(height: 4),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Email'),
                          value: _emailOnSubscribed,
                          onChanged: (v) =>
                              setState(() => _emailOnSubscribed = v),
                        ),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Push mobile'),
                          value: _pushOnSubscribed,
                          onChanged: (v) =>
                              setState(() => _pushOnSubscribed = v),
                        ),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Telegram'),
                          value: _telegramOnSubscribed,
                          onChanged: (v) =>
                              setState(() => _telegramOnSubscribed = v),
                        ),
                        const SizedBox(height: 12),
                        SizedBox(
                          width: double.infinity,
                          child: FilledButton.icon(
                            icon: _saving
                                ? const SizedBox(
                                    height: 16,
                                    width: 16,
                                    child: CircularProgressIndicator(
                                        strokeWidth: 2,
                                        color: Colors.white),
                                  )
                                : const Icon(Icons.save_outlined),
                            label: const Text('Salva'),
                            onPressed: _saving ? null : _save,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
    );
  }

  Widget _sectionLabel(String text, ColorScheme cs) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Text(
        text.toUpperCase(),
        style: TextStyle(
          fontSize: 11,
          fontWeight: FontWeight.w600,
          color: cs.onSurfaceVariant,
          letterSpacing: 0.8,
        ),
      ),
    );
  }
}
