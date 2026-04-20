import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';

class ProjectSettingsScreen extends StatefulWidget {
  final Agency agency;
  final Project project;
  const ProjectSettingsScreen({
    super.key,
    required this.agency,
    required this.project,
  });

  @override
  State<ProjectSettingsScreen> createState() => _ProjectSettingsScreenState();
}

class _ProjectSettingsScreenState extends State<ProjectSettingsScreen> {
  final _emailCtrl = TextEditingController();
  bool _emailOnGeneration = false;
  bool _emailOnPublication = false;
  bool _emailOnApproval = false;
  bool _pushOnGeneration = false;
  bool _pushOnPublication = false;
  bool _pushOnApproval = false;
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
      final res = await ApiClient.get(
          '/api/v1/agencies/${widget.agency.id}/projects/${widget.project.id}');
      final p = res['data'] as Map<String, dynamic>;
      if (!mounted) return;
      setState(() {
        _emailCtrl.text = (p['notificationEmail'] as String?) ?? '';
        _emailOnGeneration = p['notifyEmailOnGeneration'] ?? false;
        _emailOnPublication = p['notifyEmailOnPublication'] ?? false;
        _emailOnApproval = p['notifyEmailOnApprovalNeeded'] ?? false;
        _pushOnGeneration = p['notifyPushOnGeneration'] ?? false;
        _pushOnPublication = p['notifyPushOnPublication'] ?? false;
        _pushOnApproval = p['notifyPushOnApprovalNeeded'] ?? false;
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
        '/api/v1/agencies/${widget.agency.id}/projects/${widget.project.id}/email-notifications',
        {
          'notifyEmailOnGeneration': _emailOnGeneration,
          'notifyEmailOnPublication': _emailOnPublication,
          'notifyEmailOnApprovalNeeded': _emailOnApproval,
          'notificationEmail': email.isEmpty ? null : email,
          'notifyPushOnGeneration': _pushOnGeneration,
          'notifyPushOnPublication': _pushOnPublication,
          'notifyPushOnApprovalNeeded': _pushOnApproval,
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
      appBar: AppBar(
        title: Text(widget.project.name),
      ),
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
                            Icon(Icons.mail_outline, color: cs.primary),
                            const SizedBox(width: 8),
                            Text('Notifiche',
                                style: Theme.of(context)
                                    .textTheme
                                    .titleMedium
                                    ?.copyWith(fontWeight: FontWeight.w600)),
                          ],
                        ),
                        const SizedBox(height: 16),
                        Text('Email destinatario',
                            style: Theme.of(context).textTheme.labelMedium),
                        const SizedBox(height: 6),
                        TextField(
                          controller: _emailCtrl,
                          keyboardType: TextInputType.emailAddress,
                          decoration: InputDecoration(
                            hintText: 'nome@esempio.com',
                            border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(10)),
                            isDense: true,
                            contentPadding: const EdgeInsets.symmetric(
                                horizontal: 12, vertical: 12),
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Lascia vuoto per non ricevere notifiche email',
                          style: Theme.of(context)
                              .textTheme
                              .bodySmall
                              ?.copyWith(color: cs.onSurfaceVariant),
                        ),
                        const SizedBox(height: 20),
                        _sectionLabel('Email', cs),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Generazione contenuti'),
                          value: _emailOnGeneration,
                          onChanged: (v) =>
                              setState(() => _emailOnGeneration = v),
                        ),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Pubblicazione'),
                          value: _emailOnPublication,
                          onChanged: (v) =>
                              setState(() => _emailOnPublication = v),
                        ),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Contenuti da approvare'),
                          value: _emailOnApproval,
                          onChanged: (v) =>
                              setState(() => _emailOnApproval = v),
                        ),
                        const SizedBox(height: 8),
                        const Divider(),
                        const SizedBox(height: 8),
                        _sectionLabel('Push mobile', cs),
                        Text(
                          'Ricevi notifiche push su questo dispositivo. Richiede login attivo.',
                          style: Theme.of(context)
                              .textTheme
                              .bodySmall
                              ?.copyWith(color: cs.onSurfaceVariant),
                        ),
                        const SizedBox(height: 4),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Generazione contenuti'),
                          value: _pushOnGeneration,
                          onChanged: (v) =>
                              setState(() => _pushOnGeneration = v),
                        ),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Pubblicazione'),
                          value: _pushOnPublication,
                          onChanged: (v) =>
                              setState(() => _pushOnPublication = v),
                        ),
                        SwitchListTile(
                          contentPadding: EdgeInsets.zero,
                          title: const Text('Contenuti approvati'),
                          value: _pushOnApproval,
                          onChanged: (v) =>
                              setState(() => _pushOnApproval = v),
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
