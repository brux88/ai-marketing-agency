import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../api/api_client.dart';
import '../models.dart';

class ApprovalsScreen extends StatefulWidget {
  final Agency agency;
  const ApprovalsScreen({super.key, required this.agency});
  @override
  State<ApprovalsScreen> createState() => _ApprovalsScreenState();
}

class _ApprovalsScreenState extends State<ApprovalsScreen> {
  late Future<List<PendingApproval>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<PendingApproval>> _load() async {
    final res = await ApiClient.get(
        '/api/v1/agencies/${widget.agency.id}/approvals');
    return (res['data'] as List)
        .map((j) => PendingApproval.fromJson(j))
        .toList();
  }

  Future<void> _approve(String id) async {
    final messenger = ScaffoldMessenger.of(context);
    try {
      await ApiClient.put(
          '/api/v1/agencies/${widget.agency.id}/approvals/$id/approve', {});
      messenger.showSnackBar(
          const SnackBar(content: Text('Contenuto approvato')));
      if (mounted) setState(() => _future = _load());
    } catch (e) {
      messenger.showSnackBar(SnackBar(content: Text('Errore: $e')));
    }
  }

  Future<void> _reject(String id) async {
    final messenger = ScaffoldMessenger.of(context);
    try {
      await ApiClient.put(
          '/api/v1/agencies/${widget.agency.id}/approvals/$id/reject', {});
      messenger.showSnackBar(
          const SnackBar(content: Text('Contenuto rifiutato')));
      if (mounted) setState(() => _future = _load());
    } catch (e) {
      messenger.showSnackBar(SnackBar(content: Text('Errore: $e')));
    }
  }

  String _contentTypeLabel(int ct) {
    switch (ct) {
      case 0:
        return 'Blog';
      case 1:
        return 'Social';
      case 2:
        return 'Newsletter';
      default:
        return 'Altro';
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: const Text('Approvazioni in attesa'),
      ),
      body: FutureBuilder<List<PendingApproval>>(
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
          final items = snap.data ?? [];
          if (items.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.check_circle_outline,
                      size: 64,
                      color: cs.primary.withValues(alpha: 0.5)),
                  const SizedBox(height: 16),
                  Text('Nessuna approvazione in attesa',
                      style: Theme.of(context)
                          .textTheme
                          .titleMedium
                          ?.copyWith(color: cs.onSurfaceVariant)),
                  const SizedBox(height: 8),
                  Text('Tutto aggiornato!',
                      style: Theme.of(context)
                          .textTheme
                          .bodyMedium
                          ?.copyWith(color: cs.onSurfaceVariant)),
                ],
              ),
            );
          }
          return RefreshIndicator(
            onRefresh: () async => setState(() => _future = _load()),
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(vertical: 8),
              itemCount: items.length,
              itemBuilder: (_, i) => _ApprovalCard(
                approval: items[i],
                contentTypeLabel:
                    _contentTypeLabel(items[i].contentType),
                onApprove: () => _approve(items[i].id),
                onReject: () => _reject(items[i].id),
              ),
            ),
          );
        },
      ),
    );
  }
}

class _ApprovalCard extends StatelessWidget {
  final PendingApproval approval;
  final String contentTypeLabel;
  final VoidCallback onApprove;
  final VoidCallback onReject;

  const _ApprovalCard({
    required this.approval,
    required this.contentTypeLabel,
    required this.onApprove,
    required this.onReject,
  });

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Image preview
          if (approval.imageUrl != null)
            CachedNetworkImage(
              imageUrl: approval.imageUrl!,
              height: 200,
              width: double.infinity,
              fit: BoxFit.cover,
              placeholder: (context, url) => Container(
                height: 200,
                color: cs.surfaceContainerHighest,
                child:
                    const Center(child: CircularProgressIndicator()),
              ),
              errorWidget: (context, url, error) => Container(
                height: 100,
                color: cs.surfaceContainerHighest,
                child: const Icon(Icons.broken_image, size: 40),
              ),
            ),
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header row
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: cs.primaryContainer,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Text(contentTypeLabel,
                          style: TextStyle(
                              fontSize: 11,
                              color: cs.onPrimaryContainer,
                              fontWeight: FontWeight.w600)),
                    ),
                    const SizedBox(width: 8),
                    if (approval.projectName != null)
                      Expanded(
                        child: Text(approval.projectName!,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: Theme.of(context)
                                .textTheme
                                .bodySmall
                                ?.copyWith(
                                    color: cs.onSurfaceVariant)),
                      ),
                    const Spacer(),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: _scoreColor(approval.overallScore)
                            .withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.star,
                              size: 14,
                              color:
                                  _scoreColor(approval.overallScore)),
                          const SizedBox(width: 4),
                          Text(
                            '${approval.overallScore.toStringAsFixed(1)}/10',
                            style: TextStyle(
                                fontSize: 12,
                                fontWeight: FontWeight.w600,
                                color: _scoreColor(
                                    approval.overallScore)),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                // Title
                Text(approval.title,
                    style: Theme.of(context)
                        .textTheme
                        .titleMedium
                        ?.copyWith(fontWeight: FontWeight.w600)),
                const SizedBox(height: 8),
                // Body preview
                Text(
                  approval.body,
                  maxLines: 6,
                  overflow: TextOverflow.ellipsis,
                  style: Theme.of(context)
                      .textTheme
                      .bodyMedium
                      ?.copyWith(color: cs.onSurfaceVariant),
                ),
                if (approval.scoreExplanation != null &&
                    approval.scoreExplanation!.isNotEmpty) ...[
                  const SizedBox(height: 12),
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: cs.surfaceContainerHighest,
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Icon(Icons.info_outline,
                            size: 16, color: cs.onSurfaceVariant),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            approval.scoreExplanation!,
                            style: Theme.of(context)
                                .textTheme
                                .bodySmall
                                ?.copyWith(
                                    color: cs.onSurfaceVariant),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
                const SizedBox(height: 16),
                // Action buttons
                Row(
                  children: [
                    Expanded(
                      child: FilledButton.icon(
                        icon: const Icon(Icons.check, size: 18),
                        label: const Text('Approva'),
                        onPressed: onApprove,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: OutlinedButton.icon(
                        icon: Icon(Icons.close,
                            size: 18, color: cs.error),
                        label: Text('Rifiuta',
                            style: TextStyle(color: cs.error)),
                        style: OutlinedButton.styleFrom(
                          side: BorderSide(color: cs.error),
                        ),
                        onPressed: onReject,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Color _scoreColor(double score) {
    if (score >= 8) return Colors.green;
    if (score >= 6) return Colors.orange;
    return Colors.red;
  }
}
